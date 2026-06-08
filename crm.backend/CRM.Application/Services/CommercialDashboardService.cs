using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services
{
    public class CommercialDashboardService
    {
        private readonly AppDbContext _db;
        private readonly CommercialAutomationService _automation;

        public CommercialDashboardService(AppDbContext db, CommercialAutomationService automation)
        {
            _db = db;
            _automation = automation;
        }

        public async Task<CommercialDashboardDto> Get()
        {
            await _automation.RunQuoteReminders();

            var quotes = await _db.Quotes
                .Include(quote => quote.Company)
                .Include(quote => quote.Lines)
                .ToListAsync();

            var salesLines = await _db.SalesOrderLines
                .Include(line => line.Article)
                .Include(line => line.SalesOrder)
                .Where(line => line.SalesOrder.Status != "Annullato")
                .ToListAsync();

            var openQuotes = quotes.Where(quote => IsOpenQuote(quote.Status)).ToList();
            var sentQuotes = quotes.Where(quote => IsStatus(quote.Status, "Inviato")).ToList();
            var acceptedQuotes = quotes.Where(quote => IsStatus(quote.Status, "Accettato")).ToList();
            var lostQuotes = quotes.Where(quote => IsStatus(quote.Status, "Rifiutato") || IsStatus(quote.Status, "Perso")).ToList();
            var conversionBase = acceptedQuotes.Count + lostQuotes.Count;

            return new CommercialDashboardDto
            {
                OpenQuotes = openQuotes.Count,
                SentQuotes = sentQuotes.Count,
                AcceptedQuotes = acceptedQuotes.Count,
                LostQuotes = lostQuotes.Count,
                SalesOrdersToFulfill = await _db.SalesOrders.CountAsync(order => order.Status == "Da evadere"),
                DisabledArticles = await _db.Articles.CountAsync(article => !article.Active),
                ConversionRate = conversionBase == 0 ? 0 : Math.Round((decimal)acceptedQuotes.Count / conversionBase * 100, 2),
                PipelineValue = openQuotes.Sum(QuoteGrandTotal),
                AcceptedValue = acceptedQuotes.Sum(QuoteGrandTotal),
                LostValue = lostQuotes.Sum(QuoteGrandTotal),
                QuoteStatusTotals = quotes
                    .GroupBy(quote => string.IsNullOrWhiteSpace(quote.Status) ? "Senza stato" : quote.Status)
                    .Select(group => new CommercialDashboardStatusDto
                    {
                        Status = group.Key,
                        Count = group.Count(),
                        Value = group.Sum(QuoteGrandTotal)
                    })
                    .OrderByDescending(item => item.Value)
                    .ToList(),
                BestCustomers = acceptedQuotes
                    .GroupBy(quote => !string.IsNullOrWhiteSpace(quote.Company?.name) ? quote.Company.name : quote.CustomerName)
                    .Select(group => new CommercialDashboardItemDto
                    {
                        Label = string.IsNullOrWhiteSpace(group.Key) ? "Cliente non definito" : group.Key,
                        Count = group.Count(),
                        Value = group.Sum(QuoteGrandTotal)
                    })
                    .OrderByDescending(item => item.Value)
                    .Take(5)
                    .ToList(),
                TopArticles = salesLines
                    .GroupBy(line => !string.IsNullOrWhiteSpace(line.Article?.Name) ? line.Article.Name : line.Description)
                    .Select(group => new CommercialDashboardItemDto
                    {
                        Label = string.IsNullOrWhiteSpace(group.Key) ? "Articolo non definito" : group.Key,
                        Count = group.Count(),
                        Quantity = group.Sum(line => line.Quantity),
                        Value = group.Sum(SalesLineGrandTotal)
                    })
                    .OrderByDescending(item => item.Value)
                    .Take(5)
                    .ToList()
            };
        }

        private static bool IsOpenQuote(string status)
        {
            return IsStatus(status, "Bozza") || IsStatus(status, "Inviato");
        }

        private static bool IsStatus(string status, string expected)
        {
            return string.Equals(status, expected, StringComparison.OrdinalIgnoreCase);
        }

        private static decimal QuoteGrandTotal(Quote quote)
        {
            return quote.Lines.Sum(line =>
            {
                var lineTotal = LineNetTotal(line.Quantity, line.UnitPrice, line.Discount);
                return lineTotal + lineTotal * (line.VatRate / 100);
            });
        }

        private static decimal SalesLineGrandTotal(SalesOrderLine line)
        {
            var lineTotal = LineNetTotal(line.Quantity, line.UnitPrice, line.Discount);
            return lineTotal + lineTotal * (line.VatRate / 100);
        }

        private static decimal LineNetTotal(decimal quantity, decimal unitPrice, decimal discount)
        {
            var subtotal = quantity * unitPrice;
            return subtotal - subtotal * (discount / 100);
        }
    }
}

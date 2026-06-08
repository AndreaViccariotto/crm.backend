using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services
{
    public class QuoteService
    {
        private readonly AppDbContext _db;
        private readonly CustomFieldService _customFields;

        public QuoteService(AppDbContext db, CustomFieldService customFields)
        {
            _db = db;
            _customFields = customFields;
        }

        public async Task<List<QuoteResponse>> Get(string? search = null, string? status = null, int? companyId = null, int? contactId = null, DateTime? validUntilFrom = null, DateTime? validUntilTo = null)
        {
            var query = _db.Quotes
                .Include(quote => quote.Company)
                .Include(quote => quote.Contact)
                .Include(quote => quote.Lines)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim();
                query = query.Where(quote =>
                    quote.Number.Contains(normalizedSearch) ||
                    quote.CustomerName.Contains(normalizedSearch));
            }

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(quote => quote.Status == status);

            if (companyId.HasValue)
                query = query.Where(quote => quote.CompanyId == companyId.Value);

            if (contactId.HasValue)
                query = query.Where(quote => quote.ContactId == contactId.Value);

            if (validUntilFrom.HasValue)
                query = query.Where(quote => quote.ValidUntil.HasValue && quote.ValidUntil.Value.Date >= validUntilFrom.Value.Date);

            if (validUntilTo.HasValue)
                query = query.Where(quote => quote.ValidUntil.HasValue && quote.ValidUntil.Value.Date <= validUntilTo.Value.Date);

            var quotes = await query
                .OrderByDescending(quote => quote.CreatedAt)
                .ToListAsync();

            return quotes.Select(ToResponse).ToList();
        }

        public async Task<QuoteResponse?> GetById(int id)
        {
            var quote = await _db.Quotes
                .Include(item => item.Company)
                .Include(item => item.Contact)
                .Include(item => item.Lines)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (quote == null)
                return null;

            var response = ToResponse(quote);
            response.CustomFields = await _customFields.GetValues("quotes", quote.Id);
            return response;
        }

        public async Task<string> GetNextNumber()
        {
            return await GenerateNextQuoteNumber();
        }

        public async Task<QuoteResponse> Save(QuoteRequest request)
        {
            request.Number = await ResolveNumberForNewQuote(request.Number);
            await Validate(request);

            var company = await _db.Companies.FindAsync(request.CompanyId);

            var quote = new Quote
            {
                Number = request.Number.Trim(),
                CompanyId = request.CompanyId,
                ContactId = request.ContactId,
                CustomerName = company?.name ?? request.CustomerName.Trim(),
                ValidUntil = request.ValidUntil,
                Status = Normalize(request.Status, "Bozza"),
                CreatedAt = DateTime.UtcNow,
                Lines = BuildLines(request.Lines)
            };

            _db.Quotes.Add(quote);
            await _db.SaveChangesAsync();
            await _customFields.SaveValues("quotes", quote.Id, request.CustomFields);

            if (IsAccepted(quote.Status))
                await CreateOrSyncSalesOrder(quote.Id);

            return (await GetById(quote.Id))!;
        }

        public async Task<QuoteResponse?> Update(QuoteRequest request)
        {
            var quote = await _db.Quotes
                .Include(item => item.Lines)
                .FirstOrDefaultAsync(item => item.Id == request.Id);

            if (quote == null)
                return null;

            request.Number = string.IsNullOrWhiteSpace(request.Number)
                ? quote.Number
                : request.Number.Trim();

            await Validate(request);
            var company = await _db.Companies.FindAsync(request.CompanyId);

            quote.Number = request.Number.Trim();
            quote.CompanyId = request.CompanyId;
            quote.ContactId = request.ContactId;
            quote.CustomerName = company?.name ?? request.CustomerName.Trim();
            quote.ValidUntil = request.ValidUntil;
            quote.Status = Normalize(request.Status, "Bozza");
            quote.UpdatedAt = DateTime.UtcNow;

            _db.QuoteLines.RemoveRange(quote.Lines);
            quote.Lines = BuildLines(request.Lines);

            await _db.SaveChangesAsync();
            await _customFields.SaveValues("quotes", quote.Id, request.CustomFields);

            if (IsAccepted(quote.Status))
                await CreateOrSyncSalesOrder(quote.Id);

            return await GetById(quote.Id);
        }

        public async Task<string> Delete(int id)
        {
            var quote = await _db.Quotes.FindAsync(id);
            if (quote == null)
                return "Preventivo non trovato";

            await _customFields.DeleteValues("quotes", id);
            _db.Quotes.Remove(quote);
            await _db.SaveChangesAsync();

            return "Preventivo eliminato con successo";
        }

        private async System.Threading.Tasks.Task Validate(QuoteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Number))
                throw new Exception("Il numero preventivo e obbligatorio");

            if (!request.CompanyId.HasValue)
                throw new Exception("Il cliente e obbligatorio");

            var companyExists = await _db.Companies.AnyAsync(company => company.Id == request.CompanyId.Value);
            if (!companyExists)
                throw new Exception("Cliente non trovato");

            if (request.ContactId.HasValue)
            {
                var contactMatchesCompany = await _db.Contacts.AnyAsync(contact =>
                    contact.Id == request.ContactId.Value && contact.Company_Id == request.CompanyId.Value);

                if (!contactMatchesCompany)
                    throw new Exception("Il contatto selezionato non appartiene al cliente");
            }

            if (request.Lines.Count == 0)
                throw new Exception("Inserisci almeno una riga nel preventivo");

            var normalizedNumber = request.Number.Trim();
            var numberExists = await _db.Quotes.AnyAsync(quote =>
                quote.Number == normalizedNumber && quote.Id != request.Id);

            if (numberExists)
                throw new Exception("Esiste gia un preventivo con questo numero");

            foreach (var line in request.Lines)
            {
                if (string.IsNullOrWhiteSpace(line.Description))
                    throw new Exception("Ogni riga deve avere una descrizione");

                if (line.Quantity <= 0)
                    throw new Exception("La quantita deve essere maggiore di zero");

                if (line.UnitPrice < 0)
                    throw new Exception("Il prezzo non puo essere negativo");

                if (line.Discount < 0 || line.Discount > 100)
                    throw new Exception("Lo sconto deve essere tra 0 e 100");
            }
        }

        private async Task<string> ResolveNumberForNewQuote(string? requestedNumber)
        {
            var normalizedNumber = requestedNumber?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(normalizedNumber))
                return await GenerateNextQuoteNumber();

            var numberExists = await _db.Quotes.AnyAsync(quote => quote.Number == normalizedNumber);
            return numberExists ? await GenerateNextQuoteNumber() : normalizedNumber;
        }

        private async Task<string> GenerateNextQuoteNumber()
        {
            var prefix = await _db.GeneralSettings
                .Where(setting => setting.Key == "quotePrefix")
                .Select(setting => setting.Value)
                .FirstOrDefaultAsync();

            prefix = string.IsNullOrWhiteSpace(prefix) ? "PREV" : prefix.Trim();

            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var baseNumber = $"{prefix}-{today}";
            var existingNumbers = await _db.Quotes
                .Where(quote => quote.Number.StartsWith(baseNumber))
                .Select(quote => quote.Number)
                .ToListAsync();

            var maxSequence = 0;
            foreach (var number in existingNumbers)
            {
                if (number == baseNumber)
                {
                    maxSequence = Math.Max(maxSequence, 1);
                    continue;
                }

                var sequencePart = number.StartsWith($"{baseNumber}-", StringComparison.OrdinalIgnoreCase)
                    ? number.Substring(baseNumber.Length + 1)
                    : "";

                if (int.TryParse(sequencePart, out var sequence))
                    maxSequence = Math.Max(maxSequence, sequence);
            }

            return $"{baseNumber}-{maxSequence + 1:0000}";
        }

        private async System.Threading.Tasks.Task CreateOrSyncSalesOrder(int quoteId)
        {
            var quote = await _db.Quotes
                .Include(item => item.Lines)
                .FirstAsync(item => item.Id == quoteId);

            var order = await _db.SalesOrders
                .Include(item => item.Lines)
                .FirstOrDefaultAsync(item => item.QuoteId == quote.Id);

            var targetNumber = await GenerateSalesOrderNumber(quote);
            var newLines = quote.Lines
                .OrderBy(line => line.SortOrder)
                .Select(line => new SalesOrderLine
                {
                    ArticleId = line.ArticleId,
                    Description = line.Description,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    Discount = line.Discount,
                    VatRate = line.VatRate,
                    SortOrder = line.SortOrder
                })
                .ToList();

            if (order == null)
            {
                order = new SalesOrder
                {
                    QuoteId = quote.Id,
                    Number = targetNumber,
                    CompanyId = quote.CompanyId,
                    ContactId = quote.ContactId,
                    CustomerName = quote.CustomerName,
                    Status = "Da evadere",
                    OrderDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    Lines = newLines
                };
                _db.SalesOrders.Add(order);
            }
            else
            {
                order.Number = targetNumber;
                order.CompanyId = quote.CompanyId;
                order.ContactId = quote.ContactId;
                order.CustomerName = quote.CustomerName;
                _db.SalesOrderLines.RemoveRange(order.Lines);
                order.Lines = newLines;
            }

            await _db.SaveChangesAsync();
        }

        private async Task<string> GenerateSalesOrderNumber(Quote quote)
        {
            var prefix = await _db.GeneralSettings
                .Where(setting => setting.Key == "salesOrderPrefix")
                .Select(setting => setting.Value)
                .FirstOrDefaultAsync();

            prefix = string.IsNullOrWhiteSpace(prefix) ? "OV" : prefix.Trim();
            var targetNumber = $"{prefix}-{quote.Number}";
            var exists = await _db.SalesOrders.AnyAsync(order =>
                order.Number == targetNumber && order.QuoteId != quote.Id);

            return exists ? $"{targetNumber}-{quote.Id}" : targetNumber;
        }

        private static List<QuoteLine> BuildLines(List<QuoteLineRequest> lines)
        {
            return lines.Select((line, index) => new QuoteLine
            {
                ArticleId = line.ArticleId,
                Description = line.Description.Trim(),
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                Discount = line.Discount,
                VatRate = line.VatRate,
                SortOrder = index + 1
            }).ToList();
        }

        private static QuoteResponse ToResponse(Quote quote)
        {
            var lines = quote.Lines.OrderBy(line => line.SortOrder).Select(ToLineResponse).ToList();
            var total = lines.Sum(line => line.LineTotal);
            var vatTotal = lines.Sum(line => line.LineTotal * (line.VatRate / 100));

            return new QuoteResponse
            {
                Id = quote.Id,
                Number = quote.Number,
                CompanyId = quote.CompanyId,
                ContactId = quote.ContactId,
                CustomerName = quote.CustomerName,
                CompanyName = quote.Company?.name,
                ContactName = quote.Contact?.Name,
                ValidUntil = quote.ValidUntil,
                Status = quote.Status,
                Lines = lines,
                Total = total,
                VatTotal = vatTotal,
                GrandTotal = total + vatTotal
            };
        }

        private static QuoteLineResponse ToLineResponse(QuoteLine line)
        {
            var lineTotal = CalculateLineTotal(line.Quantity, line.UnitPrice, line.Discount);
            return new QuoteLineResponse
            {
                Id = line.Id,
                ArticleId = line.ArticleId,
                Description = line.Description,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                Discount = line.Discount,
                VatRate = line.VatRate,
                LineTotal = lineTotal
            };
        }

        private static decimal CalculateLineTotal(decimal quantity, decimal unitPrice, decimal discount)
        {
            var subtotal = quantity * unitPrice;
            return subtotal - subtotal * (discount / 100);
        }

        private static string Normalize(string? value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static bool IsAccepted(string status)
        {
            return string.Equals(status, "Accettato", StringComparison.OrdinalIgnoreCase);
        }
    }
}





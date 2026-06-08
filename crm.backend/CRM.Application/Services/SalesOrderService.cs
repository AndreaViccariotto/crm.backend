using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services
{
    public class SalesOrderService
    {
        private readonly AppDbContext _db;

        public SalesOrderService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<SalesOrderResponse>> Get(int? companyId = null, int? contactId = null, string? status = null)
        {
            var query = _db.SalesOrders
                .Include(order => order.Company)
                .Include(order => order.Contact)
                .Include(order => order.Lines)
                .AsQueryable();

            if (companyId.HasValue)
                query = query.Where(order => order.CompanyId == companyId.Value);

            if (contactId.HasValue)
                query = query.Where(order => order.ContactId == contactId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(order => order.Status == status);

            var orders = await query.OrderByDescending(order => order.OrderDate).ToListAsync();
            return orders.Select(ToResponse).ToList();
        }

        public async Task<SalesOrderResponse?> GetById(int id)
        {
            var order = await _db.SalesOrders
                .Include(item => item.Company)
                .Include(item => item.Contact)
                .Include(item => item.Lines)
                .FirstOrDefaultAsync(item => item.Id == id);

            return order == null ? null : ToResponse(order);
        }

        public async Task<string> UpdateStatus(int id, string status)
        {
            var order = await _db.SalesOrders.FindAsync(id);
            if (order == null)
                return "Ordine vendita non trovato";

            order.Status = string.IsNullOrWhiteSpace(status) ? order.Status : status.Trim();
            await _db.SaveChangesAsync();
            return "Ordine vendita aggiornato con successo";
        }

        private static SalesOrderResponse ToResponse(SalesOrder order)
        {
            var lines = order.Lines
                .OrderBy(line => line.SortOrder)
                .Select(line =>
                {
                    var subtotal = line.Quantity * line.UnitPrice;
                    var lineTotal = subtotal - subtotal * (line.Discount / 100);
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
                })
                .ToList();

            var total = lines.Sum(line => line.LineTotal);
            var vatTotal = lines.Sum(line => line.LineTotal * (line.VatRate / 100));

            return new SalesOrderResponse
            {
                Id = order.Id,
                Number = order.Number,
                QuoteId = order.QuoteId,
                CompanyId = order.CompanyId,
                ContactId = order.ContactId,
                CustomerName = order.CustomerName,
                CompanyName = order.Company?.name,
                ContactName = order.Contact?.Name,
                Status = order.Status,
                OrderDate = order.OrderDate,
                Lines = lines,
                Total = total,
                VatTotal = vatTotal,
                GrandTotal = total + vatTotal
            };
        }
    }
}

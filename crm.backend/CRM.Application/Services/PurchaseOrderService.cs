using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services
{
    public class PurchaseOrderService
    {
        private readonly AppDbContext _db;
        private readonly CustomFieldService _customFields;

        public PurchaseOrderService(AppDbContext db, CustomFieldService customFields)
        {
            _db = db;
            _customFields = customFields;
        }

        public async Task<List<PurchaseOrderResponse>> Get(string? search = null, string? status = null)
        {
            var query = _db.PurchaseOrders.Include(order => order.Lines).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(order => order.Number.Contains(search.Trim()) || order.SupplierName.Contains(search.Trim()));

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(order => order.Status == status);

            var orders = await query.OrderByDescending(order => order.OrderDate).ToListAsync();
            return orders.Select(ToResponse).ToList();
        }

        public async Task<PurchaseOrderResponse?> GetById(int id)
        {
            var order = await _db.PurchaseOrders
                .Include(item => item.Lines)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (order == null)
                return null;

            var response = ToResponse(order);
            response.CustomFields = await _customFields.GetValues("purchase-orders", order.Id);
            return response;
        }

        public async Task<PurchaseOrderResponse> Save(PurchaseOrderRequest request)
        {
            await Validate(request);

            var order = new PurchaseOrder
            {
                Number = request.Number.Trim(),
                SupplierName = request.SupplierName.Trim(),
                OrderDate = request.OrderDate ?? DateTime.UtcNow,
                Status = Normalize(request.Status, "Bozza"),
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                Lines = BuildLines(request.Lines)
            };

            _db.PurchaseOrders.Add(order);
            await _db.SaveChangesAsync();
            await _customFields.SaveValues("purchase-orders", order.Id, request.CustomFields);
            return await GetById(order.Id) ?? ToResponse(order);
        }

        public async Task<PurchaseOrderResponse?> Update(PurchaseOrderRequest request)
        {
            var order = await _db.PurchaseOrders
                .Include(item => item.Lines)
                .FirstOrDefaultAsync(item => item.Id == request.Id);

            if (order == null)
                return null;

            await Validate(request);

            order.Number = request.Number.Trim();
            order.SupplierName = request.SupplierName.Trim();
            order.OrderDate = request.OrderDate ?? order.OrderDate;
            order.Status = Normalize(request.Status, "Bozza");
            order.Notes = request.Notes;
            order.UpdatedAt = DateTime.UtcNow;
            _db.PurchaseOrderLines.RemoveRange(order.Lines);
            order.Lines = BuildLines(request.Lines);

            await _db.SaveChangesAsync();
            await _customFields.SaveValues("purchase-orders", order.Id, request.CustomFields);
            return await GetById(order.Id) ?? ToResponse(order);
        }

        public async Task<string> Delete(int id)
        {
            var order = await _db.PurchaseOrders.FindAsync(id);
            if (order == null)
                return "Ordine acquisto non trovato";

            await _customFields.DeleteValues("purchase-orders", id);
            _db.PurchaseOrders.Remove(order);
            await _db.SaveChangesAsync();
            return "Ordine acquisto eliminato con successo";
        }

        private async System.Threading.Tasks.Task Validate(PurchaseOrderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Number))
                throw new Exception("Il numero ordine e obbligatorio");

            if (string.IsNullOrWhiteSpace(request.SupplierName))
                throw new Exception("Il fornitore e obbligatorio");

            if (request.Lines.Count == 0)
                throw new Exception("Inserisci almeno una riga");

            var normalizedNumber = request.Number.Trim();
            var exists = await _db.PurchaseOrders.AnyAsync(order => order.Number == normalizedNumber && order.Id != request.Id);
            if (exists)
                throw new Exception("Esiste gia un ordine acquisto con questo numero");
        }

        private static List<PurchaseOrderLine> BuildLines(List<PurchaseOrderLineRequest> lines)
        {
            return lines.Select((line, index) => new PurchaseOrderLine
            {
                ArticleId = line.ArticleId,
                Description = line.Description.Trim(),
                Quantity = line.Quantity,
                UnitCost = line.UnitCost,
                VatRate = line.VatRate,
                SortOrder = index + 1
            }).ToList();
        }

        private static PurchaseOrderResponse ToResponse(PurchaseOrder order)
        {
            var lines = order.Lines
                .OrderBy(line => line.SortOrder)
                .Select(line =>
                {
                    var lineTotal = line.Quantity * line.UnitCost;
                    return new PurchaseOrderLineResponse
                    {
                        Id = line.Id,
                        ArticleId = line.ArticleId,
                        Description = line.Description,
                        Quantity = line.Quantity,
                        UnitCost = line.UnitCost,
                        VatRate = line.VatRate,
                        LineTotal = lineTotal
                    };
                })
                .ToList();

            var total = lines.Sum(line => line.LineTotal);
            var vatTotal = lines.Sum(line => line.LineTotal * (line.VatRate / 100));

            return new PurchaseOrderResponse
            {
                Id = order.Id,
                Number = order.Number,
                SupplierName = order.SupplierName,
                OrderDate = order.OrderDate,
                Status = order.Status,
                Notes = order.Notes,
                Lines = lines,
                Total = total,
                VatTotal = vatTotal,
                GrandTotal = total + vatTotal
            };
        }

        private static string Normalize(string? value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }
}





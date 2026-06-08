using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services
{
    public class TicketService
    {
        private readonly AppDbContext _db;

        public TicketService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<TicketResponse>> Get(string? search, string? status, string? priority, int? companyId)
        {
            var query = _db.Tickets.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var value = search.Trim();
                query = query.Where(ticket => ticket.Number.Contains(value) || ticket.Subject.Contains(value) ||
                    (ticket.Description != null && ticket.Description.Contains(value)));
            }

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(ticket => ticket.Status == status);

            if (!string.IsNullOrWhiteSpace(priority))
                query = query.Where(ticket => ticket.Priority == priority);

            if (companyId.HasValue)
                query = query.Where(ticket => ticket.CompanyId == companyId);

            return await query.OrderByDescending(ticket => ticket.CreatedAt).Select(ToResponse).ToListAsync();
        }

        public async Task<TicketResponse?> GetById(int id)
        {
            return await _db.Tickets.AsNoTracking().Where(ticket => ticket.Id == id).Select(ToResponse).FirstOrDefaultAsync();
        }

        public async Task<string> GetNextNumber()
        {
            var prefix = $"TCK-{DateTime.UtcNow:yyyyMMdd}-";
            var count = await _db.Tickets.CountAsync(ticket => ticket.Number.StartsWith(prefix));
            return $"{prefix}{count + 1:0000}";
        }

        public async Task<TicketResponse> Save(TicketRequest request)
        {
            Validate(request);
            var ticket = new Ticket
            {
                Number = string.IsNullOrWhiteSpace(request.Number) ? await GetNextNumber() : request.Number.Trim(),
                Subject = request.Subject.Trim(),
                Description = Clean(request.Description),
                Status = NormalizeStatus(request.Status),
                Priority = NormalizePriority(request.Priority),
                CompanyId = request.CompanyId,
                ContactId = request.ContactId,
                AssignedUserId = request.AssignedUserId,
                AssignedAt = request.AssignedUserId.HasValue ? DateTime.UtcNow : null,
                CreatedAt = DateTime.UtcNow
            };
            ApplyClosedAt(ticket);
            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();
            return await GetById(ticket.Id) ?? throw new Exception("Ticket non trovato");
        }

        public async Task<TicketResponse?> Update(TicketRequest request)
        {
            var ticket = await _db.Tickets.FindAsync(request.Id);
            if (ticket == null) return null;

            Validate(request);
            var assignmentChanged = ticket.AssignedUserId != request.AssignedUserId;
            ticket.Subject = request.Subject.Trim();
            ticket.Description = Clean(request.Description);
            ticket.Status = NormalizeStatus(request.Status);
            ticket.Priority = NormalizePriority(request.Priority);
            ticket.CompanyId = request.CompanyId;
            ticket.ContactId = request.ContactId;
            ticket.AssignedUserId = request.AssignedUserId;
            if (assignmentChanged)
                ticket.AssignedAt = request.AssignedUserId.HasValue ? DateTime.UtcNow : null;
            ticket.UpdatedAt = DateTime.UtcNow;
            ApplyClosedAt(ticket);
            await _db.SaveChangesAsync();
            return await GetById(ticket.Id);
        }

        public async Task<string> Delete(int id)
        {
            var ticket = await _db.Tickets.FindAsync(id);
            if (ticket == null) return "Ticket non trovato";

            if (await _db.Interventions.AnyAsync(item => item.TicketId == id))
                throw new Exception("Il ticket contiene rapporti di intervento e non puo essere eliminato");

            var tasks = await _db.Tasks.Where(task => task.ticket_id == id).ToListAsync();
            tasks.ForEach(task => task.ticket_id = null);
            _db.Tickets.Remove(ticket);
            await _db.SaveChangesAsync();
            return "Ticket eliminato con successo";
        }

        private static readonly System.Linq.Expressions.Expression<Func<Ticket, TicketResponse>> ToResponse =
            ticket => new TicketResponse
            {
                Id = ticket.Id,
                Number = ticket.Number,
                Subject = ticket.Subject,
                Description = ticket.Description,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CompanyId = ticket.CompanyId,
                ContactId = ticket.ContactId,
                AssignedUserId = ticket.AssignedUserId,
                CompanyName = ticket.Company != null ? ticket.Company.name : null,
                ContactName = ticket.Contact != null ? ticket.Contact.Name : null,
                AssignedUserName = ticket.AssignedUser != null ? ticket.AssignedUser.username : null,
                AssignedAt = ticket.AssignedAt,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                ClosedAt = ticket.ClosedAt,
                TaskCount = ticket.Tasks.Count(),
                InterventionCount = ticket.Interventions.Count()
            };

        private static void Validate(TicketRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Subject))
                throw new Exception("L'oggetto del ticket e obbligatorio");
        }

        private static void ApplyClosedAt(Ticket ticket)
        {
            var closed = ticket.Status is "Risolto" or "Chiuso";
            ticket.ClosedAt = closed ? ticket.ClosedAt ?? DateTime.UtcNow : null;
        }

        private static string NormalizeStatus(string? value) =>
            new[] { "Aperto", "In lavorazione", "Risolto", "Chiuso" }.Contains(value) ? value! : "Aperto";

        private static string NormalizePriority(string? value) =>
            new[] { "Bassa", "Normale", "Alta", "Urgente" }.Contains(value) ? value! : "Normale";

        private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

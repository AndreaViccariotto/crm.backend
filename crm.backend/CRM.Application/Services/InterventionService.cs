using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services
{
    public class InterventionService
    {
        private readonly AppDbContext _db;
        private readonly GeneralSettingsService _settings;

        public InterventionService(AppDbContext db, GeneralSettingsService settings)
        {
            _db = db;
            _settings = settings;
        }

        public async Task<List<InterventionResponse>> Get(string? search, string? status, string? visibility, int? ticketId, int? companyId)
        {
            var query = _db.Interventions.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var value = search.Trim();
                query = query.Where(item => item.Number.Contains(value) || item.Title.Contains(value) ||
                    (item.Description != null && item.Description.Contains(value)));
            }
            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(item => item.Status == status);
            if (!string.IsNullOrWhiteSpace(visibility)) query = query.Where(item => item.Visibility == visibility);
            if (ticketId.HasValue) query = query.Where(item => item.TicketId == ticketId);
            if (companyId.HasValue) query = query.Where(item => item.CompanyId == companyId);

            return await query.OrderByDescending(item => item.InterventionDate).Select(ToResponse).ToListAsync();
        }

        public async Task<InterventionResponse?> GetById(int id)
        {
            return await _db.Interventions.AsNoTracking().Where(item => item.Id == id).Select(ToResponse).FirstOrDefaultAsync();
        }

        public async System.Threading.Tasks.Task EnsureFromCompletedTask(Domain.Entities.Task task)
        {
            if (!task.completed || !task.ticket_id.HasValue || await _db.Interventions.AnyAsync(item => item.TaskId == task.Id))
                return;

            var ticket = await _db.Tickets.FindAsync(task.ticket_id.Value);
            if (ticket == null) return;

            var settings = await _settings.GetAssistance();
            var isPublic = settings.PublicActivityTypes.Contains(task.activity_type, StringComparer.OrdinalIgnoreCase);
            var intervention = new Intervention
            {
                Number = await GetNextNumber(),
                TicketId = ticket.Id,
                TaskId = task.Id,
                CompanyId = task.company_id ?? ticket.CompanyId,
                ContactId = task.contact_id ?? ticket.ContactId,
                UserId = task.user_id,
                Title = task.Title,
                Description = task.Description,
                WorkPerformed = task.outcome,
                InternalNotes = task.internal_notes,
                InterventionDate = task.end_date ?? task.due_date,
                StartTime = task.due_time,
                EndTime = task.end_time,
                Location = task.location,
                Visibility = isPublic ? "public" : "internal",
                Status = "Bozza",
                CreatedAt = DateTime.UtcNow
            };

            _db.Interventions.Add(intervention);

            if (settings.AutoCloseTicketWhenAllTasksCompleted &&
                !await _db.Tasks.AnyAsync(item => item.ticket_id == ticket.Id && !item.completed))
            {
                ticket.Status = "Risolto";
                ticket.ClosedAt = DateTime.UtcNow;
                ticket.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<InterventionResponse?> Update(InterventionRequest request)
        {
            var intervention = await _db.Interventions.FindAsync(request.Id);
            if (intervention == null) return null;
            EnsureEditable(intervention);
            if (string.IsNullOrWhiteSpace(request.Title)) throw new Exception("Il titolo dell'intervento e obbligatorio");

            intervention.Title = request.Title.Trim();
            intervention.Description = Clean(request.Description);
            intervention.WorkPerformed = Clean(request.WorkPerformed);
            intervention.InternalNotes = Clean(request.InternalNotes);
            intervention.InterventionDate = request.InterventionDate;
            intervention.StartTime = Clean(request.StartTime);
            intervention.EndTime = Clean(request.EndTime);
            intervention.Location = Clean(request.Location);
            intervention.Visibility = request.Visibility == "internal" ? "internal" : "public";
            intervention.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return await GetById(intervention.Id);
        }

        public async Task<InterventionResponse?> Send(int id)
        {
            var intervention = await _db.Interventions.FindAsync(id);
            if (intervention == null) return null;
            EnsureEditable(intervention);
            if (intervention.Visibility != "public")
                throw new Exception("Un intervento interno non puo essere inviato");

            intervention.Status = "Inviato";
            intervention.SentAt = DateTime.UtcNow;
            intervention.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return await GetById(intervention.Id);
        }

        public async Task<string> Delete(int id)
        {
            var intervention = await _db.Interventions.FindAsync(id);
            if (intervention == null) return "Intervento non trovato";
            EnsureEditable(intervention);
            _db.Interventions.Remove(intervention);
            await _db.SaveChangesAsync();
            return "Intervento eliminato con successo";
        }

        private async Task<string> GetNextNumber()
        {
            var prefix = $"INT-{DateTime.UtcNow:yyyyMMdd}-";
            var count = await _db.Interventions.CountAsync(item => item.Number.StartsWith(prefix));
            return $"{prefix}{count + 1:0000}";
        }

        private static void EnsureEditable(Intervention intervention)
        {
            if (intervention.Status == "Inviato")
                throw new Exception("L'intervento inviato non puo essere modificato");
        }

        private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static readonly System.Linq.Expressions.Expression<Func<Intervention, InterventionResponse>> ToResponse =
            item => new InterventionResponse
            {
                Id = item.Id,
                Number = item.Number,
                TicketId = item.TicketId,
                TicketNumber = item.Ticket != null ? item.Ticket.Number : "",
                TaskId = item.TaskId,
                CompanyId = item.CompanyId,
                CompanyName = item.Company != null ? item.Company.name : null,
                ContactId = item.ContactId,
                ContactName = item.Contact != null ? item.Contact.Name : null,
                UserId = item.UserId,
                UserName = item.User != null ? item.User.username : null,
                Title = item.Title,
                Description = item.Description,
                WorkPerformed = item.WorkPerformed,
                InternalNotes = item.InternalNotes,
                InterventionDate = item.InterventionDate,
                StartTime = item.StartTime,
                EndTime = item.EndTime,
                Location = item.Location,
                Visibility = item.Visibility,
                Status = item.Status,
                SentAt = item.SentAt,
                CreatedAt = item.CreatedAt
            };
    }
}

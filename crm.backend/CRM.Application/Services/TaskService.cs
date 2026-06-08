using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services
{
    public class TaskService
    {
        private readonly AppDbContext _db;
        private readonly CustomFieldService _customFields;
        private readonly InterventionService _interventions;

        public TaskService(AppDbContext db, CustomFieldService customFields, InterventionService interventions)
        {
            _db = db;
            _customFields = customFields;
            _interventions = interventions;
        }

        public async Task<List<TaskResponse>> Get(
            string? activityType = null,
            string? priority = null,
            int? statusId = null,
            int? userId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? search = null) 
        {
            var query = _db.Tasks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(activityType))
                query = query.Where(x => x.activity_type == activityType);

            if (!string.IsNullOrWhiteSpace(priority))
                query = query.Where(x => x.priority == priority);

            if (statusId.HasValue)
                query = query.Where(x => x.status_id == statusId.Value);

            if (userId.HasValue)
                query = query.Where(x => x.user_id == userId.Value);

            if (fromDate.HasValue)
                query = query.Where(x => x.due_date >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(x => x.due_date <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim();
                query = query.Where(x =>
                    x.Title.Contains(normalizedSearch) ||
                    (x.Description != null && x.Description.Contains(normalizedSearch)) ||
                    (x.location != null && x.location.Contains(normalizedSearch)));
            }

            return await query
                .OrderBy(x => x.due_date)
                .ThenBy(x => x.due_time)
                .Select(ToResponseProjection)
                .ToListAsync();
        }

        public async Task<TaskResponse> GetById(int id) 
        {
            var task = await _db.Tasks.FindAsync(id);
            if (task == null)
                return null;

            var response = ToResponse(task);
            response.CustomFields = await _customFields.GetValues("tasks", task.Id);
            return response;  
        }

        public async Task<List<TaskResponse>> GetByCompanyId(int companyId, DateTime? fromDate, DateTime? toDate)
        {
            return await _db.Tasks
                .Where(x => x.company_id == companyId
                                   && (!fromDate.HasValue || x.due_date >= fromDate.Value)
                                                      && (!toDate.HasValue || x.due_date <= toDate.Value))
                .Select(ToResponseProjection)
                .ToListAsync();
        }

        public async Task<List<TaskResponse>> GetByUserId(int userId, DateTime? fromDate, DateTime? toDate)
        {
            return await _db.Tasks
                .Where(x => x.user_id == userId
                    && (!fromDate.HasValue || x.due_date >= fromDate.Value)
                    && (!toDate.HasValue || x.due_date <= toDate.Value))
                .Select(ToResponseProjection)
                .ToListAsync();
        }

        public async Task<string> Save(TaskRequest body)
        {
            await ApplyTicketContext(body);

            var task = new Domain.Entities.Task
            {
                Title = body.Title,
                due_date = body.due_date,
                due_time = body.due_time,
                end_date = body.end_date,
                end_time = body.end_time,
                activity_type = NormalizeActivityType(body.activity_type),
                priority = NormalizePriority(body.priority),
                location = body.location,
                reminder_at = body.reminder_at,
                outcome = body.outcome,
                internal_notes = body.internal_notes,
                all_day = body.all_day,
                Description = body.Description,
                completed = body.completed,
                user_id = body.user_id,
                company_id = body.company_id,
                contact_id = body.contact_id,
                status_id = body.status_id,
                ticket_id = body.ticket_id
            };

            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();
            await _customFields.SaveValues("tasks", task.Id, body.CustomFields);
            await _interventions.EnsureFromCompletedTask(task);

            return "AttivitÃƒÂ  creata con successo";
        }

        public async Task<string> Update(TaskRequest body)
        {
            var task = await _db.Tasks.FindAsync(body.Id);
            if (task == null)
                return "AttivitÃ  non trovata";

            await ApplyTicketContext(body);

            task.Title = body.Title;
            task.due_date = body.due_date;
            task.due_time = body.due_time;
            task.end_date = body.end_date;
            task.end_time = body.end_time;
            task.activity_type = NormalizeActivityType(body.activity_type);
            task.priority = NormalizePriority(body.priority);
            task.location = body.location;
            task.reminder_at = body.reminder_at;
            task.outcome = body.outcome;
            task.internal_notes = body.internal_notes;
            task.all_day = body.all_day;
            task.Description = body.Description;
            task.completed = body.completed;
            task.user_id = body.user_id;
            task.company_id = body.company_id;
            task.contact_id = body.contact_id;
            task.status_id = body.status_id;
            task.ticket_id = body.ticket_id;

            await _db.SaveChangesAsync();
            await _customFields.SaveValues("tasks", task.Id, body.CustomFields);
            await _interventions.EnsureFromCompletedTask(task);

            return "AttivitÃƒÂ  aggiornata con successo";  }

        public async Task<string> Delete(int id)
        {
            var task = await _db.Tasks.FindAsync(id);
            if (task == null)
                return "AttivitÃ  non trovata";

            if (await _db.Interventions.AnyAsync(item => item.TaskId == id))
                throw new Exception("L'attivita ha generato un rapporto di intervento e non puo essere eliminata");

            await _customFields.DeleteValues("tasks", id);
            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();

            return "AttivitÃ  eliminata con successo";
        }

        private async System.Threading.Tasks.Task ApplyTicketContext(TaskRequest body)
        {
            if (!body.ticket_id.HasValue)
                return;

            var ticket = await _db.Tickets.AsNoTracking().FirstOrDefaultAsync(item => item.Id == body.ticket_id.Value);
            if (ticket == null)
                throw new Exception("Ticket collegato non trovato");

            body.company_id = ticket.CompanyId;
            body.contact_id = ticket.ContactId;
            if (ticket.AssignedUserId.HasValue)
                body.user_id = ticket.AssignedUserId.Value;
        }
        private static TaskResponse ToResponse(Domain.Entities.Task task)
        {
            return new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                due_date = task.due_date,
                due_time = task.due_time,
                end_date = task.end_date,
                end_time = task.end_time,
                activity_type = task.activity_type,
                priority = task.priority,
                location = task.location,
                reminder_at = task.reminder_at,
                outcome = task.outcome,
                internal_notes = task.internal_notes,
                all_day = task.all_day,
                Description = task.Description,
                completed = task.completed,
                user_id = task.user_id,
                company_id = task.company_id,
                contact_id = task.contact_id,
                status_id = task.status_id,
                ticket_id = task.ticket_id
            };
        }

        private static readonly System.Linq.Expressions.Expression<Func<Domain.Entities.Task, TaskResponse>> ToResponseProjection =
            task => new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                due_date = task.due_date,
                due_time = task.due_time,
                end_date = task.end_date,
                end_time = task.end_time,
                activity_type = task.activity_type,
                priority = task.priority,
                location = task.location,
                reminder_at = task.reminder_at,
                outcome = task.outcome,
                internal_notes = task.internal_notes,
                all_day = task.all_day,
                Description = task.Description,
                completed = task.completed,
                user_id = task.user_id,
                company_id = task.company_id,
                contact_id = task.contact_id,
                status_id = task.status_id,
                ticket_id = task.ticket_id
            };

        private static string NormalizeActivityType(string? activityType)
        {
            return string.IsNullOrWhiteSpace(activityType) ? "generic" : activityType.Trim();
        }

        private static string NormalizePriority(string? priority)
        {
            return string.IsNullOrWhiteSpace(priority) ? "normal" : priority.Trim();
        }
    }
}








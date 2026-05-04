using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services
{
    public class TaskService
    {
        private readonly AppDbContext _db;

        public TaskService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<TaskResponse>> Get() 
        {
            return await _db.Tasks
                .Select(x => new TaskResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    due_date = x.due_date,
                    due_time = x.due_time,
                    Description = x.Description,
                    completed = x.completed,
                    user_id = x.user_id,
                    company_id = x.company_id,
                    contact_id = x.contact_id,
                    status_id = x.status_id
                })
                .ToListAsync();
        }

        public async Task<TaskResponse> GetById(int id) 
        {
            var task = await _db.Tasks.FindAsync(id);
            if (task == null)
                return null;

            return new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                due_date = task.due_date,
                due_time = task.due_time,
                Description = task.Description,
                completed = task.completed,
                user_id = task.user_id,
                company_id = task.company_id,
                contact_id = task.contact_id,
                status_id = task.status_id
            };  
        }

        public async Task<List<TaskResponse>> GetByUserId(int userId, DateTime? fromDate, DateTime? toDate)
        {
            return await _db.Tasks
                .Where(x => x.user_id == userId
                    && (!fromDate.HasValue || x.due_date >= fromDate.Value)
                    && (!toDate.HasValue || x.due_date <= toDate.Value))
                .Select(x => new TaskResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    due_date = x.due_date,
                    due_time = x.due_time,
                    Description = x.Description,
                    completed = x.completed,
                    user_id = x.user_id,
                    company_id = x.company_id,
                    contact_id = x.contact_id,
                    status_id = x.status_id
                })
                .ToListAsync();
        }

        public async Task<string> Save(TaskRequest body)
        {
            var task = new Domain.Entities.Task
            {
                Title = body.Title,
                due_date = body.due_date,
                due_time = body.due_time,
                Description = body.Description,
                completed = body.completed,
                user_id = body.user_id,
                company_id = body.company_id,
                contact_id = body.contact_id,
                status_id = body.status_id
            };

            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();

            return "Attività creata con successo";
        }

        public async Task<string> Update(TaskRequest body)
        {
            var task = await _db.Tasks.FindAsync(body.Id);
            if (task == null)
                return "Attività non trovata";

            task.Title = body.Title;
            task.due_date = body.due_date;
            task.due_time = body.due_time;
            task.Description = body.Description;
            task.completed = body.completed;
            task.user_id = body.user_id;
            task.company_id = body.company_id;
            task.contact_id = body.contact_id;
            task.status_id = body.status_id;

            await _db.SaveChangesAsync();

            return "Attività aggiornata con successo";  
        }

        public async Task<string> Delete(int id)
        {
            var task = await _db.Tasks.FindAsync(id);
            if (task == null)
                return "Attività non trovata";

            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();

            return "Attività eliminata con successo";
        }
    }
}

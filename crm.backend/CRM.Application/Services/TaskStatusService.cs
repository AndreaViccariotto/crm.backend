using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services
{
    public class TaskStatusService
    {
        private readonly AppDbContext _db;

        public TaskStatusService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<TaskStatusResponse>> get()
        {
            return await _db.Task_Statuses.Select(x => new TaskStatusResponse
            {
                Id = x.Id,
                Name = x.Name,
                is_default = x.Is_Default,
                position = x.Position
            }).ToListAsync();
        }
    }
}

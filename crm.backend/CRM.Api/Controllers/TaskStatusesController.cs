using crm.backend.CRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace crm.backend.CRM.Api.Controllers
{
    [Route("api/[controller]")]
    public class TaskStatusesController : ControllerBase
    {
        private readonly TaskStatusService _service; 

        public TaskStatusesController(TaskStatusService service)
        {
            _service = service;
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            var taskStatuses = await _service.get();
            return Ok(taskStatuses);
        }
    }
}

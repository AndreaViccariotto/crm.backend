using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace crm.backend.CRM.Api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly CommercialNotificationService _service;

        public NotificationsController(CommercialNotificationService service)
        {
            _service = service;
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            return Ok(await _service.Get());
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpPost("dismiss")]
        public async Task<IActionResult> Dismiss([FromBody] NotificationDismissRequest request)
        {
            return Ok(await _service.Dismiss(request));
        }
    }
}

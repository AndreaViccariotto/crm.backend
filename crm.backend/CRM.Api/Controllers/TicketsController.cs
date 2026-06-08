using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace crm.backend.CRM.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "USER,ADMIN")]
    [Route("api/tickets")]
    public class TicketsController : ControllerBase
    {
        private readonly TicketService _service;
        private readonly AccessControlService _accessControl;

        public TicketsController(TicketService service, AccessControlService accessControl)
        {
            _service = service;
            _accessControl = accessControl;
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] string? status, [FromQuery] string? priority, [FromQuery] int? companyId) =>
            Ok(await _service.Get(search, status, priority, companyId));

        [HttpGet("getById")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            var result = await _service.GetById(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("next-number")]
        public async Task<IActionResult> NextNumber() => Ok(new { number = await _service.GetNextNumber() });

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] TicketRequest request) => Ok(await _service.Save(request));

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] TicketRequest request)
        {
            var result = await _service.Update(request);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            if (!await _accessControl.HasPermission("crm.delete")) return Forbid();
            return Ok(await _service.Delete(id));
        }
    }
}
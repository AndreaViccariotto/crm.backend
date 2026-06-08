using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace crm.backend.CRM.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "USER,ADMIN")]
    [Route("api/interventions")]
    public class InterventionsController : ControllerBase
    {
        private readonly InterventionService _service;
        private readonly AccessControlService _accessControl;

        public InterventionsController(InterventionService service, AccessControlService accessControl)
        {
            _service = service;
            _accessControl = accessControl;
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] string? status, [FromQuery] string? visibility, [FromQuery] int? ticketId, [FromQuery] int? companyId) =>
            Ok(await _service.Get(search, status, visibility, ticketId, companyId));

        [HttpGet("getById")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            var result = await _service.GetById(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] InterventionRequest request)
        {
            var result = await _service.Update(request);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromQuery] int id)
        {
            var result = await _service.Send(id);
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
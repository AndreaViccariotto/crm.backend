using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace crm.backend.CRM.Api.Controllers
{
    [ApiController]
    [Route("api/quotes")]
    public class QuotesController : ControllerBase
    {
        private readonly QuoteService _service;
        private readonly AccessControlService _accessControl;

        public QuotesController(QuoteService service, AccessControlService accessControl)
        {
            _service = service;
            _accessControl = accessControl;
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpGet("get")]
        public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] string? status, [FromQuery] int? companyId, [FromQuery] int? contactId, [FromQuery] DateTime? validUntilFrom, [FromQuery] DateTime? validUntilTo)
        {
            return Ok(await _service.Get(search, status, companyId, contactId, validUntilFrom, validUntilTo));
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpGet("getById")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            var quote = await _service.GetById(id);
            return quote == null ? NotFound() : Ok(quote);
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpGet("next-number")]
        public async Task<IActionResult> GetNextNumber()
        {
            return Ok(new { number = await _service.GetNextNumber() });
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] QuoteRequest request)
        {
            return Ok(await _service.Save(request));
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] QuoteRequest request)
        {
            var quote = await _service.Update(request);
            return quote == null ? NotFound() : Ok(quote);
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            if (!await _accessControl.HasPermission("crm.delete"))
                return Forbid();

            return Ok(await _service.Delete(id));
        }
    }
}


using crm.backend.CRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace crm.backend.CRM.Api.Controllers
{
    [ApiController]
    [Route("api/sales-orders")]
    public class SalesOrdersController : ControllerBase
    {
        private readonly SalesOrderService _service;

        public SalesOrdersController(SalesOrderService service)
        {
            _service = service;
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpGet("get")]
        public async Task<IActionResult> Get([FromQuery] int? companyId, [FromQuery] int? contactId, [FromQuery] string? status)
        {
            return Ok(await _service.Get(companyId, contactId, status));
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpGet("getById")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            var order = await _service.GetById(id);
            return order == null ? NotFound() : Ok(order);
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpPost("updateStatus")]
        public async Task<IActionResult> UpdateStatus([FromQuery] int id, [FromQuery] string status)
        {
            return Ok(await _service.UpdateStatus(id, status));
        }
    }
}

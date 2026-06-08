using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace crm.backend.CRM.Api.Controllers
{
    [ApiController]
    [Route("api/purchase-orders")]
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly PurchaseOrderService _service;
        private readonly AccessControlService _accessControl;

        public PurchaseOrdersController(PurchaseOrderService service, AccessControlService accessControl)
        {
            _service = service;
            _accessControl = accessControl;
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpGet("get")]
        public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] string? status)
        {
            return Ok(await _service.Get(search, status));
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpGet("getById")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            var order = await _service.GetById(id);
            return order == null ? NotFound() : Ok(order);
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] PurchaseOrderRequest request)
        {
            return Ok(await _service.Save(request));
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] PurchaseOrderRequest request)
        {
            var order = await _service.Update(request);
            return order == null ? NotFound() : Ok(order);
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

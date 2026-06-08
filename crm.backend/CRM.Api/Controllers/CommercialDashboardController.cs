using crm.backend.CRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace crm.backend.CRM.Api.Controllers
{
    [ApiController]
    [Route("api/commercial-dashboard")]
    public class CommercialDashboardController : ControllerBase
    {
        private readonly CommercialDashboardService _service;

        public CommercialDashboardController(CommercialDashboardService service)
        {
            _service = service;
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            return Ok(await _service.Get());
        }
    }
}

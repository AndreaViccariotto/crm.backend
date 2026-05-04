using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Application.Services;
using crm.backend.CRM.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace crm.backend.CRM.Api.Controllers
{
    [ApiController]
    [Route("api/companies")]
    public class CompaniesController : ControllerBase
    {
        private readonly CompanyService _service;

        public CompaniesController(CompanyService service)
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
        [HttpGet("getById")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            return Ok(await _service.GetById(id));
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] CompanyRequest company) 
        {
            var str = await _service.Save(company);
            return Ok(str);
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] CompanyRequest company)
        {
            var str = await _service.Update(company);
            return Ok(str);
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var str = await _service.Delete(id);
            return Ok(str);
        }
    }
}

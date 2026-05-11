using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Application.Services;
using crm.backend.CRM.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace crm.backend.CRM.Api.Controllers
{
    [ApiController]
    [Route("api/contacts")]
    public class ContactsController : ControllerBase
    {
        private readonly ContactService _service;

        public ContactsController(ContactService service)
        {
            _service = service;
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpPost("save")]
        public async Task<IActionResult> Save(ContactRequest request)
        {


            var str = await _service.Save(request);

            return Ok(str);
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpGet("GetById")]
        public async Task<IActionResult> GetById([FromQuery]int id)
        {
            return Ok(await _service.GetById(id));
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpGet("Get")]
        public async Task<IActionResult> Get()
        {
            return Ok(await _service.Get());
        }

        [Authorize(Roles ="USER,ADMIN")]
        [HttpGet("GetByCompanyId")]
        public async Task<IActionResult> GetByCompanyId([FromQuery] int companyId)
        {
            return Ok(await _service.GetByCompanyId(companyId));
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpPost("Update")]
        public async Task<IActionResult> Update(ContactRequest request)
        {
            var str = await _service.Update(request);

            return Ok(str);
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete([FromQuery]int id)
        {
            var str = await _service.Delete(id);

            return Ok(str);
        }
    }
}

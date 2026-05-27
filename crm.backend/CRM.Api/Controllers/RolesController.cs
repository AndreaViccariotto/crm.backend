using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace crm.backend.CRM.Api.Controllers
{
    [ApiController]
    [Route("api/roles")]
    public class RolesController: ControllerBase
    {
        private readonly RoleService _service;

        public RolesController(RoleService service)
        {
            _service = service;
        }

        [Authorize(Roles = "USER, ADMIN")]
        [HttpGet("get")]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _service.Get();
            return Ok(roles);
        }

        [Authorize(Roles = "USER, ADMIN")]
        [HttpGet("getById")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            var role = await _service.GetById(id);
            if (role == null)
                return NotFound();

            return Ok(role);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] RoleRequest roleDto)
        {
            try
            {
                var result = await _service.Save(roleDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] RoleRequest roleDto)
        {
            try
            {
                var result = await _service.Update(roleDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize(Roles = "ADMIN")]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            try
            {
                var result = await _service.Delete(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}

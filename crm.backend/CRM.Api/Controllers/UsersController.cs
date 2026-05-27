using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace crm.backend.CRM.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _service;

        public UsersController(UserService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "USER, ADMIN")]
        [Route("get")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _service.GetUsers();
            return Ok(users);
        }

        [Authorize(Roles = "USER, ADMIN")]
        [HttpGet("GetById")]
        public async Task<IActionResult> GetById([FromQuery]int id)
        {
            var user = await _service.GetUserById(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] UserDto userDto)
        {
            try
            {
                var result = await _service.Register(userDto);
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
                var result = await _service.DeleteUser(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] UserDto userDto)
        {
            try
            {
                var result = await _service.update(userDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("updateRole")]
        public async Task<IActionResult> UpdateRole([FromBody] UserRoleRequest request)
        {
            try
            {
                var result = await _service.UpdateRole(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}

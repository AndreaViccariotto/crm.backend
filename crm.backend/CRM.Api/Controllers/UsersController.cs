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
    }
}

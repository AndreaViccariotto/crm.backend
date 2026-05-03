using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace crm.backend.CRM.Api.Controllers
{
    [Route("api/[controller]")]
    public class TaskController:ControllerBase
    {
        private readonly TaskService _service;



        public TaskController(TaskService service)
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
            var result = await _service.GetById(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }


        [Authorize(Roles = "USER,ADMIN")]
        [HttpGet("getByUserId")]
        public async Task<IActionResult> GetByUserId(
        [FromQuery] int userId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
        {
            var result = await _service.GetByUserId(userId, fromDate, toDate);
            return Ok(result);
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody]TaskRequest body)
        {
            await _service.Save(body);
            return Ok();
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] TaskRequest body)
        {
            var result = await _service.Update(body);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var result = await _service.Delete(id);
            if (!result)
                return NotFound();
            return Ok();
        }

    }
}

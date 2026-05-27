using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace crm.backend.CRM.Api.Controllers
{
    [Route("api/[controller]")]
    public class FileController:ControllerBase
    {
        private readonly FileService _service;

        public FileController(FileService service)
        {
            _service = service;
        }

        [Authorize(Roles = "USER, ADMIN")]
        [HttpGet("download")]
        public async Task<IActionResult> Download([FromQuery] int fileId)
        {
            return Ok(await _service.Download(fileId));
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromBody] FileRequest req)
        {
            var str = await _service.Upload(req);
            return Ok(str);
        }

        [Authorize(Roles = "USER, ADMIN")]
        [HttpGet("GetByCompanyId")]
        public async Task<IActionResult> GetByCompanyId([FromQuery] int companyId)
        {
            return Ok(await _service.GetByCompanyId(companyId));
        }

        [Authorize(Roles = "USER, ADMIN")]
        [HttpGet("GetByTaskId")]
        public async Task<IActionResult> GetByTaskId([FromQuery] int taskId)
        {
            return Ok(await _service.GetByTaskId(taskId));
        }

        [Authorize(Roles = "USER, ADMIN", Policy = "CanDeleteCrm")]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] int fileId)
        {
            await _service.Delete(fileId);
            return Ok();
        }

        [Authorize(Roles = "USER, ADMIN")]
        [HttpPost("updateFileName")]
        public async Task<IActionResult> UpdateFileName([FromBody] FileRequest req)
        {
            await _service.UpdateFileName(req);
            return Ok();
        }
    }
}

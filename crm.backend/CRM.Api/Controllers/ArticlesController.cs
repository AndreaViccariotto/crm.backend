using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace crm.backend.CRM.Api.Controllers
{
    [ApiController]
    [Route("api/articles")]
    public class ArticlesController : ControllerBase
    {
        private readonly ArticleService _service;
        private readonly AccessControlService _accessControl;

        public ArticlesController(ArticleService service, AccessControlService accessControl)
        {
            _service = service;
            _accessControl = accessControl;
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpGet("get")]
        public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] string? category, [FromQuery] bool? active)
        {
            return Ok(await _service.Get(search, category, active));
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpGet("getById")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            var article = await _service.GetById(id);
            return article == null ? NotFound() : Ok(article);
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] ArticleRequest request)
        {
            return Ok(await _service.Save(request));
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] ArticleRequest request)
        {
            var article = await _service.Update(request);
            return article == null ? NotFound() : Ok(article);
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpPost("toggle")]
        public async Task<IActionResult> Toggle([FromQuery] int id)
        {
            var article = await _service.Toggle(id);
            return article == null ? NotFound() : Ok(article);
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

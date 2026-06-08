using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace crm.backend.CRM.Api.Controllers
{
    [ApiController]
    [Route("api/settings")]
    public class SettingsController : ControllerBase
    {
        private readonly GeneralSettingsService _service;
        private readonly CustomFieldService _customFieldService;

        public SettingsController(GeneralSettingsService service, CustomFieldService customFieldService)
        {
            _service = service;
            _customFieldService = customFieldService;
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpGet("general")]
        public async Task<IActionResult> GetGeneral()
        {
            return Ok(await _service.Get());
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("general")]
        public async Task<IActionResult> SaveGeneral([FromBody] GeneralSettingsDto settings)
        {
            return Ok(await _service.Save(settings));
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpGet("commercial")]
        public async Task<IActionResult> GetCommercial()
        {
            return Ok(await _service.GetCommercial());
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpPost("commercial")]
        public async Task<IActionResult> SaveCommercial([FromBody] CommercialSettingsDto settings)
        {
            return Ok(await _service.SaveCommercial(settings));
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpPost("commercial/logo")]
        public async Task<IActionResult> UploadCommercialLogo([FromBody] QuoteLogoUploadRequest request)
        {
            return Ok(await _service.UploadQuoteLogo(request));
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpDelete("commercial/logo")]
        public async Task<IActionResult> DeleteCommercialLogo()
        {
            return Ok(await _service.DeleteQuoteLogo());
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpGet("assistance")]
        public async Task<IActionResult> GetAssistance()
        {
            return Ok(await _service.GetAssistance());
        }

        [Authorize(Roles = "USER,ADMIN")]
        [HttpPost("assistance")]
        public async Task<IActionResult> SaveAssistance([FromBody] AssistanceSettingsDto settings)
        {
            return Ok(await _service.SaveAssistance(settings));
        }
        [Authorize(Roles = "USER,ADMIN")]
        [HttpGet("custom-fields")]
        public async Task<IActionResult> GetCustomFields()
        {
            return Ok(await _customFieldService.GetSettings());
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("custom-fields")]
        public async Task<IActionResult> SaveCustomFields([FromBody] CustomFieldSettingsDto settings)
        {
            return Ok(await _customFieldService.SaveSettings(settings));
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet("client-modules")]
        public async Task<IActionResult> GetClientModules()
        {
            return Ok(await _service.GetClientModules());
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost("client-modules")]
        public async Task<IActionResult> SaveClientModules([FromBody] ClientModulesSaveRequest request)
        {
            return Ok(await _service.SaveClientModules(request));
        }
    }
}





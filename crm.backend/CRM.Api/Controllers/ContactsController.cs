using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Application.Services;
using crm.backend.CRM.Domain.Entities;
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

        [HttpPost]
        public async Task<IActionResult> Create(CreateContactRequest request)
        {
            var contact = new Contact
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone
            };

            var id = await _service.CreateContactAsync(contact, request.CustomFields);

            return Ok(new { id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _service.GetContactAsync(id);
            return Ok(result);
        }
    }
}

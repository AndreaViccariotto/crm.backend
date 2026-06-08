using crm.backend.CRM.Domain.Entities;

namespace crm.backend.CRM.Api.DTO
{
    public class ContactResponse
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int Company_id { get; set; }
        public string company_name { get; set; }
        public Company Company { get; set; }
        public Dictionary<string, string?> CustomFields { get; set; } = new();
    }
}


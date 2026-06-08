namespace crm.backend.CRM.Api.DTO
{
    public class ContactRequest
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int Company_id { get; set; }
        public Dictionary<string, string?> CustomFields { get; set; } = new();
    }
}


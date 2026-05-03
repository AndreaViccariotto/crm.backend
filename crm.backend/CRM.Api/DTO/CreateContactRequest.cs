namespace crm.backend.CRM.Api.DTO
{
    public class CreateContactRequest
    {
        public string Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public Dictionary<int, string> CustomFields { get; set; }
    }
}

namespace crm.backend.CRM.Domain.Entities
{
    public class CustomFieldValue
    {
        public int Id { get; set; }
        public int CustomFieldId { get; set; }
        public int EntityId { get; set; }
        public string EntityName { get; set; } = "";
        public string? Value { get; set; }

        public CustomField? CustomField { get; set; }
    }
}

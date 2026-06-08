namespace crm.backend.CRM.Domain.Entities
{
    public class CustomField
    {
        public int Id { get; set; }
        public string EntityName { get; set; } = "";
        public string FieldName { get; set; } = "";
        public string Label { get; set; } = "";
        public string FieldType { get; set; } = "text";
        public string? Options { get; set; }
        public bool IsRequired { get; set; }
        public bool Active { get; set; } = true;
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<CustomFieldValue> Values { get; set; } = new List<CustomFieldValue>();
    }
}

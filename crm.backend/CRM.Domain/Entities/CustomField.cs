namespace crm.backend.CRM.Domain.Entities
{
    public class CustomField
    {
        public int Id { get; set; }
        public string EntityName { get; set; } 
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public bool IsRequired { get; set; }
    }
}

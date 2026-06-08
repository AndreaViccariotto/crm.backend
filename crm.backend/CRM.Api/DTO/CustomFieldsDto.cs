namespace crm.backend.CRM.Api.DTO
{
    public class CustomFieldSettingsDto
    {
        public List<CustomFieldModuleDto> Modules { get; set; } = new();
        public List<CustomFieldDefinitionDto> Fields { get; set; } = new();
    }

    public class CustomFieldModuleDto
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool Active { get; set; }
    }

    public class CustomFieldDefinitionDto
    {
        public int Id { get; set; }
        public string ModuleName { get; set; } = "";
        public string FieldName { get; set; } = "";
        public string Label { get; set; } = "";
        public string FieldType { get; set; } = "text";
        public string Options { get; set; } = "";
        public bool IsRequired { get; set; }
        public bool Active { get; set; } = true;
        public int SortOrder { get; set; }
    }
}

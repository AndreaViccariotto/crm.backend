namespace crm.backend.CRM.Api.DTO
{
    public class ClientModulesDto
    {
        public List<ClientModuleItemDto> Modules { get; set; } = new();
    }

    public class ClientModuleItemDto
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool Active { get; set; }
    }

    public class ClientModulesSaveRequest
    {
        public List<string> Modules { get; set; } = new();
    }
}

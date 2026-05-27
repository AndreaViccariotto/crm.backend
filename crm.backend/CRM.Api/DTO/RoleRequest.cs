namespace crm.backend.CRM.Api.DTO
{
    public class RoleRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public List<int> PermissionIds { get; set; } = new();
        public List<int> ModuleIds { get; set; } = new();
    }
}

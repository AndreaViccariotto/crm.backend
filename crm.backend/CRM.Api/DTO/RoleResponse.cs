namespace crm.backend.CRM.Api.DTO
{
    public class RoleResponse
    {
        public int id { get; set; }
        public string name { get; set; } = null!;
        public string? description { get; set; }
        public List<RoleAccessItemResponse> permissions { get; set; } = new();
        public List<RoleAccessItemResponse> modules { get; set; } = new();
    }
}

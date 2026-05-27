namespace crm.backend.CRM.Api.DTO
{
    public class AuthenticationDto
    {
        public int Id { get; set; }
        public string Role { get; set; } = null!;
        public List<string> Permissions { get; set; } = new();
        public List<string> Modules { get; set; } = new();
        public string jwt { get; set; } = null!;
    }
}

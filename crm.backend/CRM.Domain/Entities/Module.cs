namespace crm.backend.CRM.Domain.Entities
{
    public class Module
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ICollection<RoleModule> RoleModules { get; set; } = new List<RoleModule>();
    }
}

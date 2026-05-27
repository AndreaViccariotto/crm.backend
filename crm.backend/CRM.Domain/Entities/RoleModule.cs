using System.ComponentModel.DataAnnotations.Schema;

namespace crm.backend.CRM.Domain.Entities
{
    public class RoleModule
    {
        [Column("role_id")]
        public int RoleId { get; set; }

        [Column("module_id")]
        public int ModuleId { get; set; }

        public Role Role { get; set; } = null!;
        public Module Module { get; set; } = null!;
    }
}

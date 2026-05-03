using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace crm.backend.CRM.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string username { get; set; }
        public string? Email { get; set; }
        [Column("password_hash")]
        public string PasswordHash { get; set; }
        [Column("role_id")]
        public int? RoleId { get; set; }
        public Role? Role { get; set; }
    }
}

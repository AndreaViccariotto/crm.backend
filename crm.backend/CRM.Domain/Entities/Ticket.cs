using System.ComponentModel.DataAnnotations.Schema;

namespace crm.backend.CRM.Domain.Entities
{
    public class Ticket
    {
        [Column("id")] public int Id { get; set; }
        [Column("number")] public string Number { get; set; } = "";
        [Column("subject")] public string Subject { get; set; } = "";
        [Column("description")] public string? Description { get; set; }
        [Column("status")] public string Status { get; set; } = "Aperto";
        [Column("priority")] public string Priority { get; set; } = "Normale";
        [Column("company_id")] public int? CompanyId { get; set; }
        [Column("contact_id")] public int? ContactId { get; set; }
        [Column("assigned_user_id")] public int? AssignedUserId { get; set; }
        [Column("assigned_at")] public DateTime? AssignedAt { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Column("updated_at")] public DateTime? UpdatedAt { get; set; }
        [Column("closed_at")] public DateTime? ClosedAt { get; set; }

        public Company? Company { get; set; }
        public Contact? Contact { get; set; }
        public User? AssignedUser { get; set; }
        public ICollection<Task> Tasks { get; set; } = new List<Task>();
        public ICollection<Intervention> Interventions { get; set; } = new List<Intervention>();
    }
}

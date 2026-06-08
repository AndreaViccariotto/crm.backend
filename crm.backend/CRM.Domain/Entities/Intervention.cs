using System.ComponentModel.DataAnnotations.Schema;

namespace crm.backend.CRM.Domain.Entities
{
    public class Intervention
    {
        [Column("id")] public int Id { get; set; }
        [Column("number")] public string Number { get; set; } = "";
        [Column("ticket_id")] public int TicketId { get; set; }
        [Column("task_id")] public int TaskId { get; set; }
        [Column("company_id")] public int? CompanyId { get; set; }
        [Column("contact_id")] public int? ContactId { get; set; }
        [Column("user_id")] public int? UserId { get; set; }
        [Column("title")] public string Title { get; set; } = "";
        [Column("description")] public string? Description { get; set; }
        [Column("work_performed")] public string? WorkPerformed { get; set; }
        [Column("internal_notes")] public string? InternalNotes { get; set; }
        [Column("intervention_date")] public DateTime InterventionDate { get; set; } = DateTime.UtcNow;
        [Column("start_time")] public string? StartTime { get; set; }
        [Column("end_time")] public string? EndTime { get; set; }
        [Column("location")] public string? Location { get; set; }
        [Column("visibility")] public string Visibility { get; set; } = "public";
        [Column("status")] public string Status { get; set; } = "Bozza";
        [Column("sent_at")] public DateTime? SentAt { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Column("updated_at")] public DateTime? UpdatedAt { get; set; }

        public Ticket? Ticket { get; set; }
        public Task? Task { get; set; }
        public Company? Company { get; set; }
        public Contact? Contact { get; set; }
        public User? User { get; set; }
    }
}
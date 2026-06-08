namespace crm.backend.CRM.Api.DTO
{
    public class TicketRequest
    {
        public int Id { get; set; }
        public string Number { get; set; } = "";
        public string Subject { get; set; } = "";
        public string? Description { get; set; }
        public string Status { get; set; } = "Aperto";
        public string Priority { get; set; } = "Normale";
        public int? CompanyId { get; set; }
        public int? ContactId { get; set; }
        public int? AssignedUserId { get; set; }
    }

    public class TicketResponse : TicketRequest
    {
        public string? CompanyName { get; set; }
        public string? ContactName { get; set; }
        public string? AssignedUserName { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int TaskCount { get; set; }
        public int InterventionCount { get; set; }
    }

    public class InterventionRequest
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? WorkPerformed { get; set; }
        public string? InternalNotes { get; set; }
        public DateTime InterventionDate { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? Location { get; set; }
        public string Visibility { get; set; } = "public";
    }

    public class InterventionResponse : InterventionRequest
    {
        public string Number { get; set; } = "";
        public int TicketId { get; set; }
        public string TicketNumber { get; set; } = "";
        public int TaskId { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public int? ContactId { get; set; }
        public string? ContactName { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string Status { get; set; } = "Bozza";
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsLocked => string.Equals(Status, "Inviato", StringComparison.OrdinalIgnoreCase);
        public bool CanSend => string.Equals(Visibility, "public", StringComparison.OrdinalIgnoreCase) && !IsLocked;
    }
}

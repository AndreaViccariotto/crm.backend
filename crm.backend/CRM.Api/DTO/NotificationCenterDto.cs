namespace crm.backend.CRM.Api.DTO
{
    public class NotificationCenterDto
    {
        public int UnreadCount { get; set; }
        public List<NotificationItemDto> Items { get; set; } = new();
    }

    public class NotificationItemDto
    {
        public string Id { get; set; } = "";
        public string Type { get; set; } = "";
        public string Severity { get; set; } = "info";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string? Route { get; set; }
        public int? EntityId { get; set; }
    }

    public class NotificationDismissRequest
    {
        public string Id { get; set; } = "";
    }
}

namespace crm.backend.CRM.Api.DTO
{
    public class TaskRequest
    {
        public int Id { get; set; } = 0;
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime due_date { get; set; }
        public string due_time { get; set; }
        public DateTime? end_date { get; set; }
        public string? end_time { get; set; }
        public string activity_type { get; set; } = "generic";
        public string priority { get; set; } = "normal";
        public string? location { get; set; }
        public DateTime? reminder_at { get; set; }
        public string? outcome { get; set; }
        public string? internal_notes { get; set; }
        public bool all_day { get; set; }
        public bool completed { get; set; }
        public int user_id { get; set; }
        public int? company_id { get; set; }
        public int? contact_id { get; set; }
        public int? status_id { get; set; }
        public int? ticket_id { get; set; }
        public DateTime created_at { get; set;} = DateTime.Now;
        public Dictionary<string, string?> CustomFields { get; set; } = new();
    }
}


namespace crm.backend.CRM.Api.DTO
{
    public class QuoteRequest
    {
        public int Id { get; set; }
        public string Number { get; set; } = "";
        public int? CompanyId { get; set; }
        public int? ContactId { get; set; }
        public string CustomerName { get; set; } = "";
        public DateTime? ValidUntil { get; set; }
        public string Status { get; set; } = "Bozza";
        public List<QuoteLineRequest> Lines { get; set; } = new();
        public Dictionary<string, string?> CustomFields { get; set; } = new();
    }
}


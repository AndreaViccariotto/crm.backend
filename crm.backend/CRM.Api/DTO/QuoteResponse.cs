namespace crm.backend.CRM.Api.DTO
{
    public class QuoteResponse
    {
        public int Id { get; set; }
        public string Number { get; set; } = "";
        public int? CompanyId { get; set; }
        public int? ContactId { get; set; }
        public string CustomerName { get; set; } = "";
        public string? CompanyName { get; set; }
        public string? ContactName { get; set; }
        public DateTime? ValidUntil { get; set; }
        public string Status { get; set; } = "";
        public List<QuoteLineResponse> Lines { get; set; } = new();
        public decimal Total { get; set; }
        public decimal VatTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public Dictionary<string, string?> CustomFields { get; set; } = new();
    }
}


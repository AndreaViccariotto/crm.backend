namespace crm.backend.CRM.Api.DTO
{
    public class ArticleResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal Price { get; set; }
        public decimal VatRate { get; set; }
        public bool Active { get; set; }
        public Dictionary<string, string?> CustomFields { get; set; } = new();
    }
}


namespace crm.backend.CRM.Api.DTO
{
    public class ArticleRequest
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "Servizi";
        public string Unit { get; set; } = "pz";
        public decimal Price { get; set; }
        public decimal VatRate { get; set; } = 22;
        public bool Active { get; set; } = true;
        public Dictionary<string, string?> CustomFields { get; set; } = new();
    }
}


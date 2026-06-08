namespace crm.backend.CRM.Api.DTO
{
    public class QuoteLineRequest
    {
        public int? ArticleId { get; set; }
        public string Description { get; set; } = "";
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal VatRate { get; set; } = 22;
    }
}

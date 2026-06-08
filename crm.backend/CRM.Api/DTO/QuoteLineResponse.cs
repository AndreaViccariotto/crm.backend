namespace crm.backend.CRM.Api.DTO
{
    public class QuoteLineResponse
    {
        public int Id { get; set; }
        public int? ArticleId { get; set; }
        public string Description { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal VatRate { get; set; }
        public decimal LineTotal { get; set; }
    }
}

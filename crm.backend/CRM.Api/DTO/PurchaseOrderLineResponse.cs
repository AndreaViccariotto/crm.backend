namespace crm.backend.CRM.Api.DTO
{
    public class PurchaseOrderLineResponse
    {
        public int Id { get; set; }
        public int? ArticleId { get; set; }
        public string Description { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal VatRate { get; set; }
        public decimal LineTotal { get; set; }
    }
}

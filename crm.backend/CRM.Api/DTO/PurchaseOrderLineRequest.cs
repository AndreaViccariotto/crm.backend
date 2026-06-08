namespace crm.backend.CRM.Api.DTO
{
    public class PurchaseOrderLineRequest
    {
        public int? ArticleId { get; set; }
        public string Description { get; set; } = "";
        public decimal Quantity { get; set; } = 1;
        public decimal UnitCost { get; set; }
        public decimal VatRate { get; set; } = 22;
    }
}

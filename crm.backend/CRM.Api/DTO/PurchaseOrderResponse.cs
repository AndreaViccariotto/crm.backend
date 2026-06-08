namespace crm.backend.CRM.Api.DTO
{
    public class PurchaseOrderResponse
    {
        public int Id { get; set; }
        public string Number { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = "";
        public string? Notes { get; set; }
        public List<PurchaseOrderLineResponse> Lines { get; set; } = new();
        public decimal Total { get; set; }
        public decimal VatTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public Dictionary<string, string?> CustomFields { get; set; } = new();
    }
}


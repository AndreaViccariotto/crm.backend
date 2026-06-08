namespace crm.backend.CRM.Api.DTO
{
    public class PurchaseOrderRequest
    {
        public int Id { get; set; }
        public string Number { get; set; } = "";
        public string SupplierName { get; set; } = "";
        public DateTime? OrderDate { get; set; }
        public string Status { get; set; } = "Bozza";
        public string? Notes { get; set; }
        public List<PurchaseOrderLineRequest> Lines { get; set; } = new();
        public Dictionary<string, string?> CustomFields { get; set; } = new();
    }
}


namespace crm.backend.CRM.Api.DTO
{
    public class GeneralSettingsDto
    {
        public string CompanyName { get; set; } = "";
        public string Currency { get; set; } = "EUR";
        public decimal DefaultVatRate { get; set; } = 22;
        public string QuotePrefix { get; set; } = "PREV";
        public string SalesOrderPrefix { get; set; } = "OV";
        public string PurchaseOrderPrefix { get; set; } = "OA";
        public string PaymentTerms { get; set; } = "";
        public string QuoteFooterNotes { get; set; } = "";
    }
}

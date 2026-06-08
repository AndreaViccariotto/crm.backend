namespace crm.backend.CRM.Api.DTO
{
    public class CommercialDashboardDto
    {
        public int OpenQuotes { get; set; }
        public int SentQuotes { get; set; }
        public int AcceptedQuotes { get; set; }
        public int LostQuotes { get; set; }
        public int SalesOrdersToFulfill { get; set; }
        public int DisabledArticles { get; set; }
        public decimal ConversionRate { get; set; }
        public decimal PipelineValue { get; set; }
        public decimal AcceptedValue { get; set; }
        public decimal LostValue { get; set; }
        public List<CommercialDashboardStatusDto> QuoteStatusTotals { get; set; } = new();
        public List<CommercialDashboardItemDto> BestCustomers { get; set; } = new();
        public List<CommercialDashboardItemDto> TopArticles { get; set; } = new();
    }

    public class CommercialDashboardStatusDto
    {
        public string Status { get; set; } = "";
        public int Count { get; set; }
        public decimal Value { get; set; }
    }

    public class CommercialDashboardItemDto
    {
        public string Label { get; set; } = "";
        public decimal Value { get; set; }
        public decimal Quantity { get; set; }
        public int Count { get; set; }
    }
}

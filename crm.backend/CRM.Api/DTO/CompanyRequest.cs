namespace crm.backend.CRM.Api.DTO
{
    public class CompanyRequest
    {
        public int Id { get; set; } = 0;
        public string name { get; set; }
        public string vat_number { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public Dictionary<string, string?> CustomFields { get; set; } = new();
    }
}


namespace crm.backend.CRM.Api.DTO
{
    public class CompanyResponse
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string vat_number { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string city { get; set; }
    }
}

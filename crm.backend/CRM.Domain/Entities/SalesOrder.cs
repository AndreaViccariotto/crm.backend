using System.ComponentModel.DataAnnotations.Schema;

namespace crm.backend.CRM.Domain.Entities
{
    public class SalesOrder
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("number")]
        public string Number { get; set; } = null!;
        [Column("quote_id")]
        public int QuoteId { get; set; }
        [Column("company_id")]
        public int? CompanyId { get; set; }
        [Column("contact_id")]
        public int? ContactId { get; set; }
        [Column("customer_name")]
        public string CustomerName { get; set; } = null!;
        [Column("status")]
        public string Status { get; set; } = "Da evadere";
        [Column("order_date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Quote Quote { get; set; } = null!;
        public Company? Company { get; set; }
        public Contact? Contact { get; set; }
        public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
    }
}

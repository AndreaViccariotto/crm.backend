using System.ComponentModel.DataAnnotations.Schema;

namespace crm.backend.CRM.Domain.Entities
{
    public class Quote
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("number")]
        public string Number { get; set; } = null!;
        [Column("company_id")]
        public int? CompanyId { get; set; }
        [Column("contact_id")]
        public int? ContactId { get; set; }
        [Column("customer_name")]
        public string CustomerName { get; set; } = null!;
        [Column("valid_until")]
        public DateTime? ValidUntil { get; set; }
        [Column("status")]
        public string Status { get; set; } = "Bozza";
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        public Company? Company { get; set; }
        public Contact? Contact { get; set; }
        public SalesOrder? SalesOrder { get; set; }
        public ICollection<QuoteLine> Lines { get; set; } = new List<QuoteLine>();
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace crm.backend.CRM.Domain.Entities
{
    public class Article
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("code")]
        public string Code { get; set; } = null!;
        [Column("name")]
        public string Name { get; set; } = null!;
        [Column("category")]
        public string Category { get; set; } = "Servizi";
        [Column("unit")]
        public string Unit { get; set; } = "pz";
        [Column("price")]
        public decimal Price { get; set; }
        [Column("vat_rate")]
        public decimal VatRate { get; set; } = 22;
        [Column("active")]
        public bool Active { get; set; } = true;
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<QuoteLine> QuoteLines { get; set; } = new List<QuoteLine>();
    }
}

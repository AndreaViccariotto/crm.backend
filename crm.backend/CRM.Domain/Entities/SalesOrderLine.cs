using System.ComponentModel.DataAnnotations.Schema;

namespace crm.backend.CRM.Domain.Entities
{
    public class SalesOrderLine
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("sales_order_id")]
        public int SalesOrderId { get; set; }
        [Column("article_id")]
        public int? ArticleId { get; set; }
        [Column("description")]
        public string Description { get; set; } = null!;
        [Column("quantity")]
        public decimal Quantity { get; set; } = 1;
        [Column("unit_price")]
        public decimal UnitPrice { get; set; }
        [Column("discount")]
        public decimal Discount { get; set; }
        [Column("vat_rate")]
        public decimal VatRate { get; set; } = 22;
        [Column("sort_order")]
        public int SortOrder { get; set; }

        public SalesOrder SalesOrder { get; set; } = null!;
        public Article? Article { get; set; }
    }
}

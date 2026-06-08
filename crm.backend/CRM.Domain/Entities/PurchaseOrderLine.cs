using System.ComponentModel.DataAnnotations.Schema;

namespace crm.backend.CRM.Domain.Entities
{
    public class PurchaseOrderLine
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("purchase_order_id")]
        public int PurchaseOrderId { get; set; }
        [Column("article_id")]
        public int? ArticleId { get; set; }
        [Column("description")]
        public string Description { get; set; } = null!;
        [Column("quantity")]
        public decimal Quantity { get; set; } = 1;
        [Column("unit_cost")]
        public decimal UnitCost { get; set; }
        [Column("vat_rate")]
        public decimal VatRate { get; set; } = 22;
        [Column("sort_order")]
        public int SortOrder { get; set; }

        public PurchaseOrder PurchaseOrder { get; set; } = null!;
        public Article? Article { get; set; }
    }
}

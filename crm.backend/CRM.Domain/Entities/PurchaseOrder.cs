using System.ComponentModel.DataAnnotations.Schema;

namespace crm.backend.CRM.Domain.Entities
{
    public class PurchaseOrder
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("number")]
        public string Number { get; set; } = null!;
        [Column("supplier_name")]
        public string SupplierName { get; set; } = null!;
        [Column("order_date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        [Column("status")]
        public string Status { get; set; } = "Bozza";
        [Column("notes")]
        public string? Notes { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
    }
}

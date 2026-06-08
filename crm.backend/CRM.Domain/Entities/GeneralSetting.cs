using System.ComponentModel.DataAnnotations.Schema;

namespace crm.backend.CRM.Domain.Entities
{
    public class GeneralSetting
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("setting_key")]
        public string Key { get; set; } = null!;
        [Column("setting_value")]
        public string? Value { get; set; }
    }
}

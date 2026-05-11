using System.ComponentModel.DataAnnotations.Schema;

namespace crm.backend.CRM.Domain.Entities
{
    public class Contact
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public int? Company_Id { get; set; }

        [ForeignKey(nameof(Company_Id))]
        public Company? Company { get; set; }
    }
}

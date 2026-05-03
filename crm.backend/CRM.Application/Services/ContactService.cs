using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services
{
    public class ContactService
    {
        private readonly AppDbContext _db;

        public ContactService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateContactAsync(Contact contact, Dictionary<int, string> customFields)
        {
            _db.Contacts.Add(contact);
            await _db.SaveChangesAsync();

            foreach (var field in customFields)
            {
                _db.CustomFieldValues.Add(new CustomFieldValue
                {
                    CustomFieldId = field.Key,
                    EntityId = contact.Id,
                    EntityName = "contacts",
                    Value = field.Value
                });
            }

            await _db.SaveChangesAsync();

            return contact.Id;
        }

        public async Task<object> GetContactAsync(int id)
        {
            var contact = await _db.Contacts.FindAsync(id);

            var customValues = await _db.CustomFieldValues
                .Where(x => x.EntityId == id && x.EntityName == "contacts")
                .ToListAsync();

            return new
            {
                contact,
                customFields = customValues
            };
        }
    }
}

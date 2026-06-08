using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services
{
    public class ContactService
    {
        private readonly AppDbContext _db;
        private readonly CustomFieldService _customFields;

        public ContactService(AppDbContext db, CustomFieldService customFields)
        {
            _db = db;
            _customFields = customFields;
        }

        public async Task<string> Save(ContactRequest contact)
        {

            var newContact = new Contact
            {
                Name = contact.Name,
                Email = contact.Email,
                Phone = contact.Phone,
                Company_Id = contact.Company_id
            };

            _db.Contacts.Add(newContact);

            await _db.SaveChangesAsync();
            await _customFields.SaveValues("contacts", newContact.Id, contact.CustomFields);

            return "Contatto creato con successo.";
        }

        public async Task<ContactResponse?> GetById(int id)
        {
            var contact = await _db.Contacts
                .Include(c => c.Company)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contact == null)
                return null;

            return new ContactResponse
            {
                id = contact.Id,
                Name = contact.Name,
                Email = contact.Email,
                Phone = contact.Phone,
                Company_id = contact.Company_Id ?? 0,
                company_name = contact.Company != null
                    ? contact.Company.name
                    : null,

                Company = contact.Company,
                CustomFields = await _customFields.GetValues("contacts", contact.Id)
            };
        }

        public async Task<List<ContactResponse>> GetByCompanyId(int companyId)
        {
            var contacts = await _db.Contacts
                .Where(c => c.Company_Id == companyId)
                .Include(c => c.Company)
                .Select(contact => new ContactResponse
                {
                    id = contact.Id,
                    Name = contact.Name,
                    Email = contact.Email,
                    Phone = contact.Phone,
                    Company_id = contact.Company_Id ?? 0,
                    company_name = contact.Company != null
                        ? contact.Company.name
                        : null,

                    Company = contact.Company
                })
                .ToListAsync();

            return contacts;
        }

        public async Task<List<ContactResponse>> Get()
        {
            var contacts = await _db.Contacts
                .Include(c => c.Company)
                .Select(contact => new ContactResponse
                {
                    id = contact.Id,
                    Name = contact.Name,
                    Email = contact.Email,
                    Phone = contact.Phone,
                    Company_id = contact.Company_Id ?? 0,
                    company_name = contact.Company != null
                        ? contact.Company.name
                        : null,

                    Company = contact.Company
                })
                .ToListAsync();

            return contacts;
        }

        public async Task<string> Update (ContactRequest request)
        {
            var contact = await _db.Contacts.FindAsync(request.id);

            if (contact == null)
                return null;

            contact.Name = request.Name;
            contact.Email = request.Email;
            contact.Phone = request.Phone;
            contact.Company_Id = request.Company_id;

            await _db.SaveChangesAsync();
            await _customFields.SaveValues("contacts", contact.Id, request.CustomFields);

            return "Contatto aggiornato con successo";
        }

        public async Task<string> Delete(int id)
        {
                var contact = await _db.Contacts.FindAsync(id);
    
                if (contact == null)
                    return null;
    
                await _customFields.DeleteValues("contacts", id);
                _db.Contacts.Remove(contact);
                await _db.SaveChangesAsync();
    
                return "Contatto eliminato con successo";
        }
    }
}









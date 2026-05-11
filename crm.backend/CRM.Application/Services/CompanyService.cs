using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services
{
    public class CompanyService
    {
        private readonly AppDbContext _db;

        public CompanyService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<CompanyResponse>> Get()
        {
            return await _db.Companies
                .Select(x => new CompanyResponse
                {
                    Id = x.Id,
                    name = x.name,
                    vat_number = x.vat_number,
                    email = x.email,
                    phone = x.phone,
                    address = x.address,
                    city = x.city
                })
                .ToListAsync();
        }

        public async Task<CompanyResponse> GetById(int id)
        {
            var company = await _db.Companies.FindAsync(id);
            if (company == null)
                return null;

            return new CompanyResponse
            {
                Id = company.Id,
                name = company.name,
                vat_number = company.vat_number,
                email = company.email,
                phone = company.phone,
                address = company.address,
                city = company.city
            };
        }

        public async Task<List<ContactResponse>> GetContacts(int companyId)
        {
            return await _db.Contacts
                .Where(c => c.Company_Id == companyId)
                .Select(c => new ContactResponse
                {
                    id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Phone = c.Phone,
                    Company_id = c.Company_Id ?? 0,
                    company_name = c.Company != null ? c.Company.name : null
                })
                .ToListAsync();
        }

        public async Task<string> Save(CompanyRequest request)
        {
                var company = new Company
                {
                    name = request.name,
                    vat_number = request.vat_number,
                    email = request.email,
                    phone = request.phone,
                    address = request.address,
                    city = request.city,
                    created_at = DateTime.UtcNow,
                };
    
                _db.Companies.Add(company);
                await _db.SaveChangesAsync();
    
                return "Azienda salvata con successo";
        }

        public async Task<string> Update(CompanyRequest request)
        {
            var company = await _db.Companies.FindAsync(request.Id);
            if (company == null)
                return "Azienda non trovata";

            company.name = request.name;
            company.vat_number = request.vat_number;
            company.email = request.email;
            company.phone = request.phone;
            company.address = request.address;
            company.city = request.city;

            await _db.SaveChangesAsync();

            return "Azienda aggiornata con successo";
        }

        public async Task<string> Delete(int id)
        {
            var company = await _db.Companies.FindAsync(id);
            if (company == null)
                return "Azienda non trovata";

            _db.Companies.Remove(company);
            await _db.SaveChangesAsync();

            return "Azienda eliminata con successo";
        }
    }
}

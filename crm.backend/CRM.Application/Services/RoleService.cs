using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services
{
    public class RoleService
    {

        private readonly AppDbContext _db;

        public RoleService(AppDbContext context)
        {
            _db = context;
        }

        public async Task<List<RoleResponse>> Get()
        {
            return await _db.Roles
                .Select(r => new RoleResponse 
                {
                    id = r.Id,
                    name = r.Name,
                    description = r.Description
                }).ToListAsync();
        }

        public async Task<RoleResponse> GetById(int id)
        {
            var role = await _db.Roles.FindAsync(id);
            if (role == null)
                return null;

            return new RoleResponse
            {
                id = role.Id,
                name = role.Name,
                description = role.Description
            };
        }

        public async Task<RoleResponse> Save(RoleRequest roleDto)
        {
            var role = new Role
            {
                Name = roleDto.Name,
                Description = roleDto.Description
            };

            _db.Roles.Add(role);
            await _db.SaveChangesAsync();

            return new RoleResponse
            {
                id = role.Id,
                name = role.Name,
                description = role.Description
            };
        }

        public async Task<RoleResponse> Update(RoleRequest roleDto)
        {
            var role = await _db.Roles.FindAsync(roleDto.Id);
            if (role == null)
                throw new Exception("Role not found");

            role.Name = roleDto.Name;
            role.Description = roleDto.Description;

            await _db.SaveChangesAsync();

            return new RoleResponse
            {
                id = role.Id,
                name = role.Name,
                description = role.Description
            };
        }

        public async Task<bool> Delete(int id)
        {
            var role = await _db.Roles.FindAsync(id);
            if (role == null)
                throw new Exception("Role not found");

            _db.Roles.Remove(role);
            await _db.SaveChangesAsync();

            return true;
        }
    }
}

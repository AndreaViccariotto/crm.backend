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
            var roles = await _db.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .Include(r => r.RoleModules)
                    .ThenInclude(rm => rm.Module)
                .ToListAsync();

            return roles.Select(ToResponse).ToList();
        }

        public async Task<RoleResponse> GetById(int id)
        {
            var role = await _db.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .Include(r => r.RoleModules)
                    .ThenInclude(rm => rm.Module)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
                return null;

            return ToResponse(role);
        }

        public async Task<RoleResponse> Save(RoleRequest roleDto)
        {
            await ValidateRoleRequest(roleDto);

            var role = new Role
            {
                Name = roleDto.Name,
                Description = roleDto.Description,
                RolePermissions = roleDto.PermissionIds.Distinct().Select(permissionId => new RolePermission
                {
                    PermissionId = permissionId
                }).ToList(),
                RoleModules = roleDto.ModuleIds.Distinct().Select(moduleId => new RoleModule
                {
                    ModuleId = moduleId
                }).ToList()
            };

            _db.Roles.Add(role);
            await _db.SaveChangesAsync();

            return await GetById(role.Id);
        }

        public async Task<RoleResponse> Update(RoleRequest roleDto)
        {
            await ValidateRoleRequest(roleDto);

            var role = await _db.Roles
                .Include(r => r.RolePermissions)
                .Include(r => r.RoleModules)
                .FirstOrDefaultAsync(r => r.Id == roleDto.Id);

            if (role == null)
                throw new Exception("Role not found");

            role.Name = roleDto.Name;
            role.Description = roleDto.Description;

            role.RolePermissions.Clear();
            foreach (var permissionId in roleDto.PermissionIds.Distinct())
            {
                role.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permissionId
                });
            }

            role.RoleModules.Clear();
            foreach (var moduleId in roleDto.ModuleIds.Distinct())
            {
                role.RoleModules.Add(new RoleModule
                {
                    RoleId = role.Id,
                    ModuleId = moduleId
                });
            }

            await _db.SaveChangesAsync();

            return await GetById(role.Id);
        }

        public async Task<bool> Delete(int id)
        {
            var role = await _db.Roles
                .Include(r => r.RolePermissions)
                .Include(r => r.RoleModules)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
                throw new Exception("Role not found");

            var hasUsers = await _db.Users.AnyAsync(u => u.RoleId == id);
            if (hasUsers)
                throw new Exception("Non puoi eliminare un ruolo assegnato ad almeno un utente");

            role.RolePermissions.Clear();
            role.RoleModules.Clear();
            _db.Roles.Remove(role);
            await _db.SaveChangesAsync();

            return true;
        }

        private async System.Threading.Tasks.Task ValidateRoleRequest(RoleRequest roleDto)
        {
            if (string.IsNullOrWhiteSpace(roleDto.Name))
                throw new Exception("Il nome del ruolo è obbligatorio");

            var normalizedRoleName = roleDto.Name.Trim().ToUpper();
            var roleNameExists = await _db.Roles
                .AnyAsync(r => r.Name.ToUpper() == normalizedRoleName && r.Id != roleDto.Id);

            if (roleNameExists)
                throw new Exception("Esiste già un ruolo con questo nome");

            var permissionIds = roleDto.PermissionIds.Distinct().ToList();
            var existingPermissionIds = await _db.Permissions
                .Where(p => permissionIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            var missingPermissionIds = permissionIds.Except(existingPermissionIds).ToList();
            if (missingPermissionIds.Any())
                throw new Exception($"Permessi non trovati: {string.Join(", ", missingPermissionIds)}");

            var moduleIds = roleDto.ModuleIds.Distinct().ToList();
            var existingModuleIds = await _db.Modules
                .Where(m => moduleIds.Contains(m.Id))
                .Select(m => m.Id)
                .ToListAsync();

            var missingModuleIds = moduleIds.Except(existingModuleIds).ToList();
            if (missingModuleIds.Any())
                throw new Exception($"Moduli non trovati: {string.Join(", ", missingModuleIds)}");
        }

        private static RoleResponse ToResponse(Role role)
        {
            return new RoleResponse
            {
                id = role.Id,
                name = role.Name,
                description = role.Description,
                permissions = role.RolePermissions
                    .Where(rp => rp.Permission != null)
                    .Select(rp => new RoleAccessItemResponse
                    {
                        id = rp.Permission.Id,
                        name = rp.Permission.Name,
                        description = rp.Permission.Description
                    })
                    .ToList(),
                modules = role.RoleModules
                    .Where(rm => rm.Module != null)
                    .Select(rm => new RoleAccessItemResponse
                    {
                        id = rm.Module.Id,
                        name = rm.Module.Name,
                        description = rm.Module.Description
                    })
                    .ToList()
            };
        }
    }
}

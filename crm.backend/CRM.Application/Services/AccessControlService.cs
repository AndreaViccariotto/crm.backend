using System.Security.Claims;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services
{
    public class AccessControlService
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccessControlService(AppDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> HasPermission(string permissionName)
        {
            var userIdValue = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
                return false;

            return await _db.Users
                .Where(user => user.Id == userId)
                .AnyAsync(user => user.Role != null &&
                    user.Role.RolePermissions.Any(rolePermission =>
                        rolePermission.Permission.Name == permissionName));
        }
    }
}

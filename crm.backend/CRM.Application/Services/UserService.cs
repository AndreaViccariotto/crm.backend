using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Application.Services
{
    public class UserService
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;

        public UserService(AppDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        public async Task<string> Register(UserDto body)
        {
            if (await _db.Users.AnyAsync(x => x.username == body.Username))
                throw new Exception("utente già esistente");

            await EnsureRoleExists(body.RoleId);

            var user = new User
            {
                username = body.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(body.Password),
                RoleId = body.RoleId,
                Email = body.Email
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return "Utente creato con successo";
        }

        public async Task<AuthenticationDto> Login(string username, string password)
        {
            var user = await _db.Users
                .Include(x => x.Role)
                    .ThenInclude(x => x!.RolePermissions)
                    .ThenInclude(x => x.Permission)
                .Include(x => x.Role)
                    .ThenInclude(x => x!.RoleModules)
                    .ThenInclude(x => x.Module)
                .FirstOrDefaultAsync(x => x.username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new Exception("Credenziali non valide");

            var roleName = user.Role?.Name ?? "USER";
            var permissions = user.Role?.RolePermissions
                .Where(x => x.Permission != null)
                .Select(x => x.Permission.Name)
                .ToList() ?? new List<string>();

            var modules = user.Role?.RoleModules
                .Where(x => x.Module != null)
                .Select(x => x.Module.Name)
                .ToList() ?? new List<string>();

            AuthenticationDto response = new AuthenticationDto
            {
                Id = user.Id,
                Role = roleName,
                Permissions = permissions,
                Modules = modules,
                jwt = _jwt.GenerateToken(user.Id, roleName, permissions, modules)
            };

            return response;
        }

        public async Task<List<UserDto>> GetUsers()
        {
            return await _db.Users
                .Include(x => x.Role)
                .Select(x => new UserDto
                {
                    Id = x.Id,
                    Username = x.username,
                    Email = x.Email ?? "",
                    Role = x.Role != null ? x.Role.Name : "",
                    RoleId = x.RoleId
                })
                .ToListAsync();
        }

        public async Task<UserDto> GetUserById(int id)
        {
            var user = await _db.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.username,
                Email = user.Email ?? "",
                Role = user.Role != null ? user.Role.Name : "",
                RoleId = user.RoleId,
            };
        }

        public async Task<string> DeleteUser(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                throw new Exception("Utente non trovato");

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            return "Utente eliminato con successo";
        }

        public async Task<string> update(UserDto body)
        {
            var user = await _db.Users.FindAsync(body.Id);
            if (user == null)
                throw new Exception("Utente non trovato");

            await EnsureRoleExists(body.RoleId);

            user.username = body.Username;
            user.Email = body.Email;
            if (!string.IsNullOrWhiteSpace(body.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(body.Password);
            user.RoleId = body.RoleId;

            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return "Utente aggiornato con successo";
        }

        public async Task<string> UpdateRole(UserRoleRequest body)
        {
            var user = await _db.Users.FindAsync(body.UserId);
            if (user == null)
                throw new Exception("Utente non trovato");

            await EnsureRoleExists(body.RoleId);

            user.RoleId = body.RoleId;
            await _db.SaveChangesAsync();

            return "Ruolo utente aggiornato con successo";
        }

        private async System.Threading.Tasks.Task EnsureRoleExists(int? roleId)
        {
            if (roleId == null)
                return;

            var exists = await _db.Roles.AnyAsync(r => r.Id == roleId);
            if (!exists)
                throw new Exception("Ruolo non trovato");
        }

    }
}

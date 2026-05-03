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

        public async Task<string> Register(string username, string password)
        {
            if (_db.Users.Any(x => x.username == username))
                throw new Exception("utente già esistente");

            var user = new User
            {
                username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return "Utente creato con successo";
        }

        public async Task<AuthenticationDto> Login(string username, string password)
        {
            var user = await _db.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new Exception("Credenziali non valide");

            AuthenticationDto response = new AuthenticationDto
            {
                Id = user.Id,
                Role = user.Role?.Name ?? "USER",
                jwt = _jwt.GenerateToken(user.Id, user.Role?.Name ?? "USER")
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
                    Role = x.Role.Name
                })
                .ToListAsync();
        }
    }
}

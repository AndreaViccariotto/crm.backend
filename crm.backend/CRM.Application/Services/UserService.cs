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
            if (_db.Users.Any(x => x.Id == body.Id))
                throw new Exception("utente già esistente");

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
                Role = user.Role.Name,
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

            user.username = body.Username;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(body.Password);
            user.RoleId = body.RoleId;

            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return "Utente aggiornato con successo";
        }


    }
}

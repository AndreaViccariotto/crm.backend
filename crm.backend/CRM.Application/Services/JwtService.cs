using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace crm.backend.CRM.Application.Services
{
    public class JwtService
    {
        private readonly string _key;
        private readonly string? _issuer;
        private readonly string? _audience;

        public JwtService(IConfiguration config)
        {
            _key = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key non configurata");
            _issuer = config["Jwt:Issuer"];
            _audience = config["Jwt:Audience"];
        }

        public string GenerateToken(int userId, string role, IEnumerable<string>? permissions = null, IEnumerable<string>? modules = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            claims.AddRange((permissions ?? Enumerable.Empty<string>())
                .Where(permission => !string.IsNullOrWhiteSpace(permission))
                .Distinct()
                .Select(permission => new Claim("permission", permission)));

            claims.AddRange((modules ?? Enumerable.Empty<string>())
                .Where(module => !string.IsNullOrWhiteSpace(module))
                .Distinct()
                .Select(module => new Claim("module", module)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

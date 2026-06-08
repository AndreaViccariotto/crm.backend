using System.Data;
using System.Security.Cryptography;
using System.Text;
using crm.backend.CRM.Api.DTO;
using crm.backend.CRM.Domain.Entities;
using crm.backend.CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace crm.backend.CRM.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/setup")]
public class SetupController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public SetupController(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status()
    {
        return Ok(new
        {
            requiresSetup = !await _db.Users.AnyAsync(),
            requiresToken = !string.IsNullOrWhiteSpace(_configuration["Setup:Token"])
        });
    }

    [HttpPost("initialize")]
    public async Task<IActionResult> Initialize([FromBody] InitialSetupRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            return BadRequest(new { message = "Le password non coincidono." });

        var configuredToken = _configuration["Setup:Token"];
        if (!string.IsNullOrWhiteSpace(configuredToken)
            && !TokensMatch(configuredToken, request.InstallationToken))
            return Unauthorized(new { message = "Codice di installazione non valido." });

        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        if (await _db.Users.AnyAsync())
            return Conflict(new { message = "La configurazione iniziale e gia stata completata." });

        var adminRole = await _db.Roles.SingleOrDefaultAsync(role => role.Name == "ADMIN");
        if (adminRole == null)
            return Problem("Ruolo amministratore non disponibile.", statusCode: StatusCodes.Status503ServiceUnavailable);

        var username = request.Username.Trim();
        var email = request.Email.Trim();

        _db.Users.Add(new User
        {
            username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = adminRole.Id
        });

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new { message = "Amministratore creato.", username });
    }

    private static bool TokensMatch(string configuredToken, string? providedToken)
    {
        if (string.IsNullOrWhiteSpace(providedToken))
            return false;

        var configuredHash = SHA256.HashData(Encoding.UTF8.GetBytes(configuredToken));
        var providedHash = SHA256.HashData(Encoding.UTF8.GetBytes(providedToken));
        return CryptographicOperations.FixedTimeEquals(configuredHash, providedHash);
    }
}

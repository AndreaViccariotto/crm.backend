using System.ComponentModel.DataAnnotations;

namespace crm.backend.CRM.Api.DTO;

public class InitialSetupRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(80)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(180)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(12)]
    [MaxLength(200)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? InstallationToken { get; set; }
}

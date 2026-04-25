using System.ComponentModel.DataAnnotations;

namespace LMSAPI1.DTOs;

public class RegisterDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;
}

public class LoginDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class UpdateProfileDTO
{
    [Required]
    public string FullName { get; set; } = string.Empty;
}

public class AuthResponseDTO
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
}

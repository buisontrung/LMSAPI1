using LMSAPI1.DTOs;
using LMSAPI1.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LMSAPI1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;

    public UsersController(UserManager<AppUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userExists = await _userManager.FindByEmailAsync(dto.Email);
        if (userExists != null) return BadRequest("User already exists!");

        var user = new AppUser
        {
            Email = dto.Email,
            UserName = dto.Email,
            FullName = dto.FullName,
            CreatedDate = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, "Student");

        return Ok(new { Status = "Success", Message = "User created successfully!" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user != null && await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var userRole in userRoles)
            {
                // Note: Identity adds role claims dynamically so adding it to the JWT secures it
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                authClaims.Add(new Claim("role", userRole));
            }

            var token = GetToken(authClaims);

            return Ok(new AuthResponseDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Roles = userRoles
            });
        }
        return Unauthorized("Invalid credentials.");
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Purely informational endpoint for JWT logic
        return Ok(new { Status = "Success", Message = "Logged out. Please discard your JWT token locally." });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateProfileDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound("User not found.");

        user.FullName = dto.FullName;
        user.UpdatedDate = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok(new { Status = "Success", Message = "Profile updated successfully!" });
    }

    [HttpPost("{userId}/roles/{roleName}")]
    public async Task<IActionResult> AddUserToRole(Guid userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return NotFound("User not found.");

        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok(new { Status = "Success", Message = $"User added to role {roleName} successfully!" });
    }

    private JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        var key = _configuration["Jwt:Key"] ?? "superSecretKey_PleaseChangeThisInProduction123!";
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "LMS_Issuer",
            audience: _configuration["Jwt:Audience"] ?? "LMS_Audience",
            expires: DateTime.UtcNow.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsersByRole([FromQuery] string? role)
    {
        var users = _userManager.Users.Select(u => new
        {
            u.Id,
            u.FullName,
            u.Email,
            u.StudentCode,
            u.CreatedDate
        }).ToList(); // For small to medium data sets, but ideally we filter in DB.

        if (string.IsNullOrEmpty(role))
        {
            return Ok(users);
        }

        var usersInRole = await _userManager.GetUsersInRoleAsync(role);
        
        var result = usersInRole.Select(u => new
        {
            u.Id,
            u.FullName,
            u.Email,
            u.StudentCode,
            u.CreatedDate
        });

        return Ok(result);
    }
}

using LMSAPI1.DTOs;
using LMSAPI1.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSAPI1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly RoleManager<AppRole> _roleManager;

    public RolesController(RoleManager<AppRole> roleManager)
    {
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        return Ok(roles.Select(r => new RoleResponseDTO
        {
            Id = r.Id,
            Name = r.Name ?? string.Empty,
            CreatedDate = r.CreatedDate,
            UpdatedDate = r.UpdatedDate
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null) return NotFound();

        return Ok(new RoleResponseDTO
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            CreatedDate = role.CreatedDate,
            UpdatedDate = role.UpdatedDate
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var role = new AppRole
        {
            Name = dto.Name,
            CreatedDate = DateTime.UtcNow
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var responseDto = new RoleResponseDTO
        {
            Id = role.Id,
            Name = role.Name,
            CreatedDate = role.CreatedDate
        };

        return CreatedAtAction(nameof(GetById), new { id = role.Id }, responseDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null) return NotFound();

        role.Name = dto.Name;
        role.UpdatedDate = DateTime.UtcNow;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null) return NotFound();

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }
}

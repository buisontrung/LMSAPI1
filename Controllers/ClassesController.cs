using LMSAPI1.Data;
using LMSAPI1.DTOs;
using LMSAPI1.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSAPI1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClassesController : ControllerBase
{
    private readonly LMSDbContext _context;
    public ClassesController(LMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _context.Classes.Include(c => c.TrainingProgram).ToListAsync();
        return Ok(entities.Select(e => new ClassResponseDTO
        {
            Id = e.Id,
            Name = e.Name,
            TrainingProgramId = e.TrainingProgramId,
            TrainingProgramName = e.TrainingProgram?.Name ?? string.Empty,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var e = await _context.Classes.Include(c => c.TrainingProgram).FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(new ClassResponseDTO
        {
            Id = e.Id,
            Name = e.Name,
            TrainingProgramId = e.TrainingProgramId,
            TrainingProgramName = e.TrainingProgram?.Name ?? string.Empty,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateClassDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = new Class
        {
            Name = dto.Name,
            TrainingProgramId = dto.TrainingProgramId,
            CreatedDate = DateTime.UtcNow
        };
        _context.Classes.Add(entity);
        await _context.SaveChangesAsync();
        
        await _context.Entry(entity).Reference(c => c.TrainingProgram).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new ClassResponseDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            TrainingProgramId = entity.TrainingProgramId,
            TrainingProgramName = entity.TrainingProgram?.Name ?? string.Empty,
            CreatedDate = entity.CreatedDate
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateClassDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = await _context.Classes.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Name = dto.Name;
        entity.TrainingProgramId = dto.TrainingProgramId;
        entity.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.Classes.FindAsync(id);
        if (entity == null) return NotFound();
        _context.Classes.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok();
    }
}

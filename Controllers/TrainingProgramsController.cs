using LMSAPI1.Data;
using LMSAPI1.DTOs;
using LMSAPI1.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSAPI1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TrainingProgramsController : ControllerBase
{
    private readonly LMSDbContext _context;
    public TrainingProgramsController(LMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _context.TrainingPrograms.ToListAsync();
        return Ok(entities.Select(e => new TrainingProgramResponseDTO
        {
            Id = e.Id,
            Name = e.Name,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var entity = await _context.TrainingPrograms.FindAsync(id);
        if (entity == null) return NotFound();
        return Ok(new TrainingProgramResponseDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            CreatedDate = entity.CreatedDate,
            UpdatedDate = entity.UpdatedDate
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTrainingProgramDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = new TrainingProgram { Name = dto.Name, CreatedDate = DateTime.UtcNow };
        _context.TrainingPrograms.Add(entity);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new TrainingProgramResponseDTO 
        { 
            Id = entity.Id, 
            Name = entity.Name, 
            CreatedDate = entity.CreatedDate 
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateTrainingProgramDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = await _context.TrainingPrograms.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Name = dto.Name;
        entity.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.TrainingPrograms.FindAsync(id);
        if (entity == null) return NotFound();
        _context.TrainingPrograms.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok();
    }
}

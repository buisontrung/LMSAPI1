using LMSAPI1.Data;
using LMSAPI1.DTOs;
using LMSAPI1.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSAPI1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExamsController : ControllerBase
{
    private readonly LMSDbContext _context;
    public ExamsController(LMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _context.Exams.Include(e => e.Course).ToListAsync();
        return Ok(entities.Select(e => new ExamResponseDTO
        {
            Id = e.Id,
            Title = e.Title,
            Type = e.Type,
            CourseId = e.CourseId,
            CourseName = e.Course?.Title ?? string.Empty,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var e = await _context.Exams.Include(ex => ex.Course).FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(new ExamResponseDTO
        {
            Id = e.Id,
            Title = e.Title,
            Type = e.Type,
            CourseId = e.CourseId,
            CourseName = e.Course?.Title ?? string.Empty,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateExamDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = new Exam
        {
            Title = dto.Title,
            Type = dto.Type,
            CourseId = dto.CourseId,
            CreatedDate = DateTime.UtcNow
        };
        _context.Exams.Add(entity);
        await _context.SaveChangesAsync();
        await _context.Entry(entity).Reference(c => c.Course).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new ExamResponseDTO
        {
            Id = entity.Id,
            Title = entity.Title,
            Type = entity.Type,
            CourseId = entity.CourseId,
            CourseName = entity.Course?.Title ?? string.Empty,
            CreatedDate = entity.CreatedDate
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateExamDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = await _context.Exams.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Title = dto.Title;
        entity.Type = dto.Type;
        entity.CourseId = dto.CourseId;
        entity.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.Exams.FindAsync(id);
        if (entity == null) return NotFound();
        _context.Exams.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok();
    }
}

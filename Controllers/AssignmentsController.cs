using LMSAPI1.Data;
using LMSAPI1.DTOs;
using LMSAPI1.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSAPI1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AssignmentsController : ControllerBase
{
    private readonly LMSDbContext _context;
    public AssignmentsController(LMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _context.Assignments
            .Include(a => a.Course)
            .Include(a => a.Lesson)
            .ToListAsync();
            
        return Ok(entities.Select(e => new AssignmentResponseDTO
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            DueDate = e.DueDate,
            CourseId = e.CourseId,
            CourseName = e.Course?.Title,
            LessonId = e.LessonId,
            LessonName = e.Lesson?.Title,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var e = await _context.Assignments
            .Include(a => a.Course)
            .Include(a => a.Lesson)
            .FirstOrDefaultAsync(x => x.Id == id);
            
        if (e == null) return NotFound();
        return Ok(new AssignmentResponseDTO
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            DueDate = e.DueDate,
            CourseId = e.CourseId,
            CourseName = e.Course?.Title,
            LessonId = e.LessonId,
            LessonName = e.Lesson?.Title,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAssignmentDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = new Assignment
        {
            Title = dto.Title,
            Description = dto.Description,
            DueDate = dto.DueDate,
            CourseId = dto.CourseId,
            LessonId = dto.LessonId,
            CreatedDate = DateTime.UtcNow
        };
        _context.Assignments.Add(entity);
        await _context.SaveChangesAsync();

        if (entity.CourseId.HasValue) await _context.Entry(entity).Reference(c => c.Course).LoadAsync();
        if (entity.LessonId.HasValue) await _context.Entry(entity).Reference(c => c.Lesson).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new AssignmentResponseDTO
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            DueDate = entity.DueDate,
            CourseId = entity.CourseId,
            CourseName = entity.Course?.Title,
            LessonId = entity.LessonId,
            LessonName = entity.Lesson?.Title,
            CreatedDate = entity.CreatedDate
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateAssignmentDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = await _context.Assignments.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.DueDate = dto.DueDate;
        entity.CourseId = dto.CourseId;
        entity.LessonId = dto.LessonId;
        entity.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.Assignments.FindAsync(id);
        if (entity == null) return NotFound();
        _context.Assignments.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok();
    }
}

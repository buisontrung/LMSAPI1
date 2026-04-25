using LMSAPI1.Data;
using LMSAPI1.DTOs;
using LMSAPI1.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSAPI1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EnrollmentsController : ControllerBase
{
    private readonly LMSDbContext _context;
    public EnrollmentsController(LMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _context.Enrollments
            .Include(e => e.Course)
            .Include(e => e.User)
            .Include(e => e.CourseClass).ThenInclude(cc => cc.TrainingProgram)
            .ToListAsync();
            
        return Ok(entities.Select(e => new EnrollmentResponseDTO
        {
            Id = e.Id,
            EnrollmentDate = e.EnrollmentDate,
            UserId = e.UserId,
            UserName = e.User?.FullName ?? string.Empty,
            CourseId = e.CourseId,
            CourseName = e.Course?.Title ?? string.Empty,
            CourseClassId = e.CourseClassId,
            CourseClassName = e.CourseClass?.Name ?? string.Empty,
            TrainingProgramName = e.CourseClass?.TrainingProgram?.Name ?? string.Empty,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var e = await _context.Enrollments
            .Include(en => en.Course)
            .Include(en => en.User)
            .Include(en => en.CourseClass).ThenInclude(cc => cc.TrainingProgram)
            .FirstOrDefaultAsync(x => x.Id == id);
            
        if (e == null) return NotFound();
        return Ok(new EnrollmentResponseDTO
        {
            Id = e.Id,
            EnrollmentDate = e.EnrollmentDate,
            UserId = e.UserId,
            UserName = e.User?.FullName ?? string.Empty,
            CourseId = e.CourseId,
            CourseName = e.Course?.Title ?? string.Empty,
            CourseClassId = e.CourseClassId,
            CourseClassName = e.CourseClass?.Name ?? string.Empty,
            TrainingProgramName = e.CourseClass?.TrainingProgram?.Name ?? string.Empty,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateEnrollmentDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = new Enrollment
        {
            UserId = dto.UserId,
            CourseId = dto.CourseId,
            CourseClassId = dto.CourseClassId,
            EnrollmentDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };
        _context.Enrollments.Add(entity);
        await _context.SaveChangesAsync();

        await _context.Entry(entity).Reference(c => c.User).LoadAsync();
        await _context.Entry(entity).Reference(c => c.Course).LoadAsync();
        await _context.Entry(entity).Reference(c => c.CourseClass).LoadAsync();
        if (entity.CourseClass != null)
            await _context.Entry(entity.CourseClass).Reference(cc => cc.TrainingProgram).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new EnrollmentResponseDTO
        {
            Id = entity.Id,
            EnrollmentDate = entity.EnrollmentDate,
            UserId = entity.UserId,
            UserName = entity.User?.FullName ?? string.Empty,
            CourseId = entity.CourseId,
            CourseName = entity.Course?.Title ?? string.Empty,
            CourseClassId = entity.CourseClassId,
            CourseClassName = entity.CourseClass?.Name ?? string.Empty,
            TrainingProgramName = entity.CourseClass?.TrainingProgram?.Name ?? string.Empty,
            CreatedDate = entity.CreatedDate
        });
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> BulkCreate(BulkCreateEnrollmentDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existingEnrollments = await _context.Enrollments
            .Where(e => e.CourseClassId == dto.CourseClassId && dto.UserIds.Contains(e.UserId))
            .Select(e => e.UserId)
            .ToListAsync();

        var newUserIds = dto.UserIds.Except(existingEnrollments).ToList();

        if (!newUserIds.Any())
            return Ok(new { Message = "Tất cả học viên đã được thêm vào lớp này trước đó." });

        var newEnrollments = newUserIds.Select(userId => new Enrollment
        {
            UserId = userId,
            CourseId = dto.CourseId,
            CourseClassId = dto.CourseClassId,
            EnrollmentDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        }).ToList();

        _context.Enrollments.AddRange(newEnrollments);
        await _context.SaveChangesAsync();

        return Ok(new { Message = $"Đã thêm thành công {newEnrollments.Count} học viên vào lớp." });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateEnrollmentDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = await _context.Enrollments.FindAsync(id);
        if (entity == null) return NotFound();
        entity.UserId = dto.UserId;
        entity.CourseId = dto.CourseId;
        entity.CourseClassId = dto.CourseClassId;
        entity.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.Enrollments.FindAsync(id);
        if (entity == null) return NotFound();
        _context.Enrollments.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok();
    }
}

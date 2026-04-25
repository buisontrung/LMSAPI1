using LMSAPI1.Data;
using LMSAPI1.DTOs;
using LMSAPI1.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSAPI1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SubmissionsController : ControllerBase
{
    private readonly LMSDbContext _context;
    public SubmissionsController(LMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _context.Submissions
            .Include(s => s.Assignment)
            .Include(s => s.User)
            .ToListAsync();
            
        return Ok(entities.Select(e => new SubmissionResponseDTO
        {
            Id = e.Id,
            SubmissionFileUrl = e.SubmissionFileUrl,
            AssignmentId = e.AssignmentId,
            AssignmentTitle = e.Assignment?.Title ?? string.Empty,
            UserId = e.UserId,
            UserName = e.User?.FullName ?? string.Empty,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var e = await _context.Submissions
            .Include(s => s.Assignment)
            .Include(s => s.User)
            .FirstOrDefaultAsync(x => x.Id == id);
            
        if (e == null) return NotFound();
        return Ok(new SubmissionResponseDTO
        {
            Id = e.Id,
            SubmissionFileUrl = e.SubmissionFileUrl,
            AssignmentId = e.AssignmentId,
            AssignmentTitle = e.Assignment?.Title ?? string.Empty,
            UserId = e.UserId,
            UserName = e.User?.FullName ?? string.Empty,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSubmissionDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = new Submission
        {
            SubmissionFileUrl = dto.SubmissionFileUrl,
            AssignmentId = dto.AssignmentId,
            UserId = dto.UserId,
            CreatedDate = DateTime.UtcNow
        };
        _context.Submissions.Add(entity);
        await _context.SaveChangesAsync();

        await _context.Entry(entity).Reference(c => c.Assignment).LoadAsync();
        await _context.Entry(entity).Reference(c => c.User).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new SubmissionResponseDTO
        {
            Id = entity.Id,
            SubmissionFileUrl = entity.SubmissionFileUrl,
            AssignmentId = entity.AssignmentId,
            AssignmentTitle = entity.Assignment?.Title ?? string.Empty,
            UserId = entity.UserId,
            UserName = entity.User?.FullName ?? string.Empty,
            CreatedDate = entity.CreatedDate
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateSubmissionDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = await _context.Submissions.FindAsync(id);
        if (entity == null) return NotFound();
        entity.SubmissionFileUrl = dto.SubmissionFileUrl;
        entity.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.Submissions.FindAsync(id);
        if (entity == null) return NotFound();
        _context.Submissions.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok();
    }
}

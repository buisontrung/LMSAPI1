using LMSAPI1.Data;
using LMSAPI1.DTOs;
using LMSAPI1.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSAPI1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChaptersController : ControllerBase
{
    private readonly LMSDbContext _context;
    public ChaptersController(LMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _context.Chapters.Include(ch => ch.Lessons).ToListAsync();
        return Ok(entities.Select(e => new ChapterResponseDTO
        {
            Id = e.Id,
            Title = e.Title,
            Order = e.Order,
            CourseId = e.CourseId,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate,
            Lessons = e.Lessons.Select(l => new LessonResponseDTO
            {
                Id = l.Id,
                Title = l.Title,
                FileUrl = l.FileUrl,
                Order = l.Order,
                ChapterId = l.ChapterId,
                ChapterTitle = e.Title,
                CreatedDate = l.CreatedDate,
                UpdatedDate = l.UpdatedDate
            }).ToList()
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var e = await _context.Chapters.Include(ch => ch.Lessons).FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(new ChapterResponseDTO
        {
            Id = e.Id,
            Title = e.Title,
            Order = e.Order,
            CourseId = e.CourseId,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate,
            Lessons = e.Lessons.Select(l => new LessonResponseDTO
            {
                Id = l.Id,
                Title = l.Title,
                FileUrl = l.FileUrl,
                Order = l.Order,
                ChapterId = l.ChapterId,
                ChapterTitle = e.Title,
                CreatedDate = l.CreatedDate,
                UpdatedDate = l.UpdatedDate
            }).ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateChapterDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = new Chapter
        {
            Title = dto.Title,
            Order = dto.Order,
            CourseId = dto.CourseId,
            CreatedDate = DateTime.UtcNow
        };
        _context.Chapters.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new ChapterResponseDTO
        {
            Id = entity.Id,
            Title = entity.Title,
            Order = entity.Order,
            CourseId = entity.CourseId,
            CreatedDate = entity.CreatedDate
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateChapterDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = await _context.Chapters.FindAsync(id);
        if (entity == null) return NotFound();
        
        entity.Title = dto.Title;
        entity.Order = dto.Order;
        entity.CourseId = dto.CourseId;
        entity.UpdatedDate = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.Chapters.FindAsync(id);
        if (entity == null) return NotFound();
        _context.Chapters.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok();
    }
}

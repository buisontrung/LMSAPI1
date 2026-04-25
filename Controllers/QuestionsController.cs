using LMSAPI1.Data;
using LMSAPI1.DTOs;
using LMSAPI1.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSAPI1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class QuestionsController : ControllerBase
{
    private readonly LMSDbContext _context;
    public QuestionsController(LMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _context.Questions.Include(q => q.Exam).ToListAsync();
        return Ok(entities.Select(e => new QuestionResponseDTO
        {
            Id = e.Id,
            Content = e.Content,
            ExamId = e.ExamId,
            ExamTitle = e.Exam?.Title ?? string.Empty,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var e = await _context.Questions.Include(q => q.Exam).FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(new QuestionResponseDTO
        {
            Id = e.Id,
            Content = e.Content,
            ExamId = e.ExamId,
            ExamTitle = e.Exam?.Title ?? string.Empty,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateQuestionDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = new Question
        {
            Content = dto.Content,
            ExamId = dto.ExamId,
            CreatedDate = DateTime.UtcNow
        };
        _context.Questions.Add(entity);
        await _context.SaveChangesAsync();
        await _context.Entry(entity).Reference(c => c.Exam).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new QuestionResponseDTO
        {
            Id = entity.Id,
            Content = entity.Content,
            ExamId = entity.ExamId,
            ExamTitle = entity.Exam?.Title ?? string.Empty,
            CreatedDate = entity.CreatedDate
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateQuestionDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = await _context.Questions.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Content = dto.Content;
        entity.ExamId = dto.ExamId;
        entity.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.Questions.FindAsync(id);
        if (entity == null) return NotFound();
        _context.Questions.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok();
    }
}

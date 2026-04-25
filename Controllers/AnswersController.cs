using LMSAPI1.Data;
using LMSAPI1.DTOs;
using LMSAPI1.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSAPI1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AnswersController : ControllerBase
{
    private readonly LMSDbContext _context;
    public AnswersController(LMSDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _context.Answers.ToListAsync();
        return Ok(entities.Select(e => new AnswerResponseDTO
        {
            Id = e.Id,
            Content = e.Content,
            IsCorrect = e.IsCorrect,
            QuestionId = e.QuestionId,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var e = await _context.Answers.FindAsync(id);
        if (e == null) return NotFound();
        return Ok(new AnswerResponseDTO
        {
            Id = e.Id,
            Content = e.Content,
            IsCorrect = e.IsCorrect,
            QuestionId = e.QuestionId,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAnswerDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = new Answer
        {
            Content = dto.Content,
            IsCorrect = dto.IsCorrect,
            QuestionId = dto.QuestionId,
            CreatedDate = DateTime.UtcNow
        };
        _context.Answers.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new AnswerResponseDTO
        {
            Id = entity.Id,
            Content = entity.Content,
            IsCorrect = entity.IsCorrect,
            QuestionId = entity.QuestionId,
            CreatedDate = entity.CreatedDate
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateAnswerDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = await _context.Answers.FindAsync(id);
        if (entity == null) return NotFound();
        entity.Content = dto.Content;
        entity.IsCorrect = dto.IsCorrect;
        entity.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.Answers.FindAsync(id);
        if (entity == null) return NotFound();
        _context.Answers.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok();
    }
}

using LMSAPI1.Data;
using LMSAPI1.DTOs;
using LMSAPI1.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSAPI1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LessonsController : ControllerBase
{
    private readonly LMSDbContext _context;
    private readonly IWebHostEnvironment _env;

    // Các định dạng video được phép upload
    private static readonly string[] AllowedVideoExtensions = [".mp4", ".webm", ".mov", ".avi", ".mkv"];

    public LessonsController(LMSDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // -------------------------------------------------------
    // Helper: lưu file video vào wwwroot/videos/lessons/
    // -------------------------------------------------------
    private async Task<string> SaveVideoAsync(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedVideoExtensions.Contains(ext))
            throw new InvalidOperationException($"Định dạng '{ext}' không được hỗ trợ. Chỉ chấp nhận: {string.Join(", ", AllowedVideoExtensions)}");

        var folderPath = Path.Combine(
            _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
            "videos", "lessons");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var fileName = Guid.NewGuid().ToString() + ext;
        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        return "/videos/lessons/" + fileName;
    }

    // -------------------------------------------------------
    // Helper: xoá file video cũ nếu tồn tại
    // -------------------------------------------------------
    private void DeleteVideoFile(string? fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return;
        var fullPath = Path.Combine(
            _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
            fileUrl.TrimStart('/'));
        if (System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);
    }

    // -------------------------------------------------------
    // GET /api/Lessons
    // -------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _context.Lessons.Include(l => l.Chapter).ToListAsync();
        return Ok(entities.Select(e => new LessonResponseDTO
        {
            Id = e.Id,
            Title = e.Title,
            FileUrl = e.FileUrl,
            Order = e.Order,
            ChapterId = e.ChapterId,
            ChapterTitle = e.Chapter?.Title ?? string.Empty,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        }));
    }

    // -------------------------------------------------------
    // GET /api/Lessons/{id}
    // -------------------------------------------------------
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var e = await _context.Lessons.Include(l => l.Chapter).FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(new LessonResponseDTO
        {
            Id = e.Id,
            Title = e.Title,
            FileUrl = e.FileUrl,
            Order = e.Order,
            ChapterId = e.ChapterId,
            ChapterTitle = e.Chapter?.Title ?? string.Empty,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate
        });
    }

    // -------------------------------------------------------
    // POST /api/Lessons  (multipart/form-data)
    // -------------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateLessonDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (dto.VideoFile == null)
            return BadRequest("VideoFile là bắt buộc khi tạo bài học.");

        string videoUrl;
        try { videoUrl = await SaveVideoAsync(dto.VideoFile); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        var entity = new Lesson
        {
            Title = dto.Title,
            FileUrl = videoUrl,
            Order = dto.Order,
            ChapterId = dto.ChapterId,
            CreatedDate = DateTime.UtcNow
        };

        _context.Lessons.Add(entity);
        await _context.SaveChangesAsync();
        await _context.Entry(entity).Reference(l => l.Chapter).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new LessonResponseDTO
        {
            Id = entity.Id,
            Title = entity.Title,
            FileUrl = entity.FileUrl,
            Order = entity.Order,
            ChapterId = entity.ChapterId,
            ChapterTitle = entity.Chapter?.Title ?? string.Empty,
            CreatedDate = entity.CreatedDate
        });
    }

    // -------------------------------------------------------
    // PUT /api/Lessons/{id}  (multipart/form-data)
    // -------------------------------------------------------
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateLessonDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity = await _context.Lessons.FindAsync(id);
        if (entity == null) return NotFound();

        // Nếu có file video mới → lưu và xoá file cũ
        if (dto.VideoFile != null)
        {
            string newVideoUrl;
            try { newVideoUrl = await SaveVideoAsync(dto.VideoFile); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

            DeleteVideoFile(entity.FileUrl);
            entity.FileUrl = newVideoUrl;
        }

        entity.Title = dto.Title;
        entity.Order = dto.Order;
        entity.ChapterId = dto.ChapterId;
        entity.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Cập nhật bài học thành công.", FileUrl = entity.FileUrl });
    }

    // -------------------------------------------------------
    // DELETE /api/Lessons/{id}
    // -------------------------------------------------------
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.Lessons.FindAsync(id);
        if (entity == null) return NotFound();

        // Xoá file video khỏi disk
        DeleteVideoFile(entity.FileUrl);

        _context.Lessons.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Đã xoá bài học." });
    }
}

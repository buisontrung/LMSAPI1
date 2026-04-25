using LMSAPI1.Data;
using LMSAPI1.DTOs;
using LMSAPI1.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSAPI1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CoursesController : ControllerBase
{
    private readonly LMSDbContext _context;
    private readonly IWebHostEnvironment _env;

    public CoursesController(LMSDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _context.Courses.ToListAsync();

        // Lấy thống kê rating cho tất cả courses trong 1 query
        var ratingStats = await _context.CourseRatings
            .GroupBy(r => r.CourseId)
            .Select(g => new
            {
                CourseId = g.Key,
                Average = g.Average(r => (double)r.Rating),
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.CourseId);

        return Ok(entities.Select(e =>
        {
            ratingStats.TryGetValue(e.Id, out var stat);
            return new CourseResponseDTO
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                ImageUrl = e.ImageUrl,
                CreatedDate = e.CreatedDate,
                UpdatedDate = e.UpdatedDate,
                AverageRating = stat != null ? Math.Round(stat.Average, 1) : 0.0,
                RatingCount = stat?.Count ?? 0
            };
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var e = await _context.Courses
            .Include(c => c.Chapters)
                .ThenInclude(ch => ch.Lessons)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (e == null) return NotFound();

        // Thống kê rating
        var ratings = await _context.CourseRatings
            .Where(r => r.CourseId == id)
            .ToListAsync();
        var avgRating = ratings.Any() ? Math.Round(ratings.Average(r => (double)r.Rating), 1) : 0.0;

        return Ok(new CourseResponseDTO
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            ImageUrl = e.ImageUrl,
            CreatedDate = e.CreatedDate,
            UpdatedDate = e.UpdatedDate,
            AverageRating = avgRating,
            RatingCount = ratings.Count,
            Chapters = e.Chapters.OrderBy(ch => ch.Order).Select(ch => new ChapterResponseDTO
            {
                Id = ch.Id,
                Title = ch.Title,
                Order = ch.Order,
                CourseId = ch.CourseId,
                CreatedDate = ch.CreatedDate,
                UpdatedDate = ch.UpdatedDate,
                Lessons = ch.Lessons.OrderBy(l => l.Order).Select(l => new LessonResponseDTO
                {
                    Id = l.Id,
                    Title = l.Title,
                    FileUrl = l.FileUrl,
                    Order = l.Order,
                    ChapterId = l.ChapterId,
                    ChapterTitle = ch.Title,
                    CreatedDate = l.CreatedDate,
                    UpdatedDate = l.UpdatedDate
                }).ToList()
            }).ToList()
        });
    }

    /// <summary>
    /// Lấy danh sách tất cả đánh giá của một khoá học.
    /// GET /api/Courses/{id}/ratings
    /// </summary>
    [HttpGet("{id}/ratings")]
    public async Task<IActionResult> GetRatings(Guid id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null) return NotFound("Không tìm thấy khoá học.");

        var ratings = await _context.CourseRatings
            .Where(r => r.CourseId == id)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();

        var avg = ratings.Any() ? Math.Round(ratings.Average(r => (double)r.Rating), 1) : 0.0;

        return Ok(new
        {
            CourseId = id,
            CourseName = course.Title,
            AverageRating = avg,
            RatingCount = ratings.Count,
            Ratings = ratings.Select(r => new RatingResponseDTO
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User?.FullName ?? string.Empty,
                CourseId = r.CourseId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedDate = r.CreatedDate,
                UpdatedDate = r.UpdatedDate
            })
        });
    }

    /// <summary>
    /// Xóa đánh giá theo id (dành cho Admin).
    /// DELETE /api/Courses/{id}/ratings/{ratingId}
    /// </summary>
    [HttpDelete("{id}/ratings/{ratingId}")]
    public async Task<IActionResult> DeleteRating(Guid id, Guid ratingId)
    {
        var rating = await _context.CourseRatings
            .FirstOrDefaultAsync(r => r.Id == ratingId && r.CourseId == id);

        if (rating == null) return NotFound("Đánh giá không tồn tại.");

        _context.CourseRatings.Remove(rating);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Đã xóa đánh giá." });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateCourseDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        string? imageUrl = null;
        if (dto.ImageFile != null)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
            var folderPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "images", "courses");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.ImageFile.CopyToAsync(stream);
            }
            imageUrl = "/images/courses/" + fileName;
        }

        var entity = new Course
        {
            Title = dto.Title,
            Description = dto.Description,
            ImageUrl = imageUrl,
            CreatedDate = DateTime.UtcNow
        };
        _context.Courses.Add(entity);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, new CourseResponseDTO
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            ImageUrl = entity.ImageUrl,
            CreatedDate = entity.CreatedDate
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateCourseDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = await _context.Courses.FindAsync(id);
        if (entity == null) return NotFound();

        if (dto.ImageFile != null)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
            var folderPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "images", "courses");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.ImageFile.CopyToAsync(stream);
            }
            
            // Delete old file if exists
            if (!string.IsNullOrEmpty(entity.ImageUrl))
            {
                var oldFilePath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), entity.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);
            }

            entity.ImageUrl = "/images/courses/" + fileName;
        }
        
        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.UpdatedDate = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.Courses.FindAsync(id);
        if (entity == null) return NotFound();
        
        // Delete image file if exists
        if (!string.IsNullOrEmpty(entity.ImageUrl))
        {
            var oldFilePath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), entity.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);
        }

        _context.Courses.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok();
    }
}



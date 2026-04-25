using LMSAPI1.Data;
using LMSAPI1.DTOs;
using LMSAPI1.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMSAPI1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CourseClassesController : ControllerBase
{
    private readonly LMSDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public CourseClassesController(LMSDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private static CourseClassResponseDTO MapToDTO(CourseClass e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        CourseId = e.CourseId,
        CourseName = e.Course?.Title ?? string.Empty,
        TrainingProgramId = e.TrainingProgramId,
        TrainingProgramName = e.TrainingProgram?.Name ?? string.Empty,
        TeacherId = e.TeacherId,
        TeacherName = e.Teacher?.FullName ?? string.Empty,
        TeacherEmail = e.Teacher?.Email ?? string.Empty,
        CreatedDate = e.CreatedDate,
        UpdatedDate = e.UpdatedDate
    };

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entities = await _context.CourseClasses
            .Include(cc => cc.Course)
            .Include(cc => cc.TrainingProgram)
            .Include(cc => cc.Teacher)
            .ToListAsync();

        return Ok(entities.Select(MapToDTO));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var e = await _context.CourseClasses
            .Include(cc => cc.Course)
            .Include(cc => cc.TrainingProgram)
            .Include(cc => cc.Teacher)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (e == null) return NotFound();
        return Ok(MapToDTO(e));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCourseClassDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Validate teacher if provided
        if (dto.TeacherId.HasValue)
        {
            var teacher = await _userManager.FindByIdAsync(dto.TeacherId.Value.ToString());
            if (teacher == null) return BadRequest("Teacher not found.");
            var isTeacher = await _userManager.IsInRoleAsync(teacher, "Teacher");
            if (!isTeacher) return BadRequest("The specified user does not have the role of Teacher.");
        }

        var entity = new CourseClass
        {
            Name = dto.Name,
            CourseId = dto.CourseId,
            TrainingProgramId = dto.TrainingProgramId,
            TeacherId = dto.TeacherId,
            CreatedDate = DateTime.UtcNow
        };
        _context.CourseClasses.Add(entity);
        await _context.SaveChangesAsync();

        await _context.Entry(entity).Reference(c => c.Course).LoadAsync();
        await _context.Entry(entity).Reference(c => c.TrainingProgram).LoadAsync();
        if (entity.TeacherId.HasValue)
            await _context.Entry(entity).Reference(c => c.Teacher).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, MapToDTO(entity));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateCourseClassDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var entity = await _context.CourseClasses.FindAsync(id);
        if (entity == null) return NotFound();

        // Validate teacher if provided
        if (dto.TeacherId.HasValue)
        {
            var teacher = await _userManager.FindByIdAsync(dto.TeacherId.Value.ToString());
            if (teacher == null) return BadRequest("Teacher not found.");
            var isTeacher = await _userManager.IsInRoleAsync(teacher, "Teacher");
            if (!isTeacher) return BadRequest("The specified user does not have the role of Teacher.");
        }

        entity.Name = dto.Name;
        entity.CourseId = dto.CourseId;
        entity.TrainingProgramId = dto.TrainingProgramId;
        entity.TeacherId = dto.TeacherId;
        entity.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok();
    }

    /// <summary>
    /// Gán giảng viên (role=Teacher) cho một lớp học phần. 
    /// PUT /api/CourseClasses/{id}/assign-teacher
    /// </summary>
    [HttpPut("{id}/assign-teacher")]
    public async Task<IActionResult> AssignTeacher(Guid id, AssignTeacherDTO dto)
    {
        var entity = await _context.CourseClasses.Include(cc => cc.Teacher).FirstOrDefaultAsync(cc => cc.Id == id);
        if (entity == null) return NotFound("Không tìm thấy lớp học phần.");

        var teacher = await _userManager.FindByIdAsync(dto.TeacherId.ToString());
        if (teacher == null) return BadRequest("Không tìm thấy người dùng.");

        var isTeacher = await _userManager.IsInRoleAsync(teacher, "Teacher");
        if (!isTeacher) return BadRequest("Người dùng này không có vai trò Giảng viên.");

        entity.TeacherId = dto.TeacherId;
        entity.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Gán giảng viên thành công.",
            TeacherId = teacher.Id,
            TeacherName = teacher.FullName,
            TeacherEmail = teacher.Email
        });
    }

    /// <summary>
    /// Xóa giảng viên khỏi lớp học phần.
    /// DELETE /api/CourseClasses/{id}/assign-teacher
    /// </summary>
    [HttpDelete("{id}/assign-teacher")]
    public async Task<IActionResult> RemoveTeacher(Guid id)
    {
        var entity = await _context.CourseClasses.FindAsync(id);
        if (entity == null) return NotFound("Không tìm thấy lớp học phần.");

        entity.TeacherId = null;
        entity.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Đã xóa giảng viên khỏi lớp học phần." });
    }

    /// <summary>
    /// Lấy danh sách tất cả Giảng viên (role=Teacher) để chọn khi gán.
    /// GET /api/CourseClasses/teachers
    /// </summary>
    [HttpGet("teachers")]
    public async Task<IActionResult> GetAllTeachers()
    {
        var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
        return Ok(teachers.Select(t => new
        {
            Id = t.Id,
            FullName = t.FullName,
            Email = t.Email,
            UserName = t.UserName
        }));
    }

    /// <summary>
    /// Lấy danh sách các lớp học phần được phân công cho một giảng viên.
    /// GET /api/CourseClasses/teacher/{teacherId}
    /// </summary>
    [HttpGet("teacher/{teacherId}")]
    public async Task<IActionResult> GetClassesForTeacher(Guid teacherId)
    {
        var entities = await _context.CourseClasses
            .Include(cc => cc.Course)
            .Include(cc => cc.TrainingProgram)
            .Include(cc => cc.Teacher)
            .Where(cc => cc.TeacherId == teacherId)
            .ToListAsync();

        return Ok(entities.Select(MapToDTO));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _context.CourseClasses.FindAsync(id);
        if (entity == null) return NotFound();
        _context.CourseClasses.Remove(entity);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("{id}/users/{userId}/enroll")]
    public async Task<IActionResult> EnrollStudent(Guid id, Guid userId)
    {
        var courseClass = await _context.CourseClasses.FindAsync(id);
        if (courseClass == null) return NotFound("CourseClass not found.");

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound("User not found.");

        var exists = await _context.Enrollments.AnyAsync(e => e.CourseClassId == id && e.UserId == userId);
        if (exists) return BadRequest("User is already enrolled in this CourseClass.");

        var enrollment = new Enrollment
        {
            UserId = userId,
            CourseId = courseClass.CourseId,
            CourseClassId = id,
            EnrollmentDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        return Ok(new { Status = "Success", Message = "User enrolled successfully." });
    }
}

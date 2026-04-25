using System.ComponentModel.DataAnnotations;

namespace LMSAPI1.Entities;

/// <summary>
/// Theo dõi trạng thái học của từng người dùng cho từng bài học.
/// </summary>
public class UserLesson
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// Thời điểm người dùng hoàn thành bài học (null nếu chưa hoàn thành).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMSAPI1.Entities;

/// <summary>
/// Đánh giá sao của người dùng cho một khoá học (1-5 sao).
/// Mỗi người dùng chỉ đánh giá một khoá học một lần.
/// </summary>
public class CourseRating : BaseEntity
{
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }
}

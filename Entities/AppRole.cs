using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LMSAPI1.Entities;

public class AppRole : IdentityRole<Guid>
{
    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedDate { get; set; }
}

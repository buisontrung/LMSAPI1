using System.ComponentModel.DataAnnotations;

namespace LMSAPI1.DTOs;

public class CreateRoleDTO
{
    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;
}

public class UpdateRoleDTO
{
    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;
}

public class RoleResponseDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

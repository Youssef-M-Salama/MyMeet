using System.ComponentModel.DataAnnotations;

namespace MyMeet.Api.Entities;

public class User
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Provider { get; set; } = "Google";

    [Required]
    [MaxLength(255)]
    public string ProviderUserId { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [MaxLength(500)]
    public string? ProfilePicture { get; set; }

    [Required]
    [MaxLength(30)]
    public string Role { get; set; } = "User";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
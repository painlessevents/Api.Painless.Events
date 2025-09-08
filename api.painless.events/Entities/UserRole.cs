using System.ComponentModel.DataAnnotations;

namespace api.painless.events.Entities
{
    public class UserRole
    {
        [Key] public int Id { get; set; }
        [Required] public int UserId { get; set; } = 0;
        [Required][MaxLength(45)] public string Role { get; set; } = string.Empty;
        [Required] public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}

using System.ComponentModel.DataAnnotations;

namespace api.painless.events.Entities
{
    public class User
    {
        [Key] public int Id { get; set; }
        [Required] public int AccountId { get; set; } = 0;
        [Required] public int EventId { get; set; } = 0;
        [Required][MaxLength(100)] public string Username { get; set; } = string.Empty;
        [Required] public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        [Required] public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        [Required][MaxLength(320)] public string Email { get; set; } = string.Empty;
        [Required] public int EmailVerified { get; set; } = 0;
        [Required][MaxLength(100)] public string Firstname { get; set; } = string.Empty;
        [Required][MaxLength(100)] public string Lastname { get; set; } = string.Empty;
        [Required][MaxLength(300)] public string AvatarUrl { get; set; } = string.Empty;
        [Required] public int Enabled { get; set; } = 1;
        [Required] public int Deleted { get; set; } = 0;
        [Required] public DateTime Created { get; set; } = DateTime.UtcNow;



    }
}

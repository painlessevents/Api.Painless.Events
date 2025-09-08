using System.ComponentModel.DataAnnotations;

namespace api.painless.events.Entities
{
    public class Device
    {

        [Key] public int Id { get; set; }
        [Required][MaxLength(45)] public string Guid { get; set; } = System.Guid.NewGuid().ToString();
        [Required] public int AccountId { get; set; } = 0;
        [Required] public int EventId { get; set; } = 0;
        [Required] public int Pin { get; set; } = 0;
        [Required] [MaxLength(50)] public string Name { get; set; } = string.Empty;
        [Required][MaxLength(100)] public string Location { get; set; } = string.Empty;
        [Required] public int Version { get; set; } = 0;
        [Required] public DateTime LastLogon { get; set; } = DateTime.UtcNow;
        [Required] public int Enabled { get; set; } = 1;
        [Required] public int Deleted { get; set; } = 0;
        [Required] public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}

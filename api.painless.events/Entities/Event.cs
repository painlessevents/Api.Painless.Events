using System.ComponentModel.DataAnnotations;

namespace api.painless.events.Entities
{
    public class Event
    {

        [Key] public int Id { get; set; }
        [Required][MaxLength(45)] public string Guid { get; set; } = string.Empty;
        [Required] public int AccountId { get; set; } = 0;
        [Required][MaxLength(100)] public string Url { get; set; } = string.Empty;
        [Required][MaxLength(300)] public string Title { get; set; } = string.Empty;
        [Required][MaxLength(300)] public string Subtitle { get; set; } = string.Empty;
        [Required][MaxLength(100)] public string TimeZone { get; set; } = string.Empty;
        [Required] public int Enabled { get; set; } = 1;
        [Required] public int Deleted { get; set; } = 0;
        [Required] public DateTime Created { get; set; } = DateTime.UtcNow;

    }
}

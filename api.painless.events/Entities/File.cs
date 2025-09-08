using System.ComponentModel.DataAnnotations;

namespace api.painless.events.Entities
{
    public class File
    {

        [Key] public int Id { get; set; }
        [Required][MaxLength(45)] public string Guid { get; set; } = string.Empty;
        [Required] public int AccountId { get; set; } = 0;
        [Required] public int EventId { get; set; } = 0;
        [Required] [MaxLength(200)] public string Name { get; set; } = string.Empty;
        [Required][MaxLength(20)] public string Extension { get; set; } = string.Empty;
        [Required] public int Size { get; set; } = 0;
        [Required][MaxLength(1000)] public string Description { get; set; } = string.Empty;
        [Required] public int UploadedBy { get; set; } = 0;
        [Required] public int AdminAccess { get; set; } = 1;
        [Required] public int UserAccess { get; set; } = 0;
        [Required] public int VisitorAccess { get; set; } = 0;
        [Required] public int Enabled { get; set; } = 1;
        [Required] public int Deleted { get; set; } = 0;
        [Required] public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}

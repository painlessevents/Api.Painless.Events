using System.ComponentModel.DataAnnotations;

namespace api.painless.events.Entities
{
    public class FileDownload
    {

        [Key] public int Id { get; set; }
        [Required] public int UserId { get; set; } = 0;
        [Required] public int FileId { get; set; } = 0;
        [Required] public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}

using System.ComponentModel.DataAnnotations;

namespace api.painless.events.Entities
{
    public class Node
    {
        [Key] public int Id { get; set; }
        [Required] public string Domain { get; set; } = string.Empty;
        [Required] public int IsActive { get; set; } = 0;
        [Required] public double Latitude { get; set; } = 0;
        [Required] public double Longitude { get; set; } = 0;
        [Required] public DateTime Created { get; set; } = DateTime.UtcNow;

    }
}

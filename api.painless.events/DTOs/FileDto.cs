namespace api.painless.events.DTOs
{
    public class FileDto
    {
        public int Id { get; set; } = 0;
        public string Guid { get; set; } = "";
        public int AccountId { get; set; } = 0;
        public int EventId { get; set; } = 0;
        public string Name { get; set; } = "";
        public string Extension { get; set; } = "";
        public int Size { get; set; } = 0;
        public int Downloads { get; set; } = 0;
        public string Checksum { get; set; } = "";
        public string Description { get; set; } = "";
        public string Creator { get; set; } = "";
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}

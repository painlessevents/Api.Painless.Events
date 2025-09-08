namespace api.painless.events.DTOs
{
    public class LoginAccountDto
    {
        public int AccountId { get; set; } = 0;
        public int EventId { get; set; } = 0;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Persistent { get; set; } = 0;
    }
}

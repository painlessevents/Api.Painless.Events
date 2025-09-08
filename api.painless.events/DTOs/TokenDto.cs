namespace api.painless.events.DTOs
{
    public class TokenDto
    {
        public int Id { get; set; } = 0;
        public int AccountId { get; set; } = 0;
        public int EventId { get; set; } = 0;
        public string RefreshToken { get; set; } = "";
        public string AccessToken { get; set; } = "";
        public string Domain { get; set; } = "";
        public string Url { get; set; } = "";
        public string Username { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string IpTimeZone { get; set; } = "";
        public string EventTimeZone { get; set; } = "";
        public string[] Roles { get; set; } = Array.Empty<string>();
        public string[] UnsortedNodes { get; set; } = Array.Empty<string>();
        public string[] SortedNodes { get; set; } = Array.Empty<string>();
    }
}

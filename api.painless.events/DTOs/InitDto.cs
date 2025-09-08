namespace api.painless.events.DTOs
{
    public class InitDto
    {
        public int AccountId { get; set; } = 0;
        public int EventId { get; set; } = 0;
        public double Latitude { get; set; } = 0;
        public double Longitude { get; set; } = 0;
        public string IpTimeZone { get; set; } = "";
        public string EventTimeZone { get; set; } = "";
        public string[] SortedNodes { get; set; } = Array.Empty<string>();
        public string[] UnsortedNodes { get; set; } = Array.Empty<string>();
    }
}

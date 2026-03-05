namespace EBayAPI.Models
{
    public class SystemLog
    {
        public int Id { get; set; }

        public string Module { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

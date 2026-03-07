using System.ComponentModel.DataAnnotations;

namespace EBayAPI.Configurations
{
    public class OrderCleanupSettings
    {
        [Range(1, int.MaxValue, ErrorMessage = "PaymentTimeoutMinutes must be >= 1")]
        public int PaymentTimeoutMinutes { get; set; } = 30;

        [Range(1, int.MaxValue, ErrorMessage = "CleanupIntervalSeconds must be >= 1")]
        public int CleanupIntervalSeconds { get; set; } = 60;
    }
}

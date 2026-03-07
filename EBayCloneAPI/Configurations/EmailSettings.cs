using System.ComponentModel.DataAnnotations;

namespace EBayAPI.Configurations;

/// <summary>Bound from appsettings "EmailSettings" section.</summary>
public sealed class EmailSettings
{
    [Required] public string SmtpServer  { get; set; } = string.Empty;
    [Range(1, 65535)] public int SmtpPort { get; set; } = 587;
    [Required] public string SenderName  { get; set; } = string.Empty;
    [Required] public string SenderEmail { get; set; } = string.Empty;
    [Required] public string Password    { get; set; } = string.Empty;
}

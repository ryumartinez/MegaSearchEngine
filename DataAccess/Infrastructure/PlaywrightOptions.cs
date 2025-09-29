using System.ComponentModel.DataAnnotations;

namespace DataAccess.Infrastructure;

public class PlaywrightOptions
{
    /// <summary>
    /// The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Playwright";

    /// <summary>
    /// The Chrome DevTools Protocol (CDP) endpoint for Playwright to connect to.
    /// Example: "ws://127.0.0.1:9222"
    /// </summary>
    [Required]
    [Url] // Ensures it's a well-formed URI
    public string CdpEndpoint { get; set; } = string.Empty;
}
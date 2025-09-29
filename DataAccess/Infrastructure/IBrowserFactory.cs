using Microsoft.Playwright;

namespace DataAccess.Infrastructure;

public interface IBrowserFactory
{
    /// <summary>
    /// Creates and returns a new browser page.
    /// The caller is responsible for disposing of the page.
    /// </summary>
    /// <returns>A new IPage instance.</returns>
    Task<IPage> CreatePageAsync();
}
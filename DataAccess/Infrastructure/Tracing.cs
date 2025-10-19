using System.Diagnostics;

namespace DataAccess.Infrastructure;

/// <summary>
/// Contains the ActivitySource for tracing DataAccess operations.
/// </summary>
internal static class Tracing
{
    /// <summary>
    /// The single ActivitySource for all Playwright-related operations.
    /// The name "Azeta.DataAccess.Playwright" will show up in your traces.
    /// </summary>
    internal static readonly ActivitySource Source = new("DataAccess.Playwright");
}
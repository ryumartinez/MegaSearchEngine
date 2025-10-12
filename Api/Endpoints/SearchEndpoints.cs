using Manager.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

internal static class SearchEndpoints
{
    public static void MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/search");

        // Get aggregated results (existing)
        group.MapGet("/", Search)
             .WithSummary("Get aggregated search results from stored data")
             .Produces(StatusCodes.Status200OK);

        // Run a search for a single term, parse and save results
        group.MapPost("/run", RunSingle)
             .WithSummary("Fetch, parse, and save results for a single search term")
             .Accepts<string>("application/json")
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status400BadRequest);

        // Run searches for multiple terms, parse and save results
        group.MapPost("/run/batch", RunBatch)
             .WithSummary("Fetch, parse, and save results for multiple search terms")
             .Accepts<IEnumerable<string>>("application/json")
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Search(
        ISearchResultItemManager searchResultItemManager,
        [FromQuery] string? searchText) // kept for compatibility, even if manager ignores it
    {
        // If your GetAsync ever uses the request.SearchText, pass it through here.
        var result = await searchResultItemManager
            .GetAsync(new GetSearchResultItemRequest(searchText ?? string.Empty))
            .ConfigureAwait(false);

        return Results.Ok(result);
    }

    private static async Task<IResult> RunSingle(
        ISearchResultItemManager searchResultItemManager,
        [FromBody] string? searchText,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return Results.BadRequest(new { error = "searchText is required." });

        // If you want this to be cancellable by client disconnect:
        ct.ThrowIfCancellationRequested();

        await searchResultItemManager.SearchAndSaveAsync(searchText.Trim()).ConfigureAwait(false);

        // Nothing to return, we just performed the action.
        return Results.NoContent();
    }

    private static async Task<IResult> RunBatch(
        ISearchResultItemManager searchResultItemManager,
        [FromBody] IEnumerable<string>? searchTexts,
        CancellationToken ct)
    {
        if (searchTexts is null)
            return Results.BadRequest(new { error = "searchTexts is required." });

        ct.ThrowIfCancellationRequested();

        await searchResultItemManager.SearchAndSaveManyAsync(searchTexts, ct).ConfigureAwait(false);

        return Results.NoContent();
    }
}

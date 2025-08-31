using Manager.Contract;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class SearchEndpoints
{
    public static void MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/search");
        group.MapGet("/", Search);
    }
    
    private static async Task<IResult> Search(ISearchResultItemManager searchResultItemManager, [FromQuery] string searchText)
    {
        var result = await searchResultItemManager.GetAsync(searchText);
        return Results.Ok(result);
    }
}
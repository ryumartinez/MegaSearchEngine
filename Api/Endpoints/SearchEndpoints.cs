using Manager.Contract;

namespace Api.Endpoints;

public static class SearchEndpoints
{
    public static void MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/search");
        group.MapGet("/", Search);
    }
    
    private static async Task<IResult> Search(ISearchResultItemManager searchResultItemManager)
    {
        var request = new GetSearchResultItemRequest();
        var result = await searchResultItemManager.GetAsync(request);
        return Results.Ok(result);
    }
}
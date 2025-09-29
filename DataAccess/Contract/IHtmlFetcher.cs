namespace DataAccess.Contract;

public interface IHtmlFetcher
{
    Task<string> GetContentAsync(Uri url, CancellationToken ct = default);
    Task<(string html, Uri? nextPage)> GetPagedContentAsync(Uri url, CancellationToken ct = default);
}
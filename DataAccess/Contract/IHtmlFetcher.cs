namespace DataAccess.Contract;

public interface IHtmlFetcher
{
    Task<string> GetContentAsync(Uri url, CancellationToken ct = default);
    Task<string> GetContentAsync(Uri url, string readySelector, int readyTimeoutMs = 15000, CancellationToken ct = default);
}
using Engine.Models;

namespace Engine.Contract;

public interface IEcommerceParserEngine
{
    IEnumerable<EcommerceProductEngineModel> ParseSearchHtml(string html, Uri pageUrl);
}
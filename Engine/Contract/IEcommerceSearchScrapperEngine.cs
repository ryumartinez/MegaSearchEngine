using Engine.Models;

namespace Engine.Contract;

public interface IEcommerceSearchScrapperEngine
{ 
    Task<IEnumerable<EcommerceProductEngineModel>> ScrappeSearchAsync(string searchTerm);
}
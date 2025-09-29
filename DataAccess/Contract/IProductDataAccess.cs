using DataAccess.Domain;

namespace DataAccess.Contract;

public interface IProductDataAccess
{
    Task<Product> AddAsync(CreateProductAccessRequest request);
    Task<IEnumerable<Product>> AddRangeAsync(IEnumerable<CreateProductAccessRequest> requests);
    Task<IEnumerable<Product>> GetAsync(GetProductAccessRequest request);
}
using DataAccess.Contract;
using DataAccess.Domain;

namespace DataAccess;

public class ProductDataAccess : IProductDataAccess
{
    public Task<Product> AddAsync(CreateProductAccessRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Product>> AddRangeAsync(IEnumerable<CreateProductAccessRequest> requests)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Product>> GetAsync(GetProductAccessRequest request)
    {
        throw new NotImplementedException();
    }
}
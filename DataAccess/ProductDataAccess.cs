using DataAccess.Contract;
using DataAccess.Domain;
using DataAccess.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class ProductDataAccess(AppDbContext dbContext) : IProductDataAccess
{
    public async Task<Product> AddAsync(CreateProductAccessRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var product = new Product()
        {
            Description = request.Description,
            Link = request.Link,
            Title = request.Name
        };
        var result = await dbContext.Products.AddAsync(product).ConfigureAwait(false);
        return result.Entity;
    }

    public async Task<IEnumerable<Product>> AddRangeAsync(IEnumerable<CreateProductAccessRequest> requests)
    {
        ArgumentNullException.ThrowIfNull(requests);

        var products = requests.Select(x => new Product
        {
            Description = x.Description,
            Link        = x.Link,
            Title       = x.Name
        }).ToList();

        await dbContext.Products.AddRangeAsync(products).ConfigureAwait(false);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        return products;
    }

    public async Task<IEnumerable<Product>> GetAsync(GetProductAccessRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // If your DateTimes come in as Unspecified, pick a convention (e.g., UTC).
        var from = request.From.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(request.From, DateTimeKind.Utc)
            : request.From;

        var to = request.To.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(request.To, DateTimeKind.Utc)
            : request.To;

        // Treat 'To' as inclusive; use <=. If you prefer half-open ranges, switch to '< to'.
        var query = dbContext.Products
            .AsNoTracking()
            .Where(p => p.DateCreated >= from && p.DateCreated <= to)   // <-- use your actual date property
            .OrderByDescending(p => p.DateCreated)                    // stable, index-friendly sort
            .Skip(request.PageIndex * request.PageSize)             // PageIndex is 0-based here
            .Take(request.PageSize);

        return await query.ToListAsync().ConfigureAwait(false);
    }
}
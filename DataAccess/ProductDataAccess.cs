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
            Title = request.Name,
            SiteName = request.SiteName,
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
            Title       = x.Name,
            SiteName = x.SiteName
        }).ToList();

        await dbContext.Products.AddRangeAsync(products).ConfigureAwait(false);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        return products;
    }

    public async Task<IEnumerable<Product>> GetAsync(GetProductAccessRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var nowUtc = DateTime.UtcNow;

        // Defaults: last 30 days ending now (UTC)
        var toRaw   = request.To   ?? nowUtc;
        var fromRaw = request.From ?? toRaw.AddDays(-30);

        // Normalize everything to UTC; treat Unspecified as UTC by convention
        static DateTime NormalizeUtc(DateTime dt) => dt.Kind switch
        {
            DateTimeKind.Utc   => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _                  => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
        };

        var fromUtc = NormalizeUtc(fromRaw);
        var toUtc   = NormalizeUtc(toRaw);

        // Guard: if reversed, swap (or throw if you prefer)
        if (toUtc < fromUtc)
            (fromUtc, toUtc) = (toUtc, fromUtc);

        // Use half-open interval [from, to)
        var toExclusive = toUtc;

        // Pagination: 1-based PageIndex
        var pageIndex = Math.Max(1, request.PageIndex);
        var pageSize  = Math.Clamp(request.PageSize, 1, 1000);

        var query = dbContext.Products
            .AsNoTracking()
            .Where(p => p.DateCreated >= fromUtc && p.DateCreated < toExclusive)
            .OrderByDescending(p => p.DateCreated)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize);

        return await query.ToListAsync().ConfigureAwait(false);
    }
}
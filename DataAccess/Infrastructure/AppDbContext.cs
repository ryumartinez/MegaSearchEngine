using System.Security.Claims;
using DataAccess.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Infrastructure;

public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    IHttpContextAccessor http
) : IdentityDbContext<User, IdentityRole<int>, int>(options)
{
    public DbSet<Product> Products { get; set; }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private string GetCurrentUserName()
    {
        var user = http.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true) return "system";
        // you can store username or user id; choose one and be consistent
        var name = user.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(name)) return name;

        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrWhiteSpace(id) ? id : "system";
    }

    private void UpdateAuditFields()
    {
        var now = DateTime.UtcNow;
        var actor = GetCurrentUserName();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                // Set create fields on insert
                entry.Entity.DateCreated = now;
                entry.Entity.CreatedBy = actor;

                // Ensure these aren’t accidentally overwritten on updates later
                entry.Property(e => e.DateCreated).IsModified = false;
                entry.Property(e => e.CreatedBy).IsModified = false;
            }

            if (entry.State != EntityState.Added && entry.State != EntityState.Modified) continue;
            {
                entry.Entity.DateUpdated = now;
                entry.Entity.UpdatedBy = actor;

                // Make sure the creation fields stay immutable on updates
                if (entry.State != EntityState.Modified) continue;
                entry.Property(e => e.DateCreated).IsModified = false;
                entry.Property(e => e.CreatedBy).IsModified = false;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        base.OnModelCreating(builder);
        var entityTypes = builder.Model.GetEntityTypes().Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType));
        // Apply “required”/length constraints at DB level for all entities deriving from BaseEntity
        foreach (var entityType in entityTypes)
        {
            var entityTypeBuilder = builder.Entity(entityType.ClrType);

            entityTypeBuilder.Property(nameof(BaseEntity.DateCreated)).IsRequired();
            entityTypeBuilder.Property(nameof(BaseEntity.DateUpdated)).IsRequired();
            entityTypeBuilder.Property(nameof(BaseEntity.CreatedBy)).IsRequired().HasMaxLength(256);
            entityTypeBuilder.Property(nameof(BaseEntity.UpdatedBy)).IsRequired().HasMaxLength(256);

            // Optional: add an index on DateCreated if you often query by recency
            // builder.HasIndex(nameof(BaseEntity.DateCreated));
        }
    }
}
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.ProductSnapshot.Data.Models;

namespace VirtoCommerce.ProductSnapshot.Data.Repositories;

public class ProductSnapshotDbContext : DbContextBase
{
    public ProductSnapshotDbContext(DbContextOptions<ProductSnapshotDbContext> options)
        : base(options)
    {
    }

    protected ProductSnapshotDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderProductSnapshotEntity>().ToTable("OrderProductSnapshot").HasKey(x => x.Id);
        modelBuilder.Entity<OrderProductSnapshotEntity>().Property(x => x.Id).HasMaxLength(IdLength).ValueGeneratedOnAdd();

        switch (Database.ProviderName)
        {
            case "Pomelo.EntityFrameworkCore.MySql":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.ProductSnapshot.Data.MySql"));
                break;
            case "Npgsql.EntityFrameworkCore.PostgreSQL":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.ProductSnapshot.Data.PostgreSql"));
                break;
            case "Microsoft.EntityFrameworkCore.SqlServer":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.ProductSnapshot.Data.SqlServer"));
                break;
        }
    }
}

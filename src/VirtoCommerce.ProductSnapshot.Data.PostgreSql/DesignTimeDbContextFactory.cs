using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.ProductSnapshot.Data.Repositories;

namespace VirtoCommerce.ProductSnapshot.Data.PostgreSql;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ProductSnapshotDbContext>
{
    public ProductSnapshotDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<ProductSnapshotDbContext>();
        var connectionString = args.Length != 0 ? args[0] : "Server=localhost;Username=virto;Password=virto;Database=VirtoCommerce3;";

        builder.UseNpgsql(
            connectionString,
            options => options.MigrationsAssembly(typeof(PostgreSqlDataAssemblyMarker).Assembly.GetName().Name));

        return new ProductSnapshotDbContext(builder.Options);
    }
}

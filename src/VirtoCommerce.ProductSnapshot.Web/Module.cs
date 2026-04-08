using System;
using GraphQL;
using GraphQL.MicrosoftDI;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Data.MySql.Extensions;
using VirtoCommerce.Platform.Data.PostgreSql.Extensions;
using VirtoCommerce.Platform.Data.SqlServer.Extensions;
using VirtoCommerce.ProductSnapshot.Core;
using VirtoCommerce.ProductSnapshot.Core.Services;
using VirtoCommerce.ProductSnapshot.Data.Handlers;
using VirtoCommerce.ProductSnapshot.Data.MySql;
using VirtoCommerce.ProductSnapshot.Data.PostgreSql;
using VirtoCommerce.ProductSnapshot.Data.Repositories;
using VirtoCommerce.ProductSnapshot.Data.Services;
using VirtoCommerce.ProductSnapshot.Data.SqlServer;
using VirtoCommerce.ProductSnapshot.ExperienceApi;
using VirtoCommerce.ProductSnapshot.ExperienceApi.Middlewares;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Pipelines;
using VirtoCommerce.XOrder.Core.Models;

namespace VirtoCommerce.ProductSnapshot.Web;

public class Module : IModule, IHasConfiguration
{
    public ManifestModuleInfo ModuleInfo { get; set; }
    public IConfiguration Configuration { get; set; }

    public void Initialize(IServiceCollection serviceCollection)
    {
        serviceCollection.AddDbContext<ProductSnapshotDbContext>(options =>
        {
            var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");
            var connectionString = Configuration.GetConnectionString(ModuleInfo.Id) ?? Configuration.GetConnectionString("VirtoCommerce");

            switch (databaseProvider)
            {
                case "MySql":
                    options.UseMySqlDatabase(connectionString, typeof(MySqlDataAssemblyMarker), Configuration);
                    break;
                case "PostgreSql":
                    options.UsePostgreSqlDatabase(connectionString, typeof(PostgreSqlDataAssemblyMarker), Configuration);
                    break;
                default:
                    options.UseSqlServerDatabase(connectionString, typeof(SqlServerDataAssemblyMarker), Configuration);
                    break;
            }
        });

        // Xapi and Pipelines
        var graphQlBuilder = new GraphQLBuilder(serviceCollection, builder =>
        {
            builder.AddSchema(serviceCollection, typeof(XapiAssemblyMarker));
        });

        serviceCollection.AddPipeline<ExternalOrderProducts>(builder =>
        {
            builder.AddMiddleware(typeof(LoadorderProductSnapshotMiddleware));
        });

        // Register services
        serviceCollection.AddTransient<IProductSnapshotRepository, ProductSnapshotRepository>();
        serviceCollection.AddSingleton<Func<IProductSnapshotRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<IProductSnapshotRepository>());

        serviceCollection.AddTransient<IOrderProductSnapshotService, OrderProductSnapshotService>();
        serviceCollection.AddTransient<IOrderProductSnapshotSearchService, OrderProductSnapshotSearchService>();

        serviceCollection.AddTransient<ICatalogProductSnapshotProvider, VirtoCatalogSnapshotProvider>();
        serviceCollection.AddTransient<CreateOrderProductSnapshotEventHandler>();
    }

    public void PostInitialize(IApplicationBuilder appBuilder)
    {
        var serviceProvider = appBuilder.ApplicationServices;

        // Register settings
        var settingsRegistrar = serviceProvider.GetRequiredService<ISettingsRegistrar>();
        settingsRegistrar.RegisterSettings(ModuleConstants.Settings.AllSettings, ModuleInfo.Id);

        // Register permissions
        var permissionsRegistrar = serviceProvider.GetRequiredService<IPermissionsRegistrar>();
        permissionsRegistrar.RegisterPermissions(ModuleInfo.Id, "ProductSnapshot", ModuleConstants.Security.Permissions.AllPermissions);

        // Apply migrations
        using var serviceScope = serviceProvider.CreateScope();
        using var dbContext = serviceScope.ServiceProvider.GetRequiredService<ProductSnapshotDbContext>();
        dbContext.Database.Migrate();

        appBuilder.RegisterEventHandler<OrderChangedEvent, CreateOrderProductSnapshotEventHandler>();
    }

    public void Uninstall()
    {
        // Nothing to do here
    }
}

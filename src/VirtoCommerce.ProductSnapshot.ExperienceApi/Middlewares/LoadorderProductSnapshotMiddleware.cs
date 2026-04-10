using System;
using System.Threading.Tasks;
using PipelineNet.Middleware;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.ProductSnapshot.Core.Services;
using VirtoCommerce.XCatalog.Core.Models;
using VirtoCommerce.XOrder.Core.Models;

namespace VirtoCommerce.ProductSnapshot.ExperienceApi.Middlewares;

public class LoadorderProductSnapshotMiddleware : IAsyncMiddleware<ExternalOrderProducts>
{
    private readonly ICatalogProductSnapshotProvider _snapshotProvider;

    public LoadorderProductSnapshotMiddleware(ICatalogProductSnapshotProvider snapshotProvider)
    {
        _snapshotProvider = snapshotProvider;
    }

    public async Task Run(ExternalOrderProducts parameter, Func<ExternalOrderProducts, Task> next)
    {
        if (parameter.OrderId.IsNullOrEmpty() || parameter.Products.IsNullOrEmpty())
        {
            await next(parameter);
        }

        var snapshots = await _snapshotProvider.GetOrderProductSnapshotsAsync(parameter.OrderId);

        foreach (var snapshot in snapshots)
        {
            if (parameter.Products.TryGetValue(snapshot.Id, out var product) && product == null)
            {
                parameter.Products[snapshot.Id] = GetExpProduct(snapshot);
            }
        }

        await next(parameter);
    }

    private static ExpProduct GetExpProduct(CatalogProduct catalogProduct)
    {
        var result = AbstractTypeFactory<ExpProduct>.TryCreateInstance();

        result.IndexedProduct = catalogProduct;

        return result;
    }
}

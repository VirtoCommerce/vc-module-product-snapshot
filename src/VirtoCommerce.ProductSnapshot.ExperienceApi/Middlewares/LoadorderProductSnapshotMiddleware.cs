using System;
using System.Linq;
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
        if (parameter.OrderId.IsNullOrEmpty() || parameter.ProductIds.IsNullOrEmpty())
        {
            await next(parameter);
        }

        var snapshots = await _snapshotProvider.GetOrderProductSnapshotsAsync(parameter.OrderId, parameter.ProductIds);

        parameter.Products ??= [];
        foreach (var snapshot in snapshots)
        {
            if (!parameter.Products.Any(x => x.Id.EqualsIgnoreCase(snapshot.Id)))
            {
                parameter.Products.Add(GetExpProduct(snapshot));
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

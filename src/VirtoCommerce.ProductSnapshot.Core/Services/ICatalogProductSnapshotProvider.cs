using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model;

namespace VirtoCommerce.ProductSnapshot.Core.Services;

public interface ICatalogProductSnapshotProvider
{
    Task SaveOrderProductSnapshotsAsync(CustomerOrder order);

    Task<bool> HasOrderProductSnapshotsAsync(string orderId);

    Task<IList<CatalogProduct>> GetOrderProductSnapshotsAsync(string orderId);

    Task<IList<CatalogProduct>> GetOrderProductSnapshotsAsync(string orderId, IList<string> productIds);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.ProductSnapshot.Core;
using VirtoCommerce.ProductSnapshot.Core.Models;
using VirtoCommerce.ProductSnapshot.Core.Services;

namespace VirtoCommerce.ProductSnapshot.Data.Services;

public class VirtoCatalogSnapshotProvider : ICatalogProductSnapshotProvider
{
    protected virtual string ProductSnapshotResponseGroup { get; } =
        (ItemResponseGroup.ItemInfo |
        ItemResponseGroup.ItemAssets |
        ItemResponseGroup.ItemProperties |
        ItemResponseGroup.ItemEditorialReviews).ToString();

    private readonly IItemService _itemService;
    private readonly IOrderProductSnapshotService _snapshotService;
    private readonly IOrderProductSnapshotSearchService _snapshotSearchService;
    private readonly ISettingsManager _settingsManager;

    public VirtoCatalogSnapshotProvider(
        IItemService itemService,
        IOrderProductSnapshotService snapshotService,
        IOrderProductSnapshotSearchService snapshotSearchService,
        ISettingsManager settingsManager)
    {
        _itemService = itemService;
        _snapshotService = snapshotService;
        _snapshotSearchService = snapshotSearchService;
        _settingsManager = settingsManager;
    }

    public async Task SaveOrderProductSnapshotsAsync(CustomerOrder order)
    {
        if (order.Items.IsNullOrEmpty())
        {
            return;
        }

        var productIds = GetProductIds(order.Items);
        if (productIds.Count == 0)
        {
            return;
        }

        // Never overwrite existing snapshots — only create snapshots for products
        // that don't already have one for this order. Combined with the unique
        // (OrderId, ProductId) index, this enforces one-snapshot-per-product-per-order.
        var existingProductIds = await GetExistingSnapshotProductIdsAsync(order.Id, productIds);
        var newProductIds = productIds
            .Where(id => !existingProductIds.Contains(id))
            .ToList();

        if (newProductIds.Count == 0)
        {
            return;
        }

        var batchSize = await _settingsManager.GetValueAsync<int>(ModuleConstants.Settings.General.ProductSnapshotBatchSize);
        var allSnapshots = new List<OrderProductSnapshot>();

        foreach (var batchIds in newProductIds.Paginate(batchSize))
        {
            var products = await _itemService.GetNoCloneAsync(batchIds, ProductSnapshotResponseGroup);

            if (!products.IsNullOrEmpty())
            {
                allSnapshots.AddRange(CreateOrderProductSnapshots(order, products));
            }
        }

        if (allSnapshots.Count > 0)
        {
            await _snapshotService.SaveChangesAsync(allSnapshots);
        }
    }

    public async Task<bool> HasOrderProductSnapshotsAsync(string orderId)
    {
        var searchCriteria = new OrderProductSnapshotSearchCriteria
        {
            OrderIds = [orderId],
            Take = 1,
        };

        var result = await _snapshotSearchService.SearchAsync(searchCriteria);

        return result.TotalCount > 0;
    }

    public async Task<IList<CatalogProduct>> GetOrderProductSnapshotsAsync(string orderId)
    {
        var searchCriteria = new OrderProductSnapshotSearchCriteria
        {
            OrderIds = [orderId],
        };

        var snapshots = await _snapshotSearchService.SearchAllNoCloneAsync(searchCriteria);

        return snapshots.Select(x => x.Product).ToList();
    }

    public async Task<IList<CatalogProduct>> GetOrderProductSnapshotsAsync(string orderId, IList<string> productIds)
    {
        var searchCriteria = new OrderProductSnapshotSearchCriteria
        {
            OrderIds = [orderId],
            ProductIds = productIds,
        };

        var snapshots = await _snapshotSearchService.SearchAllNoCloneAsync(searchCriteria);

        return snapshots.Select(x => x.Product).ToList();
    }

    private async Task<HashSet<string>> GetExistingSnapshotProductIdsAsync(string orderId, IList<string> productIds)
    {
        var searchCriteria = new OrderProductSnapshotSearchCriteria
        {
            OrderIds = [orderId],
            ProductIds = productIds,
        };

        var existing = await _snapshotSearchService.SearchAllNoCloneAsync(searchCriteria);

        return new HashSet<string>(existing.Select(x => x.ProductId), StringComparer.OrdinalIgnoreCase);
    }

    private static List<OrderProductSnapshot> CreateOrderProductSnapshots(CustomerOrder order, IList<CatalogProduct> products)
    {
        return products
            .Where(x => x != null)
            .Select(product => new OrderProductSnapshot
            {
                OrderId = order.Id,
                ProductId = product.Id,
                Sku = product.Code,
                Product = product,
            })
            .ToList();
    }

    protected virtual IList<string> GetProductIds(ICollection<LineItem> items)
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var lineItem in items)
        {
            if (!string.IsNullOrEmpty(lineItem.ProductId))
            {
                ids.Add(lineItem.ProductId);
            }

            if (lineItem.ConfigurationItems.IsNullOrEmpty())
            {
                continue;
            }

            foreach (var configurationItem in lineItem.ConfigurationItems)
            {
                if (!string.IsNullOrEmpty(configurationItem.ProductId))
                {
                    ids.Add(configurationItem.ProductId);
                }
            }
        }

        return ids.ToList();
    }
}

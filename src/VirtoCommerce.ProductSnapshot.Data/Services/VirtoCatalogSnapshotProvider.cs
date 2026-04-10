using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.ProductSnapshot.Core.Models;
using VirtoCommerce.ProductSnapshot.Core.Services;

namespace VirtoCommerce.ProductSnapshot.Data.Services;

public class VirtoCatalogSnapshotProvider : ICatalogProductSnapshotProvider
{
    private const int BatchSize = 20;

    protected virtual string ProductSnapshotResponseGroup { get; } =
        (ItemResponseGroup.ItemInfo |
        ItemResponseGroup.ItemAssets |
        ItemResponseGroup.ItemProperties |
        ItemResponseGroup.ItemEditorialReviews).ToString();

    private readonly IItemService _itemService;
    private readonly IOrderProductSnapshotService _snapshotService;
    private readonly IOrderProductSnapshotSearchService _snapshotSearchService;

    public VirtoCatalogSnapshotProvider(IItemService itemService,
        IOrderProductSnapshotService snapshotService,
        IOrderProductSnapshotSearchService snapshotSearchService)
    {
        _itemService = itemService;
        _snapshotService = snapshotService;
        _snapshotSearchService = snapshotSearchService;
    }

    public async Task SaveOrderProductSnapshotsAsync(CustomerOrder order)
    {
        if (order.Items.IsNullOrEmpty())
        {
            return;
        }

        var productToItemsMap = GetProductToItemsMap(order.Items);
        if (productToItemsMap.Count == 0)
        {
            return;
        }

        foreach (var batchIds in productToItemsMap.Keys.Paginate(BatchSize))
        {
            var products = await _itemService.GetNoCloneAsync(batchIds, ProductSnapshotResponseGroup);

            if (products.IsNullOrEmpty())
            {
                continue;
            }

            var snapshots = CreatOrderProductSnapshots(order, productToItemsMap, products);

            await _snapshotService.SaveChangesAsync(snapshots);
        }
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

    private static List<OrderProductSnapshot> CreatOrderProductSnapshots(CustomerOrder order, IDictionary<string, List<(LineItem LineItem, ConfigurationItem ConfigurationItem)>> productToItemsMap, IList<CatalogProduct> products)
    {
        var snapshots = new List<OrderProductSnapshot>();

        foreach (var product in products.Where(x => x != null))
        {
            if (!productToItemsMap.TryGetValue(product.Id, out var items))
            {
                continue;
            }

            var orderProductSnapshot = new OrderProductSnapshot
            {
                OrderId = order.Id,
                ProductId = product.Id,
                Product = product,
            };

            foreach (var (lineItem, configurationItem) in items)
            {
                orderProductSnapshot.LineItemId = lineItem.Id;

                if (configurationItem != null)
                {
                    orderProductSnapshot.ConfigurationItemId = configurationItem.Id;
                }
            }

            snapshots.Add(orderProductSnapshot);
        }

        return snapshots;
    }

    protected virtual IDictionary<string, List<(LineItem LineItem, ConfigurationItem ConfigurationItem)>> GetProductToItemsMap(ICollection<LineItem> items)
    {
        var map = new Dictionary<string, List<(LineItem, ConfigurationItem)>>(StringComparer.OrdinalIgnoreCase);

        foreach (var lineItem in items)
        {
            AddToMap(lineItem.ProductId, lineItem);

            if (lineItem.ConfigurationItems.IsNullOrEmpty())
            {
                continue;
            }

            foreach (var configurationItem in lineItem.ConfigurationItems)
            {
                AddToMap(configurationItem.ProductId, lineItem, configurationItem);
            }
        }

        return map;

        void AddToMap(string productId, LineItem lineItem, ConfigurationItem configurationItem = null)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return;
            }

            if (!map.TryGetValue(productId, out var itemList))
            {
                itemList = [];
                map[productId] = itemList;
            }

            itemList.Add((lineItem, configurationItem));
        }
    }


}

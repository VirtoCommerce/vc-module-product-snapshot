using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.ProductSnapshot.Core.Models;
using VirtoCommerce.ProductSnapshot.Core.Services;
using VirtoCommerce.ProductSnapshot.Data.Models;
using VirtoCommerce.ProductSnapshot.Data.Repositories;

namespace VirtoCommerce.ProductSnapshot.Data.Services;

public class OrderProductSnapshotSearchService(
    Func<IProductSnapshotRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    IOrderProductSnapshotService crudService,
    IOptions<CrudOptions> crudOptions)
    : SearchService<OrderProductSnapshotSearchCriteria, OrderProductSnapshotSearchResult, OrderProductSnapshot, OrderProductSnapshotEntity>
        (repositoryFactory, platformMemoryCache, crudService, crudOptions),
        IOrderProductSnapshotSearchService
{
    protected override IQueryable<OrderProductSnapshotEntity> BuildQuery(IRepository repository, OrderProductSnapshotSearchCriteria criteria)
    {
        var query = ((IProductSnapshotRepository)repository).OrderProductSnapshots;

        if (!criteria.OrderIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.OrderIds.Contains(x.OrderId));
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(OrderProductSnapshotSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(OrderProductSnapshotEntity.CreatedDate), SortDirection = SortDirection.Descending },
                new SortInfo { SortColumn = nameof(OrderProductSnapshotEntity.Id) },
            ];
        }

        return sortInfos;
    }
}

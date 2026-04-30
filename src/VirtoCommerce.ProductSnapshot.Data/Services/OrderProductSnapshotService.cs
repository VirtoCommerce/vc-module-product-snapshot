using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.ProductSnapshot.Core.Events;
using VirtoCommerce.ProductSnapshot.Core.Models;
using VirtoCommerce.ProductSnapshot.Core.Services;
using VirtoCommerce.ProductSnapshot.Data.Models;
using VirtoCommerce.ProductSnapshot.Data.Repositories;

namespace VirtoCommerce.ProductSnapshot.Data.Services;

public class OrderProductSnapshotService(
    Func<IProductSnapshotRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    IEventPublisher eventPublisher)
    : CrudService<OrderProductSnapshot, OrderProductSnapshotEntity, ProductSnapshotChangingEvent, ProductSnapshotChangedEvent>
        (repositoryFactory, platformMemoryCache, eventPublisher),
        IOrderProductSnapshotService
{
    protected override Task<IList<OrderProductSnapshotEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((IProductSnapshotRepository)repository).GetOrderProductSnapshotsByIdsAsync(ids, responseGroup);
    }
}

using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.ProductSnapshot.Core.Models;

namespace VirtoCommerce.ProductSnapshot.Core.Services;

public interface IOrderProductSnapshotSearchService : ISearchService<OrderProductSnapshotSearchCriteria, OrderProductSnapshotSearchResult, OrderProductSnapshot>;

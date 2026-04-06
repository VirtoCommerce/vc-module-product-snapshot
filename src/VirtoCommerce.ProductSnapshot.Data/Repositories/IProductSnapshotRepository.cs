using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.ProductSnapshot.Data.Models;

namespace VirtoCommerce.ProductSnapshot.Data.Repositories;

public interface IProductSnapshotRepository : IRepository
{
    IQueryable<OrderProductSnapshotEntity> OrderProductSnapshots { get; }

    Task<IList<OrderProductSnapshotEntity>> GetOrderProductSnapshotsByIdsAsync(IList<string> ids, string responseGroup);
}

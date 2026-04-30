using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.ProductSnapshot.Data.Models;

namespace VirtoCommerce.ProductSnapshot.Data.Repositories;

public class ProductSnapshotRepository(ProductSnapshotDbContext dbContext, IUnitOfWork unitOfWork = null)
    : DbContextRepositoryBase<ProductSnapshotDbContext>(dbContext, unitOfWork),
        IProductSnapshotRepository
{
    public IQueryable<OrderProductSnapshotEntity> OrderProductSnapshots => DbContext.Set<OrderProductSnapshotEntity>();

    public virtual async Task<IList<OrderProductSnapshotEntity>> GetOrderProductSnapshotsByIdsAsync(IList<string> ids, string responseGroup)
    {
        if (ids.IsNullOrEmpty())
        {
            return [];
        }

        return ids.Count == 1
            ? await OrderProductSnapshots.Where(x => x.Id == ids.First()).ToListAsync()
            : await OrderProductSnapshots.Where(x => ids.Contains(x.Id)).ToListAsync();
    }
}

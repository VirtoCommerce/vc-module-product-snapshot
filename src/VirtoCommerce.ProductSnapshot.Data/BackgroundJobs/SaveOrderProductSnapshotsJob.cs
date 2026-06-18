using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.ProductSnapshot.Core.Services;

namespace VirtoCommerce.ProductSnapshot.Data.BackgroundJobs;

public class SaveOrderProductSnapshotsJob(
    ICustomerOrderService orderService,
    ICatalogProductSnapshotProvider productSnapshotProvider)
{
    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(string orderId)
    {
        var orders = await orderService.GetAsync([orderId], clone: false);
        var order = orders?.FirstOrDefault();

        if (order == null)
        {
            return;
        }

        try
        {
            await productSnapshotProvider.SaveOrderProductSnapshotsAsync(order);
        }
        catch (DbUpdateException)
        {
            // Re-throw unless another concurrent job already created the snapshots.
            // The unique (OrderId, ProductId) constraint prevents duplicates when two jobs
            // race for the same order — in that case the second job should simply stop.
            if (!await productSnapshotProvider.HasOrderProductSnapshotsAsync(orderId))
            {
                throw;
            }
        }
    }
}

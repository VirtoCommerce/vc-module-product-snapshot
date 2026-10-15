using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.ProductSnapshot.Core.Services;

namespace VirtoCommerce.ProductSnapshot.Data.BackgroundJobs;

/// <summary>
/// Saves the product snapshots of a created or changed order, off the request thread.
/// </summary>
public class SaveOrderProductSnapshotsJobHandler(
    ICustomerOrderService orderService,
    ICatalogProductSnapshotProvider productSnapshotProvider) : IBackgroundJobHandler<SaveOrderProductSnapshotsJobPayload>
{
    /// <summary>
    /// Retry attempts requested on enqueue; replaces the former Hangfire [AutomaticRetry(Attempts = 3)].
    /// </summary>
    public const int MaxRetryAttempts = 3;

    public virtual async Task Execute(SaveOrderProductSnapshotsJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        var orderId = payload.OrderId;

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

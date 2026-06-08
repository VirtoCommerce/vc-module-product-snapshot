using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.ProductSnapshot.Core;
using VirtoCommerce.ProductSnapshot.Data.BackgroundJobs;

namespace VirtoCommerce.ProductSnapshot.Data.Handlers;

public class CreateOrderProductSnapshotEventHandler(
    ISettingsManager settingsManager,
    IBackgroundJobClient backgroundJobClient) : IEventHandler<OrderChangedEvent>
{
    public async Task Handle(OrderChangedEvent message)
    {
        if (!await settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.ProductSnapshotEnabled))
        {
            return;
        }

        var orderIds = message.ChangedEntries
            .Where(x => x.EntryState == EntryState.Added || x.EntryState == EntryState.Modified)
            .Select(x => x.NewEntry.Id)
            .Distinct()
            .ToArray();

        foreach (var orderId in orderIds)
        {
            backgroundJobClient.Enqueue<SaveOrderProductSnapshotsJob>(job => job.ExecuteAsync(orderId));
        }
    }
}

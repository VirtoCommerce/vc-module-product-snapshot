using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.ProductSnapshot.Core;
using VirtoCommerce.ProductSnapshot.Data.BackgroundJobs;

namespace VirtoCommerce.ProductSnapshot.Data.Handlers;

public class CreateOrderProductSnapshotEventHandler(
    ISettingsManager settingsManager) : IEventHandler<OrderChangedEvent>
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

        var options = new EnqueueOptions { MaxRetryAttempts = SaveOrderProductSnapshotsJobHandler.MaxRetryAttempts };

        foreach (var orderId in orderIds)
        {
            var payload = AbstractTypeFactory<SaveOrderProductSnapshotsJobPayload>.TryCreateInstance();
            payload.OrderId = orderId;

            // The static facade, not an injected IBackgroundJob: RegisterEventHandler resolves this handler once from
            // the root provider and holds it for the process lifetime, so it must not capture a Scoped dependency.
            await BackgroundJob.Enqueue<SaveOrderProductSnapshotsJobHandler>(payload, options);
        }
    }
}

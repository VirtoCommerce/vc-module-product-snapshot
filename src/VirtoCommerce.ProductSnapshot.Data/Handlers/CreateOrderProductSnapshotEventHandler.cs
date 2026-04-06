using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.ProductSnapshot.Core;
using VirtoCommerce.ProductSnapshot.Core.Services;

namespace VirtoCommerce.ProductSnapshot.Data.Handlers;


public class CreateOrderProductSnapshotEventHandler : IEventHandler<OrderChangedEvent>
{
    private readonly ISettingsManager _settingsManager;

    private readonly ICatalogProductSnapshotProvider _productSnapshotProvider;

    public CreateOrderProductSnapshotEventHandler(ISettingsManager settingsManager, ICatalogProductSnapshotProvider productSnapshotProvider)
    {
        _settingsManager = settingsManager;
        _productSnapshotProvider = productSnapshotProvider;
    }

    public async Task Handle(OrderChangedEvent message)
    {
        if (!await _settingsManager.GetValueAsync<bool>(ModuleConstants.Settings.General.ProductSnapshotEnabled))
        {
            return;
        }

        var orders = message.ChangedEntries
            .Where(x => x.EntryState == EntryState.Added)
            .Select(x => x.NewEntry)
            .ToArray();

        foreach (var order in orders)
        {
            await _productSnapshotProvider.SaveOrderProductSnapshotsAsync(order);
        }
    }
}

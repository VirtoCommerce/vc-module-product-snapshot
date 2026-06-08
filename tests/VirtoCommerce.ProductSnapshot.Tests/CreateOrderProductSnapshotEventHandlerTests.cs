using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Moq;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.ProductSnapshot.Core;
using VirtoCommerce.ProductSnapshot.Data.BackgroundJobs;
using VirtoCommerce.ProductSnapshot.Data.Handlers;
using Xunit;

namespace VirtoCommerce.ProductSnapshot.Tests;

[Trait("Category", "Unit")]
public class CreateOrderProductSnapshotEventHandlerTests
{
    private readonly Mock<ISettingsManager> _settingsManagerMock = new();
    private readonly Mock<IBackgroundJobClient> _backgroundJobClientMock = new();
    private readonly CreateOrderProductSnapshotEventHandler _handler;

    public CreateOrderProductSnapshotEventHandlerTests()
    {
        _handler = new CreateOrderProductSnapshotEventHandler(
            _settingsManagerMock.Object,
            _backgroundJobClientMock.Object);
    }

    [Fact]
    public async Task Handle_WhenSnapshotDisabled_DoesNotEnqueueAnyJob()
    {
        SetupSnapshotEnabled(false);
        var @event = BuildEvent([BuildEntry("order-1", EntryState.Added)]);

        await _handler.Handle(@event);

        _backgroundJobClientMock.Verify(
            x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAddedOrder_EnqueuesJob()
    {
        SetupSnapshotEnabled(true);
        var @event = BuildEvent([BuildEntry("order-1", EntryState.Added)]);

        await _handler.Handle(@event);

        VerifyEnqueued("order-1", Times.Once());
    }

    [Fact]
    public async Task Handle_WhenModifiedOrder_EnqueuesJob()
    {
        SetupSnapshotEnabled(true);
        var @event = BuildEvent([BuildEntry("order-2", EntryState.Modified)]);

        await _handler.Handle(@event);

        VerifyEnqueued("order-2", Times.Once());
    }

    [Fact]
    public async Task Handle_WhenDeletedOrder_DoesNotEnqueueJob()
    {
        SetupSnapshotEnabled(true);
        var @event = BuildEvent([BuildEntry("order-3", EntryState.Deleted)]);

        await _handler.Handle(@event);

        _backgroundJobClientMock.Verify(
            x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSameOrderAppearsMultipleTimes_EnqueuesOnlyOnce()
    {
        SetupSnapshotEnabled(true);
        var @event = BuildEvent([
            BuildEntry("order-4", EntryState.Added),
            BuildEntry("order-4", EntryState.Modified),
        ]);

        await _handler.Handle(@event);

        VerifyEnqueued("order-4", Times.Once());
    }

    [Fact]
    public async Task Handle_WhenMultipleDistinctOrders_EnqueuesJobForEach()
    {
        SetupSnapshotEnabled(true);
        var @event = BuildEvent([
            BuildEntry("order-5", EntryState.Added),
            BuildEntry("order-6", EntryState.Modified),
        ]);

        await _handler.Handle(@event);

        VerifyEnqueued("order-5", Times.Once());
        VerifyEnqueued("order-6", Times.Once());
    }

    private void SetupSnapshotEnabled(bool enabled) =>
        _settingsManagerMock
            .Setup(x => x.GetObjectSettingAsync(
                ModuleConstants.Settings.General.ProductSnapshotEnabled.Name, null, null))
            .ReturnsAsync(new ObjectSettingEntry { Value = enabled });

    private void VerifyEnqueued(string orderId, Times times) =>
        _backgroundJobClientMock.Verify(
            x => x.Create(
                It.Is<Job>(j =>
                    j.Type == typeof(SaveOrderProductSnapshotsJob) &&
                    j.Method.Name == nameof(SaveOrderProductSnapshotsJob.ExecuteAsync) &&
                    (string)j.Args[0] == orderId),
                It.IsAny<IState>()),
            times);

    private static OrderChangedEvent BuildEvent(
        IEnumerable<GenericChangedEntry<CustomerOrder>> entries) =>
        new(entries);

    private static GenericChangedEntry<CustomerOrder> BuildEntry(string orderId, EntryState state) =>
        new(new CustomerOrder { Id = orderId }, state);
}

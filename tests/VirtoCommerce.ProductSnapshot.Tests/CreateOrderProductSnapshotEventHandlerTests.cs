using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.ProductSnapshot.Core;
using VirtoCommerce.ProductSnapshot.Data.BackgroundJobs;
using VirtoCommerce.ProductSnapshot.Data.Handlers;
using Xunit;

namespace VirtoCommerce.ProductSnapshot.Tests;

// Any other test class that enqueues through the static BackgroundJob facade must join this collection:
// the facade has no reset API (Initialize rejects null), so Dispose leaves a DISPOSED provider behind in
// the static, and a class racing this one would see ObjectDisposedException from it.
[Collection(nameof(CreateOrderProductSnapshotEventHandlerTests))]
[Trait("Category", "Unit")]
public class CreateOrderProductSnapshotEventHandlerTests
{
    private readonly Mock<ISettingsManager> _settingsManagerMock = new();
    private readonly CreateOrderProductSnapshotEventHandler _handler;

    public CreateOrderProductSnapshotEventHandlerTests()
    {
        _handler = new CreateOrderProductSnapshotEventHandler(_settingsManagerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenSnapshotDisabled_DoesNotEnqueueAnyJob()
    {
        using var capture = new EnqueueCapture();
        SetupSnapshotEnabled(false);
        var @event = BuildEvent([BuildEntry("order-1", EntryState.Added)]);

        await _handler.Handle(@event);

        Assert.Empty(capture.Payloads);
    }

    [Fact]
    public async Task Handle_WhenAddedOrder_EnqueuesJob()
    {
        using var capture = new EnqueueCapture();
        SetupSnapshotEnabled(true);
        var @event = BuildEvent([BuildEntry("order-1", EntryState.Added)]);

        await _handler.Handle(@event);

        VerifyEnqueued(capture, "order-1", times: 1);
    }

    [Fact]
    public async Task Handle_WhenModifiedOrder_EnqueuesJob()
    {
        using var capture = new EnqueueCapture();
        SetupSnapshotEnabled(true);
        var @event = BuildEvent([BuildEntry("order-2", EntryState.Modified)]);

        await _handler.Handle(@event);

        VerifyEnqueued(capture, "order-2", times: 1);
    }

    [Fact]
    public async Task Handle_WhenDeletedOrder_DoesNotEnqueueJob()
    {
        using var capture = new EnqueueCapture();
        SetupSnapshotEnabled(true);
        var @event = BuildEvent([BuildEntry("order-3", EntryState.Deleted)]);

        await _handler.Handle(@event);

        Assert.Empty(capture.Payloads);
    }

    [Fact]
    public async Task Handle_WhenSameOrderAppearsMultipleTimes_EnqueuesOnlyOnce()
    {
        using var capture = new EnqueueCapture();
        SetupSnapshotEnabled(true);
        var @event = BuildEvent([
            BuildEntry("order-4", EntryState.Added),
            BuildEntry("order-4", EntryState.Modified),
        ]);

        await _handler.Handle(@event);

        VerifyEnqueued(capture, "order-4", times: 1);
    }

    [Fact]
    public async Task Handle_WhenMultipleDistinctOrders_EnqueuesJobForEach()
    {
        using var capture = new EnqueueCapture();
        SetupSnapshotEnabled(true);
        var @event = BuildEvent([
            BuildEntry("order-5", EntryState.Added),
            BuildEntry("order-6", EntryState.Modified),
        ]);

        await _handler.Handle(@event);

        VerifyEnqueued(capture, "order-5", times: 1);
        VerifyEnqueued(capture, "order-6", times: 1);
    }

    [Fact]
    public async Task Handle_WhenEnqueued_KeepsRetryAttempts()
    {
        using var capture = new EnqueueCapture();
        SetupSnapshotEnabled(true);
        var @event = BuildEvent([BuildEntry("order-7", EntryState.Added)]);

        await _handler.Handle(@event);

        var options = Assert.Single(capture.Options);
        Assert.Equal(SaveOrderProductSnapshotsJobHandler.MaxRetryAttempts, options?.MaxRetryAttempts);
    }

    private void SetupSnapshotEnabled(bool enabled) =>
        _settingsManagerMock
            .Setup(x => x.GetObjectSettingAsync(
                ModuleConstants.Settings.General.ProductSnapshotEnabled.Name, null, null))
            .ReturnsAsync(new ObjectSettingEntry { Value = enabled });

    private static void VerifyEnqueued(EnqueueCapture capture, string orderId, int times) =>
        Assert.Equal(times, capture.Payloads.Count(x => x.OrderId == orderId));

    private static OrderChangedEvent BuildEvent(
        IEnumerable<GenericChangedEntry<CustomerOrder>> entries) =>
        new(entries);

    private static GenericChangedEntry<CustomerOrder> BuildEntry(string orderId, EntryState state) =>
        new(new CustomerOrder { Id = orderId }, state);

    // Captures what the handler enqueued through the static BackgroundJob facade. IBackgroundJob is registered
    // Scoped here exactly as the engine module registers it, so this also proves the facade's per-call scope
    // resolves it - the event handler itself is root-resolved and must never hold it.
    private sealed class EnqueueCapture : IDisposable
    {
        private readonly ServiceProvider _provider;

        public EnqueueCapture()
        {
            BackgroundJobMock
                .Setup(x => x.Enqueue<SaveOrderProductSnapshotsJobHandler>(It.IsAny<object>(), It.IsAny<EnqueueOptions>(), It.IsAny<CancellationToken>()))
                .Callback<object, EnqueueOptions, CancellationToken>((payload, options, _) =>
                {
                    Payloads.Add((SaveOrderProductSnapshotsJobPayload)payload);
                    Options.Add(options);
                })
                .ReturnsAsync("job-id");

            var services = new ServiceCollection();
            services.AddScoped(_ => BackgroundJobMock.Object);
            _provider = services.BuildServiceProvider(validateScopes: true);

            BackgroundJob.Initialize(_provider);
        }

        public Mock<IBackgroundJob> BackgroundJobMock { get; } = new();

        public List<SaveOrderProductSnapshotsJobPayload> Payloads { get; } = [];

        public List<EnqueueOptions> Options { get; } = [];

        public void Dispose()
        {
            _provider.Dispose();
        }
    }
}

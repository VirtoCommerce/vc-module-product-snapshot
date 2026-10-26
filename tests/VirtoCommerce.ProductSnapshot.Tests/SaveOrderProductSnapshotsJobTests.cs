using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.ProductSnapshot.Core.Services;
using VirtoCommerce.ProductSnapshot.Data.BackgroundJobs;
using Xunit;

namespace VirtoCommerce.ProductSnapshot.Tests;

[Trait("Category", "Unit")]
public class SaveOrderProductSnapshotsJobTests
{
    [Fact]
    public async Task ExecuteAsync_StillSavesForLegacyHangfireJobs()
    {
        // Hangfire stores a queued job as type + method name + serialized args, so a store written by an
        // earlier version can still invoke this method. Deleting it would strand those entries as Failed.
        var order = new CustomerOrder { Id = "order-1" };

        var orderServiceMock = new Mock<ICustomerOrderService>();
        orderServiceMock
            .Setup(x => x.GetAsync(It.Is<IList<string>>(ids => ids.Contains("order-1")), null, false))
            .ReturnsAsync([order]);

        var productSnapshotProviderMock = new Mock<ICatalogProductSnapshotProvider>();

        var job = new SaveOrderProductSnapshotsJob(
            new SaveOrderProductSnapshotsJobHandler(orderServiceMock.Object, productSnapshotProviderMock.Object));

#pragma warning disable VC0015
        await job.ExecuteAsync("order-1");
#pragma warning restore VC0015

        productSnapshotProviderMock.Verify(x => x.SaveOrderProductSnapshotsAsync(order), Times.Once);
    }
}

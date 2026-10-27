using System;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ProductSnapshot.Data.BackgroundJobs;

public class SaveOrderProductSnapshotsJob(SaveOrderProductSnapshotsJobHandler jobHandler)
{
    /// <summary>
    /// Kept for background jobs enqueued by an earlier version, which reference this method by name.
    /// New work goes through <see cref="SaveOrderProductSnapshotsJobHandler"/>; remove this once no such job
    /// can still be pending.
    /// </summary>
    // Signature is identical on purpose: Hangfire persists a queued job as type name + method name +
    // parameter types + serialized args, so changing any of them would strand already-queued jobs as Failed.
    [Obsolete("Enqueued indirectly by legacy Hangfire jobs only; new work uses SaveOrderProductSnapshotsJobHandler.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public Task ExecuteAsync(string orderId)
    {
        var payload = AbstractTypeFactory<SaveOrderProductSnapshotsJobPayload>.TryCreateInstance();
        payload.OrderId = orderId;

        return jobHandler.Execute(payload, context: null);
    }
}

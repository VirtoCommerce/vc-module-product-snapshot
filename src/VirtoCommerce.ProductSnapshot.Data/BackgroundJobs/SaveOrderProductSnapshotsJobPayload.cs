namespace VirtoCommerce.ProductSnapshot.Data.BackgroundJobs;

/// <summary>
/// Payload of the background job that saves the product snapshots of an order.
/// </summary>
public class SaveOrderProductSnapshotsJobPayload
{
    public string OrderId { get; set; }
}

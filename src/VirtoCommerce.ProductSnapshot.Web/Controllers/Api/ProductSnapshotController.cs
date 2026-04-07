using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.ProductSnapshot.Core.Models;
using VirtoCommerce.ProductSnapshot.Core.Services;
using Permissions = VirtoCommerce.ProductSnapshot.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.ProductSnapshot.Web.Controllers.Api;

[Authorize]
[Route("api/product-snapshots")]
public class ProductSnapshotController(
    IOrderProductSnapshotService crudService,
    IOrderProductSnapshotSearchService searchService,
    ICatalogProductSnapshotProvider snapshotProvider)
    : Controller
{
    [HttpPost("search")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<OrderProductSnapshotSearchResult>> Search([FromBody] OrderProductSnapshotSearchCriteria criteria)
    {
        var result = await searchService.SearchNoCloneAsync(criteria);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<OrderProductSnapshot>> GetById([FromRoute] string id, [FromQuery] string responseGroup = null)
    {
        var model = await crudService.GetNoCloneAsync(id, responseGroup);
        return Ok(model);
    }

    [HttpGet("order/{orderId}/product/{productId}")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<CatalogProduct>> GetByOrderAndProductId([FromRoute] string orderId, [FromRoute] string productId)
    {
        var productSnapshots = await snapshotProvider.GetOrderProductSnapshotsAsync(orderId);

        var productSnapshot = productSnapshots.FirstOrDefault(x => x.Id == productId);

        return Ok(productSnapshot);
    }
}

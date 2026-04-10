using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.ProductSnapshot.Core.Services;
using VirtoCommerce.XOrder.Data.Authorization;
using Permissions = VirtoCommerce.ProductSnapshot.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.ProductSnapshot.Web.Controllers.Api;

[Authorize]
[Route("api/product-snapshots")]
public class ProductSnapshotController(
    ICatalogProductSnapshotProvider snapshotProvider,
    ICustomerOrderService customerOrderService,
    IAuthorizationService authorizationService
    )
    : Controller
{
    [HttpGet("order/{orderId}/product/{productId}")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<CatalogProduct>> GetByOrderAndProductId([FromRoute] string orderId, [FromRoute] string productId)
    {
        var order = await customerOrderService.GetNoCloneAsync(orderId, CustomerOrderResponseGroup.Default.ToString());
        if (order == null)
        {
            return NotFound();
        }

        var authorizationResult = await AuthorizeOrderAsync(order);
        if (!authorizationResult.Succeeded)
        {
            return Forbid();
        }

        var productSnapshots = await snapshotProvider.GetOrderProductSnapshotsAsync(orderId);

        var productSnapshot = productSnapshots.FirstOrDefault(x => x.Id == productId);

        return Ok(productSnapshot);
    }

    private async Task<AuthorizationResult> AuthorizeOrderAsync(CustomerOrder order)
    {
        var authorizationResult = await authorizationService.AuthorizeAsync(User, order, new CanAccessOrderAuthorizationRequirement());
        return authorizationResult;
    }

}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.ProductSnapshot.Core.Models;
using VirtoCommerce.ProductSnapshot.Core.Services;
using Permissions = VirtoCommerce.ProductSnapshot.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.ProductSnapshot.Web.Controllers.Api;

[Authorize]
[Route("api/product-snapshots")]
public class ProductSnapshotController(
    IOrderProductSnapshotService crudService,
    IOrderProductSnapshotSearchService searchService)
    : Controller
{
    [HttpPost("search")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<OrderProductSnapshotSearchResult>> Search([FromBody] OrderProductSnapshotSearchCriteria criteria)
    {
        var result = await searchService.SearchNoCloneAsync(criteria);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Permissions.Create)]
    public Task<ActionResult<Core.Models.OrderProductSnapshot>> Create([FromBody] Core.Models.OrderProductSnapshot model)
    {
        model.Id = null;
        return Update(model);
    }

    [HttpPut]
    [Authorize(Permissions.Update)]
    public async Task<ActionResult<Core.Models.OrderProductSnapshot>> Update([FromBody] Core.Models.OrderProductSnapshot model)
    {
        await crudService.SaveChangesAsync([model]);
        return Ok(model);
    }

    [HttpGet("{id}")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<Core.Models.OrderProductSnapshot>> Get([FromRoute] string id, [FromQuery] string responseGroup = null)
    {
        var model = await crudService.GetNoCloneAsync(id, responseGroup);
        return Ok(model);
    }

    [HttpDelete]
    [Authorize(Permissions.Delete)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete([FromQuery] string[] ids)
    {
        await crudService.DeleteAsync(ids);
        return NoContent();
    }
}

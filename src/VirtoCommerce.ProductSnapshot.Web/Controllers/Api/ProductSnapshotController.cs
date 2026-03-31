using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Permissions = VirtoCommerce.ProductSnapshot.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.ProductSnapshot.Web.Controllers.Api;

[Authorize]
[Route("api/product-snapshot")]
public class ProductSnapshotController : Controller
{
    // GET: api/product-snapshot
    /// <summary>
    /// Get message
    /// </summary>
    /// <remarks>Return "Hello world!" message</remarks>
    [HttpGet]
    [Route("")]
    [Authorize(Permissions.Read)]
    public ActionResult<string> Get()
    {
        return Ok(new { result = "Hello world!" });
    }
}

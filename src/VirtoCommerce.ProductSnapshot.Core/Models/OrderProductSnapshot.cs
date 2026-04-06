using System;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ProductSnapshot.Core.Models;

public class OrderProductSnapshot : AuditableEntity, ICloneable
{
    public string OrderId { get; set; }

    public string LineItemId { get; set; }

    public string ConfigurationItemId { get; set; }

    public string ProductId { get; set; }

    public string Sku { get; set; }

    public CatalogProduct Product { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}

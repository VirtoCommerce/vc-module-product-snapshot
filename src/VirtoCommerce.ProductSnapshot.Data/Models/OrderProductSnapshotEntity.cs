using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Serialization;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.ProductSnapshot.Core.Models;

namespace VirtoCommerce.ProductSnapshot.Data.Models;

public class OrderProductSnapshotEntity : AuditableEntity, IDataEntity<OrderProductSnapshotEntity, OrderProductSnapshot>
{
    [Required]
    [StringLength(128)]
    public string OrderId { get; set; }

    [Required]
    [StringLength(128)]
    public string ProductId { get; set; }

    [StringLength(128)]
    public string Sku { get; set; }

    public string ProductJson { get; set; }

    public virtual OrderProductSnapshot ToModel(OrderProductSnapshot model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.OrderId = OrderId;
        model.ProductId = ProductId;
        model.Sku = Sku;

        model.Product = ProductJsonSerializer.DeserializePolymorphic<CatalogProduct>(ProductJson);

        return model;
    }

    public virtual OrderProductSnapshotEntity FromModel(OrderProductSnapshot model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        OrderId = model.OrderId;
        ProductId = model.ProductId;
        Sku = model.Sku;

        ProductJson = ProductJsonSerializer.Serialize(model.Product);

        return this;
    }

    public virtual void Patch(OrderProductSnapshotEntity target)
    {
        target.OrderId = OrderId;
        target.ProductId = ProductId;
        target.Sku = Sku;
        target.ProductJson = ProductJson;
    }
}

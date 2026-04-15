# Product Snapshot Module

## Overview

The Product Snapshot module captures and stores product information at the moment an order is created.
This ensures that customers and store operators always have access to the exact product details (prices, properties, images, descriptions)
that were valid at the time of purchase, even if the product catalog is later modified or products are deleted.

A category manager updates or removes a product from the catalog, but existing orders still display the original product information through snapshots.
If a snapshot does not exist for a given product, the system falls back to loading the current product data from the catalog.

## Features

- **Automatic snapshot creation** — listens for the `OrderChangedEvent` and asynchronously creates product snapshots when a new order is placed.
- **Full product data capture** — stores product info, assets, properties, and editorial reviews as a serialized JSON document.
- **Configurable** — snapshot creation can be enabled or disabled through a platform setting.
- **Catalog fallback** — products not covered by a snapshot are loaded by xOrder's `OrderProductResolver.LoadProductsAsync` from the live catalog. The fallback is provided by xOrder, not by this module.
- **Granular permissions** — access, create, read, update, and delete operations are controlled by dedicated permissions.
- **Extendable Product Snapshot Page** - the module provides extension points (productSnapshotDetails metaform and widget-container) on the Product Snapshot details page to display custom information related to the snapshot, such as links to related orders or custom product attributes.
- **REST API** — retrieve a product snapshot via `GET /api/product-snapshots/order/{orderId}/product/{productId}`.
- **GraphQL / Experience API integration** — the `LoadorderProductSnapshotMiddleware` transparently injects snapshots into the `ExternalOrderProducts` pipeline, so X-Order consumers receive snapshot data without additional queries.
- **Multi-database support** — SQL Server, MySQL, and PostgreSQL are supported out of the box.
- **Async snapshot creation (Soon)** — snapshot generation runs in the background to avoid impacting order processing performance. If snapshot creation fails, it is logged but does not block the order from being created.

## Screenshots

### View Product Snapshot 

<img width="902" height="627" alt="image" src="https://github.com/user-attachments/assets/66fd73d0-fa3d-473b-989d-c978793f363a" />

### Product Snapshot Blade

<img width="921" height="629" alt="image" src="https://github.com/user-attachments/assets/659a0c11-c520-455f-af48-990224835c4e" />

## Product Snapshot Blade - Fallback

<img width="890" height="566" alt="image" src="https://github.com/user-attachments/assets/ce697145-6dd4-49d6-93fd-133c5b894af9" />

## Configuration

### Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `ProductSnapshot.Enabled` | Boolean | `true` | Enables automatic product snapshot creation on new orders. Must be set to `true` to activate the module. |

Navigate to **Platform Settings > Product Snapshot > General** to configure.

### Permissions

| Permission | Description |
|------------|-------------|
| `product-snapshot:access` | Access the Product Snapshot module |
| `product-snapshot:create` | Create product snapshots |
| `product-snapshot:read` | Read product snapshots |
| `product-snapshot:update` | Update product snapshots |
| `product-snapshot:delete` | Delete product snapshots |


## Architecture Schema

### Snapshot Lifecycle

```
┌──────────────────────────────┐
│  Order Created or Updated    │
│  (new line items added)      │
└──────────────┬───────────────┘
               │ OrderChangedEvent
               │ (EntryState = Added or Modified)
               v
┌───────────────────────────────────────┐
│ CreateOrderProductSnapshotEventHandler│
│  - Checks ProductSnapshot.Enabled     │
│  - Filters entries: Added | Modified  │
└──────────────┬────────────────────────┘
               │ for each surviving order
               v
┌───────────────────────────────────────┐
│   VirtoCatalogSnapshotProvider        │
│   .SaveOrderProductSnapshotsAsync     │
│  - Collects product + configuration   │
│    item ids from order line items     │
│  - Skips ids already snapshotted      │
│    for this OrderId (idempotent)      │
│  - Loads products from Catalog        │
│    (ItemInfo | ItemAssets |           │
│     ItemProperties |                  │
│     ItemEditorialReviews)             │
│  - Serializes to JSON                 │
│  - Saves in batches of 20             │
└──────────────┬────────────────────────┘
               │
               v
┌───────────────────────────────────────┐
│      OrderProductSnapshot Table       │
│  (OrderId, ProductId, Sku, Product)   │
│  unique index: (OrderId, ProductId)   │
└───────────────────────────────────────┘
```

Snapshots are written on **both** `Added` and `Modified` order change entries, so line items added to an existing order after creation are still captured. The handler ignores `Deleted` and `Unchanged` entries. A unique `(OrderId, ProductId)` constraint combined with the idempotent skip in the provider guarantees exactly one snapshot per product per order — re-saving an unchanged order is a no-op.

### Snapshot Retrieval

```
┌──────────────────┐                 ┌──────────────────────────────┐
│  REST API Call   │                 │  GraphQL / xAPI Query        │
│  GET /api/...    │                 │  order.items { product {…} } │
└────────┬─────────┘                 └──────────────┬───────────────┘
         │                                          │
         │                                          v
         │                          ┌───────────────────────────────┐
         │                          │  IOrderProductResolver        │
         │                          │  (scoped, per-request cache)  │
         │                          └──────────────┬────────────────┘
         │                                         │
         │                                         v
         │                          ┌───────────────────────────────┐
         │                          │  ExternalOrderProducts        │
         │                          │  PipelineNet pipeline         │
         │                          └──────────────┬────────────────┘
         │                                         │
         │                                         v
         │                          ┌───────────────────────────────┐
         │                          │ LoadorderProductSnapshotMW    │
         │                          │  (this module)                │
         │                          └──────────────┬────────────────┘
         │                                         │
         v                                         v
┌──────────────────────────────────────┐   ┌───────────────────────────────┐
│   ICatalogProductSnapshotProvider    │   │  Missing products?            │
│  - Search snapshots by Order/Product │   │  OrderProductResolver         │
│  - Return CatalogProduct objects     │   │  .LoadProductsAsync           │
└──────────────────────────────────────┘   │  → XCatalog LoadProductsQuery │
                                           │  (live catalog fallback)      │
                                           └───────────────────────────────┘
```

REST API calls go straight through `ICatalogProductSnapshotProvider`. GraphQL / xAPI calls are routed through xOrder's `IOrderProductResolver`, which launches the `ExternalOrderProducts` pipeline. `LoadorderProductSnapshotMiddleware` fills in frozen snapshots; anything still missing falls through to xOrder's catalog fallback.

### Field Coverage

`LoadorderProductSnapshotMiddleware` wraps each snapshot in an `ExpProduct` with only `IndexedProduct` populated. Dynamic fields that normally come from the XCatalog pipeline (`AllPrices`, `Inventory` / `AllInventories`, `Vendor`, `Variations`, `MinVariationPrice`, availability flags) are **null or empty** in snapshot mode, because the middleware runs instead of the catalog enrichment middleware — not after it. Snapshots also capture only `ItemInfo | ItemAssets | ItemProperties | ItemEditorialReviews`, so `Associations`, `Outlines`, and `SeoInfos` on `IndexedProduct` itself are empty too.

See [docs/default-vs-snapshot-mode.md](docs/default-vs-snapshot-mode.md) for the full property-by-property matrix, pros and cons, and guidance on extending the middleware or widening the captured response group.

## Extensibility

The Product Snapshot details blade exposes the `productSnapshotDetails` [metaform](https://docs.virtocommerce.org/platform/developer-guide/latest/Platform-Manager/Extensibility-Points/metaform/) and a widget container, allowing other modules to add custom properties and widgets without modifying this module.

### Adding Custom Properties via Metaform

Register additional fields in your module's `module.js` `run` block:

```js
angular.module('YourModule')
    .run(['platformWebApp.metaFormsService', function (metaFormsService) {
        metaFormsService.registerMetaFields("productSnapshotDetails", [
            {
                name: "customWarrantyInfo",
                title: "Warranty Information",
                valueType: "ShortText"
            },
            {
                name: "isOversized",
                title: "Oversized Item",
                valueType: "Boolean"
            }
        ]);
    }]);
```

Registered fields will appear at the bottom of the Product Snapshot details blade inside the `<va-metaform>` block. The field data is bound to the blade's `currentEntity` scope.

### Adding Widgets

You can also register widgets into the `productSnapshotDetails` widget container:

```js
angular.module('YourModule')
    .run(['platformWebApp.widgetService', function (widgetService) {
        var widget = {
            controller: 'YourModule.YourWidgetController',
            template: 'Modules/$(YourModule)/Scripts/widgets/your-widget.html'
        };
        widgetService.registerWidget(widget, 'productSnapshotDetails');
    }]);
```

## References

* [Deployment](https://docs.virtocommerce.org/platform/developer-guide/Tutorials-and-How-tos/Tutorials/deploy-module-from-source-code/)
* [Installation](https://docs.virtocommerce.org/platform/user-guide/modules-installation/)
* [Home](https://virtocommerce.com)
* [Community](https://www.virtocommerce.org)
* [Download latest release](https://github.com/VirtoCommerce/vc-module-x-cart/releases/latest)


## License
Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.


# Product Snapshot Module

## Overview

The Product Snapshot module captures and stores product information at the moment an order is created. This ensures that customers and store operators always have access to the exact product details (prices, properties, images, descriptions) that were valid at the time of purchase, even if the product catalog is later modified or products are deleted.

**Key scenario:** A category manager updates or removes a product from the catalog, but existing orders still display the original product information through snapshots. If a snapshot does not exist for a given product, the system falls back to loading the current product data from the catalog.

## Features

- **Automatic snapshot creation** — listens for the `OrderChangedEvent` and asynchronously creates product snapshots when a new order is placed.
- **Full product data capture** — stores product info, assets, properties, and editorial reviews as a serialized JSON document.
- **Configurable** — snapshot creation can be enabled or disabled through a platform setting.
- **Catalog fallback** — if a snapshot does not exist for a product, the current catalog product is loaded as a fallback.
- **Granular permissions** — access, create, read, update, and delete operations are controlled by dedicated permissions.
- **REST API** — retrieve a product snapshot via `GET /api/product-snapshots/order/{orderId}/product/{productId}`.
- **GraphQL / Experience API integration** — the `LoadorderProductSnapshotMiddleware` transparently injects snapshots into the `ExternalOrderProducts` pipeline, so X-API consumers receive snapshot data without additional queries.
- **Multi-database support** — SQL Server, MySQL, and PostgreSQL are supported out of the box.

## Configuration

### Settings

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `ProductSnapshot.Enabled` | Boolean | `false` | Enables automatic product snapshot creation on new orders. Must be set to `true` to activate the module. |

Navigate to **Platform Settings > ProductSnapshot > General** to configure.

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
┌──────────────────┐
│  Order Created   │
└────────┬─────────┘
         │ OrderChangedEvent (EntryState = Added)
         v
┌───────────────────────────────────────┐
│ CreateOrderProductSnapshotEventHandler│
│  - Checks ProductSnapshot.Enabled     │
└────────┬──────────────────────────────┘
         │
         v
┌───────────────────────────────────────┐
│   VirtoCatalogSnapshotProvider        │
│  - Loads products from Catalog        │
│  - Serializes to JSON                 │
│  - Saves OrderProductSnapshot         │
└────────┬──────────────────────────────┘
         │
         v
┌───────────────────────────────────────┐
│      OrderProductSnapshot Table       │
│  (OrderId, ProductId, ProductJson)    │
└───────────────────────────────────────┘
```

### Snapshot Retrieval

```
┌──────────────────┐     ┌──────────────────────────┐
│  REST API Call   │     │  GraphQL / X-API Query   │
│  GET /api/...    │     │  ExternalOrderProducts   │
└────────┬─────────┘     └────────┬─────────────────┘
         │                        │
         v                        v
┌──────────────────────────────────────┐
│   ICatalogProductSnapshotProvider    │
│  - Search snapshots by Order/Product │
│  - Deserialize ProductJson           │
│  - Return CatalogProduct objects     │
│  - Fallback to live catalog if none  │
└──────────────────────────────────────┘
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


# Default Mode vs Product Snapshot Mode

This document compares how the `ExpProduct` returned for an order line item is populated depending on whether the **Product Snapshot** module is installed. It is meant for developers writing or reviewing GraphQL queries, resolvers, and middleware against the XOrder / XCatalog pipelines.

## TL;DR

| Aspect | Default Mode | Snapshot Mode |
| --- | --- | --- |
| Data source | Live catalog (search index + domain services) | Frozen `CatalogProduct` stored at order creation |
| Pipeline | `ExternalOrderProducts` is empty → fallback `OrderProductResolver.LoadProductsAsync` sends `LoadProductsQuery` | `ExternalOrderProducts` has `LoadorderProductSnapshotMiddleware`, fallback runs only for products not present in snapshots |
| Honors GraphQL `IncludeFields` | Yes (dynamic, from `context.SubFields`) | No — always the same captured response groups |
| Consistency after price/catalog changes | Reflects current state | Reflects order-time state |
| Extra storage | None | One row per line item |

## How each mode is wired

### Default Mode
`vc-module-x-order` registers an empty pipeline:

```csharp
// src/VirtoCommerce.XOrder.Data/Extensions/ServiceCollectionExtensions.cs
services.AddPipeline<ExternalOrderProducts>();
```

With no middleware, `ExternalOrderProducts.Products` comes back empty, so `OrderProductResolver` falls back to `LoadProductsAsync`, which sends `LoadProductsQuery` through the XCatalog `SearchProductResponse` pipeline. That pipeline hydrates the `ExpProduct` via dedicated middleware:

- `EnsureCatalogProductLoadedMiddleware`
- `EvalProductsPricesMiddleware`
- `EvalProductsDiscountsMiddleware`
- `EvalProductsTaxMiddleware`
- `EvalProductsInventoryMiddleware`
- `EvalProductsVendorMiddleware`
- `EnsurePropertyMetadataLoadedMiddleware`

Each middleware uses the request's response group / include fields to decide what to load.

### Snapshot Mode
`vc-module-product-snapshot` registers:

```csharp
// src/VirtoCommerce.ProductSnapshot.Web/Module.cs
serviceCollection.AddPipeline<ExternalOrderProducts>(builder =>
{
    builder.AddMiddleware(typeof(LoadorderProductSnapshotMiddleware));
});
```

On every `OrderChangedEvent` (Added / Modified), `CreateOrderProductSnapshotEventHandler` persists a `CatalogProduct` snapshot per line item. The snapshot is loaded with response groups:

```
ItemInfo | ItemAssets | ItemProperties | ItemEditorialReviews
```

At query time `LoadorderProductSnapshotMiddleware` produces an `ExpProduct` via:

```csharp
result.IndexedProduct = catalogProduct;   // nothing else is set
```

Anything not populated here stays at its default (null / empty list / `false`).

## What is populated on `ExpProduct`

`VirtoCommerce.XCatalog.Core.Models.ExpProduct` exposes these members (abridged). The columns show what is populated in each mode, assuming the GraphQL query requests the corresponding field.

### Core identity & catalog data

| `ExpProduct` member | Default Mode | Snapshot Mode | Notes |
| --- | --- | --- | --- |
| `Id` | ✅ | ✅ | From `IndexedProduct.Id` |
| `IndexedProduct` (`CatalogProduct`) | ✅ live | ✅ frozen | Snapshot carries `ItemInfo`, `ItemAssets`, `ItemProperties`, `ItemEditorialReviews` subset |
| `IndexedProduct.Name` / `Code` | ✅ | ✅ | |
| `IndexedProduct.Properties` | ✅ live values | ✅ frozen values | Values, display names, dictionary items frozen at order time |
| `IndexedProduct.Images` / `Assets` | ✅ | ✅ | Frozen URLs — if CDN paths change later, snapshot keeps the old ones |
| `IndexedProduct.Reviews` (descriptions) | ✅ | ✅ | |
| `IndexedProduct.SeoInfos` | ✅ live | ⚠️ only if captured in response group | Not in default snapshot response group — expect empty unless the provider is overridden |
| `IndexedProduct.Associations` | ✅ live | ❌ | `ItemAssociations` not in snapshot response group |
| `IndexedProduct.Variations` | ✅ live | ❌ | `Variations` not in snapshot response group |
| `IndexedProduct.ReferencedAssociations` | ✅ live | ❌ | |
| `IndexedProduct.Outlines` / `Category` | ✅ live | ❌ | `Outlines` / `Categories` not captured |

### Dynamic / contextual data (populated only by XCatalog pipeline middleware)

| `ExpProduct` member | Default Mode | Snapshot Mode | Notes |
| --- | --- | --- | --- |
| `AllPrices` | ✅ via `EvalProductsPricesMiddleware` | ❌ empty | No price middleware runs in snapshot path |
| `MinVariationPrice` | ✅ | ❌ null | Derived from prices which are not loaded |
| `Inventory` / `AllInventories` | ✅ via `EvalProductsInventoryMiddleware` | ❌ null / empty | Fulfillment-center inventories not loaded |
| `IsInStock` / `IsAvailable` / `IsBuyable` | ✅ computed via specs | ⚠️ computed against empty data | Specs will return `false` because prices/inventory are absent |
| `AvailableQuantity` | ✅ | ❌ 0 | Sum over `AllInventories` |
| `Vendor` | ✅ via `EvalProductsVendorMiddleware` | ❌ null | Member lookup skipped |
| `Rating` / `ReviewSummary` | ✅ if review middleware configured | ❌ | Not populated in snapshot path |
| `IsPurchased`, `InWishlist`, `WishlistIds` | ✅ via resolvers | ❌ default (`false` / empty) | |
| `RelevanceScore` | ✅ from search index | ❌ 0 | Bound from index document, not carried in snapshot |
| `IndexedVariationIds`, `IndexedMinVariationPrices` | ✅ from index binders | ❌ empty | Index bindings do not run — snapshot is not a search document |

## GraphQL impact examples

- `order.items { product { name images { url } properties { name value } } }` — **works in both modes.**
- `order.items { product { prices { list { amount } } } }` — **works in Default; empty in Snapshot** (no `AllPrices`).
- `order.items { product { availabilityData { availableQuantity } } }` — **works in Default; zero/empty in Snapshot.**
- `order.items { product { variations { id } } }` — **works in Default; empty in Snapshot** (default snapshot response group excludes variations).
- `order.items { product { vendor { id name } } }` — **works in Default; null in Snapshot.**

## Pros and cons for developers

### Default Mode

**Pros**
- Fully populated `ExpProduct` — any GraphQL field the schema exposes resolves correctly.
- Honors GraphQL `IncludeFields` dynamically, so queries fetch only what they asked for.
- Reflects latest catalog, pricing, inventory, and vendor state.
- No extra storage; nothing to backfill.

**Cons**
- A catalog edit after the order was placed changes what the user sees on the order page — name, price, property values, images can all drift.
- If the product is deleted or unpublished, the line item renders as `null`.
- Read path always hits the search index and pricing / inventory services — more expensive per-request than a simple snapshot fetch.
- Historical reporting and re-rendered emails / invoices are not reproducible.

### Snapshot Mode

**Pros**
- Order page and exports are stable: property values, images, descriptions, SKU are exactly what the customer saw at checkout.
- Survives product deletion or re-categorization; the order remains renderable.
- Cheaper read for the covered fields — one snapshot row, no catalog / pricing / inventory calls.
- Audit and compliance: you can prove what the product looked like on a given date.

**Cons**
- **Dynamic fields are null/empty** in the current middleware: `AllPrices`, `Inventory`, `AllInventories`, `Vendor`, `MinVariationPrice`, `Rating`, `Variations`, `Associations`, `Outlines`, availability flags. If the UI needs them, it must fall back to the live catalog or the middleware must be extended.
- `IncludeFields` from the GraphQL query are ignored on the snapshot path — the response group is fixed at snapshot capture time.
- Snapshots are only captured when the module is installed and `ProductSnapshotEnabled` is on. Legacy orders placed before either are missing — the resolver then falls back to live catalog, which re-introduces drift.
- Every `OrderChangedEvent` (Added / Modified) triggers a capture. Large orders / frequent status updates increase storage and write load.
- Extending the captured data is a schema change: adding a response group means a migration to re-snapshot existing orders, or accepting that old rows stay incomplete.

## Decision guide

Use **Default Mode** when:
- You need current pricing, inventory, vendor, variations, or associations on the order page.
- The catalog is relatively stable, or drift is acceptable.
- You do not need to reproduce historical order appearance.

Use **Snapshot Mode** when:
- Legal / audit / regulatory requirements demand point-in-time product data.
- The UI for completed orders should not change when the catalog changes.
- You can tolerate empty `prices` / `inventory` / `vendor` on order pages, or you extend the middleware to compose snapshot + live data for the fields that must stay current.

## Extending Snapshot Mode (common follow-ups)

If you need extra fields in the snapshot pipeline, the typical hooks are:

1. **Widen the captured response group** — override `VirtoCatalogSnapshotProvider.ProductSnapshotResponseGroup` (e.g. add `ItemAssociations`, `Variations`, `Seo`). Remember: only new orders get the widened snapshot.
2. **Compose middleware** — register additional middleware on the `ExternalOrderProducts` pipeline after `LoadorderProductSnapshotMiddleware` to hydrate dynamic fields (e.g. live prices / inventory) on top of the frozen catalog data.
3. **Override `OrderProductResolver`** — for call sites where live data must win, subclass `OrderProductResolver` and re-route to the catalog fallback even when a snapshot exists.

import * as blades from "./pages";
import * as locales from "./locales";
import { defineAppModule, useExtensionPoint } from "@vc-shell/framework";
import { markRaw } from "vue";
import type { OrdersLineItemToolbarCommand } from "./toolbar";

// "Product snapshot" on the order line item blade's toolbar (legacy: toolbarService on
// customerOrderItemDetailController, enabled when the line has a product): the product as it was when the order was placed
useExtensionPoint("orders-line-item-toolbar").add({
  id: "product-snapshot",
  priority: 3,
  component: markRaw({ render: () => null }),
  meta: {
    title: "PRODUCT_SNAPSHOT.TITLE",
    icon: "lucide-camera",
    permissions: "product-snapshot:read",
    disabled: (ctx) => !ctx.order.id || !ctx.item?.productId,
    onClick: (ctx) =>
      ctx.openBlade({ name: "ProductSnapshotDetails", options: { orderId: ctx.order.id, productId: ctx.item?.productId } }),
  } satisfies OrdersLineItemToolbarCommand,
});

export default defineAppModule({
  blades,
  locales,
});

export * from "./pages";

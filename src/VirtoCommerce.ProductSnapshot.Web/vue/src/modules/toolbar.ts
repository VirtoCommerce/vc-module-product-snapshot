// The Orders line item blade's toolbar extension point "orders-line-item-toolbar" (contract documented in the Orders
// module, vc-module-order .../vue/src/modules/pages/line-item-details.vue). Types repeated here: the contract is by shape.

export interface OrdersLineItemToolbarContext {
  order: { id?: string };
  item?: { id?: string; name?: string; productId?: string };
  openBlade: (event: { name: string; param?: string; options?: Record<string, unknown> }) => Promise<void>;
}

export interface OrdersLineItemToolbarCommand {
  title: string;
  icon?: string;
  permissions?: string | string[];
  isVisible?: (ctx: OrdersLineItemToolbarContext) => boolean;
  disabled?: (ctx: OrdersLineItemToolbarContext) => boolean;
  onClick: (ctx: OrdersLineItemToolbarContext) => unknown;
}

<template>
  <VcBlade
    :title="snapshot?.name ?? $t('PRODUCT_SNAPSHOT.TITLE')"
    :toolbar-items="bladeToolbar"
    :loading="loading"
    width="70%"
  >
    <VcContainer>
      <div
        v-if="!loading && !snapshot"
        class="snap-form"
      >
        <VcBanner
          variant="info"
          icon="lucide-camera-off"
        >
          <template #title>{{ $t("PRODUCT_SNAPSHOT.NOT_FOUND_TITLE") }}</template>
          {{ $t("PRODUCT_SNAPSHOT.NOT_FOUND_MESSAGE") }}
        </VcBanner>
      </div>

      <div
        v-else-if="snapshot"
        class="snap-form tw-space-y-4"
      >
        <div class="snap-grid snap-grid--2">
          <VcField
            :label="$t('PRODUCT_SNAPSHOT.LABELS.CODE')"
            :model-value="snapshot.code"
            copyable
          />
          <VcField
            :label="$t('PRODUCT_SNAPSHOT.LABELS.ID')"
            :model-value="snapshot.id"
            copyable
          />
        </div>

        <!-- legacy meta pills -->
        <div class="snap-pills">
          <VcStatus :variant="snapshot.isActive ? 'success' : 'danger'">
            {{ $t(snapshot.isActive ? "PRODUCT_SNAPSHOT.LABELS.ACTIVE" : "PRODUCT_SNAPSHOT.LABELS.INACTIVE") }}
          </VcStatus>
          <VcStatus
            v-for="pill in pills"
            :key="pill.text"
            :variant="pill.variant"
          >
            {{ pill.text }}
          </VcStatus>
        </div>

        <div class="snap-cards">
          <VcCard
            v-for="group in fieldGroups"
            :key="group.title"
            :header="$t(group.title)"
          >
            <div class="snap-grid snap-grid--2 snap-card-body">
              <VcField
                v-for="f in group.fields"
                :key="f.label"
                :label="$t(f.label)"
                :model-value="show(f.value)"
              />
            </div>
          </VcCard>
        </div>

        <VcCard :header="$t('PRODUCT_SNAPSHOT.GROUPS.LOCALIZED_NAMES')">
          <div class="snap-grid snap-grid--2 snap-card-body">
            <VcField
              v-for="n in localizedNames"
              :key="n.lang"
              :label="n.lang"
              :model-value="n.value"
            />
            <span
              v-if="!localizedNames.length"
              class="snap-muted"
              >{{ $t("PRODUCT_SNAPSHOT.LABELS.NO_LOCALIZED_NAMES") }}</span
            >
          </div>
        </VcCard>

        <VcCard :header="$t('PRODUCT_SNAPSHOT.GROUPS.OTHER_FIELDS')">
          <div class="snap-grid snap-grid--4 snap-card-body">
            <VcField
              v-for="f in otherFields"
              :key="f.label"
              :label="$t(f.label)"
              :model-value="show(f.value)"
            />
          </div>
        </VcCard>

        <VcCard
          v-if="snapshot.properties?.length"
          :header="$t('PRODUCT_SNAPSHOT.GROUPS.PROPERTIES', { count: snapshot.properties.length })"
        >
          <VcDataTable :items="snapshot.properties">
            <VcColumn
              id="name"
              :title="$t('PRODUCT_SNAPSHOT.LABELS.PROP_NAME')"
              mobile-role="title"
              always-visible
            />
            <VcColumn
              id="values"
              :title="$t('PRODUCT_SNAPSHOT.LABELS.PROP_VALUE')"
              mobile-role="field"
            >
              <template #body="{ data: prop }">
                <span class="snap-tags">
                  <span
                    v-for="(v, i) in prop.values ?? []"
                    :key="i"
                    class="snap-tag"
                    :class="{ 'snap-tag--inherited': prop.isInherited }"
                    >{{ v.value }}</span
                  >
                  <span
                    v-if="!prop.values?.length"
                    class="snap-muted"
                    >{{ $t("PRODUCT_SNAPSHOT.LABELS.NOT_SET") }}</span
                  >
                </span>
              </template>
            </VcColumn>
            <VcColumn
              id="type"
              :title="$t('PRODUCT_SNAPSHOT.LABELS.PROP_TYPE_FLAGS')"
              mobile-role="field"
            >
              <template #body="{ data: prop }">
                <span class="snap-tags">
                  <span class="snap-tag">{{ prop.type }}</span>
                  <span class="snap-tag">{{ prop.valueType }}</span>
                  <span
                    v-for="flag in flags(prop)"
                    :key="flag"
                    class="snap-flag"
                    >{{ $t(flag) }}</span
                  >
                </span>
              </template>
            </VcColumn>
          </VcDataTable>
        </VcCard>

        <VcCard
          v-if="snapshot.images?.length"
          :header="$t('PRODUCT_SNAPSHOT.GROUPS.IMAGES', { count: snapshot.images.length })"
        >
          <VcDataTable :items="snapshot.images">
            <VcColumn
              id="url"
              :title="$t('PRODUCT_SNAPSHOT.LABELS.IMAGE')"
              mobile-role="image"
              width="64px"
            >
              <template #body="{ data: img }">
                <VcImage
                  :src="img.url"
                  size="s"
                  aspect="1x1"
                  bordered
                  background="contain"
                />
              </template>
            </VcColumn>
            <VcColumn
              id="sortOrder"
              :title="$t('PRODUCT_SNAPSHOT.LABELS.SORT_ORDER')"
              type="number"
              mobile-role="field"
              width="64px"
            />
            <VcColumn
              id="name"
              :title="$t('PRODUCT_SNAPSHOT.LABELS.IMAGE_NAME')"
              mobile-role="title"
              always-visible
            />
            <VcColumn
              id="id"
              :title="$t('PRODUCT_SNAPSHOT.LABELS.ID')"
              mobile-role="field"
            />
          </VcDataTable>
        </VcCard>

        <VcCard
          v-if="snapshot.reviews?.length"
          :header="$t('PRODUCT_SNAPSHOT.GROUPS.REVIEWS', { count: snapshot.reviews.length })"
        >
          <div class="snap-card-body tw-space-y-4">
            <div
              v-for="review in reviews"
              :key="review.id"
              class="snap-review"
            >
              <div class="snap-pills">
                <VcStatus variant="primary">{{ review.reviewType }}</VcStatus>
                <VcStatus variant="info">{{ review.languageCode }}</VcStatus>
                <VcStatus
                  v-if="!review.isInherited"
                  variant="success"
                  >{{ $t("PRODUCT_SNAPSHOT.LABELS.ACTIVE") }}</VcStatus
                >
              </div>
              <!-- legacy rendered the sanitized HTML; shown here as its text, so no markup reaches the page -->
              <p class="snap-review__text">{{ review.text }}</p>
              <span class="snap-muted">{{ review.id }}</span>
            </div>
          </div>
        </VcCard>
      </div>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
// product-snapshot-details: the product as captured when the order was placed (opened from the order line item blade)
import { formatDateWithPattern, useApiClient, useAsync, useBlade, type IBladeToolbar } from "@vc-shell/framework";
import {
  VcBanner,
  VcBlade,
  VcCard,
  VcColumn,
  VcContainer,
  VcDataTable,
  VcField,
  VcImage,
  VcStatus,
} from "@vc-shell/framework/ui";
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { ProductSnapshotClient, type CatalogProduct, type Property } from "../../api_client/virtocommerce.productsnapshot";

defineBlade({
  url: "/product-snapshot",
  name: "ProductSnapshotDetails",
  permissions: ["product-snapshot:read"],
  // the order and product come only in options, which a reload doesn't bring back
  routable: false,
});

// fields the server returns that the generated model leaves out
type Snapshot = CatalogProduct & { isInherited?: boolean; parentCategoryIsActive?: boolean; seoObjectType?: string };
type Variant = "info" | "success" | "danger" | "primary";

const L = "PRODUCT_SNAPSHOT.LABELS.";
const { t } = useI18n({ useScope: "global" });
// { orderId, productId }: the line item's order and product, from the line item blade's toolbar context
const { options } = useBlade<{ orderId?: string; productId?: string }>();
const { getApiClient } = useApiClient(ProductSnapshotClient);
const snapshot = ref<Snapshot | null>(null);

const { loading, action: load } = useAsync(async () => {
  snapshot.value = null;
  const { orderId, productId } = options.value ?? {};
  if (!orderId || !productId) return;
  // 404 (order gone / not saved yet) and an empty body both mean "no snapshot"
  const found = (await (await getApiClient()).getByOrderAndProductId(orderId, productId).catch(() => null)) as Snapshot | null;
  snapshot.value = found?.id ? found : null;
});
load();

const show = (v: unknown) => (v === null || v === undefined || v === "" ? "—" : String(v));
const date = (v?: Date | string) => (v ? formatDateWithPattern(v, "D MMM YYYY") : undefined);

const pills = computed(() => {
  const s = snapshot.value;
  if (!s) return [];
  const list: { text: string; variant: Variant }[] = [];
  const add = (cond: unknown, text: string, variant: Variant = "info") => cond && list.push({ text, variant });
  add(s.isBuyable, t(L + "BUYABLE"), "success");
  add(s.trackInventory, t(L + "TRACK_INVENTORY"), "primary");
  add(s.productType, s.productType!);
  add(!s.isInherited, t(L + "NOT_INHERITED"));
  add(s.parentCategoryIsActive, t(L + "PARENT_CATEGORY_ACTIVE"));
  add(s.packSize != null, t(L + "PACK", { value: s.packSize }));
  add(s.minQuantity != null, t(L + "MIN_QTY", { value: s.minQuantity }));
  add(s.maxQuantity != null, t(L + "MAX_QTY", { value: s.maxQuantity === 0 ? t(L + "UNLIMITED") : s.maxQuantity }));
  add(s.priority != null, t(L + "PRIORITY", { value: s.priority }));
  return list;
});

const fieldGroups = computed(() => {
  const s = snapshot.value ?? {};
  return [
    {
      title: "PRODUCT_SNAPSHOT.GROUPS.IDENTITY",
      fields: [
        { label: L + "PRODUCT_TYPE", value: s.productType },
        { label: L + "SEO_OBJECT_TYPE", value: (s as Snapshot).seoObjectType },
        { label: L + "CATALOG_ID", value: s.catalogId },
        { label: L + "CATEGORY_ID", value: s.categoryId },
        { label: L + "OUTER_ID", value: s.outerId },
        { label: L + "MAIN_PRODUCT_ID", value: s.mainProductId },
      ],
    },
    {
      title: "PRODUCT_SNAPSHOT.GROUPS.DIMENSIONS",
      fields: [
        { label: L + "WEIGHT", value: s.weight },
        { label: L + "WEIGHT_UNIT", value: s.weightUnit },
        { label: L + "HEIGHT", value: s.height },
        { label: L + "WIDTH", value: s.width },
        { label: L + "LENGTH", value: s.length },
        { label: L + "MEASURE_UNIT", value: s.measureUnit },
      ],
    },
    {
      title: "PRODUCT_SNAPSHOT.GROUPS.DATES",
      fields: [
        { label: L + "START_DATE", value: date(s.startDate) },
        { label: L + "END_DATE", value: date(s.endDate) },
        { label: L + "CREATED_AT", value: date(s.createdDate) },
        { label: L + "MODIFIED_AT", value: date(s.modifiedDate) },
        { label: L + "CREATED_BY", value: s.createdBy },
        { label: L + "MODIFIED_BY", value: s.modifiedBy },
      ],
    },
  ];
});

const otherFields = computed(() => {
  const s = snapshot.value ?? {};
  return [
    { label: L + "VENDOR", value: s.vendor },
    { label: L + "TAX_TYPE", value: s.taxType },
    { label: L + "SHIPPING_TYPE", value: s.shippingType },
    { label: L + "PACKAGE_TYPE", value: s.packageType },
    { label: L + "GTIN", value: s.gtin },
    { label: L + "MANUFACTURER_PART", value: s.manufacturerPartNumber },
    { label: L + "ENABLE_REVIEW", value: s.enableReview },
    { label: L + "HAS_USER_AGREEMENT", value: s.hasUserAgreement },
  ];
});

const localizedNames = computed(() =>
  Object.entries(snapshot.value?.localizedName?.values ?? {}).map(([lang, value]) => ({ lang, value })),
);

const flags = (p: Property) =>
  [
    p.required && "FLAG_REQUIRED",
    p.dictionary && "FLAG_DICTIONARY",
    p.multivalue && "FLAG_MULTIVALUE",
    p.multilanguage && "FLAG_MULTILANG",
    p.isInherited && "FLAG_INHERITED",
  ]
    .filter(Boolean)
    .map((x) => L + x);

// DOMParser builds an inert document (no scripts run); only the text is shown
function plainText(html?: string) {
  if (!html) return "";
  const body = new DOMParser().parseFromString(html, "text/html").body;
  body.querySelectorAll("script, style").forEach((x) => x.remove());
  return (body.textContent ?? "").replace(/\s+/g, " ").trim();
}
// parsed once per snapshot, not on every render
const reviews = computed(() => (snapshot.value?.reviews ?? []).map((r) => ({ ...r, text: plainText(r.content) })));

const bladeToolbar = ref<IBladeToolbar[]>([
  { id: "refresh", title: t("PRODUCT_SNAPSHOT.REFRESH"), icon: "lucide-refresh-cw", clickHandler: () => load() },
]);
</script>

<style lang="scss">
/* the design's blade body padding, as in the Orders details */
.snap-form {
  padding: 20px 22px 36px;
}

.snap-pills {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.snap-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 16px;
}

.snap-card-body {
  padding: 16px;
}

.snap-grid {
  display: grid;
  gap: 12px 16px;
  &--2 {
    grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  }
  &--4 {
    grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
  }
}

.snap-muted {
  font-size: 12px;
  color: var(--color-neutral-500);
}

.snap-tags {
  display: inline-flex;
  flex-wrap: wrap;
  gap: 4px;
}

.snap-tag,
.snap-flag {
  padding: 1px 8px;
  border-radius: 10px;
  font-size: 12px;
  line-height: 18px;
  border: 1px solid var(--color-neutral-200);
  color: var(--color-neutral-800);
  background: var(--color-neutral-50);
  &--inherited {
    font-style: italic;
    color: var(--color-neutral-500);
  }
}

.snap-flag {
  border-style: dashed;
  color: var(--color-neutral-600);
}

.snap-review {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding-bottom: 16px;
  border-bottom: 1px solid var(--color-neutral-200);
  &:last-child {
    padding-bottom: 0;
    border-bottom: 0;
  }
  &__text {
    margin: 0;
    font-size: 14px;
    color: var(--color-neutral-800);
    display: -webkit-box;
    -webkit-line-clamp: 4;
    -webkit-box-orient: vertical;
    overflow: hidden;
  }
}
</style>

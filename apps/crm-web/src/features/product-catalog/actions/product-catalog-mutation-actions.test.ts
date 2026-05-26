import { beforeEach, describe, expect, it, vi } from "vitest";

const mocks = vi.hoisted(() => ({
  createProductCatalogItem: vi.fn(),
  updateProductCatalogItem: vi.fn(),
  setProductCatalogItemActiveState: vi.fn(),
  createProductCatalogCategory: vi.fn(),
  updateProductCatalogCategory: vi.fn(),
  setProductCatalogCategoryActiveState: vi.fn(),
  requireCrmActionCapability: vi.fn(async () => ({})),
}));

vi.mock("next/cache", () => ({ revalidatePath: vi.fn() }));
vi.mock("next/navigation", () => ({ redirect: vi.fn() }));
vi.mock("@/lib/security/csrf", () => ({ assertSameOriginRequest: vi.fn(async () => {}) }));
vi.mock("@/lib/i18n/request-locale", () => ({ getRequestLocale: vi.fn(async () => "en") }));
vi.mock("@/lib/i18n/crm-i18n", () => ({ tCrm: vi.fn((key: string) => key) }));
vi.mock("@/lib/crm-auth/crm-api-request-options", () => ({
  getCrmApiRequestOptions: vi.fn(async () => ({})),
}));
vi.mock("@/lib/crm-auth/require-crm-action-capability", () => ({
  requireCrmActionCapability: mocks.requireCrmActionCapability,
}));
vi.mock("@/lib/crm-api", () => ({
  crmApiClient: {
    createProductCatalogItem: mocks.createProductCatalogItem,
    updateProductCatalogItem: mocks.updateProductCatalogItem,
    setProductCatalogItemActiveState: mocks.setProductCatalogItemActiveState,
    createProductCatalogCategory: mocks.createProductCatalogCategory,
    updateProductCatalogCategory: mocks.updateProductCatalogCategory,
    setProductCatalogCategoryActiveState: mocks.setProductCatalogCategoryActiveState,
    deleteProductCatalogItem: vi.fn(),
    deleteProductCatalogCategory: vi.fn(),
  },
}));

import {
  createProductCatalogCategoryAction,
  createProductCatalogItemAction,
  updateProductCatalogCategoryAction,
  updateProductCatalogItemAction,
} from "./product-catalog-mutation-actions";

describe("product catalog mutation actions", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("sends persisted pricing/category/currency fields on create", async () => {
    mocks.createProductCatalogItem.mockResolvedValueOnce({ id: "p-1" });
    mocks.setProductCatalogItemActiveState.mockResolvedValueOnce({});

    await createProductCatalogItemAction({
      code: "SKU-1",
      name: "Premium Package",
      description: "desc",
      isActive: false,
      categoryId: "11111111-1111-4111-8111-111111111111",
      unitPrice: 245.5,
      currencyCode: "eur",
      defaultDiscountRate: 12.5,
      defaultTaxRate: 20,
    });

    expect(mocks.createProductCatalogItem).toHaveBeenCalledWith(
      {
        code: "SKU-1",
        name: "Premium Package",
        description: "desc",
        categoryId: "11111111-1111-4111-8111-111111111111",
        unitPrice: 245.5,
        currencyCode: "EUR",
        defaultDiscountRate: 12.5,
        defaultTaxRate: 20,
      },
      {},
    );
  });

  it("sends persisted pricing/category/currency fields on update", async () => {
    mocks.updateProductCatalogItem.mockResolvedValueOnce({});
    mocks.setProductCatalogItemActiveState.mockResolvedValueOnce({});

    await updateProductCatalogItemAction("22222222-2222-4222-8222-222222222222", {
      code: "SKU-2",
      name: "Basic Package",
      description: "desc",
      isActive: true,
      categoryId: "33333333-3333-4333-8333-333333333333",
      unitPrice: 99,
      currencyCode: "usd",
      defaultDiscountRate: 5,
      defaultTaxRate: 8,
    });

    expect(mocks.updateProductCatalogItem).toHaveBeenCalledWith(
      "22222222-2222-4222-8222-222222222222",
      {
        code: "SKU-2",
        name: "Basic Package",
        description: "desc",
        categoryId: "33333333-3333-4333-8333-333333333333",
        unitPrice: 99,
        currencyCode: "USD",
        defaultDiscountRate: 5,
        defaultTaxRate: 8,
      },
      {},
    );
  });

  it("creates categories without leaking product-only fields", async () => {
    mocks.createProductCatalogCategory.mockResolvedValueOnce({ id: "cat-1" });
    mocks.setProductCatalogCategoryActiveState.mockResolvedValueOnce({});

    await createProductCatalogCategoryAction({
      code: "HW",
      name: "Hardware",
      description: "Physical goods",
      isActive: false,
    });

    expect(mocks.createProductCatalogCategory).toHaveBeenCalledWith(
      {
        code: "HW",
        name: "Hardware",
        description: "Physical goods",
      },
      {},
    );
    expect(mocks.setProductCatalogCategoryActiveState).toHaveBeenCalledWith(
      "cat-1",
      { isActive: false },
      {},
    );
  });

  it("updates categories and active state", async () => {
    mocks.updateProductCatalogCategory.mockResolvedValueOnce({});
    mocks.setProductCatalogCategoryActiveState.mockResolvedValueOnce({});

    await updateProductCatalogCategoryAction("44444444-4444-4444-8444-444444444444", {
      code: "SW",
      name: "Software",
      description: "",
      isActive: true,
    });

    expect(mocks.updateProductCatalogCategory).toHaveBeenCalledWith(
      "44444444-4444-4444-8444-444444444444",
      {
        code: "SW",
        name: "Software",
        description: null,
      },
      {},
    );
    expect(mocks.setProductCatalogCategoryActiveState).toHaveBeenCalledWith(
      "44444444-4444-4444-8444-444444444444",
      { isActive: true },
      {},
    );
  });
});

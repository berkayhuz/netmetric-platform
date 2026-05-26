import { describe, expect, it } from "vitest";

import { productCatalogCategoryFormSchema } from "./product-catalog-category-form-schema";
import { productCatalogFormSchema } from "./product-catalog-form-schema";

describe("product catalog form schema", () => {
  it("accepts valid payload with pricing and currency", () => {
    const parsed = productCatalogFormSchema.safeParse({
      code: "SKU-100",
      name: "Support Plan",
      description: "Annual support",
      isActive: true,
      categoryId: undefined,
      unitPrice: 199.9,
      currencyCode: "USD",
      defaultDiscountRate: 10,
      defaultTaxRate: 20,
    });

    expect(parsed.success).toBe(true);
  });

  it("rejects negative price and invalid currency code", () => {
    const parsed = productCatalogFormSchema.safeParse({
      code: "SKU-100",
      name: "Support Plan",
      isActive: true,
      categoryId: "",
      unitPrice: -1,
      currencyCode: "US",
      defaultDiscountRate: -5,
      defaultTaxRate: 101,
    });

    expect(parsed.success).toBe(false);
  });
});

describe("product catalog category form schema", () => {
  it("accepts category payloads without product pricing fields", () => {
    const parsed = productCatalogCategoryFormSchema.safeParse({
      code: "HW",
      name: "Hardware",
      description: "Physical products",
      isActive: true,
    });

    expect(parsed.success).toBe(true);
  });
});

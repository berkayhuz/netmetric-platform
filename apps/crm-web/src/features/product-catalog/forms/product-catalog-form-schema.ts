import { z } from "zod";

import { optionalGuid, optionalLongText } from "@/features/shared/forms/schema-primitives";

export const productCatalogFormSchema = z.object({
  code: z.string().trim().min(1).max(64),
  name: z.string().trim().min(1).max(200),
  description: optionalLongText,
  isActive: z.boolean(),
  categoryId: optionalGuid,
  unitPrice: z.coerce.number().nonnegative().optional(),
  currencyCode: z.string().trim().length(3),
  defaultDiscountRate: z.coerce.number().min(0).max(100),
  defaultTaxRate: z.coerce.number().min(0).max(100),
});

export type ProductCatalogFormInput = z.input<typeof productCatalogFormSchema>;
export type ProductCatalogFormValues = z.output<typeof productCatalogFormSchema>;

import { z } from "zod";

import { optionalLongText } from "@/features/shared/forms/schema-primitives";

export const productCatalogCategoryFormSchema = z.object({
  code: z.string().trim().min(1).max(64),
  name: z.string().trim().min(1).max(200),
  description: optionalLongText,
  isActive: z.boolean(),
});

export type ProductCatalogCategoryFormInput = z.input<typeof productCatalogCategoryFormSchema>;
export type ProductCatalogCategoryFormValues = z.output<typeof productCatalogCategoryFormSchema>;

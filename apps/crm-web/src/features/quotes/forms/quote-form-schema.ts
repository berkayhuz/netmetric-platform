import { z } from "zod";

import {
  optionalDate,
  optionalGuid,
  optionalLongText,
  optionalText,
} from "@/features/shared/forms/schema-primitives";

const decimalRegex = /^\d+(\.\d+)?$/;

export const quoteLineSchema = z.object({
  productId: z.string().trim().uuid("Product ID must be a valid GUID."),
  description: optionalText,
  quantity: z.coerce.number().int().gt(0),
  unitPrice: z.string().trim().regex(decimalRegex, "Unit price must be a valid positive number."),
  discountRate: z.coerce.number().min(0).max(100),
  taxRate: z.coerce.number().min(0).max(100),
});

export const quoteFormSchema = z.object({
  quoteNumber: z.string().trim().min(1).max(64),
  proposalTitle: optionalText,
  proposalSummary: optionalLongText,
  proposalBody: optionalLongText,
  quoteDate: z
    .string()
    .trim()
    .regex(/^\d{4}-\d{2}-\d{2}$/, "Quote date is required."),
  validUntil: optionalDate,
  opportunityId: optionalGuid,
  customerId: optionalGuid,
  ownerUserId: optionalGuid,
  currencyCode: z.string().trim().length(3),
  exchangeRate: z
    .string()
    .trim()
    .regex(decimalRegex, "Exchange rate must be a valid positive number."),
  termsAndConditions: optionalLongText,
  proposalTemplateId: optionalGuid,
  items: z.array(quoteLineSchema).min(1, "At least one quote line item is required."),
  rowVersion: z.string().trim().optional().or(z.literal("")),
});

export type QuoteFormInput = z.input<typeof quoteFormSchema>;
export type QuoteFormValues = z.output<typeof quoteFormSchema>;

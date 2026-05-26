import { z } from "zod";

import {
  optionalDate,
  optionalEmail,
  optionalGuid,
  optionalLongText,
  optionalText,
} from "@/features/shared/forms/schema-primitives";

export const leadFormSchema = z.object({
  fullName: z.string().trim().min(1).max(200),
  companyName: optionalText,
  email: optionalEmail,
  phone: optionalText,
  jobTitle: optionalText,
  description: optionalLongText,
  estimatedBudget: z
    .union([
      z.number(),
      z.nan(),
      z
        .string()
        .trim()
        .regex(/^\d+(\.\d+)?$/),
      z.literal(""),
    ])
    .optional()
    .transform((value) => {
      if (typeof value === "number") {
        return Number.isNaN(value) ? "" : value.toString();
      }

      if (typeof value === "string") {
        return value;
      }

      return "";
    }),
  nextContactDate: optionalDate,
  source: z.coerce.number().int(),
  status: z.coerce.number().int(),
  priority: z.coerce.number().int(),
  companyId: optionalGuid,
  ownerUserId: optionalGuid,
  notes: optionalLongText,
  rowVersion: z.string().trim().optional().or(z.literal("")),
});

export type LeadFormInput = z.input<typeof leadFormSchema>;
export type LeadFormValues = z.output<typeof leadFormSchema>;

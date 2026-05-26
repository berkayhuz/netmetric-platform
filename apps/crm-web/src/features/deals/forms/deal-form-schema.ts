import { z } from "zod";

import { optionalGuid, optionalLongText } from "@/features/shared/forms/schema-primitives";

const decimalRegex = /^\d+(\.\d+)?$/;

export const dealFormSchema = z.object({
  dealCode: z.string().trim().min(1).max(100),
  name: z.string().trim().min(1).max(200),
  totalAmount: z
    .string()
    .trim()
    .regex(decimalRegex, "Total amount must be a valid positive number."),
  closedDate: z
    .string()
    .trim()
    .regex(/^\d{4}-\d{2}-\d{2}$/, "Closed date is required."),
  opportunityId: optionalGuid,
  companyId: optionalGuid,
  ownerUserId: optionalGuid,
  notes: optionalLongText,
  rowVersion: z.string().trim().optional().or(z.literal("")),
});

export type DealFormInput = z.input<typeof dealFormSchema>;
export type DealFormValues = z.output<typeof dealFormSchema>;

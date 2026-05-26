import { z } from "zod";

import {
  optionalDate,
  optionalGuid,
  optionalLongText,
} from "@/features/shared/forms/schema-primitives";

const decimalRegex = /^\d+(\.\d+)?$/;

export const opportunityFormSchema = z.object({
  opportunityCode: z.string().trim().min(1).max(100),
  name: z.string().trim().min(1).max(200),
  description: optionalLongText,
  estimatedAmount: z
    .string()
    .trim()
    .regex(decimalRegex, "Estimated amount must be a valid positive number."),
  expectedRevenue: z
    .string()
    .trim()
    .regex(decimalRegex, "Expected revenue must be a valid positive number.")
    .optional()
    .or(z.literal("")),
  probability: z.coerce.number().min(0).max(100),
  estimatedCloseDate: optionalDate,
  stage: z.coerce.number().int(),
  status: z.coerce.number().int(),
  priority: z.coerce.number().int(),
  leadId: optionalGuid,
  customerId: optionalGuid,
  ownerUserId: optionalGuid,
  notes: optionalLongText,
  rowVersion: z.string().trim().optional().or(z.literal("")),
});

export type OpportunityFormInput = z.input<typeof opportunityFormSchema>;
export type OpportunityFormValues = z.output<typeof opportunityFormSchema>;

import { z } from "zod";

import { optionalGuid } from "@/features/shared/forms/schema-primitives";

export const ticketSlaPolicyFormSchema = z.object({
  policyId: optionalGuid,
  name: z.string().trim().min(1).max(200),
  ticketCategoryId: optionalGuid,
  priority: z.coerce.number().int(),
  firstResponseTargetMinutes: z.coerce.number().int(),
  resolutionTargetMinutes: z.coerce.number().int(),
  isDefault: z.coerce.boolean(),
});

export type TicketSlaPolicyFormInput = z.input<typeof ticketSlaPolicyFormSchema>;

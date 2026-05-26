import { z } from "zod";

import { optionalGuid, optionalLongText } from "@/features/shared/forms/schema-primitives";

export const dealLifecycleActionSchema = z.object({
  occurredAt: z.string().trim().optional().or(z.literal("")),
  lostReasonId: optionalGuid,
  note: optionalLongText,
  rowVersion: z.string().trim().optional().or(z.literal("")),
});

export type DealLifecycleActionFormInput = z.input<typeof dealLifecycleActionSchema>;
export type DealLifecycleActionFormValues = z.output<typeof dealLifecycleActionSchema>;

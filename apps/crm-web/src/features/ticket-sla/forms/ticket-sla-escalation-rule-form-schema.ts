import { z } from "zod";

import { optionalGuid } from "@/features/shared/forms/schema-primitives";

export const ticketSlaEscalationRuleFormSchema = z.object({
  ruleId: optionalGuid,
  slaPolicyId: z.string().trim().uuid(),
  metricType: z.string().trim().min(1).max(100),
  triggerBeforeOrAfterMinutes: z.coerce.number().int(),
  actionType: z.string().trim().min(1).max(100),
  targetTeamId: optionalGuid,
  targetUserId: optionalGuid,
  isEnabled: z.coerce.boolean(),
});

export type TicketSlaEscalationRuleFormInput = z.input<typeof ticketSlaEscalationRuleFormSchema>;

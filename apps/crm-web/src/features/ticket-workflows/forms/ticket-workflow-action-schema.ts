import { z } from "zod";

import { optionalGuid, optionalLongText } from "@/features/shared/forms/schema-primitives";

export const ticketWorkflowQueueFormSchema = z.object({
  queueId: optionalGuid,
  code: z.string().trim().min(1).max(64),
  name: z.string().trim().min(1).max(128),
  description: optionalLongText,
  assignmentStrategy: z.coerce.number().int().min(1).max(3),
  isDefault: z.coerce.boolean(),
});

export const ticketWorkflowQueueUpdateFormSchema = ticketWorkflowQueueFormSchema.omit({
  code: true,
});

export const ticketWorkflowQueueAssignFormSchema = z.object({
  ticketId: z.string().trim().uuid(),
  previousQueueId: optionalGuid,
  newQueueId: z.string().trim().uuid(),
  reason: optionalLongText,
});

export const ticketWorkflowOwnerAssignFormSchema = z.object({
  ticketId: z.string().trim().uuid(),
  previousOwnerUserId: optionalGuid,
  newOwnerUserId: z.string().trim().uuid(),
  queueId: optionalGuid,
  reason: optionalLongText,
});

export const ticketWorkflowStatusChangeFormSchema = z.object({
  ticketId: z.string().trim().uuid(),
  previousStatus: z.string().trim().min(1).max(64),
  newStatus: z.string().trim().min(1).max(64),
  note: optionalLongText,
});

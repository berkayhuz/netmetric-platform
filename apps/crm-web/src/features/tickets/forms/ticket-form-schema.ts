import { z } from "zod";

import {
  optionalGuid,
  optionalLongText,
  optionalText,
} from "@/features/shared/forms/schema-primitives";

const optionalDateTime = z
  .string()
  .trim()
  .regex(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/)
  .optional()
  .or(z.literal(""));

export const ticketFormSchema = z
  .object({
    subject: z.string().trim().min(1).max(200),
    description: optionalLongText,
    ticketType: z.coerce.number().int(),
    channel: z.coerce.number().int(),
    priority: z.coerce.number().int(),
    assignedUserId: optionalGuid,
    customerId: optionalGuid,
    contactId: optionalGuid,
    ticketCategoryId: optionalGuid,
    slaPolicyId: optionalGuid,
    firstResponseDueAt: optionalDateTime,
    resolveDueAt: optionalDateTime,
    notes: optionalLongText,
    rowVersion: optionalText,
  })
  .superRefine((value, ctx) => {
    if (!value.firstResponseDueAt || !value.resolveDueAt) {
      return;
    }

    const first = new Date(value.firstResponseDueAt);
    const resolve = new Date(value.resolveDueAt);
    if (Number.isNaN(first.valueOf()) || Number.isNaN(resolve.valueOf())) {
      return;
    }

    if (resolve < first) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        path: ["resolveDueAt"],
        message: "Resolve due date must be equal to or after first response due date.",
      });
    }
  });

export type TicketFormInput = z.input<typeof ticketFormSchema>;
export type TicketFormValues = z.output<typeof ticketFormSchema>;

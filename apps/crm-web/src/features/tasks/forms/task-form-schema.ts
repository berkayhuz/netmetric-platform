import { z } from "zod";

import {
  optionalEmail,
  optionalGuid,
  optionalLongText,
  optionalText,
} from "@/features/shared/forms/schema-primitives";

const localDateTime = z
  .string()
  .trim()
  .regex(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/, "Provide a valid date-time value.");

export const taskFormSchema = z.object({
  title: z.string().trim().min(1).max(200),
  description: optionalLongText,
  ownerUserId: optionalGuid,
  dueAtUtc: localDateTime,
  priority: z.coerce.number().int().min(1).max(5),
});

export type TaskFormInput = z.input<typeof taskFormSchema>;
export type TaskFormValues = z.output<typeof taskFormSchema>;

export const meetingFormSchema = z
  .object({
    title: z.string().trim().min(1).max(200),
    startsAtUtc: localDateTime,
    endsAtUtc: localDateTime,
    organizerEmail: optionalEmail,
    attendeeSummary: optionalText,
    requiresExternalSync: z.coerce.boolean(),
  })
  .refine((value) => Date.parse(value.endsAtUtc) > Date.parse(value.startsAtUtc), {
    message: "End time must be later than start time.",
    path: ["endsAtUtc"],
  });

export type MeetingFormInput = z.input<typeof meetingFormSchema>;
export type MeetingFormValues = z.output<typeof meetingFormSchema>;

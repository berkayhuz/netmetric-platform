import { z } from "zod";

import { optionalDate, optionalLongText } from "@/features/shared/forms/schema-primitives";

export const quoteLifecycleNoteSchema = z.object({
  note: optionalLongText,
  rowVersion: z.string().trim().optional().or(z.literal("")),
});

export const quoteLifecycleReasonSchema = z.object({
  reason: z.string().trim().min(1).max(1000),
  rowVersion: z.string().trim().optional().or(z.literal("")),
});

export const quoteLifecycleDateNoteSchema = z.object({
  at: optionalDate,
  note: optionalLongText,
  rowVersion: z.string().trim().optional().or(z.literal("")),
});

export const quoteCreateRevisionSchema = z.object({
  newQuoteNumber: z.string().trim().min(1).max(64),
});

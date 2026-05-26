import { z } from "zod";

export const optionalText = z.string().trim().max(500).optional().or(z.literal(""));
export const optionalLongText = z.string().trim().max(4000).optional().or(z.literal(""));

export const optionalGuid = z
  .string()
  .trim()
  .regex(
    /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}$/,
  )
  .optional()
  .or(z.literal(""));

export const optionalEmail = z.email().optional().or(z.literal(""));

export const optionalUrl = z.url().optional().or(z.literal(""));

export const optionalDate = z
  .string()
  .regex(/^\d{4}-\d{2}-\d{2}$/)
  .optional()
  .or(z.literal(""));

export function emptyToNull(value?: string): string | null {
  if (!value) {
    return null;
  }

  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : null;
}

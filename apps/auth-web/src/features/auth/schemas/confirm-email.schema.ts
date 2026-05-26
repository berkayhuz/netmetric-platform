import { z } from "zod";

import type { ValidationText } from "./validation-text";

export function createConfirmEmailSchema(v: ValidationText) {
  return z.object({
    tenantId: z.string().trim().min(1, v.workspaceRequired),
    userId: z.string().trim().min(1, v.userIdRequired),
    token: z.string().trim().min(1, v.confirmTokenRequired),
  });
}

export type ConfirmEmailInput = z.infer<ReturnType<typeof createConfirmEmailSchema>>;

export function createResendConfirmEmailSchema(v: ValidationText) {
  return z.object({
    tenantId: z.string().trim().min(1, v.workspaceRequired),
    email: z.string().trim().min(1, v.emailRequired).email(v.emailInvalid),
  });
}

export type ResendConfirmEmailInput = z.infer<ReturnType<typeof createResendConfirmEmailSchema>>;

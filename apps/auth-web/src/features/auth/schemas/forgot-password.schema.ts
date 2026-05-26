import { z } from "zod";

import type { ValidationText } from "./validation-text";

export function createForgotPasswordSchema(v: ValidationText) {
  return z.object({
    email: z.string().trim().min(1, v.emailRequired).email(v.emailInvalid),
  });
}

export type ForgotPasswordInput = z.infer<ReturnType<typeof createForgotPasswordSchema>>;

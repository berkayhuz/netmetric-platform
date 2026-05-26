import { z } from "zod";

import type { ValidationText } from "./validation-text";

export function createRegisterSchema(v: ValidationText) {
  const nameSchema = z.string().trim().min(2, v.fullNameMin).max(120, v.fullNameMax);
  const workspaceNameSchema = z.preprocess((value) => {
    if (typeof value !== "string") {
      return value;
    }

    return value.trim().length === 0 ? undefined : value;
  }, z.string().trim().min(2, v.workspaceNameMin).max(120, v.workspaceNameMax).optional());

  return z
    .object({
      fullName: nameSchema,
      email: z.string().trim().min(1, v.emailRequired).email(v.emailInvalid),
      password: z.string().min(12, v.passwordMin).max(128, v.passwordMax),
      confirmPassword: z.string().min(1, v.confirmPasswordRequired),
      workspaceName: workspaceNameSchema,
      acceptTerms: z.coerce.boolean().refine((value) => value, { message: v.acceptTermsRequired }),
      returnUrl: z.string().trim().optional(),
    })
    .refine((value) => value.password === value.confirmPassword, {
      path: ["confirmPassword"],
      message: v.passwordMatch,
    });
}

export type RegisterInput = z.infer<ReturnType<typeof createRegisterSchema>>;

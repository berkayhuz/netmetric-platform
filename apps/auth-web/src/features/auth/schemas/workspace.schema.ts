import { z } from "zod";

import type { ValidationText } from "./validation-text";

export function createSwitchWorkspaceSchema(v: ValidationText) {
  return z.object({
    tenantId: z.string().trim().min(1, v.workspaceRequired),
    returnUrl: z.string().trim().optional(),
  });
}

export type SwitchWorkspaceInput = z.infer<ReturnType<typeof createSwitchWorkspaceSchema>>;

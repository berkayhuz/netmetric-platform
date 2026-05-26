import { z } from "zod";

import { optionalGuid } from "@/features/shared/forms/schema-primitives";

export const dealOwnerFormSchema = z.object({
  ownerUserId: optionalGuid,
});

export type DealOwnerFormInput = z.input<typeof dealOwnerFormSchema>;
export type DealOwnerFormValues = z.output<typeof dealOwnerFormSchema>;

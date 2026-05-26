import { z } from "zod";

import { optionalText } from "@/features/shared/forms/schema-primitives";

export const addressFormSchema = z.object({
  id: z.string().trim().optional().or(z.literal("")),
  addressType: z.coerce.number().int().min(0).max(4),
  line1: z.string().trim().min(1).max(300),
  line2: optionalText,
  district: optionalText,
  city: optionalText,
  state: optionalText,
  country: optionalText,
  zipCode: optionalText,
  isDefault: z.coerce.boolean(),
  rowVersion: z.string().trim().optional().or(z.literal("")),
});

export type AddressFormInput = z.input<typeof addressFormSchema>;
export type AddressFormValues = z.output<typeof addressFormSchema>;

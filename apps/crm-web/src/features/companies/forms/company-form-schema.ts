import { z } from "zod";

import {
  optionalEmail,
  optionalGuid,
  optionalLongText,
  optionalText,
  optionalUrl,
} from "@/features/shared/forms/schema-primitives";

export const companyFormSchema = z.object({
  name: z.string().trim().min(1).max(200),
  taxNumber: optionalText,
  taxOffice: optionalText,
  website: optionalUrl,
  email: optionalEmail,
  phone: optionalText,
  sector: optionalText,
  employeeCountRange: optionalText,
  annualRevenue: z.coerce.number().finite().nonnegative().optional(),
  description: optionalLongText,
  notes: optionalLongText,
  companyType: z.coerce.number().int().min(0).max(4),
  ownerUserId: optionalGuid,
  parentCompanyId: optionalGuid,
  rowVersion: z.string().trim().optional().or(z.literal("")),
});

export type CompanyFormInput = z.input<typeof companyFormSchema>;
export type CompanyFormValues = z.output<typeof companyFormSchema>;

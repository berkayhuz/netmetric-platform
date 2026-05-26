"use client";

import Image from "next/image";
import { useRouter } from "next/navigation";
import { useEffect, useRef, useState, useTransition } from "react";
import { zodResolver } from "@hookform/resolvers/zod";

import {
  Field,
  FieldContent,
  FieldError,
  FieldLabel,
  FormGrid,
  Input,
  SubmitBar,
  TextTitle,
  TextareaField,
  cn,
} from "@netmetric/ui";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@netmetric/ui/client";
import { useForm, useWatch } from "react-hook-form";

import { CrmImageUploader } from "@/components/media/crm-image-uploader";

import { CrmFormFeedback } from "@/components/forms/crm-form-feedback";
import { initialCrmMutationState } from "@/features/shared/actions/mutation-state";
import type { CrmReferenceOption } from "@/features/shared/data/form-reference-data";
import { companyTypeOptions } from "@/features/shared/forms/options";
import { getSelectDisplayLabel } from "@/features/shared/forms/select-display";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

import {
  createCompanyAction,
  updateCompanyAction,
  uploadCompanyLogoAction,
} from "../actions/company-mutation-actions";
import { companyFormSchema, type CompanyFormInput } from "./company-form-schema";

type CompanyFormProps = {
  mode: "create" | "edit";
  companyId?: string;
  initialValues?: Partial<CompanyFormInput>;
  companyOptions?: CrmReferenceOption[];
  ownerUserOptions?: CrmReferenceOption[];
};

const defaults: CompanyFormInput = {
  name: "",
  taxNumber: "",
  taxOffice: "",
  website: "",
  email: "",
  phone: "",
  sector: "",
  employeeCountRange: "",
  annualRevenue: undefined,
  description: "",
  notes: "",
  companyType: 0,
  ownerUserId: "",
  parentCompanyId: "",
  rowVersion: "",
};

function isGuidClient(value: string): boolean {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(value);
}

export function CompanyForm({
  mode,
  companyId,
  initialValues,
  companyOptions = [],
  ownerUserOptions = [],
}: Readonly<CompanyFormProps>) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [result, setResult] = useState(initialCrmMutationState);
  const [logoFile, setLogoFile] = useState<File | null>(null);
  const [logoPreviewUrl, setLogoPreviewUrl] = useState<string | null>(null);
  const form = useForm<CompanyFormInput>({
    resolver: zodResolver(companyFormSchema),
    defaultValues: { ...defaults, ...initialValues },
  });
  const companyType = useWatch({ control: form.control, name: "companyType" });

  const onSubmit = (values: CompanyFormInput) => {
    setResult(initialCrmMutationState);
    startTransition(async () => {
      const response =
        mode === "create"
          ? await createCompanyAction(values)
          : await updateCompanyAction(companyId ?? "", values);

      if (mode === "create" && response.status === "success" && logoFile && response.redirectTo) {
        const createdId = response.redirectTo.split("/").at(-1);
        if (createdId && isGuidClient(createdId)) {
          const uploadPayload = new FormData();
          uploadPayload.set("file", logoFile, logoFile.name);
          await uploadCompanyLogoAction(createdId, initialCrmMutationState, uploadPayload);
        }
      }

      setResult(response);
      if (response.fieldErrors) {
        for (const [field, errors] of Object.entries(response.fieldErrors)) {
          const first = errors[0];
          if (first) form.setError(field as keyof CompanyFormInput, { message: first });
        }
      }
      if (response.status === "success" && response.redirectTo) {
        router.push(response.redirectTo);
        router.refresh();
      }
    });
  };

  const companyTypeLabelByValue: Record<number, string> = {
    0: tCrmClient("crm.companies.options.companyType.prospect"),
    1: tCrmClient("crm.companies.options.companyType.customer"),
    2: tCrmClient("crm.companies.options.companyType.partner"),
    3: tCrmClient("crm.companies.options.companyType.vendor"),
    4: tCrmClient("crm.companies.options.companyType.competitor"),
  };
  const companyTypeDisplayOptions = companyTypeOptions.map((option) => ({
    value: String(option.value),
    label: companyTypeLabelByValue[option.value] ?? option.label,
  }));
  const ownerDisplayOptions = [{ value: "__none__", label: "-" }, ...ownerUserOptions];
  const parentCompanyDisplayOptions = [{ value: "__none__", label: "-" }, ...companyOptions];

  useEffect(() => {
    if (!logoFile) {
      setLogoPreviewUrl(null);
      return;
    }

    const objectUrl = URL.createObjectURL(logoFile);
    setLogoPreviewUrl(objectUrl);

    return () => {
      URL.revokeObjectURL(objectUrl);
    };
  }, [logoFile]);

  return (
    <form className="space-y-6" noValidate onSubmit={form.handleSubmit(onSubmit)}>
      <CrmFormFeedback state={result} />
      {mode === "create" ? (
        <div className="pb-6 border-b border-border/30">
          <CrmImageUploader
            altText="Preview"
            description={tCrmClient("crm.companies.pages.detail.logoDescription")}
            imageUrl={logoPreviewUrl}
            title={tCrmClient("crm.companies.pages.detail.logoTitle")}
            onChange={(file) => setLogoFile(file)}
          />
        </div>
      ) : null}

      <FormGrid columns={2}>
        <Field>
          <FieldLabel htmlFor="company-name">{tCrmClient("crm.companies.fields.name")}</FieldLabel>
          <FieldContent>
            <Input id="company-name" {...form.register("name")} />
            <FieldError>{form.formState.errors.name?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="company-type">
            {tCrmClient("crm.companies.fields.companyType")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={String(companyType)}
              onValueChange={(value) =>
                form.setValue("companyType", Number(value), {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="company-type">
                <SelectValue>
                  {getSelectDisplayLabel(String(companyType), companyTypeDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {companyTypeOptions.map((o) => (
                  <SelectItem key={`company-type-${o.value}`} value={String(o.value)}>
                    {companyTypeLabelByValue[o.value] ?? o.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.companyType?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="company-email">
            {tCrmClient("crm.companies.fields.email")}
          </FieldLabel>
          <FieldContent>
            <Input id="company-email" type="email" {...form.register("email")} />
            <FieldError>{form.formState.errors.email?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="company-phone">
            {tCrmClient("crm.companies.fields.phone")}
          </FieldLabel>
          <FieldContent>
            <Input id="company-phone" {...form.register("phone")} />
            <FieldError>{form.formState.errors.phone?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="company-website">
            {tCrmClient("crm.companies.fields.website")}
          </FieldLabel>
          <FieldContent>
            <Input id="company-website" type="url" {...form.register("website")} />
            <FieldError>{form.formState.errors.website?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="company-sector">
            {tCrmClient("crm.companies.fields.sector")}
          </FieldLabel>
          <FieldContent>
            <Input id="company-sector" {...form.register("sector")} />
            <FieldError>{form.formState.errors.sector?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="company-taxNumber">
            {tCrmClient("crm.companies.fields.taxNumber")}
          </FieldLabel>
          <FieldContent>
            <Input id="company-taxNumber" {...form.register("taxNumber")} />
            <FieldError>{form.formState.errors.taxNumber?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="company-taxOffice">
            {tCrmClient("crm.companies.fields.taxOffice")}
          </FieldLabel>
          <FieldContent>
            <Input id="company-taxOffice" {...form.register("taxOffice")} />
            <FieldError>{form.formState.errors.taxOffice?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="company-employeeCountRange">
            {tCrmClient("crm.companies.fields.employeeCountRange")}
          </FieldLabel>
          <FieldContent>
            <Input id="company-employeeCountRange" {...form.register("employeeCountRange")} />
            <FieldError>{form.formState.errors.employeeCountRange?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="company-annualRevenue">
            {tCrmClient("crm.companies.fields.annualRevenue")}
          </FieldLabel>
          <FieldContent>
            <Input
              id="company-annualRevenue"
              type="number"
              step="0.01"
              {...form.register("annualRevenue", { valueAsNumber: true })}
            />
            <FieldError>{form.formState.errors.annualRevenue?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="company-ownerUserId">
            {tCrmClient("crm.companies.fields.ownerUserId")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={form.watch("ownerUserId") ?? "__none__"}
              onValueChange={(value) =>
                form.setValue("ownerUserId", value && value !== "__none__" ? value : "", {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="company-ownerUserId">
                <SelectValue>
                  {getSelectDisplayLabel(
                    form.watch("ownerUserId") ?? "__none__",
                    ownerDisplayOptions,
                  )}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="__none__">-</SelectItem>
                {ownerUserOptions.map((option) => (
                  <SelectItem key={`company-owner-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.ownerUserId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="company-parentCompanyId">
            {tCrmClient("crm.companies.fields.parentCompanyId")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={form.watch("parentCompanyId") ?? "__none__"}
              onValueChange={(value) =>
                form.setValue("parentCompanyId", value && value !== "__none__" ? value : "", {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="company-parentCompanyId">
                <SelectValue>
                  {getSelectDisplayLabel(
                    form.watch("parentCompanyId") ?? "__none__",
                    parentCompanyDisplayOptions,
                  )}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="__none__">-</SelectItem>
                {companyOptions.map((option) => (
                  <SelectItem key={`company-parent-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.parentCompanyId?.message}</FieldError>
          </FieldContent>
        </Field>
      </FormGrid>

      <FormGrid>
        <TextareaField
          id="company-description"
          label={tCrmClient("crm.companies.fields.description")}
          rows={4}
          error={form.formState.errors.description?.message}
          {...form.register("description")}
        />
        <TextareaField
          id="company-notes"
          label={tCrmClient("crm.companies.fields.notes")}
          rows={4}
          error={form.formState.errors.notes?.message}
          {...form.register("notes")}
        />
      </FormGrid>

      {mode === "edit" ? <input type="hidden" {...form.register("rowVersion")} /> : null}

      <SubmitBar
        isPending={isPending}
        cancelLabel={tCrmClient("crm.forms.actions.cancel")}
        pendingLabel={tCrmClient("crm.forms.actions.saving")}
        submitLabel={
          mode === "create"
            ? tCrmClient("crm.companies.actions.create")
            : tCrmClient("crm.companies.actions.save")
        }
        onCancel={() => router.back()}
      />
    </form>
  );
}

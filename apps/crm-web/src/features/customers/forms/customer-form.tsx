"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition, useRef } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  Field,
  FieldContent,
  FieldError,
  FieldLabel,
  FormGrid,
  Input,
  SubmitBar,
  TextareaField,
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
import { customerTypeOptions, genderOptions } from "@/features/shared/forms/options";
import { getSelectDisplayLabel } from "@/features/shared/forms/select-display";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

import {
  createCustomerAction,
  updateCustomerAction,
  uploadCustomerImageAction,
} from "../actions/customer-mutation-actions";
import { customerFormSchema, type CustomerFormInput } from "./customer-form-schema";

type CustomerFormProps = {
  mode: "create" | "edit";
  customerId?: string;
  initialValues?: Partial<CustomerFormInput>;
  companyOptions?: CrmReferenceOption[];
  ownerUserOptions?: CrmReferenceOption[];
};

const defaults: CustomerFormInput = {
  firstName: "",
  lastName: "",
  title: "",
  email: "",
  mobilePhone: "",
  workPhone: "",
  personalPhone: "",
  birthDate: "",
  gender: 0,
  department: "",
  jobTitle: "",
  description: "",
  notes: "",
  ownerUserId: "",
  customerType: 0,
  identityNumber: "",
  isVip: false,
  isActive: true,
  companyId: "",
  rowVersion: "",
};

function isGuidClient(value: string): boolean {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(value);
}

export function CustomerForm({
  mode,
  customerId,
  initialValues,
  companyOptions = [],
  ownerUserOptions = [],
}: Readonly<CustomerFormProps>) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [result, setResult] = useState(initialCrmMutationState);
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);

  const form = useForm<CustomerFormInput>({
    resolver: zodResolver(customerFormSchema),
    defaultValues: { ...defaults, ...initialValues },
  });
  const gender = useWatch({ control: form.control, name: "gender" });
  const customerType = useWatch({ control: form.control, name: "customerType" });
  const isVip = useWatch({ control: form.control, name: "isVip" });
  const isActive = useWatch({ control: form.control, name: "isActive" });

  const onSubmit = (values: CustomerFormInput) => {
    setResult(initialCrmMutationState);

    startTransition(async () => {
      const response =
        mode === "create"
          ? await createCustomerAction(values)
          : await updateCustomerAction(customerId ?? "", values);

      if (mode === "create" && response.status === "success" && imageFile && response.redirectTo) {
        const createdId = response.redirectTo.split("/").at(-1);
        if (createdId && isGuidClient(createdId)) {
          const uploadPayload = new FormData();
          uploadPayload.set("file", imageFile, imageFile.name);
          await uploadCustomerImageAction(createdId, initialCrmMutationState, uploadPayload);
        }
      }

      setResult(response);

      if (response.fieldErrors) {
        for (const [field, errors] of Object.entries(response.fieldErrors)) {
          const first = errors[0];
          if (first) {
            form.setError(field as keyof CustomerFormInput, { message: first });
          }
        }
      }

      if (response.status === "success" && response.redirectTo) {
        router.push(response.redirectTo);
        router.refresh();
      }
    });
  };

  const firstNameError = form.formState.errors.firstName?.message;
  const lastNameError = form.formState.errors.lastName?.message;
  const genderLabelByValue: Record<number, string> = {
    0: tCrmClient("crm.customers.options.gender.unknown"),
    1: tCrmClient("crm.customers.options.gender.female"),
    2: tCrmClient("crm.customers.options.gender.male"),
    3: tCrmClient("crm.customers.options.gender.other"),
  };
  const customerTypeLabelByValue: Record<number, string> = {
    0: tCrmClient("crm.customers.options.customerType.individual"),
    1: tCrmClient("crm.customers.options.customerType.corporate"),
  };
  const genderDisplayOptions = genderOptions.map((option) => ({
    value: String(option.value),
    label: genderLabelByValue[option.value] ?? option.label,
  }));
  const customerTypeDisplayOptions = customerTypeOptions.map((option) => ({
    value: String(option.value),
    label: customerTypeLabelByValue[option.value] ?? option.label,
  }));
  const booleanDisplayOptions = [
    { value: "false", label: tCrmClient("crm.common.no") },
    { value: "true", label: tCrmClient("crm.common.yes") },
  ];
  const activeDisplayOptions = [
    { value: "true", label: tCrmClient("crm.common.active") },
    { value: "false", label: tCrmClient("crm.common.inactive") },
  ];
  const companyDisplayOptions = [{ value: "__none__", label: "-" }, ...companyOptions];
  const ownerDisplayOptions = [{ value: "__none__", label: "-" }, ...ownerUserOptions];

  return (
    <form className="space-y-6" noValidate onSubmit={form.handleSubmit(onSubmit)}>
      <CrmFormFeedback state={result} />

      {mode === "create" ? (
        <div className="pb-6 border-b border-border/30">
          <CrmImageUploader
            altText="Preview"
            description={tCrmClient("crm.customers.pages.detail.imageDescription")}
            imageUrl={previewUrl}
            title={tCrmClient("crm.customers.pages.detail.imageTitle")}
            onChange={(file) => {
              setImageFile(file);
              if (file) {
                setPreviewUrl(URL.createObjectURL(file));
              } else {
                setPreviewUrl(null);
              }
            }}
          />
        </div>
      ) : null}

      <FormGrid columns={2}>
        <Field>
          <FieldLabel htmlFor="customer-firstName">
            {tCrmClient("crm.customers.fields.firstName")}
          </FieldLabel>
          <FieldContent>
            <Input
              id="customer-firstName"
              {...form.register("firstName")}
              aria-invalid={Boolean(firstNameError)}
            />
            <FieldError>{firstNameError}</FieldError>
          </FieldContent>
        </Field>

        <Field>
          <FieldLabel htmlFor="customer-lastName">
            {tCrmClient("crm.customers.fields.lastName")}
          </FieldLabel>
          <FieldContent>
            <Input
              id="customer-lastName"
              {...form.register("lastName")}
              aria-invalid={Boolean(lastNameError)}
            />
            <FieldError>{lastNameError}</FieldError>
          </FieldContent>
        </Field>

        <Field>
          <FieldLabel htmlFor="customer-email">
            {tCrmClient("crm.customers.fields.email")}
          </FieldLabel>
          <FieldContent>
            <Input id="customer-email" type="email" {...form.register("email")} />
            <FieldError>{form.formState.errors.email?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="customer-mobilePhone">
            {tCrmClient("crm.customers.fields.mobilePhone")}
          </FieldLabel>
          <FieldContent>
            <Input id="customer-mobilePhone" {...form.register("mobilePhone")} />
            <FieldError>{form.formState.errors.mobilePhone?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="customer-workPhone">
            {tCrmClient("crm.customers.fields.workPhone")}
          </FieldLabel>
          <FieldContent>
            <Input id="customer-workPhone" {...form.register("workPhone")} />
            <FieldError>{form.formState.errors.workPhone?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="customer-personalPhone">
            {tCrmClient("crm.customers.fields.personalPhone")}
          </FieldLabel>
          <FieldContent>
            <Input id="customer-personalPhone" {...form.register("personalPhone")} />
            <FieldError>{form.formState.errors.personalPhone?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="customer-birthDate">
            {tCrmClient("crm.customers.fields.birthDate")}
          </FieldLabel>
          <FieldContent>
            <Input id="customer-birthDate" type="date" {...form.register("birthDate")} />
            <FieldError>{form.formState.errors.birthDate?.message}</FieldError>
          </FieldContent>
        </Field>

        <Field>
          <FieldLabel htmlFor="customer-gender">
            {tCrmClient("crm.customers.fields.gender")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={String(gender)}
              onValueChange={(value) =>
                form.setValue("gender", Number(value), { shouldDirty: true, shouldValidate: true })
              }
            >
              <SelectTrigger id="customer-gender">
                <SelectValue>
                  {getSelectDisplayLabel(String(gender), genderDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {genderOptions.map((option) => (
                  <SelectItem key={`gender-${option.value}`} value={String(option.value)}>
                    {genderLabelByValue[option.value] ?? option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </FieldContent>
        </Field>

        <Field>
          <FieldLabel htmlFor="customer-customerType">
            {tCrmClient("crm.customers.fields.customerType")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={String(customerType)}
              onValueChange={(value) =>
                form.setValue("customerType", Number(value), {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="customer-customerType">
                <SelectValue>
                  {getSelectDisplayLabel(String(customerType), customerTypeDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {customerTypeOptions.map((option) => (
                  <SelectItem key={`type-${option.value}`} value={String(option.value)}>
                    {customerTypeLabelByValue[option.value] ?? option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </FieldContent>
        </Field>

        <Field>
          <FieldLabel htmlFor="customer-vip">{tCrmClient("crm.customers.fields.vip")}</FieldLabel>
          <FieldContent>
            <Select
              value={String(isVip)}
              onValueChange={(value) =>
                form.setValue("isVip", value === "true", {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="customer-vip">
                <SelectValue>
                  {getSelectDisplayLabel(String(isVip), booleanDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="false">{tCrmClient("crm.common.no")}</SelectItem>
                <SelectItem value="true">{tCrmClient("crm.common.yes")}</SelectItem>
              </SelectContent>
            </Select>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="customer-companyId">
            {tCrmClient("crm.customers.fields.companyId")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={form.watch("companyId") ?? "__none__"}
              onValueChange={(value) =>
                form.setValue("companyId", value && value !== "__none__" ? value : "", {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="customer-companyId">
                <SelectValue>
                  {getSelectDisplayLabel(
                    form.watch("companyId") ?? "__none__",
                    companyDisplayOptions,
                  )}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="__none__">-</SelectItem>
                {companyOptions.map((option) => (
                  <SelectItem key={`customer-company-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.companyId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="customer-isActive">
            {tCrmClient("crm.customers.fields.status")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={String(isActive)}
              onValueChange={(value) =>
                form.setValue("isActive", value === "true", {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="customer-isActive">
                <SelectValue>
                  {getSelectDisplayLabel(String(isActive), activeDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="true">{tCrmClient("crm.common.active")}</SelectItem>
                <SelectItem value="false">{tCrmClient("crm.common.inactive")}</SelectItem>
              </SelectContent>
            </Select>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="customer-ownerUserId">
            {tCrmClient("crm.customers.fields.ownerUserId")}
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
              <SelectTrigger id="customer-ownerUserId">
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
                  <SelectItem key={`customer-owner-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.ownerUserId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="customer-title">
            {tCrmClient("crm.customers.fields.title")}
          </FieldLabel>
          <FieldContent>
            <Input id="customer-title" {...form.register("title")} />
            <FieldError>{form.formState.errors.title?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="customer-identityNumber">
            {tCrmClient("crm.customers.fields.identityNumber")}
          </FieldLabel>
          <FieldContent>
            <Input id="customer-identityNumber" {...form.register("identityNumber")} />
            <FieldError>{form.formState.errors.identityNumber?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="customer-department">
            {tCrmClient("crm.customers.fields.department")}
          </FieldLabel>
          <FieldContent>
            <Input id="customer-department" {...form.register("department")} />
            <FieldError>{form.formState.errors.department?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="customer-jobTitle">
            {tCrmClient("crm.customers.fields.jobTitle")}
          </FieldLabel>
          <FieldContent>
            <Input id="customer-jobTitle" {...form.register("jobTitle")} />
            <FieldError>{form.formState.errors.jobTitle?.message}</FieldError>
          </FieldContent>
        </Field>
      </FormGrid>

      <FormGrid>
        <TextareaField
          id="customer-description"
          label={tCrmClient("crm.customers.fields.description")}
          rows={4}
          error={form.formState.errors.description?.message}
          {...form.register("description")}
        />
        <TextareaField
          id="customer-notes"
          label={tCrmClient("crm.customers.fields.notes")}
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
            ? tCrmClient("crm.customers.actions.create")
            : tCrmClient("crm.customers.actions.save")
        }
        onCancel={() => router.back()}
      />
    </form>
  );
}

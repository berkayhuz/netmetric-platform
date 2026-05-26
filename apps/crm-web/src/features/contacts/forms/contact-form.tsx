"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
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

import { CrmFormFeedback } from "@/components/forms/crm-form-feedback";
import { initialCrmMutationState } from "@/features/shared/actions/mutation-state";
import type { CrmReferenceOption } from "@/features/shared/data/form-reference-data";
import { genderOptions } from "@/features/shared/forms/options";
import { getSelectDisplayLabel } from "@/features/shared/forms/select-display";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

import { createContactAction, updateContactAction } from "../actions/contact-mutation-actions";
import { contactFormSchema, type ContactFormInput } from "./contact-form-schema";

type ContactFormProps = {
  mode: "create" | "edit";
  contactId?: string;
  initialValues?: Partial<ContactFormInput>;
  companyOptions?: CrmReferenceOption[];
  customerOptions?: CrmReferenceOption[];
  ownerUserOptions?: CrmReferenceOption[];
};

const defaults: ContactFormInput = {
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
  companyId: "",
  customerId: "",
  isPrimaryContact: false,
  rowVersion: "",
};

export function ContactForm({
  mode,
  contactId,
  initialValues,
  companyOptions = [],
  customerOptions = [],
  ownerUserOptions = [],
}: Readonly<ContactFormProps>) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [result, setResult] = useState(initialCrmMutationState);
  const form = useForm<ContactFormInput>({
    resolver: zodResolver(contactFormSchema),
    defaultValues: { ...defaults, ...initialValues },
  });
  const gender = useWatch({ control: form.control, name: "gender" });
  const isPrimaryContact = useWatch({ control: form.control, name: "isPrimaryContact" });

  const onSubmit = (values: ContactFormInput) => {
    setResult(initialCrmMutationState);
    startTransition(async () => {
      const response =
        mode === "create"
          ? await createContactAction(values)
          : await updateContactAction(contactId ?? "", values);
      setResult(response);
      if (response.fieldErrors) {
        for (const [field, errors] of Object.entries(response.fieldErrors)) {
          const first = errors[0];
          if (first) form.setError(field as keyof ContactFormInput, { message: first });
        }
      }
      if (response.status === "success" && response.redirectTo) {
        router.push(response.redirectTo);
        router.refresh();
      }
    });
  };

  const genderLabelByValue: Record<number, string> = {
    0: tCrmClient("crm.contacts.options.gender.unknown"),
    1: tCrmClient("crm.contacts.options.gender.female"),
    2: tCrmClient("crm.contacts.options.gender.male"),
    3: tCrmClient("crm.contacts.options.gender.other"),
  };
  const genderDisplayOptions = genderOptions.map((option) => ({
    value: String(option.value),
    label: genderLabelByValue[option.value] ?? option.label,
  }));
  const primaryDisplayOptions = [
    { value: "false", label: tCrmClient("crm.common.no") },
    { value: "true", label: tCrmClient("crm.common.yes") },
  ];
  const companyDisplayOptions = [{ value: "__none__", label: "-" }, ...companyOptions];
  const customerDisplayOptions = [{ value: "__none__", label: "-" }, ...customerOptions];
  const ownerDisplayOptions = [{ value: "__none__", label: "-" }, ...ownerUserOptions];

  return (
    <form className="space-y-6" noValidate onSubmit={form.handleSubmit(onSubmit)}>
      <CrmFormFeedback state={result} />

      <FormGrid columns={2}>
        <Field>
          <FieldLabel htmlFor="contact-firstName">
            {tCrmClient("crm.contacts.fields.firstName")}
          </FieldLabel>
          <FieldContent>
            <Input id="contact-firstName" {...form.register("firstName")} />
            <FieldError>{form.formState.errors.firstName?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="contact-lastName">
            {tCrmClient("crm.contacts.fields.lastName")}
          </FieldLabel>
          <FieldContent>
            <Input id="contact-lastName" {...form.register("lastName")} />
            <FieldError>{form.formState.errors.lastName?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="contact-email">{tCrmClient("crm.contacts.fields.email")}</FieldLabel>
          <FieldContent>
            <Input id="contact-email" type="email" {...form.register("email")} />
            <FieldError>{form.formState.errors.email?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="contact-mobilePhone">
            {tCrmClient("crm.contacts.fields.mobilePhone")}
          </FieldLabel>
          <FieldContent>
            <Input id="contact-mobilePhone" {...form.register("mobilePhone")} />
            <FieldError>{form.formState.errors.mobilePhone?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="contact-workPhone">
            {tCrmClient("crm.contacts.fields.workPhone")}
          </FieldLabel>
          <FieldContent>
            <Input id="contact-workPhone" {...form.register("workPhone")} />
            <FieldError>{form.formState.errors.workPhone?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="contact-personalPhone">
            {tCrmClient("crm.contacts.fields.personalPhone")}
          </FieldLabel>
          <FieldContent>
            <Input id="contact-personalPhone" {...form.register("personalPhone")} />
            <FieldError>{form.formState.errors.personalPhone?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="contact-birthDate">
            {tCrmClient("crm.contacts.fields.birthDate")}
          </FieldLabel>
          <FieldContent>
            <Input id="contact-birthDate" type="date" {...form.register("birthDate")} />
            <FieldError>{form.formState.errors.birthDate?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="contact-gender">
            {tCrmClient("crm.contacts.fields.gender")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={String(gender)}
              onValueChange={(value) =>
                form.setValue("gender", Number(value), { shouldDirty: true, shouldValidate: true })
              }
            >
              <SelectTrigger id="contact-gender">
                <SelectValue>
                  {getSelectDisplayLabel(String(gender), genderDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {genderOptions.map((o) => (
                  <SelectItem key={`contact-gender-${o.value}`} value={String(o.value)}>
                    {genderLabelByValue[o.value] ?? o.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.gender?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="contact-primary">
            {tCrmClient("crm.contacts.fields.primaryContact")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={String(isPrimaryContact)}
              onValueChange={(value) =>
                form.setValue("isPrimaryContact", value === "true", {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="contact-primary">
                <SelectValue>
                  {getSelectDisplayLabel(String(isPrimaryContact), primaryDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="false">{tCrmClient("crm.common.no")}</SelectItem>
                <SelectItem value="true">{tCrmClient("crm.common.yes")}</SelectItem>
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.isPrimaryContact?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="contact-companyId">
            {tCrmClient("crm.contacts.fields.companyId")}
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
              <SelectTrigger id="contact-companyId">
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
                  <SelectItem key={`contact-company-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.companyId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="contact-customerId">
            {tCrmClient("crm.contacts.fields.customerId")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={form.watch("customerId") ?? "__none__"}
              onValueChange={(value) =>
                form.setValue("customerId", value && value !== "__none__" ? value : "", {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="contact-customerId">
                <SelectValue>
                  {getSelectDisplayLabel(
                    form.watch("customerId") ?? "__none__",
                    customerDisplayOptions,
                  )}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="__none__">-</SelectItem>
                {customerOptions.map((option) => (
                  <SelectItem key={`contact-customer-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.customerId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="contact-ownerUserId">
            {tCrmClient("crm.contacts.fields.ownerUserId")}
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
              <SelectTrigger id="contact-ownerUserId">
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
                  <SelectItem key={`contact-owner-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.ownerUserId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="contact-title">{tCrmClient("crm.contacts.fields.title")}</FieldLabel>
          <FieldContent>
            <Input id="contact-title" {...form.register("title")} />
            <FieldError>{form.formState.errors.title?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="contact-department">
            {tCrmClient("crm.contacts.fields.department")}
          </FieldLabel>
          <FieldContent>
            <Input id="contact-department" {...form.register("department")} />
            <FieldError>{form.formState.errors.department?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="contact-jobTitle">
            {tCrmClient("crm.contacts.fields.jobTitle")}
          </FieldLabel>
          <FieldContent>
            <Input id="contact-jobTitle" {...form.register("jobTitle")} />
            <FieldError>{form.formState.errors.jobTitle?.message}</FieldError>
          </FieldContent>
        </Field>
      </FormGrid>

      <FormGrid>
        <TextareaField
          id="contact-description"
          label={tCrmClient("crm.contacts.fields.description")}
          rows={4}
          error={form.formState.errors.description?.message}
          {...form.register("description")}
        />
        <TextareaField
          id="contact-notes"
          label={tCrmClient("crm.contacts.fields.notes")}
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
            ? tCrmClient("crm.contacts.actions.create")
            : tCrmClient("crm.contacts.actions.save")
        }
        onCancel={() => router.back()}
      />
    </form>
  );
}

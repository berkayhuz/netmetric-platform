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
import {
  leadSourceOptions,
  leadStatusOptions,
  priorityOptions,
} from "@/features/shared/forms/options";
import { getSelectDisplayLabel } from "@/features/shared/forms/select-display";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

import { createLeadAction, updateLeadAction } from "../actions/lead-mutation-actions";
import { leadFormSchema, type LeadFormInput } from "./lead-form-schema";

type LeadFormProps = {
  mode: "create" | "edit";
  leadId?: string;
  initialValues?: Partial<LeadFormInput>;
  customerOptions?: CrmReferenceOption[];
  companyOptions?: CrmReferenceOption[];
  ownerUserOptions?: CrmReferenceOption[];
};

const defaults: LeadFormInput = {
  fullName: "",
  companyName: "",
  email: "",
  phone: "",
  jobTitle: "",
  description: "",
  estimatedBudget: "",
  nextContactDate: "",
  source: 0,
  status: 0,
  priority: 1,
  companyId: "",
  ownerUserId: "",
  notes: "",
  rowVersion: "",
};

export function LeadForm({
  mode,
  leadId,
  initialValues,
  customerOptions = [],
  companyOptions = [],
  ownerUserOptions = [],
}: Readonly<LeadFormProps>) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [result, setResult] = useState(initialCrmMutationState);
  const [existingCustomerId, setExistingCustomerId] = useState("__none__");
  const [existingCompanyId, setExistingCompanyId] = useState("__none__");
  const form = useForm<LeadFormInput>({
    resolver: zodResolver(leadFormSchema),
    defaultValues: { ...defaults, ...initialValues },
  });
  const source = useWatch({ control: form.control, name: "source" });
  const status = useWatch({ control: form.control, name: "status" });
  const priority = useWatch({ control: form.control, name: "priority" });
  const sourceDisplayOptions = leadSourceOptions.map((option) => ({
    value: String(option.value),
    label: tCrmClient(`crm.leads.source.${option.value}`),
  }));
  const statusDisplayOptions = leadStatusOptions.map((option) => ({
    value: String(option.value),
    label: tCrmClient(`crm.leads.status.${option.value}`),
  }));
  const priorityDisplayOptions = priorityOptions.map((option) => ({
    value: String(option.value),
    label: tCrmClient(`crm.common.priority.${option.value}`),
  }));
  const customerDisplayOptions = [{ value: "__none__", label: "-" }, ...customerOptions];
  const companyDisplayOptions = [{ value: "__none__", label: "-" }, ...companyOptions];
  const ownerDisplayOptions = [{ value: "__none__", label: "-" }, ...ownerUserOptions];

  const onSubmit = (values: LeadFormInput) => {
    setResult(initialCrmMutationState);
    startTransition(async () => {
      const response =
        mode === "create"
          ? await createLeadAction(values)
          : await updateLeadAction(leadId ?? "", values);
      setResult(response);
      if (response.fieldErrors) {
        for (const [field, errors] of Object.entries(response.fieldErrors)) {
          const first = errors[0];
          if (first) form.setError(field as keyof LeadFormInput, { message: first });
        }
      }
      if (response.status === "success" && response.redirectTo) {
        router.push(response.redirectTo);
        router.refresh();
      }
    });
  };

  return (
    <form className="space-y-6" noValidate onSubmit={form.handleSubmit(onSubmit)}>
      <CrmFormFeedback state={result} />

      <FormGrid columns={2}>
        <Field>
          <FieldLabel htmlFor="lead-existingCustomer">
            {tCrmClient("crm.opportunities.fields.customerId")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={existingCustomerId}
              onValueChange={(value) => {
                const nextValue = value ?? "__none__";
                setExistingCustomerId(nextValue);
                if (value === "__none__") {
                  return;
                }
                const selected = customerOptions.find((option) => option.value === value);
                if (selected) {
                  const [namePart] = selected.label.split(" - ");
                  form.setValue("fullName", namePart ?? "", {
                    shouldDirty: true,
                    shouldValidate: true,
                  });
                }
              }}
            >
              <SelectTrigger id="lead-existingCustomer">
                <SelectValue placeholder="-">
                  {getSelectDisplayLabel(existingCustomerId, customerDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="__none__">-</SelectItem>
                {customerOptions.map((option) => (
                  <SelectItem key={`lead-customer-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="lead-existingCompany">
            {tCrmClient("crm.leads.fields.companyName")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={existingCompanyId}
              onValueChange={(value) => {
                const nextValue = value ?? "__none__";
                setExistingCompanyId(nextValue);
                if (value === "__none__") {
                  return;
                }
                const selected = companyOptions.find((option) => option.value === value);
                if (selected) {
                  const [namePart] = selected.label.split(" - ");
                  form.setValue("companyName", namePart ?? "", {
                    shouldDirty: true,
                    shouldValidate: true,
                  });
                  form.setValue("companyId", selected.value, {
                    shouldDirty: true,
                    shouldValidate: true,
                  });
                }
              }}
            >
              <SelectTrigger id="lead-existingCompany">
                <SelectValue placeholder="-">
                  {getSelectDisplayLabel(existingCompanyId, companyDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="__none__">-</SelectItem>
                {companyOptions.map((option) => (
                  <SelectItem key={`lead-existing-company-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="lead-fullName">{tCrmClient("crm.leads.fields.fullName")}</FieldLabel>
          <FieldContent>
            <Input id="lead-fullName" {...form.register("fullName")} />
            <FieldError>{form.formState.errors.fullName?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="lead-companyName">
            {tCrmClient("crm.leads.fields.companyName")}
          </FieldLabel>
          <FieldContent>
            <Input id="lead-companyName" {...form.register("companyName")} />
            <FieldError>{form.formState.errors.companyName?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="lead-email">{tCrmClient("crm.leads.fields.email")}</FieldLabel>
          <FieldContent>
            <Input id="lead-email" type="email" {...form.register("email")} />
            <FieldError>{form.formState.errors.email?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="lead-phone">{tCrmClient("crm.leads.fields.phone")}</FieldLabel>
          <FieldContent>
            <Input id="lead-phone" {...form.register("phone")} />
            <FieldError>{form.formState.errors.phone?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="lead-jobTitle">{tCrmClient("crm.leads.fields.jobTitle")}</FieldLabel>
          <FieldContent>
            <Input id="lead-jobTitle" {...form.register("jobTitle")} />
            <FieldError>{form.formState.errors.jobTitle?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="lead-estimatedBudget">
            {tCrmClient("crm.leads.fields.estimatedBudget")}
          </FieldLabel>
          <FieldContent>
            <Input
              id="lead-estimatedBudget"
              inputMode="decimal"
              {...form.register("estimatedBudget")}
            />
            <FieldError>{form.formState.errors.estimatedBudget?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="lead-nextContactDate">
            {tCrmClient("crm.leads.fields.nextContactDate")}
          </FieldLabel>
          <FieldContent>
            <Input id="lead-nextContactDate" type="date" {...form.register("nextContactDate")} />
            <FieldError>{form.formState.errors.nextContactDate?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="lead-source">{tCrmClient("crm.leads.fields.source")}</FieldLabel>
          <FieldContent>
            <Select
              value={String(source)}
              onValueChange={(value) =>
                form.setValue("source", Number(value), { shouldDirty: true, shouldValidate: true })
              }
            >
              <SelectTrigger id="lead-source">
                <SelectValue>
                  {getSelectDisplayLabel(String(source), sourceDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {leadSourceOptions.map((o) => (
                  <SelectItem key={`lead-source-${o.value}`} value={String(o.value)}>
                    {tCrmClient(`crm.leads.source.${o.value}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.source?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="lead-status">{tCrmClient("crm.leads.fields.status")}</FieldLabel>
          <FieldContent>
            <Select
              value={String(status)}
              onValueChange={(value) =>
                form.setValue("status", Number(value), { shouldDirty: true, shouldValidate: true })
              }
            >
              <SelectTrigger id="lead-status">
                <SelectValue>
                  {getSelectDisplayLabel(String(status), statusDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {leadStatusOptions.map((o) => (
                  <SelectItem key={`lead-status-${o.value}`} value={String(o.value)}>
                    {tCrmClient(`crm.leads.status.${o.value}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.status?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="lead-priority">{tCrmClient("crm.leads.fields.priority")}</FieldLabel>
          <FieldContent>
            <Select
              value={String(priority)}
              onValueChange={(value) =>
                form.setValue("priority", Number(value), {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="lead-priority">
                <SelectValue>
                  {getSelectDisplayLabel(String(priority), priorityDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {priorityOptions.map((o) => (
                  <SelectItem key={`lead-priority-${o.value}`} value={String(o.value)}>
                    {tCrmClient(`crm.common.priority.${o.value}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.priority?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="lead-companyId">
            {tCrmClient("crm.leads.fields.companyId")}
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
              <SelectTrigger id="lead-companyId">
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
                  <SelectItem key={`lead-company-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.companyId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="lead-ownerUserId">
            {tCrmClient("crm.leads.fields.ownerUserId")}
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
              <SelectTrigger id="lead-ownerUserId">
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
                  <SelectItem key={`lead-owner-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.ownerUserId?.message}</FieldError>
          </FieldContent>
        </Field>
      </FormGrid>

      <FormGrid>
        <TextareaField
          id="lead-description"
          label={tCrmClient("crm.leads.fields.description")}
          rows={4}
          error={form.formState.errors.description?.message}
          {...form.register("description")}
        />
        <TextareaField
          id="lead-notes"
          label={tCrmClient("crm.leads.fields.notes")}
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
            ? tCrmClient("crm.leads.actions.create")
            : tCrmClient("crm.leads.actions.save")
        }
        onCancel={() => router.back()}
      />
    </form>
  );
}

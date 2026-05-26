"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  Button,
  Field,
  FieldContent,
  FieldError,
  FieldLabel,
  FieldSet,
  Input,
  Textarea,
} from "@netmetric/ui";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@netmetric/ui/client";
import { useForm, useWatch } from "react-hook-form";

import { CrmFormErrorSummary } from "@/components/forms/crm-form-error-summary";
import { CrmMutationResult } from "@/components/forms/crm-mutation-result";
import { initialCrmMutationState } from "@/features/shared/actions/mutation-state";
import type { CrmReferenceOption } from "@/features/shared/data/form-reference-data";
import {
  priorityOptions,
  ticketChannelOptions,
  ticketTypeOptions,
} from "@/features/shared/forms/options";
import { getSelectDisplayLabel } from "@/features/shared/forms/select-display";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

import { createTicketAction, updateTicketAction } from "../actions/ticket-mutation-actions";
import { ticketFormSchema, type TicketFormInput } from "./ticket-form-schema";

type TicketFormProps = {
  mode: "create" | "edit";
  ticketId?: string;
  initialValues?: Partial<TicketFormInput>;
  customerOptions?: CrmReferenceOption[];
  contactOptions?: CrmReferenceOption[];
  ownerUserOptions?: CrmReferenceOption[];
};

const defaults: TicketFormInput = {
  subject: "",
  description: "",
  ticketType: 0,
  channel: 0,
  priority: 1,
  assignedUserId: "",
  customerId: "",
  contactId: "",
  ticketCategoryId: "",
  slaPolicyId: "",
  firstResponseDueAt: "",
  resolveDueAt: "",
  notes: "",
  rowVersion: "",
};

export function TicketForm({
  mode,
  ticketId,
  initialValues,
  customerOptions = [],
  contactOptions = [],
  ownerUserOptions = [],
}: Readonly<TicketFormProps>) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [result, setResult] = useState(initialCrmMutationState);
  const form = useForm<TicketFormInput>({
    resolver: zodResolver(ticketFormSchema),
    defaultValues: { ...defaults, ...initialValues },
  });
  const ticketType = useWatch({ control: form.control, name: "ticketType" });
  const channel = useWatch({ control: form.control, name: "channel" });
  const priority = useWatch({ control: form.control, name: "priority" });
  const ticketTypeDisplayOptions = ticketTypeOptions.map((option) => ({
    value: String(option.value),
    label: tCrmClient(`crm.tickets.type.${option.value}`),
  }));
  const channelDisplayOptions = ticketChannelOptions.map((option) => ({
    value: String(option.value),
    label: tCrmClient(`crm.tickets.channel.${option.value}`),
  }));
  const priorityDisplayOptions = priorityOptions.map((option) => ({
    value: String(option.value),
    label: tCrmClient(`crm.common.priority.${option.value}`),
  }));
  const ownerDisplayOptions = [{ value: "__none__", label: "-" }, ...ownerUserOptions];
  const customerDisplayOptions = [{ value: "__none__", label: "-" }, ...customerOptions];
  const contactDisplayOptions = [{ value: "__none__", label: "-" }, ...contactOptions];

  const onSubmit = (values: TicketFormInput) => {
    setResult(initialCrmMutationState);
    startTransition(async () => {
      const response =
        mode === "create"
          ? await createTicketAction(values)
          : await updateTicketAction(ticketId ?? "", values);

      setResult(response);

      if (response.fieldErrors) {
        for (const [field, errors] of Object.entries(response.fieldErrors)) {
          const first = errors[0];
          if (first) form.setError(field as keyof TicketFormInput, { message: first });
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
      <CrmFormErrorSummary
        {...(result.status === "error" && result.message ? { message: result.message } : {})}
        {...(result.fieldErrors ? { errors: result.fieldErrors } : {})}
      />
      <CrmMutationResult state={result} />

      <FieldSet className="grid gap-4 sm:grid-cols-2">
        <Field className="sm:col-span-2">
          <FieldLabel htmlFor="ticket-subject">
            {tCrmClient("crm.tickets.fields.subject")}
          </FieldLabel>
          <FieldContent>
            <Input id="ticket-subject" {...form.register("subject")} />
            <FieldError>{form.formState.errors.subject?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="ticket-type">{tCrmClient("crm.tickets.fields.type")}</FieldLabel>
          <FieldContent>
            <Select
              value={String(ticketType)}
              onValueChange={(value) =>
                form.setValue("ticketType", Number(value), {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="ticket-type">
                <SelectValue>
                  {getSelectDisplayLabel(String(ticketType), ticketTypeDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {ticketTypeOptions.map((option) => (
                  <SelectItem key={`ticket-type-${option.value}`} value={String(option.value)}>
                    {tCrmClient(`crm.tickets.type.${option.value}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.ticketType?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="ticket-channel">
            {tCrmClient("crm.tickets.fields.channel")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={String(channel)}
              onValueChange={(value) =>
                form.setValue("channel", Number(value), { shouldDirty: true, shouldValidate: true })
              }
            >
              <SelectTrigger id="ticket-channel">
                <SelectValue>
                  {getSelectDisplayLabel(String(channel), channelDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {ticketChannelOptions.map((option) => (
                  <SelectItem key={`ticket-channel-${option.value}`} value={String(option.value)}>
                    {tCrmClient(`crm.tickets.channel.${option.value}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.channel?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="ticket-priority">
            {tCrmClient("crm.tickets.fields.priority")}
          </FieldLabel>
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
              <SelectTrigger id="ticket-priority">
                <SelectValue>
                  {getSelectDisplayLabel(String(priority), priorityDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {priorityOptions.map((option) => (
                  <SelectItem key={`ticket-priority-${option.value}`} value={String(option.value)}>
                    {tCrmClient(`crm.common.priority.${option.value}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.priority?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="ticket-assignedUserId">
            {tCrmClient("crm.tickets.fields.assignedUserId")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={form.watch("assignedUserId") ?? "__none__"}
              onValueChange={(value) =>
                form.setValue("assignedUserId", value && value !== "__none__" ? value : "", {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="ticket-assignedUserId">
                <SelectValue>
                  {getSelectDisplayLabel(
                    form.watch("assignedUserId") ?? "__none__",
                    ownerDisplayOptions,
                  )}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="__none__">-</SelectItem>
                {ownerUserOptions.map((option) => (
                  <SelectItem key={`ticket-owner-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.assignedUserId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="ticket-customerId">
            {tCrmClient("crm.tickets.fields.customerId")}
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
              <SelectTrigger id="ticket-customerId">
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
                  <SelectItem key={`ticket-customer-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.customerId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="ticket-contactId">
            {tCrmClient("crm.tickets.fields.contactId")}
          </FieldLabel>
          <FieldContent>
            <Select
              value={form.watch("contactId") ?? "__none__"}
              onValueChange={(value) =>
                form.setValue("contactId", value && value !== "__none__" ? value : "", {
                  shouldDirty: true,
                  shouldValidate: true,
                })
              }
            >
              <SelectTrigger id="ticket-contactId">
                <SelectValue>
                  {getSelectDisplayLabel(
                    form.watch("contactId") ?? "__none__",
                    contactDisplayOptions,
                  )}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="__none__">-</SelectItem>
                {contactOptions.map((option) => (
                  <SelectItem key={`ticket-contact-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.contactId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="ticket-ticketCategoryId">
            {tCrmClient("crm.tickets.fields.ticketCategoryId")}
          </FieldLabel>
          <FieldContent>
            <Input id="ticket-ticketCategoryId" {...form.register("ticketCategoryId")} />
            <FieldError>{form.formState.errors.ticketCategoryId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="ticket-slaPolicyId">
            {tCrmClient("crm.tickets.fields.slaPolicyId")}
          </FieldLabel>
          <FieldContent>
            <Input id="ticket-slaPolicyId" {...form.register("slaPolicyId")} />
            <FieldError>{form.formState.errors.slaPolicyId?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="ticket-firstResponseDueAt">
            {tCrmClient("crm.tickets.fields.firstResponseDueAt")}
          </FieldLabel>
          <FieldContent>
            <Input
              id="ticket-firstResponseDueAt"
              type="datetime-local"
              {...form.register("firstResponseDueAt")}
            />
            <FieldError>{form.formState.errors.firstResponseDueAt?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="ticket-resolveDueAt">
            {tCrmClient("crm.tickets.fields.resolveDueAt")}
          </FieldLabel>
          <FieldContent>
            <Input
              id="ticket-resolveDueAt"
              type="datetime-local"
              {...form.register("resolveDueAt")}
            />
            <FieldError>{form.formState.errors.resolveDueAt?.message}</FieldError>
          </FieldContent>
        </Field>
      </FieldSet>

      <FieldSet className="grid gap-4">
        <Field>
          <FieldLabel htmlFor="ticket-description">
            {tCrmClient("crm.tickets.fields.description")}
          </FieldLabel>
          <FieldContent>
            <Textarea id="ticket-description" rows={4} {...form.register("description")} />
            <FieldError>{form.formState.errors.description?.message}</FieldError>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="ticket-notes">{tCrmClient("crm.tickets.fields.notes")}</FieldLabel>
          <FieldContent>
            <Textarea id="ticket-notes" rows={4} {...form.register("notes")} />
            <FieldError>{form.formState.errors.notes?.message}</FieldError>
          </FieldContent>
        </Field>
      </FieldSet>

      {mode === "edit" ? <input type="hidden" {...form.register("rowVersion")} /> : null}

      <div className="flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
        <Button type="button" variant="outline" onClick={() => router.back()}>
          {tCrmClient("crm.forms.actions.cancel")}
        </Button>
        <Button type="submit" disabled={isPending} aria-busy={isPending}>
          {isPending
            ? tCrmClient("crm.forms.actions.saving")
            : mode === "create"
              ? tCrmClient("crm.tickets.actions.create")
              : tCrmClient("crm.tickets.actions.save")}
        </Button>
      </div>
    </form>
  );
}

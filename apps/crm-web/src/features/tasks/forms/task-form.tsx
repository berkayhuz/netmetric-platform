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

import { CrmFormFeedback } from "@/components/forms/crm-form-feedback";
import { initialCrmMutationState } from "@/features/shared/actions/mutation-state";
import type { CrmReferenceOption } from "@/features/shared/data/form-reference-data";
import { priorityOptions } from "@/features/shared/forms/options";
import { getSelectDisplayLabel } from "@/features/shared/forms/select-display";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

import { createWorkTaskAction } from "../actions/work-management-create-actions";
import { updateTaskAction } from "../actions/work-management-task-lifecycle-actions";
import { taskFormSchema, type TaskFormInput } from "./task-form-schema";

const defaults: TaskFormInput = {
  title: "",
  description: "",
  ownerUserId: "",
  dueAtUtc: "",
  priority: 1,
};

export function TaskForm({
  ownerUserOptions = [],
  mode = "create",
  taskId,
  initialValues,
}: Readonly<{
  ownerUserOptions?: CrmReferenceOption[];
  mode?: "create" | "edit";
  taskId?: string;
  initialValues?: Partial<TaskFormInput>;
}>) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [result, setResult] = useState(initialCrmMutationState);
  const form = useForm<TaskFormInput>({
    resolver: zodResolver(taskFormSchema),
    defaultValues: { ...defaults, ...initialValues },
  });
  const priority = useWatch({ control: form.control, name: "priority" });
  const ownerDisplayOptions = [{ value: "__none__", label: "-" }, ...ownerUserOptions];
  const priorityDisplayOptions = priorityOptions.map((option) => ({
    value: String(option.value),
    label: tCrmClient(`crm.common.priority.${option.value}`),
  }));

  const onSubmit = (values: TaskFormInput) => {
    setResult(initialCrmMutationState);
    startTransition(async () => {
      const response =
        mode === "create"
          ? await createWorkTaskAction(values)
          : await updateTaskAction(taskId ?? "", values);
      setResult(response);
      if (response.fieldErrors) {
        for (const [field, errors] of Object.entries(response.fieldErrors)) {
          const first = errors[0];
          if (first) form.setError(field as keyof TaskFormInput, { message: first });
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
        <Field className="sm:col-span-2">
          <FieldLabel htmlFor="task-title">{tCrmClient("crm.tasks.fields.title")}</FieldLabel>
          <FieldContent>
            <Input id="task-title" {...form.register("title")} />
            <FieldError>{form.formState.errors.title?.message}</FieldError>
          </FieldContent>
        </Field>

        <Field className="sm:col-span-2">
          <FieldLabel htmlFor="task-description">
            {tCrmClient("crm.tasks.fields.description")}
          </FieldLabel>
          <FieldContent>
            <Textarea id="task-description" rows={4} {...form.register("description")} />
            <FieldError>{form.formState.errors.description?.message}</FieldError>
          </FieldContent>
        </Field>

        <Field>
          <FieldLabel htmlFor="task-ownerUserId">
            {tCrmClient("crm.tasks.fields.ownerUserId")}
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
              <SelectTrigger id="task-ownerUserId">
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
                  <SelectItem key={`task-owner-${option.value}`} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.ownerUserId?.message}</FieldError>
          </FieldContent>
        </Field>

        <Field>
          <FieldLabel htmlFor="task-dueAtUtc">{tCrmClient("crm.tasks.fields.dueAtUtc")}</FieldLabel>
          <FieldContent>
            <Input id="task-dueAtUtc" type="datetime-local" {...form.register("dueAtUtc")} />
            <FieldError>{form.formState.errors.dueAtUtc?.message}</FieldError>
          </FieldContent>
        </Field>

        <Field>
          <FieldLabel htmlFor="task-priority">{tCrmClient("crm.tasks.fields.priority")}</FieldLabel>
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
              <SelectTrigger id="task-priority">
                <SelectValue>
                  {getSelectDisplayLabel(String(priority), priorityDisplayOptions)}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {priorityOptions.map((o) => (
                  <SelectItem key={`task-priority-${o.value}`} value={String(o.value)}>
                    {tCrmClient(`crm.common.priority.${o.value}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError>{form.formState.errors.priority?.message}</FieldError>
          </FieldContent>
        </Field>
      </FormGrid>

      <SubmitBar
        isPending={isPending}
        cancelLabel={tCrmClient("crm.forms.actions.cancel")}
        pendingLabel={
          mode === "create"
            ? tCrmClient("crm.forms.actions.creating")
            : tCrmClient("crm.forms.actions.saving")
        }
        submitLabel={
          mode === "create"
            ? tCrmClient("crm.tasks.actions.create")
            : tCrmClient("crm.forms.actions.save")
        }
        onCancel={() => router.back()}
      />
    </form>
  );
}

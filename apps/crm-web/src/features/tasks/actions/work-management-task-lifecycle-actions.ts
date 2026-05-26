"use server";

import { revalidatePath } from "next/cache";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import { crmApiClient } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import { taskFormSchema, type TaskFormInput } from "../forms/task-form-schema";

function mapFieldErrors(
  fieldErrors: Record<string, string[] | undefined>,
): Record<string, string[]> {
  return Object.fromEntries(
    Object.entries(fieldErrors).flatMap(([key, errors]) => {
      if (!errors || errors.length === 0) {
        return [];
      }

      return [[key, errors] as const];
    }),
  );
}

function toIsoUtc(value: string): string | null {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return null;
  }

  return date.toISOString();
}

function revalidateTaskPaths(taskId: string) {
  revalidatePath("/tasks");
  revalidatePath(`/tasks/${taskId}`);
  revalidatePath(`/tasks/${taskId}/edit`);
}

export async function updateTaskAction(
  taskId: string,
  input: TaskFormInput,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/tasks/${taskId}/edit`, "tasks.edit");

  const parsed = taskFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle"),
      fieldErrors: mapFieldErrors(parsed.error.flatten().fieldErrors),
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.updateWorkTask(
      taskId,
      {
        title: parsed.data.title.trim(),
        description: emptyToNull(parsed.data.description) ?? "",
        priority: parsed.data.priority,
      },
      options,
    );

    revalidateTaskPaths(taskId);

    return {
      status: "success",
      message: tCrm("crm.forms.result.completedDescription"),
      redirectTo: `/tasks/${taskId}`,
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/tasks/${taskId}/edit`);
  }
}

export async function completeTaskAction(
  taskId: string,
  completionNote?: string,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/tasks/${taskId}`, "tasks.manage");

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.completeWorkTask(
      taskId,
      { completionNote: emptyToNull(completionNote) },
      options,
    );

    revalidateTaskPaths(taskId);
    return { status: "success", message: "Task marked as completed." };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/tasks/${taskId}`);
  }
}

export async function reopenTaskAction(taskId: string): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/tasks/${taskId}`, "tasks.manage");

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.reopenWorkTask(taskId, options);

    revalidateTaskPaths(taskId);
    return { status: "success", message: "Task reopened." };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/tasks/${taskId}`);
  }
}

export async function assignTaskOwnerAction(taskId: string): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/tasks/${taskId}`, "tasks.manage");

  return {
    status: "error",
    message: "Owner assignment needs user picker UI before activation.",
  };
}

export async function updateTaskDueDateAction(
  taskId: string,
  dueAtUtc: string,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/tasks/${taskId}`, "tasks.edit");

  const mappedDueAt = toIsoUtc(dueAtUtc);
  if (!mappedDueAt) {
    return {
      status: "error",
      message: tCrm("crm.tasks.validation.invalidDueDate"),
      fieldErrors: { dueAtUtc: [tCrm("crm.tasks.validation.validDateTime")] },
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.updateWorkTaskDueDate(taskId, { dueAtUtc: mappedDueAt }, options);

    revalidateTaskPaths(taskId);
    return { status: "success", message: "Task due date updated." };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/tasks/${taskId}`);
  }
}

export async function updateTaskReminderAction(
  taskId: string,
  reminderAtUtc: string | null,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/tasks/${taskId}`, "tasks.edit");

  const mappedReminderAt = reminderAtUtc ? toIsoUtc(reminderAtUtc) : null;
  if (reminderAtUtc && !mappedReminderAt) {
    return {
      status: "error",
      message: tCrm("crm.tasks.validation.validDateTime"),
      fieldErrors: { reminderAtUtc: [tCrm("crm.tasks.validation.validDateTime")] },
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.updateWorkTaskReminder(taskId, { reminderAtUtc: mappedReminderAt }, options);

    revalidateTaskPaths(taskId);
    return { status: "success", message: "Task reminder updated." };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/tasks/${taskId}`);
  }
}

export async function deleteTaskAction(taskId: string): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability(`/tasks/${taskId}`, "tasks.delete");

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.deleteWorkTask(taskId, options);

    revalidatePath("/tasks");
    return {
      status: "success",
      message: "Task archived.",
      redirectTo: "/tasks",
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, `/tasks/${taskId}`);
  }
}

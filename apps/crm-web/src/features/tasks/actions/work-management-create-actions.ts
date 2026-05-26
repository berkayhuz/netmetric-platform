"use server";

import { revalidatePath } from "next/cache";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import {
  crmApiClient,
  type CreateWorkTaskRequest,
  type ScheduleMeetingRequest,
} from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import {
  meetingFormSchema,
  taskFormSchema,
  type MeetingFormInput,
  type MeetingFormValues,
  type TaskFormInput,
  type TaskFormValues,
} from "../forms/task-form-schema";

function mapZodErrors(fieldErrors: Record<string, string[] | undefined>): Record<string, string[]> {
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

function mapTaskPayload(input: TaskFormValues): CreateWorkTaskRequest | null {
  const dueAtUtc = toIsoUtc(input.dueAtUtc);
  if (!dueAtUtc) {
    return null;
  }

  return {
    title: input.title.trim(),
    description: emptyToNull(input.description) ?? "",
    ownerUserId: emptyToNull(input.ownerUserId),
    dueAtUtc,
    priority: input.priority,
  };
}

function mapMeetingPayload(input: MeetingFormValues): ScheduleMeetingRequest | null {
  const startsAtUtc = toIsoUtc(input.startsAtUtc);
  const endsAtUtc = toIsoUtc(input.endsAtUtc);
  if (!startsAtUtc || !endsAtUtc) {
    return null;
  }

  return {
    title: input.title.trim(),
    startsAtUtc,
    endsAtUtc,
    organizerEmail: (input.organizerEmail ?? "").trim(),
    attendeeSummary: emptyToNull(input.attendeeSummary) ?? "",
    requiresExternalSync: input.requiresExternalSync,
  };
}

export async function createWorkTaskAction(input: TaskFormInput): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/tasks/new", "tasks.create");

  const parsed = taskFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle"),
      fieldErrors: mapZodErrors(parsed.error.flatten().fieldErrors),
    };
  }

  const payload = mapTaskPayload(parsed.data);
  if (!payload) {
    return {
      status: "error",
      message: tCrm("crm.tasks.validation.invalidDueDate"),
      fieldErrors: { dueAtUtc: [tCrm("crm.tasks.validation.validDateTime")] },
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.createWorkTask(payload, options);

    revalidatePath("/tasks");
    revalidatePath("/work-management");

    return {
      status: "success",
      message: tCrm("crm.tasks.result.created"),
      redirectTo: "/tasks",
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/tasks/new");
  }
}

export async function scheduleWorkMeetingAction(
  input: MeetingFormInput,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/tasks/meetings/new", "tasks.create");

  const parsed = meetingFormSchema.safeParse(input);
  if (!parsed.success) {
    return {
      status: "error",
      message: tCrm("crm.forms.errors.reviewTitle"),
      fieldErrors: mapZodErrors(parsed.error.flatten().fieldErrors),
    };
  }

  const payload = mapMeetingPayload(parsed.data);
  if (!payload) {
    return {
      status: "error",
      message: tCrm("crm.meetings.validation.invalidTimeRange"),
      fieldErrors: {
        startsAtUtc: [tCrm("crm.tasks.validation.validDateTime")],
        endsAtUtc: [tCrm("crm.tasks.validation.validDateTime")],
      },
    };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.scheduleWorkMeeting(payload, options);

    revalidatePath("/tasks");
    revalidatePath("/work-management");

    return {
      status: "success",
      message: tCrm("crm.meetings.result.scheduled"),
      redirectTo: "/tasks",
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/tasks/meetings/new");
  }
}

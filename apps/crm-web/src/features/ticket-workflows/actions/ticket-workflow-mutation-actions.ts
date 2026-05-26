"use server";

import { revalidatePath } from "next/cache";

import { mapCrmMutationErrorToState } from "@/features/shared/actions/mutation-error-map";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import { isGuid } from "@/features/shared/data/guid";
import { emptyToNull } from "@/features/shared/forms/schema-primitives";
import { crmApiClient } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmActionCapability } from "@/lib/crm-auth/require-crm-action-capability";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import {
  ticketWorkflowOwnerAssignFormSchema,
  ticketWorkflowQueueAssignFormSchema,
  ticketWorkflowQueueFormSchema,
  ticketWorkflowQueueUpdateFormSchema,
  ticketWorkflowStatusChangeFormSchema,
} from "../forms/ticket-workflow-action-schema";

function revalidateTicketWorkflowPaths(ticketId?: string) {
  revalidatePath("/ticket-workflows");
  revalidatePath("/tickets");
  if (ticketId && isGuid(ticketId)) {
    revalidatePath(`/tickets/${ticketId}`);
  }
}

function invalidConfirmationState(): CrmMutationState {
  return { status: "error", message: tCrm("crm.ticketWorkflows.actions.invalidConfirmation") };
}

export async function createTicketWorkflowQueueAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/ticket-workflows", "ticketWorkflow.manage");
  const parsed = ticketWorkflowQueueFormSchema.safeParse({
    queueId: formData.get("queueId"),
    code: formData.get("code"),
    name: formData.get("name"),
    description: formData.get("description"),
    assignmentStrategy: formData.get("assignmentStrategy"),
    isDefault: formData.get("isDefault") === "true",
  });

  if (!parsed.success) return { status: "error", message: tCrm("crm.forms.errors.reviewTitle") };

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.createTicketWorkflowQueue(
      {
        code: parsed.data.code,
        name: parsed.data.name,
        description: emptyToNull(parsed.data.description),
        assignmentStrategy: parsed.data.assignmentStrategy,
        isDefault: parsed.data.isDefault,
      },
      options,
    );
    revalidateTicketWorkflowPaths();
    return { status: "success", message: tCrm("crm.ticketWorkflows.actions.queueCreated") };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/ticket-workflows");
  }
}

export async function updateTicketWorkflowQueueAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/ticket-workflows", "ticketWorkflow.manage");

  const queueId = formData.get("queueId");
  if (typeof queueId !== "string" || !isGuid(queueId)) {
    return { status: "error", message: tCrm("crm.ticketWorkflows.actions.invalidQueueId") };
  }

  const parsed = ticketWorkflowQueueUpdateFormSchema.safeParse({
    queueId,
    name: formData.get("name"),
    description: formData.get("description"),
    assignmentStrategy: formData.get("assignmentStrategy"),
    isDefault: formData.get("isDefault") === "true",
  });

  if (!parsed.success) return { status: "error", message: tCrm("crm.forms.errors.reviewTitle") };

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.updateTicketWorkflowQueue(
      queueId,
      {
        name: parsed.data.name,
        description: emptyToNull(parsed.data.description),
        assignmentStrategy: parsed.data.assignmentStrategy,
        isDefault: parsed.data.isDefault,
      },
      options,
    );
    revalidateTicketWorkflowPaths();
    return { status: "success", message: tCrm("crm.ticketWorkflows.actions.queueUpdated") };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/ticket-workflows");
  }
}

export async function deleteTicketWorkflowQueueAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/ticket-workflows", "ticketWorkflow.manage");
  if (formData.get("confirm") !== "delete-ticket-workflow-queue") return invalidConfirmationState();

  const queueId = formData.get("queueId");
  if (typeof queueId !== "string" || !isGuid(queueId)) {
    return { status: "error", message: tCrm("crm.ticketWorkflows.actions.invalidQueueId") };
  }

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.deleteTicketWorkflowQueue(queueId, options);
    revalidateTicketWorkflowPaths();
    return { status: "success", message: tCrm("crm.ticketWorkflows.actions.queueDeleted") };
  } catch (error) {
    const mapped = mapCrmMutationErrorToState(error, "/ticket-workflows");
    if (mapped.message === "The requested record no longer exists.") {
      return {
        status: "error",
        message: tCrm("crm.ticketWorkflows.actions.queueAlreadyRemoved"),
      };
    }
    return mapped;
  }
}

export async function assignTicketWorkflowQueueAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/ticket-workflows", "ticketWorkflow.manage");
  const parsed = ticketWorkflowQueueAssignFormSchema.safeParse({
    ticketId: formData.get("ticketId"),
    previousQueueId: formData.get("previousQueueId"),
    newQueueId: formData.get("newQueueId"),
    reason: formData.get("reason"),
  });

  if (!parsed.success) return { status: "error", message: tCrm("crm.forms.errors.reviewTitle") };

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.assignTicketWorkflowQueue(
      parsed.data.ticketId,
      {
        previousQueueId: emptyToNull(parsed.data.previousQueueId),
        newQueueId: parsed.data.newQueueId,
        reason: emptyToNull(parsed.data.reason),
      },
      options,
    );
    revalidateTicketWorkflowPaths(parsed.data.ticketId);
    return {
      status: "success",
      message: tCrm("crm.ticketWorkflows.actions.queueAssignmentUpdated"),
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/ticket-workflows");
  }
}

export async function assignTicketWorkflowOwnerAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/tickets", "tickets.edit");
  const parsed = ticketWorkflowOwnerAssignFormSchema.safeParse({
    ticketId: formData.get("ticketId"),
    previousOwnerUserId: formData.get("previousOwnerUserId"),
    newOwnerUserId: formData.get("newOwnerUserId"),
    queueId: formData.get("queueId"),
    reason: formData.get("reason"),
  });

  if (!parsed.success) return { status: "error", message: tCrm("crm.forms.errors.reviewTitle") };

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.assignTicketWorkflowOwner(
      parsed.data.ticketId,
      {
        previousOwnerUserId: emptyToNull(parsed.data.previousOwnerUserId),
        newOwnerUserId: parsed.data.newOwnerUserId,
        queueId: emptyToNull(parsed.data.queueId),
        reason: emptyToNull(parsed.data.reason),
      },
      options,
    );
    revalidateTicketWorkflowPaths(parsed.data.ticketId);
    return {
      status: "success",
      message: tCrm("crm.ticketWorkflows.actions.ownerAssignmentUpdated"),
    };
  } catch (error) {
    return mapCrmMutationErrorToState(error, "/ticket-workflows");
  }
}

export async function changeTicketWorkflowStatusAction(
  _previous: CrmMutationState,
  formData: FormData,
): Promise<CrmMutationState> {
  await assertSameOriginRequest();
  await requireCrmActionCapability("/tickets", "tickets.edit");
  if (formData.get("confirm") !== "change-ticket-workflow-status")
    return invalidConfirmationState();

  const parsed = ticketWorkflowStatusChangeFormSchema.safeParse({
    ticketId: formData.get("ticketId"),
    previousStatus: formData.get("previousStatus"),
    newStatus: formData.get("newStatus"),
    note: formData.get("note"),
  });

  if (!parsed.success) return { status: "error", message: tCrm("crm.forms.errors.reviewTitle") };

  try {
    const options = await getCrmApiRequestOptions();
    await crmApiClient.recordTicketWorkflowStatusChange(
      parsed.data.ticketId,
      {
        previousStatus: parsed.data.previousStatus,
        newStatus: parsed.data.newStatus,
        note: emptyToNull(parsed.data.note),
      },
      options,
    );
    revalidateTicketWorkflowPaths(parsed.data.ticketId);
    return {
      status: "success",
      message: tCrm("crm.ticketWorkflows.actions.statusTransitionRecorded"),
    };
  } catch (error) {
    const mapped = mapCrmMutationErrorToState(error, "/ticket-workflows");
    if (mapped.status === "error" && mapped.message?.includes("record no longer exists")) {
      return {
        status: "error",
        message: tCrm("crm.ticketWorkflows.actions.ticketAlreadyRemoved"),
      };
    }

    if (mapped.status === "error" && mapped.message?.includes("changed elsewhere")) {
      return {
        status: "error",
        message: tCrm("crm.ticketWorkflows.actions.stateConflict"),
      };
    }

    return mapped;
  }
}

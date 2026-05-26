"use server";

import { revalidatePath } from "next/cache";

import { AccountApiError, accountApiClient } from "@/lib/account-api";
import { getAccountApiRequestOptions } from "@/lib/auth/account-api-request-options";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import { mapMutationErrorToState } from "./mutation-error-map";
import type { MutationState } from "./mutation-state";

const guidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

function readTrimmedString(formData: FormData, key: string): string {
  const value = formData.get(key);
  return typeof value === "string" ? value.trim() : "";
}

function readOptionalString(formData: FormData, key: string): string | null {
  const value = readTrimmedString(formData, key);
  return value.length > 0 ? value : null;
}

function mapInvitationMutationError(error: unknown): MutationState {
  if (error instanceof AccountApiError && error.status === 404) {
    return {
      status: "error",
      message: "Invitation is no longer available.",
    };
  }

  return mapMutationErrorToState(error, "/settings/team");
}

export async function createInvitationAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const email = readTrimmedString(formData, "email").toLowerCase();
  const firstName = readOptionalString(formData, "firstName");
  const lastName = readOptionalString(formData, "lastName");

  if (!email) {
    return {
      status: "error",
      message: "Email is required.",
      fieldErrors: {
        email: ["Enter an email address."],
      },
    };
  }

  if (!emailPattern.test(email)) {
    return {
      status: "error",
      message: "Email format is invalid.",
      fieldErrors: {
        email: ["Enter a valid email address."],
      },
    };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.createInvitation(
      {
        email,
        firstName,
        lastName,
      },
      requestOptions,
    );
    revalidatePath("/settings/team");

    return {
      status: "success",
      message: "Invitation created successfully.",
    };
  } catch (error) {
    return mapInvitationMutationError(error);
  }
}

export async function resendInvitationAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const invitationId = readTrimmedString(formData, "invitationId");
  if (!guidPattern.test(invitationId)) {
    return {
      status: "error",
      message: "Invalid invitation reference.",
    };
  }

  const confirm = readTrimmedString(formData, "confirm");
  if (confirm !== "resend-invitation") {
    return {
      status: "error",
      message: "Please confirm before resending this invitation.",
    };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.resendInvitation(invitationId, requestOptions);
    revalidatePath("/settings/team");
    return {
      status: "success",
      message: "Invitation resent successfully.",
    };
  } catch (error) {
    return mapInvitationMutationError(error);
  }
}

export async function revokeInvitationAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const invitationId = readTrimmedString(formData, "invitationId");
  if (!guidPattern.test(invitationId)) {
    return {
      status: "error",
      message: "Invalid invitation reference.",
    };
  }

  const confirm = readTrimmedString(formData, "confirm");
  if (confirm !== "revoke-invitation") {
    return {
      status: "error",
      message: "Please confirm before revoking this invitation.",
    };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.revokeInvitation(invitationId, requestOptions);
    revalidatePath("/settings/team");
    return {
      status: "success",
      message: "Invitation revoked successfully.",
    };
  } catch (error) {
    return mapInvitationMutationError(error);
  }
}

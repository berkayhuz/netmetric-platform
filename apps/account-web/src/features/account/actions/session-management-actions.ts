"use server";

import { revalidatePath } from "next/cache";
import { AccountApiError, accountApiClient } from "@/lib/account-api";
import { getAccountApiRequestOptions } from "@/lib/auth/account-api-request-options";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import { mapMutationErrorToState } from "./mutation-error-map";
import type { MutationState } from "./mutation-state";

const guidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

function isValidGuid(value: string): boolean {
  return guidPattern.test(value);
}

function readRequiredString(formData: FormData, key: string): string {
  const raw = formData.get(key);
  return typeof raw === "string" ? raw.trim() : "";
}

function mapSessionMutationError(error: unknown, returnPath: string): MutationState {
  if (error instanceof AccountApiError && error.status === 404) {
    return {
      status: "error",
      message: "The session or device was already removed.",
    };
  }

  return mapMutationErrorToState(error, returnPath);
}

export async function revokeSessionAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const sessionId = readRequiredString(formData, "sessionId");
  if (!isValidGuid(sessionId)) {
    return {
      status: "error",
      message: "Invalid session reference.",
    };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.revokeSession(sessionId, requestOptions);
    revalidatePath("/");
    revalidatePath("/security");
    revalidatePath("/security/sessions");
    return {
      status: "success",
      message: "Session revoked successfully.",
    };
  } catch (error) {
    return mapSessionMutationError(error, "/security/sessions");
  }
}

export async function revokeOtherSessionsAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const confirm = readRequiredString(formData, "confirm");
  if (confirm !== "revoke-others") {
    return {
      status: "error",
      message: "Please confirm before revoking other sessions.",
    };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.revokeOtherSessions(requestOptions);
    revalidatePath("/");
    revalidatePath("/security");
    revalidatePath("/security/sessions");
    return {
      status: "success",
      message: "Other sessions were revoked successfully.",
    };
  } catch (error) {
    return mapSessionMutationError(error, "/security/sessions");
  }
}

export async function revokeTrustedDeviceAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const deviceId = readRequiredString(formData, "deviceId");
  if (!isValidGuid(deviceId)) {
    return {
      status: "error",
      message: "Invalid device reference.",
    };
  }

  const confirm = readRequiredString(formData, "confirm");
  if (confirm !== "revoke-device") {
    return {
      status: "error",
      message: "Please confirm before revoking this trusted device.",
    };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.revokeTrustedDevice(deviceId, requestOptions);
    revalidatePath("/security");
    revalidatePath("/security/sessions");
    return {
      status: "success",
      message: "Trusted device revoked successfully.",
    };
  } catch (error) {
    return mapSessionMutationError(error, "/security/sessions");
  }
}

export async function revokeOtherTrustedDevicesAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();
  const confirm = readRequiredString(formData, "confirm");
  if (confirm !== "revoke-other-devices") {
    return {
      status: "error",
      message: "Please confirm before revoking other trusted devices.",
    };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.revokeOtherTrustedDevices(requestOptions);
    revalidatePath("/security");
    revalidatePath("/security/sessions");
    return {
      status: "success",
      message: "Other trusted devices were revoked successfully.",
    };
  } catch (error) {
    return mapSessionMutationError(error, "/security/sessions");
  }
}

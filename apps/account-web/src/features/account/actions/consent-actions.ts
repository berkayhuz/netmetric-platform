"use server";

import { revalidatePath } from "next/cache";

import { AccountApiError, accountApiClient } from "@/lib/account-api";
import { getAccountApiRequestOptions } from "@/lib/auth/account-api-request-options";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import { mapMutationErrorToState } from "./mutation-error-map";
import type { MutationState } from "./mutation-state";

const consentTypePattern = /^[a-z0-9][a-z0-9._-]{1,63}$/i;

function readTrimmedString(formData: FormData, key: string): string {
  const value = formData.get(key);
  return typeof value === "string" ? value.trim() : "";
}

function mapConsentMutationError(error: unknown): MutationState {
  if (error instanceof AccountApiError && error.status === 404) {
    return {
      status: "error",
      message: "This consent is no longer available.",
    };
  }

  return mapMutationErrorToState(error, "/privacy");
}

export async function acceptConsentAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const confirm = readTrimmedString(formData, "confirm");
  if (confirm !== "accept-consent") {
    return {
      status: "error",
      message: "Please confirm before accepting this consent.",
    };
  }

  const consentType = readTrimmedString(formData, "consentType");
  const version = readTrimmedString(formData, "version");

  if (!consentTypePattern.test(consentType)) {
    return {
      status: "error",
      message: "Invalid consent reference.",
    };
  }

  if (!version) {
    return {
      status: "error",
      message: "Consent version is required.",
      fieldErrors: {
        version: ["Consent version is required."],
      },
    };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.acceptConsent(consentType, { version }, requestOptions);
    revalidatePath("/privacy");
    revalidatePath("/settings");
    return {
      status: "success",
      message: "Consent accepted successfully.",
    };
  } catch (error) {
    return mapConsentMutationError(error);
  }
}

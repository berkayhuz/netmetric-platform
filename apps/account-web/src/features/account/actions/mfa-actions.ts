"use server";

import { revalidatePath } from "next/cache";

import { accountApiClient } from "@/lib/account-api";
import { getAccountApiRequestOptions } from "@/lib/auth/account-api-request-options";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import { mapMutationErrorToState } from "./mutation-error-map";
import type { MutationState } from "./mutation-state";

function readRequiredTrimmedString(formData: FormData, key: string): string {
  const value = formData.get(key);
  return typeof value === "string" ? value.trim() : "";
}

export async function setupMfaAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const confirm = readRequiredTrimmedString(formData, "confirm");
  if (confirm !== "setup-mfa") {
    return {
      status: "error",
      message: "Please confirm MFA setup before continuing.",
    };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    const response = await accountApiClient.setupMfa(requestOptions);

    return {
      status: "success",
      message: "MFA setup initiated. Confirm with your authenticator code.",
      data: {
        setup: {
          sharedKey: response.sharedKey,
          authenticatorUri: response.authenticatorUri,
        },
      },
    };
  } catch (error) {
    return mapMutationErrorToState(error, "/security/mfa");
  }
}

export async function confirmMfaAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const verificationCode = readRequiredTrimmedString(formData, "verificationCode");
  if (!verificationCode) {
    return {
      status: "error",
      message: "Verification code is required.",
      fieldErrors: {
        verificationCode: ["Enter the verification code."],
      },
    };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    const response = await accountApiClient.confirmMfa({ verificationCode }, requestOptions);

    revalidatePath("/");
    revalidatePath("/security");
    revalidatePath("/security/mfa");

    return {
      status: "success",
      message: "MFA enabled successfully.",
      data: {
        mfaEnabled: response.isEnabled,
        recoveryCodes: response.recoveryCodes,
      },
    };
  } catch (error) {
    return mapMutationErrorToState(error, "/security/mfa");
  }
}

export async function disableMfaAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const confirm = readRequiredTrimmedString(formData, "confirm");
  if (confirm !== "disable-mfa") {
    return {
      status: "error",
      message: "Please confirm MFA disable action.",
    };
  }

  const verificationCode = readRequiredTrimmedString(formData, "verificationCode");
  if (!verificationCode) {
    return {
      status: "error",
      message: "Verification code is required to disable MFA.",
      fieldErrors: {
        verificationCode: ["Enter your current authenticator code."],
      },
    };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.disableMfa({ verificationCode }, requestOptions);

    revalidatePath("/");
    revalidatePath("/security");
    revalidatePath("/security/mfa");

    return {
      status: "success",
      message: "MFA disabled successfully.",
    };
  } catch (error) {
    return mapMutationErrorToState(error, "/security/mfa");
  }
}

export async function regenerateRecoveryCodesAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const confirm = readRequiredTrimmedString(formData, "confirm");
  if (confirm !== "regenerate-recovery-codes") {
    return {
      status: "error",
      message: "Please confirm recovery code regeneration.",
    };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    const response = await accountApiClient.regenerateMfaRecoveryCodes(requestOptions);

    revalidatePath("/security");
    revalidatePath("/security/mfa");

    return {
      status: "success",
      message: "Recovery codes regenerated. Save these codes now.",
      data: {
        recoveryCodes: response.recoveryCodes,
      },
    };
  } catch (error) {
    return mapMutationErrorToState(error, "/security/mfa");
  }
}

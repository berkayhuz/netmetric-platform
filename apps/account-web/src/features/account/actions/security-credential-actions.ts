"use server";

import { revalidatePath } from "next/cache";

import { accountApiClient } from "@/lib/account-api";
import { getAccountApiRequestOptions } from "@/lib/auth/account-api-request-options";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import { mapMutationErrorToState } from "./mutation-error-map";
import type { MutationState } from "./mutation-state";

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

function readRequiredString(formData: FormData, key: string): string {
  const value = formData.get(key);
  return typeof value === "string" ? value : "";
}

function readRequiredTrimmedString(formData: FormData, key: string): string {
  return readRequiredString(formData, key).trim();
}

export async function changePasswordAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const currentPassword = readRequiredString(formData, "currentPassword");
  const newPassword = readRequiredString(formData, "newPassword");
  const confirmNewPassword = readRequiredString(formData, "confirmNewPassword");

  if (!currentPassword || !newPassword || !confirmNewPassword) {
    return {
      status: "error",
      message: "Please complete all password fields.",
    };
  }

  if (newPassword !== confirmNewPassword) {
    return {
      status: "error",
      message: "New password and confirmation do not match.",
      fieldErrors: {
        confirmNewPassword: ["Passwords must match."],
      },
    };
  }

  if (newPassword === currentPassword) {
    return {
      status: "error",
      message: "New password must be different from the current password.",
      fieldErrors: {
        newPassword: ["Enter a different password."],
      },
    };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.changePassword(
      {
        currentPassword,
        newPassword,
        confirmNewPassword,
      },
      requestOptions,
    );

    revalidatePath("/");
    revalidatePath("/security");
    revalidatePath("/security/password");

    return {
      status: "success",
      message: "Password changed successfully.",
    };
  } catch (error) {
    return mapMutationErrorToState(error, "/security/password");
  }
}

export async function requestEmailChangeAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const newEmail = readRequiredTrimmedString(formData, "newEmail").toLowerCase();
  const currentPassword = readRequiredString(formData, "currentPassword");

  if (!newEmail || !currentPassword) {
    return {
      status: "error",
      message: "Please enter the new email and current password.",
    };
  }

  if (!emailPattern.test(newEmail)) {
    return {
      status: "error",
      message: "Enter a valid email address.",
      fieldErrors: {
        newEmail: ["Email format is invalid."],
      },
    };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    const response = await accountApiClient.requestEmailChange(
      {
        newEmail,
        currentPassword,
      },
      requestOptions,
    );

    revalidatePath("/security");
    revalidatePath("/security/password");

    return {
      status: "success",
      message: response.confirmationRequired
        ? "Email change requested. Confirm the change from your verification link."
        : "Email change request submitted.",
    };
  } catch (error) {
    return mapMutationErrorToState(error, "/security/password");
  }
}

export async function confirmEmailChangeAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const token = readRequiredTrimmedString(formData, "token");
  if (!token) {
    return {
      status: "error",
      message: "Confirmation token is required.",
      fieldErrors: {
        token: ["Confirmation token is required."],
      },
    };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    const response = await accountApiClient.confirmEmailChange({ token }, requestOptions);

    revalidatePath("/security");
    revalidatePath("/security/password");
    revalidatePath("/profile");

    return {
      status: "success",
      message: `Email changed successfully to ${response.newEmail}.`,
    };
  } catch (error) {
    return mapMutationErrorToState(error, "/security/password");
  }
}

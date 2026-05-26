"use server";

import { revalidatePath } from "next/cache";

import { AccountApiError, accountApiClient } from "@/lib/account-api";
import { getAccountApiRequestOptions } from "@/lib/auth/account-api-request-options";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import { mapMutationErrorToState } from "./mutation-error-map";
import type { MutationState } from "./mutation-state";

const guidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

function readTrimmedString(formData: FormData, key: string): string {
  const value = formData.get(key);
  return typeof value === "string" ? value.trim() : "";
}

function parseRoleList(value: string): string[] {
  if (!value) {
    return [];
  }

  return value
    .split(",")
    .map((part) => part.trim())
    .filter((part) => part.length > 0);
}

function mapMemberRoleMutationError(error: unknown): MutationState {
  if (error instanceof AccountApiError && error.status === 404) {
    return {
      status: "error",
      message: "Member is no longer available.",
    };
  }

  return mapMutationErrorToState(error, "/settings/team");
}

export async function updateMemberRolesAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const confirm = readTrimmedString(formData, "confirm");
  if (confirm !== "update-member-roles") {
    return {
      status: "error",
      message: "Please confirm before updating member roles.",
    };
  }

  const userId = readTrimmedString(formData, "userId");
  if (!guidPattern.test(userId)) {
    return {
      status: "error",
      message: "Invalid member reference.",
    };
  }

  const allowedRoles = new Set(parseRoleList(readTrimmedString(formData, "allowedRoles")));
  if (allowedRoles.size === 0) {
    return {
      status: "error",
      message: "Role catalog is not available for updates.",
    };
  }

  const protectedRoles = new Set(parseRoleList(readTrimmedString(formData, "protectedRoles")));
  const currentRoles = new Set(parseRoleList(readTrimmedString(formData, "currentRoles")));

  const selectedRoles = formData
    .getAll("roles")
    .filter((value): value is string => typeof value === "string")
    .map((value) => value.trim())
    .filter((value) => value.length > 0);

  if (selectedRoles.length === 0) {
    return {
      status: "error",
      message: "Select at least one role.",
      fieldErrors: {
        roles: ["Select at least one role."],
      },
    };
  }

  if (selectedRoles.some((role) => !allowedRoles.has(role))) {
    return {
      status: "error",
      message: "One or more selected roles are invalid.",
    };
  }

  for (const role of protectedRoles) {
    if (currentRoles.has(role) && !selectedRoles.includes(role)) {
      return {
        status: "error",
        message: "Protected roles cannot be removed from this member.",
      };
    }
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.updateMemberRoles(
      userId,
      {
        roles: selectedRoles,
      },
      requestOptions,
    );
    revalidatePath("/settings/team");
    revalidatePath("/settings");
    return {
      status: "success",
      message: "Member roles updated successfully.",
    };
  } catch (error) {
    return mapMemberRoleMutationError(error);
  }
}

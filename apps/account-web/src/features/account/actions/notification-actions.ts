"use server";

import { revalidatePath } from "next/cache";

import {
  AccountApiError,
  accountApiClient,
  type UpdateNotificationPreferenceItemRequest,
} from "@/lib/account-api";
import { getAccountApiRequestOptions } from "@/lib/auth/account-api-request-options";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import { mapMutationErrorToState } from "./mutation-error-map";
import type { MutationState } from "./mutation-state";

const guidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

function readTrimmedString(formData: FormData, key: string): string {
  const value = formData.get(key);
  return typeof value === "string" ? value.trim() : "";
}

function isValidGuid(value: string): boolean {
  return guidPattern.test(value);
}

function mapNotificationMutationError(error: unknown): MutationState {
  if (error instanceof AccountApiError && error.status === 404) {
    return {
      status: "error",
      message: "Notification was already removed or is no longer available.",
    };
  }

  return mapMutationErrorToState(error, "/notifications");
}

export async function markNotificationAsReadAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const notificationId = readTrimmedString(formData, "notificationId");
  if (!isValidGuid(notificationId)) {
    return { status: "error", message: "Invalid notification reference." };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.markNotificationAsRead(notificationId, requestOptions);
    revalidatePath("/");
    revalidatePath("/notifications");
    return { status: "success", message: "Notification marked as read." };
  } catch (error) {
    return mapNotificationMutationError(error);
  }
}

export async function markAllNotificationsAsReadAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const confirm = readTrimmedString(formData, "confirm");
  if (confirm !== "mark-all-read") {
    return { status: "error", message: "Please confirm before marking all notifications as read." };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.markAllNotificationsAsRead(requestOptions);
    revalidatePath("/");
    revalidatePath("/notifications");
    return { status: "success", message: "All notifications marked as read." };
  } catch (error) {
    return mapNotificationMutationError(error);
  }
}

export async function deleteNotificationAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const notificationId = readTrimmedString(formData, "notificationId");
  if (!isValidGuid(notificationId)) {
    return { status: "error", message: "Invalid notification reference." };
  }

  const confirm = readTrimmedString(formData, "confirm");
  if (confirm !== "delete-notification") {
    return { status: "error", message: "Please confirm before removing this notification." };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.deleteNotification(notificationId, requestOptions);
    revalidatePath("/");
    revalidatePath("/notifications");
    return { status: "success", message: "Notification removed." };
  } catch (error) {
    return mapNotificationMutationError(error);
  }
}

export async function updateNotificationPreferencesAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const entries: UpdateNotificationPreferenceItemRequest[] = [];
  for (const [key, value] of formData.entries()) {
    if (!key.startsWith("pref:")) {
      continue;
    }

    if (typeof value !== "string") {
      continue;
    }

    const [channel, category] = value.split("|");
    if (!channel || !category) {
      continue;
    }

    const enabled = formData.get(`enabled:${channel}|${category}`) === "on";
    entries.push({ channel, category, isEnabled: enabled });
  }

  if (entries.length === 0) {
    return {
      status: "error",
      message: "No notification preferences were submitted.",
    };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.updateNotificationPreferences({ items: entries }, requestOptions);
    revalidatePath("/");
    revalidatePath("/notifications");
    revalidatePath("/preferences");
    return {
      status: "success",
      message: "Notification preferences updated.",
    };
  } catch (error) {
    return mapMutationErrorToState(error, "/notifications");
  }
}

"use server";

import { revalidatePath } from "next/cache";
import { cookies } from "next/headers";
import {
  isSupportedLanguageCode,
  resolveSupportedLanguageCode,
  UI_DATE_FORMAT_COOKIE_NAME,
  UI_LOCALE_COOKIE_NAME,
  UI_POST_LOGIN_REDIRECT_COOKIE_NAME,
  UI_THEME_COOKIE_NAME,
  UI_TIME_ZONE_COOKIE_NAME,
} from "@netmetric/i18n";

import { accountApiClient, type UpdateUserPreferenceRequest } from "@/lib/account-api";
import { getAccountApiRequestOptions } from "@/lib/auth/account-api-request-options";
import { resolveLocaleCookieOptions } from "@/lib/locale-cookie";
import {
  resolvePostLoginDestination,
  resolvePostLoginDestinationUrl,
} from "@/lib/post-login-destination";
import { resolvePreferenceCookiesFromPayload } from "@/lib/ui-preference-cookies";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import { mapMutationErrorToState } from "./mutation-error-map";
import type { MutationState } from "./mutation-state";

const faviconUploadLimitBytes = 1024 * 1024;
const allowedFaviconMimeTypes = new Set([
  "image/png",
  "image/svg+xml",
  "image/x-icon",
  "image/vnd.microsoft.icon",
]);

function readOptionalString(formData: FormData, key: string): string | null {
  const value = formData.get(key);
  if (typeof value !== "string") {
    return null;
  }

  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : null;
}

function readRequiredString(formData: FormData, key: string): string {
  const value = readOptionalString(formData, key);
  return value ?? "";
}

export async function updatePreferencesAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();
  const requestedLanguage = readRequiredString(formData, "language");
  if (!isSupportedLanguageCode(requestedLanguage)) {
    return {
      status: "error",
      message: "Selected language is not supported by this frontend build.",
      fieldErrors: {
        language: ["Selected language is not supported."],
      },
    };
  }

  const payload: UpdateUserPreferenceRequest = {
    theme: readRequiredString(formData, "theme"),
    language: resolveSupportedLanguageCode(requestedLanguage),
    timeZone: readRequiredString(formData, "timeZone"),
    dateFormat: readRequiredString(formData, "dateFormat"),
    postLoginDestination: resolvePostLoginDestination(
      readRequiredString(formData, "postLoginDestination"),
    ),
    defaultOrganizationId: readOptionalString(formData, "defaultOrganizationId"),
    version: readOptionalString(formData, "version"),
  };

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.updatePreferences(payload, requestOptions);
    const cookieStore = await cookies();
    const cookieOptions = resolveLocaleCookieOptions();
    const cookieValues = resolvePreferenceCookiesFromPayload(payload);
    cookieStore.set(UI_LOCALE_COOKIE_NAME, cookieValues.locale, cookieOptions);
    cookieStore.set(UI_THEME_COOKIE_NAME, cookieValues.theme, cookieOptions);
    cookieStore.set(UI_TIME_ZONE_COOKIE_NAME, cookieValues.timeZone, cookieOptions);
    cookieStore.set(UI_DATE_FORMAT_COOKIE_NAME, cookieValues.dateFormat, cookieOptions);
    cookieStore.set(
      UI_POST_LOGIN_REDIRECT_COOKIE_NAME,
      resolvePostLoginDestinationUrl(payload.postLoginDestination),
      cookieOptions,
    );
    revalidatePath("/preferences");
    revalidatePath("/settings");

    return {
      status: "success",
      message: "Preferences updated successfully.",
    };
  } catch (error) {
    return mapMutationErrorToState(error, "/preferences");
  }
}

export async function uploadFaviconAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const fileValue = formData.get("faviconFile");
  if (!(fileValue instanceof File)) {
    return {
      status: "error",
      message: "Favicon file is required.",
      fieldErrors: {
        faviconFile: ["Select an ICO, PNG, or SVG file."],
      },
    };
  }

  if (fileValue.size <= 0) {
    return {
      status: "error",
      message: "Favicon file is empty.",
      fieldErrors: {
        faviconFile: ["Select a non-empty favicon file."],
      },
    };
  }

  if (fileValue.size > faviconUploadLimitBytes) {
    return {
      status: "error",
      message: "Favicon file is too large. Maximum size is 1 MB.",
      fieldErrors: {
        faviconFile: ["Maximum file size is 1 MB."],
      },
    };
  }

  if (!allowedFaviconMimeTypes.has(fileValue.type)) {
    return {
      status: "error",
      message: "Unsupported favicon file type. Use ICO, PNG, or SVG.",
      fieldErrors: {
        faviconFile: ["Supported types: ICO, PNG, SVG."],
      },
    };
  }

  const uploadFormData = new FormData();
  uploadFormData.set("file", fileValue);

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.uploadPreferencesFavicon(uploadFormData, requestOptions);
    revalidatePath("/preferences");
    revalidatePath("/");

    return {
      status: "success",
      message: "Favicon uploaded successfully.",
    };
  } catch (error) {
    return mapMutationErrorToState(error, "/preferences");
  }
}

export async function removeFaviconAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const confirm = readRequiredString(formData, "confirm");
  if (confirm !== "delete-favicon") {
    return {
      status: "error",
      message: "Please confirm before deleting the favicon.",
    };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.removePreferencesFavicon(requestOptions);
    revalidatePath("/preferences");
    revalidatePath("/");

    return {
      status: "success",
      message: "Favicon removed successfully.",
    };
  } catch (error) {
    return mapMutationErrorToState(error, "/preferences");
  }
}

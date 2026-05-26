"use server";

import { revalidatePath } from "next/cache";
import { cookies } from "next/headers";
import {
  isSupportedLanguageCode,
  resolveLocale,
  resolveSupportedLanguageCode,
  translate,
  UI_LOCALE_COOKIE_NAME,
} from "@netmetric/i18n";

import { AccountApiError, accountApiClient, type UpdateMyProfileRequest } from "@/lib/account-api";
import { getAccountApiRequestOptions } from "@/lib/auth/account-api-request-options";
import { assertSameOriginRequest } from "@/lib/security/csrf";

import { mapMutationErrorToState } from "./mutation-error-map";
import type { MutationState } from "./mutation-state";

const avatarUploadLimitBytes = 10 * 1024 * 1024;
const allowedAvatarMimeTypes = new Set(["image/png", "image/jpeg", "image/webp"]);

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

function mapAvatarMutationError(error: unknown): MutationState {
  if (error instanceof AccountApiError) {
    if (error.status === 404) {
      return {
        status: "error",
        message: "Avatar was already removed.",
      };
    }

    if (error.status === 413) {
      return {
        status: "error",
        message: "Avatar file is too large. Maximum size is 10 MB.",
      };
    }

    if (error.status === 415) {
      return {
        status: "error",
        message: "Unsupported avatar file type. Use PNG, JPEG, or WEBP.",
      };
    }
  }

  return mapMutationErrorToState(error, "/profile");
}

export async function updateProfileAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();
  const cookieStore = await cookies();
  const locale = resolveLocale(cookieStore.get(UI_LOCALE_COOKIE_NAME)?.value);
  const requestedCulture = readRequiredString(formData, "culture");

  if (!isSupportedLanguageCode(requestedCulture)) {
    return {
      status: "error",
      message: "Selected language is not supported by this frontend build.",
      fieldErrors: {
        culture: ["Selected language is not supported."],
      },
    };
  }

  const payload: UpdateMyProfileRequest = {
    firstName: readRequiredString(formData, "firstName"),
    lastName: readRequiredString(formData, "lastName"),
    phoneCountryIso2: readOptionalString(formData, "phoneCountryIso2"),
    phoneNationalNumber: readOptionalString(formData, "phoneNationalNumber"),
    jobTitle: readOptionalString(formData, "jobTitle"),
    department: readOptionalString(formData, "department"),
    timeZone: readRequiredString(formData, "timeZone"),
    culture: resolveSupportedLanguageCode(requestedCulture),
    version: readOptionalString(formData, "version"),
  };

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.updateProfile(payload, requestOptions);
    revalidatePath("/profile");
    revalidatePath("/settings");
    revalidatePath("/");

    return {
      status: "success",
      message: translate("account.profile.updated.description", { locale }),
    };
  } catch (error) {
    return mapMutationErrorToState(error, "/profile");
  }
}

export async function uploadAvatarAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const fileValue = formData.get("avatarFile");
  if (!(fileValue instanceof File)) {
    return {
      status: "error",
      message: "Avatar file is required.",
      fieldErrors: {
        avatarFile: ["Select an avatar image file."],
      },
    };
  }

  if (fileValue.size <= 0) {
    return {
      status: "error",
      message: "Avatar file is empty.",
      fieldErrors: {
        avatarFile: ["Select a non-empty avatar file."],
      },
    };
  }

  if (fileValue.size > avatarUploadLimitBytes) {
    return {
      status: "error",
      message: "Avatar file is too large. Maximum size is 10 MB.",
      fieldErrors: {
        avatarFile: ["Maximum file size is 10 MB."],
      },
    };
  }

  if (!allowedAvatarMimeTypes.has(fileValue.type)) {
    return {
      status: "error",
      message: "Unsupported avatar file type. Use PNG, JPEG, or WEBP.",
      fieldErrors: {
        avatarFile: ["Supported types: PNG, JPEG, WEBP."],
      },
    };
  }

  const uploadFormData = new FormData();
  uploadFormData.set("file", fileValue);

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.uploadProfileAvatar(uploadFormData, requestOptions);
    revalidatePath("/profile");
    revalidatePath("/settings");
    revalidatePath("/");

    return {
      status: "success",
      message: "Avatar uploaded successfully.",
    };
  } catch (error) {
    return mapAvatarMutationError(error);
  }
}

export async function removeAvatarAction(
  _previous: MutationState,
  formData: FormData,
): Promise<MutationState> {
  await assertSameOriginRequest();

  const confirm = readRequiredString(formData, "confirm");
  if (confirm !== "delete-avatar") {
    return {
      status: "error",
      message: "Please confirm before deleting the avatar.",
    };
  }

  try {
    const requestOptions = await getAccountApiRequestOptions();
    await accountApiClient.removeProfileAvatar(requestOptions);
    revalidatePath("/profile");
    revalidatePath("/settings");
    revalidatePath("/");

    return {
      status: "success",
      message: "Avatar removed successfully.",
    };
  } catch (error) {
    return mapAvatarMutationError(error);
  }
}

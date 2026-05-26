import "server-only";

import { cache } from "react";

import {
  accountApiClient,
  type AccountOptionsResponse,
  type MyProfileResponse,
  type UserPreferenceResponse,
} from "@/lib/account-api";
import type { AccountDateSettings } from "@/lib/account-date";
import { getAccountApiRequestOptions } from "@/lib/auth/account-api-request-options";
import { getCurrentAccountSession } from "@/lib/auth/account-session";
import { getSupportedLanguageOptions } from "@/lib/supported-language-options";

export const getOverviewForPage = cache(async () => {
  const session = await getCurrentAccountSession();
  if (!session.authenticated) {
    throw new Error("Authentication required.");
  }

  return session.overview;
});

export const getProfileForPage = cache(async (): Promise<MyProfileResponse> => {
  const requestOptions = await getAccountApiRequestOptions();
  return accountApiClient.getProfile(requestOptions);
});

export const getPreferencesForPage = cache(async (): Promise<UserPreferenceResponse> => {
  const requestOptions = await getAccountApiRequestOptions();
  return accountApiClient.getPreferences(requestOptions);
});

export const getAccountOptionsForPage = cache(async (): Promise<AccountOptionsResponse> => {
  const requestOptions = await getAccountApiRequestOptions();
  const options = await accountApiClient.getOptions(requestOptions);
  return {
    ...options,
    languages: getSupportedLanguageOptions(),
  };
});

export const getAccountDateSettingsForPage = cache(async (): Promise<AccountDateSettings> => {
  const preferences = await getPreferencesForPage();
  return {
    locale: preferences.language,
    timeZone: preferences.timeZone,
    dateFormat: preferences.dateFormat,
  };
});

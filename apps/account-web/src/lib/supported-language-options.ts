import { getSupportedLanguages } from "@netmetric/i18n";

import type { AccountOptionItem } from "./account-api";

let cachedSupportedLanguageOptions: AccountOptionItem[] | null = null;

export function getSupportedLanguageOptions(): AccountOptionItem[] {
  cachedSupportedLanguageOptions ??= getSupportedLanguages().map((language) => ({
    value: language.code,
    label: `${language.englishName} (${language.code})`,
  }));

  return cachedSupportedLanguageOptions;
}

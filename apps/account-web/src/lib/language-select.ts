import { canonicalizeLocale } from "@netmetric/i18n";

import type { AccountOptionItem } from "./account-api";

export function resolveLanguageSelectState(
  savedLocale: string | null | undefined,
  inputOptions: AccountOptionItem[],
): {
  selectedValue: string;
  options: AccountOptionItem[];
} {
  const fallbackLocale = inputOptions[0]?.value ?? "en-US";
  const canonicalSaved = canonicalizeLocale(savedLocale);

  const options = [...inputOptions];
  const optionMap = new Map(options.map((option) => [option.value.toLowerCase(), option]));

  const findSelectedValue = (locale: string): string | null => {
    const normalized = locale.toLowerCase();
    const exact = optionMap.get(normalized);
    if (exact) {
      return exact.value;
    }

    const base = normalized.split("-")[0];
    if (!base) {
      return null;
    }

    const baseMatch = optionMap.get(base);
    if (baseMatch) {
      return baseMatch.value;
    }

    const partial = options.find((option) => option.value.toLowerCase().startsWith(`${base}-`));
    return partial?.value ?? null;
  };

  if (!canonicalSaved) {
    const fallbackSelected = findSelectedValue(fallbackLocale);
    if (fallbackSelected) {
      return { selectedValue: fallbackSelected, options };
    }

    return { selectedValue: fallbackLocale, options };
  }

  const selected = findSelectedValue(canonicalSaved);
  if (selected) {
    return { selectedValue: selected, options };
  }

  return { selectedValue: fallbackLocale, options };
}

import { translate, type MessageKey } from "@netmetric/i18n";

function getClientLocale(): string | null {
  if (typeof document === "undefined") {
    return null;
  }

  return document.documentElement.lang || null;
}

export function tAccount(
  key: string,
  localeInput?: string | null,
  params?: Record<string, string | number>,
): string {
  return params
    ? translate(key as MessageKey, { locale: localeInput, params })
    : translate(key as MessageKey, { locale: localeInput });
}

export function tAccountClient(key: string, params?: Record<string, string | number>): string {
  return tAccount(key, getClientLocale(), params);
}

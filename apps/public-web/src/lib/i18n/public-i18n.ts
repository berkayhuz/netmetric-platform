import { translate, type MessageKey } from "@netmetric/i18n";

export function tPublic(
  key: string,
  localeInput?: string | null,
  params?: Record<string, string | number>,
): string {
  return params
    ? translate(key as MessageKey, { locale: localeInput, params })
    : translate(key as MessageKey, { locale: localeInput });
}

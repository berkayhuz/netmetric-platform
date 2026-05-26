import type { Metadata } from "next";

import type { Locale, MessageKey } from "@netmetric/i18n";

import { getTranslator } from "./auth-i18n.shared";

const appTitleKey: MessageKey = "meta.appTitle";

export function createAuthMetadata(locale: Locale, titleKey: MessageKey): Metadata {
  const t = getTranslator(locale);
  const appTitle = t(appTitleKey);

  return {
    title: `${t(titleKey)} | ${appTitle}`,
    description: t("meta.appDescription"),
  };
}

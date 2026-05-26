import type { MessageKey } from "@netmetric/i18n";

import { authRoutes, externalRoutes } from "./auth-routes";

export type AuthNavLink = {
  labelKey: MessageKey;
  href: string;
  external?: boolean;
};

export const authHeaderLinks: readonly AuthNavLink[] = [
  { labelKey: "nav.login", href: authRoutes.login },
  { labelKey: "nav.register", href: authRoutes.register },
];

export const authFooterLinks: readonly AuthNavLink[] = [
  { labelKey: "nav.privacy", href: externalRoutes.privacy, external: true },
  { labelKey: "nav.terms", href: externalRoutes.terms, external: true },
  { labelKey: "nav.support", href: externalRoutes.support, external: true },
];

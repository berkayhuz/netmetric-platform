import { tPublic } from "@/lib/i18n/public-i18n";

const primaryNavDefinitions = [
  { href: "/product", key: "public.nav.product" },
  { href: "/crm", key: "public.nav.crm" },
  { href: "/tools", key: "public.nav.tools" },
  { href: "/developers", key: "public.nav.developers" },
  { href: "/security", key: "public.nav.security" },
  { href: "/pricing", key: "public.nav.pricing" },
] as const;

const companyLinkDefinitions = [
  { href: "/about", key: "public.nav.about" },
  { href: "/contact", key: "public.nav.contact" },
  { href: "/status", key: "public.nav.status" },
] as const;

const legalLinkDefinitions = [
  { href: "/privacy", key: "public.nav.privacy" },
  { href: "/terms", key: "public.nav.terms" },
  { href: "/cookies", key: "public.nav.cookies" },
] as const;

function localizeLinks<TLink extends { href: string; key: string }>(
  links: readonly TLink[],
  locale?: string | null,
) {
  return links.map((link) => ({
    href: link.href,
    label: tPublic(link.key, locale),
  }));
}

export function getPrimaryNavLinks(locale?: string | null) {
  return localizeLinks(primaryNavDefinitions, locale);
}

export function getCompanyLinks(locale?: string | null) {
  return localizeLinks(companyLinkDefinitions, locale);
}

export function getLegalLinks(locale?: string | null) {
  return localizeLinks(legalLinkDefinitions, locale);
}

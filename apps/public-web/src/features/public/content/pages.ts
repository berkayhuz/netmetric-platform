import { tPublic } from "@/lib/i18n/public-i18n";

export type PublicPageContent = {
  badge?: string;
  title: string;
  lead: string;
  description: string;
  highlights: Array<{ title: string; description: string }>;
};

export const publicPageKeys = [
  "home",
  "product",
  "crm",
  "tools",
  "developers",
  "security",
  "pricing",
  "about",
  "contact",
  "status",
  "privacy",
  "terms",
  "cookies",
] as const;

export type PublicPageKey = (typeof publicPageKeys)[number];

export function getPageContent(key: PublicPageKey, locale?: string | null): PublicPageContent {
  const prefix = `public.pages.${key}`;
  const badge = tPublic(`${prefix}.badge`, locale);

  return {
    ...(badge === `${prefix}.badge` ? {} : { badge }),
    title: tPublic(`${prefix}.title`, locale),
    lead: tPublic(`${prefix}.lead`, locale),
    description: tPublic(`${prefix}.description`, locale),
    highlights: [0, 1, 2].map((index) => ({
      title: tPublic(`${prefix}.highlights.${index}.title`, locale),
      description: tPublic(`${prefix}.highlights.${index}.description`, locale),
    })),
  };
}

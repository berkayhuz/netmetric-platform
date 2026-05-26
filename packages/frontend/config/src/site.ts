import type { SiteConfig } from "@netmetric/types";

export const publicSiteConfig = {
  name: "NetMetric",
  title: "NetMetric — Modern CRM Platform",
  description:
    "NetMetric is a modern CRM platform for sales, customer operations, automation, analytics and business growth.",
  url: "https://netmetric.net",
  nav: [
    {
      title: "Product",
      href: "/product",
    },
    {
      title: "CRM",
      href: "/crm",
    },
    {
      title: "Tools",
      href: "/tools",
    },
    {
      title: "Developers",
      href: "/developers",
    },
    {
      title: "Pricing",
      href: "/pricing",
    },
  ],
} satisfies SiteConfig;

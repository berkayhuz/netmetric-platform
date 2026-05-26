export type NavItem = {
  title: string;
  href: string;
  description?: string;
  disabled?: boolean;
  external?: boolean;
};

export type SiteConfig = {
  name: string;
  title: string;
  description: string;
  url: string;
  nav: NavItem[];
};

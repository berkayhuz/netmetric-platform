import Link from "next/link";

import { ThemeToggle } from "@netmetric/ui/client";
import { Muted, Text } from "@netmetric/ui";

import { publicEnv } from "@/lib/public-env";

type PublicFooterLink = {
  href: string;
  label: string;
};

type PublicFooterCopy = {
  description: string;
  platform: string;
  legal: string;
  apiEndpoint: string;
};

export function PublicFooter({
  legalLinks,
  copy,
}: {
  legalLinks: readonly PublicFooterLink[];
  copy: PublicFooterCopy;
}) {
  return (
    <footer className="border-t bg-background">
      <div className="mx-auto grid w-full max-w-7xl gap-6 px-4 py-10 sm:px-6 lg:grid-cols-3">
        <div className="space-y-2">
          <Text className="font-semibold">NetMetric</Text>
          <Muted>{copy.description}</Muted>
        </div>

        <div className="space-y-2">
          <Text className="font-medium">{copy.platform}</Text>
          <ul className="space-y-1 text-sm">
            <li>
              <a href={publicEnv.authUrl} className="hover:underline">
                auth.netmetric.net
              </a>
            </li>
            <li>
              <a href={publicEnv.accountUrl} className="hover:underline">
                account.netmetric.net
              </a>
            </li>
            <li>
              <a href={publicEnv.apiUrl} className="hover:underline">
                {copy.apiEndpoint}
              </a>
            </li>
          </ul>
        </div>

        <div className="space-y-2">
          <Text className="font-medium">{copy.legal}</Text>
          <ul className="space-y-1 text-sm">
            {legalLinks.map((link) => (
              <li key={link.href}>
                <Link href={link.href} className="hover:underline">
                  {link.label}
                </Link>
              </li>
            ))}
          </ul>
          <div className="pt-2">
            <ThemeToggle />
          </div>
        </div>
      </div>
    </footer>
  );
}

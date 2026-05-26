import Link from "next/link";
import { Badge, Text } from "@netmetric/ui";

import type { AccountOverviewResponse } from "@/lib/account-api";

import { AccountPagePanel } from "./account-page-panel";
import { AccountSection } from "./account-section";
import { ReadOnlyValue } from "./read-only-value";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type OverviewDataPanelProps = {
  overview: AccountOverviewResponse;
};

export function OverviewDataPanel({ overview }: OverviewDataPanelProps) {
  return (
    <AccountPagePanel
      title={tAccountClient("account.overview.title")}
      description={tAccountClient("account.overview.description")}
    >
      <div className="grid gap-4 md:grid-cols-2">
        <AccountSection
          title={tAccountClient("account.overview.identityTitle")}
          description={tAccountClient("account.overview.identityDescription")}
          contentClassName="space-y-3"
        >
          <div className="flex items-center justify-between gap-3">
            <Text className="text-sm text-muted-foreground">
              {tAccountClient("account.fields.displayName")}
            </Text>
            <ReadOnlyValue value={overview.displayName} />
          </div>
          <div className="flex items-center justify-between gap-3">
            <Text className="text-sm text-muted-foreground">
              {tAccountClient("account.fields.avatarUrl")}
            </Text>
            <ReadOnlyValue value={overview.avatarUrl} />
          </div>
          <div className="flex items-center justify-between gap-3">
            <Text className="text-sm text-muted-foreground">
              {tAccountClient("account.overview.organizationsTitle")}
            </Text>
            <ReadOnlyValue value={overview.organizations.length} />
          </div>
        </AccountSection>

        <AccountSection
          title={tAccountClient("account.security.title")}
          description={tAccountClient("account.security.postureDescription")}
          contentClassName="space-y-3"
        >
          <div className="flex items-center justify-between gap-3">
            <Text className="text-sm text-muted-foreground">
              {tAccountClient("account.mfa.enabledLabel")}
            </Text>
            <Badge variant={overview.isMfaEnabled ? "secondary" : "outline"}>
              {overview.isMfaEnabled
                ? tAccountClient("account.common.enabled")
                : tAccountClient("account.common.disabled")}
            </Badge>
          </div>
          <div className="flex items-center justify-between gap-3">
            <Text className="text-sm text-muted-foreground">
              {tAccountClient("account.sessions.activeTitle")}
            </Text>
            <ReadOnlyValue value={overview.activeSessionCount} />
          </div>
          <div className="flex items-center justify-between gap-3">
            <Text className="text-sm text-muted-foreground">
              {tAccountClient("account.security.lastEvent")}
            </Text>
            <ReadOnlyValue value={overview.lastSecurityEventAt} />
          </div>
        </AccountSection>
      </div>

      <AccountSection
        title={tAccountClient("account.overview.quickLinks")}
        description={tAccountClient("account.overview.quickLinksDescription")}
      >
        <nav aria-label={tAccountClient("account.a11y.overviewQuickLinks")}>
          <ul className="grid gap-2 sm:grid-cols-2">
            {[
              { href: "/profile", label: tAccountClient("account.profile.title") },
              { href: "/preferences", label: tAccountClient("account.preferences.title") },
              { href: "/security", label: tAccountClient("account.security.title") },
              { href: "/notifications", label: tAccountClient("account.notifications.title") },
            ].map((item) => (
              <li key={item.href}>
                <Link
                  href={item.href}
                  className="inline-flex h-8 items-center rounded-sm border border-border px-3 text-sm font-medium transition-colors hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                >
                  {item.label}
                </Link>
              </li>
            ))}
          </ul>
        </nav>
      </AccountSection>
    </AccountPagePanel>
  );
}

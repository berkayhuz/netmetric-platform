import Link from "next/link";
import { Badge, Text } from "@netmetric/ui";

import { AccountPagePanel } from "./account-page-panel";
import { AccountSection } from "./account-section";
import { tAccountClient } from "@/lib/i18n/account-i18n";
import type {
  AccountOverviewResponse,
  MfaStatusResponse,
  TrustedDevicesResponse,
  UserSessionsResponse,
} from "@/lib/account-api";

type SecurityOverviewPanelProps = {
  overview: AccountOverviewResponse;
  mfaStatus: MfaStatusResponse;
  sessions: UserSessionsResponse;
  trustedDevices: TrustedDevicesResponse;
};

export function SecurityOverviewPanel({
  overview,
  mfaStatus,
  sessions,
  trustedDevices,
}: SecurityOverviewPanelProps) {
  return (
    <AccountPagePanel
      title={tAccountClient("account.security.title")}
      description={tAccountClient("account.security.postureDescription")}
    >
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <SummaryCard
          label={tAccountClient("account.mfa.short")}
          value={
            mfaStatus.isEnabled
              ? tAccountClient("account.common.enabled")
              : tAccountClient("account.common.disabled")
          }
          note={
            mfaStatus.hasAuthenticator
              ? tAccountClient("account.mfa.authenticatorReady")
              : tAccountClient("account.mfa.noAuthenticatorConfigured")
          }
        />
        <SummaryCard
          label={tAccountClient("account.sessions.activeTitle")}
          value={String(sessions.items.filter((item) => item.isActive).length)}
          note={tAccountClient("account.security.currentCount", {
            count: overview.activeSessionCount,
          })}
        />
        <SummaryCard
          label={tAccountClient("account.sessions.trustedTitle")}
          value={String(trustedDevices.items.length)}
          note={tAccountClient("account.sessions.revokeNextPhase")}
        />
        <SummaryCard
          label={tAccountClient("account.mfa.recoveryCodesTitle")}
          value={String(mfaStatus.recoveryCodesRemaining)}
          note={tAccountClient("account.mfa.regenerationNextPhase")}
        />
      </div>

      <AccountSection
        title={tAccountClient("account.security.actionsRoadmap")}
        description={tAccountClient("account.security.actionsRoadmapDescription")}
      >
        <ul className="grid gap-2 sm:grid-cols-2">
          {[
            {
              href: "/security/password",
              label: tAccountClient("account.security.passwordManagement"),
            },
            { href: "/security/mfa", label: tAccountClient("account.mfa.managementTitle") },
            {
              href: "/security/sessions",
              label: tAccountClient("account.sessions.managementTitle"),
            },
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
      </AccountSection>
    </AccountPagePanel>
  );
}

function SummaryCard({ label, value, note }: { label: string; value: string; note: string }) {
  return (
    <section className="space-y-3 border-b border-border/70 pb-5">
      <div className="space-y-1">
        <Text className="text-sm text-muted-foreground">{label}</Text>
        <Text className="text-xl font-semibold text-foreground">{value}</Text>
      </div>
      <Badge variant="outline">{note}</Badge>
    </section>
  );
}

import Link from "next/link";
import { Text } from "@netmetric/ui";

import { AccountPagePanel } from "./account-page-panel";
import { AccountField, AccountSection } from "./account-section";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type SettingsHubPanelProps = {
  displayName?: string | null;
  language?: string | null;
  timeZone?: string | null;
};

export function SettingsHubPanel({ displayName, language, timeZone }: SettingsHubPanelProps) {
  return (
    <AccountPagePanel
      title={tAccountClient("account.settings.title")}
      description={tAccountClient("account.settings.description")}
    >
      <AccountSection
        title={tAccountClient("account.settings.currentContextTitle")}
        description={tAccountClient("account.settings.currentContextDescription")}
        contentClassName="grid gap-2 sm:grid-cols-3"
      >
        <Stat label={tAccountClient("account.fields.displayName")} value={displayName} />
        <Stat label={tAccountClient("account.profile.fields.language")} value={language} />
        <Stat label={tAccountClient("account.profile.fields.timeZone")} value={timeZone} />
      </AccountSection>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {[
          {
            href: "/profile",
            title: tAccountClient("account.nav.profile"),
            description: tAccountClient("account.settings.cards.profileDescription"),
          },
          {
            href: "/preferences",
            title: tAccountClient("account.nav.preferences"),
            description: tAccountClient("account.settings.cards.preferencesDescription"),
          },
          {
            href: "/security",
            title: tAccountClient("account.nav.security"),
            description: tAccountClient("account.settings.cards.securityDescription"),
          },
          {
            href: "/notifications",
            title: tAccountClient("account.nav.notifications"),
            description: tAccountClient("account.settings.cards.notificationsDescription"),
          },
          {
            href: "/privacy",
            title: tAccountClient("account.nav.privacy"),
            description: tAccountClient("account.settings.cards.privacyDescription"),
          },
          {
            href: "/settings/team",
            title: tAccountClient("account.nav.team"),
            description: tAccountClient("account.settings.cards.teamDescription"),
          },
        ].map((item) => (
          <section key={item.href} className="space-y-4 border-b border-border/70 pb-5">
            <div className="space-y-1">
              <Text className="text-base font-semibold text-foreground">{item.title}</Text>
              <Text className="text-sm text-muted-foreground">{item.description}</Text>
            </div>
            <Link
              href={item.href}
              className="inline-flex h-8 items-center rounded-sm border border-border px-3 text-sm font-medium transition-colors hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            >
              {tAccountClient("account.settings.openSection", { title: item.title })}
            </Link>
          </section>
        ))}
      </div>
    </AccountPagePanel>
  );
}

function Stat({ label, value }: { label: string; value: string | null | undefined }) {
  return (
    <AccountField label={label} value={value || tAccountClient("account.common.notAvailable")} />
  );
}

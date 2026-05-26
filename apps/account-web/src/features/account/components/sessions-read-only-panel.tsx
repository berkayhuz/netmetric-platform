import { Text } from "@netmetric/ui";

import type { TrustedDevicesResponse, UserSessionsResponse } from "@/lib/account-api";
import { tAccountClient } from "@/lib/i18n/account-i18n";
import { AccountPagePanel } from "./account-page-panel";
import { AccountSection } from "./account-section";

type SessionsReadOnlyPanelProps = {
  sessions: UserSessionsResponse;
  trustedDevices: TrustedDevicesResponse;
};

export function SessionsReadOnlyPanel({ sessions, trustedDevices }: SessionsReadOnlyPanelProps) {
  return (
    <AccountPagePanel
      title={tAccountClient("account.sessions.title")}
      description={tAccountClient("account.sessions.readOnlyDescription")}
    >
      <AccountSection
        title={tAccountClient("account.sessions.activeTitle")}
        description={tAccountClient("account.sessions.activeDescription")}
        contentClassName="space-y-3"
      >
        {sessions.items.length === 0 ? (
          <Text className="text-muted-foreground">
            {tAccountClient("account.sessions.emptySessions")}
          </Text>
        ) : (
          sessions.items.map((session) => (
            <section key={session.id} className="space-y-2 border-b border-border/70 pb-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <Text className="font-medium">
                  {session.deviceName || tAccountClient("account.sessions.unknownDevice")}
                </Text>
                <Text className="text-sm text-muted-foreground">
                  {session.isCurrent
                    ? tAccountClient("account.sessions.currentSession")
                    : session.isActive
                      ? tAccountClient("account.common.active")
                      : tAccountClient("account.common.inactive")}
                </Text>
              </div>
              <Text className="text-sm text-muted-foreground">
                {tAccountClient("account.sessions.userAgentLabel")}:{" "}
                {session.userAgent || tAccountClient("account.common.notAvailable")}
              </Text>
              <Text className="text-sm text-muted-foreground">
                {tAccountClient("account.sessions.ipLabel")}:{" "}
                {session.ipAddress || tAccountClient("account.common.notAvailable")}
              </Text>
              <Text className="text-sm text-muted-foreground">
                {tAccountClient("account.sessions.locationLabel")}:{" "}
                {session.approximateLocation || tAccountClient("account.common.notAvailable")}
              </Text>
              <Text className="text-sm text-muted-foreground">
                {tAccountClient("account.sessions.lastSeenLabel")}: {session.lastSeenAt}
              </Text>
            </section>
          ))
        )}
      </AccountSection>

      <AccountSection
        title={tAccountClient("account.sessions.trustedTitle")}
        description={tAccountClient("account.sessions.trustedDescription")}
        contentClassName="space-y-3"
      >
        {trustedDevices.items.length === 0 ? (
          <Text className="text-muted-foreground">
            {tAccountClient("account.sessions.emptyDevices")}
          </Text>
        ) : (
          trustedDevices.items.map((device) => (
            <section key={device.id} className="space-y-2 border-b border-border/70 pb-4">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <Text className="font-medium">{device.name}</Text>
                <Text className="text-sm text-muted-foreground">
                  {device.isCurrent
                    ? tAccountClient("account.sessions.currentDevice")
                    : device.isActive
                      ? tAccountClient("account.sessions.trusted")
                      : tAccountClient("account.sessions.expired")}
                </Text>
              </div>
              <Text className="text-sm text-muted-foreground">
                {tAccountClient("account.sessions.userAgentLabel")}:{" "}
                {device.userAgent || tAccountClient("account.common.notAvailable")}
              </Text>
              <Text className="text-sm text-muted-foreground">
                {tAccountClient("account.sessions.ipLabel")}:{" "}
                {device.ipAddress || tAccountClient("account.common.notAvailable")}
              </Text>
              <Text className="text-sm text-muted-foreground">
                {tAccountClient("account.sessions.trustedAtLabel")}: {device.trustedAt}
              </Text>
              <Text className="text-sm text-muted-foreground">
                {tAccountClient("account.sessions.expiresAtLabel")}: {device.expiresAt}
              </Text>
            </section>
          ))
        )}
      </AccountSection>
    </AccountPagePanel>
  );
}

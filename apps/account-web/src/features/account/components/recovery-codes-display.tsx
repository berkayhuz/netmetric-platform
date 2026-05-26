"use client";

import { Text } from "@netmetric/ui";
import { tAccountClient } from "@/lib/i18n/account-i18n";
import { AccountSection } from "./account-section";

type RecoveryCodesDisplayProps = {
  codes: string[];
};

export function RecoveryCodesDisplay({ codes }: RecoveryCodesDisplayProps) {
  if (codes.length === 0) {
    return null;
  }

  return (
    <AccountSection
      title={tAccountClient("account.mfa.recoveryCodesTitle")}
      description={tAccountClient("account.mfa.recoveryCodesDescription")}
      contentClassName="space-y-2"
    >
      <div className="grid gap-2 sm:grid-cols-2">
        {codes.map((code, index) => (
          <Text
            key={`${code}-${index}`}
            className="rounded-sm border border-border bg-muted px-3 py-2 font-mono text-sm"
          >
            {code}
          </Text>
        ))}
      </div>
    </AccountSection>
  );
}

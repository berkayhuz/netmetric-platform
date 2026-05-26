"use client";

import { useActionState } from "react";
import { useFormStatus } from "react-dom";
import {
  Badge,
  Button,
  Field,
  FieldContent,
  FieldError,
  FieldLabel,
  Input,
  Text,
} from "@netmetric/ui";
import type { ReactNode } from "react";

import type { MfaStatusResponse } from "@/lib/account-api";

import {
  confirmMfaAction,
  disableMfaAction,
  regenerateRecoveryCodesAction,
  setupMfaAction,
} from "../actions/mfa-actions";
import { initialMutationState } from "../actions/mutation-state";
import { AccountPagePanel } from "./account-page-panel";
import { AccountSection } from "./account-section";
import { SecurityActionResult } from "./security-action-result";
import { RecoveryCodesDisplay } from "./recovery-codes-display";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type MfaManagementPanelProps = {
  mfaStatus: MfaStatusResponse;
};

function PendingButton({
  idleLabel,
  pendingLabel,
  variant = "default",
}: {
  idleLabel: string;
  pendingLabel: string;
  variant?: "default" | "outline" | "destructive";
}) {
  const { pending } = useFormStatus();
  return (
    <Button size="xs" type="submit" disabled={pending} variant={variant}>
      {pending ? pendingLabel : idleLabel}
    </Button>
  );
}

export function MfaManagementPanel({ mfaStatus }: MfaManagementPanelProps) {
  const [setupState, setupFormAction] = useActionState(setupMfaAction, initialMutationState);
  const [confirmState, confirmFormAction] = useActionState(confirmMfaAction, initialMutationState);
  const [disableState, disableFormAction] = useActionState(disableMfaAction, initialMutationState);
  const [recoveryState, recoveryFormAction] = useActionState(
    regenerateRecoveryCodesAction,
    initialMutationState,
  );

  const setupData = setupState.data?.setup;
  const recoveryCodes = confirmState.data?.recoveryCodes ?? recoveryState.data?.recoveryCodes ?? [];

  return (
    <AccountPagePanel
      title={tAccountClient("account.mfa.title")}
      description={tAccountClient("account.mfa.description")}
      contentClassName="w-full lg:w-96"
    >
      <AccountSection
        title={tAccountClient("account.mfa.statusTitle")}
        description={tAccountClient("account.mfa.statusDescription")}
        contentClassName="space-y-3"
      >
        <Row
          label={tAccountClient("account.mfa.enabledLabel")}
          value={
            mfaStatus.isEnabled ? (
              <Badge variant="secondary">{tAccountClient("account.common.enabled")}</Badge>
            ) : (
              <Badge variant="outline">{tAccountClient("account.common.disabled")}</Badge>
            )
          }
        />
        <Row
          label={tAccountClient("account.mfa.authenticatorConfigured")}
          value={
            mfaStatus.hasAuthenticator ? (
              <Badge variant="secondary">{tAccountClient("account.common.configured")}</Badge>
            ) : (
              <Badge variant="outline">{tAccountClient("account.common.notConfigured")}</Badge>
            )
          }
        />
        <Row
          label={tAccountClient("account.mfa.recoveryCodesRemaining")}
          value={<Text>{String(mfaStatus.recoveryCodesRemaining)}</Text>}
        />
      </AccountSection>

      {!mfaStatus.isEnabled ? (
        <AccountSection
          title={tAccountClient("account.mfa.setupTitle")}
          description={tAccountClient("account.mfa.setupDescription")}
          contentClassName="space-y-4"
        >
          <SecurityActionResult
            state={setupState}
            successTitle={tAccountClient("account.mfa.setupStarted")}
            errorTitle={tAccountClient("account.mfa.setupFailed")}
          />
          <form action={setupFormAction} className="space-y-3">
            <Input type="hidden" name="confirm" value="setup-mfa" />
            <Text className="text-sm text-muted-foreground">
              {tAccountClient("account.mfa.setupConfirm")}
            </Text>
            <PendingButton
              idleLabel={tAccountClient("account.mfa.startSetup")}
              pendingLabel={tAccountClient("account.mfa.starting")}
            />
          </form>

          {setupData ? (
            <AccountSection
              title={tAccountClient("account.mfa.setupDetails")}
              description={tAccountClient("account.mfa.setupDetailsDescription")}
              contentClassName="space-y-3"
            >
              <div className="space-y-1">
                <Text className="text-sm text-muted-foreground">
                  {tAccountClient("account.mfa.manualKey")}
                </Text>
                <Text className="rounded-sm border border-border bg-muted px-3 py-2 font-mono text-sm">
                  {setupData.sharedKey}
                </Text>
              </div>
              <div className="space-y-1">
                <Text className="text-sm text-muted-foreground">
                  {tAccountClient("account.mfa.authenticatorUri")}
                </Text>
                <Text className="break-all rounded-sm border border-border bg-muted px-3 py-2 font-mono text-xs">
                  {setupData.authenticatorUri}
                </Text>
              </div>
            </AccountSection>
          ) : null}
        </AccountSection>
      ) : null}

      {!mfaStatus.isEnabled || setupData ? (
        <AccountSection
          title={tAccountClient("account.mfa.confirmTitle")}
          description={tAccountClient("account.mfa.confirmDescription")}
          contentClassName="space-y-4"
        >
          <SecurityActionResult
            state={confirmState}
            successTitle={tAccountClient("account.mfa.enabledSuccess")}
            errorTitle={tAccountClient("account.mfa.confirmFailed")}
          />
          <form action={confirmFormAction} className="space-y-3" noValidate>
            <Field>
              <FieldLabel htmlFor="verificationCode">
                {tAccountClient("account.mfa.verificationCode")}
              </FieldLabel>
              <FieldContent>
                <Input
                  id="verificationCode"
                  name="verificationCode"
                  inputMode="numeric"
                  autoComplete="one-time-code"
                  aria-invalid={Boolean(confirmState.fieldErrors?.verificationCode?.[0])}
                  aria-describedby={
                    confirmState.fieldErrors?.verificationCode?.[0]
                      ? "verificationCode-error"
                      : undefined
                  }
                />
                <FieldError id="verificationCode-error">
                  {confirmState.fieldErrors?.verificationCode?.[0]}
                </FieldError>
              </FieldContent>
            </Field>
            <PendingButton
              idleLabel={tAccountClient("account.mfa.enable")}
              pendingLabel={tAccountClient("account.mfa.enabling")}
            />
          </form>
        </AccountSection>
      ) : null}

      {mfaStatus.isEnabled ? (
        <>
          <AccountSection
            title={tAccountClient("account.mfa.disableTitle")}
            description={tAccountClient("account.mfa.disableDescription")}
            contentClassName="space-y-4"
          >
            <SecurityActionResult
              state={disableState}
              successTitle={tAccountClient("account.mfa.disabledSuccess")}
              errorTitle={tAccountClient("account.mfa.disableFailed")}
            />
            <form action={disableFormAction} className="space-y-3" noValidate>
              <Input type="hidden" name="confirm" value="disable-mfa" />
              <Field>
                <FieldLabel htmlFor="disableVerificationCode">
                  {tAccountClient("account.mfa.verificationCode")}
                </FieldLabel>
                <FieldContent>
                  <Input
                    id="disableVerificationCode"
                    name="verificationCode"
                    inputMode="numeric"
                    autoComplete="one-time-code"
                    aria-invalid={Boolean(disableState.fieldErrors?.verificationCode?.[0])}
                    aria-describedby={
                      disableState.fieldErrors?.verificationCode?.[0]
                        ? "disableVerificationCode-error"
                        : undefined
                    }
                  />
                  <FieldError id="disableVerificationCode-error">
                    {disableState.fieldErrors?.verificationCode?.[0]}
                  </FieldError>
                </FieldContent>
              </Field>
              <PendingButton
                idleLabel={tAccountClient("account.mfa.disable")}
                pendingLabel={tAccountClient("account.mfa.disabling")}
                variant="destructive"
              />
            </form>
          </AccountSection>

          <AccountSection
            title={tAccountClient("account.mfa.regenerateRecoveryCodes")}
            description={tAccountClient("account.mfa.regenerateDescription")}
            contentClassName="space-y-4"
          >
            <SecurityActionResult
              state={recoveryState}
              successTitle={tAccountClient("account.mfa.recoveryRegenerated")}
              errorTitle={tAccountClient("account.mfa.regenerationFailed")}
            />
            <form action={recoveryFormAction} className="space-y-3">
              <Input type="hidden" name="confirm" value="regenerate-recovery-codes" />
              <Text className="text-sm text-muted-foreground">
                {tAccountClient("account.mfa.regenerateConfirm")}
              </Text>
              <PendingButton
                idleLabel={tAccountClient("account.mfa.regenerate")}
                pendingLabel={tAccountClient("account.mfa.regenerating")}
              />
            </form>
          </AccountSection>
        </>
      ) : null}

      <RecoveryCodesDisplay codes={recoveryCodes} />
    </AccountPagePanel>
  );
}

function Row({ label, value }: { label: string; value: ReactNode }) {
  return (
    <div className="flex items-center justify-between gap-3">
      <Text className="text-sm text-muted-foreground">{label}</Text>
      {value}
    </div>
  );
}

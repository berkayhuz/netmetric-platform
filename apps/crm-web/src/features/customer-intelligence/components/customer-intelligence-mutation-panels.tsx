"use client";

import { useActionState } from "react";
import { useRouter } from "next/navigation";
import { useEffect } from "react";
import {
  Button,
  Field,
  FieldContent,
  FieldLabel,
  FormGrid,
  Input,
  SectionCard,
  Textarea,
} from "@netmetric/ui";
import { CrmMutationResult } from "@/components/forms/crm-mutation-result";
import { initialCrmMutationState } from "@/features/shared/actions/mutation-state";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

import {
  appendActivityFormAction,
  detectDuplicatesFormAction,
  mergeEntitiesFormAction,
  resolveIdentityFormAction,
  trackCdpEventFormAction,
  upsertRelationshipFormAction,
} from "@/features/customer-intelligence/actions/customer-intelligence-mutation-actions";

type CustomerIntelligenceMutationPanelsProps = {
  customerId: string;
  canManageDuplicates: boolean;
  canManageIntelligence: boolean;
};

export function CustomerIntelligenceMutationPanels({
  customerId,
  canManageDuplicates,
  canManageIntelligence,
}: Readonly<CustomerIntelligenceMutationPanelsProps>) {
  const router = useRouter();
  const [detectState, detectAction] = useActionState(
    detectDuplicatesFormAction,
    initialCrmMutationState,
  );
  const [mergeState, mergeAction] = useActionState(
    mergeEntitiesFormAction,
    initialCrmMutationState,
  );
  const [activityState, activityAction] = useActionState(
    appendActivityFormAction,
    initialCrmMutationState,
  );
  const [relationshipState, relationshipAction] = useActionState(
    upsertRelationshipFormAction,
    initialCrmMutationState,
  );
  const [eventState, eventAction] = useActionState(
    trackCdpEventFormAction,
    initialCrmMutationState,
  );
  const [identityState, identityAction] = useActionState(
    resolveIdentityFormAction,
    initialCrmMutationState,
  );

  useEffect(() => {
    const redirectTo =
      detectState.redirectTo ??
      mergeState.redirectTo ??
      activityState.redirectTo ??
      relationshipState.redirectTo ??
      eventState.redirectTo ??
      identityState.redirectTo;
    if (redirectTo) {
      router.push(redirectTo);
      router.refresh();
    }
  }, [
    activityState.redirectTo,
    detectState.redirectTo,
    eventState.redirectTo,
    identityState.redirectTo,
    mergeState.redirectTo,
    relationshipState.redirectTo,
    router,
  ]);

  if (!canManageDuplicates && !canManageIntelligence) {
    return (
      <SectionCard
        title={tCrmClient("crm.customerIntelligence.mutations.title")}
        description={tCrmClient("crm.customerIntelligence.mutations.description")}
        className="mt-6"
      >
        <p className="text-xs text-muted-foreground">
          {tCrmClient("crm.customerIntelligence.mutations.noPermission")}
        </p>
      </SectionCard>
    );
  }

  return (
    <SectionCard
      title={tCrmClient("crm.customerIntelligence.mutations.title")}
      description={tCrmClient("crm.customerIntelligence.mutations.description")}
      className="mt-6"
      contentClassName="space-y-4"
    >
      <FormGrid columns={2}>
        {canManageDuplicates ? (
          <form action={detectAction} className="space-y-2 rounded-lg border p-3">
            <CrmMutationResult state={detectState} />
            <input type="hidden" name="subjectId" value={customerId} />
            <input type="hidden" name="entityType" value="Customer" />
            <p className="text-xs font-semibold">
              {tCrmClient("crm.customerIntelligence.mutations.detect.title")}
            </p>
            <Button type="submit" size="sm" className="w-full">
              {tCrmClient("crm.customerIntelligence.mutations.detect.submit")}
            </Button>
          </form>
        ) : null}

        {canManageDuplicates ? (
          <form action={mergeAction} className="space-y-2 rounded-lg border p-3">
            <CrmMutationResult state={mergeState} />
            <input type="hidden" name="primaryEntityId" value={customerId} />
            <input type="hidden" name="primaryEntityType" value="Customer" />
            <input type="hidden" name="secondaryEntityType" value="Customer" />
            <p className="text-xs font-semibold">
              {tCrmClient("crm.customerIntelligence.mutations.merge.title")}
            </p>
            <Field>
              <FieldLabel className="text-xs">
                {tCrmClient("crm.customerIntelligence.mutations.merge.secondaryId")}
              </FieldLabel>
              <FieldContent>
                <Input
                  name="secondaryEntityId"
                  placeholder={tCrmClient("crm.customerIntelligence.mutations.merge.secondaryId")}
                  required
                />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel className="text-xs">
                {tCrmClient("crm.customerIntelligence.mutations.merge.reason")}
              </FieldLabel>
              <FieldContent>
                <Input
                  name="reason"
                  placeholder={tCrmClient("crm.customerIntelligence.mutations.merge.reason")}
                  required
                />
              </FieldContent>
            </Field>
            <Button type="submit" size="sm" className="w-full">
              {tCrmClient("crm.customerIntelligence.mutations.merge.submit")}
            </Button>
          </form>
        ) : null}

        {canManageIntelligence ? (
          <form action={activityAction} className="space-y-2 rounded-lg border p-3">
            <CrmMutationResult state={activityState} />
            <input type="hidden" name="subjectId" value={customerId} />
            <p className="text-xs font-semibold">
              {tCrmClient("crm.customerIntelligence.mutations.activity.title")}
            </p>
            <Field>
              <FieldLabel className="text-xs">
                {tCrmClient("crm.customerIntelligence.mutations.activity.name")}
              </FieldLabel>
              <FieldContent>
                <Input
                  name="name"
                  placeholder={tCrmClient("crm.customerIntelligence.mutations.activity.name")}
                  required
                />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel className="text-xs">
                {tCrmClient("crm.customerIntelligence.mutations.activity.category")}
              </FieldLabel>
              <FieldContent>
                <Input
                  name="category"
                  placeholder={tCrmClient("crm.customerIntelligence.mutations.activity.category")}
                  required
                />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel className="text-xs">
                {tCrmClient("crm.customerIntelligence.mutations.activity.channel")}
              </FieldLabel>
              <FieldContent>
                <Input
                  name="channel"
                  placeholder={tCrmClient("crm.customerIntelligence.mutations.activity.channel")}
                />
              </FieldContent>
            </Field>
            <Button type="submit" size="sm" className="w-full">
              {tCrmClient("crm.customerIntelligence.mutations.activity.submit")}
            </Button>
          </form>
        ) : null}

        {canManageIntelligence ? (
          <form action={relationshipAction} className="space-y-2 rounded-lg border p-3">
            <CrmMutationResult state={relationshipState} />
            <input type="hidden" name="sourceEntityId" value={customerId} />
            <p className="text-xs font-semibold">
              {tCrmClient("crm.customerIntelligence.mutations.relationship.title")}
            </p>
            <Field>
              <FieldLabel className="text-xs">
                {tCrmClient("crm.customerIntelligence.mutations.relationship.targetId")}
              </FieldLabel>
              <FieldContent>
                <Input
                  name="targetEntityId"
                  placeholder={tCrmClient(
                    "crm.customerIntelligence.mutations.relationship.targetId",
                  )}
                  required
                />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel className="text-xs">
                {tCrmClient("crm.customerIntelligence.mutations.relationship.name")}
              </FieldLabel>
              <FieldContent>
                <Input
                  name="name"
                  placeholder={tCrmClient("crm.customerIntelligence.mutations.relationship.name")}
                  required
                />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel className="text-xs">
                {tCrmClient("crm.customerIntelligence.mutations.relationship.type")}
              </FieldLabel>
              <FieldContent>
                <Input
                  name="relationshipType"
                  placeholder={tCrmClient("crm.customerIntelligence.mutations.relationship.type")}
                  required
                />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel className="text-xs">Strength Score</FieldLabel>
              <FieldContent>
                <Input
                  name="strengthScore"
                  type="number"
                  min="0"
                  max="1"
                  step="0.01"
                  defaultValue="0.5"
                />
              </FieldContent>
            </Field>
            <Button type="submit" size="sm" className="w-full">
              {tCrmClient("crm.customerIntelligence.mutations.relationship.submit")}
            </Button>
          </form>
        ) : null}

        {canManageIntelligence ? (
          <form action={eventAction} className="space-y-2 rounded-lg border p-3">
            <CrmMutationResult state={eventState} />
            <input type="hidden" name="subjectId" value={customerId} />
            <p className="text-xs font-semibold">
              {tCrmClient("crm.customerIntelligence.mutations.event.title")}
            </p>
            <Field>
              <FieldLabel className="text-xs">
                {tCrmClient("crm.customerIntelligence.mutations.event.source")}
              </FieldLabel>
              <FieldContent>
                <Input
                  name="source"
                  placeholder={tCrmClient("crm.customerIntelligence.mutations.event.source")}
                  required
                />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel className="text-xs">
                {tCrmClient("crm.customerIntelligence.mutations.event.eventName")}
              </FieldLabel>
              <FieldContent>
                <Input
                  name="eventName"
                  placeholder={tCrmClient("crm.customerIntelligence.mutations.event.eventName")}
                  required
                />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel className="text-xs">
                {tCrmClient("crm.customerIntelligence.mutations.event.channel")}
              </FieldLabel>
              <FieldContent>
                <Input
                  name="channel"
                  placeholder={tCrmClient("crm.customerIntelligence.mutations.event.channel")}
                />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel className="text-xs">
                {tCrmClient("crm.customerIntelligence.mutations.event.identityKey")}
              </FieldLabel>
              <FieldContent>
                <Input
                  name="identityKey"
                  placeholder={tCrmClient("crm.customerIntelligence.mutations.event.identityKey")}
                />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel className="text-xs">Properties JSON</FieldLabel>
              <FieldContent>
                <Textarea
                  name="propertiesJson"
                  placeholder='{"page":"billing"}'
                  className="font-mono"
                />
              </FieldContent>
            </Field>
            <Button type="submit" size="sm" className="w-full">
              {tCrmClient("crm.customerIntelligence.mutations.event.submit")}
            </Button>
          </form>
        ) : null}

        {canManageIntelligence ? (
          <form action={identityAction} className="space-y-2 rounded-lg border p-3">
            <CrmMutationResult state={identityState} />
            <input type="hidden" name="subjectId" value={customerId} />
            <p className="text-xs font-semibold">
              {tCrmClient("crm.customerIntelligence.mutations.identity.title")}
            </p>
            <Field>
              <FieldLabel className="text-xs">
                {tCrmClient("crm.customerIntelligence.mutations.identity.type")}
              </FieldLabel>
              <FieldContent>
                <Input
                  name="identityType"
                  placeholder={tCrmClient("crm.customerIntelligence.mutations.identity.type")}
                  required
                />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel className="text-xs">
                {tCrmClient("crm.customerIntelligence.mutations.identity.value")}
              </FieldLabel>
              <FieldContent>
                <Input
                  name="identityValue"
                  placeholder={tCrmClient("crm.customerIntelligence.mutations.identity.value")}
                  required
                />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel className="text-xs">Confidence Score</FieldLabel>
              <FieldContent>
                <Input
                  name="confidenceScore"
                  type="number"
                  min="0"
                  max="1"
                  step="0.01"
                  defaultValue="0.8"
                />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel className="text-xs">
                {tCrmClient("crm.customerIntelligence.mutations.identity.notes")}
              </FieldLabel>
              <FieldContent>
                <Textarea
                  name="resolutionNotes"
                  placeholder={tCrmClient("crm.customerIntelligence.mutations.identity.notes")}
                />
              </FieldContent>
            </Field>
            <Button type="submit" size="sm" className="w-full">
              {tCrmClient("crm.customerIntelligence.mutations.identity.submit")}
            </Button>
          </form>
        ) : null}
      </FormGrid>
    </SectionCard>
  );
}

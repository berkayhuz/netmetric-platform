import Link from "next/link";
import { notFound } from "next/navigation";
import { Button } from "@netmetric/ui";

import { CrmDeleteConfirmForm } from "@/components/delete/crm-delete-confirm-form";
import { CrmDeleteZone } from "@/components/delete/crm-delete-zone";
import { CrmEntityDetailPanel } from "@/components/shell/crm-entity-detail-panel";
import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { ActivityComposer } from "@/features/activities/components/activity-composer";
import { ActivityTimelinePanel } from "@/features/activities/components/activity-timeline-panel";
import {
  deleteContactAction,
  setPrimaryContactAction,
} from "@/features/contacts/actions/contact-mutation-actions";
import { ContactDetailActions } from "@/features/contacts/components/contact-detail-actions";
import {
  getContactActivitiesTimelineData,
  getContactDetailData,
} from "@/features/contacts/data/contacts-data";
import { isGuid } from "@/features/shared/data/guid";
import { CrmApiError, type ActivityTimelineFeed, type ContactDetailDto } from "@/lib/crm-api";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestDateSettings } from "@/lib/i18n/request-date-settings";

export default async function ContactDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const resolved = await params;
  const session = await requireCrmSession(`/contacts/${resolved.id}`);
  const dateSettings = await getRequestDateSettings();
  const locale = dateSettings.locale;
  const canEdit = crmCapabilityAllows(session.capabilities, "contacts.edit");
  const canDelete = crmCapabilityAllows(session.capabilities, "contacts.delete");
  const canReadActivities = crmCapabilityAllows(session.capabilities, "activities.read");
  const canCreateActivities = crmCapabilityAllows(session.capabilities, "activities.create");

  if (!isGuid(resolved.id)) {
    notFound();
  }

  let contact: ContactDetailDto;
  let unifiedTimelineFeed: ActivityTimelineFeed = {
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 10,
  };
  let isUnifiedTimelineUnavailable = false;

  try {
    contact = await getContactDetailData(resolved.id, `/contacts/${resolved.id}`);
    if (canReadActivities) {
      try {
        unifiedTimelineFeed = await getContactActivitiesTimelineData(
          resolved.id,
          `/contacts/${resolved.id}`,
          {
            page: 1,
            pageSize: 10,
          },
        );
      } catch {
        isUnifiedTimelineUnavailable = true;
      }
    }
  } catch (error) {
    if (error instanceof CrmApiError && error.kind === "not_found") {
      notFound();
    }

    handleCrmApiPageError(error, `/contacts/${resolved.id}`);
  }

  return (
    <CrmPageShell
      title={contact.fullName}
      description={tCrm("crm.contacts.pages.detail.description", locale)}
      actions={
        canEdit ? (
          <Button asChild>
            <Link href={`/contacts/${resolved.id}/edit`}>
              {tCrm("crm.contacts.actions.edit", locale)}
            </Link>
          </Button>
        ) : undefined
      }
    >
      <CrmEntityDetailPanel
        title={tCrm("crm.contacts.pages.detail.profileTitle", locale)}
        fields={[
          { label: tCrm("crm.contacts.fields.firstName", locale), value: contact.firstName },
          { label: tCrm("crm.contacts.fields.lastName", locale), value: contact.lastName },
          { label: tCrm("crm.contacts.fields.email", locale), value: contact.email },
          {
            label: tCrm("crm.contacts.fields.mobilePhoneShort", locale),
            value: contact.mobilePhone,
          },
          { label: tCrm("crm.contacts.fields.company", locale), value: contact.companyName },
          { label: tCrm("crm.contacts.fields.customer", locale), value: contact.customerName },
          {
            label: tCrm("crm.contacts.fields.primaryContact", locale),
            value: contact.isPrimaryContact
              ? tCrm("crm.common.yes", locale)
              : tCrm("crm.common.no", locale),
          },
          {
            label: tCrm("crm.contacts.fields.status", locale),
            value: contact.isActive
              ? tCrm("crm.common.active", locale)
              : tCrm("crm.common.inactive", locale),
          },
        ]}
      />
      {canReadActivities ? (
        <div className="space-y-4">
          {canCreateActivities ? (
            <ActivityComposer
              primaryRecord={{ entityType: "contact", entityId: resolved.id }}
              locale={locale}
            />
          ) : null}
          <ActivityTimelinePanel
            activities={unifiedTimelineFeed.items}
            dateSettings={dateSettings}
            locale={locale}
            title={tCrm("crm.activities.sections.unifiedTimelinePreviewTitle", locale)}
            description={tCrm("crm.activities.sections.unifiedTimelinePreviewDescription", locale)}
            unavailable={isUnifiedTimelineUnavailable}
          />
        </div>
      ) : null}
      {canEdit ? (
        <ContactDetailActions
          isPrimaryContact={contact.isPrimaryContact}
          setPrimaryAction={setPrimaryContactAction.bind(null, resolved.id)}
        />
      ) : null}
      {canDelete ? (
        <CrmDeleteZone
          title={tCrm("crm.contacts.actions.delete", locale)}
          description={tCrm("crm.contacts.pages.detail.deleteDescription", locale)}
          locale={locale}
        >
          <CrmDeleteConfirmForm
            entityLabel={tCrm("crm.contacts.entityName", locale)}
            entityName={contact.fullName}
            confirmValue="delete-contact"
            action={deleteContactAction.bind(null, resolved.id)}
          />
        </CrmDeleteZone>
      ) : null}
    </CrmPageShell>
  );
}

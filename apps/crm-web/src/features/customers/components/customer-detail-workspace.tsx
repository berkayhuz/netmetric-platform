"use client";

import Link from "next/link";
import { useState } from "react";
import {
  Badge,
  Button,
  Field,
  FieldContent,
  FieldLabel,
  Input,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
  Text,
  Textarea,
} from "@netmetric/ui";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@netmetric/ui/client";
import { Users, GitBranch, Shield, Activity, History, Settings } from "lucide-react";

import { ActivityTimelinePanel } from "@/features/activities/components/activity-timeline-panel";
import { ActivityComposer } from "@/features/activities/components/activity-composer";
import {
  changeCustomerLifecycleStageFormAction,
  markCustomerVipFormAction,
  mergeCustomersFormAction,
  recalculateCustomerDataQualityFormAction,
  recalculateCustomerRelationshipHealthFormAction,
  revokeCustomerConsentFormAction,
  shareCustomerRecordFormAction,
  upsertCustomerConsentFormAction,
} from "@/features/customers/actions/customer-mutation-actions";
import type {
  ActivityTimelineFeed,
  ContactListItemDto,
  CrmPagedResult,
  Customer360Dto,
  CustomerAccountHierarchyDto,
  CustomerAccountHierarchyNodeDto,
  CustomerAuditEventDto,
  CustomerConsentDto,
  CustomerDetailDto,
  CustomerDuplicateWarningDto,
} from "@/lib/crm-api";
import {
  type CrmDateSettings,
  formatCrmDate,
  formatCrmDateTime,
} from "@/lib/date-time/crm-date-time";
import { tCrm } from "@/lib/i18n/crm-i18n";

const lifecycleStageOptions = [
  { value: 0, label: "Lead" },
  { value: 1, label: "Prospect" },
  { value: 2, label: "Active customer" },
  { value: 3, label: "Churn risk" },
  { value: 4, label: "Churned" },
  { value: 5, label: "Reactivated" },
] as const;

const consentChannelOptions = ["Email", "SMS", "Phone", "WhatsApp", "Push"] as const;
const consentPurposeOptions = ["Marketing", "Transactional", "Support", "Product updates"] as const;
const consentStatusOptions = ["Unknown", "Opted in", "Opted out", "Revoked"] as const;
const consentSourceOptions = ["Manual", "Import", "Form", "API", "Portal"] as const;
const accessLevelOptions = ["Read", "Comment", "Edit", "Owner delegate"] as const;

type CustomerDetailWorkspaceProps = {
  customer: CustomerDetailDto;
  customer360: Customer360Dto | null;
  contacts: CrmPagedResult<ContactListItemDto> | null;
  consents: CustomerConsentDto[];
  hierarchy: CustomerAccountHierarchyDto | null;
  auditTimeline: CustomerAuditEventDto[];
  duplicateWarnings: CustomerDuplicateWarningDto[];
  canManage: boolean;
  canReviewDuplicates: boolean;
  canReadHealth: boolean;
  canReadActivities: boolean;
  canCreateActivities: boolean;
  unifiedTimeline: ActivityTimelineFeed;
  isUnifiedTimelineUnavailable: boolean;
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
};

export function CustomerDetailWorkspace({
  customer,
  customer360,
  contacts,
  consents,
  hierarchy,
  auditTimeline,
  duplicateWarnings,
  canManage,
  canReviewDuplicates,
  canReadHealth,
  canReadActivities,
  canCreateActivities,
  unifiedTimeline,
  isUnifiedTimelineUnavailable,
  dateSettings,
  locale,
}: Readonly<CustomerDetailWorkspaceProps>) {
  return (
    <div className="w-full space-y-6">
      <Tabs defaultValue="contacts" className="w-full flex flex-col">
        <TabsList
          variant="line"
          className="border-b border-border/30 pb-0 w-full justify-start gap-4 sm:gap-6 bg-transparent px-1 h-10"
        >
          <TabsTrigger value="contacts" className="gap-2 py-2 cursor-pointer">
            <Users className="size-4 opacity-70" />
            <span>{tCrm("crm.customers.fields.contacts", locale)}</span>
            {contacts?.totalCount ? (
              <Badge
                variant="secondary"
                className="h-4.5 rounded-full px-1.5 text-[10px] font-mono font-medium"
              >
                {contacts.totalCount}
              </Badge>
            ) : null}
          </TabsTrigger>
          <TabsTrigger value="hierarchy" className="gap-2 py-2 cursor-pointer">
            <GitBranch className="size-4 opacity-70" />
            <span>{tCrm("crm.customers.pages.detail.hierarchyTitle", locale)}</span>
          </TabsTrigger>
          <TabsTrigger value="consents" className="gap-2 py-2 cursor-pointer">
            <Shield className="size-4 opacity-70" />
            <span>{tCrm("crm.customers.pages.detail.consentsTitle", locale)}</span>
            {consents.length ? (
              <Badge
                variant="secondary"
                className="h-4.5 rounded-full px-1.5 text-[10px] font-mono font-medium"
              >
                {consents.length}
              </Badge>
            ) : null}
          </TabsTrigger>
          <TabsTrigger value="activity" className="gap-2 py-2 cursor-pointer">
            <Activity className="size-4 opacity-70" />
            <span>{tCrm("crm.customers.pages.detail.customer360Title", locale)}</span>
          </TabsTrigger>
          <TabsTrigger value="audit" className="gap-2 py-2 cursor-pointer">
            <History className="size-4 opacity-70" />
            <span>{tCrm("crm.customers.pages.detail.auditTitle", locale)}</span>
          </TabsTrigger>
          {canManage || canReadHealth || canReviewDuplicates ? (
            <TabsTrigger value="actions" className="gap-2 py-2 cursor-pointer sm:ml-auto">
              <Settings className="size-4 opacity-70" />
              <span>Actions</span>
            </TabsTrigger>
          ) : null}
        </TabsList>

        <div className="mt-6">
          <TabsContent value="contacts" className="outline-none">
            <CustomerContactsSection contacts={contacts} locale={locale} />
          </TabsContent>
          <TabsContent value="hierarchy" className="outline-none">
            <CustomerHierarchySection hierarchy={hierarchy} locale={locale} />
          </TabsContent>
          <TabsContent value="consents" className="outline-none">
            <CustomerConsentsSection
              customer={customer}
              consents={consents}
              canManage={canManage}
              dateSettings={dateSettings}
              locale={locale}
            />
          </TabsContent>
          <TabsContent value="activity" className="outline-none">
            <Customer360ActivitySection
              customer360={customer360}
              dateSettings={dateSettings}
              locale={locale}
            />
          </TabsContent>
          <TabsContent value="audit" className="outline-none">
            <CustomerAuditTimelineSection
              auditTimeline={auditTimeline}
              dateSettings={dateSettings}
              locale={locale}
            />
          </TabsContent>
          {canManage || canReadHealth || canReviewDuplicates ? (
            <TabsContent value="actions" className="outline-none space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                {canManage ? (
                  <CustomerStatusActions
                    customer={customer}
                    lifecycleStage={customer360?.lifecycleStage ?? null}
                    locale={locale}
                  />
                ) : null}
                {canManage || canReadHealth ? (
                  <CustomerRecalculateActions
                    customer={customer}
                    canManage={canManage}
                    canReadHealth={canReadHealth}
                    locale={locale}
                  />
                ) : null}
                {canManage ? <CustomerConsentAction customer={customer} locale={locale} /> : null}
                {canManage ? <CustomerShareAction customer={customer} locale={locale} /> : null}
                {canReviewDuplicates ? (
                  <CustomerMergeAction
                    customer={customer}
                    warnings={duplicateWarnings}
                    locale={locale}
                  />
                ) : null}
              </div>
            </TabsContent>
          ) : null}
        </div>
      </Tabs>
      {canReadActivities ? (
        <div className="space-y-4">
          {canCreateActivities ? (
            <ActivityComposer
              primaryRecord={{ entityType: "customer", entityId: customer.id }}
              locale={locale}
            />
          ) : null}
          <ActivityTimelinePanel
            activities={unifiedTimeline.items}
            dateSettings={dateSettings}
            locale={locale ?? "en"}
            title={tCrm("crm.activities.sections.unifiedTimelinePreviewTitle", locale)}
            description={tCrm("crm.activities.sections.unifiedTimelinePreviewDescription", locale)}
            unavailable={isUnifiedTimelineUnavailable}
          />
        </div>
      ) : null}
    </div>
  );
}

function CustomerStatusActions({
  customer,
  lifecycleStage: customerLifecycleStage,
  locale,
}: Readonly<{
  customer: CustomerDetailDto;
  lifecycleStage?: string | number | null;
  locale?: string | null | undefined;
}>) {
  const [isVip, setIsVip] = useState(String(customer.isVip));
  const initialStage =
    customerLifecycleStage !== undefined && customerLifecycleStage !== null
      ? String(customerLifecycleStage)
      : String(lifecycleStageOptions[0].value);
  const [lifecycleStage, setLifecycleStage] = useState(initialStage);

  return (
    <div className="space-y-4">
      <h3 className="text-sm font-semibold tracking-tight text-foreground flex items-center gap-2 border-b border-border/30 pb-2">
        <Settings className="size-4 text-muted-foreground" />
        <span>{tCrm("crm.customers.pages.detail.actionsTitle", locale)}</span>
      </h3>
      <div className="space-y-6 pt-2">
        <form action={markCustomerVipFormAction.bind(null, customer.id)} className="space-y-3">
          <Field>
            <FieldLabel htmlFor="customer-vip-action" className="text-xs text-muted-foreground">
              {tCrm("crm.customers.fields.vip", locale)}
            </FieldLabel>
            <FieldContent>
              <Select value={isVip} onValueChange={(val) => setIsVip(val ?? "false")} name="isVip">
                <SelectTrigger id="customer-vip-action" className="w-full text-sm bg-background">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="false">{tCrm("crm.common.no", locale)}</SelectItem>
                  <SelectItem value="true">{tCrm("crm.common.yes", locale)}</SelectItem>
                </SelectContent>
              </Select>
            </FieldContent>
          </Field>
          <Button type="submit" size="sm">
            {tCrm("crm.customers.actions.updateVip", locale)}
          </Button>
        </form>

        <div className="border-t border-border/30 pt-5">
          <form
            action={changeCustomerLifecycleStageFormAction.bind(null, customer.id)}
            className="space-y-3"
          >
            <Field>
              <FieldLabel
                htmlFor="customer-lifecycle-action"
                className="text-xs text-muted-foreground"
              >
                {tCrm("crm.customers.fields.lifecycleStage", locale)}
              </FieldLabel>
              <FieldContent>
                <Select
                  id="customer-lifecycle-action"
                  name="newStage"
                  value={lifecycleStage}
                  onValueChange={(val) => setLifecycleStage(val ?? "0")}
                >
                  <SelectTrigger className="w-full text-sm bg-background">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {lifecycleStageOptions.map((option) => (
                      <SelectItem key={option.value} value={String(option.value)}>
                        {option.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel
                htmlFor="customer-lifecycle-reason"
                className="text-xs text-muted-foreground"
              >
                {tCrm("crm.customers.fields.reason", locale)}
              </FieldLabel>
              <FieldContent>
                <Textarea
                  id="customer-lifecycle-reason"
                  name="reason"
                  rows={2}
                  className="text-sm bg-background"
                />
              </FieldContent>
            </Field>
            <Button type="submit" size="sm">
              {tCrm("crm.customers.actions.changeLifecycle", locale)}
            </Button>
          </form>
        </div>
      </div>
    </div>
  );
}

function CustomerRecalculateActions({
  customer,
  canManage,
  canReadHealth,
  locale,
}: Readonly<{
  customer: CustomerDetailDto;
  canManage: boolean;
  canReadHealth: boolean;
  locale?: string | null | undefined;
}>) {
  return (
    <div className="space-y-4">
      <h3 className="text-sm font-semibold tracking-tight text-foreground flex items-center gap-2 border-b border-border/30 pb-2">
        <Activity className="size-4 text-muted-foreground" />
        <span>{tCrm("crm.customers.pages.detail.recalculateTitle", locale)}</span>
      </h3>
      <div className="space-y-4 pt-2">
        <Text className="text-xs text-muted-foreground">
          {tCrm("crm.customers.pages.detail.recalculateDescription", locale)}
        </Text>
        <div className="flex flex-wrap gap-2 pt-2">
          {canManage ? (
            <form action={recalculateCustomerDataQualityFormAction.bind(null, customer.id)}>
              <Button type="submit" variant="outline" size="sm">
                {tCrm("crm.customers.actions.recalculateDataQuality", locale)}
              </Button>
            </form>
          ) : null}
          {canReadHealth ? (
            <form action={recalculateCustomerRelationshipHealthFormAction.bind(null, customer.id)}>
              <Button type="submit" variant="outline" size="sm">
                {tCrm("crm.customers.actions.recalculateRelationshipHealth", locale)}
              </Button>
            </form>
          ) : null}
        </div>
      </div>
    </div>
  );
}

function CustomerConsentAction({
  customer,
  locale,
}: Readonly<{ customer: CustomerDetailDto; locale?: string | null | undefined }>) {
  return (
    <div className="space-y-4 md:col-span-2 border-t border-border/30 pt-6">
      <h3 className="text-sm font-semibold tracking-tight text-foreground flex items-center gap-2 border-b border-border/30 pb-2">
        <Shield className="size-4 text-muted-foreground" />
        <span>{tCrm("crm.customers.actions.upsertConsent", locale)}</span>
      </h3>
      <form
        action={upsertCustomerConsentFormAction.bind(null, customer.id)}
        className="grid gap-4 md:grid-cols-2 pt-2"
      >
        <EnumField
          id="customer-consent-channel"
          name="channel"
          label={tCrm("crm.customers.fields.consentChannel", locale)}
          options={consentChannelOptions}
        />
        <EnumField
          id="customer-consent-purpose"
          name="purpose"
          label={tCrm("crm.customers.fields.consentPurpose", locale)}
          options={consentPurposeOptions}
        />
        <EnumField
          id="customer-consent-status"
          name="status"
          label={tCrm("crm.customers.fields.consentStatus", locale)}
          options={consentStatusOptions}
        />
        <EnumField
          id="customer-consent-source"
          name="source"
          label={tCrm("crm.customers.fields.consentSource", locale)}
          options={consentSourceOptions}
        />
        <Field>
          <FieldLabel
            htmlFor="customer-consent-valid-until"
            className="text-xs text-muted-foreground"
          >
            {tCrm("crm.customers.fields.validUntilUtc", locale)}
          </FieldLabel>
          <FieldContent>
            <Input
              id="customer-consent-valid-until"
              name="validUntilUtc"
              type="datetime-local"
              className="text-sm bg-background"
            />
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="customer-consent-reason" className="text-xs text-muted-foreground">
            {tCrm("crm.customers.fields.reason", locale)}
          </FieldLabel>
          <FieldContent>
            <Input id="customer-consent-reason" name="reason" className="text-sm bg-background" />
          </FieldContent>
        </Field>
        <Field className="md:col-span-2">
          <FieldLabel htmlFor="customer-consent-evidence" className="text-xs text-muted-foreground">
            {tCrm("crm.customers.fields.evidenceText", locale)}
          </FieldLabel>
          <FieldContent>
            <Textarea
              id="customer-consent-evidence"
              name="evidenceText"
              rows={2}
              className="text-sm bg-background"
            />
          </FieldContent>
        </Field>
        <div className="md:col-span-2 pt-2">
          <Button type="submit" size="sm">
            {tCrm("crm.customers.actions.upsertConsent", locale)}
          </Button>
        </div>
      </form>
    </div>
  );
}

function CustomerShareAction({
  customer,
  locale,
}: Readonly<{ customer: CustomerDetailDto; locale?: string | null | undefined }>) {
  return (
    <div className="space-y-4 md:col-span-2 border-t border-border/30 pt-6">
      <h3 className="text-sm font-semibold tracking-tight text-foreground flex items-center gap-2 border-b border-border/30 pb-2">
        <Users className="size-4 text-muted-foreground" />
        <span>{tCrm("crm.customers.actions.shareRecord", locale)}</span>
      </h3>
      <form
        action={shareCustomerRecordFormAction.bind(null, customer.id)}
        className="grid gap-4 md:grid-cols-2 pt-2"
      >
        <Field>
          <FieldLabel htmlFor="customer-share-user" className="text-xs text-muted-foreground">
            {tCrm("crm.customers.fields.sharedWithUserId", locale)}
          </FieldLabel>
          <FieldContent>
            <Input
              id="customer-share-user"
              name="sharedWithUserId"
              className="text-sm bg-background"
            />
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="customer-share-team" className="text-xs text-muted-foreground">
            {tCrm("crm.customers.fields.sharedWithTeamId", locale)}
          </FieldLabel>
          <FieldContent>
            <Input
              id="customer-share-team"
              name="sharedWithTeamId"
              className="text-sm bg-background"
            />
          </FieldContent>
        </Field>
        <EnumField
          id="customer-share-access"
          name="accessLevel"
          label={tCrm("crm.customers.fields.accessLevel", locale)}
          options={accessLevelOptions}
        />
        <Field>
          <FieldLabel
            htmlFor="customer-share-valid-until"
            className="text-xs text-muted-foreground"
          >
            {tCrm("crm.customers.fields.validUntilUtc", locale)}
          </FieldLabel>
          <FieldContent>
            <Input
              id="customer-share-valid-until"
              name="validUntilUtc"
              type="datetime-local"
              className="text-sm bg-background"
            />
          </FieldContent>
        </Field>
        <Field className="md:col-span-2">
          <FieldLabel htmlFor="customer-share-reason" className="text-xs text-muted-foreground">
            {tCrm("crm.customers.fields.reason", locale)}
          </FieldLabel>
          <FieldContent>
            <Textarea
              id="customer-share-reason"
              name="reason"
              rows={2}
              className="text-sm bg-background"
            />
          </FieldContent>
        </Field>
        <div className="md:col-span-2 pt-2">
          <Button type="submit" size="sm">
            {tCrm("crm.customers.actions.shareRecord", locale)}
          </Button>
        </div>
      </form>
    </div>
  );
}

function CustomerMergeAction({
  customer,
  warnings,
  locale,
}: Readonly<{
  customer: CustomerDetailDto;
  warnings: CustomerDuplicateWarningDto[];
  locale?: string | null | undefined;
}>) {
  const [duplicateId, setDuplicateId] = useState(warnings[0]?.candidateId ?? "");

  return (
    <div className="space-y-4 md:col-span-2 border-t border-border/30 pt-6">
      <h3 className="text-sm font-semibold tracking-tight text-foreground flex items-center gap-2 border-b border-border/30 pb-2">
        <GitBranch className="size-4 text-muted-foreground" />
        <span>{tCrm("crm.customers.actions.merge", locale)}</span>
      </h3>
      <form action={mergeCustomersFormAction} className="space-y-4 pt-2">
        <input name="masterCustomerId" type="hidden" value={customer.id} />
        <Field>
          <FieldLabel htmlFor="customer-merge-duplicate" className="text-xs text-muted-foreground">
            {tCrm("crm.customers.duplicates.candidate", locale)}
          </FieldLabel>
          <FieldContent>
            <Select
              id="customer-merge-duplicate"
              name="duplicateCustomerId"
              value={duplicateId}
              onValueChange={(val) => setDuplicateId(val ?? "")}
            >
              <SelectTrigger className="w-full text-sm bg-background">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {warnings.map((warning) => (
                  <SelectItem key={warning.candidateId} value={warning.candidateId}>
                    {warning.candidateId}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel
            htmlFor="customer-merge-resolved-fields"
            className="text-xs text-muted-foreground"
          >
            {tCrm("crm.customers.fields.resolvedFieldsJson", locale)}
          </FieldLabel>
          <FieldContent>
            <Textarea
              id="customer-merge-resolved-fields"
              name="resolvedFieldsJson"
              rows={3}
              defaultValue="{}"
              className="text-sm bg-background font-mono"
            />
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor="customer-merge-reason" className="text-xs text-muted-foreground">
            {tCrm("crm.customers.fields.reason", locale)}
          </FieldLabel>
          <FieldContent>
            <Textarea
              id="customer-merge-reason"
              name="reason"
              rows={2}
              className="text-sm bg-background"
            />
          </FieldContent>
        </Field>
        <div className="pt-2">
          <Button type="submit" size="sm" disabled={warnings.length === 0}>
            {tCrm("crm.customers.actions.merge", locale)}
          </Button>
        </div>
      </form>
    </div>
  );
}

function CustomerContactsSection({
  contacts,
  locale,
}: Readonly<{
  contacts: CrmPagedResult<ContactListItemDto> | null;
  locale?: string | null | undefined;
}>) {
  return (
    <div className="space-y-4">
      {!contacts || contacts.items.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-12 text-center border border-dashed border-border/40 rounded-xl bg-muted/5">
          <Users className="size-8 text-muted-foreground/40 mb-3" />
          <Text className="text-sm text-muted-foreground font-medium">
            {tCrm("crm.customers.states.noContacts", locale)}
          </Text>
        </div>
      ) : (
        <div className="overflow-hidden rounded-xl border border-border/40 bg-muted/5">
          <Table>
            <TableHeader className="bg-muted/10">
              <TableRow>
                <TableHead className="font-semibold text-xs text-muted-foreground">
                  {tCrm("crm.contacts.fields.fullName", locale)}
                </TableHead>
                <TableHead className="font-semibold text-xs text-muted-foreground">
                  {tCrm("crm.contacts.fields.email", locale)}
                </TableHead>
                <TableHead className="font-semibold text-xs text-muted-foreground">
                  {tCrm("crm.contacts.fields.mobilePhoneShort", locale)}
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {contacts.items.map((contact) => (
                <TableRow key={contact.id} className="hover:bg-muted/5 transition-colors">
                  <TableCell className="font-medium">
                    <Link
                      className="underline-offset-4 hover:underline text-primary"
                      href={`/contacts/${contact.id}`}
                    >
                      {contact.fullName}
                    </Link>
                  </TableCell>
                  <TableCell className="text-muted-foreground">{contact.email ?? "-"}</TableCell>
                  <TableCell className="text-muted-foreground">
                    {contact.mobilePhone ?? "-"}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}
    </div>
  );
}

function CustomerConsentsSection({
  customer,
  consents,
  canManage,
  dateSettings,
  locale,
}: Readonly<{
  customer: CustomerDetailDto;
  consents: CustomerConsentDto[];
  canManage: boolean;
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
}>) {
  return (
    <div className="space-y-4">
      {consents.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-12 text-center border border-dashed border-border/40 rounded-xl bg-muted/5">
          <Shield className="size-8 text-muted-foreground/40 mb-3" />
          <Text className="text-sm text-muted-foreground font-medium">
            {tCrm("crm.customers.states.noConsents", locale)}
          </Text>
        </div>
      ) : (
        <div className="overflow-hidden rounded-xl border border-border/40 bg-muted/5">
          <Table>
            <TableHeader className="bg-muted/10">
              <TableRow>
                <TableHead className="font-semibold text-xs text-muted-foreground">
                  {tCrm("crm.customers.fields.consentChannel", locale)}
                </TableHead>
                <TableHead className="font-semibold text-xs text-muted-foreground">
                  {tCrm("crm.customers.fields.consentPurpose", locale)}
                </TableHead>
                <TableHead className="font-semibold text-xs text-muted-foreground">
                  {tCrm("crm.customers.fields.consentStatus", locale)}
                </TableHead>
                <TableHead className="font-semibold text-xs text-muted-foreground">
                  {tCrm("crm.customers.fields.consentSource", locale)}
                </TableHead>
                <TableHead className="font-semibold text-xs text-muted-foreground">
                  {tCrm("crm.customers.fields.validUntilUtc", locale)}
                </TableHead>
                <TableHead className="font-semibold text-xs text-muted-foreground text-right">
                  {tCrm("crm.modules.workspace.status", locale)}
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {consents.map((consent) => (
                <TableRow key={consent.id} className="hover:bg-muted/5 transition-colors">
                  <TableCell className="font-medium">
                    {enumLabel(consent.channel, consentChannelOptions)}
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    {enumLabel(consent.purpose, consentPurposeOptions)}
                  </TableCell>
                  <TableCell>
                    <Badge variant="outline" className="text-xs bg-background/50">
                      {enumLabel(consent.status, consentStatusOptions)}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    {enumLabel(consent.source, consentSourceOptions)}
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    {formatCrmDate(consent.validUntilUtc, dateSettings)}
                  </TableCell>
                  <TableCell className="text-right">
                    {canManage ? (
                      <form
                        action={revokeCustomerConsentFormAction.bind(null, customer.id, consent.id)}
                        className="flex items-center justify-end gap-2"
                      >
                        <Input
                          className="max-w-40 h-8 text-xs bg-background"
                          name="reason"
                          placeholder={tCrm("crm.customers.fields.reason", locale)}
                        />
                        <Button type="submit" variant="outline" size="sm" className="h-8 text-xs">
                          {tCrm("crm.customers.actions.revokeConsent", locale)}
                        </Button>
                      </form>
                    ) : null}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}
    </div>
  );
}

function CustomerHierarchySection({
  hierarchy,
  locale,
}: Readonly<{
  hierarchy: CustomerAccountHierarchyDto | null;
  locale?: string | null | undefined;
}>) {
  const roots = hierarchy?.roots ?? [];
  return (
    <div className="space-y-4">
      {roots.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-12 text-center border border-dashed border-border/40 rounded-xl bg-muted/5">
          <GitBranch className="size-8 text-muted-foreground/40 mb-3" />
          <Text className="text-sm text-muted-foreground font-medium">
            {tCrm("crm.customers.states.noHierarchy", locale)}
          </Text>
        </div>
      ) : (
        <div className="space-y-3">
          {roots.map((node) => (
            <HierarchyNode key={node.id} node={node} />
          ))}
        </div>
      )}
    </div>
  );
}

function HierarchyNode({ node }: Readonly<{ node: CustomerAccountHierarchyNodeDto }>) {
  return (
    <div className="rounded-xl border border-border/30 bg-muted/5 p-4 space-y-3">
      <div className="flex items-center justify-between gap-3">
        <span className="font-medium text-sm text-foreground">{node.name}</span>
        <Badge variant="outline" className="text-xs bg-background/50">
          {node.relationshipType}
        </Badge>
      </div>
      {node.children.length > 0 ? (
        <div className="mt-2 space-y-2 border-l border-border/40 pl-4">
          {node.children.map((child) => (
            <HierarchyNode key={child.id} node={child} />
          ))}
        </div>
      ) : null}
    </div>
  );
}

function CustomerAuditTimelineSection({
  auditTimeline,
  dateSettings,
  locale,
}: Readonly<{
  auditTimeline: CustomerAuditEventDto[];
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
}>) {
  return (
    <div className="space-y-4">
      {auditTimeline.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-12 text-center border border-dashed border-border/40 rounded-xl bg-muted/5">
          <History className="size-8 text-muted-foreground/40 mb-3" />
          <Text className="text-sm text-muted-foreground font-medium">
            {tCrm("crm.customers.states.noAudit", locale)}
          </Text>
        </div>
      ) : (
        <div className="overflow-hidden rounded-xl border border-border/40 bg-muted/5">
          <Table>
            <TableHeader className="bg-muted/10">
              <TableRow>
                <TableHead className="font-semibold text-xs text-muted-foreground">
                  {tCrm("crm.customers.fields.changedAt", locale)}
                </TableHead>
                <TableHead className="font-semibold text-xs text-muted-foreground">
                  {tCrm("crm.customers.fields.action", locale)}
                </TableHead>
                <TableHead className="font-semibold text-xs text-muted-foreground">
                  {tCrm("crm.customers.fields.fieldName", locale)}
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {auditTimeline.map((event) => (
                <TableRow key={event.id} className="hover:bg-muted/5 transition-colors">
                  <TableCell className="text-muted-foreground font-mono text-xs">
                    {formatCrmDateTime(event.occurredAtUtc, dateSettings)}
                  </TableCell>
                  <TableCell className="font-medium text-foreground">
                    {String(event.action)}
                  </TableCell>
                  <TableCell className="text-muted-foreground font-mono text-xs">
                    {event.fieldName ?? "-"}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}
    </div>
  );
}

function Customer360ActivitySection({
  customer360,
  dateSettings,
  locale,
}: Readonly<{
  customer360: Customer360Dto | null;
  dateSettings: CrmDateSettings;
  locale?: string | null | undefined;
}>) {
  const timeline = customer360?.timeline ?? [];
  return (
    <div className="space-y-4">
      {timeline.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-12 text-center border border-dashed border-border/40 rounded-xl bg-muted/5">
          <Activity className="size-8 text-muted-foreground/40 mb-3" />
          <Text className="text-sm text-muted-foreground font-medium">
            {tCrm("crm.customers.states.noTimeline", locale)}
          </Text>
        </div>
      ) : (
        <div className="overflow-hidden rounded-xl border border-border/40 bg-muted/5">
          <Table>
            <TableHeader className="bg-muted/10">
              <TableRow>
                <TableHead className="font-semibold text-xs text-muted-foreground">
                  {tCrm("crm.customers.fields.changedAt", locale)}
                </TableHead>
                <TableHead className="font-semibold text-xs text-muted-foreground">
                  {tCrm("crm.customers.fields.source", locale)}
                </TableHead>
                <TableHead className="font-semibold text-xs text-muted-foreground">
                  {tCrm("crm.customers.fields.action", locale)}
                </TableHead>
                <TableHead className="font-semibold text-xs text-muted-foreground">
                  {tCrm("crm.customers.fields.titleText", locale)}
                </TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {timeline.map((event) => (
                <TableRow
                  key={`${event.occurredAtUtc}-${event.source}-${event.action}`}
                  className="hover:bg-muted/5 transition-colors"
                >
                  <TableCell className="text-muted-foreground font-mono text-xs">
                    {formatCrmDateTime(event.occurredAtUtc, dateSettings)}
                  </TableCell>
                  <TableCell className="font-medium text-foreground">{event.source}</TableCell>
                  <TableCell className="text-muted-foreground">{event.action}</TableCell>
                  <TableCell className="text-foreground">{event.title}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}
    </div>
  );
}

function EnumField({
  id,
  name,
  label,
  options,
}: Readonly<{ id: string; name: string; label: string; options: readonly string[] }>) {
  const [value, setValue] = useState(String(0));
  return (
    <Field>
      <FieldLabel htmlFor={id} className="text-xs text-muted-foreground">
        {label}
      </FieldLabel>
      <FieldContent>
        <Select name={name} value={value} onValueChange={(val) => setValue(val ?? "0")}>
          <SelectTrigger id={id} className="w-full text-sm bg-background">
            <SelectValue>{options[Number(value)] ?? ""}</SelectValue>
          </SelectTrigger>
          <SelectContent>
            {options.map((option, index) => (
              <SelectItem key={option} value={String(index)}>
                {option}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </FieldContent>
    </Field>
  );
}

function enumLabel(value: string | number, labels: readonly string[]): string {
  if (typeof value === "number") {
    return labels[value] ?? String(value);
  }

  const numeric = Number(value);
  if (Number.isInteger(numeric)) {
    return labels[numeric] ?? value;
  }

  return value;
}

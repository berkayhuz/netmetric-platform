"use client";

import { useActionState, useState } from "react";
import { useFormStatus } from "react-dom";
import {
  Button,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  Field,
  FieldContent,
  FieldLabel,
  Input,
} from "@netmetric/ui";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@netmetric/ui/client";

import { CrmMutationResult } from "@/components/forms/crm-mutation-result";
import {
  initialCrmMutationState,
  type CrmMutationState,
} from "@/features/shared/actions/mutation-state";
import { getSelectDisplayLabel } from "@/features/shared/forms/select-display";
import type { SupportInboxConnectionDto } from "@/lib/crm-api";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

function SubmitButton({ label }: Readonly<{ label: string }>) {
  const { pending } = useFormStatus();
  return (
    <Button type="submit" disabled={pending} aria-busy={pending}>
      {pending ? tCrmClient("crm.forms.actions.processing") : label}
    </Button>
  );
}

function InlineSelect({
  id,
  name,
  defaultValue,
  options,
}: Readonly<{
  id: string;
  name: string;
  defaultValue: string;
  options: { value: string; label: string }[];
}>) {
  const [value, setValue] = useState(defaultValue);
  const displayOptions =
    options.length > 0 ? options : [{ value: defaultValue, label: defaultValue }];
  return (
    <>
      <input type="hidden" id={id} name={name} value={value} />
      <Select value={value} onValueChange={(next) => setValue(next ?? defaultValue)}>
        <SelectTrigger>
          <SelectValue>{getSelectDisplayLabel(value, displayOptions)}</SelectValue>
        </SelectTrigger>
        <SelectContent>
          {options.map((option) => (
            <SelectItem key={`${id}-${option.value}`} value={option.value}>
              {option.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </>
  );
}

function ConnectionPicker({
  id,
  name = "connectionId",
  connections,
}: Readonly<{
  id: string;
  name?: string;
  connections: SupportInboxConnectionDto[];
}>) {
  if (connections.length === 0) {
    return <Input id={id} name={name} />;
  }

  return (
    <InlineSelect
      id={id}
      name={name}
      defaultValue={connections[0]?.id ?? ""}
      options={connections.map((connection) => ({
        value: connection.id,
        label: `${connection.name} (${connection.emailAddress})`,
      }))}
    />
  );
}

function BooleanSelect({
  id,
  name,
  defaultValue,
}: Readonly<{ id: string; name: string; defaultValue: boolean }>) {
  return (
    <InlineSelect
      id={id}
      name={name}
      defaultValue={String(defaultValue)}
      options={[
        { value: "true", label: tCrmClient("crm.common.boolean.true") },
        { value: "false", label: tCrmClient("crm.common.boolean.false") },
      ]}
    />
  );
}

function ConnectionFields({ prefix }: Readonly<{ prefix: string }>) {
  return (
    <>
      <Field>
        <FieldLabel htmlFor={`${prefix}-name`}>
          {tCrmClient("crm.supportInbox.fields.name")}
        </FieldLabel>
        <FieldContent>
          <Input id={`${prefix}-name`} name="name" />
        </FieldContent>
      </Field>
      <div className="grid gap-3 sm:grid-cols-2">
        <Field>
          <FieldLabel htmlFor={`${prefix}-host`}>
            {tCrmClient("crm.supportInbox.fields.host")}
          </FieldLabel>
          <FieldContent>
            <Input id={`${prefix}-host`} name="host" />
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor={`${prefix}-port`}>
            {tCrmClient("crm.supportInbox.fields.port")}
          </FieldLabel>
          <FieldContent>
            <Input id={`${prefix}-port`} name="port" type="number" defaultValue="993" />
          </FieldContent>
        </Field>
      </div>
      <div className="grid gap-3 sm:grid-cols-2">
        <Field>
          <FieldLabel htmlFor={`${prefix}-username`}>
            {tCrmClient("crm.supportInbox.fields.username")}
          </FieldLabel>
          <FieldContent>
            <Input id={`${prefix}-username`} name="username" />
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor={`${prefix}-secret`}>
            {tCrmClient("crm.supportInbox.fields.secretReference")}
          </FieldLabel>
          <FieldContent>
            <Input id={`${prefix}-secret`} name="secretReference" />
          </FieldContent>
        </Field>
      </div>
      <Field>
        <FieldLabel htmlFor={`${prefix}-ssl`}>
          {tCrmClient("crm.supportInbox.fields.ssl")}
        </FieldLabel>
        <FieldContent>
          <BooleanSelect id={`${prefix}-ssl`} name="useSsl" defaultValue={true} />
        </FieldContent>
      </Field>
    </>
  );
}

function RuleFields({ prefix }: Readonly<{ prefix: string }>) {
  return (
    <>
      <Field>
        <FieldLabel htmlFor={`${prefix}-name`}>
          {tCrmClient("crm.supportInbox.fields.name")}
        </FieldLabel>
        <FieldContent>
          <Input id={`${prefix}-name`} name="name" />
        </FieldContent>
      </Field>
      <div className="grid gap-3 sm:grid-cols-2">
        <Field>
          <FieldLabel htmlFor={`${prefix}-sender`}>
            {tCrmClient("crm.supportInbox.fields.matchSender")}
          </FieldLabel>
          <FieldContent>
            <Input id={`${prefix}-sender`} name="matchSender" />
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor={`${prefix}-subject`}>
            {tCrmClient("crm.supportInbox.fields.matchSubject")}
          </FieldLabel>
          <FieldContent>
            <Input id={`${prefix}-subject`} name="matchSubjectContains" />
          </FieldContent>
        </Field>
      </div>
      <div className="grid gap-3 sm:grid-cols-3">
        <Field>
          <FieldLabel htmlFor={`${prefix}-queue`}>
            {tCrmClient("crm.supportInbox.fields.assignToQueueId")}
          </FieldLabel>
          <FieldContent>
            <Input id={`${prefix}-queue`} name="assignToQueueId" />
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor={`${prefix}-category`}>
            {tCrmClient("crm.supportInbox.fields.ticketCategoryId")}
          </FieldLabel>
          <FieldContent>
            <Input id={`${prefix}-category`} name="ticketCategoryId" />
          </FieldContent>
        </Field>
        <Field>
          <FieldLabel htmlFor={`${prefix}-sla`}>
            {tCrmClient("crm.supportInbox.fields.slaPolicyId")}
          </FieldLabel>
          <FieldContent>
            <Input id={`${prefix}-sla`} name="slaPolicyId" />
          </FieldContent>
        </Field>
      </div>
      <Field>
        <FieldLabel htmlFor={`${prefix}-auto`}>
          {tCrmClient("crm.supportInbox.fields.autoCreateTicket")}
        </FieldLabel>
        <FieldContent>
          <BooleanSelect id={`${prefix}-auto`} name="autoCreateTicket" defaultValue={true} />
        </FieldContent>
      </Field>
    </>
  );
}

export function SupportInboxMutationPanels({
  connections,
  createConnectionAction,
  updateConnectionAction,
  syncConnectionAction,
  createRuleAction,
  updateRuleAction,
}: Readonly<{
  connections: SupportInboxConnectionDto[];
  createConnectionAction: (
    state: CrmMutationState,
    formData: FormData,
  ) => Promise<CrmMutationState>;
  updateConnectionAction: (
    state: CrmMutationState,
    formData: FormData,
  ) => Promise<CrmMutationState>;
  syncConnectionAction: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
  createRuleAction: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
  updateRuleAction: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
}>) {
  const [createConnectionState, createConnectionFormAction] = useActionState(
    createConnectionAction,
    initialCrmMutationState,
  );
  const [updateConnectionState, updateConnectionFormAction] = useActionState(
    updateConnectionAction,
    initialCrmMutationState,
  );
  const [syncState, syncFormAction] = useActionState(syncConnectionAction, initialCrmMutationState);
  const [createRuleState, createRuleFormAction] = useActionState(
    createRuleAction,
    initialCrmMutationState,
  );
  const [updateRuleState, updateRuleFormAction] = useActionState(
    updateRuleAction,
    initialCrmMutationState,
  );

  return (
    <div className="grid gap-4 lg:grid-cols-2">
      <Card>
        <CardHeader>
          <CardTitle>{tCrmClient("crm.supportInbox.operations.createConnection.title")}</CardTitle>
          <CardDescription>
            {tCrmClient("crm.supportInbox.operations.createConnection.description")}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <CrmMutationResult state={createConnectionState} />
          <form action={createConnectionFormAction} className="space-y-3">
            <Field>
              <FieldLabel htmlFor="create-support-provider">
                {tCrmClient("crm.supportInbox.fields.provider")}
              </FieldLabel>
              <FieldContent>
                <InlineSelect
                  id="create-support-provider"
                  name="provider"
                  defaultValue="1"
                  options={[
                    { value: "1", label: "IMAP" },
                    { value: "2", label: "Exchange" },
                    { value: "3", label: "Gmail" },
                    { value: "4", label: "Microsoft 365" },
                  ]}
                />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor="create-support-email">
                {tCrmClient("crm.supportInbox.fields.email")}
              </FieldLabel>
              <FieldContent>
                <Input id="create-support-email" name="emailAddress" type="email" />
              </FieldContent>
            </Field>
            <ConnectionFields prefix="create-support" />
            <SubmitButton label={tCrmClient("crm.supportInbox.actions.createConnection")} />
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{tCrmClient("crm.supportInbox.operations.updateConnection.title")}</CardTitle>
          <CardDescription>
            {tCrmClient("crm.supportInbox.operations.updateConnection.description")}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <CrmMutationResult state={updateConnectionState} />
          <form action={updateConnectionFormAction} className="space-y-3">
            <Field>
              <FieldLabel htmlFor="update-support-connection">
                {tCrmClient("crm.supportInbox.fields.connection")}
              </FieldLabel>
              <FieldContent>
                <ConnectionPicker id="update-support-connection" connections={connections} />
              </FieldContent>
            </Field>
            <ConnectionFields prefix="update-support" />
            <Field>
              <FieldLabel htmlFor="update-support-active">
                {tCrmClient("crm.supportInbox.fields.state")}
              </FieldLabel>
              <FieldContent>
                <BooleanSelect id="update-support-active" name="isActive" defaultValue={true} />
              </FieldContent>
            </Field>
            <SubmitButton label={tCrmClient("crm.supportInbox.actions.updateConnection")} />
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{tCrmClient("crm.supportInbox.operations.sync.title")}</CardTitle>
          <CardDescription>
            {tCrmClient("crm.supportInbox.operations.sync.description")}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <CrmMutationResult state={syncState} />
          <form action={syncFormAction} className="space-y-3">
            <Field>
              <FieldLabel htmlFor="sync-support-connection">
                {tCrmClient("crm.supportInbox.fields.connection")}
              </FieldLabel>
              <FieldContent>
                <ConnectionPicker id="sync-support-connection" connections={connections} />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor="sync-support-dry-run">
                {tCrmClient("crm.supportInbox.fields.dryRun")}
              </FieldLabel>
              <FieldContent>
                <BooleanSelect id="sync-support-dry-run" name="dryRun" defaultValue={true} />
              </FieldContent>
            </Field>
            <SubmitButton label={tCrmClient("crm.supportInbox.actions.sync")} />
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{tCrmClient("crm.supportInbox.operations.createRule.title")}</CardTitle>
          <CardDescription>
            {tCrmClient("crm.supportInbox.operations.createRule.description")}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <CrmMutationResult state={createRuleState} />
          <form action={createRuleFormAction} className="space-y-3">
            <Field>
              <FieldLabel htmlFor="create-rule-connection">
                {tCrmClient("crm.supportInbox.fields.connection")}
              </FieldLabel>
              <FieldContent>
                <ConnectionPicker id="create-rule-connection" connections={connections} />
              </FieldContent>
            </Field>
            <RuleFields prefix="create-rule" />
            <SubmitButton label={tCrmClient("crm.supportInbox.actions.createRule")} />
          </form>
        </CardContent>
      </Card>

      <Card className="lg:col-span-2">
        <CardHeader>
          <CardTitle>{tCrmClient("crm.supportInbox.operations.updateRule.title")}</CardTitle>
          <CardDescription>
            {tCrmClient("crm.supportInbox.operations.updateRule.description")}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <CrmMutationResult state={updateRuleState} />
          <form action={updateRuleFormAction} className="space-y-3">
            <Field>
              <FieldLabel htmlFor="update-rule-id">
                {tCrmClient("crm.supportInbox.fields.ruleId")}
              </FieldLabel>
              <FieldContent>
                <Input id="update-rule-id" name="ruleId" />
              </FieldContent>
            </Field>
            <RuleFields prefix="update-rule" />
            <Field>
              <FieldLabel htmlFor="update-rule-active">
                {tCrmClient("crm.supportInbox.fields.state")}
              </FieldLabel>
              <FieldContent>
                <BooleanSelect id="update-rule-active" name="isActive" defaultValue={true} />
              </FieldContent>
            </Field>
            <SubmitButton label={tCrmClient("crm.supportInbox.actions.updateRule")} />
          </form>
        </CardContent>
      </Card>
    </div>
  );
}

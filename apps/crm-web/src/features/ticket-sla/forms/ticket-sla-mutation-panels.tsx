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

export function TicketSlaMutationPanels({
  createPolicyAction,
  updatePolicyAction,
  deletePolicyAction,
  createRuleAction,
  updateRuleAction,
}: Readonly<{
  createPolicyAction: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
  updatePolicyAction: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
  deletePolicyAction: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
  createRuleAction: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
  updateRuleAction: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
}>) {
  const [createPolicyState, createPolicyFormAction] = useActionState(
    createPolicyAction,
    initialCrmMutationState,
  );
  const [updatePolicyState, updatePolicyFormAction] = useActionState(
    updatePolicyAction,
    initialCrmMutationState,
  );
  const [deletePolicyState, deletePolicyFormAction] = useActionState(
    deletePolicyAction,
    initialCrmMutationState,
  );
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
          <CardTitle>{tCrmClient("crm.ticketSla.mutations.createPolicy.title")}</CardTitle>
          <CardDescription>
            {tCrmClient("crm.ticketSla.mutations.createPolicy.description")}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <CrmMutationResult state={createPolicyState} />
          <form action={createPolicyFormAction} className="space-y-3">
            <Field>
              <FieldLabel htmlFor="create-policy-name">
                {tCrmClient("crm.ticketSla.fields.name")}
              </FieldLabel>
              <FieldContent>
                <Input id="create-policy-name" name="name" />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor="create-policy-category">
                {tCrmClient("crm.ticketSla.fields.ticketCategoryId")}
              </FieldLabel>
              <FieldContent>
                <Input id="create-policy-category" name="ticketCategoryId" />
              </FieldContent>
            </Field>
            <div className="grid gap-3 sm:grid-cols-3">
              <Field>
                <FieldLabel htmlFor="create-policy-priority">
                  {tCrmClient("crm.ticketSla.fields.priority")}
                </FieldLabel>
                <FieldContent>
                  <Input
                    id="create-policy-priority"
                    name="priority"
                    type="number"
                    defaultValue="1"
                  />
                </FieldContent>
              </Field>
              <Field>
                <FieldLabel htmlFor="create-policy-first">
                  {tCrmClient("crm.ticketSla.fields.firstResponseTargetMinutes")}
                </FieldLabel>
                <FieldContent>
                  <Input
                    id="create-policy-first"
                    name="firstResponseTargetMinutes"
                    type="number"
                    defaultValue="30"
                  />
                </FieldContent>
              </Field>
              <Field>
                <FieldLabel htmlFor="create-policy-resolution">
                  {tCrmClient("crm.ticketSla.fields.resolutionTargetMinutes")}
                </FieldLabel>
                <FieldContent>
                  <Input
                    id="create-policy-resolution"
                    name="resolutionTargetMinutes"
                    type="number"
                    defaultValue="240"
                  />
                </FieldContent>
              </Field>
            </div>
            <Field>
              <FieldLabel htmlFor="create-policy-default">
                {tCrmClient("crm.ticketSla.fields.isDefault")}
              </FieldLabel>
              <FieldContent>
                <InlineSelect
                  id="create-policy-default"
                  name="isDefault"
                  defaultValue="false"
                  options={[
                    { value: "false", label: tCrmClient("crm.common.boolean.false") },
                    { value: "true", label: tCrmClient("crm.common.boolean.true") },
                  ]}
                />
              </FieldContent>
            </Field>
            <SubmitButton label={tCrmClient("crm.ticketSla.actions.createPolicy")} />
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{tCrmClient("crm.ticketSla.mutations.updatePolicy.title")}</CardTitle>
          <CardDescription>
            {tCrmClient("crm.ticketSla.mutations.updatePolicy.description")}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <CrmMutationResult state={updatePolicyState} />
          <form action={updatePolicyFormAction} className="space-y-3">
            <Field>
              <FieldLabel htmlFor="update-policy-id">
                {tCrmClient("crm.ticketSla.fields.policyId")}
              </FieldLabel>
              <FieldContent>
                <Input id="update-policy-id" name="policyId" />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor="update-policy-name">
                {tCrmClient("crm.ticketSla.fields.name")}
              </FieldLabel>
              <FieldContent>
                <Input id="update-policy-name" name="name" />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor="update-policy-category">
                {tCrmClient("crm.ticketSla.fields.ticketCategoryId")}
              </FieldLabel>
              <FieldContent>
                <Input id="update-policy-category" name="ticketCategoryId" />
              </FieldContent>
            </Field>
            <div className="grid gap-3 sm:grid-cols-3">
              <Field>
                <FieldLabel htmlFor="update-policy-priority">
                  {tCrmClient("crm.ticketSla.fields.priority")}
                </FieldLabel>
                <FieldContent>
                  <Input
                    id="update-policy-priority"
                    name="priority"
                    type="number"
                    defaultValue="1"
                  />
                </FieldContent>
              </Field>
              <Field>
                <FieldLabel htmlFor="update-policy-first">
                  {tCrmClient("crm.ticketSla.fields.firstResponseTargetMinutes")}
                </FieldLabel>
                <FieldContent>
                  <Input
                    id="update-policy-first"
                    name="firstResponseTargetMinutes"
                    type="number"
                    defaultValue="30"
                  />
                </FieldContent>
              </Field>
              <Field>
                <FieldLabel htmlFor="update-policy-resolution">
                  {tCrmClient("crm.ticketSla.fields.resolutionTargetMinutes")}
                </FieldLabel>
                <FieldContent>
                  <Input
                    id="update-policy-resolution"
                    name="resolutionTargetMinutes"
                    type="number"
                    defaultValue="240"
                  />
                </FieldContent>
              </Field>
            </div>
            <Field>
              <FieldLabel htmlFor="update-policy-default">
                {tCrmClient("crm.ticketSla.fields.isDefault")}
              </FieldLabel>
              <FieldContent>
                <InlineSelect
                  id="update-policy-default"
                  name="isDefault"
                  defaultValue="false"
                  options={[
                    { value: "false", label: tCrmClient("crm.common.boolean.false") },
                    { value: "true", label: tCrmClient("crm.common.boolean.true") },
                  ]}
                />
              </FieldContent>
            </Field>
            <SubmitButton label={tCrmClient("crm.ticketSla.actions.updatePolicy")} />
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{tCrmClient("crm.ticketSla.mutations.deletePolicy.title")}</CardTitle>
          <CardDescription>
            {tCrmClient("crm.ticketSla.mutations.deletePolicy.description")}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <CrmMutationResult state={deletePolicyState} />
          <form action={deletePolicyFormAction} className="space-y-3">
            <input type="hidden" name="confirm" value="delete-sla-policy" />
            <Field>
              <FieldLabel htmlFor="delete-policy-id">
                {tCrmClient("crm.ticketSla.fields.policyId")}
              </FieldLabel>
              <FieldContent>
                <Input id="delete-policy-id" name="policyId" />
              </FieldContent>
            </Field>
            <SubmitButton label={tCrmClient("crm.ticketSla.actions.deletePolicy")} />
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{tCrmClient("crm.ticketSla.mutations.createRule.title")}</CardTitle>
          <CardDescription>
            {tCrmClient("crm.ticketSla.mutations.createRule.description")}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <CrmMutationResult state={createRuleState} />
          <form action={createRuleFormAction} className="space-y-3">
            <Field>
              <FieldLabel htmlFor="create-rule-policy">
                {tCrmClient("crm.ticketSla.fields.slaPolicyId")}
              </FieldLabel>
              <FieldContent>
                <Input id="create-rule-policy" name="slaPolicyId" />
              </FieldContent>
            </Field>
            <div className="grid gap-3 sm:grid-cols-2">
              <Field>
                <FieldLabel htmlFor="create-rule-metric">
                  {tCrmClient("crm.ticketSla.fields.metricType")}
                </FieldLabel>
                <FieldContent>
                  <InlineSelect
                    id="create-rule-metric"
                    name="metricType"
                    defaultValue="FirstResponse"
                    options={[
                      {
                        value: "FirstResponse",
                        label: tCrmClient("crm.ticketSla.metric.firstResponse"),
                      },
                      { value: "Resolution", label: tCrmClient("crm.ticketSla.metric.resolution") },
                    ]}
                  />
                </FieldContent>
              </Field>
              <Field>
                <FieldLabel htmlFor="create-rule-action">
                  {tCrmClient("crm.ticketSla.fields.actionType")}
                </FieldLabel>
                <FieldContent>
                  <InlineSelect
                    id="create-rule-action"
                    name="actionType"
                    defaultValue="NotifyOwner"
                    options={[
                      { value: "None", label: tCrmClient("crm.ticketSla.action.none") },
                      {
                        value: "NotifyOwner",
                        label: tCrmClient("crm.ticketSla.action.notifyOwner"),
                      },
                      {
                        value: "NotifyManager",
                        label: tCrmClient("crm.ticketSla.action.notifyManager"),
                      },
                      {
                        value: "ReassignQueue",
                        label: tCrmClient("crm.ticketSla.action.reassignQueue"),
                      },
                      {
                        value: "IncreasePriority",
                        label: tCrmClient("crm.ticketSla.action.increasePriority"),
                      },
                      {
                        value: "EscalateToTeam",
                        label: tCrmClient("crm.ticketSla.action.escalateToTeam"),
                      },
                    ]}
                  />
                </FieldContent>
              </Field>
            </div>
            <Field>
              <FieldLabel htmlFor="create-rule-trigger">
                {tCrmClient("crm.ticketSla.fields.triggerMinutes")}
              </FieldLabel>
              <FieldContent>
                <Input
                  id="create-rule-trigger"
                  name="triggerBeforeOrAfterMinutes"
                  type="number"
                  defaultValue="0"
                />
              </FieldContent>
            </Field>
            <div className="grid gap-3 sm:grid-cols-2">
              <Field>
                <FieldLabel htmlFor="create-rule-team">
                  {tCrmClient("crm.ticketSla.fields.targetTeamId")}
                </FieldLabel>
                <FieldContent>
                  <Input id="create-rule-team" name="targetTeamId" />
                </FieldContent>
              </Field>
              <Field>
                <FieldLabel htmlFor="create-rule-user">
                  {tCrmClient("crm.ticketSla.fields.targetUserId")}
                </FieldLabel>
                <FieldContent>
                  <Input id="create-rule-user" name="targetUserId" />
                </FieldContent>
              </Field>
            </div>
            <Field>
              <FieldLabel htmlFor="create-rule-enabled">
                {tCrmClient("crm.ticketSla.fields.enabled")}
              </FieldLabel>
              <FieldContent>
                <InlineSelect
                  id="create-rule-enabled"
                  name="isEnabled"
                  defaultValue="true"
                  options={[
                    { value: "true", label: tCrmClient("crm.common.boolean.true") },
                    { value: "false", label: tCrmClient("crm.common.boolean.false") },
                  ]}
                />
              </FieldContent>
            </Field>
            <SubmitButton label={tCrmClient("crm.ticketSla.actions.createRule")} />
          </form>
        </CardContent>
      </Card>

      <Card className="lg:col-span-2">
        <CardHeader>
          <CardTitle>{tCrmClient("crm.ticketSla.mutations.updateRule.title")}</CardTitle>
          <CardDescription>
            {tCrmClient("crm.ticketSla.mutations.updateRule.description")}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <CrmMutationResult state={updateRuleState} />
          <form action={updateRuleFormAction} className="space-y-3">
            <div className="grid gap-3 sm:grid-cols-2">
              <Field>
                <FieldLabel htmlFor="update-rule-id">
                  {tCrmClient("crm.ticketSla.fields.ruleId")}
                </FieldLabel>
                <FieldContent>
                  <Input id="update-rule-id" name="ruleId" />
                </FieldContent>
              </Field>
              <Field>
                <FieldLabel htmlFor="update-rule-policy">
                  {tCrmClient("crm.ticketSla.fields.slaPolicyId")}
                </FieldLabel>
                <FieldContent>
                  <Input id="update-rule-policy" name="slaPolicyId" />
                </FieldContent>
              </Field>
            </div>
            <div className="grid gap-3 sm:grid-cols-2">
              <Field>
                <FieldLabel htmlFor="update-rule-metric">
                  {tCrmClient("crm.ticketSla.fields.metricType")}
                </FieldLabel>
                <FieldContent>
                  <InlineSelect
                    id="update-rule-metric"
                    name="metricType"
                    defaultValue="FirstResponse"
                    options={[
                      {
                        value: "FirstResponse",
                        label: tCrmClient("crm.ticketSla.metric.firstResponse"),
                      },
                      { value: "Resolution", label: tCrmClient("crm.ticketSla.metric.resolution") },
                    ]}
                  />
                </FieldContent>
              </Field>
              <Field>
                <FieldLabel htmlFor="update-rule-action">
                  {tCrmClient("crm.ticketSla.fields.actionType")}
                </FieldLabel>
                <FieldContent>
                  <InlineSelect
                    id="update-rule-action"
                    name="actionType"
                    defaultValue="NotifyOwner"
                    options={[
                      { value: "None", label: tCrmClient("crm.ticketSla.action.none") },
                      {
                        value: "NotifyOwner",
                        label: tCrmClient("crm.ticketSla.action.notifyOwner"),
                      },
                      {
                        value: "NotifyManager",
                        label: tCrmClient("crm.ticketSla.action.notifyManager"),
                      },
                      {
                        value: "ReassignQueue",
                        label: tCrmClient("crm.ticketSla.action.reassignQueue"),
                      },
                      {
                        value: "IncreasePriority",
                        label: tCrmClient("crm.ticketSla.action.increasePriority"),
                      },
                      {
                        value: "EscalateToTeam",
                        label: tCrmClient("crm.ticketSla.action.escalateToTeam"),
                      },
                    ]}
                  />
                </FieldContent>
              </Field>
            </div>
            <Field>
              <FieldLabel htmlFor="update-rule-trigger">
                {tCrmClient("crm.ticketSla.fields.triggerMinutes")}
              </FieldLabel>
              <FieldContent>
                <Input
                  id="update-rule-trigger"
                  name="triggerBeforeOrAfterMinutes"
                  type="number"
                  defaultValue="0"
                />
              </FieldContent>
            </Field>
            <div className="grid gap-3 sm:grid-cols-2">
              <Field>
                <FieldLabel htmlFor="update-rule-team">
                  {tCrmClient("crm.ticketSla.fields.targetTeamId")}
                </FieldLabel>
                <FieldContent>
                  <Input id="update-rule-team" name="targetTeamId" />
                </FieldContent>
              </Field>
              <Field>
                <FieldLabel htmlFor="update-rule-user">
                  {tCrmClient("crm.ticketSla.fields.targetUserId")}
                </FieldLabel>
                <FieldContent>
                  <Input id="update-rule-user" name="targetUserId" />
                </FieldContent>
              </Field>
            </div>
            <Field>
              <FieldLabel htmlFor="update-rule-enabled">
                {tCrmClient("crm.ticketSla.fields.enabled")}
              </FieldLabel>
              <FieldContent>
                <InlineSelect
                  id="update-rule-enabled"
                  name="isEnabled"
                  defaultValue="true"
                  options={[
                    { value: "true", label: tCrmClient("crm.common.boolean.true") },
                    { value: "false", label: tCrmClient("crm.common.boolean.false") },
                  ]}
                />
              </FieldContent>
            </Field>
            <SubmitButton label={tCrmClient("crm.ticketSla.actions.updateRule")} />
          </form>
        </CardContent>
      </Card>
    </div>
  );
}

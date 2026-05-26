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
  Textarea,
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

export function TicketWorkflowMutationPanels({
  createQueueAction,
  updateQueueAction,
  deleteQueueAction,
  assignQueueAction,
  assignOwnerAction,
  statusChangeAction,
}: Readonly<{
  createQueueAction: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
  updateQueueAction: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
  deleteQueueAction: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
  assignQueueAction: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
  assignOwnerAction: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
  statusChangeAction: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
}>) {
  const [createQueueState, createQueueFormAction] = useActionState(
    createQueueAction,
    initialCrmMutationState,
  );
  const [updateQueueState, updateQueueFormAction] = useActionState(
    updateQueueAction,
    initialCrmMutationState,
  );
  const [deleteQueueState, deleteQueueFormAction] = useActionState(
    deleteQueueAction,
    initialCrmMutationState,
  );
  const [assignQueueState, assignQueueFormAction] = useActionState(
    assignQueueAction,
    initialCrmMutationState,
  );
  const [assignOwnerState, assignOwnerFormAction] = useActionState(
    assignOwnerAction,
    initialCrmMutationState,
  );
  const [statusChangeState, statusChangeFormAction] = useActionState(
    statusChangeAction,
    initialCrmMutationState,
  );

  return (
    <div className="grid gap-4 lg:grid-cols-2">
      <Card>
        <CardHeader>
          <CardTitle>{tCrmClient("crm.ticketWorkflows.mutations.createQueue.title")}</CardTitle>
          <CardDescription>
            {tCrmClient("crm.ticketWorkflows.mutations.createQueue.description")}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <CrmMutationResult state={createQueueState} />
          <form action={createQueueFormAction} className="space-y-3">
            <Field>
              <FieldLabel htmlFor="create-queue-code">
                {tCrmClient("crm.ticketWorkflows.fields.code")}
              </FieldLabel>
              <FieldContent>
                <Input id="create-queue-code" name="code" />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor="create-queue-name">
                {tCrmClient("crm.ticketWorkflows.fields.name")}
              </FieldLabel>
              <FieldContent>
                <Input id="create-queue-name" name="name" />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor="create-queue-description">
                {tCrmClient("crm.ticketWorkflows.fields.description")}
              </FieldLabel>
              <FieldContent>
                <Textarea id="create-queue-description" name="description" rows={3} />
              </FieldContent>
            </Field>
            <div className="grid gap-3 sm:grid-cols-2">
              <Field>
                <FieldLabel htmlFor="create-queue-strategy">
                  {tCrmClient("crm.ticketWorkflows.fields.assignmentStrategy")}
                </FieldLabel>
                <FieldContent>
                  <InlineSelect
                    id="create-queue-strategy"
                    name="assignmentStrategy"
                    defaultValue="1"
                    options={[
                      { value: "1", label: tCrmClient("crm.ticketWorkflows.strategy.manual") },
                      { value: "2", label: tCrmClient("crm.ticketWorkflows.strategy.roundRobin") },
                      { value: "3", label: tCrmClient("crm.ticketWorkflows.strategy.leastLoaded") },
                    ]}
                  />
                </FieldContent>
              </Field>
              <Field>
                <FieldLabel htmlFor="create-queue-default">
                  {tCrmClient("crm.ticketWorkflows.fields.isDefault")}
                </FieldLabel>
                <FieldContent>
                  <InlineSelect
                    id="create-queue-default"
                    name="isDefault"
                    defaultValue="false"
                    options={[
                      { value: "false", label: tCrmClient("crm.common.boolean.false") },
                      { value: "true", label: tCrmClient("crm.common.boolean.true") },
                    ]}
                  />
                </FieldContent>
              </Field>
            </div>
            <SubmitButton label={tCrmClient("crm.ticketWorkflows.actions.createQueue")} />
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{tCrmClient("crm.ticketWorkflows.mutations.updateQueue.title")}</CardTitle>
          <CardDescription>
            {tCrmClient("crm.ticketWorkflows.mutations.updateQueue.description")}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <CrmMutationResult state={updateQueueState} />
          <form action={updateQueueFormAction} className="space-y-3">
            <Field>
              <FieldLabel htmlFor="update-queue-id">
                {tCrmClient("crm.ticketWorkflows.fields.queueId")}
              </FieldLabel>
              <FieldContent>
                <Input id="update-queue-id" name="queueId" />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor="update-queue-name">
                {tCrmClient("crm.ticketWorkflows.fields.name")}
              </FieldLabel>
              <FieldContent>
                <Input id="update-queue-name" name="name" />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor="update-queue-description">
                {tCrmClient("crm.ticketWorkflows.fields.description")}
              </FieldLabel>
              <FieldContent>
                <Textarea id="update-queue-description" name="description" rows={3} />
              </FieldContent>
            </Field>
            <div className="grid gap-3 sm:grid-cols-2">
              <Field>
                <FieldLabel htmlFor="update-queue-strategy">
                  {tCrmClient("crm.ticketWorkflows.fields.assignmentStrategy")}
                </FieldLabel>
                <FieldContent>
                  <InlineSelect
                    id="update-queue-strategy"
                    name="assignmentStrategy"
                    defaultValue="1"
                    options={[
                      { value: "1", label: tCrmClient("crm.ticketWorkflows.strategy.manual") },
                      { value: "2", label: tCrmClient("crm.ticketWorkflows.strategy.roundRobin") },
                      { value: "3", label: tCrmClient("crm.ticketWorkflows.strategy.leastLoaded") },
                    ]}
                  />
                </FieldContent>
              </Field>
              <Field>
                <FieldLabel htmlFor="update-queue-default">
                  {tCrmClient("crm.ticketWorkflows.fields.isDefault")}
                </FieldLabel>
                <FieldContent>
                  <InlineSelect
                    id="update-queue-default"
                    name="isDefault"
                    defaultValue="false"
                    options={[
                      { value: "false", label: tCrmClient("crm.common.boolean.false") },
                      { value: "true", label: tCrmClient("crm.common.boolean.true") },
                    ]}
                  />
                </FieldContent>
              </Field>
            </div>
            <SubmitButton label={tCrmClient("crm.ticketWorkflows.actions.updateQueue")} />
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{tCrmClient("crm.ticketWorkflows.mutations.deleteQueue.title")}</CardTitle>
          <CardDescription>
            {tCrmClient("crm.ticketWorkflows.mutations.deleteQueue.description")}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <CrmMutationResult state={deleteQueueState} />
          <form action={deleteQueueFormAction} className="space-y-3">
            <input type="hidden" name="confirm" value="delete-ticket-workflow-queue" />
            <Field>
              <FieldLabel htmlFor="delete-queue-id">
                {tCrmClient("crm.ticketWorkflows.fields.queueId")}
              </FieldLabel>
              <FieldContent>
                <Input id="delete-queue-id" name="queueId" />
              </FieldContent>
            </Field>
            <SubmitButton label={tCrmClient("crm.ticketWorkflows.actions.deleteQueue")} />
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{tCrmClient("crm.ticketWorkflows.mutations.assignQueue.title")}</CardTitle>
          <CardDescription>
            {tCrmClient("crm.ticketWorkflows.mutations.assignQueue.description")}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <CrmMutationResult state={assignQueueState} />
          <form action={assignQueueFormAction} className="space-y-3">
            <Field>
              <FieldLabel htmlFor="assign-queue-ticket">
                {tCrmClient("crm.ticketWorkflows.fields.ticketId")}
              </FieldLabel>
              <FieldContent>
                <Input id="assign-queue-ticket" name="ticketId" />
              </FieldContent>
            </Field>
            <div className="grid gap-3 sm:grid-cols-2">
              <Field>
                <FieldLabel htmlFor="assign-queue-prev">
                  {tCrmClient("crm.ticketWorkflows.fields.previousQueueId")}
                </FieldLabel>
                <FieldContent>
                  <Input id="assign-queue-prev" name="previousQueueId" />
                </FieldContent>
              </Field>
              <Field>
                <FieldLabel htmlFor="assign-queue-new">
                  {tCrmClient("crm.ticketWorkflows.fields.newQueueId")}
                </FieldLabel>
                <FieldContent>
                  <Input id="assign-queue-new" name="newQueueId" />
                </FieldContent>
              </Field>
            </div>
            <Field>
              <FieldLabel htmlFor="assign-queue-reason">
                {tCrmClient("crm.ticketWorkflows.fields.reason")}
              </FieldLabel>
              <FieldContent>
                <Textarea id="assign-queue-reason" name="reason" rows={3} />
              </FieldContent>
            </Field>
            <SubmitButton label={tCrmClient("crm.ticketWorkflows.actions.assignQueue")} />
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{tCrmClient("crm.ticketWorkflows.mutations.assignOwner.title")}</CardTitle>
          <CardDescription>
            {tCrmClient("crm.ticketWorkflows.mutations.assignOwner.description")}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <CrmMutationResult state={assignOwnerState} />
          <form action={assignOwnerFormAction} className="space-y-3">
            <Field>
              <FieldLabel htmlFor="assign-owner-ticket">
                {tCrmClient("crm.ticketWorkflows.fields.ticketId")}
              </FieldLabel>
              <FieldContent>
                <Input id="assign-owner-ticket" name="ticketId" />
              </FieldContent>
            </Field>
            <div className="grid gap-3 sm:grid-cols-2">
              <Field>
                <FieldLabel htmlFor="assign-owner-prev">
                  {tCrmClient("crm.ticketWorkflows.fields.previousOwnerId")}
                </FieldLabel>
                <FieldContent>
                  <Input id="assign-owner-prev" name="previousOwnerUserId" />
                </FieldContent>
              </Field>
              <Field>
                <FieldLabel htmlFor="assign-owner-new">
                  {tCrmClient("crm.ticketWorkflows.fields.newOwnerId")}
                </FieldLabel>
                <FieldContent>
                  <Input id="assign-owner-new" name="newOwnerUserId" />
                </FieldContent>
              </Field>
            </div>
            <Field>
              <FieldLabel htmlFor="assign-owner-queue">
                {tCrmClient("crm.ticketWorkflows.fields.queueId")}
              </FieldLabel>
              <FieldContent>
                <Input id="assign-owner-queue" name="queueId" />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor="assign-owner-reason">
                {tCrmClient("crm.ticketWorkflows.fields.reason")}
              </FieldLabel>
              <FieldContent>
                <Textarea id="assign-owner-reason" name="reason" rows={3} />
              </FieldContent>
            </Field>
            <SubmitButton label={tCrmClient("crm.ticketWorkflows.actions.assignOwner")} />
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{tCrmClient("crm.ticketWorkflows.mutations.changeStatus.title")}</CardTitle>
          <CardDescription>
            {tCrmClient("crm.ticketWorkflows.mutations.changeStatus.description")}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-3">
          <CrmMutationResult state={statusChangeState} />
          <form action={statusChangeFormAction} className="space-y-3">
            <input type="hidden" name="confirm" value="change-ticket-workflow-status" />
            <Field>
              <FieldLabel htmlFor="status-ticket-id">
                {tCrmClient("crm.ticketWorkflows.fields.ticketId")}
              </FieldLabel>
              <FieldContent>
                <Input id="status-ticket-id" name="ticketId" />
              </FieldContent>
            </Field>
            <div className="grid gap-3 sm:grid-cols-2">
              <Field>
                <FieldLabel htmlFor="status-prev">
                  {tCrmClient("crm.ticketWorkflows.fields.previousStatus")}
                </FieldLabel>
                <FieldContent>
                  <Input id="status-prev" name="previousStatus" />
                </FieldContent>
              </Field>
              <Field>
                <FieldLabel htmlFor="status-new">
                  {tCrmClient("crm.ticketWorkflows.fields.newStatus")}
                </FieldLabel>
                <FieldContent>
                  <Input id="status-new" name="newStatus" />
                </FieldContent>
              </Field>
            </div>
            <Field>
              <FieldLabel htmlFor="status-note">
                {tCrmClient("crm.ticketWorkflows.fields.note")}
              </FieldLabel>
              <FieldContent>
                <Textarea id="status-note" name="note" rows={3} />
              </FieldContent>
            </Field>
            <SubmitButton label={tCrmClient("crm.ticketWorkflows.actions.changeStatus")} />
          </form>
        </CardContent>
      </Card>
    </div>
  );
}

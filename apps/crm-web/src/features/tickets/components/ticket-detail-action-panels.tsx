"use client";

import { useActionState } from "react";
import { useFormStatus } from "react-dom";
import { Button, Field, FieldContent, FieldLabel, Input, Textarea } from "@netmetric/ui";

import { CrmMutationResult } from "@/components/forms/crm-mutation-result";
import {
  attachTicketSlaToTicketAction,
  markTicketFirstResponseAction,
  markTicketResolvedAction,
  runDueTicketEscalationsAction,
} from "@/features/ticket-sla/actions/ticket-sla-mutation-actions";
import {
  assignTicketWorkflowOwnerAction,
  assignTicketWorkflowQueueAction,
  changeTicketWorkflowStatusAction,
} from "@/features/ticket-workflows/actions/ticket-workflow-mutation-actions";
import { initialCrmMutationState } from "@/features/shared/actions/mutation-state";
import type { TicketDetailDto, TicketSlaPolicyDto, TicketWorkflowQueueDto } from "@/lib/crm-api";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

function SubmitButton({ label }: Readonly<{ label: string }>) {
  const { pending } = useFormStatus();

  return (
    <Button type="submit" disabled={pending} aria-busy={pending}>
      {pending ? tCrmClient("crm.forms.actions.processing") : label}
    </Button>
  );
}

export function TicketDetailActionPanels({
  ticket,
  queues,
  policies,
  currentQueueId,
  canManageWorkflow,
  canManageSla,
}: Readonly<{
  ticket: TicketDetailDto;
  queues: TicketWorkflowQueueDto[];
  policies: TicketSlaPolicyDto[];
  currentQueueId?: string | null;
  canManageWorkflow: boolean;
  canManageSla: boolean;
}>) {
  const [queueState, queueAction] = useActionState(
    assignTicketWorkflowQueueAction,
    initialCrmMutationState,
  );
  const [ownerState, ownerAction] = useActionState(
    assignTicketWorkflowOwnerAction,
    initialCrmMutationState,
  );
  const [statusState, statusAction] = useActionState(
    changeTicketWorkflowStatusAction,
    initialCrmMutationState,
  );
  const [attachSlaState, attachSlaAction] = useActionState(
    attachTicketSlaToTicketAction,
    initialCrmMutationState,
  );
  const [firstResponseState, firstResponseAction] = useActionState(
    markTicketFirstResponseAction,
    initialCrmMutationState,
  );
  const [resolvedState, resolvedAction] = useActionState(
    markTicketResolvedAction,
    initialCrmMutationState,
  );
  const [runDueState, runDueAction] = useActionState(
    runDueTicketEscalationsAction,
    initialCrmMutationState,
  );

  return (
    <div className="grid gap-6 xl:grid-cols-2">
      {canManageWorkflow ? (
        <div className="space-y-5">
          <PanelTitle title={tCrmClient("crm.tickets.workflow.actionsTitle")} />
          <form action={queueAction} className="space-y-3">
            <CrmMutationResult state={queueState} />
            <input type="hidden" name="ticketId" value={ticket.id} />
            <input type="hidden" name="previousQueueId" value={currentQueueId ?? ""} />
            <Field>
              <FieldLabel htmlFor="ticket-detail-new-queue">
                {tCrmClient("crm.ticketWorkflows.fields.queueId")}
              </FieldLabel>
              <FieldContent>
                <select
                  className="h-10 w-full rounded-md border border-input bg-background px-3 text-sm"
                  defaultValue={currentQueueId ?? ""}
                  id="ticket-detail-new-queue"
                  name="newQueueId"
                  required
                >
                  <option value="">{tCrmClient("crm.tickets.workflow.selectQueue")}</option>
                  {queues.map((queue) => (
                    <option key={queue.id} value={queue.id}>
                      {queue.name}
                    </option>
                  ))}
                </select>
              </FieldContent>
            </Field>
            <TextareaField id="ticket-detail-queue-reason" name="reason" />
            <SubmitButton label={tCrmClient("crm.ticketWorkflows.actions.assignQueue")} />
          </form>

          <form action={ownerAction} className="space-y-3">
            <CrmMutationResult state={ownerState} />
            <input type="hidden" name="ticketId" value={ticket.id} />
            <input type="hidden" name="previousOwnerUserId" value={ticket.assignedUserId ?? ""} />
            <input type="hidden" name="queueId" value={currentQueueId ?? ""} />
            <TextField
              id="ticket-detail-new-owner"
              label={tCrmClient("crm.tickets.fields.assignedUserId")}
              name="newOwnerUserId"
              required
            />
            <TextareaField id="ticket-detail-owner-reason" name="reason" />
            <SubmitButton label={tCrmClient("crm.ticketWorkflows.actions.assignOwner")} />
          </form>

          <form action={statusAction} className="space-y-3">
            <CrmMutationResult state={statusState} />
            <input type="hidden" name="confirm" value="change-ticket-workflow-status" />
            <input type="hidden" name="ticketId" value={ticket.id} />
            <input type="hidden" name="previousStatus" value={String(ticket.status)} />
            <TextField
              defaultValue={String(ticket.status)}
              id="ticket-detail-new-status"
              label={tCrmClient("crm.tickets.fields.status")}
              name="newStatus"
              required
            />
            <TextareaField id="ticket-detail-status-note" name="note" />
            <SubmitButton label={tCrmClient("crm.ticketWorkflows.actions.changeStatus")} />
          </form>
        </div>
      ) : null}

      {canManageSla ? (
        <div className="space-y-5">
          <PanelTitle title={tCrmClient("crm.tickets.sla.actionsTitle")} />
          <form action={attachSlaAction} className="space-y-3">
            <CrmMutationResult state={attachSlaState} />
            <input type="hidden" name="ticketId" value={ticket.id} />
            <Field>
              <FieldLabel htmlFor="ticket-detail-sla-policy">
                {tCrmClient("crm.ticketSla.fields.policyId")}
              </FieldLabel>
              <FieldContent>
                <select
                  className="h-10 w-full rounded-md border border-input bg-background px-3 text-sm"
                  defaultValue={ticket.slaPolicyId ?? ""}
                  id="ticket-detail-sla-policy"
                  name="slaPolicyId"
                  required
                >
                  <option value="">{tCrmClient("crm.tickets.sla.selectPolicy")}</option>
                  {policies.map((policy) => (
                    <option key={policy.id} value={policy.id}>
                      {policy.name}
                    </option>
                  ))}
                </select>
              </FieldContent>
            </Field>
            <DateTimeField id="ticket-detail-sla-created" name="createdAtUtc" />
            <SubmitButton label={tCrmClient("crm.tickets.sla.attach")} />
          </form>

          <form action={firstResponseAction} className="space-y-3">
            <CrmMutationResult state={firstResponseState} />
            <input type="hidden" name="ticketId" value={ticket.id} />
            <DateTimeField id="ticket-detail-first-response" name="respondedAtUtc" />
            <SubmitButton label={tCrmClient("crm.tickets.sla.firstResponse")} />
          </form>

          <form action={resolvedAction} className="space-y-3">
            <CrmMutationResult state={resolvedState} />
            <input type="hidden" name="ticketId" value={ticket.id} />
            <DateTimeField id="ticket-detail-resolved" name="resolvedAtUtc" />
            <SubmitButton label={tCrmClient("crm.tickets.sla.resolved")} />
          </form>

          <form action={runDueAction} className="space-y-3">
            <CrmMutationResult state={runDueState} />
            <input type="hidden" name="ticketId" value={ticket.id} />
            <DateTimeField id="ticket-detail-run-due" name="utcNow" />
            <SubmitButton label={tCrmClient("crm.tickets.sla.runDue")} />
          </form>
        </div>
      ) : null}
    </div>
  );
}

function PanelTitle({ title }: Readonly<{ title: string }>) {
  return <h3 className="text-sm font-medium">{title}</h3>;
}

function TextField({
  defaultValue,
  id,
  label,
  name,
  required,
}: Readonly<{
  defaultValue?: string | null | undefined;
  id: string;
  label: string;
  name: string;
  required?: boolean;
}>) {
  return (
    <Field>
      <FieldLabel htmlFor={id}>{label}</FieldLabel>
      <FieldContent>
        <Input defaultValue={defaultValue ?? ""} id={id} name={name} required={required} />
      </FieldContent>
    </Field>
  );
}

function TextareaField({ id, name }: Readonly<{ id: string; name: string }>) {
  return (
    <Field>
      <FieldLabel htmlFor={id}>{tCrmClient("crm.tickets.workflow.reason")}</FieldLabel>
      <FieldContent>
        <Textarea id={id} name={name} rows={3} />
      </FieldContent>
    </Field>
  );
}

function DateTimeField({ id, name }: Readonly<{ id: string; name: string }>) {
  return (
    <Field>
      <FieldLabel htmlFor={id}>{tCrmClient("crm.tickets.sla.occurredAt")}</FieldLabel>
      <FieldContent>
        <Input id={id} name={name} type="datetime-local" />
      </FieldContent>
    </Field>
  );
}

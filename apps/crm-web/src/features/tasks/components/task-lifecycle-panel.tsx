"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import {
  Button,
  Field,
  FieldContent,
  FieldError,
  FieldLabel,
  Input,
  Textarea,
} from "@netmetric/ui";

import { CrmFormFeedback } from "@/components/forms/crm-form-feedback";
import { initialCrmMutationState } from "@/features/shared/actions/mutation-state";

import {
  completeTaskAction,
  deleteTaskAction,
  reopenTaskAction,
  updateTaskDueDateAction,
  updateTaskReminderAction,
} from "../actions/work-management-task-lifecycle-actions";

type TaskLifecyclePanelProps = {
  taskId: string;
  status: string;
  dueAtUtc: string;
  reminderAtUtc?: string | null;
  canEdit: boolean;
  canManage: boolean;
  canDelete: boolean;
};

function toLocalDateTimeInput(value?: string | null): string {
  if (!value) {
    return "";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "";
  }

  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  const hour = String(date.getHours()).padStart(2, "0");
  const minute = String(date.getMinutes()).padStart(2, "0");
  return `${year}-${month}-${day}T${hour}:${minute}`;
}

export function TaskLifecyclePanel({
  taskId,
  status,
  dueAtUtc,
  reminderAtUtc,
  canEdit,
  canManage,
  canDelete,
}: Readonly<TaskLifecyclePanelProps>) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [result, setResult] = useState(initialCrmMutationState);
  const [dueDateValue, setDueDateValue] = useState(toLocalDateTimeInput(dueAtUtc));
  const [reminderValue, setReminderValue] = useState(toLocalDateTimeInput(reminderAtUtc));
  const [completionNote, setCompletionNote] = useState("");
  const isCompleted = status.toLowerCase() === "completed";

  return (
    <section className="space-y-4 rounded-lg border border-border/60 p-4">
      <h2 className="text-sm font-semibold">Task actions</h2>
      <CrmFormFeedback state={result} />

      {canManage ? (
        <div className="flex flex-wrap gap-2">
          <Button
            type="button"
            size="sm"
            variant="outline"
            className="h-8"
            disabled={isPending}
            onClick={() =>
              startTransition(async () => {
                const next = isCompleted
                  ? await reopenTaskAction(taskId)
                  : await completeTaskAction(taskId, completionNote);
                setResult(next);
                if (next.status === "success") {
                  router.refresh();
                }
              })
            }
          >
            {isCompleted ? "Reopen task" : "Complete task"}
          </Button>
          {canDelete ? (
            <Button
              type="button"
              size="sm"
              variant="destructive"
              className="h-8"
              disabled={isPending}
              onClick={() => {
                const confirmed = window.confirm("Archive this task?");
                if (!confirmed) {
                  return;
                }

                startTransition(async () => {
                  const next = await deleteTaskAction(taskId);
                  setResult(next);
                  if (next.status === "success") {
                    router.push("/tasks");
                    router.refresh();
                  }
                });
              }}
            >
              Delete
            </Button>
          ) : null}
        </div>
      ) : null}

      {!isCompleted && canManage ? (
        <Field>
          <FieldLabel htmlFor="task-completion-note">Completion note</FieldLabel>
          <FieldContent>
            <Textarea
              id="task-completion-note"
              rows={2}
              value={completionNote}
              onChange={(event) => setCompletionNote(event.target.value)}
            />
          </FieldContent>
        </Field>
      ) : null}

      {canEdit ? (
        <div className="grid gap-3 sm:grid-cols-2">
          <Field>
            <FieldLabel htmlFor="task-due-date">Due date</FieldLabel>
            <FieldContent>
              <Input
                id="task-due-date"
                type="datetime-local"
                value={dueDateValue}
                onChange={(event) => setDueDateValue(event.target.value)}
              />
              <FieldError />
            </FieldContent>
          </Field>

          <Field>
            <FieldLabel htmlFor="task-reminder">Reminder</FieldLabel>
            <FieldContent>
              <Input
                id="task-reminder"
                type="datetime-local"
                value={reminderValue}
                onChange={(event) => setReminderValue(event.target.value)}
              />
              <FieldError />
            </FieldContent>
          </Field>

          <div className="sm:col-span-2 flex flex-wrap gap-2">
            <Button
              type="button"
              size="sm"
              className="h-8"
              disabled={isPending}
              onClick={() =>
                startTransition(async () => {
                  const next = await updateTaskDueDateAction(taskId, dueDateValue);
                  setResult(next);
                  if (next.status === "success") {
                    router.refresh();
                  }
                })
              }
            >
              Update due date
            </Button>
            <Button
              type="button"
              size="sm"
              variant="outline"
              className="h-8"
              disabled={isPending}
              onClick={() =>
                startTransition(async () => {
                  const next = await updateTaskReminderAction(taskId, reminderValue || null);
                  setResult(next);
                  if (next.status === "success") {
                    router.refresh();
                  }
                })
              }
            >
              Save reminder
            </Button>
            <Button
              type="button"
              size="sm"
              variant="ghost"
              className="h-8"
              disabled={isPending}
              onClick={() =>
                startTransition(async () => {
                  setReminderValue("");
                  const next = await updateTaskReminderAction(taskId, null);
                  setResult(next);
                  if (next.status === "success") {
                    router.refresh();
                  }
                })
              }
            >
              Clear reminder
            </Button>
          </div>
        </div>
      ) : null}

      <p className="text-xs text-muted-foreground">
        Owner assignment is backend-ready and intentionally deferred until user picker is available.
      </p>
    </section>
  );
}

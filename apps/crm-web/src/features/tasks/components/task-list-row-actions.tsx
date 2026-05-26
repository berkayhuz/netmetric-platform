"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";
import { Button } from "@netmetric/ui";

import { CrmMutationResult } from "@/components/forms/crm-mutation-result";
import { initialCrmMutationState } from "@/features/shared/actions/mutation-state";

import {
  completeTaskAction,
  deleteTaskAction,
  reopenTaskAction,
} from "../actions/work-management-task-lifecycle-actions";

type TaskListRowActionsProps = {
  taskId: string;
  taskStatus: string;
  canEdit: boolean;
  canManage: boolean;
  canDelete: boolean;
};

export function TaskListRowActions({
  taskId,
  taskStatus,
  canEdit,
  canManage,
  canDelete,
}: Readonly<TaskListRowActionsProps>) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [result, setResult] = useState(initialCrmMutationState);
  const isCompleted = taskStatus.toLowerCase() === "completed";

  return (
    <div className="flex items-center gap-1">
      <CrmMutationResult state={result} />
      <Button asChild size="sm" variant="ghost" className="h-8 px-2 text-xs">
        <Link href={`/tasks/${taskId}`}>View</Link>
      </Button>
      {canEdit ? (
        <Button asChild size="sm" variant="ghost" className="h-8 px-2 text-xs">
          <Link href={`/tasks/${taskId}/edit`}>Edit</Link>
        </Button>
      ) : null}
      {canManage ? (
        <Button
          type="button"
          size="sm"
          variant="outline"
          className="h-8 px-2 text-xs"
          disabled={isPending}
          onClick={() =>
            startTransition(async () => {
              const next = isCompleted
                ? await reopenTaskAction(taskId)
                : await completeTaskAction(taskId);
              setResult(next);
              if (next.status === "success") {
                router.refresh();
              }
            })
          }
        >
          {isCompleted ? "Reopen" : "Complete"}
        </Button>
      ) : null}
      {canDelete ? (
        <Button
          type="button"
          size="sm"
          variant="destructive"
          className="h-8 px-2 text-xs"
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
                router.refresh();
              }
            });
          }}
        >
          Delete
        </Button>
      ) : null}
    </div>
  );
}

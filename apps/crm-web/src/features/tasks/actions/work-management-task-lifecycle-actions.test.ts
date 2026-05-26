import { beforeEach, describe, expect, it, vi } from "vitest";

const mocks = vi.hoisted(() => ({
  updateWorkTask: vi.fn(),
  completeWorkTask: vi.fn(),
  reopenWorkTask: vi.fn(),
  updateWorkTaskDueDate: vi.fn(),
  updateWorkTaskReminder: vi.fn(),
  deleteWorkTask: vi.fn(),
  requireCrmActionCapability: vi.fn(async () => ({})),
}));

vi.mock("next/cache", () => ({ revalidatePath: vi.fn() }));
vi.mock("@/lib/security/csrf", () => ({ assertSameOriginRequest: vi.fn(async () => {}) }));
vi.mock("@/lib/i18n/crm-i18n", () => ({ tCrm: vi.fn((key: string) => key) }));
vi.mock("@/lib/crm-auth/crm-api-request-options", () => ({
  getCrmApiRequestOptions: vi.fn(async () => ({})),
}));
vi.mock("@/lib/crm-auth/require-crm-action-capability", () => ({
  requireCrmActionCapability: mocks.requireCrmActionCapability,
}));
vi.mock("@/lib/crm-api", () => ({
  crmApiClient: {
    updateWorkTask: mocks.updateWorkTask,
    completeWorkTask: mocks.completeWorkTask,
    reopenWorkTask: mocks.reopenWorkTask,
    updateWorkTaskDueDate: mocks.updateWorkTaskDueDate,
    updateWorkTaskReminder: mocks.updateWorkTaskReminder,
    deleteWorkTask: mocks.deleteWorkTask,
  },
}));

import {
  completeTaskAction,
  deleteTaskAction,
  reopenTaskAction,
  updateTaskAction,
  updateTaskDueDateAction,
  updateTaskReminderAction,
} from "./work-management-task-lifecycle-actions";

describe("work management task lifecycle actions", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("updates task payload using edit endpoint", async () => {
    mocks.updateWorkTask.mockResolvedValueOnce({});

    await updateTaskAction("task-1", {
      title: "Task",
      description: "Desc",
      ownerUserId: "",
      dueAtUtc: "2026-05-24T10:00",
      priority: 3,
    });

    expect(mocks.updateWorkTask).toHaveBeenCalledWith(
      "task-1",
      {
        title: "Task",
        description: "Desc",
        priority: 3,
      },
      {},
    );
  });

  it("completes and reopens tasks", async () => {
    mocks.completeWorkTask.mockResolvedValueOnce({});
    mocks.reopenWorkTask.mockResolvedValueOnce({});

    await completeTaskAction("task-1", "done");
    await reopenTaskAction("task-1");

    expect(mocks.completeWorkTask).toHaveBeenCalledWith("task-1", { completionNote: "done" }, {});
    expect(mocks.reopenWorkTask).toHaveBeenCalledWith("task-1", {});
  });

  it("updates due date and reminder payload as ISO", async () => {
    mocks.updateWorkTaskDueDate.mockResolvedValueOnce({});
    mocks.updateWorkTaskReminder.mockResolvedValueOnce({});

    await updateTaskDueDateAction("task-1", "2026-05-24T10:00");
    await updateTaskReminderAction("task-1", "2026-05-24T09:30");

    expect(mocks.updateWorkTaskDueDate).toHaveBeenCalledWith(
      "task-1",
      { dueAtUtc: expect.stringContaining("2026-05-24T") },
      {},
    );
    expect(mocks.updateWorkTaskReminder).toHaveBeenCalledWith(
      "task-1",
      { reminderAtUtc: expect.stringContaining("2026-05-24T") },
      {},
    );
  });

  it("deletes task with delete endpoint", async () => {
    mocks.deleteWorkTask.mockResolvedValueOnce(undefined);

    await deleteTaskAction("task-1");

    expect(mocks.deleteWorkTask).toHaveBeenCalledWith("task-1", {});
  });
});

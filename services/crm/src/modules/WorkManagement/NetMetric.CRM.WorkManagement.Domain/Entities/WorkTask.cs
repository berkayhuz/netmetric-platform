// <copyright file="WorkTask.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.WorkManagement.Domain.Enums;
using NetMetric.Entities;

namespace NetMetric.CRM.WorkManagement.Domain.Entities;

public sealed class WorkTask : AuditableEntity
{
    private WorkTask()
    {
    }

    public WorkTask(string title, string description, Guid? ownerUserId, DateTime dueAtUtc, int priority)
    {
        Title = string.IsNullOrWhiteSpace(title) ? throw new ArgumentException("Task title is required.", nameof(title)) : title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? string.Empty : description.Trim();
        OwnerUserId = ownerUserId;
        DueAtUtc = dueAtUtc;
        Priority = Math.Clamp(priority, 1, 5);
        Status = WorkItemStatus.Planned;
    }

    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public Guid? OwnerUserId { get; private set; }
    public DateTime DueAtUtc { get; private set; }
    public DateTime? ReminderAtUtc { get; private set; }
    public int Priority { get; private set; }
    public WorkItemStatus Status { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public Guid? CompletedByUserId { get; private set; }
    public string? CompletionNote { get; private set; }

    public void UpdateDetails(string title, string description, int priority)
    {
        EnsureMutable();

        Title = string.IsNullOrWhiteSpace(title) ? throw new ArgumentException("Task title is required.", nameof(title)) : title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? string.Empty : description.Trim();
        Priority = Math.Clamp(priority, 1, 5);
    }

    public void AssignOwner(Guid? ownerUserId)
    {
        EnsureMutable();
        OwnerUserId = ownerUserId;
    }

    public void SetDueDate(DateTime dueAtUtc)
    {
        EnsureMutable();
        DueAtUtc = dueAtUtc;

        if (ReminderAtUtc.HasValue && ReminderAtUtc.Value > DueAtUtc)
        {
            throw new ArgumentException("Task reminder cannot be later than due date.", nameof(dueAtUtc));
        }
    }

    public void SetReminder(DateTime? reminderAtUtc)
    {
        EnsureMutable();

        if (reminderAtUtc.HasValue && reminderAtUtc.Value > DueAtUtc)
        {
            throw new ArgumentException("Task reminder cannot be later than due date.", nameof(reminderAtUtc));
        }

        ReminderAtUtc = reminderAtUtc;
    }

    public void MarkCompleted(Guid? completedByUserId, string? completionNote = null)
    {
        EnsureMutable();
        if (Status == WorkItemStatus.Completed)
        {
            return;
        }

        Status = WorkItemStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
        CompletedByUserId = completedByUserId;
        CompletionNote = string.IsNullOrWhiteSpace(completionNote) ? null : completionNote.Trim();
    }

    public void Reopen()
    {
        if (Status != WorkItemStatus.Completed && Status != WorkItemStatus.Cancelled)
        {
            throw new InvalidOperationException("Only completed or cancelled tasks can be reopened.");
        }

        Status = WorkItemStatus.InProgress;
        CompletedAtUtc = null;
        CompletedByUserId = null;
        CompletionNote = null;
    }

    private void EnsureMutable()
    {
        if (Status == WorkItemStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled tasks cannot be modified.");
        }
    }
}

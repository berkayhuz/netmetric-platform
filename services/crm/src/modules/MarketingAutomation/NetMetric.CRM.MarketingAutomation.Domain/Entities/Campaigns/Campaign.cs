// <copyright file="Campaign.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.MarketingAutomation.Domain.Entities.Campaigns;

public class Campaign : AuditableEntity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string Channel { get; private set; } = CampaignChannels.Email;
    public string Status { get; private set; } = CampaignStatuses.Draft;
    public Guid? SegmentId { get; private set; }
    public Guid? EmailTemplateId { get; private set; }
    public decimal BudgetAmount { get; private set; }
    public decimal ExpectedRevenueAmount { get; private set; }
    public int FrequencyCapPerContact { get; private set; } = 3;
    public int FrequencyCapWindowDays { get; private set; } = 7;
    public DateTime? ScheduledAtUtc { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? PausedAtUtc { get; private set; }
    public DateTime? CanceledAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public string ApprovalStatus { get; private set; } = CampaignApprovalStatuses.NotRequired;
    public Guid? ApprovalWorkflowId { get; private set; }
    public string UtmCampaign { get; private set; } = string.Empty;

    private Campaign() { }

    public Campaign(string code, string name, string? description = null)
    {
        Code = Guard.AgainstNullOrWhiteSpace(code);
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        UtmCampaign = NormalizeCode(code);
    }

    public void Update(
        string name,
        string? description,
        Guid? segmentId = null,
        Guid? emailTemplateId = null,
        decimal budgetAmount = 0,
        decimal expectedRevenueAmount = 0,
        int frequencyCapPerContact = 3,
        int frequencyCapWindowDays = 7,
        string? utmCampaign = null)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SegmentId = segmentId;
        EmailTemplateId = emailTemplateId;
        BudgetAmount = Math.Max(0, budgetAmount);
        ExpectedRevenueAmount = Math.Max(0, expectedRevenueAmount);
        FrequencyCapPerContact = Math.Clamp(frequencyCapPerContact, 1, 100);
        FrequencyCapWindowDays = Math.Clamp(frequencyCapWindowDays, 1, 365);
        UtmCampaign = string.IsNullOrWhiteSpace(utmCampaign) ? NormalizeCode(Code) : NormalizeCode(utmCampaign);
    }

    public void RequireApproval(Guid workflowId)
    {
        ApprovalWorkflowId = Guard.AgainstEmpty(workflowId);
        ApprovalStatus = CampaignApprovalStatuses.Pending;
    }

    public void Approve()
    {
        ApprovalStatus = CampaignApprovalStatuses.Approved;
    }

    public void Schedule(DateTime scheduledAtUtc)
    {
        if (ApprovalStatus == CampaignApprovalStatuses.Pending)
        {
            throw new InvalidOperationException("Campaign approval is pending.");
        }

        ScheduledAtUtc = scheduledAtUtc;
        Status = CampaignStatuses.Scheduled;
        SetActive(true);
    }

    public void MarkRunning(DateTime startedAtUtc)
    {
        StartedAtUtc ??= startedAtUtc;
        Status = CampaignStatuses.Running;
        PausedAtUtc = null;
    }

    public void Pause(DateTime pausedAtUtc)
    {
        if (Status is not CampaignStatuses.Running and not CampaignStatuses.Scheduled)
        {
            throw new InvalidOperationException("Only scheduled or running campaigns can be paused.");
        }

        Status = CampaignStatuses.Paused;
        PausedAtUtc = pausedAtUtc;
    }

    public void Resume(DateTime scheduledAtUtc)
    {
        if (Status != CampaignStatuses.Paused)
        {
            throw new InvalidOperationException("Only paused campaigns can be resumed.");
        }

        ScheduledAtUtc = scheduledAtUtc;
        Status = CampaignStatuses.Scheduled;
        PausedAtUtc = null;
    }

    public void Cancel(DateTime canceledAtUtc)
    {
        Status = CampaignStatuses.Canceled;
        CanceledAtUtc = canceledAtUtc;
        SetActive(false);
    }

    public void Complete(DateTime completedAtUtc)
    {
        Status = CampaignStatuses.Completed;
        CompletedAtUtc = completedAtUtc;
    }

    private static string NormalizeCode(string value)
        => Guard.AgainstNullOrWhiteSpace(value).Trim().ToLowerInvariant().Replace(' ', '-');
}

public static class CampaignChannels
{
    public const string Email = "email";
}

public static class CampaignStatuses
{
    public const string Draft = "draft";
    public const string Scheduled = "scheduled";
    public const string Running = "running";
    public const string Paused = "paused";
    public const string Completed = "completed";
    public const string Canceled = "canceled";
}

public static class CampaignApprovalStatuses
{
    public const string NotRequired = "not-required";
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
}

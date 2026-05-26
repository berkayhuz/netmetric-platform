// <copyright file="EmailCampaign.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.MarketingAutomation.Domain.Entities.EmailCampaigns;

public class EmailCampaign : AuditableEntity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string FromName { get; private set; } = string.Empty;
    public string FromEmail { get; private set; } = string.Empty;
    public string HtmlBody { get; private set; } = string.Empty;
    public string TextBody { get; private set; } = string.Empty;
    public string DeliverabilityStatus { get; private set; } = EmailDeliverabilityStatuses.Unverified;
    public DateTime? LastPreviewedAtUtc { get; private set; }

    private EmailCampaign() { }

    public EmailCampaign(string code, string name, string? description = null, string subject = "", string fromName = "", string fromEmail = "", string htmlBody = "", string textBody = "")
    {
        Code = Guard.AgainstNullOrWhiteSpace(code);
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Subject = subject.Trim();
        FromName = fromName.Trim();
        FromEmail = fromEmail.Trim().ToLowerInvariant();
        HtmlBody = htmlBody.Trim();
        TextBody = textBody.Trim();
    }

    public void Update(string name, string? description, string subject = "", string fromName = "", string fromEmail = "", string htmlBody = "", string textBody = "")
    {
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Subject = subject.Trim();
        FromName = fromName.Trim();
        FromEmail = fromEmail.Trim().ToLowerInvariant();
        HtmlBody = htmlBody.Trim();
        TextBody = textBody.Trim();
    }

    public void MarkDeliverability(string status)
    {
        DeliverabilityStatus = Guard.AgainstNullOrWhiteSpace(status);
    }

    public void MarkPreviewed(DateTime previewedAtUtc)
    {
        LastPreviewedAtUtc = previewedAtUtc;
    }
}

public static class EmailDeliverabilityStatuses
{
    public const string Unverified = "unverified";
    public const string Verified = "verified";
    public const string Failed = "failed";
}

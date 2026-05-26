// <copyright file="MeetingSchedule.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.WorkManagement.Domain.Entities;

public sealed class MeetingSchedule : AuditableEntity
{
    private MeetingSchedule()
    {
    }

    public MeetingSchedule(string title, DateTime startsAtUtc, DateTime endsAtUtc, string organizerEmail, string attendeeSummary, bool requiresExternalSync)
    {
        Title = string.IsNullOrWhiteSpace(title) ? throw new ArgumentException("Meeting title is required.", nameof(title)) : title.Trim();
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc <= startsAtUtc ? throw new ArgumentException("Meeting end time must be after start time.", nameof(endsAtUtc)) : endsAtUtc;
        OrganizerEmail = string.IsNullOrWhiteSpace(organizerEmail) ? throw new ArgumentException("Organizer email is required.", nameof(organizerEmail)) : organizerEmail.Trim();
        AttendeeSummary = string.IsNullOrWhiteSpace(attendeeSummary) ? string.Empty : attendeeSummary.Trim();
        RequiresExternalSync = requiresExternalSync;
    }

    public string Title { get; private set; } = null!;
    public DateTime StartsAtUtc { get; private set; }
    public DateTime EndsAtUtc { get; private set; }
    public string OrganizerEmail { get; private set; } = null!;
    public string AttendeeSummary { get; private set; } = string.Empty;
    public bool RequiresExternalSync { get; private set; }
}

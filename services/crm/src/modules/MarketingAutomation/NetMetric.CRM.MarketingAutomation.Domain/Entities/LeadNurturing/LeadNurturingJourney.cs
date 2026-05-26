// <copyright file="LeadNurturingJourney.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.MarketingAutomation.Domain.Entities.LeadNurturing;

public class LeadNurturingJourney : AuditableEntity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string Status { get; private set; } = JourneyStatuses.Draft;
    public Guid? EntrySegmentId { get; private set; }
    public string StepDefinitionJson { get; private set; } = "[]";
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? PausedAtUtc { get; private set; }

    private LeadNurturingJourney() { }

    public LeadNurturingJourney(string code, string name, string? description = null, Guid? entrySegmentId = null, string stepDefinitionJson = "[]")
    {
        Code = Guard.AgainstNullOrWhiteSpace(code);
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        EntrySegmentId = entrySegmentId;
        StepDefinitionJson = string.IsNullOrWhiteSpace(stepDefinitionJson) ? "[]" : stepDefinitionJson.Trim();
    }

    public void Update(string name, string? description, Guid? entrySegmentId, string stepDefinitionJson)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        EntrySegmentId = entrySegmentId;
        StepDefinitionJson = string.IsNullOrWhiteSpace(stepDefinitionJson) ? "[]" : stepDefinitionJson.Trim();
    }

    public void Start(DateTime startedAtUtc)
    {
        Status = JourneyStatuses.Running;
        StartedAtUtc ??= startedAtUtc;
        PausedAtUtc = null;
    }

    public void Pause(DateTime pausedAtUtc)
    {
        Status = JourneyStatuses.Paused;
        PausedAtUtc = pausedAtUtc;
    }
}

public static class JourneyStatuses
{
    public const string Draft = "draft";
    public const string Running = "running";
    public const string Paused = "paused";
    public const string Completed = "completed";
}

// <copyright file="Segment.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.MarketingAutomation.Domain.Entities.Segments;

public class Segment : AuditableEntity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string SegmentType { get; private set; } = SegmentTypes.Dynamic;
    public string CriteriaJson { get; private set; } = "{}";
    public bool IsSuppression { get; private set; }
    public int LastEvaluatedCount { get; private set; }
    public DateTime? LastEvaluatedAtUtc { get; private set; }

    private Segment() { }

    public Segment(string code, string name, string? description = null, string segmentType = SegmentTypes.Dynamic, string criteriaJson = "{}", bool isSuppression = false)
    {
        Code = Guard.AgainstNullOrWhiteSpace(code);
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SegmentType = NormalizeType(segmentType);
        CriteriaJson = NormalizeJson(criteriaJson);
        IsSuppression = isSuppression;
    }

    public void Update(string name, string? description, string segmentType, string criteriaJson, bool isSuppression)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SegmentType = NormalizeType(segmentType);
        CriteriaJson = NormalizeJson(criteriaJson);
        IsSuppression = isSuppression;
    }

    public void MarkEvaluated(int count, DateTime evaluatedAtUtc)
    {
        LastEvaluatedCount = Math.Max(0, count);
        LastEvaluatedAtUtc = evaluatedAtUtc;
    }

    private static string NormalizeType(string value)
        => value.Trim().ToLowerInvariant() switch
        {
            SegmentTypes.Static => SegmentTypes.Static,
            SegmentTypes.Suppression => SegmentTypes.Suppression,
            _ => SegmentTypes.Dynamic
        };

    private static string NormalizeJson(string? json) => string.IsNullOrWhiteSpace(json) ? "{}" : json.Trim();
}

public static class SegmentTypes
{
    public const string Dynamic = "dynamic";
    public const string Static = "static";
    public const string Suppression = "suppression";
}

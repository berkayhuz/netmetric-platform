// <copyright file="MarketingSegmentEvaluator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using NetMetric.CRM.MarketingAutomation.Application.Abstractions;
using NetMetric.CRM.MarketingAutomation.Contracts.DTOs;

namespace NetMetric.CRM.MarketingAutomation.Infrastructure.Processing;

public sealed class MarketingSegmentEvaluator : IMarketingSegmentEvaluator
{
    public MarketingSegmentEvaluationDto Evaluate(Guid segmentId, string criteriaJson, IReadOnlyCollection<MarketingAudienceMemberInput> audience)
    {
        var matched = audience
            .Where(member => Matches(criteriaJson, member.PayloadJson))
            .Select(member => new MarketingAudienceMemberDto(MarketingUtilities.HashEmail(member.EmailAddress), member.EmailAddress, member.ContactId, "criteria-match"))
            .ToList();

        return new MarketingSegmentEvaluationDto(segmentId, matched.Count, matched);
    }

    private static bool Matches(string criteriaJson, string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(criteriaJson) || criteriaJson.Trim() == "{}")
        {
            return true;
        }

        using var document = JsonDocument.Parse(criteriaJson);
        return MatchNode(document.RootElement, payloadJson);
    }

    private static bool MatchNode(JsonElement node, string payloadJson)
    {
        if (node.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (node.TryGetProperty("operator", out var logical) && node.TryGetProperty("conditions", out var conditions))
        {
            var children = conditions.ValueKind == JsonValueKind.Array ? conditions.EnumerateArray().Select(x => MatchNode(x, payloadJson)).ToList() : [];
            return (logical.GetString() ?? "and").ToLowerInvariant() switch
            {
                "or" => children.Any(x => x),
                "not" => children.Count == 1 && !children[0],
                _ => children.Count > 0 && children.All(x => x)
            };
        }

        var field = node.TryGetProperty("field", out var fieldElement) ? fieldElement.GetString() : null;
        if (string.IsNullOrWhiteSpace(field))
        {
            return false;
        }

        var op = node.TryGetProperty("comparison", out var comparison) ? comparison.GetString() ?? "eq" : "eq";
        var expected = node.TryGetProperty("value", out var value) ? ReadValue(value) : null;
        var actual = MarketingUtilities.ReadString(payloadJson, field);

        return op.ToLowerInvariant() switch
        {
            "exists" => actual is not null,
            "notexists" => actual is null,
            "contains" => actual?.Contains(expected ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true,
            "neq" or "notequals" => !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            "in" => value.ValueKind == JsonValueKind.Array && value.EnumerateArray().Any(x => string.Equals(ReadValue(x), actual, StringComparison.OrdinalIgnoreCase)),
            _ => string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase)
        };
    }

    private static string? ReadValue(JsonElement element)
        => element.ValueKind == JsonValueKind.String ? element.GetString() : element.GetRawText();
}

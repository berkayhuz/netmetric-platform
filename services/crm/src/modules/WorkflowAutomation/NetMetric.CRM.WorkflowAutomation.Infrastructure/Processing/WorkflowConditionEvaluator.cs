// <copyright file="WorkflowConditionEvaluator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Globalization;
using System.Text.Json;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Rules;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

public sealed class WorkflowConditionEvaluator : IWorkflowConditionEvaluator
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public WorkflowConditionEvaluationResult Evaluate(string conditionDefinitionJson, string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(conditionDefinitionJson) || conditionDefinitionJson.Trim() == "{}")
        {
            return CreateResult(true, []);
        }

        using var document = JsonDocument.Parse(conditionDefinitionJson);
        var failures = new List<string>();
        var matched = EvaluateNode(document.RootElement, payloadJson, failures);
        return CreateResult(matched, failures);
    }

    private static bool EvaluateNode(JsonElement node, string payloadJson, List<string> failures)
    {
        if (node.ValueKind != JsonValueKind.Object)
        {
            failures.Add("Condition node must be an object.");
            return false;
        }

        if (node.TryGetProperty("operator", out var logicalOperator) && node.TryGetProperty("conditions", out var conditions))
        {
            var op = JsonWorkflowValueReader.AsString(logicalOperator)?.ToLowerInvariant() ?? "and";
            var childEvaluations = conditions.ValueKind == JsonValueKind.Array
                ? conditions.EnumerateArray()
                    .Select(condition =>
                    {
                        var childFailures = new List<string>();
                        var matched = EvaluateNode(condition, payloadJson, childFailures);
                        return new ChildEvaluation(matched, childFailures);
                    })
                    .ToList()
                : [];

            var matched = op switch
            {
                "or" => childEvaluations.Any(x => x.Matched),
                "not" => childEvaluations.Count == 1 && !childEvaluations[0].Matched,
                _ => childEvaluations.Count > 0 && childEvaluations.All(x => x.Matched)
            };

            if (!matched)
            {
                var relevantFailures = op == "and"
                    ? childEvaluations.Where(x => !x.Matched).SelectMany(x => x.Failures)
                    : childEvaluations.SelectMany(x => x.Failures);
                failures.AddRange(relevantFailures);
            }

            return matched;
        }

        var field = node.TryGetProperty("field", out var fieldElement) ? JsonWorkflowValueReader.AsString(fieldElement) : null;
        var comparisonOperator = node.TryGetProperty("comparison", out var comparisonElement)
            ? JsonWorkflowValueReader.AsString(comparisonElement)
            : node.TryGetProperty("op", out var opElement)
                ? JsonWorkflowValueReader.AsString(opElement)
                : node.TryGetProperty("operator", out var operatorElement)
                    ? JsonWorkflowValueReader.AsString(operatorElement)
                    : "eq";

        if (string.IsNullOrWhiteSpace(field))
        {
            failures.Add("Condition field is required.");
            return false;
        }

        var actual = JsonWorkflowValueReader.TryRead(payloadJson, field);
        var result = Compare(actual, comparisonOperator ?? "eq", node.TryGetProperty("value", out var expected) ? expected : default);
        if (!result)
        {
            failures.Add($"{field} did not satisfy {comparisonOperator ?? "eq"}.");
        }

        return result;
    }

    private static bool Compare(JsonElement? actual, string op, JsonElement expected)
    {
        var normalizedOp = op.ToLowerInvariant();
        if (normalizedOp == "exists")
        {
            return actual.HasValue;
        }

        if (!actual.HasValue)
        {
            return normalizedOp == "notexists";
        }

        var actualString = JsonWorkflowValueReader.AsString(actual.Value);
        var expectedString = expected.ValueKind == JsonValueKind.Undefined ? null : JsonWorkflowValueReader.AsString(expected);

        return normalizedOp switch
        {
            "eq" or "equals" => string.Equals(actualString, expectedString, StringComparison.OrdinalIgnoreCase),
            "neq" or "notequals" => !string.Equals(actualString, expectedString, StringComparison.OrdinalIgnoreCase),
            "contains" => actualString?.Contains(expectedString ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true,
            "startsWith" or "startswith" => actualString?.StartsWith(expectedString ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true,
            "endsWith" or "endswith" => actualString?.EndsWith(expectedString ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true,
            "in" => expected.ValueKind == JsonValueKind.Array && expected.EnumerateArray().Any(value => string.Equals(JsonWorkflowValueReader.AsString(value), actualString, StringComparison.OrdinalIgnoreCase)),
            "gt" => CompareDecimal(actual.Value, expected, static comparison => comparison > 0),
            "gte" => CompareDecimal(actual.Value, expected, static comparison => comparison >= 0),
            "lt" => CompareDecimal(actual.Value, expected, static comparison => comparison < 0),
            "lte" => CompareDecimal(actual.Value, expected, static comparison => comparison <= 0),
            "after" => CompareDate(actual.Value, expected, static comparison => comparison > 0),
            "before" => CompareDate(actual.Value, expected, static comparison => comparison < 0),
            _ => false
        };
    }

    private static bool CompareDecimal(JsonElement actual, JsonElement expected, Func<int, bool> predicate)
    {
        var actualNumber = JsonWorkflowValueReader.AsDecimal(actual);
        var expectedNumber = expected.ValueKind == JsonValueKind.Number
            ? JsonWorkflowValueReader.AsDecimal(expected)
            : decimal.TryParse(JsonWorkflowValueReader.AsString(expected), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;

        return actualNumber.HasValue && expectedNumber.HasValue && predicate(actualNumber.Value.CompareTo(expectedNumber.Value));
    }

    private static bool CompareDate(JsonElement actual, JsonElement expected, Func<int, bool> predicate)
    {
        var actualDate = JsonWorkflowValueReader.AsDateTime(actual);
        var expectedDate = JsonWorkflowValueReader.AsDateTime(expected);
        return actualDate.HasValue && expectedDate.HasValue && predicate(actualDate.Value.CompareTo(expectedDate.Value));
    }

    private static WorkflowConditionEvaluationResult CreateResult(bool matched, IReadOnlyCollection<string> failures)
    {
        var json = JsonSerializer.Serialize(new { matched, failures }, SerializerOptions);
        return new WorkflowConditionEvaluationResult(matched, json, failures);
    }

    private sealed record ChildEvaluation(bool Matched, IReadOnlyCollection<string> Failures);
}

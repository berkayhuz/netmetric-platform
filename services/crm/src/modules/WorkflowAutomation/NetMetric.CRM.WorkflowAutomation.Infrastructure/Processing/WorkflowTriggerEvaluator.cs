// <copyright file="WorkflowTriggerEvaluator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Rules;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.AutomationRules;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

public sealed class WorkflowTriggerEvaluator : IWorkflowTriggerEvaluator
{
    public bool IsMatch(AutomationRule rule, WorkflowRuleExecutionRequest request)
    {
        if (!rule.IsActive)
        {
            return false;
        }

        if (request.RuleId.HasValue && rule.Id != request.RuleId.Value)
        {
            return false;
        }

        if (!string.Equals(rule.TriggerType, request.TriggerType, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.Equals(rule.EntityType, request.EntityType, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        using var definition = JsonDocument.Parse(string.IsNullOrWhiteSpace(rule.TriggerDefinitionJson) ? "{}" : rule.TriggerDefinitionJson);
        var root = definition.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
        {
            return true;
        }

        if (root.TryGetProperty("entityTypes", out var entityTypes) && entityTypes.ValueKind == JsonValueKind.Array)
        {
            var hasEntityType = entityTypes.EnumerateArray()
                .Select(JsonWorkflowValueReader.AsString)
                .Any(value => string.Equals(value, request.EntityType, StringComparison.OrdinalIgnoreCase));
            if (!hasEntityType)
            {
                return false;
            }
        }

        if (root.TryGetProperty("event", out var eventName))
        {
            var configuredEvent = JsonWorkflowValueReader.AsString(eventName);
            if (!string.IsNullOrWhiteSpace(configuredEvent) &&
                !string.Equals(configuredEvent, request.TriggerType, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        if (root.TryGetProperty("payloadField", out var payloadField))
        {
            var field = JsonWorkflowValueReader.AsString(payloadField);
            if (!string.IsNullOrWhiteSpace(field) && JsonWorkflowValueReader.TryRead(request.PayloadJson, field) is null)
            {
                return false;
            }
        }

        return true;
    }
}

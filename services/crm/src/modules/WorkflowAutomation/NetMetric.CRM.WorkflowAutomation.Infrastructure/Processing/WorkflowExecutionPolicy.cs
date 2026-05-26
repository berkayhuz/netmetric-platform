// <copyright file="WorkflowExecutionPolicy.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.AutomationRules;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

public static class WorkflowExecutionPolicy
{
    public static string ComputeIdempotencyKey(Guid tenantId, AutomationRule rule, string triggerType, string entityType, Guid? entityId, string payloadJson, string? suppliedKey)
    {
        if (!string.IsNullOrWhiteSpace(suppliedKey))
        {
            return $"{tenantId:N}:{rule.Id:N}:{rule.Version}:{suppliedKey.Trim()}";
        }

        var hashInput = $"{tenantId:N}|{rule.Id:N}|{rule.Version}|{triggerType}|{entityType}|{entityId:N}|{NormalizeJson(payloadJson)}";
        return $"{tenantId:N}:{rule.Id:N}:{rule.Version}:{Hash(hashInput)}";
    }

    public static string ComputeLoopFingerprint(Guid tenantId, AutomationRule rule, string triggerType, string entityType, Guid? entityId)
        => $"{tenantId:N}:{rule.Id:N}:{triggerType.Trim().ToLowerInvariant()}:{entityType.Trim().ToLowerInvariant()}:{entityId:N}";

    public static TimeSpan ComputeRetryDelay(int attemptNumber, WorkflowAutomationOptions options)
    {
        var exponent = Math.Max(0, attemptNumber - 1);
        var seconds = options.BaseRetryDelay.TotalSeconds * Math.Pow(2, exponent);
        return TimeSpan.FromSeconds(Math.Min(seconds, options.MaxRetryDelay.TotalSeconds));
    }

    private static string NormalizeJson(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
            return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (JsonException)
        {
            return json.Trim();
        }
    }

    private static string Hash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

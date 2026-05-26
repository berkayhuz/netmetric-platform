// <copyright file="WorkflowActionPermissionGuard.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Rules;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

public sealed class WorkflowActionPermissionGuard(ICurrentUserService currentUserService) : IWorkflowActionPermissionGuard
{
    public Task AuthorizeAsync(WorkflowActionPermissionContext context, CancellationToken cancellationToken)
    {
        var requiredPermission = string.IsNullOrWhiteSpace(context.RequiredPermission)
            ? ResolveDefaultPermission(context.ActionType)
            : context.RequiredPermission.Trim();

        if (currentUserService.IsAuthenticated && currentUserService.HasPermission(requiredPermission))
        {
            return Task.CompletedTask;
        }

        if (PermissionSnapshotContains(context.PermissionSnapshotJson, requiredPermission))
        {
            return Task.CompletedTask;
        }

        throw new UnauthorizedAccessException($"Workflow action '{context.ActionType}' requires permission '{requiredPermission}'.");
    }

    private static string ResolveDefaultPermission(string actionType)
        => actionType.Trim().ToLowerInvariant() switch
        {
            "approval.start" => "workflow.approvals.manage",
            "assignment.route" => "workflow.assignment-rules.manage",
            "webhook.post" => "workflow.webhooks.manage",
            _ => "workflow.rules.manage"
        };

    private static bool PermissionSnapshotContains(string permissionSnapshotJson, string requiredPermission)
    {
        if (string.IsNullOrWhiteSpace(permissionSnapshotJson))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(permissionSnapshotJson);
            return document.RootElement.ValueKind == JsonValueKind.Array &&
                   document.RootElement.EnumerateArray()
                       .Select(JsonWorkflowValueReader.AsString)
                       .Any(value => string.Equals(value, requiredPermission, StringComparison.OrdinalIgnoreCase));
        }
        catch (JsonException)
        {
            return false;
        }
    }
}

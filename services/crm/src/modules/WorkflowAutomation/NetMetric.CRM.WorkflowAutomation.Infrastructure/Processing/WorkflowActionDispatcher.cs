// <copyright file="WorkflowActionDispatcher.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Rules;
using NetMetric.CRM.WorkflowAutomation.Contracts.DTOs;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.ApprovalWorkflows;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.AssignmentRules;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.AutomationReminders;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.RuleExecutionLogs;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.WebhookSubscriptions;
using NetMetric.CRM.WorkflowAutomation.Infrastructure.Persistence;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

public sealed class WorkflowActionDispatcher(
    WorkflowAutomationDbContext dbContext,
    IWorkflowActionPermissionGuard permissionGuard,
    IWorkflowPayloadRedactor payloadRedactor,
    HttpClient httpClient,
    WebhookOutboundRequestValidator webhookRequestValidator,
    ILogger<WorkflowActionDispatcher> logger,
    IOptions<WorkflowAutomationOptions> options) : IWorkflowActionDispatcher
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly WorkflowAutomationOptions _options = options.Value;

    public async Task<WorkflowActionDispatchResult> DispatchAsync(WorkflowActionDispatchContext context, CancellationToken cancellationToken)
    {
        var actions = ParseActions(context.ActionDefinitionJson);
        var results = new List<WorkflowActionSimulationDto>();
        var executed = 0;

        foreach (var action in actions)
        {
            var actionType = ReadString(action, "type") ?? throw new WorkflowPermanentException("Workflow action type is required.", "action_type_required");
            var requiredPermission = ReadString(action, "requiredPermission");
            await permissionGuard.AuthorizeAsync(new WorkflowActionPermissionContext(actionType, requiredPermission, context.PermissionSnapshotJson, context.DryRun), cancellationToken);

            if (context.DryRun)
            {
                results.Add(new WorkflowActionSimulationDto(actionType, "planned", "Action passed validation and permission checks.", true, requiredPermission));
                continue;
            }

            switch (actionType.Trim().ToLowerInvariant())
            {
                case "audit.log":
                    results.Add(new WorkflowActionSimulationDto(actionType, "completed", ReadString(action, "message") ?? "Audit action recorded in execution log.", true, requiredPermission));
                    executed += 1;
                    break;
                case "reminder.create":
                    await CreateReminderAsync(context, action, cancellationToken);
                    results.Add(new WorkflowActionSimulationDto(actionType, "completed", "Reminder was created.", true, requiredPermission));
                    executed += 1;
                    break;
                case "approval.start":
                    await StartApprovalAsync(context, action, cancellationToken);
                    results.Add(new WorkflowActionSimulationDto(actionType, "completed", "Approval workflow was started.", true, requiredPermission));
                    executed += 1;
                    break;
                case "assignment.route":
                    await CreateAssignmentRouteAsync(context, action, cancellationToken);
                    results.Add(new WorkflowActionSimulationDto(actionType, "completed", "Assignment route was recorded.", true, requiredPermission));
                    executed += 1;
                    break;
                case "webhook.post":
                    await PostWebhookAsync(context, action, cancellationToken);
                    results.Add(new WorkflowActionSimulationDto(actionType, "completed", "Webhook delivery was attempted and logged.", true, requiredPermission));
                    executed += 1;
                    break;
                case "delay.enqueue":
                    await EnqueueDelayedExecutionAsync(context, action, cancellationToken);
                    results.Add(new WorkflowActionSimulationDto(actionType, "completed", "Delayed workflow execution was queued.", true, requiredPermission));
                    executed += 1;
                    break;
                default:
                    throw new WorkflowPermanentException($"Unsupported workflow action type '{actionType}'.", "unsupported_action_type");
            }
        }

        return new WorkflowActionDispatchResult(executed, JsonSerializer.Serialize(results, SerializerOptions), results);
    }

    private async Task CreateReminderAsync(WorkflowActionDispatchContext context, JsonElement action, CancellationToken cancellationToken)
    {
        var dueInMinutes = ReadInt(action, "dueInMinutes") ?? 60;
        var reminder = AutomationReminder.Create(
            ReadString(action, "name") ?? $"{context.Rule.Name} reminder",
            context.EntityType,
            DateTime.UtcNow.AddMinutes(Math.Max(1, dueInMinutes)),
            ReadString(action, "reminderType"),
            ReadRawJson(action, "recipientSelector"),
            payloadRedactor.RedactJson(context.PayloadJson),
            context.EntityId,
            context.Rule.Id,
            context.ExecutionLogId,
            ReadInt(action, "escalateInMinutes") is { } escalateInMinutes ? DateTime.UtcNow.AddMinutes(Math.Max(1, escalateInMinutes)) : null);

        await dbContext.AutomationReminders.AddAsync(reminder, cancellationToken);
    }

    private async Task StartApprovalAsync(WorkflowActionDispatchContext context, JsonElement action, CancellationToken cancellationToken)
    {
        var workflow = ApprovalWorkflow.Create(
            ReadString(action, "name") ?? $"{context.Rule.Name} approval",
            context.EntityType,
            context.EntityId,
            context.Rule.Id,
            ReadRawJson(action, "routingPolicy"),
            ReadRawJson(action, "escalationPolicy"),
            ReadRawJson(action, "slaPolicy"));

        var steps = action.TryGetProperty("steps", out var stepsElement) && stepsElement.ValueKind == JsonValueKind.Array
            ? stepsElement.EnumerateArray().ToList()
            : [];

        if (steps.Count == 0)
        {
            workflow.AddStep("Approval", 1, ReadRawJson(action, "approverSelector") ?? "{}");
        }
        else
        {
            var sequence = 1;
            foreach (var step in steps)
            {
                workflow.AddStep(
                    ReadString(step, "name") ?? $"Step {sequence}",
                    ReadInt(step, "sequence") ?? sequence,
                    ReadRawJson(step, "approverSelector") ?? "{}",
                    ReadBool(step, "isRequired") ?? true,
                    ReadInt(step, "dueInMinutes"),
                    ReadRawJson(step, "escalationTarget"));
                sequence += 1;
            }
        }

        workflow.Activate();
        await dbContext.ApprovalWorkflows.AddAsync(workflow, cancellationToken);
    }

    private async Task CreateAssignmentRouteAsync(WorkflowActionDispatchContext context, JsonElement action, CancellationToken cancellationToken)
    {
        var assignment = AssignmentRule.Create(
            ReadString(action, "name") ?? $"{context.Rule.Name} assignment route",
            context.EntityType,
            ReadRawJson(action, "condition") ?? "{}",
            ReadRawJson(action, "assigneeSelector"),
            context.EntityId,
            context.Rule.Id,
            ReadRawJson(action, "fallbackAssignee"),
            ReadInt(action, "priority") ?? context.Rule.Priority);

        await dbContext.AssignmentRules.AddAsync(assignment, cancellationToken);
    }

    private async Task PostWebhookAsync(WorkflowActionDispatchContext context, JsonElement action, CancellationToken cancellationToken)
    {
        var targetUrl = ReadString(action, "targetUrl") ?? throw new WorkflowPermanentException("webhook.post action requires targetUrl.", "webhook_target_required");
        var eventKey = ReadString(action, "eventKey") ?? $"{context.TriggerType}.{context.EntityType}";
        ValidatedWebhookTarget target;
        try
        {
            target = await webhookRequestValidator.ValidateAsync(targetUrl, cancellationToken);
        }
        catch (WorkflowPermanentException exception) when (exception.ErrorCode.StartsWith("webhook_target", StringComparison.Ordinal))
        {
            await RecordBlockedWebhookAsync(context, eventKey, targetUrl, exception, cancellationToken);
            throw;
        }

        var secret = ReadString(action, "secret") ?? ReadString(action, "secretKey") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new WorkflowPermanentException("webhook.post action requires a signing secret.", "webhook_secret_required");
        }

        var payload = new
        {
            eventKey,
            tenantId = context.TenantId,
            ruleId = context.Rule.Id,
            executionLogId = context.ExecutionLogId,
            entityType = context.EntityType,
            entityId = context.EntityId,
            payload = JsonSerializer.Deserialize<JsonElement>(string.IsNullOrWhiteSpace(context.PayloadJson) ? "{}" : context.PayloadJson)
        };
        var payloadJson = JsonSerializer.Serialize(payload, SerializerOptions);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var signature = SignPayload(secret, $"{timestamp}.{payloadJson}");
        var signatureHeader = $"t={timestamp},v1={signature}";

        var delivery = WebhookDeliveryLog.Create(
            context.TenantId,
            context.Rule.Id,
            context.ExecutionLogId,
            eventKey,
            target.SafeTarget,
            payloadRedactor.RedactJson(payloadJson),
            WebhookSignatureAuditMetadata.Create(signatureHeader),
            context.CorrelationId,
            maxAttempts: context.Rule.MaxAttempts);

        await dbContext.WebhookDeliveryLogs.AddAsync(delivery, cancellationToken);

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_options.WebhookRequestTimeout);
            using var request = new HttpRequestMessage(HttpMethod.Post, target.Uri)
            {
                Content = JsonContent.Create(payload)
            };
            WebhookHttpClientHandlerFactory.ApplyRequestConnectionPolicy(request, _options.StrictWebhookConnectionPinning);
            request.Options.Set(WebhookHttpClientHandlerFactory.ValidatedAddressesOption, target.ResolvedAddresses);
            request.Headers.TryAddWithoutValidation("X-NetMetric-Signature", signatureHeader);
            request.Headers.TryAddWithoutValidation("X-NetMetric-Event-Id", context.ExecutionLogId.ToString("N"));
            request.Headers.TryAddWithoutValidation("X-NetMetric-Correlation-Id", context.CorrelationId);

            logger.LogInformation(
                "Dispatching workflow webhook for tenant {TenantId}, rule {RuleId}, execution {ExecutionLogId} to host {WebhookHost}.",
                context.TenantId,
                context.Rule.Id,
                context.ExecutionLogId,
                target.Uri.Host);

            using var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                timeoutCts.Token);
            var responseBody = await WebhookResponsePreviewReader.ReadAsync(response.Content, _options.SafeWebhookMaxResponseBytes, timeoutCts.Token);
            delivery.MarkAttempt((int)response.StatusCode, payloadRedactor.RedactText(responseBody), DateTime.UtcNow);
            if (!response.IsSuccessStatusCode)
            {
                throw new WorkflowTransientException($"Webhook target returned HTTP {(int)response.StatusCode}.", "webhook_delivery_failed");
            }

            logger.LogInformation(
                "Workflow webhook completed for tenant {TenantId}, rule {RuleId}, execution {ExecutionLogId} with HTTP {StatusCode}.",
                context.TenantId,
                context.Rule.Id,
                context.ExecutionLogId,
                (int)response.StatusCode);
        }
        catch (HttpRequestException exception)
        {
            delivery.MarkFailure(payloadRedactor.RedactText(exception.Message), DateTime.UtcNow.Add(WorkflowExecutionPolicy.ComputeRetryDelay(delivery.AttemptNumber + 1, _options)));
            throw new WorkflowTransientException("Webhook delivery failed with a transient HTTP error.", "webhook_http_error");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            delivery.MarkFailure("Webhook delivery timed out.", DateTime.UtcNow.Add(WorkflowExecutionPolicy.ComputeRetryDelay(delivery.AttemptNumber + 1, _options)));
            throw new WorkflowTransientException("Webhook delivery timed out.", "webhook_timeout");
        }
    }

    private async Task RecordBlockedWebhookAsync(
        WorkflowActionDispatchContext context,
        string eventKey,
        string targetUrl,
        WorkflowPermanentException exception,
        CancellationToken cancellationToken)
    {
        var delivery = WebhookDeliveryLog.CreateBlockedAttempt(
            context.TenantId,
            context.Rule.Id,
            context.ExecutionLogId,
            eventKey,
            WebhookOutboundRequestValidator.CreateSafeAuditTarget(targetUrl),
            $"Blocked by outbound policy: {exception.ErrorCode}",
            context.CorrelationId,
            maxAttempts: context.Rule.MaxAttempts);

        await dbContext.WebhookDeliveryLogs.AddAsync(delivery, cancellationToken);
        logger.LogWarning(
            "Blocked workflow webhook for tenant {TenantId}, rule {RuleId}, execution {ExecutionLogId}: {Reason}.",
            context.TenantId,
            context.Rule.Id,
            context.ExecutionLogId,
            exception.ErrorCode);
    }

    private async Task EnqueueDelayedExecutionAsync(WorkflowActionDispatchContext context, JsonElement action, CancellationToken cancellationToken)
    {
        var delaySeconds = ReadInt(action, "delaySeconds") ?? 60;
        var scheduledAt = DateTime.UtcNow.AddSeconds(Math.Max(1, delaySeconds));
        var idempotencyKey = $"{context.TenantId:N}:{context.Rule.Id:N}:{context.Rule.Version}:delay:{context.ExecutionLogId:N}:{delaySeconds}";

        var exists = await dbContext.RuleExecutionLogs
            .IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == context.TenantId && x.IdempotencyKey == idempotencyKey, cancellationToken);
        if (exists)
        {
            return;
        }

        var delayed = RuleExecutionLog.Queue(
            context.TenantId,
            context.Rule.Id,
            context.Rule.Version,
            context.Rule.Name,
            context.TriggerType,
            context.EntityType,
            context.EntityId,
            idempotencyKey,
            $"{context.CorrelationId}:delayed",
            WorkflowExecutionPolicy.ComputeLoopFingerprint(context.TenantId, context.Rule, context.TriggerType, context.EntityType, context.EntityId),
            1,
            context.Rule.MaxAttempts,
            scheduledAt,
            payloadRedactor.RedactJson(context.PayloadJson),
            context.PermissionSnapshotJson,
            null);

        await dbContext.RuleExecutionLogs.AddAsync(delayed, cancellationToken);
    }

    private static IReadOnlyCollection<JsonElement> ParseActions(string actionDefinitionJson)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(actionDefinitionJson) ? "[]" : actionDefinitionJson);
        if (document.RootElement.ValueKind == JsonValueKind.Array)
        {
            return document.RootElement.EnumerateArray().Select(x => x.Clone()).ToList();
        }

        if (document.RootElement.ValueKind == JsonValueKind.Object)
        {
            return [document.RootElement.Clone()];
        }

        throw new WorkflowPermanentException("Action definition must be a JSON object or array.", "invalid_action_definition");
    }

    private static string SignPayload(string secret, string value)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    }

    private static string? ReadString(JsonElement element, string property)
        => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(property, out var value)
            ? JsonWorkflowValueReader.AsString(value)
            : null;

    private static int? ReadInt(JsonElement element, string property)
        => element.ValueKind == JsonValueKind.Object &&
           element.TryGetProperty(property, out var value) &&
           int.TryParse(JsonWorkflowValueReader.AsString(value), out var parsed)
            ? parsed
            : null;

    private static bool? ReadBool(JsonElement element, string property)
        => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(property, out var value)
            ? value.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String when bool.TryParse(value.GetString(), out var parsed) => parsed,
                _ => null
            }
            : null;

    private static string? ReadRawJson(JsonElement element, string property)
        => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(property, out var value)
            ? value.GetRawText()
            : null;

}

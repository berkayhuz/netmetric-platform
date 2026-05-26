// <copyright file="WorkflowErrorClassifier.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.RegularExpressions;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

public sealed record ClassifiedWorkflowError(
    bool IsRetryable,
    string Classification,
    string ErrorCode,
    string SanitizedMessage,
    TimeSpan? RetryAfter = null);

public sealed class WorkflowPermanentException(string message, string errorCode = "permanent_error") : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}

public sealed class WorkflowTransientException(string message, string errorCode = "transient_error", TimeSpan? retryAfter = null) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
    public TimeSpan? RetryAfter { get; } = retryAfter;
}

public static partial class WorkflowErrorClassifier
{
    public static ClassifiedWorkflowError Classify(Exception exception)
        => exception switch
        {
            WorkflowTransientException transient => new(true, "transient", transient.ErrorCode, Sanitize(transient.Message), transient.RetryAfter),
            WorkflowPermanentException permanent => new(false, "permanent", permanent.ErrorCode, Sanitize(permanent.Message)),
            UnauthorizedAccessException unauthorized => new(false, "authorization", "permission_denied", Sanitize(unauthorized.Message)),
            InvalidOperationException invalidOperation => new(false, "configuration", "invalid_workflow_configuration", Sanitize(invalidOperation.Message)),
            OperationCanceledException => new(true, "transient", "operation_canceled", "Workflow operation was canceled."),
            _ => new(true, "unexpected", "unexpected_error", Sanitize(exception.Message))
        };

    public static ClassifiedWorkflowError NonRetryable(string classification, string code, string message)
        => new(false, classification, code, Sanitize(message));

    public static string Sanitize(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "Workflow execution failed.";
        }

        var sanitized = SecretLikePattern().Replace(message, "$1=[redacted]");
        sanitized = BearerPattern().Replace(sanitized, "Bearer [redacted]");
        return sanitized.Length <= 1000 ? sanitized : sanitized[..1000];
    }

    [GeneratedRegex("(?i)(secret|token|password|api[_-]?key|authorization|signature)\\s*[:=]\\s*[^\\s,;]+")]
    private static partial Regex SecretLikePattern();

    [GeneratedRegex("(?i)Bearer\\s+[A-Za-z0-9._~+/=-]+")]
    private static partial Regex BearerPattern();
}

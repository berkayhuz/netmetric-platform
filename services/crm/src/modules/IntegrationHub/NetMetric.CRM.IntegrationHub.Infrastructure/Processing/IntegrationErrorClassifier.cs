// <copyright file="IntegrationErrorClassifier.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.RegularExpressions;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Connectors;

namespace NetMetric.CRM.IntegrationHub.Infrastructure.Processing;

public sealed record ClassifiedIntegrationError(
    bool IsRetryable,
    string Classification,
    string ErrorCode,
    string SanitizedMessage,
    TimeSpan? RetryAfter = null);

public static partial class IntegrationErrorClassifier
{
    public static ClassifiedIntegrationError Classify(Exception exception)
    {
        return exception switch
        {
            IntegrationRateLimitedException rateLimited => new(
                true,
                "rate_limit",
                rateLimited.ErrorCode,
                Sanitize(rateLimited.Message),
                rateLimited.RetryAfter),
            IntegrationTransientException transient => new(
                true,
                "transient",
                transient.ErrorCode,
                Sanitize(transient.Message)),
            IntegrationPermanentException permanent => new(
                false,
                "permanent",
                permanent.ErrorCode,
                Sanitize(permanent.Message)),
            OperationCanceledException => new(
                true,
                "transient",
                "operation_canceled",
                "Connector operation was canceled."),
            _ => new(
                true,
                "unexpected",
                "unexpected_error",
                Sanitize(exception.Message))
        };
    }

    public static ClassifiedIntegrationError NonRetryable(string classification, string code, string message)
        => new(false, classification, code, Sanitize(message));

    public static string Sanitize(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "Integration job failed.";
        }

        var sanitized = SecretLikePattern().Replace(message, "$1=[redacted]");
        sanitized = BearerPattern().Replace(sanitized, "Bearer [redacted]");
        return sanitized.Length <= 1000 ? sanitized : sanitized[..1000];
    }

    [GeneratedRegex("(?i)(secret|token|password|api[_-]?key)\\s*[:=]\\s*[^\\s,;]+")]
    private static partial Regex SecretLikePattern();

    [GeneratedRegex("(?i)Bearer\\s+[A-Za-z0-9._~+/=-]+")]
    private static partial Regex BearerPattern();
}

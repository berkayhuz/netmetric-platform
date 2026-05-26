// <copyright file="IntegrationProcessingAbstractions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.IntegrationHub.Application.Abstractions.Processing;

public interface IIntegrationJobProcessor
{
    Task<int> ProcessDueJobsAsync(CancellationToken cancellationToken);
}

public interface IIntegrationJobProcessingState
{
    bool IsEnabled { get; }
}

public interface IIntegrationWebhookSecurityService
{
    bool ValidateSignature(
        string secret,
        string payload,
        string timestamp,
        string signature,
        DateTime nowUtc,
        TimeSpan tolerance);

    string ComputePayloadHash(string payload);

    string ComputeSignatureHash(string signature);

    string CreateSignature(string secret, string payload, string timestamp);
}

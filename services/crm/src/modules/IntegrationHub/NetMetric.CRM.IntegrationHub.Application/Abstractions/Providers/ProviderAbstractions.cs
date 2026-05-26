// <copyright file="ProviderAbstractions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.IntegrationHub.Application.Abstractions.Providers;

public sealed record ProviderSecretSet(
    string? AccessToken,
    string? WebhookSigningSecret,
    string? VerifyToken);

public sealed record ProviderFieldDescriptor(
    string Key,
    string Label,
    string Description,
    bool IsSecret,
    bool Required,
    string Placeholder,
    string ValidationHint);

public sealed record ProviderCapabilityDescriptor(
    bool InboundMessages,
    bool OutboundMessages,
    bool Attachments,
    bool ReadReceipts,
    bool Templates,
    bool Webhooks,
    bool TestConnection);

public sealed record ProviderCatalogItem(
    string ProviderKey,
    string DisplayName,
    string Description,
    string IconKey,
    string Status,
    ProviderCapabilityDescriptor Capabilities,
    IReadOnlyCollection<ProviderFieldDescriptor> RequiredFields,
    string HelpText,
    bool ProductionReady);

public sealed record ProviderValidationInput(
    string ProviderKey,
    string DisplayName,
    string? AccessToken,
    string? WebhookSigningSecret,
    string? ConfigurationJson);

public sealed record ProviderValidationResult(
    bool IsValid,
    string Status,
    string Code,
    string Message);

public sealed record ProviderConnectionTestResult(
    bool Succeeded,
    string Status,
    string Code,
    string Message);

public sealed record ProviderWebhookVerificationInput(
    string ProviderKey,
    string? VerifyMode,
    string? VerifyToken,
    string? Challenge,
    string? SignatureHeader,
    string RawPayload,
    ProviderSecretSet Secrets);

public sealed record ProviderWebhookVerificationResult(
    bool IsValid,
    string Code,
    string Message,
    string? ChallengeResponse = null);

public sealed record NormalizedInboundMessage(
    string ProviderKey,
    string ExternalEventId,
    string ExternalMessageId,
    string ChannelAccountId,
    string ExternalConversationId,
    string SenderExternalId,
    string SenderDisplayName,
    string Text,
    DateTime SentAtUtc);

public sealed record ProviderInboundNormalizationResult(
    bool Succeeded,
    string Code,
    string Message,
    IReadOnlyCollection<NormalizedInboundMessage> Messages);

public sealed record ProviderOutboundSendInput(
    string ProviderKey,
    string RecipientExternalId,
    string Text,
    ProviderSecretSet Secrets,
    string? ConfigurationJson);

public sealed record ProviderOutboundSendResult(
    bool Succeeded,
    string Code,
    string Message,
    string? ExternalMessageId = null);

public interface IIntegrationProviderCatalog
{
    IReadOnlyCollection<ProviderCatalogItem> List();
    ProviderCatalogItem? Find(string providerKey);
}

public interface IProviderCredentialValidator
{
    ProviderValidationResult Validate(ProviderValidationInput input);
}

public interface IProviderConnectionTester
{
    ProviderConnectionTestResult Test(ProviderValidationInput input);
}

public interface IProviderWebhookVerifier
{
    string ProviderKey { get; }
    ProviderWebhookVerificationResult VerifyChallenge(ProviderWebhookVerificationInput input);
    ProviderWebhookVerificationResult VerifyPayload(ProviderWebhookVerificationInput input);
}

public interface IProviderInboundNormalizer
{
    string ProviderKey { get; }
    ProviderInboundNormalizationResult Normalize(string rawPayload);
}

public interface IProviderOutboundSender
{
    string ProviderKey { get; }
    ProviderOutboundSendResult Send(ProviderOutboundSendInput input);
}

public interface IProviderAdapter :
    IProviderWebhookVerifier,
    IProviderInboundNormalizer,
    IProviderOutboundSender
{
}

public interface IProviderAdapterRegistry
{
    IProviderAdapter? Resolve(string providerKey);
}

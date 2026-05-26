// <copyright file="OutboxContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Application.Abstractions.Outbox;

public interface IAccountOutboxWriter
{
    Task EnqueueAsync<TPayload>(
        Guid tenantId,
        string type,
        TPayload payload,
        string? correlationId,
        CancellationToken cancellationToken = default);
}

public static class AccountOutboxEventTypes
{
    public const string ProfileUpdated = "account.profile.updated";
    public const string PreferencesUpdated = "account.preferences.updated";
    public const string AvatarChanged = "account.avatar.changed";
    public const string AvatarDeleted = "account.avatar.deleted";
    public const string SessionRevoked = "account.session.revoked";
}

public sealed record AccountProfileUpdatedEvent(
    int EventVersion,
    Guid TenantId,
    Guid UserId,
    string? CorrelationId,
    DateTimeOffset OccurredAtUtc,
    IReadOnlyCollection<string> ChangedFields);

public sealed record AccountPreferencesUpdatedEvent(
    int EventVersion,
    Guid TenantId,
    Guid UserId,
    string? CorrelationId,
    DateTimeOffset OccurredAtUtc,
    string Theme,
    string Language,
    string TimeZone,
    string DateFormat);

public sealed record AccountAvatarChangedEvent(
    int EventVersion,
    Guid TenantId,
    Guid UserId,
    string? CorrelationId,
    DateTimeOffset OccurredAtUtc,
    Guid MediaAssetId,
    string ContentType,
    long SizeBytes,
    Guid? PreviousMediaAssetId);

public sealed record AccountAvatarDeletedEvent(
    int EventVersion,
    Guid TenantId,
    Guid UserId,
    string? CorrelationId,
    DateTimeOffset OccurredAtUtc,
    Guid? MediaAssetId);

public sealed record AccountSessionRevokedEvent(
    int EventVersion,
    Guid TenantId,
    Guid UserId,
    string? CorrelationId,
    DateTimeOffset OccurredAtUtc,
    Guid SessionId,
    string Reason);

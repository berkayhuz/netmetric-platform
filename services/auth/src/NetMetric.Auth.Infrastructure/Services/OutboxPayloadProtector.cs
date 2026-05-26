// <copyright file="OutboxPayloadProtector.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.DataProtection;
using NetMetric.Auth.Contracts.IntegrationEvents;

namespace NetMetric.Auth.Infrastructure.Services;

public sealed class OutboxPayloadProtector
{
    public const string ProtectedPayloadPrefix = "protected:v1:";

    private static readonly HashSet<string> SensitiveEventNames = new(StringComparer.Ordinal)
    {
        AuthEmailConfirmationRequestedV1.EventName,
        AuthPasswordResetRequestedV1.EventName,
        AuthEmailChangeRequestedV1.EventName,
        "notification.requested"
    };

    private readonly IDataProtector protector;

    public OutboxPayloadProtector(IDataProtectionProvider dataProtectionProvider)
    {
        protector = dataProtectionProvider.CreateProtector("NetMetric.Auth.Outbox.Payload.v1");
    }

    public string ProtectForStorage(string eventName, string payload)
    {
        if (!SensitiveEventNames.Contains(eventName) ||
            payload.StartsWith(ProtectedPayloadPrefix, StringComparison.Ordinal))
        {
            return payload;
        }

        return ProtectedPayloadPrefix + protector.Protect(payload);
    }

    public string UnprotectForPublish(string payload)
    {
        if (!payload.StartsWith(ProtectedPayloadPrefix, StringComparison.Ordinal))
        {
            return payload;
        }

        return protector.Unprotect(payload[ProtectedPayloadPrefix.Length..]);
    }
}

// <copyright file="AuditMetadataSanitizer.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Options;

namespace NetMetric.Account.Infrastructure.Audit;

public interface IAuditMetadataSanitizer
{
    IReadOnlyDictionary<string, string>? Sanitize(IReadOnlyDictionary<string, string>? metadata);
}

public sealed class AuditMetadataSanitizer(IOptions<AccountAuditOptions> options) : IAuditMetadataSanitizer
{
    private const string Mask = "***";

    public IReadOnlyDictionary<string, string>? Sanitize(IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
        {
            return null;
        }

        var maskedKeys = options.Value.MaskedMetadataKeys
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return metadata.ToDictionary(
            pair => pair.Key,
            pair => maskedKeys.Contains(pair.Key) ? Mask : pair.Value,
            StringComparer.OrdinalIgnoreCase);
    }
}

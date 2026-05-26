// <copyright file="IRecoveryCodeService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Abstractions;

public interface IRecoveryCodeService
{
    IReadOnlyCollection<string> GenerateCodes(int count);
    string HashCode(Guid tenantId, Guid userId, string code);
}

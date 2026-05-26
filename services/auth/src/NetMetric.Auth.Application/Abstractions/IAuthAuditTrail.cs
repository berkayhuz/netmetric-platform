// <copyright file="IAuthAuditTrail.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Auth.Application.Records;

namespace NetMetric.Auth.Application.Abstractions;

public interface IAuthAuditTrail
{
    Task WriteAsync(AuthAuditRecord record, CancellationToken cancellationToken);
}

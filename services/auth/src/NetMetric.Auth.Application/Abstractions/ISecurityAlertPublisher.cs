// <copyright file="ISecurityAlertPublisher.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Auth.Application.Records;

namespace NetMetric.Auth.Application.Abstractions;

public interface ISecurityAlertPublisher
{
    Task PublishAsync(SecurityAlert alert, CancellationToken cancellationToken);
}

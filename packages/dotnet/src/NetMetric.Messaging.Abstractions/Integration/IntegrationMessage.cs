// <copyright file="IntegrationMessage.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Messaging.Abstractions;

public sealed record IntegrationMessage(
    IntegrationEventMetadata Metadata,
    string Payload);

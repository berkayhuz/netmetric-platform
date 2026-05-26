// <copyright file="ChannelAccountDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Omnichannel.Contracts.DTOs;

public sealed record ChannelAccountDto(Guid Id, string Name, string ChannelType, string ExternalAccountId, string RoutingKey, bool IsActive);

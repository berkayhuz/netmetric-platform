// <copyright file="ChannelConversationDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Omnichannel.Contracts.DTOs;

public sealed record ChannelConversationDto(Guid Id, Guid AccountId, string Subject, string CustomerDisplayName, string Status, DateTime LastMessageAtUtc);

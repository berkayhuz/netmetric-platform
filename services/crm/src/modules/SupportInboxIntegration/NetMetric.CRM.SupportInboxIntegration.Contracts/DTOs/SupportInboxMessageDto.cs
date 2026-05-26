// <copyright file="SupportInboxMessageDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.SupportInboxIntegration.Contracts.DTOs;

public sealed record SupportInboxMessageDto(Guid Id, Guid ConnectionId, Guid? TicketId, string ExternalMessageId, string FromAddress, string Subject, DateTime ReceivedAtUtc, string Status, string? ProcessingError);

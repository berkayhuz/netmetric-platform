// <copyright file="GetSupportInboxMessagesQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.SupportInboxIntegration.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.SupportInboxIntegration.Application.Queries.Messages.GetSupportInboxMessages;

public sealed record GetSupportInboxMessagesQuery(Guid? ConnectionId, bool? LinkedToTicket, int Page, int PageSize) : IRequest<PagedResult<SupportInboxMessageDto>>;

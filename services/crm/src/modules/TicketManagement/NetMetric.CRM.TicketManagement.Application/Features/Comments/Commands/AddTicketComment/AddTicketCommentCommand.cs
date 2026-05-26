// <copyright file="AddTicketCommentCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketManagement.Contracts.DTOs;

namespace NetMetric.CRM.TicketManagement.Application.Features.Comments.Commands.AddTicketComment;

public sealed record AddTicketCommentCommand(Guid TicketId, string Comment, bool IsInternal) : IRequest<TicketCommentDto>;

// <copyright file="ChangeTicketStatusCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.TicketManagement.Application.Commands.Tickets;

public sealed record ChangeTicketStatusCommand(Guid TicketId, TicketStatusType Status, string? Note) : IRequest;

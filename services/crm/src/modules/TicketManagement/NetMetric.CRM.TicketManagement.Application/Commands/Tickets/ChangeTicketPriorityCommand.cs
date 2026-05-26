// <copyright file="ChangeTicketPriorityCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.TicketManagement.Application.Commands.Tickets;

public sealed record ChangeTicketPriorityCommand(Guid TicketId, PriorityType Priority) : IRequest;

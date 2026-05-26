// <copyright file="SoftDeleteTicketCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketManagement.Contracts.DTOs;

namespace NetMetric.CRM.TicketManagement.Application.Commands.Tickets;

public sealed record SoftDeleteTicketCommand(Guid TicketId) : IRequest;

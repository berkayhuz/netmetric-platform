// <copyright file="BulkSoftDeleteTicketsCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketManagement.Contracts.DTOs;

namespace NetMetric.CRM.TicketManagement.Application.Features.Bulk.Commands.BulkSoftDeleteTickets;

public sealed record BulkSoftDeleteTicketsCommand(IReadOnlyCollection<Guid> TicketIds) : IRequest<int>;

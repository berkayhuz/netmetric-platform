// <copyright file="UpdateTicketCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketManagement.Contracts.DTOs;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.TicketManagement.Application.Commands.Tickets;

public sealed record UpdateTicketCommand(Guid TicketId, string Subject, string? Description, TicketType TicketType, TicketChannelType Channel, PriorityType Priority, Guid? AssignedUserId, Guid? CustomerId, Guid? ContactId, Guid? TicketCategoryId, Guid? SlaPolicyId, DateTime? FirstResponseDueAt, DateTime? ResolveDueAt, string? Notes, byte[]? RowVersion) : IRequest<TicketDetailDto>;

// <copyright file="CreateTicketQueueCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.TicketWorkflowManagement.Domain.Enums;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Commands.Queues.CreateTicketQueue;

public sealed record CreateTicketQueueCommand(
    string Code,
    string Name,
    string? Description,
    TicketQueueAssignmentStrategy AssignmentStrategy,
    bool IsDefault) : IRequest<Guid>;

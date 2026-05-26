// <copyright file="AssignTicketOwnerCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Commands.Assignments.AssignTicketOwner;

public sealed record AssignTicketOwnerCommand(Guid TicketId, Guid? PreviousOwnerUserId, Guid NewOwnerUserId, Guid? QueueId, string? Reason) : IRequest;

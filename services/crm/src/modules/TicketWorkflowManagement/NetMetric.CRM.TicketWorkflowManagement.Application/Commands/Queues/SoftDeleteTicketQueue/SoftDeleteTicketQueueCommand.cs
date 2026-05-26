// <copyright file="SoftDeleteTicketQueueCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Commands.Queues.SoftDeleteTicketQueue;

public sealed record SoftDeleteTicketQueueCommand(Guid QueueId) : IRequest;

// <copyright file="RecordTicketStatusChangeCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Commands.StatusHistory.RecordTicketStatusChange;

public sealed record RecordTicketStatusChangeCommand(Guid TicketId, string PreviousStatus, string NewStatus, string? Note) : IRequest;

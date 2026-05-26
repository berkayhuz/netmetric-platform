// <copyright file="RunDueEscalationsCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.TicketSlaManagement.Application.Commands.TicketSla;

public sealed record RunDueEscalationsCommand(DateTime UtcNow) : IRequest<int>;

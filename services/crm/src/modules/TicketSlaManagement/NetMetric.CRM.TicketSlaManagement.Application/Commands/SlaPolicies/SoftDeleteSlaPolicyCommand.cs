// <copyright file="SoftDeleteSlaPolicyCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.TicketSlaManagement.Application.Commands.SlaPolicies;

public sealed record SoftDeleteSlaPolicyCommand(Guid Id) : IRequest;

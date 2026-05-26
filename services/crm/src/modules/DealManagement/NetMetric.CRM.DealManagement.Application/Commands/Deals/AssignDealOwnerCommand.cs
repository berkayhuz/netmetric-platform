// <copyright file="AssignDealOwnerCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.DealManagement.Application.Commands.Deals;

public sealed record AssignDealOwnerCommand(Guid DealId, Guid? OwnerUserId) : IRequest;

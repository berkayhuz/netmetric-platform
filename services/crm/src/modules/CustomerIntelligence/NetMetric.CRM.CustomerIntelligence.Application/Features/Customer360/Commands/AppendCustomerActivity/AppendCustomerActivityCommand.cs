// <copyright file="AppendCustomerActivityCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Customer360.Commands.AppendCustomerActivity;

public sealed record AppendCustomerActivityCommand(
    string SubjectType,
    Guid SubjectId,
    string Name,
    string Category,
    string? Channel,
    string? EntityType,
    Guid? RelatedEntityId,
    string? DataJson,
    DateTime? OccurredAtUtc) : IRequest<Customer360ActivityDto>;

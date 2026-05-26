// <copyright file="TrackBehavioralEventCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Cdp.Commands.TrackBehavioralEvent;

public sealed record TrackBehavioralEventCommand(
    string Source,
    string EventName,
    string SubjectType,
    Guid SubjectId,
    string? IdentityKey,
    string? Channel,
    string? PropertiesJson,
    DateTime? OccurredAtUtc) : IRequest<BehavioralEventDto>;

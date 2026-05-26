// <copyright file="CreateSavedViewCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.SavedViews.Commands.CreateSavedView;

public sealed record CreateSavedViewCommand(string Name, string Scope, string FilterJson) : IRequest<Guid>;

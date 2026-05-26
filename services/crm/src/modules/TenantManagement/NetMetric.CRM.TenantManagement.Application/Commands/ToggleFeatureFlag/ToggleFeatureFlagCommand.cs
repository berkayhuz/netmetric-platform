// <copyright file="ToggleFeatureFlagCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.TenantManagement.Application.Commands.ToggleFeatureFlag;

public sealed record ToggleFeatureFlagCommand(Guid TenantId, string Key, bool IsEnabled, DateTime? EffectiveFromUtc) : IRequest;

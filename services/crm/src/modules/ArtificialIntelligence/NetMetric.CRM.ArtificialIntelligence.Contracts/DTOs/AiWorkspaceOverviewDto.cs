// <copyright file="AiWorkspaceOverviewDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.ArtificialIntelligence.Contracts.DTOs;

public sealed record AiWorkspaceOverviewDto(int ActiveProviderCount, int AutomationPolicyCount, int CompletedRunCount, int FailedRunCount, IReadOnlyList<AiProviderConnectionDto> Providers);

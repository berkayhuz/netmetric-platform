// <copyright file="AiProviderConnectionDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.ArtificialIntelligence.Contracts.DTOs;

public sealed record AiProviderConnectionDto(Guid Id, string Name, string Provider, string ModelName, string Endpoint, IReadOnlyList<string> Capabilities, bool IsActive);

// <copyright file="UpsertAiProviderCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.ArtificialIntelligence.Contracts.DTOs;
using NetMetric.CRM.ArtificialIntelligence.Domain.Enums;

namespace NetMetric.CRM.ArtificialIntelligence.Application.Commands.Providers.UpsertAiProvider;

public sealed record UpsertAiProviderCommand(Guid? Id, string Name, AiProviderType Provider, string ModelName, string Endpoint, string SecretReference, IReadOnlyList<AiCapabilityType> Capabilities, bool IsActive) : IRequest<AiProviderConnectionDto>;

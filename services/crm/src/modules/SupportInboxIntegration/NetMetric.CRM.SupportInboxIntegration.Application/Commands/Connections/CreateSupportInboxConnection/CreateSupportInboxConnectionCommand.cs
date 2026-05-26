// <copyright file="CreateSupportInboxConnectionCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.SupportInboxIntegration.Contracts.DTOs;
using NetMetric.CRM.SupportInboxIntegration.Domain.Enums;

namespace NetMetric.CRM.SupportInboxIntegration.Application.Commands.Connections.CreateSupportInboxConnection;

public sealed record CreateSupportInboxConnectionCommand(string Name, SupportInboxProviderType Provider, string EmailAddress, string Host, int Port, string Username, string SecretReference, bool UseSsl) : IRequest<SupportInboxConnectionDto>;

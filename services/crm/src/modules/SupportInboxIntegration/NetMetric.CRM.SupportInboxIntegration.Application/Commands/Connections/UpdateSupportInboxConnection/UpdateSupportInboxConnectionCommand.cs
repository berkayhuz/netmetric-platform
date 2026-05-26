// <copyright file="UpdateSupportInboxConnectionCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.SupportInboxIntegration.Contracts.DTOs;

namespace NetMetric.CRM.SupportInboxIntegration.Application.Commands.Connections.UpdateSupportInboxConnection;

public sealed record UpdateSupportInboxConnectionCommand(Guid ConnectionId, string Name, string Host, int Port, string Username, string SecretReference, bool UseSsl, bool IsActive) : IRequest<SupportInboxConnectionDto>;

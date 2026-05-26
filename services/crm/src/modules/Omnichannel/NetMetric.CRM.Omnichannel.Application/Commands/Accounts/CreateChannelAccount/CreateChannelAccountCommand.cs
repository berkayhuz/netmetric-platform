// <copyright file="CreateChannelAccountCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.Omnichannel.Contracts.DTOs;
using NetMetric.CRM.Omnichannel.Domain.Enums;

namespace NetMetric.CRM.Omnichannel.Application.Commands.Accounts.CreateChannelAccount;

public sealed record CreateChannelAccountCommand(string Name, ChannelType ChannelType, string ExternalAccountId, string SecretReference, string RoutingKey) : IRequest<ChannelAccountDto>;

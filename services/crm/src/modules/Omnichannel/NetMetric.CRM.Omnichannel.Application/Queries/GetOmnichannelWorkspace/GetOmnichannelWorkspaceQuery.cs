// <copyright file="GetOmnichannelWorkspaceQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.Omnichannel.Contracts.DTOs;

namespace NetMetric.CRM.Omnichannel.Application.Queries.GetOmnichannelWorkspace;

public sealed record GetOmnichannelWorkspaceQuery : IRequest<OmnichannelWorkspaceDto>;

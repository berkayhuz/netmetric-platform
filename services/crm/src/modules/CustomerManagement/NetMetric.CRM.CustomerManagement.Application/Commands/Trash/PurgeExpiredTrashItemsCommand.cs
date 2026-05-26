// <copyright file="PurgeExpiredTrashItemsCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Trash;

public sealed record PurgeExpiredTrashItemsCommand(int BatchSize = 100, Guid? TenantId = null) : IRequest<int>;

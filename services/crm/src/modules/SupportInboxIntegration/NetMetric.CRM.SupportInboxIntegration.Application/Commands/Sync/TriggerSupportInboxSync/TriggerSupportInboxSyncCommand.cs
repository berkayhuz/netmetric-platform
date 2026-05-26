// <copyright file="TriggerSupportInboxSyncCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.SupportInboxIntegration.Application.Commands.Sync.TriggerSupportInboxSync;

public sealed record TriggerSupportInboxSyncCommand(Guid ConnectionId, bool DryRun) : IRequest;

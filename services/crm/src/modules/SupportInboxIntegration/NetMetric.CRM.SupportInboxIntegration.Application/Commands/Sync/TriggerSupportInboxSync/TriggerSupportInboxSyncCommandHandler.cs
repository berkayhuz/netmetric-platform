// <copyright file="TriggerSupportInboxSyncCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.SupportInboxIntegration.Application.Abstractions.Services;

namespace NetMetric.CRM.SupportInboxIntegration.Application.Commands.Sync.TriggerSupportInboxSync;

public sealed class TriggerSupportInboxSyncCommandHandler(ISupportInboxSynchronizationService synchronizationService) : IRequestHandler<TriggerSupportInboxSyncCommand>
{
    public async Task Handle(TriggerSupportInboxSyncCommand request, CancellationToken cancellationToken)
        => await synchronizationService.RunAsync(request.ConnectionId, request.DryRun, cancellationToken);
}

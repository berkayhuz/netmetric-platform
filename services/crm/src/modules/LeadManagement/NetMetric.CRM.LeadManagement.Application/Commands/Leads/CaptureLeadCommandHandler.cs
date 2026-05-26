// <copyright file="CaptureLeadCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Services;

namespace NetMetric.CRM.LeadManagement.Application.Commands.Leads;

public sealed class CaptureLeadCommandHandler(ILeadCaptureService captureService)
    : IRequestHandler<CaptureLeadCommand, Guid>
{
    public Task<Guid> Handle(CaptureLeadCommand request, CancellationToken cancellationToken)
        => captureService.CaptureAsync(request, cancellationToken);
}

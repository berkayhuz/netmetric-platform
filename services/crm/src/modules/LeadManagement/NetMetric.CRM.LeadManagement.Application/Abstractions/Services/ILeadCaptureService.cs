// <copyright file="ILeadCaptureService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.LeadManagement.Application.Commands.Leads;

namespace NetMetric.CRM.LeadManagement.Application.Abstractions.Services;

public interface ILeadCaptureService
{
    Task<Guid> CaptureAsync(CaptureLeadCommand request, CancellationToken cancellationToken);
}

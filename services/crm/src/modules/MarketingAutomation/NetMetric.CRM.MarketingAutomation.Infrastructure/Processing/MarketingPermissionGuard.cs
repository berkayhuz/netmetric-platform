// <copyright file="MarketingPermissionGuard.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.MarketingAutomation.Application.Abstractions;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.MarketingAutomation.Infrastructure.Processing;

public sealed class MarketingPermissionGuard(ICurrentUserService currentUserService) : IMarketingPermissionGuard
{
    public void Ensure(string permission)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.HasPermission(permission))
        {
            throw new UnauthorizedAccessException($"Marketing automation requires permission '{permission}'.");
        }
    }
}

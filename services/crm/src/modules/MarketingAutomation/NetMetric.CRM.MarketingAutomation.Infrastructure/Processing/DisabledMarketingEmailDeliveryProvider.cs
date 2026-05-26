// <copyright file="DisabledMarketingEmailDeliveryProvider.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.MarketingAutomation.Application.Abstractions;

namespace NetMetric.CRM.MarketingAutomation.Infrastructure.Processing;

public sealed class DisabledMarketingEmailDeliveryProvider : IMarketingEmailDeliveryProvider
{
    public Task<MarketingEmailDeliveryProviderResult> SendAsync(MarketingEmailDeliveryProviderRequest request, CancellationToken cancellationToken)
        => Task.FromException<MarketingEmailDeliveryProviderResult>(
            new InvalidOperationException("Marketing email delivery provider is not configured."));
}

// <copyright file="IEmailProvider.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Notification.Infrastructure.Modules.Email.Application;

public interface IEmailProvider
{
    string Name { get; }

    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}

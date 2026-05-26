// <copyright file="EmailMessage.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Notification.Infrastructure.Modules.Email.Application;

public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string TextBody,
    string? CorrelationId);

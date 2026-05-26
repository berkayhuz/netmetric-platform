// <copyright file="AccountSecurityNotificationPublisherTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Moq;
using NetMetric.Account.Application.Abstractions.Outbox;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Infrastructure.Outbox;
using NetMetric.Account.Infrastructure.Security;

namespace NetMetric.Account.Api.UnitTests;

public sealed class AccountSecurityNotificationPublisherTests
{
    [Fact]
    public async Task PublishAsync_ShouldEnqueueSecurityNotificationOutboxEvent()
    {
        var outbox = new Mock<IAccountOutboxWriter>();
        var publisher = new OutboxSecurityNotificationPublisher(outbox.Object);
        var request = new SecurityNotificationRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "password_changed",
            "127.0.0.1",
            "tests",
            DateTimeOffset.UtcNow);

        await publisher.PublishAsync(request, CancellationToken.None);

        outbox.Verify(x => x.EnqueueAsync(
            request.TenantId,
            OutboxEventTypes.SecurityNotificationRequested,
            request,
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

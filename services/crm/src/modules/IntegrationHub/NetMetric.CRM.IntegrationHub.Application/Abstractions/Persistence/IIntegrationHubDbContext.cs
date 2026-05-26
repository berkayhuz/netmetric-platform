// <copyright file="IIntegrationHubDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.IntegrationHub.Domain.Entities;

namespace NetMetric.CRM.IntegrationHub.Application.Abstractions.Persistence;

public interface IIntegrationHubDbContext
{
    DbSet<IntegrationConnection> IntegrationConnections { get; }
    DbSet<IntegrationJob> IntegrationJobs { get; }
    DbSet<IntegrationLogEntry> IntegrationLogEntries { get; }
    DbSet<WebhookSubscription> WebhookSubscriptions { get; }
    DbSet<IntegrationDeadLetterEntry> IntegrationDeadLetters { get; }
    DbSet<IntegrationJobExecutionLog> IntegrationJobExecutionLogs { get; }
    DbSet<IntegrationWebhookDelivery> WebhookDeliveries { get; }
    DbSet<ProviderCredential> ProviderCredentials { get; }
    DbSet<IntegrationApiKey> IntegrationApiKeys { get; }
    DbSet<WebhookDeliveryAttempt> WebhookDeliveryAttempts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

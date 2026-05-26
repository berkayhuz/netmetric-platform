// <copyright file="CustomerManagementTrashRetentionProcessor.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Commands.Trash;
using NetMetric.CRM.CustomerManagement.Infrastructure.Persistence;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.TrashRetention;

public sealed class CustomerManagementTrashRetentionProcessor(
    CustomerManagementDbContext dbContext,
    IMediator mediator,
    IOptions<CustomerManagementTrashRetentionOptions> options,
    ILogger<CustomerManagementTrashRetentionProcessor> logger) : ICustomerManagementTrashRetentionProcessor
{
    public async Task<TrashRetentionRunResult> ProcessCycleAsync(CancellationToken cancellationToken)
    {
        var configured = options.Value;
        if (!configured.Enabled)
        {
            return new TrashRetentionRunResult(0, 0);
        }

        var nowUtc = DateTime.UtcNow;
        var maxTenants = Math.Max(1, configured.MaxTenantsPerRun);
        var batchSize = Math.Max(1, configured.BatchSize);

        var tenantIds = await dbContext.GlobalTrashItems
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x =>
                (x.EntityType == CrmTrashEntityTypes.Contact
                 || x.EntityType == CrmTrashEntityTypes.Customer
                 || x.EntityType == CrmTrashEntityTypes.Company
                 || x.EntityType == CrmTrashEntityTypes.Lead
                 || x.EntityType == CrmTrashEntityTypes.Deal
                 || x.EntityType == CrmTrashEntityTypes.Opportunity
                 || x.EntityType == CrmTrashEntityTypes.Quote
                 || x.EntityType == CrmTrashEntityTypes.Ticket
                 || x.EntityType == CrmTrashEntityTypes.ProductCatalogItem)
                && x.Status == CrmTrashStatuses.Active
                && x.ExpiresAtUtc <= nowUtc)
            .Select(x => x.TenantId)
            .Distinct()
            .OrderBy(x => x)
            .Take(maxTenants)
            .ToListAsync(cancellationToken);

        var processedTenants = 0;
        var purgedItems = 0;

        foreach (var tenantId in tenantIds)
        {
            try
            {
                var purged = await mediator.Send(
                    new PurgeExpiredTrashItemsCommand(batchSize, tenantId),
                    cancellationToken);
                processedTenants++;
                purgedItems += purged;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Trash retention purge failed for tenant {TenantId}. Continuing with next tenant.",
                    tenantId);
            }
        }

        return new TrashRetentionRunResult(processedTenants, purgedItems);
    }
}

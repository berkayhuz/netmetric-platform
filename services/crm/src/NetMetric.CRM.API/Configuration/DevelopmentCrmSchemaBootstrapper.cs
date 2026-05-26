// <copyright file="DevelopmentCrmSchemaBootstrapper.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.AnalyticsReporting.Infrastructure.Persistence;
using NetMetric.CRM.ArtificialIntelligence.Infrastructure.Persistence;
using NetMetric.CRM.CalendarSync.Infrastructure.Persistence;
using NetMetric.CRM.ContractLifecycle.Infrastructure.Persistence;
using NetMetric.CRM.CustomerIntelligence.Infrastructure.Persistence;
using NetMetric.CRM.CustomerManagement.Infrastructure.Persistence;
using NetMetric.CRM.DealManagement.Infrastructure.Persistence;
using NetMetric.CRM.DocumentManagement.Infrastructure.Persistence;
using NetMetric.CRM.FinanceOperations.Infrastructure.Persistence;
using NetMetric.CRM.IntegrationHub.Infrastructure.Persistence;
using NetMetric.CRM.KnowledgeBaseManagement.Infrastructure.Persistence;
using NetMetric.CRM.LeadManagement.Infrastructure.Persistence;
using NetMetric.CRM.MarketingAutomation.Infrastructure.Persistence;
using NetMetric.CRM.Omnichannel.Infrastructure.Persistence;
using NetMetric.CRM.OpportunityManagement.Infrastructure.Persistence;
using NetMetric.CRM.PipelineManagement.Infrastructure.Persistence;
using NetMetric.CRM.ProductCatalog.Infrastructure.Persistence;
using NetMetric.CRM.QuoteManagement.Infrastructure.Persistence;
using NetMetric.CRM.SalesForecasting.Infrastructure.Persistence;
using NetMetric.CRM.SupportInboxIntegration.Infrastructure.Persistence;
using NetMetric.CRM.TagManagement.Infrastructure.Persistence;
using NetMetric.CRM.TenantManagement.Infrastructure.Persistence;
using NetMetric.CRM.TicketManagement.Infrastructure.Persistence;
using NetMetric.CRM.TicketSlaManagement.Infrastructure.Persistence;
using NetMetric.CRM.TicketWorkflowManagement.Infrastructure.Persistence;
using NetMetric.CRM.WorkflowAutomation.Infrastructure.Persistence;
using NetMetric.CRM.WorkManagement.Infrastructure.Persistence;

namespace NetMetric.CRM.API.Configuration;

internal static class DevelopmentCrmSchemaBootstrapper
{
    public static async Task EnsureSchemasAsync(IServiceProvider services, ILogger logger)
    {
        await EnsureDbContextSchemaAsync<CustomerManagementDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<TenantManagementDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<AnalyticsReportingDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<ArtificialIntelligenceDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<CalendarSyncDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<ContractLifecycleDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<CustomerIntelligenceDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<DealManagementDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<DocumentManagementDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<FinanceOperationsDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<IntegrationHubDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<KnowledgeBaseManagementDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<LeadManagementDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<MarketingAutomationDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<OmnichannelDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<OpportunityManagementDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<PipelineManagementDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<ProductCatalogDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<QuoteManagementDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<SalesForecastingDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<SupportInboxIntegrationDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<TagManagementDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<TicketManagementDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<TicketSlaManagementDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<TicketWorkflowManagementDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<WorkflowAutomationDbContext>(services, logger);
        await EnsureDbContextSchemaAsync<WorkManagementDbContext>(services, logger);

        await EnsureCustomerManagementCompatibilityPatchAsync(services, logger);
        await EnsureDealManagementCompatibilityPatchAsync(services, logger);
        await EnsureLeadManagementCompatibilityPatchAsync(services, logger);
        await EnsureOpportunityManagementCompatibilityPatchAsync(services, logger);
        await EnsureQuoteManagementCompatibilityPatchAsync(services, logger);
        await EnsurePipelineManagementCompatibilityPatchAsync(services, logger);
        await EnsureProductCatalogCompatibilityPatchAsync(services, logger);
        await EnsureTicketManagementCompatibilityPatchAsync(services, logger);
        await EnsureWorkManagementCompatibilityPatchAsync(services, logger);
    }

    private static async Task EnsureDbContextSchemaAsync<TContext>(
        IServiceProvider services,
        ILogger logger)
        where TContext : DbContext
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TContext>();

            await db.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{DbContext} schema bootstrap failed during development startup.",
                typeof(TContext).Name);
        }
    }

    private static async Task EnsureDealManagementCompatibilityPatchAsync(
        IServiceProvider services,
        ILogger logger)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DealManagementDbContext>();

            await db.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[DealManagementOutboxMessages]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [DealManagementOutboxMessages]
                    (
                        [Id] uniqueidentifier NOT NULL,
                        [TenantId] uniqueidentifier NOT NULL,
                        [EventName] nvarchar(256) NOT NULL,
                        [EventVersion] int NOT NULL,
                        [RoutingKey] nvarchar(256) NOT NULL,
                        [PayloadJson] nvarchar(max) NOT NULL,
                        [OccurredAtUtc] datetimeoffset NOT NULL,
                        [ProcessedAtUtc] datetimeoffset NULL,
                        [LockedUntilUtc] datetimeoffset NULL,
                        [LockedBy] nvarchar(128) NULL,
                        [NextAttemptAtUtc] datetimeoffset NULL,
                        [DeadLetteredAtUtc] datetimeoffset NULL,
                        [AttemptCount] int NOT NULL CONSTRAINT [DF_DealManagementOutboxMessages_AttemptCount] DEFAULT (0),
                        [LastError] nvarchar(1024) NULL,
                        [CorrelationId] nvarchar(128) NULL,
                        [IdempotencyKey] nvarchar(160) NULL,
                        [Version] rowversion NOT NULL,
                        CONSTRAINT [PK_DealManagementOutboxMessages] PRIMARY KEY ([Id])
                    );

                    CREATE INDEX [IX_DealManagementOutboxMessages_Processing]
                        ON [DealManagementOutboxMessages] ([ProcessedAtUtc], [DeadLetteredAtUtc], [NextAttemptAtUtc], [LockedUntilUtc], [OccurredAtUtc]);

                    CREATE INDEX [IX_DealManagementOutboxMessages_Tenant_Event_Occurred]
                        ON [DealManagementOutboxMessages] ([TenantId], [EventName], [OccurredAtUtc]);

                    CREATE UNIQUE INDEX [IX_DealManagementOutboxMessages_IdempotencyKey]
                        ON [DealManagementOutboxMessages] ([IdempotencyKey])
                        WHERE [IdempotencyKey] IS NOT NULL;
                END
                """);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Deal Management compatibility patch failed during development startup.");
        }
    }

    private static async Task EnsureCustomerManagementCompatibilityPatchAsync(
        IServiceProvider services,
        ILogger logger)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CustomerManagementDbContext>();

            await db.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[Customers]', N'U') IS NOT NULL AND COL_LENGTH('Customers', 'ProfileImageStorageKey') IS NULL
                    ALTER TABLE [Customers] ADD [ProfileImageStorageKey] nvarchar(512) NULL;

                IF OBJECT_ID(N'[Customers]', N'U') IS NOT NULL AND COL_LENGTH('Customers', 'ProfileImageUrl') IS NULL
                    ALTER TABLE [Customers] ADD [ProfileImageUrl] nvarchar(1024) NULL;

                IF OBJECT_ID(N'[Customers]', N'U') IS NOT NULL AND COL_LENGTH('Customers', 'ProfileImageContentType') IS NULL
                    ALTER TABLE [Customers] ADD [ProfileImageContentType] nvarchar(128) NULL;

                IF OBJECT_ID(N'[Companies]', N'U') IS NOT NULL AND COL_LENGTH('Companies', 'LogoStorageKey') IS NULL
                    ALTER TABLE [Companies] ADD [LogoStorageKey] nvarchar(512) NULL;

                IF OBJECT_ID(N'[Companies]', N'U') IS NOT NULL AND COL_LENGTH('Companies', 'LogoUrl') IS NULL
                    ALTER TABLE [Companies] ADD [LogoUrl] nvarchar(1024) NULL;

                IF OBJECT_ID(N'[Companies]', N'U') IS NOT NULL AND COL_LENGTH('Companies', 'LogoContentType') IS NULL
                    ALTER TABLE [Companies] ADD [LogoContentType] nvarchar(128) NULL;
                """);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Customer Management compatibility patch failed during development startup.");
        }
    }

    private static async Task EnsureProductCatalogCompatibilityPatchAsync(
        IServiceProvider services,
        ILogger logger)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();

            await db.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[CatalogProduct]', N'U') IS NOT NULL AND COL_LENGTH('CatalogProduct', 'CategoryId') IS NULL
                    ALTER TABLE [CatalogProduct] ADD [CategoryId] uniqueidentifier NULL;

                IF OBJECT_ID(N'[CatalogProduct]', N'U') IS NOT NULL AND COL_LENGTH('CatalogProduct', 'UnitPrice') IS NULL
                    ALTER TABLE [CatalogProduct] ADD [UnitPrice] decimal(18,2) NULL;

                IF OBJECT_ID(N'[CatalogProduct]', N'U') IS NOT NULL AND COL_LENGTH('CatalogProduct', 'CurrencyCode') IS NULL
                    ALTER TABLE [CatalogProduct] ADD [CurrencyCode] nvarchar(3) NOT NULL CONSTRAINT [DF_CatalogProduct_CurrencyCode] DEFAULT ('USD');

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[CatalogProduct]') AND name = N'CurrencyCode')
                    UPDATE [CatalogProduct] SET [CurrencyCode] = 'USD' WHERE [CurrencyCode] IS NULL;

                IF OBJECT_ID(N'[CatalogProduct]', N'U') IS NOT NULL AND COL_LENGTH('CatalogProduct', 'DefaultDiscountRate') IS NULL
                    ALTER TABLE [CatalogProduct] ADD [DefaultDiscountRate] decimal(5,2) NOT NULL CONSTRAINT [DF_CatalogProduct_DefaultDiscountRate] DEFAULT (0);

                IF OBJECT_ID(N'[CatalogProduct]', N'U') IS NOT NULL AND COL_LENGTH('CatalogProduct', 'DefaultTaxRate') IS NULL
                    ALTER TABLE [CatalogProduct] ADD [DefaultTaxRate] decimal(5,2) NOT NULL CONSTRAINT [DF_CatalogProduct_DefaultTaxRate] DEFAULT (0);

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[CatalogProduct]') AND name = N'DefaultDiscountRate' AND is_nullable = 1)
                BEGIN
                    UPDATE [CatalogProduct] SET [DefaultDiscountRate] = 0 WHERE [DefaultDiscountRate] IS NULL;
                    ALTER TABLE [CatalogProduct] ALTER COLUMN [DefaultDiscountRate] decimal(5,2) NOT NULL;
                END

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[CatalogProduct]') AND name = N'DefaultTaxRate' AND is_nullable = 1)
                BEGIN
                    UPDATE [CatalogProduct] SET [DefaultTaxRate] = 0 WHERE [DefaultTaxRate] IS NULL;
                    ALTER TABLE [CatalogProduct] ALTER COLUMN [DefaultTaxRate] decimal(5,2) NOT NULL;
                END

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[CatalogProduct]') AND name = N'DefaultDiscountRate')
                    AND NOT EXISTS (
                        SELECT 1
                        FROM sys.default_constraints dc
                        INNER JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
                        WHERE dc.parent_object_id = OBJECT_ID(N'[CatalogProduct]') AND c.name = N'DefaultDiscountRate'
                    )
                    ALTER TABLE [CatalogProduct] ADD CONSTRAINT [DF_CatalogProduct_DefaultDiscountRate] DEFAULT (0) FOR [DefaultDiscountRate];

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[CatalogProduct]') AND name = N'DefaultTaxRate')
                    AND NOT EXISTS (
                        SELECT 1
                        FROM sys.default_constraints dc
                        INNER JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
                        WHERE dc.parent_object_id = OBJECT_ID(N'[CatalogProduct]') AND c.name = N'DefaultTaxRate'
                    )
                    ALTER TABLE [CatalogProduct] ADD CONSTRAINT [DF_CatalogProduct_DefaultTaxRate] DEFAULT (0) FOR [DefaultTaxRate];

                IF OBJECT_ID(N'[CatalogProduct]', N'U') IS NOT NULL AND COL_LENGTH('CatalogProduct', 'PrimaryImageMediaAssetId') IS NULL
                    ALTER TABLE [CatalogProduct] ADD [PrimaryImageMediaAssetId] uniqueidentifier NULL;

                IF OBJECT_ID(N'[CatalogProduct]', N'U') IS NOT NULL AND COL_LENGTH('CatalogProduct', 'PrimaryImageUrl') IS NULL
                    ALTER TABLE [CatalogProduct] ADD [PrimaryImageUrl] nvarchar(2048) NULL;

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[CatalogProduct]') AND name = N'PrimaryImageMediaAssetId' AND is_nullable = 0)
                    ALTER TABLE [CatalogProduct] ALTER COLUMN [PrimaryImageMediaAssetId] uniqueidentifier NULL;

                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[CatalogProduct]') AND name = N'PrimaryImageUrl' AND is_nullable = 0)
                    ALTER TABLE [CatalogProduct] ALTER COLUMN [PrimaryImageUrl] nvarchar(2048) NULL;

                IF OBJECT_ID(N'[CatalogProduct]', N'U') IS NOT NULL
                    AND OBJECT_ID(N'[CatalogCategory]', N'U') IS NOT NULL
                    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CatalogProduct_CatalogCategory_CategoryId')
                    ALTER TABLE [CatalogProduct]
                    ADD CONSTRAINT [FK_CatalogProduct_CatalogCategory_CategoryId]
                    FOREIGN KEY ([CategoryId]) REFERENCES [CatalogCategory]([Id]) ON DELETE SET NULL;

                IF OBJECT_ID(N'[CatalogProduct]', N'U') IS NOT NULL
                BEGIN
                    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[CatalogProduct]') AND name = N'IX_CatalogProduct_TenantId_Code')
                        DROP INDEX [IX_CatalogProduct_TenantId_Code] ON [CatalogProduct];

                    CREATE UNIQUE INDEX [IX_CatalogProduct_TenantId_Code]
                        ON [CatalogProduct] ([TenantId], [Code])
                        WHERE [IsDeleted] = 0;
                END
                """);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Product Catalog compatibility patch failed during development startup.");
        }
    }

    private static async Task EnsureLeadManagementCompatibilityPatchAsync(
        IServiceProvider services,
        ILogger logger)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LeadManagementDbContext>();

            await db.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[LeadManagementOutboxMessages]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [LeadManagementOutboxMessages]
                    (
                        [Id] uniqueidentifier NOT NULL,
                        [TenantId] uniqueidentifier NOT NULL,
                        [EventName] nvarchar(256) NOT NULL,
                        [EventVersion] int NOT NULL,
                        [RoutingKey] nvarchar(256) NOT NULL,
                        [PayloadJson] nvarchar(max) NOT NULL,
                        [OccurredAtUtc] datetimeoffset NOT NULL,
                        [ProcessedAtUtc] datetimeoffset NULL,
                        [LockedUntilUtc] datetimeoffset NULL,
                        [LockedBy] nvarchar(128) NULL,
                        [NextAttemptAtUtc] datetimeoffset NULL,
                        [DeadLetteredAtUtc] datetimeoffset NULL,
                        [AttemptCount] int NOT NULL CONSTRAINT [DF_LeadManagementOutboxMessages_AttemptCount] DEFAULT (0),
                        [LastError] nvarchar(1024) NULL,
                        [CorrelationId] nvarchar(128) NULL,
                        [IdempotencyKey] nvarchar(160) NULL,
                        [Version] rowversion NOT NULL,
                        CONSTRAINT [PK_LeadManagementOutboxMessages] PRIMARY KEY ([Id])
                    );

                    CREATE INDEX [IX_LeadManagementOutboxMessages_Processing]
                        ON [LeadManagementOutboxMessages] ([ProcessedAtUtc], [DeadLetteredAtUtc], [NextAttemptAtUtc], [LockedUntilUtc], [OccurredAtUtc]);

                    CREATE INDEX [IX_LeadManagementOutboxMessages_Tenant_Event_Occurred]
                        ON [LeadManagementOutboxMessages] ([TenantId], [EventName], [OccurredAtUtc]);

                    CREATE UNIQUE INDEX [IX_LeadManagementOutboxMessages_IdempotencyKey]
                        ON [LeadManagementOutboxMessages] ([IdempotencyKey])
                        WHERE [IdempotencyKey] IS NOT NULL;
                END
                """);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lead Management compatibility patch failed during development startup.");
        }
    }

    private static async Task EnsureTicketManagementCompatibilityPatchAsync(
        IServiceProvider services,
        ILogger logger)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TicketManagementDbContext>();

            await db.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[TicketManagementOutboxMessages]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [TicketManagementOutboxMessages]
                    (
                        [Id] uniqueidentifier NOT NULL,
                        [TenantId] uniqueidentifier NOT NULL,
                        [EventName] nvarchar(256) NOT NULL,
                        [EventVersion] int NOT NULL,
                        [RoutingKey] nvarchar(256) NOT NULL,
                        [PayloadJson] nvarchar(max) NOT NULL,
                        [OccurredAtUtc] datetimeoffset NOT NULL,
                        [ProcessedAtUtc] datetimeoffset NULL,
                        [LockedUntilUtc] datetimeoffset NULL,
                        [LockedBy] nvarchar(128) NULL,
                        [NextAttemptAtUtc] datetimeoffset NULL,
                        [DeadLetteredAtUtc] datetimeoffset NULL,
                        [AttemptCount] int NOT NULL CONSTRAINT [DF_TicketManagementOutboxMessages_AttemptCount] DEFAULT (0),
                        [LastError] nvarchar(1024) NULL,
                        [CorrelationId] nvarchar(128) NULL,
                        [IdempotencyKey] nvarchar(160) NULL,
                        [Version] rowversion NOT NULL,
                        CONSTRAINT [PK_TicketManagementOutboxMessages] PRIMARY KEY ([Id])
                    );

                    CREATE INDEX [IX_TicketManagementOutboxMessages_Processing]
                        ON [TicketManagementOutboxMessages] ([ProcessedAtUtc], [DeadLetteredAtUtc], [NextAttemptAtUtc], [LockedUntilUtc], [OccurredAtUtc]);

                    CREATE INDEX [IX_TicketManagementOutboxMessages_Tenant_Event_Occurred]
                        ON [TicketManagementOutboxMessages] ([TenantId], [EventName], [OccurredAtUtc]);

                    CREATE UNIQUE INDEX [IX_TicketManagementOutboxMessages_IdempotencyKey]
                        ON [TicketManagementOutboxMessages] ([IdempotencyKey])
                        WHERE [IdempotencyKey] IS NOT NULL;
                END
                """);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ticket Management compatibility patch failed during development startup.");
        }
    }

    private static async Task EnsureOpportunityManagementCompatibilityPatchAsync(
        IServiceProvider services,
        ILogger logger)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OpportunityManagementDbContext>();

            await db.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[OpportunityManagementOutboxMessages]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [OpportunityManagementOutboxMessages]
                    (
                        [Id] uniqueidentifier NOT NULL,
                        [TenantId] uniqueidentifier NOT NULL,
                        [EventName] nvarchar(256) NOT NULL,
                        [EventVersion] int NOT NULL,
                        [RoutingKey] nvarchar(256) NOT NULL,
                        [PayloadJson] nvarchar(max) NOT NULL,
                        [OccurredAtUtc] datetimeoffset NOT NULL,
                        [ProcessedAtUtc] datetimeoffset NULL,
                        [LockedUntilUtc] datetimeoffset NULL,
                        [LockedBy] nvarchar(128) NULL,
                        [NextAttemptAtUtc] datetimeoffset NULL,
                        [DeadLetteredAtUtc] datetimeoffset NULL,
                        [AttemptCount] int NOT NULL CONSTRAINT [DF_OpportunityManagementOutboxMessages_AttemptCount] DEFAULT (0),
                        [LastError] nvarchar(1024) NULL,
                        [CorrelationId] nvarchar(128) NULL,
                        [IdempotencyKey] nvarchar(160) NULL,
                        [Version] rowversion NOT NULL,
                        CONSTRAINT [PK_OpportunityManagementOutboxMessages] PRIMARY KEY ([Id])
                    );

                    CREATE INDEX [IX_OpportunityManagementOutboxMessages_Processing]
                        ON [OpportunityManagementOutboxMessages] ([ProcessedAtUtc], [DeadLetteredAtUtc], [NextAttemptAtUtc], [LockedUntilUtc], [OccurredAtUtc]);

                    CREATE INDEX [IX_OpportunityManagementOutboxMessages_Tenant_Event_Occurred]
                        ON [OpportunityManagementOutboxMessages] ([TenantId], [EventName], [OccurredAtUtc]);

                    CREATE UNIQUE INDEX [IX_OpportunityManagementOutboxMessages_IdempotencyKey]
                        ON [OpportunityManagementOutboxMessages] ([IdempotencyKey])
                        WHERE [IdempotencyKey] IS NOT NULL;
                END
                """);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Opportunity Management compatibility patch failed during development startup.");
        }
    }

    private static async Task EnsureQuoteManagementCompatibilityPatchAsync(
        IServiceProvider services,
        ILogger logger)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<QuoteManagementDbContext>();

            await db.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[QuoteManagementOutboxMessages]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [QuoteManagementOutboxMessages]
                    (
                        [Id] uniqueidentifier NOT NULL,
                        [TenantId] uniqueidentifier NOT NULL,
                        [EventName] nvarchar(256) NOT NULL,
                        [EventVersion] int NOT NULL,
                        [RoutingKey] nvarchar(256) NOT NULL,
                        [PayloadJson] nvarchar(max) NOT NULL,
                        [OccurredAtUtc] datetimeoffset NOT NULL,
                        [ProcessedAtUtc] datetimeoffset NULL,
                        [LockedUntilUtc] datetimeoffset NULL,
                        [LockedBy] nvarchar(128) NULL,
                        [NextAttemptAtUtc] datetimeoffset NULL,
                        [DeadLetteredAtUtc] datetimeoffset NULL,
                        [AttemptCount] int NOT NULL CONSTRAINT [DF_QuoteManagementOutboxMessages_AttemptCount] DEFAULT (0),
                        [LastError] nvarchar(1024) NULL,
                        [CorrelationId] nvarchar(128) NULL,
                        [IdempotencyKey] nvarchar(160) NULL,
                        [Version] rowversion NOT NULL,
                        CONSTRAINT [PK_QuoteManagementOutboxMessages] PRIMARY KEY ([Id])
                    );

                    CREATE INDEX [IX_QuoteManagementOutboxMessages_Processing]
                        ON [QuoteManagementOutboxMessages] ([ProcessedAtUtc], [DeadLetteredAtUtc], [NextAttemptAtUtc], [LockedUntilUtc], [OccurredAtUtc]);

                    CREATE INDEX [IX_QuoteManagementOutboxMessages_Tenant_Event_Occurred]
                        ON [QuoteManagementOutboxMessages] ([TenantId], [EventName], [OccurredAtUtc]);

                    CREATE UNIQUE INDEX [IX_QuoteManagementOutboxMessages_IdempotencyKey]
                        ON [QuoteManagementOutboxMessages] ([IdempotencyKey])
                        WHERE [IdempotencyKey] IS NOT NULL;
                END
                """);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "QuoteManagement development compatibility patch skipped.");
        }
    }

    private static async Task EnsurePipelineManagementCompatibilityPatchAsync(
        IServiceProvider services,
        ILogger logger)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PipelineManagementDbContext>();

            await db.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[PipelineManagementOutboxMessages]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [PipelineManagementOutboxMessages]
                    (
                        [Id] uniqueidentifier NOT NULL,
                        [TenantId] uniqueidentifier NOT NULL,
                        [EventName] nvarchar(256) NOT NULL,
                        [EventVersion] int NOT NULL,
                        [RoutingKey] nvarchar(256) NOT NULL,
                        [PayloadJson] nvarchar(max) NOT NULL,
                        [OccurredAtUtc] datetimeoffset NOT NULL,
                        [ProcessedAtUtc] datetimeoffset NULL,
                        [LockedUntilUtc] datetimeoffset NULL,
                        [LockedBy] nvarchar(128) NULL,
                        [NextAttemptAtUtc] datetimeoffset NULL,
                        [DeadLetteredAtUtc] datetimeoffset NULL,
                        [AttemptCount] int NOT NULL CONSTRAINT [DF_PipelineManagementOutboxMessages_AttemptCount] DEFAULT (0),
                        [LastError] nvarchar(1024) NULL,
                        [CorrelationId] nvarchar(128) NULL,
                        [IdempotencyKey] nvarchar(160) NULL,
                        [Version] rowversion NOT NULL,
                        CONSTRAINT [PK_PipelineManagementOutboxMessages] PRIMARY KEY ([Id])
                    );

                    CREATE INDEX [IX_PipelineManagementOutboxMessages_Processing]
                        ON [PipelineManagementOutboxMessages] ([ProcessedAtUtc], [DeadLetteredAtUtc], [NextAttemptAtUtc], [LockedUntilUtc], [OccurredAtUtc]);

                    CREATE INDEX [IX_PipelineManagementOutboxMessages_Tenant_Event_Occurred]
                        ON [PipelineManagementOutboxMessages] ([TenantId], [EventName], [OccurredAtUtc]);

                    CREATE UNIQUE INDEX [IX_PipelineManagementOutboxMessages_IdempotencyKey]
                        ON [PipelineManagementOutboxMessages] ([IdempotencyKey])
                        WHERE [IdempotencyKey] IS NOT NULL;
                END
                """);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Pipeline Management compatibility patch failed during development startup.");
        }
    }

    private static async Task EnsureWorkManagementCompatibilityPatchAsync(
        IServiceProvider services,
        ILogger logger)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<WorkManagementDbContext>();

            await db.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[WorkTasks]', N'U') IS NOT NULL AND COL_LENGTH('WorkTasks', 'ReminderAtUtc') IS NULL
                    ALTER TABLE [WorkTasks] ADD [ReminderAtUtc] datetime2 NULL;

                IF OBJECT_ID(N'[WorkTasks]', N'U') IS NOT NULL AND COL_LENGTH('WorkTasks', 'CompletedAtUtc') IS NULL
                    ALTER TABLE [WorkTasks] ADD [CompletedAtUtc] datetime2 NULL;

                IF OBJECT_ID(N'[WorkTasks]', N'U') IS NOT NULL AND COL_LENGTH('WorkTasks', 'CompletedByUserId') IS NULL
                    ALTER TABLE [WorkTasks] ADD [CompletedByUserId] uniqueidentifier NULL;

                IF OBJECT_ID(N'[WorkTasks]', N'U') IS NOT NULL AND COL_LENGTH('WorkTasks', 'CompletionNote') IS NULL
                    ALTER TABLE [WorkTasks] ADD [CompletionNote] nvarchar(1000) NULL;
                """);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Work Management compatibility patch failed during development startup.");
        }
    }
}

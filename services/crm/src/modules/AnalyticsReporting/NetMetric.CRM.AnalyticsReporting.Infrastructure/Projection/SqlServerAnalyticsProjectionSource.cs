// <copyright file="SqlServerAnalyticsProjectionSource.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetMetric.CRM.AnalyticsReporting.Application.Abstractions;
using NetMetric.CRM.AnalyticsReporting.Application.Abstractions.Projection;
using NetMetric.CRM.AnalyticsReporting.Application.Batchs;

namespace NetMetric.CRM.AnalyticsReporting.Infrastructure.Projection;

public sealed class SqlServerAnalyticsProjectionSource(
    IConfiguration configuration,
    ILogger<SqlServerAnalyticsProjectionSource> logger) : IAnalyticsProjectionSource
{
    public async Task<AnalyticsProjectionBatch> ReadAsync(CancellationToken cancellationToken)
    {
        var projectedAtUtc = DateTime.UtcNow;
        var tenantNames = await ReadTenantNamesAsync(cancellationToken);
        var activeUsers = await ReadActiveUsersAsync(cancellationToken);
        var userNames = await ReadUserNamesAsync(cancellationToken);
        var customers = await ReadCustomerCountsAsync(cancellationToken);
        var salesFunnels = await ReadSalesFunnelsAsync(cancellationToken);
        var supportKpis = await ReadSupportKpisAsync(cancellationToken);
        var revenueAging = await ReadRevenueAgingAsync(projectedAtUtc, cancellationToken);
        var userProductivity = await ReadUserProductivityAsync(userNames, cancellationToken);

        var tenantIds = new HashSet<Guid>(
            tenantNames.Keys
                .Concat(activeUsers.Keys)
                .Concat(customers.Keys)
                .Concat(salesFunnels.Keys)
                .Concat(supportKpis.Keys)
                .Concat(revenueAging.Keys)
                .Concat(userProductivity.Select(x => x.TenantId)));

        var tenants = tenantIds
            .Select(tenantId => new TenantProjection(
                tenantId,
                tenantNames.GetValueOrDefault(tenantId, tenantId.ToString("D")),
                activeUsers.GetValueOrDefault(tenantId),
                customers.GetValueOrDefault(tenantId),
                revenueAging.GetValueOrDefault(tenantId)?.TotalRevenue ?? 0,
                supportKpis.GetValueOrDefault(tenantId)?.OpenTickets ?? 0))
            .OrderBy(x => x.TenantName)
            .ToArray();

        return new AnalyticsProjectionBatch(
            projectedAtUtc,
            tenants,
            salesFunnels.Values.ToArray(),
            [],
            revenueAging.Values.Select(x => x.ToProjection()).ToArray(),
            supportKpis.Values.ToArray(),
            userProductivity);
    }

    private async Task<Dictionary<Guid, string>> ReadTenantNamesAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT [TenantId], [Name]
            FROM [dbo].[TenantProfiles]
            WHERE [IsDeleted] = 0
            """;

        return await ReadSafeAsync(
            "TenantManagementConnection",
            "TenantProfiles",
            null,
            async connection =>
            {
                var result = new Dictionary<Guid, string>();
                await using var command = new SqlCommand(sql, connection);
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    result[reader.GetGuid(0)] = reader.GetString(1);
                }

                return result;
            },
            []);
    }

    private Task<Dictionary<Guid, int>> ReadActiveUsersAsync(CancellationToken cancellationToken)
        => ReadCountByTenantAsync(
            "AccountConnection",
            "account_user_profiles",
            null,
            null,
            cancellationToken);

    private async Task<Dictionary<(Guid TenantId, Guid UserId), string>> ReadUserNamesAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT [TenantId], [UserId], [DisplayName]
            FROM [dbo].[account_user_profiles]
            """;

        return await ReadSafeAsync(
            "AccountConnection",
            "account_user_profiles",
            null,
            async connection =>
            {
                var result = new Dictionary<(Guid TenantId, Guid UserId), string>();
                await using var command = new SqlCommand(sql, connection);
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    result[(reader.GetGuid(0), reader.GetGuid(1))] = reader.GetString(2);
                }

                return result;
            },
            []);
    }

    private Task<Dictionary<Guid, int>> ReadCustomerCountsAsync(CancellationToken cancellationToken)
        => ReadCountByTenantAsync(
            "CustomerManagementConnection",
            "Customers",
            null,
            "[IsDeleted] = 0",
            cancellationToken);

    private async Task<Dictionary<Guid, SalesFunnelProjection>> ReadSalesFunnelsAsync(CancellationToken cancellationToken)
    {
        var leads = await ReadSafeAsync(
            "LeadManagementConnection",
            "Leads",
            null,
            async connection =>
            {
                const string sql = """
                    SELECT
                        [TenantId],
                        SUM(CASE WHEN [Status] IN (0, 1, 8, 9) THEN 1 ELSE 0 END) AS [OpenLeads],
                        SUM(CASE WHEN [Status] = 2 THEN 1 ELSE 0 END) AS [QualifiedLeads]
                    FROM [dbo].[Leads]
                    WHERE [IsDeleted] = 0
                    GROUP BY [TenantId]
                    """;

                var result = new Dictionary<Guid, LeadFunnelRow>();
                await using var command = new SqlCommand(sql, connection);
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    result[reader.GetGuid(0)] = new LeadFunnelRow(GetInt32(reader, 1), GetInt32(reader, 2));
                }

                return result;
            },
            []);

        var opportunities = await ReadSafeAsync(
            "OpportunityManagementConnection",
            "Opportunities",
            null,
            async connection =>
            {
                const string sql = """
                    SELECT
                        [TenantId],
                        COUNT_BIG(*) AS [OpenOpportunities],
                        COALESCE(SUM([EstimatedAmount]), 0) AS [PipelineValue]
                    FROM [dbo].[Opportunities]
                    WHERE [IsDeleted] = 0 AND [Status] = 0
                    GROUP BY [TenantId]
                    """;

                var result = new Dictionary<Guid, OpportunityFunnelRow>();
                await using var command = new SqlCommand(sql, connection);
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    result[reader.GetGuid(0)] = new OpportunityFunnelRow(GetInt32(reader, 1), GetDecimal(reader, 2));
                }

                return result;
            },
            []);

        var wonDeals = await ReadSafeAsync(
            "DealManagementConnection",
            "Deals",
            null,
            async connection =>
            {
                const string sql = """
                    SELECT [TenantId], COUNT_BIG(*)
                    FROM [dbo].[Deals]
                    WHERE [IsDeleted] = 0
                      AND LOWER(COALESCE([Outcome], [Stage], '')) IN ('won', 'closedwon', 'closed won')
                    GROUP BY [TenantId]
                    """;

                var result = new Dictionary<Guid, int>();
                await using var command = new SqlCommand(sql, connection);
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    result[reader.GetGuid(0)] = GetInt32(reader, 1);
                }

                return result;
            },
            []);

        return leads.Keys
            .Concat(opportunities.Keys)
            .Concat(wonDeals.Keys)
            .Distinct()
            .ToDictionary(
                tenantId => tenantId,
                tenantId =>
                {
                    var lead = leads.GetValueOrDefault(tenantId);
                    var opportunity = opportunities.GetValueOrDefault(tenantId);
                    return new SalesFunnelProjection(
                        tenantId,
                        lead.OpenLeads,
                        lead.QualifiedLeads,
                        opportunity.OpenOpportunities,
                        wonDeals.GetValueOrDefault(tenantId),
                        opportunity.PipelineValue);
                });
    }

    private async Task<Dictionary<Guid, SupportKpiProjection>> ReadSupportKpisAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                [TenantId],
                SUM(CASE WHEN [Status] IN (0, 1, 2) THEN 1 ELSE 0 END) AS [OpenTickets],
                SUM(CASE WHEN [Status] IN (0, 1, 2)
                          AND (([FirstResponseDueAt] IS NOT NULL AND [FirstResponseDueAt] < SYSUTCDATETIME())
                               OR ([ResolveDueAt] IS NOT NULL AND [ResolveDueAt] < SYSUTCDATETIME()))
                         THEN 1 ELSE 0 END) AS [OverdueTickets],
                COALESCE(AVG(CASE WHEN [Status] IN (3, 4) AND [ClosedAt] IS NOT NULL
                                  THEN DATEDIFF(minute, [OpenedAt], [ClosedAt]) / 60.0 END), 0) AS [ResolutionHours]
            FROM [ticketing].[Tickets]
            WHERE [IsDeleted] = 0
            GROUP BY [TenantId]
            """;

        return await ReadSafeAsync(
            "TicketManagementConnection",
            "Tickets",
            "ticketing",
            async connection =>
            {
                var result = new Dictionary<Guid, SupportKpiProjection>();
                await using var command = new SqlCommand(sql, connection);
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    var tenantId = reader.GetGuid(0);
                    result[tenantId] = new SupportKpiProjection(
                        tenantId,
                        GetInt32(reader, 1),
                        GetInt32(reader, 2),
                        0,
                        GetDecimal(reader, 3));
                }

                return result;
            },
            []);
    }

    private async Task<Dictionary<Guid, RevenueAgingRow>> ReadRevenueAgingAsync(DateTime projectedAtUtc, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                [TenantId],
                COALESCE(SUM(CASE WHEN [Status] NOT IN ('Paid', 'Cancelled', 'Canceled') AND ([DueDateUtc] IS NULL OR [DueDateUtc] >= @TodayUtc) THEN [Amount] ELSE 0 END), 0) AS [CurrentAmount],
                COALESCE(SUM(CASE WHEN [Status] NOT IN ('Paid', 'Cancelled', 'Canceled') AND [DueDateUtc] < @TodayUtc AND [DueDateUtc] >= DATEADD(day, -30, @TodayUtc) THEN [Amount] ELSE 0 END), 0) AS [Days30],
                COALESCE(SUM(CASE WHEN [Status] NOT IN ('Paid', 'Cancelled', 'Canceled') AND [DueDateUtc] < DATEADD(day, -30, @TodayUtc) AND [DueDateUtc] >= DATEADD(day, -60, @TodayUtc) THEN [Amount] ELSE 0 END), 0) AS [Days60],
                COALESCE(SUM(CASE WHEN [Status] NOT IN ('Paid', 'Cancelled', 'Canceled') AND [DueDateUtc] < DATEADD(day, -60, @TodayUtc) THEN [Amount] ELSE 0 END), 0) AS [Days90Plus],
                COALESCE(SUM(CASE WHEN [Status] IN ('Paid', 'Issued', 'Sent') THEN [Amount] ELSE 0 END), 0) AS [TotalRevenue]
            FROM [dbo].[Invoice]
            WHERE [IsDeleted] = 0
            GROUP BY [TenantId]
            """;

        return await ReadSafeAsync(
            "FinanceOperationsConnection",
            "Invoice",
            null,
            async connection =>
            {
                var result = new Dictionary<Guid, RevenueAgingRow>();
                await using var command = new SqlCommand(sql, connection);
                command.Parameters.Add(new SqlParameter("@TodayUtc", SqlDbType.DateTime2) { Value = projectedAtUtc.Date });
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    result[reader.GetGuid(0)] = new RevenueAgingRow(
                        reader.GetGuid(0),
                        GetDecimal(reader, 1),
                        GetDecimal(reader, 2),
                        GetDecimal(reader, 3),
                        GetDecimal(reader, 4),
                        GetDecimal(reader, 5));
                }

                return result;
            },
            []);
    }

    private async Task<IReadOnlyCollection<UserProductivityProjection>> ReadUserProductivityAsync(
        IReadOnlyDictionary<(Guid TenantId, Guid UserId), string> userNames,
        CancellationToken cancellationToken)
    {
        var activities = await ReadUserMetricAsync(
            "WorkManagementConnection",
            "WorkActivities",
            null,
            """
            SELECT [TenantId], TRY_CONVERT(uniqueidentifier, [CreatedBy]) AS [UserId], COUNT_BIG(*)
            FROM [dbo].[WorkActivities]
            WHERE [IsDeleted] = 0 AND TRY_CONVERT(uniqueidentifier, [CreatedBy]) IS NOT NULL
            GROUP BY [TenantId], TRY_CONVERT(uniqueidentifier, [CreatedBy])
            """,
            cancellationToken);

        var tickets = await ReadUserMetricAsync(
            "TicketManagementConnection",
            "Tickets",
            "ticketing",
            """
            SELECT [TenantId], [AssignedUserId] AS [UserId], COUNT_BIG(*)
            FROM [ticketing].[Tickets]
            WHERE [IsDeleted] = 0 AND [AssignedUserId] IS NOT NULL AND [Status] IN (3, 4)
            GROUP BY [TenantId], [AssignedUserId]
            """,
            cancellationToken);

        var deals = await ReadUserMetricAsync(
            "DealManagementConnection",
            "Deals",
            null,
            """
            SELECT [TenantId], [OwnerUserId] AS [UserId], COUNT_BIG(*)
            FROM [dbo].[Deals]
            WHERE [IsDeleted] = 0 AND [OwnerUserId] IS NOT NULL
              AND LOWER(COALESCE([Outcome], [Stage], '')) IN ('won', 'closedwon', 'closed won')
            GROUP BY [TenantId], [OwnerUserId]
            """,
            cancellationToken);

        return activities.Keys
            .Concat(tickets.Keys)
            .Concat(deals.Keys)
            .Distinct()
            .Select(key => new UserProductivityProjection(
                key.TenantId,
                key.UserId,
                userNames.GetValueOrDefault(key, key.UserId.ToString("D")),
                activities.GetValueOrDefault(key),
                tickets.GetValueOrDefault(key),
                deals.GetValueOrDefault(key)))
            .OrderByDescending(x => x.DealsWon)
            .ThenByDescending(x => x.TicketsClosed)
            .ThenByDescending(x => x.ActivitiesCompleted)
            .Take(100)
            .ToArray();
    }

    private async Task<Dictionary<(Guid TenantId, Guid UserId), int>> ReadUserMetricAsync(
        string connectionName,
        string tableName,
        string? schema,
        string sql,
        CancellationToken cancellationToken)
        => await ReadSafeAsync(
            connectionName,
            tableName,
            schema,
            async connection =>
            {
                var result = new Dictionary<(Guid TenantId, Guid UserId), int>();
                await using var command = new SqlCommand(sql, connection);
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    result[(reader.GetGuid(0), reader.GetGuid(1))] = GetInt32(reader, 2);
                }

                return result;
            },
            []);

    private async Task<Dictionary<Guid, int>> ReadCountByTenantAsync(
        string connectionName,
        string tableName,
        string? schema,
        string? whereClause,
        CancellationToken cancellationToken)
    {
        var fullName = FullTableName(schema, tableName);
        var sql = $"""
            SELECT [TenantId], COUNT_BIG(*)
            FROM {fullName}
            {(string.IsNullOrWhiteSpace(whereClause) ? string.Empty : $"WHERE {whereClause}")}
            GROUP BY [TenantId]
            """;

        return await ReadSafeAsync(
            connectionName,
            tableName,
            schema,
            async connection =>
            {
                var result = new Dictionary<Guid, int>();
                await using var command = new SqlCommand(sql, connection);
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    result[reader.GetGuid(0)] = GetInt32(reader, 1);
                }

                return result;
            },
            []);
    }

    private async Task<T> ReadSafeAsync<T>(
        string connectionName,
        string tableName,
        string? schema,
        Func<SqlConnection, Task<T>> read,
        T fallback)
    {
        var connectionString = GetConnectionString(connectionName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogWarning(
                "Analytics projection skipped source because required connection string key is missing. Source={Source} Table={Table}",
                connectionName,
                FullObjectName(schema, tableName));
            return fallback;
        }

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            if (!await TableExistsAsync(connection, schema, tableName))
            {
                logger.LogInformation(
                    "Analytics projection source table does not exist yet. Source={Source} Table={Table}",
                    connectionName,
                    FullObjectName(schema, tableName));
                return fallback;
            }

            return await read(connection);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Analytics projection source read failed. Source={Source} Table={Table}",
                connectionName,
                FullObjectName(schema, tableName));
            return fallback;
        }
    }

    private string? GetConnectionString(string name)
        => configuration.GetConnectionString(name);

    private static async Task<bool> TableExistsAsync(SqlConnection connection, string? schema, string tableName)
    {
        const string sql = "SELECT CASE WHEN OBJECT_ID(@ObjectName, 'U') IS NULL THEN 0 ELSE 1 END";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@ObjectName", SqlDbType.NVarChar, 260)
        {
            Value = FullObjectName(schema, tableName)
        });

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) == 1;
    }

    private static string FullObjectName(string? schema, string tableName)
        => $"{(string.IsNullOrWhiteSpace(schema) ? "dbo" : schema)}.{tableName}";

    private static string FullTableName(string? schema, string tableName)
        => $"[{(string.IsNullOrWhiteSpace(schema) ? "dbo" : schema)}].[{tableName}]";

    private static int GetInt32(SqlDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? 0 : Convert.ToInt32(reader.GetValue(ordinal));

    private static decimal GetDecimal(SqlDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? 0 : Convert.ToDecimal(reader.GetValue(ordinal));

    private readonly record struct LeadFunnelRow(int OpenLeads, int QualifiedLeads);
    private readonly record struct OpportunityFunnelRow(int OpenOpportunities, decimal PipelineValue);

    private sealed record RevenueAgingRow(
        Guid TenantId,
        decimal CurrentAmount,
        decimal Days30,
        decimal Days60,
        decimal Days90Plus,
        decimal TotalRevenue)
    {
        public RevenueAgingProjection ToProjection() =>
            new(TenantId, CurrentAmount, Days30, Days60, Days90Plus);
    }
}

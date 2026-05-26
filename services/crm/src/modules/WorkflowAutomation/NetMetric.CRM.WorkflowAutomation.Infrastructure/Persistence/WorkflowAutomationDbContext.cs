// <copyright file="WorkflowAutomationDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Persistence;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.ApprovalSteps;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.ApprovalWorkflows;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.AssignmentRules;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.AutomationReminders;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.AutomationRules;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.RuleExecutionLogs;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.WebhookSubscriptions;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Persistence;

public sealed class WorkflowAutomationDbContext : AppDbContext, IWorkflowAutomationDbContext
{
    private readonly AuditSaveChangesInterceptor _auditInterceptor;
    private readonly SoftDeleteSaveChangesInterceptor _softDeleteInterceptor;
    private readonly TenantIsolationSaveChangesInterceptor _tenantInterceptor;

    public WorkflowAutomationDbContext(
        DbContextOptions<WorkflowAutomationDbContext> options,
        ITenantContext tenantContext,
        AuditSaveChangesInterceptor auditInterceptor,
        SoftDeleteSaveChangesInterceptor softDeleteInterceptor,
        TenantIsolationSaveChangesInterceptor tenantInterceptor) : base(options, tenantContext)
    {
        _auditInterceptor = auditInterceptor;
        _softDeleteInterceptor = softDeleteInterceptor;
        _tenantInterceptor = tenantInterceptor;
    }

    public DbSet<ApprovalWorkflow> ApprovalWorkflows => Set<ApprovalWorkflow>();
    public DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();
    public DbSet<AssignmentRule> AssignmentRules => Set<AssignmentRule>();
    public DbSet<AutomationRule> AutomationRules => Set<AutomationRule>();
    public DbSet<AutomationRuleVersion> AutomationRuleVersions => Set<AutomationRuleVersion>();
    public DbSet<AutomationReminder> AutomationReminders => Set<AutomationReminder>();
    public DbSet<WebhookSubscription> WebhookSubscriptions => Set<WebhookSubscription>();
    public DbSet<WebhookDeliveryLog> WebhookDeliveryLogs => Set<WebhookDeliveryLog>();
    public DbSet<RuleExecutionLog> RuleExecutionLogs => Set<RuleExecutionLog>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_tenantInterceptor, _auditInterceptor, _softDeleteInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkflowAutomationDbContext).Assembly);
        RemoveSqlServerColumnTypesForSqlite(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private void RemoveSqlServerColumnTypesForSqlite(ModelBuilder modelBuilder)
    {
        if (!string.Equals(Database.ProviderName, "Microsoft.EntityFrameworkCore.Sqlite", StringComparison.Ordinal))
        {
            return;
        }

        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(entity => entity.GetProperties()))
        {
            var columnType = property.GetColumnType();
            if (columnType is null)
            {
                continue;
            }

            if (columnType.StartsWith("nvarchar", StringComparison.OrdinalIgnoreCase) ||
                columnType.StartsWith("varbinary", StringComparison.OrdinalIgnoreCase) ||
                columnType.Equals("datetime2", StringComparison.OrdinalIgnoreCase) ||
                columnType.Equals("uniqueidentifier", StringComparison.OrdinalIgnoreCase) ||
                columnType.Equals("bit", StringComparison.OrdinalIgnoreCase) ||
                columnType.Equals("int", StringComparison.OrdinalIgnoreCase))
            {
                property.SetColumnType(null);
            }
        }
    }
}

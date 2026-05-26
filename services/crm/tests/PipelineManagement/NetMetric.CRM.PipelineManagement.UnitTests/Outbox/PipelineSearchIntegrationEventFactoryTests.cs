// <copyright file="PipelineSearchIntegrationEventFactoryTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.RegularExpressions;
using FluentAssertions;
using NetMetric.CRM.PipelineManagement.Application.Security;
using NetMetric.CRM.PipelineManagement.Domain.Entities;
using NetMetric.CRM.PipelineManagement.Infrastructure.Services;
using NetMetric.Search.Application.Security;
using NetMetric.Search.Contracts.Documents;

namespace NetMetric.CRM.PipelineManagement.UnitTests.Outbox;

public sealed class PipelineSearchIntegrationEventFactoryTests
{
    [Fact]
    public void CreatePipelineIndexRequested_Should_Map_Required_Search_Document_Fields()
    {
        var tenantId = Guid.NewGuid();
        var pipeline = CreatePipeline(tenantId);

        var integrationEvent = PipelineSearchIntegrationEventFactory.CreatePipelineIndexRequested(
            pipeline,
            tenantId,
            "corr-1",
            "cause-1",
            DateTimeOffset.UtcNow);

        integrationEvent.Document.Source.Should().Be(SearchDocumentSource.Crm);
        integrationEvent.Document.Type.Should().Be("pipeline");
        integrationEvent.Document.Visibility.Should().Be(SearchDocumentVisibility.Permission);
        integrationEvent.Document.PermissionMatchMode.Should().Be(SearchPermissionMatchMode.Any);
        integrationEvent.Document.TenantId.Should().Be(tenantId);
        integrationEvent.Document.RequiredPermissions.Should().ContainSingle(PipelineManagementPermissions.PipelinesRead);
        integrationEvent.Document.Locale.Should().Be(SearchDocumentLocales.Neutral);
        integrationEvent.Document.Url.Should().Be($"/pipeline?pipelineId={pipeline.Id:D}");
        integrationEvent.Document.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void CreatePipelineIndexRequested_Should_Create_Stable_Meilisearch_Safe_Id()
    {
        var tenantId = Guid.NewGuid();
        var pipeline = CreatePipeline(tenantId);
        var expectedId = $"crm-pipeline-{tenantId:N}-{pipeline.Id:N}";

        var integrationEvent = PipelineSearchIntegrationEventFactory.CreatePipelineIndexRequested(
            pipeline,
            tenantId,
            null,
            null,
            DateTimeOffset.UtcNow);

        integrationEvent.Document.Id.Should().Be(expectedId);
        Regex.IsMatch(integrationEvent.Document.Id, "^[a-z0-9-]+$").Should().BeTrue();
    }

    [Fact]
    public void CreatePipelineIndexRequested_Should_Contain_Only_Safe_Content_Fields()
    {
        var tenantId = Guid.NewGuid();
        var pipeline = CreatePipeline(tenantId);

        var integrationEvent = PipelineSearchIntegrationEventFactory.CreatePipelineIndexRequested(
            pipeline,
            tenantId,
            null,
            null,
            DateTimeOffset.UtcNow);

        integrationEvent.Document.Content.Should().Be(pipeline.Name);
        integrationEvent.Document.Content.ToLowerInvariant().Should().NotContain("internal notes");
        integrationEvent.Document.Content.ToLowerInvariant().Should().NotContain("sensitive");
    }

    [Fact]
    public void CreatePipelineDeleteRequested_Should_Use_Same_Document_Id_As_Index()
    {
        var tenantId = Guid.NewGuid();
        var pipeline = CreatePipeline(tenantId);

        var indexEvent = PipelineSearchIntegrationEventFactory.CreatePipelineIndexRequested(
            pipeline,
            tenantId,
            "corr-1",
            "cause-1",
            DateTimeOffset.UtcNow);
        var deleteEvent = PipelineSearchIntegrationEventFactory.CreatePipelineDeleteRequested(
            pipeline.Id,
            tenantId,
            "corr-1",
            "cause-1",
            DateTimeOffset.UtcNow);

        deleteEvent.DocumentId.Should().Be(indexEvent.Document.Id);
        deleteEvent.Source.Should().Be(SearchDocumentSource.Crm);
        deleteEvent.Type.Should().Be("pipeline");
        deleteEvent.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public void CreatePipelineIndexRequested_Should_Pass_SearchDocumentSecurityValidator()
    {
        var tenantId = Guid.NewGuid();
        var pipeline = CreatePipeline(tenantId);
        var integrationEvent = PipelineSearchIntegrationEventFactory.CreatePipelineIndexRequested(
            pipeline,
            tenantId,
            null,
            null,
            DateTimeOffset.UtcNow);

        var errors = SearchDocumentSecurityValidator.Validate(integrationEvent.Document);
        errors.Should().BeEmpty();
    }

    private static Pipeline CreatePipeline(Guid tenantId)
        => new()
        {
            TenantId = tenantId,
            Name = "Enterprise Sales Pipeline",
            Description = "sensitive stage explanation and internal notes",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddHours(-3)
        };
}

// <copyright file="ToolsDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.Tools.Application.Abstractions.Persistence;
using NetMetric.Tools.Domain.Entities;
using NetMetric.Tools.Domain.Enums;

namespace NetMetric.Tools.Persistence;

public sealed class ToolsDbContext : DbContext, IToolsDbContext
{
    public DbSet<ToolCategory> ToolCategories => Set<ToolCategory>();
    public DbSet<ToolDefinition> ToolDefinitions => Set<ToolDefinition>();
    public DbSet<ToolRun> ToolRuns => Set<ToolRun>();
    public DbSet<ToolArtifact> ToolArtifacts => Set<ToolArtifact>();

    public ToolsDbContext(DbContextOptions<ToolsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ToolsDbContext).Assembly);

        var imageCategoryId = Guid.Parse("ce572157-fba8-4381-bff6-1227099c2f4d");
        var generatorCategoryId = Guid.Parse("9f09b436-43f6-48d0-b13d-a92a55ea3be1");
        var pdfCategoryId = Guid.Parse("11ad909e-6d0f-452d-9577-52995819111f");

        modelBuilder.Entity<ToolCategory>().HasData(
            new { Id = imageCategoryId, Slug = "image", Title = "Image Tools", Description = "Fast image conversion and optimization tools.", SortOrder = 1 },
            new { Id = generatorCategoryId, Slug = "generators", Title = "Generators", Description = "Simple productivity generators.", SortOrder = 2 },
            new { Id = pdfCategoryId, Slug = "pdf", Title = "PDF Tools", Description = "PDF utilities and transformations.", SortOrder = 3 }
        );

        modelBuilder.Entity<ToolDefinition>().HasData(
            ToolDefinitionSeed(Guid.Parse("5f5be8d9-9ef4-46c4-9f7f-95ac4a3888a8"), "qr-generator", "QR Generator", "Generate QR codes instantly in your browser.", generatorCategoryId, true, ToolAvailabilityStatus.Enabled, "image/png"),
            ToolDefinitionSeed(Guid.Parse("dc4a1808-2554-41b2-a188-b4de133a131f"), "png-to-jpg", "PNG to JPG", "Convert PNG files to JPG without server upload.", imageCategoryId, true, ToolAvailabilityStatus.Enabled, "image/png,image/jpeg"),
            ToolDefinitionSeed(Guid.Parse("f2657e6a-f113-4ca4-8678-408f6dd3136f"), "jpg-to-png", "JPG to PNG", "Convert JPG files to PNG directly in your browser.", imageCategoryId, true, ToolAvailabilityStatus.Enabled, "image/jpeg,image/png"),
            ToolDefinitionSeed(Guid.Parse("bda040a4-45ef-44dc-b7e4-ed0406a2ca0d"), "pdf-compress", "PDF Compress", "Coming soon PDF compression.", pdfCategoryId, false, ToolAvailabilityStatus.ComingSoon, "application/pdf"),
            ToolDefinitionSeed(Guid.Parse("19ad1fef-c0d4-42ad-92f3-ee66e6aa36ef"), "image-resize", "Image Resize", "Coming soon image resizing.", imageCategoryId, false, ToolAvailabilityStatus.ComingSoon, "image/png,image/jpeg,image/webp"),
            ToolDefinitionSeed(Guid.Parse("24c7b651-4cdc-41ec-b056-4f818f511502"), "json-format", "JSON Formatter", "Coming soon JSON formatter.", generatorCategoryId, false, ToolAvailabilityStatus.ComingSoon, "application/json,text/plain"),
            ToolDefinitionSeed(Guid.Parse("f670cf63-cf5b-4eb6-80f1-1b8dcbf819f8"), "uuid-generator", "UUID Generator", "Coming soon UUID generator.", generatorCategoryId, false, ToolAvailabilityStatus.ComingSoon, "text/plain"),
            ToolDefinitionSeed(Guid.Parse("27f4f87f-d3ea-446f-b9c8-270f8f938dab"), "password-generator", "Password Generator", "Coming soon password generator.", generatorCategoryId, false, ToolAvailabilityStatus.ComingSoon, "text/plain")
        );
    }

    private static object ToolDefinitionSeed(Guid id, string slug, string title, string description, Guid categoryId, bool isEnabled, ToolAvailabilityStatus status, string mimes)
        => new
        {
            Id = id,
            Slug = slug,
            Title = title,
            Description = description,
            SeoTitle = $"{title} | NetMetric Tools",
            SeoDescription = description,
            CategoryId = categoryId,
            ExecutionMode = ToolExecutionMode.Browser,
            AvailabilityStatus = status,
            IsEnabled = isEnabled,
            GuestMaxFileBytes = 5L * 1024L * 1024L,
            AuthenticatedMaxSaveBytes = 10L * 1024L * 1024L,
            AcceptedMimeTypesCsv = mimes
        };
}

// <copyright file="ToolDefinition.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Tools.Domain.Enums;
using NetMetric.Tools.Domain.ValueObjects;

namespace NetMetric.Tools.Domain.Entities;

public sealed class ToolDefinition
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Slug { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string SeoTitle { get; private set; }
    public string SeoDescription { get; private set; }
    public Guid CategoryId { get; private set; }
    public ToolExecutionMode ExecutionMode { get; private set; }
    public ToolAvailabilityStatus AvailabilityStatus { get; private set; }
    public bool IsEnabled { get; private set; }
    public long GuestMaxFileBytes { get; private set; }
    public long AuthenticatedMaxSaveBytes { get; private set; }
    public string AcceptedMimeTypesCsv { get; private set; }

    private ToolDefinition()
    {
        Slug = string.Empty;
        Title = string.Empty;
        Description = string.Empty;
        SeoTitle = string.Empty;
        SeoDescription = string.Empty;
        AcceptedMimeTypesCsv = string.Empty;
    }

    public ToolDefinition(
        string slug,
        string title,
        string description,
        string seoTitle,
        string seoDescription,
        Guid categoryId,
        ToolExecutionMode executionMode,
        ToolAvailabilityStatus availabilityStatus,
        bool isEnabled,
        long guestMaxFileBytes,
        long authenticatedMaxSaveBytes,
        IEnumerable<string> acceptedMimeTypes)
    {
        Slug = new ToolSlug(slug).Value;
        Title = Require(title, nameof(title));
        Description = Require(description, nameof(description));
        SeoTitle = Require(seoTitle, nameof(seoTitle));
        SeoDescription = Require(seoDescription, nameof(seoDescription));
        if (categoryId == Guid.Empty) throw new ArgumentException("Category id is required.", nameof(categoryId));

        CategoryId = categoryId;
        ExecutionMode = executionMode;
        AvailabilityStatus = availabilityStatus;
        IsEnabled = isEnabled;
        GuestMaxFileBytes = guestMaxFileBytes;
        AuthenticatedMaxSaveBytes = authenticatedMaxSaveBytes;

        var mimes = acceptedMimeTypes?.Where(static x => !string.IsNullOrWhiteSpace(x)).Select(static x => x.Trim().ToLowerInvariant()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
            ?? [];

        if (mimes.Length == 0)
        {
            throw new ArgumentException("At least one accepted MIME type is required.", nameof(acceptedMimeTypes));
        }

        AcceptedMimeTypesCsv = string.Join(',', mimes);
    }

    public IReadOnlyCollection<string> AcceptedMimeTypes =>
        AcceptedMimeTypesCsv.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    private static string Require(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{name} is required.", name);
        }

        return value.Trim();
    }
}

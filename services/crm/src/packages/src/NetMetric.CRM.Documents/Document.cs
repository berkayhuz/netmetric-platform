// <copyright file="Document.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Documents;

public class Document : AuditableEntity
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string? Url { get; set; }
    public string PathOrUrl
    {
        get => Url ?? string.Empty;
        set => Url = value;
    }
    public long FileSize { get; set; }
    public bool IsPrivate { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
    public Guid? ContactId { get; set; }
    public Contact? Contact { get; set; }
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
}

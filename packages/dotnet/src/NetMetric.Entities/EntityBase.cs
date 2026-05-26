// <copyright file="EntityBase.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities.Abstractions;

namespace NetMetric.Entities;

public abstract class EntityBase : ITenantEntity, ISoftDeletable, IAuditable, IHasRowVersion
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public bool IsActive { get; protected set; } = true;
    public void SetActive(bool isActive) => IsActive = isActive;
    public void Activate() => SetActive(true);
    public void Deactivate() => SetActive(false);
}

// <copyright file="ContactUpsertRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerManagement.Contracts.Requests;

public sealed class ContactUpsertRequest
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? MobilePhone { get; set; }
    public string? WorkPhone { get; set; }
    public string? PersonalPhone { get; set; }
    public DateTime? BirthDate { get; set; }
    public GenderType Gender { get; set; } = GenderType.Unknown;
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? CustomerId { get; set; }
    public bool IsPrimaryContact { get; set; }
    public string? RowVersion { get; set; }
}

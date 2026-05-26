// <copyright file="Contact.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Core;

public class Contact : AuditableEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? MobilePhone { get; set; }
    public string? WorkPhone { get; set; }
    public string? PersonalPhone { get; set; }
    public DateTime? BirthDate { get; set; }
    public GenderType Gender { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public bool IsPrimaryContact { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();

    public void SetNotes(string? notes) => Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
}

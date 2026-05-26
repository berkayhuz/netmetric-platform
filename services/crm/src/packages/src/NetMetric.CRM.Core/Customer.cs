// <copyright file="Customer.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Core;

public class Customer : AuditableEntity
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
    public string? ProfileImageStorageKey { get; private set; }
    public string? ProfileImageUrl { get; private set; }
    public string? ProfileImageContentType { get; private set; }
    public Guid? OwnerUserId { get; set; }
    public CustomerType CustomerType { get; set; }
    public string? IdentityNumber { get; set; }
    public bool IsVip { get; set; }
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
    public ICollection<Contact> Contacts { get; set; } = [];
    public ICollection<Address> Addresses { get; set; } = [];
    public string FullName => $"{FirstName} {LastName}".Trim();

    public EntityReference ToEntityReference() => new(CrmEntityTypes.Customer, Id);

    public void SetNotes(string? notes) => Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

    public void SetProfileImage(string? storageKey, string? imageUrl, string? contentType)
    {
        ProfileImageStorageKey = string.IsNullOrWhiteSpace(storageKey) ? null : storageKey.Trim();
        ProfileImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        ProfileImageContentType = string.IsNullOrWhiteSpace(contentType) ? null : contentType.Trim();
    }
}

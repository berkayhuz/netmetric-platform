// <copyright file="ContactTrashIndexingTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Integration;
using NetMetric.CRM.DealManagement.Application.Abstractions.Integration;
using NetMetric.CRM.DealManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Infrastructure.Persistence;
using NetMetric.CRM.CustomerManagement.Infrastructure.Services;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Integration;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Integration;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.ProductCatalog.Infrastructure.Persistence;
using NetMetric.CRM.ProductCatalog.Domain.Entities.Categories;
using NetMetric.CRM.ProductCatalog.Domain.Entities.Products;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Integration;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.Sales;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Integration;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.Support;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Repository;
using NetMetric.Tenancy;
using NetMetric.CRM.ProductCatalog.Infrastructure.Services;

namespace NetMetric.CRM.CustomerManagement.UnitTests.Commands;

public sealed class ContactTrashIndexingTests
{
    [Fact]
    public async Task SoftDeleteContact_ShouldCreateGlobalTrashItem_WithExpectedFields()
    {
        await using var fixture = await Fixture.CreateAsync();
        var contact = fixture.CreateContact(firstName: "Ada", lastName: "Lovelace", email: "ada@example.com");

        await fixture.Service.SoftDeleteAsync(contact.Id, CancellationToken.None);

        var trashItem = await fixture.DbContext.GlobalTrashItems
            .IgnoreQueryFilters()
            .SingleAsync(x => x.EntityType == CrmTrashEntityTypes.Contact && x.EntityId == contact.Id);

        trashItem.TenantId.Should().Be(fixture.CurrentUser.TenantId);
        trashItem.Status.Should().Be(CrmTrashStatuses.Active);
        trashItem.DisplayName.Should().Be("Ada Lovelace");
        trashItem.SourceModule.Should().Be("contacts");
        trashItem.OriginalRoute.Should().Be($"/contacts/{contact.Id:D}");
        trashItem.DeletedByUserId.Should().Be(fixture.CurrentUser.UserId);
        trashItem.ExpiresAtUtc.Should().Be(trashItem.DeletedAtUtc.AddDays(7));
    }

    [Fact]
    public async Task AddContactDeletionAsync_ShouldNotCreateDuplicateActiveTrashItem()
    {
        await using var fixture = await Fixture.CreateAsync();
        var contact = fixture.CreateContact(firstName: "Grace", lastName: "Hopper", email: "grace@example.com");

        await fixture.TrashWriter.AddContactDeletionAsync(contact, CancellationToken.None);
        await fixture.TrashWriter.AddContactDeletionAsync(contact, CancellationToken.None);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        var count = await fixture.DbContext.GlobalTrashItems
            .IgnoreQueryFilters()
            .CountAsync(x =>
                x.TenantId == fixture.CurrentUser.TenantId
                && x.EntityType == CrmTrashEntityTypes.Contact
                && x.EntityId == contact.Id
                && x.Status == CrmTrashStatuses.Active);

        count.Should().Be(1);
    }

    [Fact]
    public async Task SoftDeleteContact_ShouldKeepTenantIsolationForTrashQuery()
    {
        await using var fixture = await Fixture.CreateAsync();
        var contact = fixture.CreateContact(firstName: "Tenant", lastName: "One", email: "tenant1@example.com");

        await fixture.Service.SoftDeleteAsync(contact.Id, CancellationToken.None);

        var visible = await fixture.DbContext.GlobalTrashItems.ToListAsync(CancellationToken.None);
        visible.Should().HaveCount(1);
        visible[0].TenantId.Should().Be(fixture.CurrentUser.TenantId);
    }

    [Fact]
    public async Task RestoreFromTrash_ShouldReactivateContact_AndMarkTrashItemRestored()
    {
        await using var fixture = await Fixture.CreateAsync();
        var contact = fixture.CreatePersistedDeletedContact(firstName: "Restore", lastName: "Me", email: "restore@example.com");
        var trashItem = fixture.CreateActiveTrashItemForContact(contact.Id);
        await fixture.DbContext.GlobalTrashItems.AddAsync(trashItem);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        await fixture.Service.RestoreFromTrashAsync(trashItem.Id, CancellationToken.None);

        var restoredContact = await fixture.DbContext.Contacts
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == contact.Id);
        restoredContact.IsDeleted.Should().BeFalse();
        restoredContact.DeletedAt.Should().BeNull();
        restoredContact.DeletedBy.Should().BeNull();

        var restoredTrashItem = await fixture.DbContext.GlobalTrashItems
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == trashItem.Id);
        restoredTrashItem.Status.Should().Be(CrmTrashStatuses.Restored);
        restoredTrashItem.RestoredAtUtc.Should().NotBeNull();
        restoredTrashItem.RestoredByUserId.Should().Be(fixture.CurrentUser.UserId);
    }

    [Fact]
    public async Task RestoreFromTrash_ShouldRejectExpiredActiveItems()
    {
        await using var fixture = await Fixture.CreateAsync();
        var contact = fixture.CreatePersistedDeletedContact(firstName: "Expired", lastName: "Item", email: "expired@example.com");
        var trashItem = fixture.CreateActiveTrashItemForContact(contact.Id);
        trashItem.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
        await fixture.DbContext.GlobalTrashItems.AddAsync(trashItem);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        var act = async () => await fixture.Service.RestoreFromTrashAsync(trashItem.Id, CancellationToken.None);
        await act.Should().ThrowAsync<ConflictAppException>();
    }

    [Fact]
    public async Task RestoreFromTrash_ShouldRejectUnsupportedEntityTypes()
    {
        await using var fixture = await Fixture.CreateAsync();
        var trashItem = new GlobalTrashItem
        {
            TenantId = fixture.CurrentUser.TenantId,
            EntityType = "unsupported-type",
            EntityId = Guid.NewGuid(),
            DisplayName = "Unsupported",
            SourceModule = "product-catalog",
            DeletedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            Status = CrmTrashStatuses.Active,
        };
        await fixture.DbContext.GlobalTrashItems.AddAsync(trashItem);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        var act = async () => await fixture.Service.RestoreFromTrashAsync(trashItem.Id, CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestAppException>();
    }

    [Fact]
    public async Task RestoreFromTrash_ShouldReactivateCatalogProduct_AndMarkTrashItemRestored()
    {
        await using var fixture = await Fixture.CreateAsync();
        var category = fixture.CreatePersistedActiveCatalogCategory("cat-01", "Hardware");
        var product = fixture.CreatePersistedDeletedCatalogProduct("prd-01", "Keyboard", category.Id);
        var trashItem = fixture.CreateActiveTrashItem(CrmTrashEntityTypes.ProductCatalogItem, product.Id, "Deleted product", "product-catalog");
        await fixture.DbContext.GlobalTrashItems.AddAsync(trashItem, CancellationToken.None);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        await fixture.Service.RestoreFromTrashAsync(trashItem.Id, CancellationToken.None);

        var restoredProduct = await fixture.ProductCatalogDbContext.Products
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == product.Id, CancellationToken.None);
        restoredProduct.IsDeleted.Should().BeFalse();
        restoredProduct.DeletedAt.Should().BeNull();
        restoredProduct.DeletedBy.Should().BeNull();

        var restoredTrashItem = await fixture.DbContext.GlobalTrashItems
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == trashItem.Id, CancellationToken.None);
        restoredTrashItem.Status.Should().Be(CrmTrashStatuses.Restored);
    }

    [Fact]
    public async Task RestoreFromTrash_ShouldRejectCatalogProduct_WhenCategoryDependencyDeleted()
    {
        await using var fixture = await Fixture.CreateAsync();
        var category = fixture.CreatePersistedDeletedCatalogCategory("cat-deleted", "Legacy");
        var product = fixture.CreatePersistedDeletedCatalogProduct("prd-02", "Legacy Keyboard", category.Id);
        var trashItem = fixture.CreateActiveTrashItem(CrmTrashEntityTypes.ProductCatalogItem, product.Id, "Deleted product", "product-catalog");
        await fixture.DbContext.GlobalTrashItems.AddAsync(trashItem, CancellationToken.None);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        var act = async () => await fixture.Service.RestoreFromTrashAsync(trashItem.Id, CancellationToken.None);
        await act.Should().ThrowAsync<ConflictAppException>()
            .WithMessage("*category dependency*");
    }

    [Fact]
    public async Task RestoreFromTrash_ShouldReactivateCustomer_AndMarkTrashItemRestored()
    {
        await using var fixture = await Fixture.CreateAsync();
        var customer = fixture.CreatePersistedDeletedCustomer("Ada", "Byron", "ada.byron@example.com");
        var trashItem = fixture.CreateActiveTrashItem(CrmTrashEntityTypes.Customer, customer.Id, "Deleted customer", "customers");
        await fixture.DbContext.GlobalTrashItems.AddAsync(trashItem);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        await fixture.Service.RestoreFromTrashAsync(trashItem.Id, CancellationToken.None);

        var restored = await fixture.DbContext.Customers
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == customer.Id, CancellationToken.None);
        restored.IsDeleted.Should().BeFalse();
        restored.DeletedAt.Should().BeNull();
        restored.DeletedBy.Should().BeNull();
    }

    [Fact]
    public async Task RestoreFromTrash_ShouldReactivateCompany_AndMarkTrashItemRestored()
    {
        await using var fixture = await Fixture.CreateAsync();
        var company = fixture.CreatePersistedDeletedCompany("Acme", "contact@acme.example");
        var trashItem = fixture.CreateActiveTrashItem(CrmTrashEntityTypes.Company, company.Id, "Deleted company", "companies");
        await fixture.DbContext.GlobalTrashItems.AddAsync(trashItem);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        await fixture.Service.RestoreFromTrashAsync(trashItem.Id, CancellationToken.None);

        var restored = await fixture.DbContext.Companies
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == company.Id, CancellationToken.None);
        restored.IsDeleted.Should().BeFalse();
        restored.DeletedAt.Should().BeNull();
        restored.DeletedBy.Should().BeNull();
    }

    [Fact]
    public async Task PurgeExpiredTrashItems_ShouldHardDeleteSoftDeletedContact_AndMarkTrashItemPurged()
    {
        await using var fixture = await Fixture.CreateAsync();
        var contact = fixture.CreatePersistedDeletedContact(firstName: "Purge", lastName: "Target", email: "purge@example.com");
        var trashItem = fixture.CreateActiveTrashItemForContact(contact.Id);
        trashItem.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
        await fixture.DbContext.GlobalTrashItems.AddAsync(trashItem);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        var purgedCount = await fixture.Service.PurgeExpiredTrashItemsAsync(100, CancellationToken.None);

        purgedCount.Should().Be(1);
        var purgedContact = await fixture.DbContext.Contacts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == contact.Id, CancellationToken.None);
        purgedContact.Should().BeNull();

        var purgedItem = await fixture.DbContext.GlobalTrashItems
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == trashItem.Id, CancellationToken.None);
        purgedItem.Status.Should().Be(CrmTrashStatuses.Purged);
        purgedItem.PurgedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task PurgeExpiredTrashItems_ShouldSkipNonExpiredRestoredAndUnsupportedItems()
    {
        await using var fixture = await Fixture.CreateAsync();
        var expiredDeletedContact = fixture.CreatePersistedDeletedContact("Expired", "Contact", "expired-contact@example.com");
        var notExpiredDeletedContact = fixture.CreatePersistedDeletedContact("Future", "Contact", "future-contact@example.com");
        var restoredDeletedContact = fixture.CreatePersistedDeletedContact("Restored", "Contact", "restored-contact@example.com");

        var expiredItem = fixture.CreateActiveTrashItemForContact(expiredDeletedContact.Id);
        expiredItem.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);

        var nonExpiredItem = fixture.CreateActiveTrashItemForContact(notExpiredDeletedContact.Id);
        nonExpiredItem.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30);

        var restoredItem = fixture.CreateActiveTrashItemForContact(restoredDeletedContact.Id);
        restoredItem.Status = CrmTrashStatuses.Restored;

        var unsupportedItem = new GlobalTrashItem
        {
            TenantId = fixture.CurrentUser.TenantId,
            EntityType = "unsupported",
            EntityId = Guid.NewGuid(),
            DisplayName = "Catalog item",
            SourceModule = "product-catalog",
            DeletedAtUtc = DateTime.UtcNow.AddMinutes(-20),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-5),
            Status = CrmTrashStatuses.Active,
        };

        await fixture.DbContext.GlobalTrashItems.AddRangeAsync(expiredItem, nonExpiredItem, restoredItem, unsupportedItem);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        var purgedCount = await fixture.Service.PurgeExpiredTrashItemsAsync(100, CancellationToken.None);

        purgedCount.Should().Be(1);
        var expiredAfter = await fixture.DbContext.GlobalTrashItems.IgnoreQueryFilters()
            .SingleAsync(x => x.Id == expiredItem.Id, CancellationToken.None);
        expiredAfter.Status.Should().Be(CrmTrashStatuses.Purged);

        var nonExpiredAfter = await fixture.DbContext.GlobalTrashItems.IgnoreQueryFilters()
            .SingleAsync(x => x.Id == nonExpiredItem.Id, CancellationToken.None);
        nonExpiredAfter.Status.Should().Be(CrmTrashStatuses.Active);

        var restoredAfter = await fixture.DbContext.GlobalTrashItems.IgnoreQueryFilters()
            .SingleAsync(x => x.Id == restoredItem.Id, CancellationToken.None);
        restoredAfter.Status.Should().Be(CrmTrashStatuses.Restored);

        var unsupportedAfter = await fixture.DbContext.GlobalTrashItems.IgnoreQueryFilters()
            .SingleAsync(x => x.Id == unsupportedItem.Id, CancellationToken.None);
        unsupportedAfter.Status.Should().Be(CrmTrashStatuses.Active);
    }

    [Fact]
    public async Task PurgeExpiredTrashItems_ShouldHardDeleteSoftDeletedCustomer_AndMarkTrashItemPurged()
    {
        await using var fixture = await Fixture.CreateAsync();
        var customer = fixture.CreatePersistedDeletedCustomer("Purge", "Customer", "purge-customer@example.com");
        var trashItem = fixture.CreateActiveTrashItem(CrmTrashEntityTypes.Customer, customer.Id, "Deleted customer", "customers");
        trashItem.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
        await fixture.DbContext.GlobalTrashItems.AddAsync(trashItem, CancellationToken.None);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        var purgedCount = await fixture.Service.PurgeExpiredTrashItemsAsync(100, CancellationToken.None);

        purgedCount.Should().Be(1);
        var purgedCustomer = await fixture.DbContext.Customers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == customer.Id, CancellationToken.None);
        purgedCustomer.Should().BeNull();

        var purgedItem = await fixture.DbContext.GlobalTrashItems
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == trashItem.Id, CancellationToken.None);
        purgedItem.Status.Should().Be(CrmTrashStatuses.Purged);
        purgedItem.PurgedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task PurgeExpiredTrashItems_ShouldHardDeleteSoftDeletedCompany_AndMarkTrashItemPurged()
    {
        await using var fixture = await Fixture.CreateAsync();
        var company = fixture.CreatePersistedDeletedCompany("Purge Company", "purge-company@example.com");
        var trashItem = fixture.CreateActiveTrashItem(CrmTrashEntityTypes.Company, company.Id, "Deleted company", "companies");
        trashItem.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
        await fixture.DbContext.GlobalTrashItems.AddAsync(trashItem, CancellationToken.None);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        var purgedCount = await fixture.Service.PurgeExpiredTrashItemsAsync(100, CancellationToken.None);

        purgedCount.Should().Be(1);
        var purgedCompany = await fixture.DbContext.Companies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == company.Id, CancellationToken.None);
        purgedCompany.Should().BeNull();

        var purgedItem = await fixture.DbContext.GlobalTrashItems
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == trashItem.Id, CancellationToken.None);
        purgedItem.Status.Should().Be(CrmTrashStatuses.Purged);
        purgedItem.PurgedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task PurgeExpiredTrashItems_ShouldHardDeleteSoftDeletedCatalogProduct_AndMarkTrashItemPurged()
    {
        await using var fixture = await Fixture.CreateAsync();
        var category = fixture.CreatePersistedActiveCatalogCategory("cat-purge", "Purge Category");
        var product = fixture.CreatePersistedDeletedCatalogProduct("prd-purge", "Purge Product", category.Id);
        var trashItem = fixture.CreateActiveTrashItem(CrmTrashEntityTypes.ProductCatalogItem, product.Id, "Deleted product", "product-catalog");
        trashItem.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
        await fixture.DbContext.GlobalTrashItems.AddAsync(trashItem, CancellationToken.None);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        var purgedCount = await fixture.Service.PurgeExpiredTrashItemsAsync(100, CancellationToken.None);

        purgedCount.Should().Be(1);
        var purgedProduct = await fixture.ProductCatalogDbContext.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == product.Id, CancellationToken.None);
        purgedProduct.Should().BeNull();

        var purgedItem = await fixture.DbContext.GlobalTrashItems
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == trashItem.Id, CancellationToken.None);
        purgedItem.Status.Should().Be(CrmTrashStatuses.Purged);
        purgedItem.PurgedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task PurgeExpiredTrashItems_ShouldSkip_WhenCatalogProductAlreadyActive()
    {
        await using var fixture = await Fixture.CreateAsync();
        var category = fixture.CreatePersistedActiveCatalogCategory("cat-active", "Active Category");
        var product = fixture.CreatePersistedActiveCatalogProduct("prd-active", "Active Product", category.Id);
        var trashItem = fixture.CreateActiveTrashItem(CrmTrashEntityTypes.ProductCatalogItem, product.Id, "Deleted product", "product-catalog");
        trashItem.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
        await fixture.DbContext.GlobalTrashItems.AddAsync(trashItem, CancellationToken.None);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        var purgedCount = await fixture.Service.PurgeExpiredTrashItemsAsync(100, CancellationToken.None);

        purgedCount.Should().Be(0);
        var itemAfter = await fixture.DbContext.GlobalTrashItems.IgnoreQueryFilters()
            .SingleAsync(x => x.Id == trashItem.Id, CancellationToken.None);
        itemAfter.Status.Should().Be(CrmTrashStatuses.Active);
    }

    [Fact]
    public async Task PurgeExpiredTrashItems_ShouldSkip_WhenCustomerAlreadyActive()
    {
        await using var fixture = await Fixture.CreateAsync();
        var activeCustomer = fixture.CreatePersistedActiveCustomer("Active", "Customer", "active-customer@example.com");
        var expiredItem = fixture.CreateActiveTrashItem(CrmTrashEntityTypes.Customer, activeCustomer.Id, "Deleted customer", "customers");
        expiredItem.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
        await fixture.DbContext.GlobalTrashItems.AddAsync(expiredItem, CancellationToken.None);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        var purgedCount = await fixture.Service.PurgeExpiredTrashItemsAsync(100, CancellationToken.None);

        purgedCount.Should().Be(0);
        var itemAfter = await fixture.DbContext.GlobalTrashItems.IgnoreQueryFilters()
            .SingleAsync(x => x.Id == expiredItem.Id, CancellationToken.None);
        itemAfter.Status.Should().Be(CrmTrashStatuses.Active);
    }

    [Fact]
    public async Task PurgeExpiredTrashItems_ShouldSkip_WhenCompanyAlreadyActive()
    {
        await using var fixture = await Fixture.CreateAsync();
        var activeCompany = fixture.CreatePersistedActiveCompany("Active Company", "active-company@example.com");
        var expiredItem = fixture.CreateActiveTrashItem(CrmTrashEntityTypes.Company, activeCompany.Id, "Deleted company", "companies");
        expiredItem.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
        await fixture.DbContext.GlobalTrashItems.AddAsync(expiredItem, CancellationToken.None);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        var purgedCount = await fixture.Service.PurgeExpiredTrashItemsAsync(100, CancellationToken.None);

        purgedCount.Should().Be(0);
        var itemAfter = await fixture.DbContext.GlobalTrashItems.IgnoreQueryFilters()
            .SingleAsync(x => x.Id == expiredItem.Id, CancellationToken.None);
        itemAfter.Status.Should().Be(CrmTrashStatuses.Active);
    }

    [Fact]
    public async Task PurgeExpiredTrashItems_ShouldBeIdempotent_WhenReRun()
    {
        await using var fixture = await Fixture.CreateAsync();
        var contact = fixture.CreatePersistedDeletedContact("Idempotent", "Contact", "idempotent@example.com");
        var expiredItem = fixture.CreateActiveTrashItemForContact(contact.Id);
        expiredItem.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
        await fixture.DbContext.GlobalTrashItems.AddAsync(expiredItem);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        var firstRun = await fixture.Service.PurgeExpiredTrashItemsAsync(100, CancellationToken.None);
        var secondRun = await fixture.Service.PurgeExpiredTrashItemsAsync(100, CancellationToken.None);

        firstRun.Should().Be(1);
        secondRun.Should().Be(0);
    }

    [Fact]
    public async Task PurgeExpiredTrashItems_ShouldSkip_WhenContactAlreadyActive()
    {
        await using var fixture = await Fixture.CreateAsync();
        var activeContact = fixture.CreatePersistedActiveContact("Active", "Contact", "active-contact@example.com");
        var expiredItem = fixture.CreateActiveTrashItemForContact(activeContact.Id);
        expiredItem.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
        await fixture.DbContext.GlobalTrashItems.AddAsync(expiredItem);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        var purgedCount = await fixture.Service.PurgeExpiredTrashItemsAsync(100, CancellationToken.None);

        purgedCount.Should().Be(0);
        var itemAfter = await fixture.DbContext.GlobalTrashItems.IgnoreQueryFilters()
            .SingleAsync(x => x.Id == expiredItem.Id, CancellationToken.None);
        itemAfter.Status.Should().Be(CrmTrashStatuses.Active);
    }

    [Fact]
    public async Task PurgeExpiredTrashItems_ShouldMarkExpiredActiveItemPurged_WhenContactMissing()
    {
        await using var fixture = await Fixture.CreateAsync();
        var missingContactId = Guid.NewGuid();
        var expiredItem = fixture.CreateActiveTrashItemForContact(missingContactId);
        expiredItem.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
        await fixture.DbContext.GlobalTrashItems.AddAsync(expiredItem);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        var purgedCount = await fixture.Service.PurgeExpiredTrashItemsAsync(100, CancellationToken.None);

        purgedCount.Should().Be(1);
        var itemAfter = await fixture.DbContext.GlobalTrashItems.IgnoreQueryFilters()
            .SingleAsync(x => x.Id == expiredItem.Id, CancellationToken.None);
        itemAfter.Status.Should().Be(CrmTrashStatuses.Purged);
        itemAfter.PurgedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task PurgeExpiredTrashItems_ShouldPreserveTenantIsolation()
    {
        await using var fixture = await Fixture.CreateAsync();
        var otherTenantItem = new GlobalTrashItem
        {
            TenantId = Guid.NewGuid(),
            EntityType = CrmTrashEntityTypes.Contact,
            EntityId = Guid.NewGuid(),
            DisplayName = "Other tenant",
            SourceModule = "contacts",
            DeletedAtUtc = DateTime.UtcNow.AddMinutes(-10),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1),
            Status = CrmTrashStatuses.Active,
        };

        await fixture.DbContext.GlobalTrashItems.AddAsync(otherTenantItem);
        await fixture.DbContext.SaveChangesAsync(CancellationToken.None);

        var purgedCount = await fixture.Service.PurgeExpiredTrashItemsAsync(100, CancellationToken.None);

        purgedCount.Should().Be(0);
        var unchanged = await fixture.DbContext.GlobalTrashItems
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == otherTenantItem.Id, CancellationToken.None);
        unchanged.Status.Should().Be(CrmTrashStatuses.Active);
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly SqliteConnection _productCatalogConnection;

        private Fixture(
            SqliteConnection connection,
            SqliteConnection productCatalogConnection,
            CustomerManagementDbContext dbContext,
            ProductCatalogDbContext productCatalogDbContext,
            ContactAdministrationService service,
            GlobalTrashIndexWriter trashWriter,
            FakeCurrentUserService currentUser)
        {
            _connection = connection;
            _productCatalogConnection = productCatalogConnection;
            DbContext = dbContext;
            ProductCatalogDbContext = productCatalogDbContext;
            Service = service;
            TrashWriter = trashWriter;
            CurrentUser = currentUser;
        }

        public CustomerManagementDbContext DbContext { get; }
        public ProductCatalogDbContext ProductCatalogDbContext { get; }
        public ContactAdministrationService Service { get; }
        public GlobalTrashIndexWriter TrashWriter { get; }
        public FakeCurrentUserService CurrentUser { get; }

        public static async Task<Fixture> CreateAsync()
        {
            var tenantId = Guid.NewGuid();
            var currentUser = new FakeCurrentUserService(
                Guid.NewGuid(),
                tenantId,
                "pilot-user",
                "pilot-user@netmetric.test",
                [
                    "contacts.delete",
                    "customers.delete",
                    "companies.delete",
                    "catalog.products.manage",
                    "crm.customer-management.contacts.manage",
                    "crm.customer-management.customers.manage",
                    "crm.customer-management.companies.manage"
                ]);
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var productCatalogConnection = new SqliteConnection("Data Source=:memory:");
            await productCatalogConnection.OpenAsync();

            var options = new DbContextOptionsBuilder<CustomerManagementDbContext>()
                .UseSqlite(connection)
                .AddInterceptors(new SoftDeleteSaveChangesInterceptor(currentUser))
                .Options;

            var dbContext = new CustomerManagementDbContext(options, new FixedTenantProvider(tenantId));
            await dbContext.Database.EnsureCreatedAsync();
            var productCatalogOptions = new DbContextOptionsBuilder<ProductCatalogDbContext>()
                .UseSqlite(productCatalogConnection)
                .Options;
            var productCatalogDbContext = new ProductCatalogDbContext(
                productCatalogOptions,
                currentUser,
                new TenantIsolationSaveChangesInterceptor(currentUser, currentUser, currentUser),
                new AuditSaveChangesInterceptor(currentUser),
                new SoftDeleteSaveChangesInterceptor(currentUser));
            await productCatalogDbContext.Database.EnsureCreatedAsync();
            var contacts = new Dictionary<Guid, Contact>();
            var repository = new Mock<IRepository<Contact, Guid>>();
            repository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, CancellationToken _) => contacts.TryGetValue(id, out var contact) ? contact : null);
            repository.Setup(x => x.Remove(It.IsAny<Contact>()))
                .Callback<Contact>(entity => contacts.Remove(entity.Id));
            var outbox = new Mock<ICustomerManagementOutbox>();
            var leadDbContext = new Mock<ILeadManagementDbContext>();
            var dealDbContext = new Mock<IDealManagementDbContext>();
            var opportunityDbContext = new Mock<IOpportunityManagementDbContext>();
            var quoteDbContext = new Mock<IQuoteManagementDbContext>();
            var ticketDbContext = new Mock<ITicketManagementDbContext>();
            var leadOutbox = new Mock<ILeadManagementOutbox>();
            var dealOutbox = new Mock<IDealManagementOutbox>();
            var opportunityOutbox = new Mock<IOpportunityManagementOutbox>();
            var quoteOutbox = new Mock<IQuoteManagementOutbox>();
            var ticketOutbox = new Mock<ITicketManagementOutbox>();
            outbox.Setup(x => x.EnqueueContactDeletedAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            outbox.Setup(x => x.EnqueueContactRestoredAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            outbox.Setup(x => x.EnqueueContactPurgedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            outbox.Setup(x => x.EnqueueCustomerRestoredAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            outbox.Setup(x => x.EnqueueCustomerPurgedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            outbox.Setup(x => x.EnqueueCompanyRestoredAsync(It.IsAny<Company>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            outbox.Setup(x => x.EnqueueCompanyPurgedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            leadOutbox.Setup(x => x.EnqueueLeadRestoredAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            dealOutbox.Setup(x => x.EnqueueDealRestoredAsync(It.IsAny<Deal>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            opportunityOutbox.Setup(x => x.EnqueueOpportunityRestoredAsync(It.IsAny<Opportunity>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            quoteOutbox.Setup(x => x.EnqueueQuoteRestoredAsync(It.IsAny<Quote>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            quoteOutbox.Setup(x => x.EnqueueQuotePurgedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            ticketOutbox.Setup(x => x.EnqueueTicketRestoredAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            ticketOutbox.Setup(x => x.EnqueueTicketPurgedAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var trashWriter = new GlobalTrashIndexWriter(dbContext, currentUser);
            var service = new ContactAdministrationService(
                dbContext,
                leadDbContext.Object,
                dealDbContext.Object,
                opportunityDbContext.Object,
                quoteDbContext.Object,
                ticketDbContext.Object,
                new ProductCatalogTrashRestoreService(productCatalogDbContext),
                new ProductCatalogTrashPurgeService(productCatalogDbContext),
                repository.Object,
                outbox.Object,
                leadOutbox.Object,
                dealOutbox.Object,
                opportunityOutbox.Object,
                quoteOutbox.Object,
                ticketOutbox.Object,
                trashWriter,
                currentUser);
            return new Fixture(connection, productCatalogConnection, dbContext, productCatalogDbContext, service, trashWriter, currentUser) { Contacts = contacts };
        }

        public Dictionary<Guid, Contact> Contacts { get; private init; } = [];

        public Contact CreateContact(string firstName, string lastName, string email)
        {
            var contact = new Contact
            {
                TenantId = CurrentUser.TenantId,
                FirstName = firstName,
                LastName = lastName,
                Email = email
            };
            Contacts[contact.Id] = contact;
            return contact;
        }

        public Contact CreatePersistedDeletedContact(string firstName, string lastName, string email)
        {
            var contact = new Contact
            {
                TenantId = CurrentUser.TenantId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow.AddMinutes(-10),
                DeletedBy = CurrentUser.UserId.ToString("D"),
                RowVersion = [1, 2, 3]
            };

            DbContext.Database.ExecuteSqlInterpolated($"""
                INSERT INTO Contacts
                    (Id, TenantId, FirstName, LastName, Email, MobilePhone, WorkPhone, PersonalPhone, OwnerUserId, Gender, Notes, IsPrimaryContact, IsActive, CreatedAt, IsDeleted, DeletedAt, DeletedBy, RowVersion)
                VALUES
                    ({contact.Id}, {contact.TenantId}, {contact.FirstName}, {contact.LastName}, {contact.Email}, {contact.MobilePhone}, {contact.WorkPhone}, {contact.PersonalPhone}, {contact.OwnerUserId}, {(int)contact.Gender}, {contact.Notes}, {contact.IsPrimaryContact}, {contact.IsActive}, {DateTime.UtcNow}, {true}, {contact.DeletedAt}, {contact.DeletedBy}, {contact.RowVersion})
                """);
            return contact;
        }

        public Contact CreatePersistedActiveContact(string firstName, string lastName, string email)
        {
            var contact = new Contact
            {
                TenantId = CurrentUser.TenantId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsDeleted = false,
                RowVersion = [4, 5, 6]
            };

            DbContext.Database.ExecuteSqlInterpolated($"""
                INSERT INTO Contacts
                    (Id, TenantId, FirstName, LastName, Email, MobilePhone, WorkPhone, PersonalPhone, OwnerUserId, Gender, Notes, IsPrimaryContact, IsActive, CreatedAt, IsDeleted, DeletedAt, DeletedBy, RowVersion)
                VALUES
                    ({contact.Id}, {contact.TenantId}, {contact.FirstName}, {contact.LastName}, {contact.Email}, {contact.MobilePhone}, {contact.WorkPhone}, {contact.PersonalPhone}, {contact.OwnerUserId}, {(int)contact.Gender}, {contact.Notes}, {contact.IsPrimaryContact}, {contact.IsActive}, {DateTime.UtcNow}, {false}, {null}, {null}, {contact.RowVersion})
                """);
            return contact;
        }

        public GlobalTrashItem CreateActiveTrashItemForContact(Guid contactId)
            => CreateActiveTrashItem(CrmTrashEntityTypes.Contact, contactId, "Deleted contact", "contacts");

        public GlobalTrashItem CreateActiveTrashItem(string entityType, Guid entityId, string displayName, string sourceModule)
            => new()
            {
                TenantId = CurrentUser.TenantId,
                EntityType = entityType,
                EntityId = entityId,
                DisplayName = displayName,
                SourceModule = sourceModule,
                OriginalRoute = $"/{sourceModule}/{entityId:D}",
                DeletedAtUtc = DateTime.UtcNow.AddMinutes(-10),
                DeletedByUserId = CurrentUser.UserId,
                DeletedByDisplayName = CurrentUser.UserName,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
                Status = CrmTrashStatuses.Active
            };

        public Customer CreatePersistedDeletedCustomer(string firstName, string lastName, string email)
        {
            var customer = new Customer
            {
                TenantId = CurrentUser.TenantId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow.AddMinutes(-10),
                DeletedBy = CurrentUser.UserId.ToString("D"),
                RowVersion = [7, 8, 9]
            };

            DbContext.Database.ExecuteSqlInterpolated($"""
                INSERT INTO Customers
                    (Id, TenantId, FirstName, LastName, Email, MobilePhone, WorkPhone, PersonalPhone, OwnerUserId, Gender, Notes, CustomerType, IsVip, IsActive, CreatedAt, IsDeleted, DeletedAt, DeletedBy, RowVersion)
                VALUES
                    ({customer.Id}, {customer.TenantId}, {customer.FirstName}, {customer.LastName}, {customer.Email}, {customer.MobilePhone}, {customer.WorkPhone}, {customer.PersonalPhone}, {customer.OwnerUserId}, {(int)customer.Gender}, {customer.Notes}, {(int)customer.CustomerType}, {customer.IsVip}, {customer.IsActive}, {DateTime.UtcNow}, {true}, {customer.DeletedAt}, {customer.DeletedBy}, {customer.RowVersion})
                """);
            return customer;
        }

        public Company CreatePersistedDeletedCompany(string name, string email)
        {
            var company = new Company
            {
                TenantId = CurrentUser.TenantId,
                Name = name,
                Email = email,
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow.AddMinutes(-10),
                DeletedBy = CurrentUser.UserId.ToString("D"),
                RowVersion = [10, 11, 12]
            };

            DbContext.Database.ExecuteSqlInterpolated($"""
                INSERT INTO Companies
                    (Id, TenantId, Name, Website, Email, Phone, Notes, CompanyType, IsActive, CreatedAt, IsDeleted, DeletedAt, DeletedBy, RowVersion)
                VALUES
                    ({company.Id}, {company.TenantId}, {company.Name}, {company.Website}, {company.Email}, {company.Phone}, {company.Notes}, {(int)company.CompanyType}, {company.IsActive}, {DateTime.UtcNow}, {true}, {company.DeletedAt}, {company.DeletedBy}, {company.RowVersion})
                """);
            return company;
        }

        public Customer CreatePersistedActiveCustomer(string firstName, string lastName, string email)
        {
            var customer = new Customer
            {
                TenantId = CurrentUser.TenantId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                IsDeleted = false,
                RowVersion = [13, 14, 15]
            };

            DbContext.Database.ExecuteSqlInterpolated($"""
                INSERT INTO Customers
                    (Id, TenantId, FirstName, LastName, Email, MobilePhone, WorkPhone, PersonalPhone, OwnerUserId, Gender, Notes, CustomerType, IsVip, IsActive, CreatedAt, IsDeleted, DeletedAt, DeletedBy, RowVersion)
                VALUES
                    ({customer.Id}, {customer.TenantId}, {customer.FirstName}, {customer.LastName}, {customer.Email}, {customer.MobilePhone}, {customer.WorkPhone}, {customer.PersonalPhone}, {customer.OwnerUserId}, {(int)customer.Gender}, {customer.Notes}, {(int)customer.CustomerType}, {customer.IsVip}, {customer.IsActive}, {DateTime.UtcNow}, {false}, {null}, {null}, {customer.RowVersion})
                """);
            return customer;
        }

        public Company CreatePersistedActiveCompany(string name, string email)
        {
            var company = new Company
            {
                TenantId = CurrentUser.TenantId,
                Name = name,
                Email = email,
                IsDeleted = false,
                RowVersion = [16, 17, 18]
            };

            DbContext.Database.ExecuteSqlInterpolated($"""
                INSERT INTO Companies
                    (Id, TenantId, Name, Website, Email, Phone, Notes, CompanyType, IsActive, CreatedAt, IsDeleted, DeletedAt, DeletedBy, RowVersion)
                VALUES
                    ({company.Id}, {company.TenantId}, {company.Name}, {company.Website}, {company.Email}, {company.Phone}, {company.Notes}, {(int)company.CompanyType}, {company.IsActive}, {DateTime.UtcNow}, {false}, {null}, {null}, {company.RowVersion})
                """);
            return company;
        }

        public CatalogCategory CreatePersistedActiveCatalogCategory(string code, string name)
        {
            var category = new CatalogCategory(code, name)
            {
                TenantId = CurrentUser.TenantId,
                IsDeleted = false,
            };

            ProductCatalogDbContext.Categories.Add(category);
            ProductCatalogDbContext.SaveChanges();
            return category;
        }

        public CatalogCategory CreatePersistedDeletedCatalogCategory(string code, string name)
        {
            var category = new CatalogCategory(code, name)
            {
                TenantId = CurrentUser.TenantId,
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow.AddMinutes(-10),
                DeletedBy = CurrentUser.UserId.ToString("D"),
            };

            ProductCatalogDbContext.Categories.Add(category);
            ProductCatalogDbContext.SaveChanges();
            return category;
        }

        public CatalogProduct CreatePersistedDeletedCatalogProduct(string code, string name, Guid? categoryId)
        {
            var product = new CatalogProduct(code, name, "desc", categoryId, 100, "USD", 0, 0)
            {
                TenantId = CurrentUser.TenantId,
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow.AddMinutes(-10),
                DeletedBy = CurrentUser.UserId.ToString("D"),
            };

            ProductCatalogDbContext.Products.Add(product);
            ProductCatalogDbContext.SaveChanges();
            return product;
        }

        public CatalogProduct CreatePersistedActiveCatalogProduct(string code, string name, Guid? categoryId)
        {
            var product = new CatalogProduct(code, name, "desc", categoryId, 100, "USD", 0, 0)
            {
                TenantId = CurrentUser.TenantId,
                IsDeleted = false,
            };

            ProductCatalogDbContext.Products.Add(product);
            ProductCatalogDbContext.SaveChanges();
            return product;
        }

        public async ValueTask DisposeAsync()
        {
            await ProductCatalogDbContext.DisposeAsync();
            await DbContext.DisposeAsync();
            await _productCatalogConnection.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private sealed class FixedTenantProvider(Guid tenantId) : ITenantProvider
    {
        public Guid? TenantId { get; } = tenantId;
    }

    private sealed class FakeCurrentUserService(
        Guid userId,
        Guid tenantId,
        string userName,
        string email,
        IReadOnlyCollection<string> permissions) : ICurrentUserService, ITenantContext, ITenantProvider
    {
        public Guid UserId { get; } = userId;
        public Guid TenantId { get; } = tenantId;
        public bool IsAuthenticated => true;
        public string? UserName { get; } = userName;
        public string? Email { get; } = email;
        Guid? ITenantContext.TenantId => TenantId;
        Guid? ITenantProvider.TenantId => TenantId;
        public IReadOnlyCollection<string> Roles => [];
        public IReadOnlyCollection<string> Permissions { get; } = permissions;
        public bool IsInRole(string role) => false;
        public bool HasPermission(string permission)
            => Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
}

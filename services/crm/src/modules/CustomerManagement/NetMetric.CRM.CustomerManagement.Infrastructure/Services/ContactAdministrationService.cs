// <copyright file="ContactAdministrationService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Integration;
using NetMetric.CRM.CustomerManagement.Application.Commands.Contacts;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.CRM.DealManagement.Application.Abstractions.Integration;
using NetMetric.CRM.DealManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Infrastructure.Persistence;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Integration;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Integration;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Integration;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.Sales;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Integration;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.Support;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;
using NetMetric.Repository;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Services;

public sealed class ContactAdministrationService(
    CustomerManagementDbContext dbContext,
    ILeadManagementDbContext leadManagementDbContext,
    IDealManagementDbContext dealManagementDbContext,
    IOpportunityManagementDbContext opportunityManagementDbContext,
    IQuoteManagementDbContext quoteManagementDbContext,
    ITicketManagementDbContext ticketManagementDbContext,
    IGlobalTrashProductCatalogRestoreService productCatalogRestoreService,
    IGlobalTrashProductCatalogPurgeService productCatalogPurgeService,
    IRepository<Contact, Guid> contactRepository,
    ICustomerManagementOutbox outbox,
    ILeadManagementOutbox leadOutbox,
    IDealManagementOutbox dealOutbox,
    IOpportunityManagementOutbox opportunityOutbox,
    IQuoteManagementOutbox quoteOutbox,
    ITicketManagementOutbox ticketOutbox,
    IGlobalTrashIndexWriter trashIndexWriter,
    ICurrentUserService currentUserService)
    : IContactAdministrationService
{
    private readonly CustomerManagementDbContext _dbContext = dbContext;
    private readonly ILeadManagementDbContext _leadManagementDbContext = leadManagementDbContext;
    private readonly IDealManagementDbContext _dealManagementDbContext = dealManagementDbContext;
    private readonly IOpportunityManagementDbContext _opportunityManagementDbContext = opportunityManagementDbContext;
    private readonly IQuoteManagementDbContext _quoteManagementDbContext = quoteManagementDbContext;
    private readonly ITicketManagementDbContext _ticketManagementDbContext = ticketManagementDbContext;
    private readonly IGlobalTrashProductCatalogRestoreService _productCatalogRestoreService = productCatalogRestoreService;
    private readonly IGlobalTrashProductCatalogPurgeService _productCatalogPurgeService = productCatalogPurgeService;
    private readonly IRepository<Contact, Guid> _contactRepository = contactRepository;
    private readonly ICustomerManagementOutbox _outbox = outbox;
    private readonly ILeadManagementOutbox _leadOutbox = leadOutbox;
    private readonly IDealManagementOutbox _dealOutbox = dealOutbox;
    private readonly IOpportunityManagementOutbox _opportunityOutbox = opportunityOutbox;
    private readonly IQuoteManagementOutbox _quoteOutbox = quoteOutbox;
    private readonly ITicketManagementOutbox _ticketOutbox = ticketOutbox;
    private readonly IGlobalTrashIndexWriter _trashIndexWriter = trashIndexWriter;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<ContactDetailDto> CreateAsync(CreateContactCommand request, CancellationToken cancellationToken = default)
    {
        var entity = new Contact
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Title = TrimToNull(request.Title),
            Email = TrimToNull(request.Email),
            MobilePhone = TrimToNull(request.MobilePhone),
            WorkPhone = TrimToNull(request.WorkPhone),
            PersonalPhone = TrimToNull(request.PersonalPhone),
            BirthDate = request.BirthDate,
            Gender = request.Gender,
            Department = TrimToNull(request.Department),
            JobTitle = TrimToNull(request.JobTitle),
            Description = TrimToNull(request.Description),
            OwnerUserId = request.OwnerUserId,
            CompanyId = request.CompanyId,
            CustomerId = request.CustomerId,
            IsPrimaryContact = request.IsPrimaryContact
        };

        entity.SetNotes(request.Notes);

        await EnsurePrimaryContactConsistencyAsync(entity, null, cancellationToken);
        await _contactRepository.AddAsync(entity, cancellationToken);
        await _outbox.EnqueueContactCreatedAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        entity = await LoadContactAsync(entity.Id, cancellationToken);
        return entity.ToDetailDto();
    }

    public async Task<ContactDetailDto> UpdateAsync(UpdateContactCommand request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Contacts
            .FirstOrDefaultAsync(x => x.Id == request.ContactId, cancellationToken)
            ?? throw new NotFoundAppException("Contact not found.");

        ConcurrencyHelper.ApplyRowVersion(_dbContext, entity, request.RowVersion);

        entity.FirstName = request.FirstName.Trim();
        entity.LastName = request.LastName.Trim();
        entity.Title = TrimToNull(request.Title);
        entity.Email = TrimToNull(request.Email);
        entity.MobilePhone = TrimToNull(request.MobilePhone);
        entity.WorkPhone = TrimToNull(request.WorkPhone);
        entity.PersonalPhone = TrimToNull(request.PersonalPhone);
        entity.BirthDate = request.BirthDate;
        entity.Gender = request.Gender;
        entity.Department = TrimToNull(request.Department);
        entity.JobTitle = TrimToNull(request.JobTitle);
        entity.Description = TrimToNull(request.Description);
        entity.OwnerUserId = request.OwnerUserId;
        entity.CompanyId = request.CompanyId;
        entity.CustomerId = request.CustomerId;
        entity.IsPrimaryContact = request.IsPrimaryContact;
        entity.SetNotes(request.Notes);

        await EnsurePrimaryContactConsistencyAsync(entity, entity.Id, cancellationToken);
        await _outbox.EnqueueContactUpdatedAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        entity = await LoadContactAsync(entity.Id, cancellationToken);
        return entity.ToDetailDto();
    }

    public async Task SetPrimaryAsync(Guid contactId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Contacts.FirstOrDefaultAsync(x => x.Id == contactId, cancellationToken)
            ?? throw new NotFoundAppException("Contact not found.");

        entity.IsPrimaryContact = true;
        await EnsurePrimaryContactConsistencyAsync(entity, entity.Id, cancellationToken);
        await _outbox.EnqueuePrimaryContactChangedAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(Guid contactId, CancellationToken cancellationToken = default)
    {
        var entity = await _contactRepository.GetByIdAsync(contactId, cancellationToken)
            ?? throw new NotFoundAppException("Contact not found.");

        await _trashIndexWriter.AddContactDeletionAsync(entity, cancellationToken);
        _contactRepository.Remove(entity);
        await _outbox.EnqueueContactDeletedAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreFromTrashAsync(Guid trashItemId, CancellationToken cancellationToken = default)
    {
        var trashItem = await _dbContext.GlobalTrashItems
            .FirstOrDefaultAsync(x => x.Id == trashItemId, cancellationToken)
            ?? throw new NotFoundAppException("Trash item not found.");

        if (trashItem.Status != CrmTrashStatuses.Active)
        {
            throw new ConflictAppException("Trash item is not active.");
        }

        EnsureRestoreCapability(trashItem.EntityType);

        if (trashItem.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new ConflictAppException("Trash item has expired and cannot be restored.");
        }

        switch (trashItem.EntityType)
        {
            case CrmTrashEntityTypes.Contact:
            {
                var contact = await _dbContext.Contacts
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(
                        x => x.Id == trashItem.EntityId && x.TenantId == trashItem.TenantId,
                        cancellationToken)
                    ?? throw new NotFoundAppException("Contact not found.");

                if (!contact.IsDeleted)
                {
                    throw new ConflictAppException("Contact is already active.");
                }

                contact.IsDeleted = false;
                contact.DeletedAt = null;
                contact.DeletedBy = null;

                await _outbox.EnqueueContactRestoredAsync(contact, cancellationToken);
                break;
            }
            case CrmTrashEntityTypes.Customer:
            {
                var customer = await _dbContext.Customers
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(
                        x => x.Id == trashItem.EntityId && x.TenantId == trashItem.TenantId,
                        cancellationToken)
                    ?? throw new NotFoundAppException("Customer not found.");

                if (!customer.IsDeleted)
                {
                    throw new ConflictAppException("Customer is already active.");
                }

                customer.IsDeleted = false;
                customer.DeletedAt = null;
                customer.DeletedBy = null;

                await _outbox.EnqueueCustomerRestoredAsync(customer, cancellationToken);
                break;
            }
            case CrmTrashEntityTypes.Company:
            {
                var company = await _dbContext.Companies
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(
                        x => x.Id == trashItem.EntityId && x.TenantId == trashItem.TenantId,
                        cancellationToken)
                    ?? throw new NotFoundAppException("Company not found.");

                if (!company.IsDeleted)
                {
                    throw new ConflictAppException("Company is already active.");
                }

                company.IsDeleted = false;
                company.DeletedAt = null;
                company.DeletedBy = null;

                await _outbox.EnqueueCompanyRestoredAsync(company, cancellationToken);
                break;
            }
            case CrmTrashEntityTypes.Lead:
            {
                var lead = await _leadManagementDbContext.Leads
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(
                        x => x.Id == trashItem.EntityId && x.TenantId == trashItem.TenantId,
                        cancellationToken)
                    ?? throw new NotFoundAppException("Lead not found.");

                if (!lead.IsDeleted)
                {
                    throw new ConflictAppException("Lead is already active.");
                }

                await EnsureLeadRestoreDependenciesAsync(lead, cancellationToken);
                lead.IsDeleted = false;
                lead.DeletedAt = null;
                lead.DeletedBy = null;
                await _leadOutbox.EnqueueLeadRestoredAsync(lead, cancellationToken);
                await _leadManagementDbContext.SaveChangesAsync(cancellationToken);
                break;
            }
            case CrmTrashEntityTypes.Deal:
            {
                var deal = await _dealManagementDbContext.Deals
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(
                        x => x.Id == trashItem.EntityId && x.TenantId == trashItem.TenantId,
                        cancellationToken)
                    ?? throw new NotFoundAppException("Deal not found.");

                if (!deal.IsDeleted)
                {
                    throw new ConflictAppException("Deal is already active.");
                }

                await EnsureDealRestoreDependenciesAsync(deal, cancellationToken);
                deal.IsDeleted = false;
                deal.DeletedAt = null;
                deal.DeletedBy = null;
                await _dealOutbox.EnqueueDealRestoredAsync(deal, cancellationToken);
                await _dealManagementDbContext.SaveChangesAsync(cancellationToken);
                break;
            }
            case CrmTrashEntityTypes.Opportunity:
            {
                var opportunity = await _opportunityManagementDbContext.Opportunities
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(
                        x => x.Id == trashItem.EntityId && x.TenantId == trashItem.TenantId,
                        cancellationToken)
                    ?? throw new NotFoundAppException("Opportunity not found.");

                if (!opportunity.IsDeleted)
                {
                    throw new ConflictAppException("Opportunity is already active.");
                }

                await EnsureOpportunityRestoreDependenciesAsync(opportunity, cancellationToken);
                opportunity.IsDeleted = false;
                opportunity.DeletedAt = null;
                opportunity.DeletedBy = null;
                await _opportunityOutbox.EnqueueOpportunityRestoredAsync(opportunity, cancellationToken);
                await _opportunityManagementDbContext.SaveChangesAsync(cancellationToken);
                break;
            }
            case CrmTrashEntityTypes.Quote:
            {
                var quote = await _quoteManagementDbContext.Quotes
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(
                        x => x.Id == trashItem.EntityId && x.TenantId == trashItem.TenantId,
                        cancellationToken)
                    ?? throw new NotFoundAppException("Quote not found.");

                if (!quote.IsDeleted)
                {
                    throw new ConflictAppException("Quote is already active.");
                }

                await EnsureQuoteRestoreDependenciesAsync(quote, cancellationToken);
                quote.IsDeleted = false;
                quote.DeletedAt = null;
                quote.DeletedBy = null;
                await _quoteOutbox.EnqueueQuoteRestoredAsync(quote, cancellationToken);
                await _quoteManagementDbContext.SaveChangesAsync(cancellationToken);
                break;
            }
            case CrmTrashEntityTypes.Ticket:
            {
                var ticket = await _ticketManagementDbContext.Tickets
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(
                        x => x.Id == trashItem.EntityId && x.TenantId == trashItem.TenantId,
                        cancellationToken)
                    ?? throw new NotFoundAppException("Ticket not found.");

                if (!ticket.IsDeleted)
                {
                    throw new ConflictAppException("Ticket is already active.");
                }

                await EnsureTicketRestoreDependenciesAsync(ticket, cancellationToken);
                ticket.IsDeleted = false;
                ticket.DeletedAt = null;
                ticket.DeletedBy = null;
                await _ticketOutbox.EnqueueTicketRestoredAsync(ticket, cancellationToken);
                await _ticketManagementDbContext.SaveChangesAsync(cancellationToken);
                break;
            }
            case CrmTrashEntityTypes.ProductCatalogItem:
            {
                await _productCatalogRestoreService.RestoreCatalogProductFromTrashAsync(trashItem, cancellationToken);
                break;
            }
            default:
                throw new BadRequestAppException("Only contact, customer, company, lead, deal, opportunity, quote, ticket, and catalog product trash items can be restored in this phase.");
        }

        trashItem.Status = CrmTrashStatuses.Restored;
        trashItem.RestoredAtUtc = DateTime.UtcNow;
        trashItem.RestoredByUserId = _currentUserService.UserId == Guid.Empty ? null : _currentUserService.UserId;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> PurgeExpiredTrashItemsAsync(int batchSize = 100, CancellationToken cancellationToken = default)
    {
        var tenantId = _currentUserService.TenantId;
        if (tenantId == Guid.Empty)
        {
            return 0;
        }

        return await PurgeExpiredTrashItemsForTenantAsync(tenantId, batchSize, cancellationToken);
    }

    public async Task<int> PurgeExpiredTrashItemsForTenantAsync(Guid tenantId, int batchSize = 100, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
        {
            return 0;
        }

        var nowUtc = DateTime.UtcNow;
        var normalizedBatchSize = batchSize < 1 ? 100 : batchSize > 500 ? 500 : batchSize;

        var expiredItems = await _dbContext.GlobalTrashItems
            .IgnoreQueryFilters()
            .Where(x =>
                x.TenantId == tenantId
                && x.Status == CrmTrashStatuses.Active
                && (x.EntityType == CrmTrashEntityTypes.Contact
                    || x.EntityType == CrmTrashEntityTypes.Customer
                    || x.EntityType == CrmTrashEntityTypes.Company
                    || x.EntityType == CrmTrashEntityTypes.Lead
                    || x.EntityType == CrmTrashEntityTypes.Deal
                    || x.EntityType == CrmTrashEntityTypes.Opportunity
                    || x.EntityType == CrmTrashEntityTypes.Quote
                    || x.EntityType == CrmTrashEntityTypes.Ticket
                    || x.EntityType == CrmTrashEntityTypes.ProductCatalogItem)
                && x.ExpiresAtUtc <= nowUtc)
            .OrderBy(x => x.ExpiresAtUtc)
            .Take(normalizedBatchSize)
            .ToListAsync(cancellationToken);

        var purgedCount = 0;
        foreach (var trashItem in expiredItems)
        {
            switch (trashItem.EntityType)
            {
                case CrmTrashEntityTypes.Contact:
                    purgedCount += await TryPurgeContactAsync(trashItem, nowUtc, cancellationToken);
                    break;
                case CrmTrashEntityTypes.Customer:
                    purgedCount += await TryPurgeCustomerAsync(trashItem, nowUtc, cancellationToken);
                    break;
                case CrmTrashEntityTypes.Company:
                    purgedCount += await TryPurgeCompanyAsync(trashItem, nowUtc, cancellationToken);
                    break;
                case CrmTrashEntityTypes.Lead:
                    purgedCount += await TryPurgeLeadAsync(trashItem, nowUtc, cancellationToken);
                    break;
                case CrmTrashEntityTypes.Deal:
                    purgedCount += await TryPurgeDealAsync(trashItem, nowUtc, cancellationToken);
                    break;
                case CrmTrashEntityTypes.Opportunity:
                    purgedCount += await TryPurgeOpportunityAsync(trashItem, nowUtc, cancellationToken);
                    break;
                case CrmTrashEntityTypes.Quote:
                    purgedCount += await TryPurgeQuoteAsync(trashItem, nowUtc, cancellationToken);
                    break;
                case CrmTrashEntityTypes.Ticket:
                    purgedCount += await TryPurgeTicketAsync(trashItem, nowUtc, cancellationToken);
                    break;
                case CrmTrashEntityTypes.ProductCatalogItem:
                    purgedCount += await _productCatalogPurgeService.PurgeCatalogProductFromTrashAsync(trashItem, nowUtc, cancellationToken);
                    break;
            }
        }

        if (purgedCount > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return purgedCount;
    }

    private async Task EnsurePrimaryContactConsistencyAsync(Contact target, Guid? selfId, CancellationToken cancellationToken)
    {
        if (!target.IsPrimaryContact)
            return;

        if (target.CompanyId.HasValue)
        {
            var companyContacts = await _dbContext.Contacts
                .Where(x => x.CompanyId == target.CompanyId && (!selfId.HasValue || x.Id != selfId.Value))
                .ToListAsync(cancellationToken);

            companyContacts.ForEach(x => x.IsPrimaryContact = false);
        }

        if (target.CustomerId.HasValue)
        {
            var customerContacts = await _dbContext.Contacts
                .Where(x => x.CustomerId == target.CustomerId && (!selfId.HasValue || x.Id != selfId.Value))
                .ToListAsync(cancellationToken);

            customerContacts.ForEach(x => x.IsPrimaryContact = false);
        }
    }

    private Task<Contact> LoadContactAsync(Guid contactId, CancellationToken cancellationToken)
        => _dbContext.Contacts
            .Include(x => x.Company)
            .Include(x => x.Customer)
            .FirstAsync(x => x.Id == contactId, cancellationToken);

    private static void MarkTrashItemAsPurged(GlobalTrashItem trashItem, DateTime nowUtc)
    {
        trashItem.Status = CrmTrashStatuses.Purged;
        trashItem.PurgedAtUtc = nowUtc;
    }

    private async Task<int> TryPurgeContactAsync(GlobalTrashItem trashItem, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var contact = await _dbContext.Contacts
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == trashItem.EntityId && x.TenantId == trashItem.TenantId,
                cancellationToken);

        if (contact is null)
        {
            MarkTrashItemAsPurged(trashItem, nowUtc);
            return 1;
        }

        if (!contact.IsDeleted)
        {
            return 0;
        }

        var deletedRows = await _dbContext.Contacts
            .IgnoreQueryFilters()
            .Where(x =>
                x.Id == contact.Id
                && x.TenantId == contact.TenantId
                && x.IsDeleted)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedRows < 1)
        {
            return 0;
        }

        await _outbox.EnqueueContactPurgedAsync(
            contact.TenantId,
            contact.Id,
            contact.FullName,
            contact.OwnerUserId,
            cancellationToken);

        MarkTrashItemAsPurged(trashItem, nowUtc);
        return 1;
    }

    private async Task<int> TryPurgeCustomerAsync(GlobalTrashItem trashItem, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == trashItem.EntityId && x.TenantId == trashItem.TenantId,
                cancellationToken);

        if (customer is null)
        {
            MarkTrashItemAsPurged(trashItem, nowUtc);
            return 1;
        }

        if (!customer.IsDeleted)
        {
            return 0;
        }

        try
        {
            var deletedRows = await _dbContext.Customers
                .IgnoreQueryFilters()
                .Where(x =>
                    x.Id == customer.Id
                    && x.TenantId == customer.TenantId
                    && x.IsDeleted)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedRows < 1)
            {
                return 0;
            }
        }
        catch (DbUpdateException)
        {
            return 0;
        }

        await _outbox.EnqueueCustomerPurgedAsync(
            customer.TenantId,
            customer.Id,
            customer.FullName,
            customer.OwnerUserId,
            cancellationToken);

        MarkTrashItemAsPurged(trashItem, nowUtc);
        return 1;
    }

    private async Task<int> TryPurgeCompanyAsync(GlobalTrashItem trashItem, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var company = await _dbContext.Companies
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == trashItem.EntityId && x.TenantId == trashItem.TenantId,
                cancellationToken);

        if (company is null)
        {
            MarkTrashItemAsPurged(trashItem, nowUtc);
            return 1;
        }

        if (!company.IsDeleted)
        {
            return 0;
        }

        try
        {
            var deletedRows = await _dbContext.Companies
                .IgnoreQueryFilters()
                .Where(x =>
                    x.Id == company.Id
                    && x.TenantId == company.TenantId
                    && x.IsDeleted)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedRows < 1)
            {
                return 0;
            }
        }
        catch (DbUpdateException)
        {
            return 0;
        }

        await _outbox.EnqueueCompanyPurgedAsync(
            company.TenantId,
            company.Id,
            company.Name,
            company.OwnerUserId,
            cancellationToken);

        MarkTrashItemAsPurged(trashItem, nowUtc);
        return 1;
    }

    private async Task<int> TryPurgeLeadAsync(GlobalTrashItem trashItem, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var lead = await _leadManagementDbContext.Leads
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == trashItem.EntityId && x.TenantId == trashItem.TenantId,
                cancellationToken);

        if (lead is null)
        {
            MarkTrashItemAsPurged(trashItem, nowUtc);
            return 1;
        }

        if (!lead.IsDeleted)
        {
            return 0;
        }

        try
        {
            var deletedRows = await _leadManagementDbContext.Leads
                .IgnoreQueryFilters()
                .Where(x =>
                    x.Id == lead.Id
                    && x.TenantId == lead.TenantId
                    && x.IsDeleted)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedRows < 1)
            {
                return 0;
            }
        }
        catch (DbUpdateException)
        {
            return 0;
        }

        await _leadOutbox.EnqueueLeadPurgedAsync(
            lead.TenantId,
            lead.Id,
            lead.FullName,
            lead.OwnerUserId,
            cancellationToken);

        MarkTrashItemAsPurged(trashItem, nowUtc);
        return 1;
    }

    private async Task<int> TryPurgeDealAsync(GlobalTrashItem trashItem, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var deal = await _dealManagementDbContext.Deals
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == trashItem.EntityId && x.TenantId == trashItem.TenantId,
                cancellationToken);

        if (deal is null)
        {
            MarkTrashItemAsPurged(trashItem, nowUtc);
            return 1;
        }

        if (!deal.IsDeleted)
        {
            return 0;
        }

        try
        {
            var deletedRows = await _dealManagementDbContext.Deals
                .IgnoreQueryFilters()
                .Where(x =>
                    x.Id == deal.Id
                    && x.TenantId == deal.TenantId
                    && x.IsDeleted)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedRows < 1)
            {
                return 0;
            }
        }
        catch (DbUpdateException)
        {
            return 0;
        }

        await _dealOutbox.EnqueueDealPurgedAsync(
            deal.TenantId,
            deal.Id,
            deal.Name,
            deal.OwnerUserId,
            cancellationToken);

        MarkTrashItemAsPurged(trashItem, nowUtc);
        return 1;
    }

    private async Task<int> TryPurgeOpportunityAsync(GlobalTrashItem trashItem, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var opportunity = await _opportunityManagementDbContext.Opportunities
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == trashItem.EntityId && x.TenantId == trashItem.TenantId,
                cancellationToken);

        if (opportunity is null)
        {
            MarkTrashItemAsPurged(trashItem, nowUtc);
            return 1;
        }

        if (!opportunity.IsDeleted)
        {
            return 0;
        }

        try
        {
            var deletedRows = await _opportunityManagementDbContext.Opportunities
                .IgnoreQueryFilters()
                .Where(x =>
                    x.Id == opportunity.Id
                    && x.TenantId == opportunity.TenantId
                    && x.IsDeleted)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedRows < 1)
            {
                return 0;
            }
        }
        catch (DbUpdateException)
        {
            return 0;
        }

        await _opportunityOutbox.EnqueueOpportunityPurgedAsync(
            opportunity.TenantId,
            opportunity.Id,
            opportunity.Name,
            opportunity.OwnerUserId,
            cancellationToken);

        MarkTrashItemAsPurged(trashItem, nowUtc);
        return 1;
    }

    private async Task<int> TryPurgeQuoteAsync(GlobalTrashItem trashItem, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var quote = await _quoteManagementDbContext.Quotes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == trashItem.EntityId && x.TenantId == trashItem.TenantId,
                cancellationToken);

        if (quote is null)
        {
            MarkTrashItemAsPurged(trashItem, nowUtc);
            return 1;
        }

        if (!quote.IsDeleted)
        {
            return 0;
        }

        try
        {
            var deletedRows = await _quoteManagementDbContext.Quotes
                .IgnoreQueryFilters()
                .Where(x =>
                    x.Id == quote.Id
                    && x.TenantId == quote.TenantId
                    && x.IsDeleted)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedRows < 1)
            {
                return 0;
            }
        }
        catch (DbUpdateException)
        {
            return 0;
        }

        await _quoteOutbox.EnqueueQuotePurgedAsync(
            quote.TenantId,
            quote.Id,
            quote.QuoteNumber,
            quote.OwnerUserId,
            cancellationToken);

        MarkTrashItemAsPurged(trashItem, nowUtc);
        return 1;
    }

    private async Task<int> TryPurgeTicketAsync(GlobalTrashItem trashItem, DateTime nowUtc, CancellationToken cancellationToken)
    {
        var ticket = await _ticketManagementDbContext.Tickets
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == trashItem.EntityId && x.TenantId == trashItem.TenantId,
                cancellationToken);

        if (ticket is null)
        {
            MarkTrashItemAsPurged(trashItem, nowUtc);
            return 1;
        }

        if (!ticket.IsDeleted)
        {
            return 0;
        }

        try
        {
            var deletedRows = await _ticketManagementDbContext.Tickets
                .IgnoreQueryFilters()
                .Where(x =>
                    x.Id == ticket.Id
                    && x.TenantId == ticket.TenantId
                    && x.IsDeleted)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedRows < 1)
            {
                return 0;
            }
        }
        catch (DbUpdateException)
        {
            return 0;
        }

        await _ticketOutbox.EnqueueTicketPurgedAsync(
            ticket.TenantId,
            ticket.Id,
            ticket.Subject,
            ticket.AssignedUserId,
            cancellationToken);

        MarkTrashItemAsPurged(trashItem, nowUtc);
        return 1;
    }

    private static string? TrimToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task EnsureLeadRestoreDependenciesAsync(Lead lead, CancellationToken cancellationToken)
    {
        if (lead.ConvertedCustomerId.HasValue)
        {
            var convertedCustomer = await _dbContext.Customers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.Id == lead.ConvertedCustomerId.Value && x.TenantId == lead.TenantId,
                    cancellationToken);
            if (convertedCustomer is null || convertedCustomer.IsDeleted)
            {
                throw new ConflictAppException("Lead cannot be restored because its converted customer is unavailable.");
            }
        }
    }

    private async Task EnsureDealRestoreDependenciesAsync(Deal deal, CancellationToken cancellationToken)
    {
        if (deal.CustomerId.HasValue)
        {
            var customer = await _dbContext.Customers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.Id == deal.CustomerId.Value && x.TenantId == deal.TenantId,
                    cancellationToken);
            if (customer is null || customer.IsDeleted)
            {
                throw new ConflictAppException("Deal cannot be restored because its customer dependency is unavailable.");
            }
        }

        if (deal.CompanyId.HasValue)
        {
            var company = await _dbContext.Companies
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.Id == deal.CompanyId.Value && x.TenantId == deal.TenantId,
                    cancellationToken);
            if (company is null || company.IsDeleted)
            {
                throw new ConflictAppException("Deal cannot be restored because its company dependency is unavailable.");
            }
        }

        if (deal.OpportunityId.HasValue)
        {
            var opportunity = await _opportunityManagementDbContext.Opportunities
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.Id == deal.OpportunityId.Value && x.TenantId == deal.TenantId,
                    cancellationToken);
            if (opportunity is null || opportunity.IsDeleted)
            {
                throw new ConflictAppException("Deal cannot be restored because its opportunity dependency is unavailable.");
            }
        }
    }

    private async Task EnsureOpportunityRestoreDependenciesAsync(Opportunity opportunity, CancellationToken cancellationToken)
    {
        if (opportunity.CustomerId.HasValue)
        {
            var customer = await _dbContext.Customers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.Id == opportunity.CustomerId.Value && x.TenantId == opportunity.TenantId,
                    cancellationToken);
            if (customer is null || customer.IsDeleted)
            {
                throw new ConflictAppException("Opportunity cannot be restored because its customer dependency is unavailable.");
            }
        }

        if (opportunity.CompanyId.HasValue)
        {
            var company = await _dbContext.Companies
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.Id == opportunity.CompanyId.Value && x.TenantId == opportunity.TenantId,
                    cancellationToken);
            if (company is null || company.IsDeleted)
            {
                throw new ConflictAppException("Opportunity cannot be restored because its company dependency is unavailable.");
            }
        }

        if (opportunity.LeadId.HasValue)
        {
            var lead = await _leadManagementDbContext.Leads
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.Id == opportunity.LeadId.Value && x.TenantId == opportunity.TenantId,
                    cancellationToken);
            if (lead is null || lead.IsDeleted)
            {
                throw new ConflictAppException("Opportunity cannot be restored because its lead dependency is unavailable.");
            }
        }
    }

    private async Task EnsureQuoteRestoreDependenciesAsync(Quote quote, CancellationToken cancellationToken)
    {
        if (quote.CustomerId.HasValue)
        {
            var customer = await _dbContext.Customers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.Id == quote.CustomerId.Value && x.TenantId == quote.TenantId,
                    cancellationToken);
            if (customer is null || customer.IsDeleted)
            {
                throw new ConflictAppException("Quote cannot be restored because its customer dependency is unavailable.");
            }
        }

        if (quote.OpportunityId.HasValue)
        {
            var opportunity = await _opportunityManagementDbContext.Opportunities
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.Id == quote.OpportunityId.Value && x.TenantId == quote.TenantId,
                    cancellationToken);
            if (opportunity is null || opportunity.IsDeleted)
            {
                throw new ConflictAppException("Quote cannot be restored because its opportunity dependency is unavailable.");
            }
        }
    }

    private async Task EnsureTicketRestoreDependenciesAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        if (ticket.CustomerId.HasValue)
        {
            var customer = await _dbContext.Customers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.Id == ticket.CustomerId.Value && x.TenantId == ticket.TenantId,
                    cancellationToken);
            if (customer is null || customer.IsDeleted)
            {
                throw new ConflictAppException("Ticket cannot be restored because its customer dependency is unavailable.");
            }
        }

        if (ticket.ContactId.HasValue)
        {
            var contact = await _dbContext.Contacts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.Id == ticket.ContactId.Value && x.TenantId == ticket.TenantId,
                    cancellationToken);
            if (contact is null || contact.IsDeleted)
            {
                throw new ConflictAppException("Ticket cannot be restored because its contact dependency is unavailable.");
            }
        }
    }

    private void EnsureRestoreCapability(string entityType)
    {
        var allowed = entityType switch
        {
            CrmTrashEntityTypes.Contact => HasAnyPermission(
                "contacts.delete",
                "contacts.manage",
                "crm.customer-management.contacts.manage"),
            CrmTrashEntityTypes.Customer => HasAnyPermission(
                "customers.delete",
                "customers.manage",
                "crm.customer-management.customers.manage"),
            CrmTrashEntityTypes.Company => HasAnyPermission(
                "companies.delete",
                "companies.manage",
                "crm.customer-management.companies.manage"),
            CrmTrashEntityTypes.Lead => HasAnyPermission(
                "leads.delete",
                "leads.manage"),
            CrmTrashEntityTypes.Deal => HasAnyPermission(
                "deals.delete",
                "deals.manage"),
            CrmTrashEntityTypes.Opportunity => HasAnyPermission(
                "opportunities.delete",
                "opportunities.manage"),
            CrmTrashEntityTypes.Quote => HasAnyPermission(
                "quotes.delete",
                "quotes.manage"),
            CrmTrashEntityTypes.Ticket => HasAnyPermission(
                "tickets.delete",
                "tickets.manage"),
            CrmTrashEntityTypes.ProductCatalogItem => HasAnyPermission(
                "catalog.products.manage",
                "productCatalog.manage"),
            _ => true
        };

        if (!allowed)
        {
            throw new ForbiddenAppException("You do not have permission to restore this trash item.");
        }
    }

    private bool HasAnyPermission(params string[] permissions)
        => permissions.Any(_currentUserService.HasPermission);
}

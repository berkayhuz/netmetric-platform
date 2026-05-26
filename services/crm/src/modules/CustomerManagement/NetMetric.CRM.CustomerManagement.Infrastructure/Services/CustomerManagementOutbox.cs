// <copyright file="CustomerManagementOutbox.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics;
using System.Text.Json;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Integration;
using NetMetric.CRM.CustomerManagement.Application.IntegrationEvents;
using NetMetric.CRM.CustomerManagement.Domain.Outbox;
using NetMetric.CRM.CustomerManagement.Infrastructure.Persistence;
using NetMetric.CurrentUser;
using NetMetric.Notification.Contracts.IntegrationEvents.V1;
using NetMetric.Notification.Contracts.Notifications.Enums;
using NetMetric.Notification.Contracts.Notifications.Models;
using NetMetric.Search.Contracts.IntegrationEvents.V1;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Services;

public sealed class CustomerManagementOutbox(
    CustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService) : ICustomerManagementOutbox
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task EnqueueCustomerCreatedAsync(Customer customer, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId(customer.TenantId);

        await EnqueueLifecycleAndNotificationAsync(
            tenantId,
            customer.Id,
            "customer",
            CustomerManagementIntegrationEventNames.CustomerCreated,
            customer.OwnerUserId,
            customer.FullName,
            "Customer created",
            $"Customer {customer.FullName} was created.",
            cancellationToken);

        await EnqueueCustomerSearchIndexAsync(tenantId, customer, cancellationToken);
    }

    public async Task EnqueueCustomerUpdatedAsync(Customer customer, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId(customer.TenantId);

        await EnqueueLifecycleAsync(
            tenantId,
            customer.Id,
            "customer",
            CustomerManagementIntegrationEventNames.CustomerUpdated,
            customer.OwnerUserId,
            new Dictionary<string, string> { ["customerName"] = customer.FullName },
            cancellationToken);

        await EnqueueCustomerSearchIndexAsync(tenantId, customer, cancellationToken);
    }

    public async Task EnqueueCustomerDeletedAsync(Customer customer, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId(customer.TenantId);

        await EnqueueLifecycleAsync(
            tenantId,
            customer.Id,
            "customer",
            CustomerManagementIntegrationEventNames.CustomerDeleted,
            customer.OwnerUserId,
            new Dictionary<string, string> { ["customerName"] = customer.FullName },
            cancellationToken);

        await EnqueueCustomerSearchDeleteAsync(tenantId, customer.Id, cancellationToken);
    }

    public async Task EnqueueCustomerRestoredAsync(Customer customer, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId(customer.TenantId);

        await EnqueueLifecycleAsync(
            tenantId,
            customer.Id,
            "customer",
            CustomerManagementIntegrationEventNames.CustomerRestored,
            customer.OwnerUserId,
            new Dictionary<string, string> { ["customerName"] = customer.FullName },
            cancellationToken);

        await EnqueueCustomerSearchIndexAsync(tenantId, customer, cancellationToken);
    }

    public Task EnqueueCustomerPurgedAsync(
        Guid tenantId,
        Guid customerId,
        string? customerName,
        Guid? ownerUserId,
        CancellationToken cancellationToken)
        => EnqueueLifecycleAsync(
            tenantId,
            customerId,
            "customer",
            CustomerManagementIntegrationEventNames.CustomerPurged,
            ownerUserId,
            new Dictionary<string, string>
            {
                ["customerName"] = string.IsNullOrWhiteSpace(customerName) ? "Deleted customer" : customerName.Trim()
            },
            cancellationToken);

    public async Task EnqueueCompanyCreatedAsync(Company company, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId(company.TenantId);

        await EnqueueLifecycleAndNotificationAsync(
            tenantId,
            company.Id,
            "company",
            CustomerManagementIntegrationEventNames.CompanyCreated,
            company.OwnerUserId,
            company.Name,
            "Company created",
            $"Company {company.Name} was created.",
            cancellationToken);

        await EnqueueCompanySearchIndexAsync(tenantId, company, cancellationToken);
    }

    public async Task EnqueueCompanyUpdatedAsync(Company company, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId(company.TenantId);

        await EnqueueLifecycleAsync(
            tenantId,
            company.Id,
            "company",
            CustomerManagementIntegrationEventNames.CompanyUpdated,
            company.OwnerUserId,
            new Dictionary<string, string> { ["companyName"] = company.Name },
            cancellationToken);

        await EnqueueCompanySearchIndexAsync(tenantId, company, cancellationToken);
    }

    public async Task EnqueueCompanyDeletedAsync(Company company, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId(company.TenantId);

        await EnqueueLifecycleAsync(
            tenantId,
            company.Id,
            "company",
            CustomerManagementIntegrationEventNames.CompanyDeleted,
            company.OwnerUserId,
            new Dictionary<string, string> { ["companyName"] = company.Name },
            cancellationToken);

        await EnqueueCompanySearchDeleteAsync(tenantId, company.Id, cancellationToken);
    }

    public async Task EnqueueCompanyRestoredAsync(Company company, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId(company.TenantId);

        await EnqueueLifecycleAsync(
            tenantId,
            company.Id,
            "company",
            CustomerManagementIntegrationEventNames.CompanyRestored,
            company.OwnerUserId,
            new Dictionary<string, string> { ["companyName"] = company.Name },
            cancellationToken);

        await EnqueueCompanySearchIndexAsync(tenantId, company, cancellationToken);
    }

    public Task EnqueueCompanyPurgedAsync(
        Guid tenantId,
        Guid companyId,
        string? companyName,
        Guid? ownerUserId,
        CancellationToken cancellationToken)
        => EnqueueLifecycleAsync(
            tenantId,
            companyId,
            "company",
            CustomerManagementIntegrationEventNames.CompanyPurged,
            ownerUserId,
            new Dictionary<string, string>
            {
                ["companyName"] = string.IsNullOrWhiteSpace(companyName) ? "Deleted company" : companyName.Trim()
            },
            cancellationToken);

    public async Task EnqueueContactCreatedAsync(Contact contact, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId(contact.TenantId);

        await EnqueueLifecycleAndNotificationAsync(
            tenantId,
            contact.Id,
            "contact",
            CustomerManagementIntegrationEventNames.ContactCreated,
            contact.OwnerUserId,
            contact.FullName,
            "Contact created",
            $"Contact {contact.FullName} was created.",
            cancellationToken);

        await EnqueueContactSearchIndexAsync(tenantId, contact, cancellationToken);
    }

    public async Task EnqueueContactUpdatedAsync(Contact contact, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId(contact.TenantId);

        await EnqueueLifecycleAsync(
            tenantId,
            contact.Id,
            "contact",
            CustomerManagementIntegrationEventNames.ContactUpdated,
            contact.OwnerUserId,
            new Dictionary<string, string> { ["contactName"] = contact.FullName },
            cancellationToken);

        await EnqueueContactSearchIndexAsync(tenantId, contact, cancellationToken);
    }

    public async Task EnqueueContactDeletedAsync(Contact contact, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId(contact.TenantId);

        await EnqueueLifecycleAsync(
            tenantId,
            contact.Id,
            "contact",
            CustomerManagementIntegrationEventNames.ContactDeleted,
            contact.OwnerUserId,
            new Dictionary<string, string> { ["contactName"] = contact.FullName },
            cancellationToken);

        await EnqueueContactSearchDeleteAsync(tenantId, contact.Id, cancellationToken);
    }

    public async Task EnqueueContactRestoredAsync(Contact contact, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId(contact.TenantId);

        await EnqueueLifecycleAsync(
            tenantId,
            contact.Id,
            "contact",
            CustomerManagementIntegrationEventNames.ContactRestored,
            contact.OwnerUserId,
            new Dictionary<string, string> { ["contactName"] = contact.FullName },
            cancellationToken);

        await EnqueueContactSearchIndexAsync(tenantId, contact, cancellationToken);
    }

    public Task EnqueueContactPurgedAsync(
        Guid tenantId,
        Guid contactId,
        string? contactName,
        Guid? ownerUserId,
        CancellationToken cancellationToken)
        => EnqueueLifecycleAsync(
            tenantId,
            contactId,
            "contact",
            CustomerManagementIntegrationEventNames.ContactPurged,
            ownerUserId,
            new Dictionary<string, string>
            {
                ["contactName"] = string.IsNullOrWhiteSpace(contactName) ? "Deleted contact" : contactName.Trim()
            },
            cancellationToken);

    public Task EnqueuePrimaryContactChangedAsync(Contact contact, CancellationToken cancellationToken)
        => EnqueueLifecycleAsync(
            ResolveTenantId(contact.TenantId),
            contact.Id,
            "contact",
            CustomerManagementIntegrationEventNames.ContactPrimaryChanged,
            contact.OwnerUserId,
            new Dictionary<string, string>
            {
                ["contactName"] = contact.FullName,
                ["customerId"] = contact.CustomerId?.ToString("N") ?? string.Empty,
                ["companyId"] = contact.CompanyId?.ToString("N") ?? string.Empty
            },
            cancellationToken);

    private async Task EnqueueLifecycleAndNotificationAsync(
        Guid tenantId,
        Guid entityId,
        string entityType,
        string eventName,
        Guid? ownerUserId,
        string displayName,
        string subject,
        string textBody,
        CancellationToken cancellationToken)
    {
        await EnqueueLifecycleAsync(
            tenantId,
            entityId,
            entityType,
            eventName,
            ownerUserId,
            new Dictionary<string, string> { ["displayName"] = displayName },
            cancellationToken);

        if (ownerUserId is null || ownerUserId == Guid.Empty)
        {
            return;
        }

        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:{eventName}:{entityId:N}:notification";
        var notification = new NotificationRequestedV1(
            Guid.NewGuid(),
            tenantId,
            ownerUserId,
            "crm.customer-management",
            NotificationCategory.System,
            NotificationPriority.Normal,
            new NotificationRecipient(ownerUserId, null, null, null, null),
            [NotificationChannel.InApp],
            subject,
            textBody,
            null,
            new NotificationTemplateData($"crm.{eventName}", new Dictionary<string, string> { ["displayName"] = displayName }),
            new Dictionary<string, string>
            {
                ["entityType"] = entityType,
                ["entityId"] = entityId.ToString("N"),
                ["eventName"] = eventName
            },
            correlationId,
            idempotencyKey,
            occurredAt.UtcDateTime);

        await AddOutboxMessageAsync(
            tenantId,
            NotificationRequestedV1.EventName,
            NotificationRequestedV1.EventVersion,
            NotificationRequestedV1.RoutingKey,
            notification,
            occurredAt,
            correlationId,
            idempotencyKey,
            cancellationToken);
    }

    private Task EnqueueLifecycleAsync(
        Guid tenantId,
        Guid entityId,
        string entityType,
        string eventName,
        Guid? ownerUserId,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:{eventName}:{entityId:N}:{occurredAt.ToUnixTimeMilliseconds()}";
        var payload = new CustomerLifecycleIntegrationEventV1(
            Guid.NewGuid(),
            tenantId,
            entityId,
            entityType,
            eventName,
            ownerUserId,
            metadata,
            correlationId,
            occurredAt);

        return AddOutboxMessageAsync(
            tenantId,
            eventName,
            CustomerLifecycleIntegrationEventV1.EventVersion,
            $"{eventName}.v1",
            payload,
            occurredAt,
            correlationId,
            idempotencyKey,
            cancellationToken);
    }

    private async Task AddOutboxMessageAsync<TPayload>(
        Guid tenantId,
        string eventName,
        int eventVersion,
        string routingKey,
        TPayload payload,
        DateTimeOffset occurredAt,
        string? correlationId,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var message = CustomerManagementOutboxMessage.Create(
            tenantId,
            eventName,
            eventVersion,
            routingKey,
            JsonSerializer.Serialize(payload, SerializerOptions),
            occurredAt,
            correlationId,
            idempotencyKey);

        await dbContext.OutboxMessages.AddAsync(message, cancellationToken);
    }

    private Task EnqueueCustomerSearchIndexAsync(Guid tenantId, Customer customer, CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:search.customer.index:{customer.Id:N}:{occurredAt.ToUnixTimeMilliseconds()}";
        var payload = CustomerSearchIntegrationEventFactory.CreateCustomerIndexRequested(
            customer,
            tenantId,
            correlationId,
            correlationId,
            occurredAt);

        return AddOutboxMessageAsync(
            tenantId,
            SearchDocumentIndexRequestedV1.EventName,
            SearchDocumentIndexRequestedV1.EventVersion,
            "search.index.crm",
            payload,
            occurredAt,
            correlationId,
            idempotencyKey,
            cancellationToken);
    }

    private Task EnqueueCustomerSearchDeleteAsync(Guid tenantId, Guid customerId, CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:search.customer.delete:{customerId:N}:{occurredAt.ToUnixTimeMilliseconds()}";
        var payload = CustomerSearchIntegrationEventFactory.CreateCustomerDeleteRequested(
            customerId,
            tenantId,
            correlationId,
            correlationId,
            occurredAt);

        return AddOutboxMessageAsync(
            tenantId,
            SearchDocumentDeleteRequestedV1.EventName,
            SearchDocumentDeleteRequestedV1.EventVersion,
            "search.delete.crm",
            payload,
            occurredAt,
            correlationId,
            idempotencyKey,
            cancellationToken);
    }

    private Task EnqueueCompanySearchIndexAsync(Guid tenantId, Company company, CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:search.company.index:{company.Id:N}:{occurredAt.ToUnixTimeMilliseconds()}";
        var payload = CompanySearchIntegrationEventFactory.CreateCompanyIndexRequested(
            company,
            tenantId,
            correlationId,
            correlationId,
            occurredAt);

        return AddOutboxMessageAsync(
            tenantId,
            SearchDocumentIndexRequestedV1.EventName,
            SearchDocumentIndexRequestedV1.EventVersion,
            "search.index.crm",
            payload,
            occurredAt,
            correlationId,
            idempotencyKey,
            cancellationToken);
    }

    private Task EnqueueCompanySearchDeleteAsync(Guid tenantId, Guid companyId, CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:search.company.delete:{companyId:N}:{occurredAt.ToUnixTimeMilliseconds()}";
        var payload = CompanySearchIntegrationEventFactory.CreateCompanyDeleteRequested(
            companyId,
            tenantId,
            correlationId,
            correlationId,
            occurredAt);

        return AddOutboxMessageAsync(
            tenantId,
            SearchDocumentDeleteRequestedV1.EventName,
            SearchDocumentDeleteRequestedV1.EventVersion,
            "search.delete.crm",
            payload,
            occurredAt,
            correlationId,
            idempotencyKey,
            cancellationToken);
    }

    private Task EnqueueContactSearchIndexAsync(Guid tenantId, Contact contact, CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:search.contact.index:{contact.Id:N}:{occurredAt.ToUnixTimeMilliseconds()}";
        var payload = ContactSearchIntegrationEventFactory.CreateContactIndexRequested(
            contact,
            tenantId,
            correlationId,
            correlationId,
            occurredAt);

        return AddOutboxMessageAsync(
            tenantId,
            SearchDocumentIndexRequestedV1.EventName,
            SearchDocumentIndexRequestedV1.EventVersion,
            "search.index.crm",
            payload,
            occurredAt,
            correlationId,
            idempotencyKey,
            cancellationToken);
    }

    private Task EnqueueContactSearchDeleteAsync(Guid tenantId, Guid contactId, CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:search.contact.delete:{contactId:N}:{occurredAt.ToUnixTimeMilliseconds()}";
        var payload = ContactSearchIntegrationEventFactory.CreateContactDeleteRequested(
            contactId,
            tenantId,
            correlationId,
            correlationId,
            occurredAt);

        return AddOutboxMessageAsync(
            tenantId,
            SearchDocumentDeleteRequestedV1.EventName,
            SearchDocumentDeleteRequestedV1.EventVersion,
            "search.delete.crm",
            payload,
            occurredAt,
            correlationId,
            idempotencyKey,
            cancellationToken);
    }

    private static string? GetCorrelationId()
        => Activity.Current?.TraceId.ToString();

    private Guid ResolveTenantId(Guid tenantId)
        => tenantId == Guid.Empty ? currentUserService.EnsureTenant() : tenantId;
}

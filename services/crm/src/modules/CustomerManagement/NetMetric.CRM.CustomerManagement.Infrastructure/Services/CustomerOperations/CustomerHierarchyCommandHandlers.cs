// <copyright file="CustomerHierarchyCommandHandlers.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerIntelligence.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.AccountHierarchyNodes;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Customer360;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Features.Customer360;
using NetMetric.CRM.CustomerManagement.Application.Features.CustomerOperations;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Services.CustomerOperations;

public sealed class GetAccountHierarchyQueryHandler(
    ICustomerManagementDbContext customerDbContext,
    ICurrentUserService currentUserService,
    ICustomerAccountHierarchyProvider hierarchyProvider) : IRequestHandler<GetAccountHierarchyQuery, CustomerAccountHierarchyDto>
{
    public async Task<CustomerAccountHierarchyDto> Handle(GetAccountHierarchyQuery request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        var customer = await customerDbContext.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Id == request.CustomerId, cancellationToken)
            ?? throw new NotFoundAppException("Customer not found.");
        return await hierarchyProvider.GetHierarchyAsync(tenantId, customer.Id, customer.CompanyId, cancellationToken);
    }
}

public sealed class AddAccountHierarchyNodeCommandHandler(
    ICustomerManagementDbContext customerDbContext,
    ICustomerIntelligenceDbContext intelligenceDbContext,
    ICurrentUserService currentUserService) : IRequestHandler<AddAccountHierarchyNodeCommand, Guid>
{
    public async Task<Guid> Handle(AddAccountHierarchyNodeCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        await ValidateCompaniesAsync(customerDbContext, tenantId, request.CompanyId, request.ParentCompanyId, cancellationToken);
        if (request.ParentCompanyId == request.CompanyId)
            throw new ValidationAppException("Company cannot be its own parent.");
        var edges = await LoadEdgesAsync(intelligenceDbContext, tenantId, cancellationToken);
        if (edges.Any(x => x.CompanyId == request.CompanyId && !x.IsDeleted))
            throw new ValidationAppException("Active hierarchy relation already exists for company.");
        if (request.ParentCompanyId.HasValue && CreatesCycle(request.CompanyId, request.ParentCompanyId.Value, edges))
            throw new ValidationAppException("Circular account hierarchy is not allowed.");

        var payload = JsonSerializer.Serialize(new { companyId = request.CompanyId, parentCompanyId = request.ParentCompanyId, relationshipType = request.RelationshipType.ToString(), request.DisplayOrder, request.IsPrimary });
        var node = AccountHierarchyNode.Create(request.RelationshipType.ToString(), "AccountHierarchy", request.CompanyId, payload);
        node.TenantId = tenantId;
        await intelligenceDbContext.AccountHierarchyNodes.AddAsync(node, cancellationToken);
        await intelligenceDbContext.SaveChangesAsync(cancellationToken);
        return node.Id;
    }

    internal static async Task ValidateCompaniesAsync(ICustomerManagementDbContext dbContext, Guid tenantId, Guid companyId, Guid? parentCompanyId, CancellationToken cancellationToken)
    {
        var ids = new[] { companyId, parentCompanyId ?? companyId }.Distinct().ToList();
        var count = await dbContext.Companies.AsNoTracking().CountAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive && ids.Contains(x.Id), cancellationToken);
        if (count != ids.Count)
            throw new ValidationAppException("Hierarchy companies must exist in current tenant.");
    }

    internal static async Task<List<Edge>> LoadEdgesAsync(ICustomerIntelligenceDbContext dbContext, Guid tenantId, CancellationToken cancellationToken)
        => (await dbContext.AccountHierarchyNodes.AsNoTracking()
                .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive && x.EntityType == "AccountHierarchy")
                .Select(x => new { x.Id, x.RelatedEntityId, x.DataJson, x.IsDeleted })
                .ToListAsync(cancellationToken))
            .Select(x => Edge.Parse(x.Id, x.RelatedEntityId, x.DataJson, x.IsDeleted))
            .Where(x => x.CompanyId.HasValue)
            .ToList();

    internal static bool CreatesCycle(Guid companyId, Guid parentCompanyId, IReadOnlyList<Edge> edges)
    {
        var cursor = parentCompanyId;
        var visited = new HashSet<Guid>();
        while (visited.Add(cursor))
        {
            if (cursor == companyId)
            {
                return true;
            }

            var parent = edges.FirstOrDefault(x => x.CompanyId == cursor)?.ParentCompanyId;
            if (!parent.HasValue)
            {
                return false;
            }

            cursor = parent.Value;
        }
        return true;
    }

    internal sealed record Edge(Guid Id, Guid? CompanyId, Guid? ParentCompanyId, bool IsDeleted)
    {
        public static Edge Parse(Guid id, Guid? relatedEntityId, string? json, bool isDeleted)
        {
            Guid? companyId = relatedEntityId;
            Guid? parentCompanyId = null;
            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("companyId", out var c) && Guid.TryParse(c.GetString(), out var parsedCompany)) companyId = parsedCompany;
                    if (doc.RootElement.TryGetProperty("parentCompanyId", out var p) && Guid.TryParse(p.GetString(), out var parsedParent)) parentCompanyId = parsedParent;
                }
                catch (JsonException) { }
            }
            return new Edge(id, companyId, parentCompanyId, isDeleted);
        }
    }
}

public sealed class MoveAccountHierarchyNodeCommandHandler(
    ICustomerManagementDbContext customerDbContext,
    ICustomerIntelligenceDbContext intelligenceDbContext,
    ICurrentUserService currentUserService) : IRequestHandler<MoveAccountHierarchyNodeCommand, Guid>
{
    public async Task<Guid> Handle(MoveAccountHierarchyNodeCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        var node = await intelligenceDbContext.AccountHierarchyNodes.FirstOrDefaultAsync(x => x.TenantId == tenantId && !x.IsDeleted && x.Id == request.NodeId, cancellationToken)
            ?? throw new NotFoundAppException("Hierarchy node not found.");
        var edge = AddAccountHierarchyNodeCommandHandler.Edge.Parse(node.Id, node.RelatedEntityId, node.DataJson, node.IsDeleted);
        if (!edge.CompanyId.HasValue) throw new ValidationAppException("Hierarchy node is missing company id.");
        await ValidateCompaniesAsync(customerDbContext, tenantId, edge.CompanyId.Value, request.NewParentCompanyId, cancellationToken);
        var edges = await AddAccountHierarchyNodeCommandHandler.LoadEdgesAsync(intelligenceDbContext, tenantId, cancellationToken);
        if (request.NewParentCompanyId.HasValue && AddAccountHierarchyNodeCommandHandler.CreatesCycle(edge.CompanyId.Value, request.NewParentCompanyId.Value, edges.Where(x => x.Id != node.Id).ToList()))
            throw new ValidationAppException("Circular account hierarchy is not allowed.");
        node.IsDeleted = true;
        var payload = JsonSerializer.Serialize(new { companyId = edge.CompanyId.Value, parentCompanyId = request.NewParentCompanyId, relationshipType = node.Name, displayOrder = 0, isPrimary = false });
        var replacement = AccountHierarchyNode.Create(node.Name, "AccountHierarchy", edge.CompanyId, payload);
        replacement.TenantId = tenantId;
        await intelligenceDbContext.AccountHierarchyNodes.AddAsync(replacement, cancellationToken);
        await intelligenceDbContext.SaveChangesAsync(cancellationToken);
        return replacement.Id;
    }

    private static Task ValidateCompaniesAsync(ICustomerManagementDbContext dbContext, Guid tenantId, Guid companyId, Guid? parentCompanyId, CancellationToken cancellationToken)
        => AddAccountHierarchyNodeCommandHandler.ValidateCompaniesAsync(dbContext, tenantId, companyId, parentCompanyId, cancellationToken);
}

public sealed class RemoveAccountHierarchyNodeCommandHandler(ICustomerIntelligenceDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<RemoveAccountHierarchyNodeCommand>
{
    public async Task Handle(RemoveAccountHierarchyNodeCommand request, CancellationToken cancellationToken)
    {
        var node = await dbContext.AccountHierarchyNodes.FirstOrDefaultAsync(x => x.TenantId == currentUserService.EnsureTenant() && !x.IsDeleted && x.Id == request.NodeId, cancellationToken)
            ?? throw new NotFoundAppException("Hierarchy node not found.");
        node.IsDeleted = true;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class RemoveCustomerStakeholderCommandHandler(ICustomerManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<RemoveCustomerStakeholderCommand>
{
    public async Task Handle(RemoveCustomerStakeholderCommand request, CancellationToken cancellationToken)
    {
        var stakeholder = await dbContext.CustomerStakeholders.FirstOrDefaultAsync(x => x.TenantId == currentUserService.EnsureTenant() && !x.IsDeleted && x.Id == request.StakeholderId, cancellationToken)
            ?? throw new NotFoundAppException("Stakeholder not found.");
        stakeholder.IsDeleted = true;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

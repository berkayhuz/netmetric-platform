// <copyright file="CustomerIntelligenceHierarchyProviders.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerIntelligence.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Customer360;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Features.Customer360;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Services.CustomerOperations;

public sealed class CustomerIntelligenceAccountHierarchyProvider(
    ICustomerIntelligenceDbContext intelligenceDbContext,
    ICustomerManagementDbContext customerManagementDbContext) : ICustomerAccountHierarchyProvider
{
    public async Task<CustomerAccountHierarchyDto> GetHierarchyAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken)
    {
        if (!companyId.HasValue)
            return new CustomerAccountHierarchyDto([]);

        var companyIds = await customerManagementDbContext.Companies.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive)
            .Select(x => new { x.Id, x.Name, x.ParentCompanyId })
            .ToListAsync(cancellationToken);

        var hierarchyNodes = await intelligenceDbContext.AccountHierarchyNodes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive)
            .Where(x => x.EntityType == "Company" || x.EntityType == "AccountHierarchy")
            .ToListAsync(cancellationToken);

        var parsed = hierarchyNodes
            .Select(x => Parse(x.Id, x.Name, x.RelatedEntityId, x.DataJson))
            .Where(x => x.CompanyId.HasValue)
            .ToList();

        var edges = parsed.Count == 0
            ? companyIds.Where(x => x.ParentCompanyId.HasValue).Select(x => new HierarchyEdge(Guid.NewGuid(), x.Id, x.ParentCompanyId, x.Name, "Subsidiary", 0, false)).ToList()
            : parsed.Select(x => new HierarchyEdge(x.Id, x.CompanyId!.Value, x.ParentCompanyId, x.Name, x.RelationshipType, x.DisplayOrder, x.IsPrimary)).ToList();

        var reachable = new HashSet<Guid> { companyId.Value };
        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var edge in edges.Where(x => x.CompanyId.HasValue))
            {
                var childId = edge.CompanyId!.Value;
                if (edge.ParentCompanyId.HasValue && reachable.Contains(childId) && reachable.Add(edge.ParentCompanyId.Value)) changed = true;
                if (edge.ParentCompanyId.HasValue && reachable.Contains(edge.ParentCompanyId.Value) && reachable.Add(childId)) changed = true;
            }
        }

        var names = companyIds.ToDictionary(x => x.Id, x => x.Name);
        var scopedEdges = edges.Where(x => x.CompanyId.HasValue && reachable.Contains(x.CompanyId.Value)).ToList();
        var roots = scopedEdges
            .Where(x => !x.ParentCompanyId.HasValue || !reachable.Contains(x.ParentCompanyId.Value))
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => Build(x, scopedEdges, names, new HashSet<Guid>()))
            .ToList();

        if (roots.Count == 0 && names.TryGetValue(companyId.Value, out var name))
            roots.Add(new CustomerAccountHierarchyNodeDto(companyId.Value, companyId.Value, null, name, "Self", 0, true, []));

        return new CustomerAccountHierarchyDto(roots);
    }

    private static CustomerAccountHierarchyNodeDto Build(HierarchyEdge edge, IReadOnlyList<HierarchyEdge> edges, IReadOnlyDictionary<Guid, string> names, HashSet<Guid> path)
    {
        var companyId = edge.CompanyId!.Value;
        if (!path.Add(companyId))
            return new CustomerAccountHierarchyNodeDto(edge.Id, companyId, edge.ParentCompanyId, edge.Name, edge.RelationshipType, edge.DisplayOrder, edge.IsPrimary, []);

        var children = edges
            .Where(x => x.ParentCompanyId == edge.CompanyId)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => Build(x with { Name = x.CompanyId.HasValue && names.TryGetValue(x.CompanyId.Value, out var childName) ? childName : x.Name }, edges, names, new HashSet<Guid>(path)))
            .ToList();

        return new CustomerAccountHierarchyNodeDto(edge.Id, companyId, edge.ParentCompanyId, names.TryGetValue(companyId, out var name) ? name : edge.Name, edge.RelationshipType, edge.DisplayOrder, edge.IsPrimary, children);
    }

    private static HierarchyEdge Parse(Guid id, string name, Guid? relatedEntityId, string? dataJson)
    {
        Guid? companyId = relatedEntityId;
        Guid? parentCompanyId = null;
        var relationshipType = "Subsidiary";
        var displayOrder = 0;
        var isPrimary = false;
        if (!string.IsNullOrWhiteSpace(dataJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(dataJson);
                var root = doc.RootElement;
                companyId = ReadGuid(root, "companyId") ?? companyId;
                parentCompanyId = ReadGuid(root, "parentCompanyId");
                relationshipType = ReadString(root, "relationshipType") ?? relationshipType;
                displayOrder = ReadInt(root, "displayOrder") ?? displayOrder;
                isPrimary = ReadBool(root, "isPrimary") ?? isPrimary;
            }
            catch (JsonException) { }
        }
        return new HierarchyEdge(id, companyId, parentCompanyId, name, relationshipType, displayOrder, isPrimary);
    }

    private static Guid? ReadGuid(JsonElement root, string name) => root.TryGetProperty(name, out var value) && Guid.TryParse(value.GetString(), out var parsed) ? parsed : null;
    private static string? ReadString(JsonElement root, string name) => root.TryGetProperty(name, out var value) ? value.GetString() : null;
    private static int? ReadInt(JsonElement root, string name) => root.TryGetProperty(name, out var value) && value.TryGetInt32(out var parsed) ? parsed : null;
    private static bool? ReadBool(JsonElement root, string name) => root.TryGetProperty(name, out var value) && (value.ValueKind is JsonValueKind.True or JsonValueKind.False) ? value.GetBoolean() : null;

    private sealed record HierarchyEdge(Guid Id, Guid? CompanyId, Guid? ParentCompanyId, string Name, string RelationshipType, int DisplayOrder, bool IsPrimary);
}

public sealed class CustomerManagementStakeholderMapProvider(ICustomerManagementDbContext dbContext) : ICustomerStakeholderMapProvider
{
    public async Task<IReadOnlyList<CustomerStakeholderMapItemDto>> GetStakeholdersAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken)
    {
        if (!companyId.HasValue)
            return [];

        return await dbContext.CustomerStakeholders.AsNoTracking()
            .Join(dbContext.Contacts.AsNoTracking(),
                stakeholder => stakeholder.ContactId,
                contact => contact.Id,
                (stakeholder, contact) => new { stakeholder, contact })
            .Where(x => x.stakeholder.TenantId == tenantId && !x.stakeholder.IsDeleted && x.stakeholder.IsActive)
            .Where(x => x.contact.TenantId == tenantId && !x.contact.IsDeleted && x.contact.IsActive)
            .Where(x => x.stakeholder.CompanyId == companyId.Value)
            .OrderByDescending(x => x.stakeholder.IsPrimary)
            .ThenBy(x => x.contact.FirstName)
            .Select(x => new CustomerStakeholderMapItemDto(
                x.stakeholder.Id,
                x.stakeholder.CompanyId,
                x.stakeholder.ContactId,
                x.contact.FullName,
                x.stakeholder.Role,
                x.stakeholder.InfluenceLevel,
                x.stakeholder.Sentiment,
                x.stakeholder.IsPrimary))
            .ToListAsync(cancellationToken);
    }
}

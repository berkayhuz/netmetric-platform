// <copyright file="CustomersController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.CustomerManagement.Application.Commands.Customers;
using NetMetric.CRM.CustomerManagement.Application.Features.Customer360;
using NetMetric.CRM.CustomerManagement.Application.Features.CustomerOperations;
using NetMetric.CRM.CustomerManagement.Application.Queries.Contacts;
using NetMetric.CRM.CustomerManagement.Application.Queries.Customers;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.CRM.CustomerManagement.Contracts.Requests;
using NetMetric.CRM.CustomerManagement.Domain.Entities.CustomerOperations;
using NetMetric.CRM.Types;
using NetMetric.Pagination;

namespace NetMetric.CRM.API.Controllers.CustomerManagement;

[ApiController]
[Route("api/customers")]
[Route("api/v1/customers")]
public sealed class CustomersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.CustomersRead)]
    public async Task<ActionResult<PagedResult<CustomerListItemDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] CustomerType? customerType,
        [FromQuery] bool? isVip,
        [FromQuery] Guid? companyId,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null,
        CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new GetCustomersQuery(search, customerType, isVip, companyId, isActive, page, pageSize, sortBy, sortDirection), cancellationToken));

    [HttpGet("{customerId:guid}/contacts")]
    [Authorize(Policy = AuthorizationPolicies.ContactsRead)]
    public async Task<ActionResult<PagedResult<ContactListItemDto>>> GetContacts(
        Guid customerId,
        [FromQuery] string? search,
        [FromQuery] bool? isPrimary,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null,
        CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new GetContactsQuery(search, null, customerId, isPrimary, isActive, page, pageSize, sortBy, sortDirection), cancellationToken));

    [HttpGet("{customerId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CustomersRead)]
    public async Task<ActionResult<CustomerDetailDto>> GetById(Guid customerId, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetCustomerByIdQuery(customerId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{customerId:guid}/360")]
    [Authorize(Policy = AuthorizationPolicies.CustomersRead)]
    public async Task<ActionResult<Customer360Dto>> Get360(Guid customerId, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new GetCustomer360Query(customerId), cancellationToken));

    [HttpGet("{customerId:guid}/consents")]
    [Authorize(Policy = AuthorizationPolicies.CustomersRead)]
    public async Task<ActionResult<IReadOnlyList<CustomerConsentDto>>> GetConsents(Guid customerId, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new GetCustomerConsentSummaryQuery(customerId), cancellationToken));

    [HttpGet("{customerId:guid}/hierarchy")]
    [Authorize(Policy = AuthorizationPolicies.CustomersRead)]
    public async Task<ActionResult<CustomerAccountHierarchyDto>> GetHierarchy(Guid customerId, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new GetAccountHierarchyQuery(customerId), cancellationToken));

    [HttpPost("hierarchy")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<ActionResult<Guid>> AddHierarchyNode([FromBody] AddAccountHierarchyNodeRequest request, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new AddAccountHierarchyNodeCommand(request.CompanyId, request.ParentCompanyId, request.RelationshipType, request.DisplayOrder, request.IsPrimary), cancellationToken));

    [HttpPost("hierarchy/{nodeId:guid}/move")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<ActionResult<Guid>> MoveHierarchyNode(Guid nodeId, [FromBody] MoveAccountHierarchyNodeRequest request, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new MoveAccountHierarchyNodeCommand(nodeId, request.NewParentCompanyId, request.Reason), cancellationToken));

    [HttpDelete("hierarchy/{nodeId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<IActionResult> RemoveHierarchyNode(Guid nodeId, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new RemoveAccountHierarchyNodeCommand(nodeId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{customerId:guid}/consents")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<ActionResult<Guid>> UpsertConsent(Guid customerId, [FromBody] UpsertCustomerConsentRequest request, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new UpsertCustomerConsentCommand(customerId, request.Channel, request.Purpose, request.Status, request.Source, request.ValidUntilUtc, request.EvidenceText, request.EvidenceIpAddress, request.EvidenceUserAgent, request.Reason), cancellationToken));

    [HttpPost("{customerId:guid}/consents/{consentId:guid}/revoke")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<IActionResult> RevokeConsent(Guid customerId, Guid consentId, [FromBody] RevokeCustomerConsentRequest request, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new RevokeCustomerConsentCommand(customerId, consentId, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPost("{customerId:guid}/lifecycle-stage")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<IActionResult> ChangeLifecycleStage(Guid customerId, [FromBody] ChangeCustomerLifecycleStageRequest request, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new ChangeCustomerLifecycleStageCommand(customerId, request.NewStage, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPost("{customerId:guid}/data-quality/recalculate")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<ActionResult<int>> RecalculateDataQuality(Guid customerId, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new RecalculateCustomerDataQualityCommand(customerId), cancellationToken));

    [HttpPost("{customerId:guid}/relationship-health/recalculate")]
    [Authorize(Policy = AuthorizationPolicies.CustomerHealthRead)]
    public async Task<ActionResult<int>> RecalculateRelationshipHealth(Guid customerId, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new RecalculateCustomerRelationshipHealthCommand(customerId), cancellationToken));

    [HttpGet("{customerId:guid}/duplicates")]
    [Authorize(Policy = AuthorizationPolicies.CustomerDuplicatesRead)]
    public async Task<ActionResult<IReadOnlyList<CustomerDuplicateWarningDto>>> FindDuplicates(Guid customerId, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new FindCustomerDuplicatesQuery(customerId), cancellationToken));

    [HttpGet("duplicates/merge-preview")]
    [Authorize(Policy = AuthorizationPolicies.CustomerDuplicatesManage)]
    public async Task<ActionResult<CustomerMergePreviewDto>> GetMergePreview([FromQuery] Guid masterCustomerId, [FromQuery] Guid duplicateCustomerId, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new GetDuplicateMergePreviewQuery(masterCustomerId, duplicateCustomerId), cancellationToken));

    [HttpPost("merge")]
    [Authorize(Policy = AuthorizationPolicies.CustomerDuplicatesManage)]
    public async Task<ActionResult<Guid>> MergeCustomers([FromBody] MergeCustomersRequest request, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new MergeCustomersCommand(request.MasterCustomerId, request.DuplicateCustomerId, request.ResolvedFields, request.Reason), cancellationToken));

    [HttpGet("{customerId:guid}/audit-timeline")]
    [Authorize(Policy = AuthorizationPolicies.CustomerTimelineRead)]
    public async Task<ActionResult<IReadOnlyList<CustomerAuditEventDto>>> GetAuditTimeline(Guid customerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new GetCustomerAuditTimelineQuery(customerId, page, pageSize), cancellationToken));

    [HttpPost("{customerId:guid}/shares")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<ActionResult<Guid>> ShareCustomer(Guid customerId, [FromBody] ShareCustomerRecordRequest request, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new ShareCustomerRecordCommand(CustomerEntityType.Customer, customerId, request.SharedWithUserId, request.SharedWithTeamId, request.AccessLevel, request.ValidUntilUtc, request.Reason), cancellationToken));

    [HttpDelete("{customerId:guid}/shares/{shareId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<IActionResult> RevokeCustomerShare(Guid customerId, Guid shareId, CancellationToken cancellationToken = default)
    {
        _ = customerId;
        await mediator.Send(new RevokeCustomerRecordShareCommand(shareId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{customerId:guid}/stakeholders/{stakeholderId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<IActionResult> RemoveStakeholder(Guid customerId, Guid stakeholderId, CancellationToken cancellationToken = default)
    {
        _ = customerId;
        await mediator.Send(new RemoveCustomerStakeholderCommand(stakeholderId), cancellationToken);
        return NoContent();
    }

    [HttpGet("search")]
    [Authorize(Policy = AuthorizationPolicies.CustomerSearchRead)]
    public async Task<ActionResult<IReadOnlyList<CustomerSearchResultDto>>> SearchCustomers([FromQuery] string term, [FromQuery] int take = 20, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new SearchCustomersQuery(term, take), cancellationToken));

    [HttpPost("import-batches")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<ActionResult<Guid>> CreateImportBatch([FromBody] CreateCustomerImportBatchRequest request, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new CreateCustomerImportBatchCommand(request.FileName, request.Source, request.Rows.Select(x => new CustomerImportRawRow(x)).ToList()), cancellationToken));

    [HttpGet("import-batches")]
    [Authorize(Policy = AuthorizationPolicies.CustomersRead)]
    public async Task<ActionResult<IReadOnlyList<CustomerImportBatchDto>>> ListImportBatches([FromQuery] int take = 50, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new ListCustomerImportBatchesQuery(take), cancellationToken));

    [HttpGet("import-batches/{batchId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CustomersRead)]
    public async Task<ActionResult<CustomerImportBatchDto>> GetImportBatch(Guid batchId, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new GetCustomerImportBatchQuery(batchId), cancellationToken));

    [HttpPost("import-batches/{batchId:guid}/preview")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<ActionResult<CustomerImportBatchDto>> PreviewImportBatch(Guid batchId, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new PreviewCustomerImportMappingCommand(batchId), cancellationToken));

    [HttpPost("import-batches/{batchId:guid}/validate")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<ActionResult<CustomerImportBatchDto>> ValidateImportBatch(Guid batchId, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new ValidateCustomerImportBatchCommand(batchId), cancellationToken));

    [HttpPost("import-batches/{batchId:guid}/commit")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<ActionResult<CustomerImportBatchDto>> CommitImportBatch(Guid batchId, [FromBody] CommitCustomerImportBatchRequest request, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new CommitCustomerImportBatchCommand(batchId, request.DuplicateStrategy), cancellationToken));

    [HttpPost("import-batches/{batchId:guid}/cancel")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<IActionResult> CancelImportBatch(Guid batchId, [FromBody] CancelCustomerImportBatchRequest request, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new CancelCustomerImportBatchCommand(batchId, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<ActionResult<CustomerDetailDto>> Create([FromBody] CustomerUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new CreateCustomerCommand(
            request.FirstName,
            request.LastName,
            request.Title,
            request.Email,
            request.MobilePhone,
            request.WorkPhone,
            request.PersonalPhone,
            request.BirthDate,
            request.Gender,
            request.Department,
            request.JobTitle,
            request.Description,
            request.Notes,
            request.OwnerUserId,
            request.CustomerType,
            request.IdentityNumber,
            request.IsVip,
            request.IsActive,
            request.CompanyId), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { customerId = result.Id }, result);
    }

    [HttpPut("{customerId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<ActionResult<CustomerDetailDto>> Update(Guid customerId, [FromBody] CustomerUpsertRequest request, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new UpdateCustomerCommand(
            customerId,
            request.FirstName,
            request.LastName,
            request.Title,
            request.Email,
            request.MobilePhone,
            request.WorkPhone,
            request.PersonalPhone,
            request.BirthDate,
            request.Gender,
            request.Department,
            request.JobTitle,
            request.Description,
            request.Notes,
            request.OwnerUserId,
            request.CustomerType,
            request.IdentityNumber,
            request.IsVip,
            request.IsActive,
            request.CompanyId,
            request.RowVersion), cancellationToken));

    [HttpPost("{customerId:guid}/image")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<ActionResult<CustomerDetailDto>> UploadImage(
        Guid customerId,
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("Image file is required.");
        }

        await using var stream = file.OpenReadStream();
        return Ok(await mediator.Send(new UploadCustomerImageCommand(
            customerId,
            file.FileName,
            file.ContentType,
            stream,
            file.Length), cancellationToken));
    }

    [HttpDelete("{customerId:guid}/image")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<ActionResult<CustomerDetailDto>> RemoveImage(Guid customerId, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new RemoveCustomerImageCommand(customerId), cancellationToken));

    [HttpPost("{customerId:guid}/vip")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<IActionResult> MarkVip(Guid customerId, [FromQuery] bool isVip = true, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new MarkCustomerAsVipCommand(customerId, isVip), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{customerId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<IActionResult> SoftDelete(Guid customerId, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new SoftDeleteCustomerCommand(customerId), cancellationToken);
        return NoContent();
    }

    public sealed record UpsertCustomerConsentRequest([property: JsonRequired] CustomerConsentChannel Channel, [property: JsonRequired] CustomerConsentPurpose Purpose, [property: JsonRequired] CustomerConsentStatus Status, [property: JsonRequired] CustomerConsentSource Source, DateTime? ValidUntilUtc, string? EvidenceText, string? EvidenceIpAddress, string? EvidenceUserAgent, string? Reason);
    public sealed record RevokeCustomerConsentRequest(string Reason);
    public sealed record ChangeCustomerLifecycleStageRequest([property: JsonRequired] CustomerLifecycleStage NewStage, string? Reason);
    public sealed record MergeCustomersRequest([property: JsonRequired] Guid MasterCustomerId, [property: JsonRequired] Guid DuplicateCustomerId, IReadOnlyDictionary<string, string?> ResolvedFields, string Reason);
    public sealed record ShareCustomerRecordRequest(Guid? SharedWithUserId, Guid? SharedWithTeamId, [property: JsonRequired] CustomerRecordAccessLevel AccessLevel, DateTime? ValidUntilUtc, string Reason);
    public sealed record AddAccountHierarchyNodeRequest([property: JsonRequired] Guid CompanyId, Guid? ParentCompanyId, [property: JsonRequired] CustomerRelationshipType RelationshipType, [property: JsonRequired] int DisplayOrder, [property: JsonRequired] bool IsPrimary);
    public sealed record MoveAccountHierarchyNodeRequest(Guid? NewParentCompanyId, string Reason);
    public sealed record CreateCustomerImportBatchRequest(string FileName, string Source, IReadOnlyList<IReadOnlyDictionary<string, string?>> Rows);
    public sealed record CommitCustomerImportBatchRequest([property: JsonRequired] CustomerDuplicateStrategy DuplicateStrategy);
    public sealed record CancelCustomerImportBatchRequest(string? Reason);
}

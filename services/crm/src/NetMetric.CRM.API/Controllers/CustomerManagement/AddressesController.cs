// <copyright file="AddressesController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.CustomerManagement.Application.Commands.Addresses;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.CRM.CustomerManagement.Contracts.Requests;

namespace NetMetric.CRM.API.Controllers.CustomerManagement;

[ApiController]
[Route("api/addresses")]
public sealed class AddressesController(IMediator mediator) : ControllerBase
{
    [HttpPost("companies/{companyId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CompaniesManage)]
    public async Task<ActionResult<AddressDto>> AddToCompany(Guid companyId, [FromBody] AddressUpsertRequest request, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new AddCompanyAddressCommand(
            companyId,
            request.AddressType,
            request.Line1,
            request.Line2,
            request.District,
            request.City,
            request.State,
            request.Country,
            request.ZipCode,
            request.IsDefault), cancellationToken));

    [HttpPost("customers/{customerId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<ActionResult<AddressDto>> AddToCustomer(Guid customerId, [FromBody] AddressUpsertRequest request, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new AddCustomerAddressCommand(
            customerId,
            request.AddressType,
            request.Line1,
            request.Line2,
            request.District,
            request.City,
            request.State,
            request.Country,
            request.ZipCode,
            request.IsDefault), cancellationToken));

    [HttpPut("{addressId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<ActionResult<AddressDto>> Update(Guid addressId, [FromBody] AddressUpsertRequest request, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new UpdateAddressCommand(
            addressId,
            request.AddressType,
            request.Line1,
            request.Line2,
            request.District,
            request.City,
            request.State,
            request.Country,
            request.ZipCode,
            request.IsDefault,
            request.RowVersion), cancellationToken));

    [HttpPost("{addressId:guid}/set-default")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<IActionResult> SetDefault(Guid addressId, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new SetDefaultAddressCommand(addressId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{addressId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CustomersManage)]
    public async Task<IActionResult> SoftDelete(Guid addressId, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new SoftDeleteAddressCommand(addressId), cancellationToken);
        return NoContent();
    }
}

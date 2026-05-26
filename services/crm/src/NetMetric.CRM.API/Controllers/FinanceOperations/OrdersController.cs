// <copyright file="OrdersController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.FinanceOperations.Application.Features.Orders.Commands.CreateSalesOrder;
using NetMetric.CRM.FinanceOperations.Application.Features.Orders.Queries.GetSalesOrders;
using NetMetric.CRM.FinanceOperations.Application.Features.Orders.Queries.GetSalesOrderById;

namespace NetMetric.CRM.API.Controllers.FinanceOperations;

[ApiController]
[Route("api/orders")]
[Authorize(Policy = AuthorizationPolicies.OrdersRead)]
public sealed class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSalesOrdersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> Get(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSalesOrderByIdQuery(orderId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.OrdersManage)]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateSalesOrderCommand(request.Code, request.Name, request.Description), cancellationToken);
        return CreatedAtAction(nameof(Get), new { orderId = result.Id }, result);
    }

    public sealed record CreateSalesOrderRequest(string Code, string Name, string? Description);
}

// <copyright file="CustomerCommandHandlerDelegationTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Moq;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;
using NetMetric.CRM.CustomerManagement.Application.Commands.Customers;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerManagement.UnitTests.Commands;

public sealed class CustomerCommandHandlerDelegationTests
{
    [Fact]
    public async Task CreateCustomerCommandHandler_Should_Call_Service()
    {
        var service = new Mock<ICustomerAdministrationService>();
        var command = new CreateCustomerCommand("Berkay", "Huz", null, "berkay@test.com", "555", null, null, null, GenderType.Male, null, null, null, null, null, CustomerType.Individual, null, true, true, null);
        var expected = new CustomerDetailDto(Guid.NewGuid(), "Berkay", "Huz", "Berkay Huz", null, "berkay@test.com", "555", null, null, null, GenderType.Male, null, null, null, null, null, CustomerType.Individual, null, true, null, null, true, Array.Empty<AddressDto>(), Array.Empty<CustomerContactSummaryDto>(), "rv");
        service.Setup(x => x.CreateAsync(command, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var sut = new CreateCustomerCommandHandler(service.Object);
        var result = await sut.Handle(command, CancellationToken.None);

        result.Should().Be(expected);
    }
}

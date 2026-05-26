// <copyright file="CompanyCommandHandlerDelegationTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Moq;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;
using NetMetric.CRM.CustomerManagement.Application.Commands.Companies;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerManagement.UnitTests.Commands;

public sealed class CompanyCommandHandlerDelegationTests
{
    [Fact]
    public async Task CreateCompanyCommandHandler_Should_Call_Service()
    {
        var service = new Mock<ICompanyAdministrationService>();
        var command = new CreateCompanyCommand("Acme", null, null, null, null, null, null, null, null, null, null, CompanyType.Customer, null, null);
        var expected = new CompanyDetailDto(Guid.NewGuid(), "Acme", null, null, null, null, null, null, null, null, null, null, CompanyType.Customer, null, null, true, Array.Empty<AddressDto>(), "rv");
        service.Setup(x => x.CreateAsync(command, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var sut = new CreateCompanyCommandHandler(service.Object);
        var result = await sut.Handle(command, CancellationToken.None);

        result.Should().Be(expected);
    }
}

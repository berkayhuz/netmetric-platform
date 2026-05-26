// <copyright file="ContactCommandHandlerDelegationTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Moq;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;
using NetMetric.CRM.CustomerManagement.Application.Commands.Contacts;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerManagement.UnitTests.Commands;

public sealed class ContactCommandHandlerDelegationTests
{
    [Fact]
    public async Task CreateContactCommandHandler_Should_Call_Service()
    {
        var service = new Mock<IContactAdministrationService>();
        var command = new CreateContactCommand("Berkay", "Huz", null, "berkay@test.com", "555", null, null, null, GenderType.Male, null, null, null, null, null, null, Guid.NewGuid(), true);
        var expected = new ContactDetailDto(Guid.NewGuid(), "Berkay", "Huz", "Berkay Huz", null, "berkay@test.com", "555", null, null, null, GenderType.Male, null, null, null, null, null, null, null, null, null, true, true, "rv");
        service.Setup(x => x.CreateAsync(command, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var sut = new CreateContactCommandHandler(service.Object);
        var result = await sut.Handle(command, CancellationToken.None);

        result.Should().Be(expected);
    }
}

// <copyright file="TicketCommandHandlerDelegationTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Moq;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Services;
using NetMetric.CRM.TicketManagement.Application.Commands.Tickets;
using NetMetric.CRM.TicketManagement.Contracts.DTOs;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.TicketManagement.UnitTests.Commands;

public sealed class TicketCommandHandlerDelegationTests
{
    [Fact]
    public async Task CreateTicketHandler_Should_Delegate_To_Service()
    {
        var service = new Mock<ITicketAdministrationService>(MockBehavior.Strict);
        var command = new CreateTicketCommand(
            "Subject",
            "Description",
            TicketType.Support,
            TicketChannelType.Web,
            PriorityType.Medium,
            null, null, null, null, null, null, null, null);

        service
            .Setup(x => x.CreateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TicketDetailDto(
                Guid.NewGuid(),
                "TKT-1",
                "Subject",
                "Description",
                TicketStatusType.New,
                PriorityType.Medium,
                TicketType.Support,
                TicketChannelType.Web,
                null, null, null, null, null,
                DateTime.UtcNow, null, null, null, null, true, [], []));

        var sut = new CreateTicketCommandHandler(service.Object);

        await sut.Handle(command, CancellationToken.None);

        service.VerifyAll();
    }
}

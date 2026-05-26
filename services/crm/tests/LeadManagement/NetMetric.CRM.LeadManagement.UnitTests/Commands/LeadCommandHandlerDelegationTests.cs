// <copyright file="LeadCommandHandlerDelegationTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Moq;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Services;
using NetMetric.CRM.LeadManagement.Application.Commands.Leads;
using NetMetric.CRM.LeadManagement.Contracts.DTOs;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.LeadManagement.UnitTests.Commands;

public sealed class LeadCommandHandlerDelegationTests
{
    [Fact]
    public async Task CreateLeadHandler_Should_Delegate_To_Service()
    {
        var command = new CreateLeadCommand(
            "Ada Lovelace",
            "Analytical Engines Ltd",
            "ada@example.com",
            "555-11-22",
            "CTO",
            null,
            null,
            null,
            LeadSourceType.Manual,
            LeadStatusType.New,
            PriorityType.Medium,
            null,
            null,
            null);

        var expected = new LeadDetailDto(
            Guid.NewGuid(),
            "LEAD-001",
            "Ada Lovelace",
            "Analytical Engines Ltd",
            "ada@example.com",
            "555-11-22",
            "CTO",
            null,
            null,
            null,
            LeadSourceType.Manual,
            LeadStatusType.New,
            PriorityType.Medium,
            null,
            null,
            null,
            null,
            0m,
            0m,
            LeadGradeType.Unassigned,
            QualificationFrameworkType.None,
            null,
            null,
            null,
            false,
            null,
            null,
            null,
            null,
            true,
            [],
            [],
            Convert.ToBase64String([1, 2, 3]));

        var service = new Mock<ILeadAdministrationService>();
        service
            .Setup(x => x.CreateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = new CreateLeadCommandHandler(service.Object);

        var result = await sut.Handle(command, CancellationToken.None);

        result.Should().Be(expected);
    }
}

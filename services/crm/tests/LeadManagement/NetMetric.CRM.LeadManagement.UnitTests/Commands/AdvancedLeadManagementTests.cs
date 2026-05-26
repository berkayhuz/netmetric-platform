// <copyright file="AdvancedLeadManagementTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Moq;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Services;
using NetMetric.CRM.LeadManagement.Application.Commands.Leads;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.LeadManagement.UnitTests.Commands;

public class AdvancedLeadManagementTests
{
    private readonly Mock<ILeadCaptureService> _captureServiceMock;

    public AdvancedLeadManagementTests()
    {
        _captureServiceMock = new Mock<ILeadCaptureService>();
    }

    [Fact]
    public async Task CaptureLeadCommand_Should_Invoke_CaptureService()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        _captureServiceMock
            .Setup(s => s.CaptureAsync(It.IsAny<CaptureLeadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        var handler = new CaptureLeadCommandHandler(_captureServiceMock.Object);
        var command = new CaptureLeadCommand(
            "John Doe",
            "john@acme.com",
            null,
            "Acme",
            null,
            "Notes",
            LeadSourceType.WebForm,
            null,
            null,
            "google",
            "cpc",
            "spring_sale",
            null,
            null,
            null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedId);
        _captureServiceMock.Verify(s => s.CaptureAsync(
            It.IsAny<CaptureLeadCommand>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void UpsertLeadScoreCommand_Should_Have_Correct_Properties()
    {
        var leadId = Guid.NewGuid();
        var command = new UpsertLeadScoreCommand(leadId, 25.5m, "Meeting attended");

        command.LeadId.Should().Be(leadId);
        command.Score.Should().Be(25.5m);
        command.ScoreReason.Should().Be("Meeting attended");
    }

    [Fact]
    public void UpsertLeadQualificationCommand_Should_Have_Correct_Properties()
    {
        var leadId = Guid.NewGuid();
        var command = new UpsertLeadQualificationCommand(leadId, QualificationFrameworkType.BANT, "{\"budget\": \"100K\"}");

        command.LeadId.Should().Be(leadId);
        command.FrameworkType.Should().Be(QualificationFrameworkType.BANT);
        command.QualificationDataJson.Should().Be("{\"budget\": \"100K\"}");
    }
}

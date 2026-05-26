// <copyright file="AccountControllerCommandDispatchTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NetMetric.Account.Api.Controllers;
using NetMetric.Account.Application.Devices.Commands;
using NetMetric.Account.Application.Security.Mfa;
using NetMetric.Account.Application.Sessions.Commands;
using NetMetric.Account.Contracts.Security;

namespace NetMetric.Account.Api.UnitTests;

public sealed class AccountControllerCommandDispatchTests
{
    [Fact]
    public async Task SessionsController_Revoke_ShouldDispatchCommand()
    {
        var sessionId = Guid.NewGuid();
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.IsAny<RevokeSessionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NetMetric.Account.Application.Common.Result.Success());

        var controller = new SessionsController(mediator.Object);
        var response = await controller.Revoke(sessionId, CancellationToken.None);

        response.Should().BeOfType<NoContentResult>();
        mediator.Verify(x => x.Send(It.Is<RevokeSessionCommand>(c => c.SessionId == sessionId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TrustedDevicesController_RevokeOthers_ShouldDispatchCommand()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.IsAny<RevokeOtherTrustedDevicesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NetMetric.Account.Application.Common.Result.Success());

        var controller = new TrustedDevicesController(mediator.Object);
        var response = await controller.RevokeOthers(CancellationToken.None);

        response.Should().BeOfType<NoContentResult>();
        mediator.Verify(x => x.Send(It.IsAny<RevokeOtherTrustedDevicesCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SecurityController_ConfirmMfa_ShouldDispatchCode()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.IsAny<ConfirmMfaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NetMetric.Account.Application.Common.Result<ConfirmMfaResponse>.Success(new ConfirmMfaResponse(true, ["A"])));

        var controller = new SecurityController(mediator.Object);
        var response = await controller.ConfirmMfa(new ConfirmMfaRequest("123456"), CancellationToken.None);

        response.Result.Should().BeOfType<OkObjectResult>();
        mediator.Verify(x => x.Send(It.Is<ConfirmMfaCommand>(c => c.VerificationCode == "123456"), It.IsAny<CancellationToken>()), Times.Once);
    }
}

// <copyright file="AddTicketCommentCommandValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.TicketManagement.Application.Features.Comments.Commands.AddTicketComment;
using NetMetric.CRM.TicketManagement.Application.Validators;

namespace NetMetric.CRM.TicketManagement.UnitTests.Validators;

public sealed class AddTicketCommentCommandValidatorTests
{
    private readonly AddTicketCommentCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Fail_For_Empty_Comment()
    {
        var result = _validator.Validate(new AddTicketCommentCommand(Guid.NewGuid(), "", false));
        result.IsValid.Should().BeFalse();
    }
}

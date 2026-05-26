// <copyright file="UpdateNoteCommandValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.CustomerManagement.Application.Features.Notes.Commands.UpdateNote;
using NetMetric.CRM.CustomerManagement.Application.Validators;

namespace NetMetric.CRM.CustomerManagement.Tests.Validators;

public sealed class UpdateNoteCommandValidatorTests
{
    [Fact]
    public void Should_Fail_When_Title_Is_Empty()
    {
        var validator = new UpdateNoteCommandValidator();
        var result = validator.Validate(new UpdateNoteCommand
        {
            NoteId = Guid.NewGuid(),
            Title = string.Empty,
            Content = "content"
        });

        Assert.False(result.IsValid);
    }
}

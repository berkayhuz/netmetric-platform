// <copyright file="ScheduleMeetingCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Meetings.ScheduleMeeting;

public sealed class ScheduleMeetingCommandValidator : AbstractValidator<ScheduleMeetingCommand>
{
    public ScheduleMeetingCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(160);
        RuleFor(x => x.OrganizerEmail).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.AttendeeSummary).MaximumLength(2000);
        RuleFor(x => x.StartsAtUtc).GreaterThan(DateTime.UtcNow.AddMinutes(-1));
        RuleFor(x => x.EndsAtUtc).GreaterThan(x => x.StartsAtUtc);
    }
}

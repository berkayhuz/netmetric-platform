// <copyright file="UpdateMyProfileCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using MediatR;
using NetMetric.Account.Application.Abstractions.Audit;
using NetMetric.Account.Application.Abstractions.Outbox;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;
using NetMetric.Account.Application.Profiles.Queries;
using NetMetric.Account.Contracts.Profiles;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Profiles;
using NetMetric.Clock;
using NetMetric.Localization;

namespace NetMetric.Account.Application.Profiles.Commands;

public sealed record UpdateMyProfileCommand(UpdateMyProfileRequest Request) : IRequest<Result<MyProfileResponse>>;

public sealed class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.Request.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.PhoneCountryIso2).MaximumLength(2);
        RuleFor(x => x.Request.PhoneNationalNumber).MaximumLength(32);
        RuleFor(x => x.Request.AvatarUrl)
            .Must(string.IsNullOrWhiteSpace)
            .WithMessage("Avatar URL is managed by the avatar upload endpoints.");
        RuleFor(x => x.Request.JobTitle).MaximumLength(120);
        RuleFor(x => x.Request.Department).MaximumLength(120);
        RuleFor(x => x.Request.TimeZone).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.TimeZone)
            .Must(TimeZoneNormalizer.IsValid)
            .WithMessage("Time zone must be a valid system time zone identifier.");
        RuleFor(x => x.Request.Culture)
            .NotEmpty()
            .MaximumLength(20)
            .Must(NetMetricCultures.IsSupportedCulture)
            .WithMessage("Culture must be a valid BCP-47 culture tag (example: en, en-US, tr, zh-CN).");
    }
}

public sealed class UpdateMyProfileCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IClock clock,
    IRepository<IAccountDbContext, UserProfile> profiles,
    IAccountDbContext dbContext,
    IConcurrencyTokenWriter concurrencyTokenWriter,
    IAccountAuditWriter auditWriter,
    IAccountOutboxWriter outboxWriter)
    : IRequestHandler<UpdateMyProfileCommand, Result<MyProfileResponse>>
{
    public async Task<Result<MyProfileResponse>> Handle(UpdateMyProfileCommand command, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var tenantId = TenantId.From(currentUser.TenantId);
        var userId = UserId.From(currentUser.UserId);

        var profile = await profiles.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, cancellationToken);

        if (profile is null)
        {
            profile = UserProfile.Create(tenantId, userId, command.Request.FirstName, command.Request.LastName, clock.UtcNow, command.Request.Culture);
            await profiles.AddAsync(profile, cancellationToken);
        }

        var version = VersionEncoding.TryDecode(command.Request.Version);
        if (version is not null)
        {
            concurrencyTokenWriter.SetOriginalVersion(profile, version);
        }

        var effectiveTimeZone = TimeZoneNormalizer.NormalizeOrDefault(command.Request.TimeZone);
        var normalizedPhone = PhoneNumberNormalizer.Normalize(command.Request.PhoneCountryIso2, command.Request.PhoneNationalNumber);
        if (!string.IsNullOrWhiteSpace(command.Request.PhoneCountryIso2) || !string.IsNullOrWhiteSpace(command.Request.PhoneNationalNumber))
        {
            if (normalizedPhone is null)
            {
                return Result<MyProfileResponse>.Failure(Error.Validation("Phone number is invalid for the selected country."));
            }
        }
        profile.Update(
            command.Request.FirstName,
            command.Request.LastName,
            normalizedPhone,
            command.Request.JobTitle,
            command.Request.Department,
            effectiveTimeZone,
            command.Request.Culture,
            clock.UtcNow);

        await dbContext.SaveChangesAsync(cancellationToken);

        await outboxWriter.EnqueueAsync(
            currentUser.TenantId,
            AccountOutboxEventTypes.ProfileUpdated,
            new AccountProfileUpdatedEvent(
                1,
                currentUser.TenantId,
                currentUser.UserId,
                currentUser.CorrelationId,
                clock.UtcNow,
                ["firstName", "lastName", "phoneNumber", "jobTitle", "department", "timeZone", "culture"]),
            currentUser.CorrelationId,
            cancellationToken);

        await auditWriter.WriteAsync(
            new AccountAuditWriteRequest(
                currentUser.TenantId,
                currentUser.UserId,
                AccountAuditEventTypes.ProfileUpdated,
                "Information",
                currentUser.CorrelationId,
                currentUser.IpAddress,
                currentUser.UserAgent,
                null),
            cancellationToken);

        return Result<MyProfileResponse>.Success(ProfileMapper.ToResponse(profile));
    }
}

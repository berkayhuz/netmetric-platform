// <copyright file="GetMyProfileQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;
using NetMetric.Account.Application.Abstractions.Identity;
using NetMetric.Account.Contracts.Profiles;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Profiles;
using NetMetric.Clock;
using NetMetric.Localization;

namespace NetMetric.Account.Application.Profiles.Queries;

public sealed record GetMyProfileQuery : IRequest<Result<MyProfileResponse>>;

public sealed class GetMyProfileQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IRepository<IAccountDbContext, UserProfile> profiles,
    IAccountDbContext dbContext,
    IClock clock,
    IIdentityAccountClient identityAccountClient)
    : IRequestHandler<GetMyProfileQuery, Result<MyProfileResponse>>
{
    public async Task<Result<MyProfileResponse>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var tenantId = TenantId.From(currentUser.TenantId);
        var userId = UserId.From(currentUser.UserId);

        var profile = await profiles.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, cancellationToken);
        if (profile is null)
        {
            var (firstName, lastName) = await ProfileBootstrapNameResolver.ResolveAsync(
                identityAccountClient,
                currentUser.TenantId,
                currentUser.UserId,
                cancellationToken);
            profile = UserProfile.Create(tenantId, userId, firstName, lastName, clock.UtcNow);
            await profiles.AddAsync(profile, cancellationToken);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                profile = await profiles.Query
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, cancellationToken);
            }
        }
        else if (ProfileBootstrapNameResolver.IsPlaceholderName(profile.FirstName, profile.LastName))
        {
            var (firstName, lastName) = await ProfileBootstrapNameResolver.ResolveAsync(
                identityAccountClient,
                currentUser.TenantId,
                currentUser.UserId,
                cancellationToken);

            if (!ProfileBootstrapNameResolver.IsPlaceholderName(firstName, lastName))
            {
                profile.Update(
                    firstName,
                    lastName,
                    profile.PhoneNumber,
                    profile.JobTitle,
                    profile.Department,
                    profile.TimeZone,
                    profile.Culture,
                    clock.UtcNow);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        if (profile is null)
        {
            return Result<MyProfileResponse>.Failure(Error.NotFound("Profile"));
        }

        return Result<MyProfileResponse>.Success(ProfileMapper.ToResponse(profile));
    }
}

internal static class ProfileMapper
{
    public static MyProfileResponse ToResponse(UserProfile profile)
    {
        var (iso2, callingCode, nationalNumber) = PhoneNumberNormalizer.Split(profile.PhoneNumber);
        var culture = NetMetricCultures.NormalizeOrDefault(profile.Culture);
        return new(
            profile.Id,
            profile.TenantId.Value,
            profile.UserId.Value,
            profile.FirstName,
            profile.LastName,
            profile.DisplayName,
            profile.PhoneNumber,
            iso2,
            callingCode,
            nationalNumber,
            profile.AvatarUrl,
            profile.JobTitle,
            profile.Department,
            TimeZoneNormalizer.NormalizeOrDefault(profile.TimeZone),
            culture,
            VersionEncoding.Encode(profile.Version));
    }
}

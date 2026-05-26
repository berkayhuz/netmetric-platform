// <copyright file="GetMyPreferencesQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;
using NetMetric.Account.Contracts.Preferences;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Preferences;
using NetMetric.Clock;
using NetMetric.Localization;

namespace NetMetric.Account.Application.Preferences.Queries;

public sealed record GetMyPreferencesQuery : IRequest<Result<UserPreferenceResponse>>;

public sealed class GetMyPreferencesQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IRepository<IAccountDbContext, UserPreference> preferences,
    IAccountDbContext dbContext,
    IClock clock)
    : IRequestHandler<GetMyPreferencesQuery, Result<UserPreferenceResponse>>
{
    public async Task<Result<UserPreferenceResponse>> Handle(GetMyPreferencesQuery request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var tenantId = TenantId.From(currentUser.TenantId);
        var userId = UserId.From(currentUser.UserId);

        var preference = await preferences.Query
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, cancellationToken);

        if (preference is null)
        {
            preference = UserPreference.CreateDefault(tenantId, userId, clock.UtcNow);
            await preferences.AddAsync(preference, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result<UserPreferenceResponse>.Success(PreferenceMapper.ToResponse(preference));
    }
}

internal static class PreferenceMapper
{
    private const string DefaultDateFormat = "yyyy-MM-dd";

    public static UserPreferenceResponse ToResponse(UserPreference preference)
    {
        var language = NetMetricCultures.NormalizeOrDefault(preference.Language);
        var dateFormat = AccountOptionsCatalog.GetDateFormats().Any(x => string.Equals(x.Value, preference.DateFormat, StringComparison.Ordinal))
            ? preference.DateFormat
            : DefaultDateFormat;

        var theme = preference.Theme == ThemePreference.Default
            ? ThemePreference.System
            : preference.Theme;

        return new(
            preference.Id,
            theme.ToString(),
            language,
            TimeZoneNormalizer.NormalizeOrDefault(preference.TimeZone),
            dateFormat,
            preference.PostLoginDestination.ToString(),
            preference.DefaultOrganizationId,
            preference.FaviconUrl,
            preference.CrmDashboardPreferencesJson,
            VersionEncoding.Encode(preference.Version));
    }
}

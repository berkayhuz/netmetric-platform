// <copyright file="AcceptConsentCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using MediatR;
using NetMetric.Account.Application.Abstractions.Audit;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Consents;
using NetMetric.Clock;

namespace NetMetric.Account.Application.Consents.Commands;

public sealed record AcceptConsentCommand(string ConsentType, string Version) : IRequest<Result>;

public sealed class AcceptConsentCommandValidator : AbstractValidator<AcceptConsentCommand>
{
    public AcceptConsentCommandValidator()
    {
        RuleFor(x => x.ConsentType).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Version).NotEmpty().MaximumLength(40);
    }
}

public sealed class AcceptConsentCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IClock clock,
    IRepository<IAccountDbContext, UserConsent> consents,
    IAccountDbContext dbContext,
    IAccountAuditWriter auditWriter)
    : IRequestHandler<AcceptConsentCommand, Result>
{
    public async Task<Result> Handle(AcceptConsentCommand command, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var consent = UserConsent.Accept(
            TenantId.From(currentUser.TenantId),
            UserId.From(currentUser.UserId),
            command.ConsentType,
            command.Version,
            clock.UtcNow,
            currentUser.IpAddress,
            currentUser.UserAgent);

        await consents.AddAsync(consent, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditWriter.WriteAsync(
            new AccountAuditWriteRequest(
                currentUser.TenantId,
                currentUser.UserId,
                AccountAuditEventTypes.ConsentAccepted,
                "Information",
                currentUser.CorrelationId,
                currentUser.IpAddress,
                currentUser.UserAgent,
                new Dictionary<string, string>
                {
                    ["consentType"] = command.ConsentType,
                    ["version"] = command.Version
                }),
            cancellationToken);

        return Result.Success();
    }
}

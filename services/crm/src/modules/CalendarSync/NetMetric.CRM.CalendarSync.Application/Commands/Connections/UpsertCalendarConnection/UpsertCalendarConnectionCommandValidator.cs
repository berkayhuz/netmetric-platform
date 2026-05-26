// <copyright file="UpsertCalendarConnectionCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.Security.EndpointGuard;

namespace NetMetric.CRM.CalendarSync.Application.Commands.Connections.UpsertCalendarConnection;

public sealed class UpsertCalendarConnectionCommandValidator : AbstractValidator<UpsertCalendarConnectionCommand>
{
    public UpsertCalendarConnectionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.CalendarIdentifier).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SecretReference)
            .NotEmpty()
            .MaximumLength(300)
            .Must(ExternalEndpointGuard.IsSecretReference)
            .WithMessage("SecretReference must be a secret manager reference such as secret://, vault://, keyvault://, azure-keyvault://, aws-secretsmanager://, or gcp-secretmanager://.");
        RuleFor(x => x.Provider).IsInEnum();
        RuleFor(x => x.SyncDirection).IsInEnum();
    }
}

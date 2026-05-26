// <copyright file="CreateChannelAccountCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.Security.EndpointGuard;

namespace NetMetric.CRM.Omnichannel.Application.Commands.Accounts.CreateChannelAccount;

public sealed class CreateChannelAccountCommandValidator : AbstractValidator<CreateChannelAccountCommand>
{
    public CreateChannelAccountCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.ExternalAccountId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SecretReference)
            .NotEmpty()
            .MaximumLength(300)
            .Must(ExternalEndpointGuard.IsSecretReference)
            .WithMessage("SecretReference must be a secret manager reference such as secret://, vault://, keyvault://, azure-keyvault://, aws-secretsmanager://, or gcp-secretmanager://.");
        RuleFor(x => x.RoutingKey).NotEmpty().MaximumLength(120);
        RuleFor(x => x.ChannelType).IsInEnum();
    }
}

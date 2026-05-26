// <copyright file="CreateSupportInboxConnectionCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Net;
using FluentValidation;
using NetMetric.CRM.SupportInboxIntegration.Application.Commands.Connections.CreateSupportInboxConnection;
using NetMetric.Security.EndpointGuard;

namespace NetMetric.CRM.SupportInboxIntegration.Application.Validators;

public sealed class CreateSupportInboxConnectionCommandValidator : AbstractValidator<CreateSupportInboxConnectionCommand>
{
    public CreateSupportInboxConnectionCommandValidator(Func<string, IPAddress[]>? resolveHost = null)
    {
        resolveHost ??= Dns.GetHostAddresses;

        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EmailAddress).NotEmpty().EmailAddress();
        RuleFor(x => x.Host)
            .NotEmpty()
            .MaximumLength(200)
            .Must(host => ExternalEndpointGuard.IsPublicDnsHost(host, resolveHost))
            .WithMessage("Host must be a public DNS host and cannot point to local or private network hosts.");
        RuleFor(x => x.Port).Must(port => port is 443 or 993 or 995);
        RuleFor(x => x.UseSsl).Equal(true);
        RuleFor(x => x.Username).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SecretReference)
            .NotEmpty()
            .MaximumLength(300)
            .Must(ExternalEndpointGuard.IsSecretReference)
            .WithMessage("SecretReference must be a secret manager reference such as secret://, vault://, keyvault://, azure-keyvault://, aws-secretsmanager://, or gcp-secretmanager://.");
    }
}

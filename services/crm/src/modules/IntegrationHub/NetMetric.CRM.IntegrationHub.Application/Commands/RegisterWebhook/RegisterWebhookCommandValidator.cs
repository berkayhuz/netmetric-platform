// <copyright file="RegisterWebhookCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Net;
using FluentValidation;
using NetMetric.Security.EndpointGuard;

namespace NetMetric.CRM.IntegrationHub.Application.Commands.RegisterWebhook;

public sealed class RegisterWebhookCommandValidator : AbstractValidator<RegisterWebhookCommand>
{
    public RegisterWebhookCommandValidator(Func<string, IPAddress[]>? resolveHost = null)
    {
        resolveHost ??= Dns.GetHostAddresses;

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(180);

        RuleFor(x => x.EventKey)
            .NotEmpty()
            .MaximumLength(100)
            .Matches("^[a-z0-9._:-]+$")
            .WithMessage("EventKey may only contain lowercase letters, numbers, dots, underscores, colons, and hyphens.");

        RuleFor(x => x.SecretKey)
            .NotEmpty()
            .MinimumLength(32)
            .MaximumLength(256);

        RuleFor(x => x.TargetUrl)
            .NotEmpty()
            .MaximumLength(2048)
            .Must(targetUrl => ExternalEndpointGuard.IsTrustedHttpsEndpoint(targetUrl, resolveHost))
            .WithMessage("TargetUrl must be a trusted HTTPS endpoint and cannot point to local or private network hosts.");

        RuleFor(x => x.TimeoutSeconds)
            .InclusiveBetween(1, 30);

        RuleFor(x => x.MaxAttempts)
            .InclusiveBetween(1, 10);
    }
}

// <copyright file="FakeEmailSender.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.TestKit.Fakes;

public sealed class FakeEmailSender
{
    private readonly List<EmailEnvelope> _messages = [];

    public IReadOnlyCollection<EmailEnvelope> Messages => _messages;

    public void Send(string to, string subject, string body)
    {
        _messages.Add(new EmailEnvelope(to, subject, body));
    }

    public sealed record EmailEnvelope(string To, string Subject, string Body);
}


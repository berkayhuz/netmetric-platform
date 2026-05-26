// <copyright file="CalendarConnection.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.CalendarSync.Domain.Enums;
using NetMetric.Entities;

namespace NetMetric.CRM.CalendarSync.Domain.Entities;

public sealed class CalendarConnection : AuditableEntity
{
    private CalendarConnection()
    {
    }

    public CalendarConnection(string name, CalendarProviderType provider, string calendarIdentifier, string secretReference, CalendarSyncDirection syncDirection)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Connection name is required.", nameof(name)) : name.Trim();
        Provider = provider;
        CalendarIdentifier = string.IsNullOrWhiteSpace(calendarIdentifier) ? throw new ArgumentException("Calendar identifier is required.", nameof(calendarIdentifier)) : calendarIdentifier.Trim();
        SecretReference = string.IsNullOrWhiteSpace(secretReference) ? throw new ArgumentException("Secret reference is required.", nameof(secretReference)) : secretReference.Trim();
        SyncDirection = syncDirection;
    }

    public string Name { get; private set; } = null!;
    public CalendarProviderType Provider { get; private set; }
    public string CalendarIdentifier { get; private set; } = null!;
    public string SecretReference { get; private set; } = null!;
    public CalendarSyncDirection SyncDirection { get; private set; }

    public void Update(string name, string calendarIdentifier, string secretReference, CalendarSyncDirection syncDirection, bool isActive)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Connection name is required.", nameof(name)) : name.Trim();
        CalendarIdentifier = string.IsNullOrWhiteSpace(calendarIdentifier) ? throw new ArgumentException("Calendar identifier is required.", nameof(calendarIdentifier)) : calendarIdentifier.Trim();
        SecretReference = string.IsNullOrWhiteSpace(secretReference) ? throw new ArgumentException("Secret reference is required.", nameof(secretReference)) : secretReference.Trim();
        SyncDirection = syncDirection;
        SetActive(isActive);
    }
}

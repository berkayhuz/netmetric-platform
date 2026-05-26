// <copyright file="AddressUpsertRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerManagement.Contracts.Requests;

public sealed class AddressUpsertRequest
{
    public AddressType AddressType { get; set; } = AddressType.Other;
    public string Line1 { get; set; } = null!;
    public string? Line2 { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? ZipCode { get; set; }
    public bool IsDefault { get; set; }
    public string? RowVersion { get; set; }
}

// <copyright file="DocumentManagementPermissions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.DocumentManagement.Application.Security;

public static class DocumentManagementPermissions
{
    public const string DocumentsRead = "documents.read";
    public const string DocumentsManage = "documents.manage";
    public const string VersionsManage = "documents.versions.manage";
    public const string ApprovalsManage = "documents.approvals.manage";
}

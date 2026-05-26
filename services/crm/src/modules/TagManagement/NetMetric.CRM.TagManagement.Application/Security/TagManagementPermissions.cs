// <copyright file="TagManagementPermissions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.TagManagement.Application.Security;

public static class TagManagementPermissions
{
    public const string TagsRead = "tags.read";
    public const string TagsManage = "tags.manage";
    public const string TagGroupsManage = "tags.groups.manage";
    public const string SmartLabelsManage = "tags.smart-labels.manage";
    public const string ClassificationsManage = "tags.classifications.manage";
}

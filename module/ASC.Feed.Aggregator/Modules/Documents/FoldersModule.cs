/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Files.Core;
using ASC.Files.Core.Data;
using ASC.Files.Core.Security;
using ASC.Web.Core;
using ASC.Web.Core.Files;

namespace ASC.Feed.Aggregator.Modules.Documents
{
    internal class FoldersModule : FeedModule
    {
        private const string folderItem = "folder";
        private const string sharedFolderItem = "sharedFolder";


        protected override string Table
        {
            get { return "files_folder"; }
        }

        protected override string LastUpdatedColumn
        {
            get { return "create_on"; }
        }

        protected override string TenantColumn
        {
            get { return "tenant_id"; }
        }

        protected override string DbId
        {
            get { return Constants.FilesDbId; }
        }


        public override string Name
        {
            get { return Constants.FoldersModule; }
        }

        public override string Product
        {
            get { return ModulesHelper.DocumentsProductName; }
        }

        public override Guid ProductID
        {
            get { return ModulesHelper.DocumentsProductID; }
        }

        public override bool VisibleFor(Feed feed, object data, Guid userId)
        {
            if (!WebItemSecurity.IsAvailableForUser(ProductID.ToString(), userId)) return false;

            var folder = (FileEntry)data;

            bool targetCond;
            if (feed.Target != null)
            {
                if (!string.IsNullOrEmpty(folder.SharedToMeBy) && folder.SharedToMeBy == userId.ToString())
                    return false;

                var owner = new Guid((string)feed.Target);
                var groupUsers = CoreContext.UserManager.GetUsersByGroup(owner).Select(x => x.ID).ToList();
                if (!groupUsers.Any())
                {
                    groupUsers.Add(owner);
                }
                targetCond = groupUsers.Contains(userId);
            }
            else
            {
                targetCond = true;
            }

            return targetCond &&
                   new FileSecurity(new DaoFactory()).CanRead(folder, userId);
        }

        public override IEnumerable<int> GetTenantsWithFeeds(DateTime fromTime)
        {
            var q1 = new SqlQuery("files_folder")
                .Select("tenant_id")
                .Where(Exp.Gt("modified_on", fromTime))
                .GroupBy(1)
                .Having(Exp.Gt("count(*)", 0));

            var q2 = new SqlQuery("files_security")
                .Select("tenant_id")
                .Where(Exp.Gt("timestamp", fromTime))
                .GroupBy(1)
                .Having(Exp.Gt("count(*)", 0));

            using (var db = new DbManager(DbId))
            {
                return db.ExecuteList(q1).ConvertAll(r => Convert.ToInt32(r[0]))
                         .Union(db.ExecuteList(q2).ConvertAll(r => Convert.ToInt32(r[0])));
            }
        }

        public override IEnumerable<Tuple<Feed, object>> GetFeeds(FeedFilter filter)
        {
            var q1 = new SqlQuery("files_folder f")
                .Select(FolderColumns().Select(f => "f." + f).ToArray())
                .Select(DocumentsDbHelper.GetRootFolderType("parent_id"))
                .Select("null, null, null")
                .Where(
                    Exp.Eq("f.tenant_id", filter.Tenant) &
                    Exp.Eq("f.folder_type", 0) &
                    Exp.Between("f.create_on", filter.Time.From, filter.Time.To)
                );

            var q2 = new SqlQuery("files_folder f")
                .LeftOuterJoin("files_security s",
                               Exp.EqColumns("s.entry_id", "f.id") &
                               Exp.Eq("s.tenant_id", filter.Tenant) &
                               Exp.Eq("s.entry_type", (int)FileEntryType.Folder)
                )
                .Select(FolderColumns().Select(f => "f." + f).ToArray())
                .Select(DocumentsDbHelper.GetRootFolderType("parent_id"))
                .Select("s.timestamp, s.owner, s.subject")
                .Where(
                    Exp.Eq("f.tenant_id", filter.Tenant) &
                    Exp.Eq("f.folder_type", 0) &
                    Exp.Lt("s.security", 3) &
                    Exp.Between("s.timestamp", filter.Time.From, filter.Time.To)
                );

            using (var db = new DbManager(DbId))
            {
                var folders = db.ExecuteList(q1.UnionAll(q2)).ConvertAll(ToFolder);
                return folders
                    .Where(f => f.RootFolderType != FolderType.TRASH && f.RootFolderType != FolderType.BUNCH)
                    .Select(f => new Tuple<Feed, object>(ToFeed(f), f));
            }
        }


        private static IEnumerable<string> FolderColumns()
        {
            return new[]
                {
                    "id",
                    "parent_id",
                    "title",
                    "create_by",
                    "create_on",
                    "modified_by",
                    "modified_on",
                    "foldersCount",
                    "filesCount" // 8
                };
        }

        private static Folder ToFolder(object[] r)
        {
            return new Folder
                {
                    ID = Convert.ToInt32(r[0]),
                    ParentFolderID = Convert.ToInt32(r[1]),
                    Title = Convert.ToString(r[2]),
                    CreateBy = new Guid(Convert.ToString(r[3])),
                    CreateOn = Convert.ToDateTime(r[4]),
                    ModifiedBy = new Guid(Convert.ToString(r[5])),
                    ModifiedOn = Convert.ToDateTime(r[6]),
                    TotalSubFolders = Convert.ToInt32(r[7]),
                    TotalFiles = Convert.ToInt32(r[8]),
                    RootFolderType = DocumentsDbHelper.ParseRootFolderType(r[9]),
                    RootFolderCreator = DocumentsDbHelper.ParseRootFolderCreator(r[9]),
                    RootFolderId = DocumentsDbHelper.ParseRootFolderId(r[9]),
                    SharedToMeOn = r[10] != null ? Convert.ToDateTime(r[10]) : (DateTime?)null,
                    SharedToMeBy = r[11] != null ? Convert.ToString(r[11]) : null,
                    // here stored subject of the folder share 
                    CreateByString = r[12] != null ? Convert.ToString(r[12]) : null
                };
        }

        private Feed ToFeed(Folder folder)
        {
            var rootFolder = new FolderDao(Tenant, DbId).GetFolder(folder.ParentFolderID);

            if (folder.SharedToMeOn.HasValue)
            {
                var feed = new Feed(new Guid(folder.SharedToMeBy), folder.SharedToMeOn.Value, true)
                    {
                        Item = sharedFolderItem,
                        ItemId = string.Format("{0}_{1}", folder.ID, folder.CreateByString),
                        ItemUrl = FilesLinkUtility.GetFileRedirectPreviewUrl(folder.ID, false),
                        Product = Product,
                        Module = Name,
                        Title = folder.Title,
                        ExtraLocation = rootFolder.FolderType == FolderType.DEFAULT ? rootFolder.Title : string.Empty,
                        ExtraLocationUrl = rootFolder.FolderType == FolderType.DEFAULT ? FilesLinkUtility.GetFileRedirectPreviewUrl(folder.ParentFolderID, false) : string.Empty,
                        Keywords = string.Format("{0}", folder.Title),
                        HasPreview = false,
                        CanComment = false,
                        Target = folder.CreateByString,
                        GroupId = GetGroupId(sharedFolderItem, new Guid(folder.SharedToMeBy), folder.ParentFolderID.ToString())
                    };

                return feed;
            }

            return new Feed(folder.CreateBy, folder.CreateOn)
                {
                    Item = folderItem,
                    ItemId = folder.ID.ToString(),
                    ItemUrl = FilesLinkUtility.GetFileRedirectPreviewUrl(folder.ID, false),
                    Product = Product,
                    Module = Name,
                    Title = folder.Title,
                    ExtraLocation = rootFolder.FolderType == FolderType.DEFAULT ? rootFolder.Title : string.Empty,
                    ExtraLocationUrl = rootFolder.FolderType == FolderType.DEFAULT ? FilesLinkUtility.GetFileRedirectPreviewUrl(folder.ParentFolderID, false) : string.Empty,
                    Keywords = string.Format("{0}", folder.Title),
                    HasPreview = false,
                    CanComment = false,
                    Target = null,
                    GroupId = GetGroupId(folderItem, folder.CreateBy, folder.ParentFolderID.ToString())
                };
        }
    }
}
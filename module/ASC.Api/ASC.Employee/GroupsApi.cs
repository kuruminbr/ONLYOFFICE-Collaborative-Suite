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
using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Exceptions;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Api.Utils;
using ASC.Core;
using ASC.Core.Users;
using ASC.MessagingSystem;

namespace ASC.Api.Employee
{
    ///<summary>
    ///Groups access
    ///</summary>
    public class GroupsApi : IApiEntryPoint
    {
        private readonly ApiContext _context;

        public string Name
        {
            get { return "group"; }
        }

        public GroupsApi(ApiContext context)
        {
            _context = context;
        }

        ///<summary>
        ///Returns the general information about all groups, such as group ID and group manager
        ///</summary>
        ///<short>
        ///All groups
        ///</short>
        ///<returns>List of groups</returns>
        /// <remarks>
        /// This method returns partial group info
        /// </remarks>
        [Read("")]
        public IEnumerable<GroupWrapperSummary> GetAll()
        {
            return CoreContext.UserManager.GetDepartments().Select(x => new GroupWrapperSummary(x)).ToSmartList();
        }

        ///<summary>
        ///Returns the detailed information about the selected group: group name, category, description, manager, users and parent group if any
        ///</summary>
        ///<short>
        ///Specific group
        ///</short>
        ///<param name="groupid">Group ID</param>
        ///<returns>Group</returns>
        /// <remarks>
        /// That method returns full group info
        /// </remarks>
        [Read("{groupid}")]
        public GroupWrapperFull GetById(Guid groupid)
        {
            return new GroupWrapperFull(GetGroupInfo(groupid), true);
        }

        ///<summary>
        ///Returns the group list for user
        ///</summary>
        ///<short>
        ///User groups
        ///</short>
        ///<param name="userid">User ID</param>
        ///<returns>Group</returns>
        /// <remarks>
        /// That method returns user groups
        /// </remarks>
        [Read("user/{userid}")]
        public IEnumerable<GroupWrapperSummary> GetByUserId(Guid userid)
        {
            return CoreContext.UserManager.GetUserGroups(userid).Select(x => new GroupWrapperSummary(x)).ToSmartList();
        }

        /// <summary>
        /// Adds a new group with the group manager, name and users specified
        /// </summary>
        /// <short>
        /// Add new group
        /// </short>
        /// <param name="groupManager">Group manager</param>
        /// <param name="groupName">Group name</param>
        /// <param name="members">List of group users</param>
        /// <returns>Newly created group</returns>
        [Create("")]
        public GroupWrapperFull AddGroup(Guid groupManager, string groupName, IEnumerable<Guid> members)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditGroups, Core.Users.Constants.Action_AddRemoveUser);

            var group = CoreContext.GroupManager.SaveGroupInfo(new GroupInfo {Name = groupName});

            TransferUserToDepartment(groupManager, @group, true);
            if (members != null)
            {
                foreach (var member in members)
                {
                    TransferUserToDepartment(member, group, false);
                }
            }

            MessageService.Send(_context, MessageAction.GroupCreated, group.Name);

            return GetById(group.ID);
        }

        /// <summary>
        /// Updates an existing group changing the group manager, name and/or users
        /// </summary>
        /// <short>
        /// Update existing group
        /// </short>
        /// <param name="groupid">Group ID</param>
        /// <param name="groupManager">Group manager</param>
        /// <param name="groupName">Group name</param>
        /// <param name="members">List of group users</param>
        /// <returns>Newly created group</returns>
        [Update("{groupid}")]
        public GroupWrapperFull UpdateGroup(Guid groupid, Guid groupManager, string groupName, IEnumerable<Guid> members)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditGroups, Core.Users.Constants.Action_AddRemoveUser);
            var group = CoreContext.GroupManager.GetGroups().SingleOrDefault(x => x.ID == groupid).NotFoundIfNull("group not found");
            if (group.ID == Core.Users.Constants.LostGroupInfo.ID)
            {
                throw new ItemNotFoundException("group not found");
            }

            group.Name = groupName ?? group.Name;
            CoreContext.GroupManager.SaveGroupInfo(group);

            RemoveMembersFrom(groupid, CoreContext.UserManager.GetUsersByGroup(groupid).Select(u => u.ID).Where(id => !members.Contains(id)));

            TransferUserToDepartment(groupManager, @group, true);
            if (members != null)
            {
                foreach (var member in members)
                {
                    TransferUserToDepartment(member, group, false);
                }
            }

            MessageService.Send(_context, MessageAction.GroupUpdated, group.Name);

            return GetById(groupid);
        }

        /// <summary>
        /// Deletes the selected group from the list of groups on the portal
        /// </summary>
        /// <short>
        /// Delete group
        /// </short>
        /// <param name="groupid">Group ID</param>
        /// <returns></returns>
        [Delete("{groupid}")]
        public GroupWrapperFull DeleteGroup(Guid groupid)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditGroups, Core.Users.Constants.Action_AddRemoveUser);
            var @group = GetGroupInfo(groupid);
            var groupWrapperFull = new GroupWrapperFull(group, false);

            CoreContext.GroupManager.DeleteGroup(groupid);

            MessageService.Send(_context, MessageAction.GroupDeleted, group.Name);

            return groupWrapperFull;
        }

        private static GroupInfo GetGroupInfo(Guid groupid)
        {
            var group =
                CoreContext.GroupManager.GetGroups().SingleOrDefault(x => x.ID == groupid).NotFoundIfNull(
                    "group not found");
            if (group.ID == Core.Users.Constants.LostGroupInfo.ID)
                throw new ItemNotFoundException("group not found");
            return @group;
        }

        /// <summary>
        /// Move all the users from the selected group to another one specified
        /// </summary>
        /// <short>
        /// Move group users
        /// </short>
        /// <param name="groupid">ID of group to move from</param>
        /// <param name="newgroupid">ID of group to move to</param>
        /// <returns>Group info which users were moved</returns>
        [Update("{groupid}/members/{newgroupid}")]
        public GroupWrapperFull TransferMembersTo(Guid groupid, Guid newgroupid)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditGroups, Core.Users.Constants.Action_AddRemoveUser);
            var oldgroup = GetGroupInfo(groupid);

            var newgroup = GetGroupInfo(newgroupid);

            var users = CoreContext.UserManager.GetUsersByGroup(oldgroup.ID);
            foreach (var userInfo in users)
            {
                TransferUserToDepartment(userInfo.ID, newgroup, false);
            }
            return GetById(newgroupid);
        }

        /// <summary>
        /// Manages the group users deleting the current users and setting new ones instead
        /// </summary>
        /// <short>
        /// Set group users
        /// </short>
        /// <param name="groupid">Group ID</param>
        /// <param name="members">User list</param>
        /// <returns></returns>
        [Create("{groupid}/members")]
        public GroupWrapperFull SetMembersTo(Guid groupid, IEnumerable<Guid> members)
        {
            RemoveMembersFrom(groupid, CoreContext.UserManager.GetUsersByGroup(groupid).Select(x => x.ID));
            AddMembersTo(groupid, members);
            return GetById(groupid);
        }

        /// <summary>
        /// Add new group users keeping the current users and adding new ones
        /// </summary>
        /// <short>
        /// Add group users
        /// </short>
        /// <param name="groupid">Group ID</param>
        /// <param name="members">User list</param>
        /// <returns></returns>
        [Update("{groupid}/members")]
        public GroupWrapperFull AddMembersTo(Guid groupid, IEnumerable<Guid> members)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditGroups, Core.Users.Constants.Action_AddRemoveUser);
            var group = GetGroupInfo(groupid);

            foreach (var userId in members)
            {
                TransferUserToDepartment(userId, group, false);
            }
            return GetById(group.ID);
        }

        /// <summary>
        /// Sets the user with the ID sent as a manager to the selected group
        /// </summary>
        /// <short>
        /// Set group manager
        /// </short>
        /// <param name="groupid">Group ID</param>
        /// <param name="userid">User ID to become manager</param>
        /// <returns></returns>
        /// <exception cref="ItemNotFoundException"></exception>
        [Update("{groupid}/manager")]
        public GroupWrapperFull SetManager(Guid groupid, Guid userid)
        {
            var group = GetGroupInfo(groupid);
            if (CoreContext.UserManager.UserExists(userid))
            {
                CoreContext.UserManager.SetDepartmentManager(group.ID, userid);
            }
            else
            {
                throw new ItemNotFoundException("user not found");
            }
            return GetById(groupid);
        }

        /// <summary>
        /// Removes the specified group users with all the rest current group users retained
        /// </summary>
        /// <short>
        /// Remove group users
        /// </short>
        /// <param name="groupid">Group ID</param>
        /// <param name="members">User list</param>
        /// <returns></returns>
        [Delete("{groupid}/members")]
        public GroupWrapperFull RemoveMembersFrom(Guid groupid, IEnumerable<Guid> members)
        {
            SecurityContext.DemandPermissions(Core.Users.Constants.Action_EditGroups, Core.Users.Constants.Action_AddRemoveUser);
            var group = GetGroupInfo(groupid);

            foreach (var userId in members)
            {
                RemoveUserFromDepartment(userId, group);
            }
            return GetById(group.ID);
        }

        private static void RemoveUserFromDepartment(Guid userId, GroupInfo @group)
        {
            if (!CoreContext.UserManager.UserExists(userId)) return;
            
            var user = CoreContext.UserManager.GetUsers(userId);
            CoreContext.UserManager.RemoveUserFromGroup(user.ID, @group.ID);
            CoreContext.UserManager.SaveUserInfo(user);
        }

        private static void TransferUserToDepartment(Guid userId, GroupInfo group, bool setAsManager)
        {
            if (!CoreContext.UserManager.UserExists(userId) && userId != Guid.Empty) return;
            
            if (setAsManager)
            {
                CoreContext.UserManager.SetDepartmentManager(@group.ID, userId);
            }
            CoreContext.UserManager.AddUserIntoGroup(userId, @group.ID);
        }
    }
}
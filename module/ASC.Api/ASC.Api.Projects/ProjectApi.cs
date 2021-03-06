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
using System.Web.Configuration;
using ASC.Api.Documents;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Api.Projects.Calendars;
using ASC.Web.Core.Calendars;
using ASC.Common.Data;
using ASC.Projects.Engine;
using ASC.Core;

namespace ASC.Api.Projects
{
    ///<summary>
    /// Projects access
    ///</summary>
    public partial class ProjectApi : ProjectApiBase, IApiEntryPoint
    {
        private readonly DocumentsApi documentsApi;

        ///<summary>
        /// Api name entry
        ///</summary>
        public string Name
        {
            get { return "project"; }
        }

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="context"></param>
        ///<param name="documentsApi">Docs api</param>
        public ProjectApi(ApiContext context, DocumentsApi documentsApi)
        {
            this.documentsApi = documentsApi;

            _context = context;
        }

        internal static List<BaseCalendar> GetUserCalendars(Guid userId)
        {
            if (!DbRegistry.IsDatabaseRegistered(DbId))
                DbRegistry.RegisterDatabase(DbId, WebConfigurationManager.ConnectionStrings[DbId]);

            var tenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;
            var engineFactory = new EngineFactory(DbId, tenantId);

            var cals = new List<BaseCalendar>();
            var engine = engineFactory.GetProjectEngine();
            var projects = engine.GetByParticipant(userId);

            if (projects != null)
            {
                var team = engine.GetTeam(projects.Select(r => r.ID).ToList());

                foreach (var project in projects)
                {
                    var p = project;

                    var sharingOptions = new SharingOptions();
                    foreach (var participant in team.Where(r => r.ProjectID == p.ID))
                    {
                        sharingOptions.PublicItems.Add(new SharingOptions.PublicItem {Id = participant.ID, IsGroup = false});
                    }

                    var index = project.ID % CalendarColors.BaseColors.Count;
                    cals.Add(new ProjectCalendar(
                                 engineFactory,
                                 userId,
                                 project,
                                 CalendarColors.BaseColors[index].BackgroudColor,
                                 CalendarColors.BaseColors[index].TextColor,
                                 sharingOptions, false));
                }
            }

            var folowingProjects = engine.GetFollowing(userId);
            if (folowingProjects != null)
            {
                var team = engine.GetTeam(folowingProjects.Select(r => r.ID).ToList());

                foreach (var project in folowingProjects)
                {
                    var p = project;

                    if (projects != null && projects.Exists(proj => proj.ID == project.ID)) continue;

                    var sharingOptions = new SharingOptions();
                    sharingOptions.PublicItems.Add(new SharingOptions.PublicItem {Id = userId, IsGroup = false});
                    foreach (var participant in team.Where(r => r.ProjectID == p.ID))
                    {
                        sharingOptions.PublicItems.Add(new SharingOptions.PublicItem {Id = participant.ID, IsGroup = false});
                    }

                    var index = project.ID % CalendarColors.BaseColors.Count;
                    cals.Add(new ProjectCalendar(
                                 engineFactory,
                                 userId,
                                 project,
                                 CalendarColors.BaseColors[index].BackgroudColor,
                                 CalendarColors.BaseColors[index].TextColor,
                                 sharingOptions, true));
                }
            }

            return cals;
        }
    }
}
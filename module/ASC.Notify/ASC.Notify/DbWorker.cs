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
using System.Data;
using System.Linq;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Notify.Config;
using ASC.Notify.Messages;
using log4net;

namespace ASC.Notify
{
    class DbWorker
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NotifyService));
        private readonly string dbid;
        private readonly object syncRoot = new object();


        public DbWorker()
        {
            var connectionString = NotifyServiceCfg.ConnectionString;
            dbid = connectionString.Name;
            if (!DbRegistry.IsDatabaseRegistered(dbid))
            {
                DbRegistry.RegisterDatabase(dbid, connectionString);
            }
        }

        public void SaveMessage(NotifyMessage m)
        {
            using (var db = GetDb())
            using (var tx = db.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                var i = new SqlInsert("notify_queue")
                    .InColumns("notify_id", "tenant_id", "sender", "reciever", "subject", "content_type", "content", "sender_type", "creation_date", "reply_to")
                    .Values(0, m.Tenant, m.From, m.To, m.Subject, m.ContentType, m.Content, m.Sender, m.CreationDate, m.ReplyTo)
                    .Identity(0, 0, true);

                var id = db.ExecuteScalar<int>(i);

                i = new SqlInsert("notify_info")
                    .InColumns("notify_id", "state", "attempts", "modify_date", "priority")
                    .Values(id, 0, 0, DateTime.UtcNow, m.Priority);
                db.ExecuteNonQuery(i);

                tx.Commit();
            }
        }

        public IDictionary<int, NotifyMessage> GetMessages(int count)
        {
            lock (syncRoot)
            {
                using (var db = GetDb())
                using (var tx = db.BeginTransaction())
                {
                    var q = new SqlQuery("notify_queue q")
                        .InnerJoin("notify_info i", Exp.EqColumns("q.notify_id", "i.notify_id"))
                        .Select("q.notify_id", "q.tenant_id", "q.sender", "q.reciever", "q.subject", "q.content_type", "q.content", "q.sender_type", "q.creation_date", "q.reply_to")
                        .Where(Exp.Eq("i.state", MailSendingState.NotSended) | (Exp.Eq("i.state", MailSendingState.Error) & Exp.Lt("i.modify_date", DateTime.UtcNow - NotifyServiceCfg.AttemptsInterval)))
                        .OrderBy("i.priority", true)
                        .OrderBy("i.notify_id", true)
                        .SetMaxResults(count);

                    var messages = db
                        .ExecuteList(q)
                        .ToDictionary(
                            r => Convert.ToInt32(r[0]),
                            r => new NotifyMessage
                            {
                                Tenant = Convert.ToInt32(r[1]),
                                From = (string)r[2],
                                To = (string)r[3],
                                Subject = (string)r[4],
                                ContentType = (string)r[5],
                                Content = (string)r[6],
                                Sender = (string)r[7],
                                CreationDate = Convert.ToDateTime(r[8]),
                                ReplyTo = (string)r[9],
                            });

                    var u = new SqlUpdate("notify_info").Set("state", MailSendingState.Sending).Where(Exp.In("notify_id", messages.Keys));
                    db.ExecuteNonQuery(u);
                    tx.Commit();

                    return messages;
                }
            }
        }


        public void ResetStates()
        {
            using (var db = GetDb())
            {
                var u = new SqlUpdate("notify_info").Set("state", 0).Where("state", 1);
                db.ExecuteNonQuery(u);
            }
        }

        public void SetState(int id, MailSendingState result)
        {
            using (var db = GetDb())
            using (var tx = db.BeginTransaction())
            {
                if (result == MailSendingState.Sended)
                {
                    var d = new SqlDelete("notify_info").Where("notify_id", id);
                    db.ExecuteNonQuery(d);
                    if (NotifyServiceCfg.DeleteSendedMessages)
                    {
                        d = new SqlDelete("notify_queue").Where("notify_id", id);
                        db.ExecuteNonQuery(d);
                    }
                }
                else
                {
                    if (result == MailSendingState.Error)
                    {
                        var q = new SqlQuery("notify_info").Select("attempts").Where("notify_id", id);
                        var attempts = db.ExecuteScalar<int>(q);
                        if (NotifyServiceCfg.MaxAttempts <= attempts + 1)
                        {
                            result = MailSendingState.FatalError;
                        }
                    }
                    var u = new SqlUpdate("notify_info")
                        .Set("state", (int)result)
                        .Set("attempts = attempts + 1")
                        .Set("modify_date", DateTime.UtcNow)
                        .Where("notify_id", id);
                    db.ExecuteNonQuery(u);
                }
                tx.Commit();
            }
        }


        private DbManager GetDb()
        {
            return new DbManager(dbid);
        }
    }
}

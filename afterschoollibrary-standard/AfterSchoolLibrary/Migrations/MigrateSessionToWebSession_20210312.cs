using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class MigrateSessionToWebSession_20210312
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    //.. we have to go with raw sql query as we have lots of data in session table
                    try
                    {
                        Helpers.Utilities.SystemLog("Session migration started");

                        var sessions = Session.Load(msgs: amsgs, dc: adc);

                        foreach (var item in sessions)
                        {
                            WebSession webSession = new WebSession();
                            webSession.CreateDate = item.CreateDate;
                            webSession.CreatedBy = item.CreatedBy;
                            webSession.LastModifiedDate = item.LastModifiedDate;
                            webSession.LastModifiedBy = item.LastModifiedBy;
                            webSession.IPAddress = item.IPAddress;
                            webSession.IPAddressTimeZone = null;
                            webSession.UserAgent = item.UserAgent;
                            webSession.UserLocation = null;
                            webSession.UrlReferrer = item.UrlReferrer;
                            webSession.PreviousId = null;
                            webSession.WebUserId = null;
                            webSession.AccountId = null;

                            webSession.Save(adc);

                            var action = SessionAction.LoadBySessionId(item.ID.Value, amsgs, adc).FirstOrDefault();

                            if (action.IsNotEmpty())
                            {
                                WebSessionAction sessionAction = new WebSessionAction();
                                sessionAction.CreateDate = action.CreateDate;
                                sessionAction.CreatedBy = action.CreatedBy;
                                sessionAction.SessionId = webSession.ID;
                                sessionAction.ActorId = action.WebUserId;
                                sessionAction.AccountId = action.AccountId;
                                sessionAction.ActionTypeId = 0;
                                sessionAction.ObjectTypeId = 0;
                                sessionAction.ObjectId = action.ActivityId.IsNotEmpty() ? action.ActivityId : action.ActivityGroupId;
                                sessionAction.IndirectObjectId = null;
                                sessionAction.ActionStringFormat = "{objecttype} Updated";
                                sessionAction.ActionString = action.ActivityId.IsNotEmpty() ? "Activity Updated" : "ActivityGroup Updated";

                                sessionAction.Save(adc);
                            }                            
                        }

                        //var dq = WebSession.get_DatabaseQuery(WebSession.ObjectMap, typeof(WebSession).Name);
                        //dq.Connection = adc;
                        //var dt = dq.ExecuteSelect($@"
                        //    INSERT INTO WebSession (CreateDate, CreatedBy, LastModifiedDate, LastModifiedBy, IPAddress, IPAddressTimeZone, UserAgent, UserLocation, UrlReferrer, PreviousId, WebUserId, AccountId)
                        //    SELECT CreateDate, CreatedBy, LastModifiedDate, LastModifiedBy, IPAddress, NULL AS IPAddressTimeZone, UserAgent, NULL AS UserLocation, UrlReferrer, NULL AS PreviousId, NULL AS WebUserId, NULL AS AccountId
                        //    FROM SESSION", 
                        //    amsgs);

                        //Helpers.Utilities.SystemLog("Session migration completed");


                        //Helpers.Utilities.SystemLog("Session action migration started");

                        //var dq2 = WebSessionAction.get_DatabaseQuery(WebSessionAction.ObjectMap, typeof(WebSessionAction).Name);
                        //dq2.Connection = adc;
                        //var dt2 = dq2.ExecuteSelect($@"
                        //    INSERT INTO WebSessionAction (CreateDate, CreatedBy, SessionId, ActorId, AccountId, ActionTypeId, ObjectTypeId, ObjectId, IndirectObjectId, ActionStringFormat, ActionString)
                        //    SELECT CreateDate, CreatedBy, SessionId, WebUserId, AccountId, SessionActionTypeId, NULL, 
	                       //     CASE WHEN ActivityId IS NULL THEN ActivityGroupId ELSE ActivityId END AS ObjectId, NULL, 
	                       //     '{{objecttype}} Updated' AS ActionStringFormat, 
	                       //     CASE WHEN ActivityId IS NULL THEN 'ActivityGroup Updated' ELSE 'Activity Updated' END AS ActionString
                        //    FROM SessionAction",
                        //    amsgs);

                        Helpers.Utilities.SystemLog("Session migration completed");
                    }
                    catch (System.Exception ex)
                    {
                        Helpers.Utilities.SystemErrorLog(ex);
                    }                   

                },
                uniqueIdenfier: "MigrateSessionToWebSession-20210312",
                msgs: msgs,
                dc: dc
            );
        }
    }
}

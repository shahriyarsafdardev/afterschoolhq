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
                        var count = WebSession.GetCount(null, amsgs, adc);
                        if (count <= 0)
                        {
                            Helpers.Utilities.SystemLog("Session migration started");

                            var dq = WebSession.get_DatabaseQuery(WebSession.ObjectMap, typeof(WebSession).Name);
                            dq.Connection = adc;
                            dq.Timeout = 0;
                            var dt = dq.ExecuteSelect($@"
                            SET IDENTITY_INSERT WebSession ON

                            INSERT INTO WebSession (ID, CreateDate, CreatedBy, LastModifiedDate, LastModifiedBy, IPAddress, IPAddressTimeZone, UserAgent, UserLocation, UrlReferrer, PreviousId, WebUserId, AccountId)
                            SELECT ID, CreateDate, CreatedBy, LastModifiedDate, LastModifiedBy, IPAddress, NULL AS IPAddressTimeZone, UserAgent, NULL AS UserLocation, UrlReferrer, NULL AS PreviousId, NULL AS WebUserId, NULL AS AccountId
                            FROM SESSION	                            

                            SET IDENTITY_INSERT WebSession OFF

                            INSERT INTO WebSessionAction (CreateDate, CreatedBy, SessionId, ActorId, AccountId, ActionTypeId, ObjectTypeId, ObjectId, IndirectObjectId, ActionStringFormat, ActionString)
                            SELECT CreateDate, CreatedBy, SessionId, WebUserId, AccountId, NULL, NULL, 
	                            CASE WHEN ActivityId IS NULL THEN ActivityGroupId ELSE ActivityId END AS ObjectId, NULL, 
	                            '{{objecttype}} Updated' AS ActionStringFormat, 
	                            CASE WHEN ActivityId IS NULL THEN 'ActivityGroup Updated' ELSE 'Activity Updated' END AS ActionString
                            FROM SessionAction",
                                amsgs);

                            Helpers.Utilities.SystemLog("Session migration completed");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Helpers.Utilities.SystemErrorLog(ex);
                    }                   

                },
                uniqueIdenfier: "MigrateSessionToWebSession_20210312",
                msgs: msgs,
                dc: dc
            );
        }
    }
}

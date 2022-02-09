using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AfterSchoolLibrary.Migrations
{
    class Migration_20220202_SessionActionToWebSessionAction
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var accounts = Account.LoadByAccountTypeIds(
                                      Enum.GetValues(typeof(AccountType)).Cast<AccountType>().ToList(),
                                      false,
                                      null,
                                      null,
                                      amsgs,
                                      adc);
                    accounts.ForEach(account =>
                    {
                        var getOldSessions = SessionAction.LoadByAccountId(
                                             account.ID.Value,
                                             new List<SessionActionType>() {
                                                 SessionActionType.ActivityCreate,
                                                 SessionActionType.ActivityGroupCreate,
                                                 SessionActionType.ActivityGroupPublish,
                                                 SessionActionType.ActivityGroupUnpublish,
                                                 SessionActionType.ActivityMoveToApplication,
                                                 SessionActionType.ActivityMoveToReserved,
                                                 SessionActionType.ActivityMoveToWaitingList,
                                                 SessionActionType.ActivityPublish,
                                                 SessionActionType.ActivityRegistrationCompleted,
                                                 SessionActionType.ActivityRegistrationUpdated,
                                                 SessionActionType.ActivityUnpublish,
                                                 SessionActionType.RegistrationAddActivity,
                                                 SessionActionType.RegistrationRemoveActivity,
                                             },
                                             null, 
                                             null,
                                             msgs);
                        getOldSessions.ForEach(session =>
                        {
                            var webSession = MigrateSessionToWebSession(session);
                            msgs.AddRange(webSession.Save(dc));
                        });
                    });
                },
                uniqueIdenfier: "Migration_20220202_SessionActionToWebSessionAction",
                msgs: msgs,
                dc: dc
            );
        }
        public static WebSessionAction MigrateSessionToWebSession(SessionAction session)
        {
            var webSessionAction = new WebSessionAction();
            webSessionAction.AccountId = session.AccountId;
            if (session.SessionActionTypeId == SessionActionType.ActivityGroupCreate || session.SessionActionTypeId == SessionActionType.ActivityGroupPublish || session.SessionActionTypeId == SessionActionType.ActivityGroupUnpublish)
            {
                webSessionAction.ObjectTypeId = ObjectType.ActivityGroup; //ActivityGroup
                webSessionAction.ObjectId = session.ActivityGroupId;
            } 
            else
            {
                webSessionAction.ObjectTypeId = ObjectType.Activity;
                webSessionAction.ObjectId = session.ActivityId;
            }
            webSessionAction.ActionString = session.ToString();
            webSessionAction.CreateDate = session.CreateDate;
            webSessionAction.CreatedBy = session.CreatedBy;
            return webSessionAction;
        }
    }
}

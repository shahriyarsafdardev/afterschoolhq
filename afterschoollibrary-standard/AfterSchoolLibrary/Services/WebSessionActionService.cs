using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;

namespace AfterSchoolLibrary.Services
{
    public static class WebSessionActionService
    {
        public static WebSessionAction CreateNewLog(WebUser actor, int? webSessionId, string actionStringFormat, string actionString, int? accountId = (int?)null,
            WebSessionActionType actionType = WebSessionActionType.NotSet, ObjectType objectType = ObjectType.NotSet, ObjectType indirectObjectType = ObjectType.NotSet,
            int? objectId = (int?)null, int? indirectObjectId = (int?)null,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            WebSessionAction webSessionAction = null;

            Database.ExecuteTransaction
            (
               action: (SystemMessages amsgs, DatabaseConnection adc) =>
               {
                   webSessionAction = new WebSessionAction();
                   webSessionAction.SessionId = webSessionId;
                   webSessionAction.AccountId = accountId;
                   webSessionAction.ActionTypeId = actionType;
                   webSessionAction.ObjectTypeId = objectType;
                   webSessionAction.ObjectId = objectId;
                   webSessionAction.IndirectObjectId = indirectObjectId;
                   webSessionAction.IndirectObjectTypeId = indirectObjectType;
                   webSessionAction.ActionStringFormat = actionStringFormat;
                   webSessionAction.ActionString = actionString;
                   webSessionAction.CreateDate = DateTime.UtcNow;
                   webSessionAction.CreatedBy = actor?.ID.Value;

                   msgs.AddRange(webSessionAction.Save(dc));
               },
               onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
               onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
               onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
               dc: dc
            );

            return webSessionAction;
        }
    }
}

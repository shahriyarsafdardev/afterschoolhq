using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Helpers
{
    class WebSessionHelper
    {

        public class ActionStringObject
        {
            string ActionString {get; set;}
            string ActionStringFormat { get; set; }

            public ActionStringObject(string actionString, string actionStringFormat)
            {
                this.ActionString = actionString;
                this.ActionStringFormat = actionStringFormat;
            }

        }
        #region Action String Builders

        public static ActionStringObject GetActionString(ObjectType objectType, string actorName, WebSessionActionType actionType, string text, ObjectType indirectObjectType = ObjectType.NotSet, string indirectObjectText = null )
            {
                // Select Cases where there is no indirectObject
                if(indirectObjectType == ObjectType.NotSet)
                {
                    return CreateActionString(objectType, actorName, actionType, text);
                }

                return CreateActionStringwithIndirectObject(objectType, actorName, actionType, text, indirectObjectType, indirectObjectText);
        }

        private static ActionStringObject CreateActionStringwithIndirectObject(ObjectType objectType, string actorName, WebSessionActionType actionType, string text, ObjectType indirectObjectType, string indirectObjectText)
        {
            switch (objectType)
            {
                case ObjectType.DirectoryAccount:
                    return new ActionStringObject(
                        $"{actorName} {actionType} TO {text} FROM {objectType} {indirectObjectText}",
                        "{actor} {actiontype} To {object} FROM {objectType} {indirectObject}");

                case ObjectType.Payout:
                    return new ActionStringObject(
                        $"{actorName} {actionType} {text} For Activity {indirectObjectText}",
                        "{actor} {actiontype} {object} For Activity {indirectObject}");

                case ObjectType.SubscriptionPlan:
                case ObjectType.Registrant:
                    return new ActionStringObject(
                        $"{actorName} Subscribed {text} For Activity {indirectObjectText}",
                        "{actor} {action} {object} For Activity {indirectObject}");

                default:
                    return new ActionStringObject(
                        $"{actorName} {actionType}",
                        "{actor} {action}");
            }
        }

        private static ActionStringObject CreateActionString(ObjectType objectType, string actorName, WebSessionActionType actionType, string text)
        {
            switch (objectType)
            {
                case ObjectType.Activity:
                case ObjectType.Account:
                case ObjectType.Registrant:
                case ObjectType.DirectoryAccount:
                    return new ActionStringObject(
                        $"{actorName} {actionType} {objectType} {text}", 
                        "{actor} {actiontype} {objecttype} {object.Name}");

                case ObjectType.ContactList:
                    return new ActionStringObject(
                        $"{actorName} Tried to Contact Account {text}",
                        "{actor} Tried to {actiontype} Account {object.Name}");

                case ObjectType.WebUser:
                    return new ActionStringObject(
                        $"{actorName} Signed up",
                        "{actor} Signed up");

                case ObjectType.Registration:
                    if (actionType == WebSessionActionType.Interested)
                    {
                        return new ActionStringObject(
                            $"{actorName} Has Registered Interest For The Activity {text}",
                           "{actor} Has Registered {action} For The Activity {object.Name}");
                    }
                    return new ActionStringObject(
                          $"{actorName} Has {actionType} For The Activity {text}",
                          "{actor} Has  {action} For The Activity {object.Name}");


                default:
                    return new ActionStringObject(
                        $"{actorName} {actionType}",
                        "{actor} {action}");
            }
        }

        #endregion
    }
}

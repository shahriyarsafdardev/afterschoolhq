using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Helpers
{
    class WebSessionHelper
    {


        #region Action String Builders

        public static Dictionary<string, string> GetActionString(ObjectType objectType, string actorName, WebSessionActionType actionType, string text, ObjectType indirectObjectType = ObjectType.NotSet, string indirectObjectText = null)
        {
            // Select Cases where there is no indirectObject
            if (indirectObjectType == ObjectType.NotSet)
            {
                return CreateActionString(objectType, actorName, actionType, text);
            }

            return CreateActionStringwithIndirectObject(objectType, actorName, actionType, text, indirectObjectType, indirectObjectText);
        }

        private static Dictionary<string, string> CreateActionStringwithIndirectObject(ObjectType objectType, string actorName, WebSessionActionType actionType, string text, ObjectType indirectObjectType, string indirectObjectText)
        {
            switch (objectType)
            {
                case ObjectType.DirectoryAccount:
                    return new Dictionary<string, string>
                    {
                        {"ActionString",$"{actorName} {actionType} TO {text} FROM {objectType} {indirectObjectText}"  },
                        {"ActionStringFormat", "{actor} {actiontype} To {object} FROM {objectType} {indirectObject}" }
                    };

                case ObjectType.Payout:
                    return new Dictionary<string, string>
                    {
                        {"ActionString", $"{actorName} {actionType} {text} For Activity {indirectObjectText}"  },
                        {"ActionStringFormat", "{actor} {actiontype} {object} For Activity {indirectObject}" }
                    };

                case ObjectType.SubscriptionPlan:
                case ObjectType.Registrant:
                    return new Dictionary<string, string>
                    {
                        {"ActionString", $"{actorName} Subscribed {text} For Activity {indirectObjectText}"  },
                        {"ActionStringFormat", "{actor} {action} {object} For Activity {indirectObject}" }
                    };

                default:
                    return new Dictionary<string, string>
                    {
                        {"ActionString", $"{actorName} {actionType}"  },
                        {"ActionStringFormat", "{actor} {action}" }
                    };
            }
        }

        private static Dictionary<string, string> CreateActionString(ObjectType objectType, string actorName, WebSessionActionType actionType, string text)
        {
            switch (objectType)
            {
                case ObjectType.Activity:
                case ObjectType.Account:
                case ObjectType.Registrant:
                case ObjectType.DirectoryAccount:
                    return new Dictionary<string, string>
                    {
                        {"ActionString",$"{actorName} {actionType} {objectType} {text}" },
                        { "ActionStringFormat", "{actor} {actiontype} {objecttype} {object.Name}" }
                    };

                case ObjectType.ContactList:
                    return new Dictionary<string, string>
                    {
                        {"ActionString",$"{actorName} Tried to Contact Account {text}" },
                        { "ActionStringFormat", "{actor} Tried to {actiontype} Account {object.Name}" }
                    };

                case ObjectType.WebUser:
                    return new Dictionary<string, string>
                    {
                        {"ActionString",$"{actorName} Signed up as a {objectType}" },
                        { "ActionStringFormat", "{actor} Signed up as a {objecttype}" }
                    };

                case ObjectType.Registration:
                    if (actionType == WebSessionActionType.Interested)
                    {
                        return new Dictionary<string, string>
                        {
                            {"ActionString",$"{actorName} Has Registered Interest For The Activity {text}" },
                            { "ActionStringFormat", "{actor} Has Registered {action} For The Activity {object.Name}" }
                        };
                    }
                    return new Dictionary<string, string>
                        {
                            {"ActionString",$"{actorName} Has {actionType} For The Activity {text}" },
                            { "ActionStringFormat", "{actor} Has  {action} For The Activity {object.Name}" }
                        };


                default:
                    return new Dictionary<string, string>
                        {
                            {"ActionString",$"{actorName} {actionType}" },
                            { "ActionStringFormat",   "{actor} {action}" }
                        };
            }
        }

        #endregion
    }
}

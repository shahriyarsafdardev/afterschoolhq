using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Helpers
{
    public class WebSessionHelper
    {

        #region Action String Builders

        public static Dictionary<string, string> GetActionString(ObjectType objectType, string actorName, WebSessionActionType actionType, string objectText, ObjectType indirectObjectType = ObjectType.NotSet, string indirectObjectText = null)
        {
            var actionStringDict = CreateActionString(actorName, actionType, objectType, objectText);
            // Add to Cases where there is an indirectObject
            if (indirectObjectType != ObjectType.NotSet && indirectObjectText != "")
            {
                actionStringDict = AppendIndirectObjectText(actionStringDict, actionType, indirectObjectType, indirectObjectText);
            }

            return actionStringDict;
        }

        private static Dictionary<string, string> CreateActionString(string actorName, WebSessionActionType actionType, ObjectType objectType, string objectText)
        {
            var formatParams = new object[] { objectType, actorName, actionType, objectText };

            switch (actionType)
            {
                case WebSessionActionType.NotSet:
                    // this way we only have to change one string, no chance of there being discrepancy between the ActionString and ActionStringFormat
                    var notSetFormat = "{actorName} ACTION NOT SET  {objectType} {objectText}";
                    return new Dictionary<string, string>
                    {
                        {"ActionString", string.Format(notSetFormat, formatParams) },
                        {"ActionStringFormat", notSetFormat }
                    };
                case WebSessionActionType.BeganRegistration:
                    var actorActionOfObjectFormat = "{actorName} {actionType} of {objectType} {objectText}";
                    return new Dictionary<string, string>
                    {
                        {"ActionString", string.Format(actorActionOfObjectFormat, formatParams) },
                        {"ActionStringFormat", actorActionOfObjectFormat }
                    };
                case WebSessionActionType.Refunded:
                    var actorWasActionObjectTextFormat = "{actorName} was {actionType} {objectText}";
                    return new Dictionary<string, string>
                    {
                        {"ActionString", string.Format(actorWasActionObjectTextFormat, formatParams) },
                        {"ActionStringFormat", actorWasActionObjectTextFormat }
                    };
                case WebSessionActionType.Requested:
                    if (objectType == ObjectType.RefundRequest)
                    {
                        var refundamountFormat = "{actorName} {actionType} refund amount {objectText}";
                        return new Dictionary<string, string>
                        {
                            {"ActionString", string.Format(refundamountFormat, formatParams) },
                            {"ActionStringFormat", refundamountFormat }
                        };
                    }
                    else
                    {
                        var accessToFormat = "{actorName} {actionType} access to {objectType} {objectText}";
                        return new Dictionary<string, string>
                        {
                            {"ActionString", string.Format(accessToFormat, formatParams) },
                            {"ActionStringFormat", accessToFormat }
                        };
                    }
                case WebSessionActionType.LoggedIn:
                case WebSessionActionType.LoggedOut:
                case WebSessionActionType.SignedUp:
                    var actorActionFormat = "{actorName} {actionType}";
                    var formattest = string.Format(actorActionFormat, formatParams);
                    return new Dictionary<string, string>
                    {
                        {"ActionString", string.Format(actorActionFormat, formatParams) },
                        {"ActionStringFormat", actorActionFormat }
                    };
                case WebSessionActionType.Interested:
                    var actorIsActionInObject = "{actorName} is {actionType} In {objectType} {objectText}";
                    return new Dictionary<string, string>
                    {
                        {"ActionString", string.Format(actorIsActionInObject, formatParams) },
                        {"ActionStringFormat", actorIsActionInObject }
                    };
                case WebSessionActionType.Moved:
                case WebSessionActionType.Navigated:
                    var actorActionObjectTo = "{actorName} {actionType} to {objectType} {objectText} ";
                    return new Dictionary<string, string>
                    {
                        {"ActionString", string.Format(actorActionObjectTo, formatParams) },
                        {"ActionStringFormat", actorActionObjectTo }
                    };
                case WebSessionActionType.Registered:
                case WebSessionActionType.Paid:
                    var actorActionObjectText = "{actorName} {actionType} {objectText} ";
                    return new Dictionary<string, string>
                    {
                        {"ActionString", string.Format(actorActionObjectText, formatParams) },
                        {"ActionStringFormat", actorActionObjectText }
                    };

                case WebSessionActionType.Browsed:
                    var browseFormat = "{actorName} {actionType} {objectText}'s Programs ";
                    return new Dictionary<string, string>
                    {
                        {"ActionString", string.Format(browseFormat, formatParams) },
                        {"ActionStringFormat", browseFormat }
                    };

                //  --- Remaining ActionTypes ---
                //  case WebSessionActionType.Created:
                //  case WebSessionActionType.Updated:

                //  case WebSessionActionType.Published:
                //  case WebSessionActionType.Unpublished:
                //  case WebSessionActionType.Sent:

                //  case WebSessionActionType.Archived:
                //  case WebSessionActionType.ManuallyRegistered:
                //  case WebSessionActionType.ManuallyPaid:
                //  case WebSessionActionType.ManuallyRefunded:
                //  case WebSessionActionType.Approved:
                //  case WebSessionActionType.Verified:
                //  case WebSessionActionType.Viewed:
                //  case WebSessionActionType.Searched:
                //  case WebSessionActionType.Paged:
                //  case WebSessionActionType.Connected:
                //  case WebSessionActionType.Copied:
                //  case WebSessionActionType.Restored:

                //  case WebSessionActionType.Followed:
                //  case WebSessionActionType.Unfollowed:
                //  case WebSessionActionType.Shared:
                //  case WebSessionActionType.Contacted:
                default:
                    var actorActionObjectTypeTextFormat = "{actorName} {actionType} {objectType} {objectText}";
                    return new Dictionary<string, string>
                    {
                        {"ActionString", string.Format(actorActionObjectTypeTextFormat, formatParams) },
                        {"ActionStringFormat", actorActionObjectTypeTextFormat }
                    };
            }

        }

        private static Dictionary<string, string> AppendIndirectObjectText(Dictionary<string, string> actionStringDict, WebSessionActionType actionType, ObjectType indirectObjectType, string indirectObjectText)
        {
            var formatParams = new object[] { indirectObjectType, indirectObjectText };

            // shared formats
            var toFormat = " to {indirectObjectType {indirectObjectText}";
            var forFormat = " for {indirectObjectType} {indirectObjectText}";

            switch (actionType)
            {
                case WebSessionActionType.NotSet:
                    //  this way we only have to change one string
                    //  no chance of there being discrepancy between the ActionString and ActionStringFormat
                    var notSetFormat = " --- {indirectObjectType} {indirectObjectText}";
                    return new Dictionary<string, string>
                    { 
                        {"ActionString", actionStringDict.GetValueOrDefault("ActionString") + string.Format(notSetFormat, formatParams) },
                        {"ActionStringFormat", actionStringDict.GetValueOrDefault("ActionStringFormat") + notSetFormat }
                    };
                case WebSessionActionType.BeganRegistration:
                case WebSessionActionType.Refunded:
                case WebSessionActionType.Requested:
                case WebSessionActionType.Created:
                case WebSessionActionType.Registered:
                case WebSessionActionType.Archived:
                case WebSessionActionType.ManuallyRegistered:
                case WebSessionActionType.ManuallyPaid:
                case WebSessionActionType.ManuallyRefunded:
                case WebSessionActionType.Approved:
                case WebSessionActionType.Searched:
                    return new Dictionary<string, string>
                    {
                        {"ActionString", actionStringDict.GetValueOrDefault("ActionString") + string.Format(forFormat, formatParams) },
                        {"ActionStringFormat", actionStringDict.GetValueOrDefault("ActionStringFormat") + forFormat }
                    };

                case WebSessionActionType.Followed:
                case WebSessionActionType.Unfollowed:
                    var ofFormat = " of {indirectObjectType} {indirectObjectText}";
                    return new Dictionary<string, string>
                    {
                        {"ActionString", actionStringDict.GetValueOrDefault("ActionString") + string.Format(ofFormat, formatParams) },
                        {"ActionStringFormat", actionStringDict.GetValueOrDefault("ActionStringFormat") + ofFormat }
                    };
                
       
                case WebSessionActionType.LoggedIn:
                case WebSessionActionType.LoggedOut:
                case WebSessionActionType.SignedUp:
                    return actionStringDict;

                case WebSessionActionType.Viewed:
                case WebSessionActionType.Interested:
                    var byFormat = " by {indirectObjectType {indirectObjectText}";
                    return new Dictionary<string, string>
                    {
                        {"ActionString", actionStringDict.GetValueOrDefault("ActionString") + string.Format(byFormat, formatParams) },
                        {"ActionStringFormat", actionStringDict.GetValueOrDefault("ActionStringFormat") + byFormat }
                    };

                
                case WebSessionActionType.Updated:
                case WebSessionActionType.Published:
                case WebSessionActionType.Sent:
                case WebSessionActionType.Paged:
                case WebSessionActionType.Connected:
                case WebSessionActionType.Copied:
                case WebSessionActionType.Restored:
                   
                    return new Dictionary<string, string>
                    {
                        {"ActionString", actionStringDict.GetValueOrDefault("ActionString") + string.Format(toFormat, formatParams) },
                        {"ActionStringFormat", actionStringDict.GetValueOrDefault("ActionStringFormat") + toFormat }
                    };

                case WebSessionActionType.Unpublished:
                case WebSessionActionType.Navigated:
                case WebSessionActionType.Browsed:
                case WebSessionActionType.Moved:
                    var fromFormat = " from {indirectObjectType {indirectObjectText}";
                    return new Dictionary<string, string>
                    {
                        {"ActionString", actionStringDict.GetValueOrDefault("ActionString") + string.Format(fromFormat, formatParams) },
                        {"ActionStringFormat", actionStringDict.GetValueOrDefault("ActionStringFormat") + fromFormat }
                    };

                case WebSessionActionType.Paid:
                    // to Account
                    if(indirectObjectType == ObjectType.Account)
                    {
                        return new Dictionary<string, string>
                        {
                            {"ActionString", actionStringDict.GetValueOrDefault("ActionString") + string.Format(toFormat, formatParams) },
                            {"ActionStringFormat", actionStringDict.GetValueOrDefault("ActionStringFormat") + toFormat }
                        };
                    } 
                    else // for Object
                    {
                        return new Dictionary<string, string>
                        {
                            {"ActionString", actionStringDict.GetValueOrDefault("ActionString") + string.Format(forFormat, formatParams) },
                            {"ActionStringFormat", actionStringDict.GetValueOrDefault("ActionStringFormat") + forFormat }
                        };
                    }
                    
                //  --- remaining ActionTypes ---
                //  case WebSessionActionType.Verified:                
                //  case WebSessionActionType.Shared:
                //  case WebSessionActionType.Contacted:
                default:
                    var actorActionObjectTypeTextFormat = " {indirectObjectType {indirectObjectText";
                    return new Dictionary<string, string>
                    {
                        {"ActionString",  actionStringDict.GetValueOrDefault("ActionString") + string.Format(actorActionObjectTypeTextFormat, formatParams) },
                        {"ActionStringFormat",  actionStringDict.GetValueOrDefault("ActionStringFormat") + actorActionObjectTypeTextFormat }
                    };
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BasicLibrary;

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

        private static string ReplaceParams(string template, Dictionary<string, string> formatParams)
        {
            return formatParams.Aggregate(template, (args, pair) => args.Replace($"{{{pair.Key}}}", pair.Key == "actionType" ? pair.Value.SplitByUpperCase() : pair.Value));
        }

        private static Dictionary<string, string> FormatTemplate(string template, Dictionary<string, string> formatParams)
        {
            return new Dictionary<string, string>()
            {
                { "ActionString", ReplaceParams(template, formatParams)  },
                {"ActionStringFormat", template }
            };
        }

        private static Dictionary<string, string> CreateActionString(string actorName, WebSessionActionType actionType, ObjectType objectType, string objectText)
        {
            var formatParams = new Dictionary<string, string>
            {
                { "actorName", actorName },
                { "actionType", actionType.ToString().SplitByUpperCase() },
                { "objectType", objectType.ToString().SplitByUpperCase() },
                {"objectText", objectText }
            };

            switch (actionType)
            {
                case WebSessionActionType.NotSet:
                    // this way we only have to change one string, no chance of there being discrepancy between the ActionString and ActionStringFormat
                    var notSetTemplate = "{actorName} ACTION NOT SET  {objectType} {objectText}";
                    return FormatTemplate(notSetTemplate, formatParams);

                case WebSessionActionType.BeganRegistration:
                    var actorActionOfObjectTemplate = "{actorName} {actionType} of {objectType} {objectText}";
                    return FormatTemplate(actorActionOfObjectTemplate, formatParams);

                case WebSessionActionType.Refunded:
                case WebSessionActionType.ManuallyRefunded:
                    var actorHasActionObjectTextFormat = "{actorName} has {actionType} {objectText}";
                    return FormatTemplate(actorHasActionObjectTextFormat, formatParams);

                case WebSessionActionType.Requested:
                    if (objectType == ObjectType.RefundRequest)
                    {
                        var refundAmountTemplate = "{actorName} {actionType} refund amount {objectText}";
                        return FormatTemplate(refundAmountTemplate, formatParams);
                    }
                    else
                    {
                        var accessToTemplate = "{actorName} {actionType} access to {objectType} {objectText}";
                        return FormatTemplate(accessToTemplate, formatParams);
                    }

                case WebSessionActionType.LoggedIn:
                case WebSessionActionType.LoggedOut:
                case WebSessionActionType.SignedUp:
                    var actorActionTemplate = "{actorName} {actionType}";
                    return FormatTemplate(actorActionTemplate, formatParams);

                case WebSessionActionType.Interested:
                    var actorIsActionInObjectTemplate = "{actorName} is {actionType} In {objectType} {objectText}";
                    return FormatTemplate(actorIsActionInObjectTemplate, formatParams); 

                case WebSessionActionType.Moved:
                case WebSessionActionType.Navigated:
                    var actorActionObjectToTemplate = "{actorName} {actionType} to {objectType} {objectText} ";
                    return FormatTemplate(actorActionObjectToTemplate, formatParams);

                case WebSessionActionType.Registered:
                case WebSessionActionType.ManuallyRegistered:
                case WebSessionActionType.ManuallyPaid:
                case WebSessionActionType.Paid:
                    var actorActionObjectTextTemplate = "{actorName} {actionType} {objectText} ";
                    return FormatTemplate(actorActionObjectTextTemplate, formatParams);

                case WebSessionActionType.Browsed:
                    var browsedTemplate = "{actorName} {actionType} {objectText}'s Programs ";
                    return FormatTemplate(browsedTemplate, formatParams);

                #region Remaining Action Types
                /* 
                 *  --- Remaining ActionTypes ---
                 *  case WebSessionActionType.Created:
                 *  case WebSessionActionType.Updated:
                 *  case WebSessionActionType.Published:
                 *  case WebSessionActionType.Unpublished:
                 *  case WebSessionActionType.Sent:
                 *  case WebSessionActionType.Archived:
                 *  case WebSessionActionType.Approved:
                 *  case WebSessionActionType.Verified:
                 *  case WebSessionActionType.Viewed:
                 *  case WebSessionActionType.Searched:
                 *  case WebSessionActionType.Paged:
                 *  case WebSessionActionType.Connected:
                 *  case WebSessionActionType.Copied:
                 *  case WebSessionActionType.Restored:
                 *  case WebSessionActionType.Followed:
                 *  case WebSessionActionType.Unfollowed:
                 *  case WebSessionActionType.Shared:
                 *  case WebSessionActionType.Contacted:
                 *  case WebSessionActionType.Added:
                 *  case WebSessionActionType.Removed:
                */
                #endregion
                default:
                    var actorActionObjectTypeTextTemplate = "{actorName} {actionType} {objectType} {objectText}";
                    return FormatTemplate(actorActionObjectTypeTextTemplate, formatParams);
            }

        }

        private static Dictionary<string, string> AppendFormatTemplate(Dictionary<string, string> actionStringDict, string template, Dictionary<string, string> formatParams)
        {
            return new Dictionary<string, string>
            {
                { "ActionString",  actionStringDict.GetValueOrDefault("ActionString") + ReplaceParams(template, formatParams) },
                { "ActionStringFormat", actionStringDict.GetValueOrDefault("ActionStringFormat") + template  }
            };
        }

        private static Dictionary<string, string> AppendIndirectObjectText(Dictionary<string, string> actionStringDict, WebSessionActionType actionType, ObjectType indirectObjectType, string indirectObjectText)
        {
            var formatParams = new Dictionary<string, string>
            {
                { "indirectObjectType", indirectObjectType.ToString().SplitByUpperCase() },
                { "indirectObjectText", indirectObjectText }
            };


            // shared templates
            var toTemplate = " to {indirectObjectType} {indirectObjectText}";
            var forTemplate = " for {indirectObjectType} {indirectObjectText}";

            switch (actionType)
            {
                case WebSessionActionType.NotSet:
                    //  this way we only have to change one string
                    //  no chance of there being discrepancy between the ActionString and ActionStringFormat
                    var notSetTemplate = " --- Action Not Set --- {indirectObjectType} {indirectObjectText}";
                    return AppendFormatTemplate(actionStringDict, notSetTemplate, formatParams);

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
                case WebSessionActionType.Updated:
                    return AppendFormatTemplate(actionStringDict, forTemplate, formatParams);

                case WebSessionActionType.Followed:
                case WebSessionActionType.Unfollowed:
                    var ofTemplate = " of {indirectObjectType} {indirectObjectText}";
                    return AppendFormatTemplate(actionStringDict, ofTemplate, formatParams);


                case WebSessionActionType.LoggedIn:
                case WebSessionActionType.LoggedOut:
                case WebSessionActionType.SignedUp:
                    return actionStringDict;

                case WebSessionActionType.Viewed:
                case WebSessionActionType.Interested:
                    var byTemplate = " by {indirectObjectType} {indirectObjectText}";
                    return AppendFormatTemplate(actionStringDict, byTemplate, formatParams);

                case WebSessionActionType.Published:
                case WebSessionActionType.Sent:
                case WebSessionActionType.Paged:
                case WebSessionActionType.Connected:
                case WebSessionActionType.Copied:
                case WebSessionActionType.Restored:
                case WebSessionActionType.Added:
                    return AppendFormatTemplate(actionStringDict, toTemplate, formatParams);

                case WebSessionActionType.Unpublished:
                case WebSessionActionType.Navigated:
                case WebSessionActionType.Browsed:
                case WebSessionActionType.Moved:
                case WebSessionActionType.Removed:
                    var fromTemplate = " from {indirectObjectType} {indirectObjectText}";
                    return AppendFormatTemplate(actionStringDict, fromTemplate, formatParams);

                case WebSessionActionType.Paid:
                    // to Account
                    if(indirectObjectType == ObjectType.Account)
                    {
                        return AppendFormatTemplate(actionStringDict, toTemplate, formatParams);
                    } 
                    else // for Object
                    {
                        return AppendFormatTemplate(actionStringDict, forTemplate, formatParams);
                    }
                    
                //  --- remaining ActionTypes ---
                //  case WebSessionActionType.Verified:                
                //  case WebSessionActionType.Shared:
                //  case WebSessionActionType.Contacted:
                default:
                    var actorActionObjectTypeTextTemplate = " {indirectObjectType} {indirectObjectText}";
                    return AppendFormatTemplate(actionStringDict, actorActionObjectTypeTextTemplate, formatParams);
            }
        }

        #endregion
    }
}

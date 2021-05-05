using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using System.Web.Mvc;
using AfterSchool.Models;
using AfterSchoolLibrary.Services;

namespace AfterSchool.Helpers
{
    public static class ActivitySetupHelper
    {

        public static string SaveButton(bool isTop, bool isPrimary)
        {
            return $"<button type=\"submit\" class=\"{ButtonStyle(isTop, isPrimary)}\"><i class=\"s7-diskette\"></i> Save</button>";
        }

        public static string NavButton(Activity activity, string action, string cmd, bool isTop, bool isPrimary, HttpRequestBase request)
        {
            var canPublishActivity = SessionVariables.CurrentUser.CanPublishActivity(SessionVariables.CurrentAccount, activity);

            return $"<button type=\"submit\" class=\"{ButtonStyle(isTop, isPrimary)}\" onclick=\"ActivitySetup.navigateActivity('{GenerateUrl(activity, action, "activity", cmd, request)}')\">{ButtonText(cmd, isTop, canPublishActivity)}</button>";
        }

        public static string NavButtonWithSetupValidation(Activity activity, string action, string cmd, bool isTop, bool isPrimary, HttpRequestBase request)
        {
            var canPublishActivity = SessionVariables.CurrentUser.CanPublishActivity(SessionVariables.CurrentAccount, activity);

            return $"<button type=\"submit\" class=\"{ButtonStyle(isTop, isPrimary)}\" onclick=\"return validateSetup(1, event, '{GenerateUrl(activity, action, "activity", cmd, request)}')\">{ButtonText(cmd, isTop, canPublishActivity)}</button>";
        }

        public static string NavButton(ActivityGroup group, string action, string cmd, bool isTop, bool isPrimary, HttpRequestBase request)
        {
            return $"<button type=\"submit\" class=\"{ButtonStyle(isTop, isPrimary)}\" onclick=\"ActivitySetup.navigateActivity('{GenerateUrl(group, action, "group", cmd, request)}')\">{ButtonText(cmd, isTop, true)}</button>";
        }

        private static string ButtonText(string cmd, bool isTop, bool canPublishActivity)
        {
            if (cmd == "prev")
            {
                if (isTop)
                    return "&laquo;";

                return "&laquo; Previous";
            }
            else if (cmd == "next")
            {
                if (isTop)
                    return "&raquo;";

                return "Save &amp; Next &raquo;";
            }
            else if (cmd == "done")
            {
                if (isTop)
                    return "&raquo;";

                return "Done &raquo;";
            }
            else if (cmd == "publish")
            {
                if (canPublishActivity)
                    return "Save &amp; Review &raquo;";

                if (isTop)
                    return "Submit &raquo;";

                return "Submit to Publish &raquo;";
            }
            else if (cmd == "signupperiods")
            {
                if (isTop)
                    return "&raquo;";

                return "Save &amp; Continue &raquo;";
            }

            return "";
        }

        private static string GenerateUrl(Activity activity, string action, string model, string cmd, HttpRequestBase request)
        {
            var x = new UrlHelper(request.RequestContext);
            return x.Action(action, model, new { id = activity.Uid, cmd = cmd, isnew = request["isnew"], source = request["source"] });
        }
        private static string GenerateUrl(ActivityGroup activity, string action, string model, string cmd, HttpRequestBase request)
        {
            var x = new UrlHelper(request.RequestContext);
            return x.Action(action, model, new { id = activity.Uid, cmd = cmd, isnew = request["isnew"], source = request["source"] });
        }

        private static string ButtonStyle(bool isTop, bool isPrimary)
        {
            return $"{(isTop ? TopButtonStyle : BottomButtonStyle)} {(isPrimary ? PrimaryStyle : DefaultStyle)}";
        }

        private static string TopButtonStyle = "btn pull-right btn-space";
        private static string BottomButtonStyle = "btn btn-space btn-lg btn-block";
        private static string PrimaryStyle = "btn-primary";
        private static string DefaultStyle = "btn-default";
    }
}
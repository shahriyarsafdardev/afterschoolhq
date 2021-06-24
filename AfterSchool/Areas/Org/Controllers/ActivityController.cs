using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using Hangfire;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Helpers;
using ServiceStack.Text;

namespace AfterSchool.Areas.Org.Controllers
{
    public class ActivityController : BaseLoggedInController
    {
        public ActivityController() : base(AccountService.ProviderAccountTypes.ToList())
        {

        }

        public ActionResult Summary(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (activity.IsEmpty())
                return HttpNotFound();

            var actions = SessionAction.LoadByAccountIdByActivityId(
                activity.ID.Value,
                currentAccount.ID.Value,
                new List<SessionActionType>() {
                    SessionActionType.ActivityRegistrationUpdated,
                    SessionActionType.ActivityRegistrationCompleted,
                    SessionActionType.ActivityCreate,
                    SessionActionType.ActivityPublish,
                    SessionActionType.ActivityUnpublish,
                    SessionActionType.RegistrationAddActivity,
                    SessionActionType.RegistrationRemoveActivity,
                    SessionActionType.ActivityMoveToApplication,
                    SessionActionType.ActivityMoveToReserved,
                    SessionActionType.ActivityMoveToWaitingList
                },
                0,
                15,
                msgs);

            ViewBag.ParticipationObject = activity.GetParticipationObject(msgs);
            ViewBag.Messages = msgs;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.Actions = actions;
            ViewBag.Schedule = activity.GetSchedule();
            ViewBag.TotalPaid = activity.GetTotalAmountPaid();
            return View(activity);
        }

        [HttpGet]
        public ActionResult Index(string group, string pn, string rpp, string archive, string copyerror, string type)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            int pageNumber = 1;
            int recordsPerPage = 10;
            bool isArchived = false;
            var activityType = ActivityType.Activity;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            if (archive.IsBoolean())
                isArchived = archive.ToBoolean();

            if (type.IsNotEmpty() && type.IsNumber())
                activityType = type.ToInteger().ToEnum<ActivityType>();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            List<Activity> activities = null;
            var totalActivityCount = 0;
            var hasArchivedActivities = false;

            ActivityGroup activityGroup = null;
            if (group.IsGuid())
            {
                activityGroup = currentAccount.GetActivityGroup(group.ToGuid(), msgs);
                totalActivityCount = activityGroup.GetActivityCount(ActivityType.Activity, isArchived, false, null, msgs);
                activities = activityGroup.GetActivities(ActivityType.Activity, isArchived, false, start, length, msgs: msgs);
                hasArchivedActivities = activityGroup.GetActivityCount(ActivityType.Activity, true, false, null, msgs) > 0;
            }
            else
            {
                var getUngroupActivitiesOnly = currentAccount.AccountTypeId == AccountType.School;

                totalActivityCount = currentAccount.GetActivityCount(
                    SessionVariables.AccountUserType, 
                    SessionVariables.CurrentUser, 
                    activityType, 
                    isArchived, 
                    false,
                    getUngroupActivitiesOnly, 
                    msgs);

                activities = currentAccount.GetActivities(
                    SessionVariables.AccountUserType, 
                    SessionVariables.CurrentUser, 
                    activityType, 
                    isArchived, 
                    false,
                    getUngroupActivitiesOnly, 
                    start, 
                    length, 
                    msgs);

                hasArchivedActivities = currentAccount.GetActivityCount(
                    SessionVariables.AccountUserType, 
                    SessionVariables.CurrentUser, 
                    activityType,
                    true, 
                    false,
                    getUngroupActivitiesOnly, 
                    msgs) > 0;
            }

            var totalPages = Math.Ceiling(totalActivityCount.ToDouble() / recordsPerPage).ToInteger();

            var finalActivities = activities
                .OrderBy(x => x.Name)
                .ToList();

            if (!finalActivities.Any() && archive == "1")
                return RedirectToAction("index");

            if (copyerror.IsBoolean() && copyerror.ToBoolean())
                msgs.Add("Error creating copy", MessageType.Error);

            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Messages = msgs;
            ViewBag.Groups = currentAccount.GetSessions();
            ViewBag.Group = activityGroup;
            ViewBag.NeedToUpgrade = totalActivityCount >= SessionVariables.AccountPackage.GetFeature(FeatureType.Activities).GetMaxValue();
            ViewBag.HasArchivedActivities = hasArchivedActivities;
            ViewBag.ActivityType = activityType;

            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalActivityCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };
            return View(finalActivities);
        }


        #region Edit Activity
        [HttpGet]
        public ActionResult EditSetup(string id, string group, string type)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var msgs = new SystemMessages();
            ActivityGroup activityGroup = null;
            Activity activity;

            if (id.IsGuid())
            {
                activity = currentAccount.GetActivity(id.ToGuid(), msgs);
                activityGroup = activity.GetActivityGroup(currentAccount.ID.Value, msgs);
            }
            else
            {
                activity = currentAccount.GetDefaultNewActivity();
                activity.RegistrationModeId = RegistrationMode.Register;

                if (type.IsNotEmpty() && type.IsNumber())
                    activity.ActivityTypeId = type.ToInteger().ToEnum<ActivityType>();

                if (activity.ActivityTypeId == ActivityType.LearningPod)
                    activity.IsInviteOnly = true;

                //this means a new activity is being added to an existing enrollment group
                if (group.IsGuid())
                {
                    activityGroup = currentAccount.GetActivityGroup(group.ToGuid(), msgs);
                    activity.RegistrationModeId = activityGroup.RegistrationModeId;
                }
            }

            LoadEditSetupViewBag(activity, activityGroup, msgs);
            ViewBag.ActiveMenu = "EditSetup";
            
            return View(activity);
        }

        [HttpPost]
        public ActionResult EditSetup(string id, string name, string seatsavailable, string waitinglistseats, string registrationperiod, string registrationopendate, string registrationopentime, string registrationclosedate, string registrationclosetime, string timezone, string form, string regmode, string group, string cmd, string type, string inviteonly, string minseatsrequired, string isAdvanceSettingEnabled)
        {
            var currentAccount = SessionVariables.CurrentAccount;            
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            Activity activity;
            Activity defaultActivity;
            ActivityGroup activityGroup = null;
            ActivityForm activeForm;

            if (id.IsEmpty() || !id.IsGuid())
                return HttpNotFound();

            activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            defaultActivity = currentAccount.GetDefaultNewActivity();

            if (name.IsEmpty())
                msgs.Add("Activity name is required.", MessageType.UserError);
            else if (name.Length > 75)
                msgs.Add("Activity name must not be greater than 75 characters.", MessageType.UserError);

            if (form.IsEmpty() && (isAdvanceSettingEnabled.IsEmpty() || !isAdvanceSettingEnabled.ToBoolean()))
            {
                if (activity.IsNotEmpty())
                    //.. edit activity
                    activeForm = activity.GetActiveActivityForm(msgs);
                else
                    //.. create activity
                    activeForm = currentAccount.GetDefaultActivityForm(msgs);

                if (activeForm.IsEmpty())
                {
                    var defaultForm = currentAccount.CreateDefaultForm(currentUser, msgs);
                    currentAccount.SetDefaultForm(defaultForm, currentUser);

                    activeForm = currentAccount.GetDefaultActivityForm(msgs);
                }

                form = activeForm.FormId.Value.ToString();
            }

            if (msgs.HasNoErrors())
            {
                var values = new Dictionary<string, object>()
                {
                    { "name", name.MaxChars(75) },
                    { "seatsavailable", seatsavailable },
                    { "waitinglistseats", waitinglistseats },
                    { "registrationperiod", registrationperiod },
                    { "registrationopendate", registrationopendate },
                    { "registrationopentime", registrationopentime },
                    { "registrationclosedate", registrationclosedate },
                    { "registrationclosetime", registrationclosetime },
                    { "timezone", timezone },
                    { "form", form },
                    { "regmode", regmode.IsNotEmpty() ? regmode : ((int)RegistrationMode.Register).ToString() },
                    { "group", group },
                    { "type", type },
                    { "inviteonly", inviteonly.IsBoolean().ToString() },
                    { "minseatsrequired", minseatsrequired },
                    { "gradestart", activity.IsNotEmpty()  ? activity.GradeLevelStart : defaultActivity.GradeLevelStart },
                    { "gradeend", activity.IsNotEmpty() ? activity.GradeLevelEnd :defaultActivity.GradeLevelEnd },
                    { "genderrestrictions", activity.IsNotEmpty() && activity.GenderRestriction.IsNotEmpty() ? (int)activity.GenderRestriction :(int)defaultActivity.GenderRestriction },
                    { "agestart", activity.IsNotEmpty() ? activity.AgeStart : defaultActivity.AgeStart },
                    { "ageend",activity.IsNotEmpty() ? activity.AgeEnd : defaultActivity.AgeEnd }
                };

                if (activity.IsNotEmpty())
                {
                    activity = ActivityService.EditExisting(
                        loggedInUserId: currentUser.ID.Value,
                        account: currentAccount,
                        activityId: activity.ID.Value.ToString(),
                        values: values,
                        msgs: msgs
                    );
                    activityGroup = activity.GetActivityGroup();

                    // Show Success Message
                    // LOG: Someone Updated Activity ActivityName for Account AccountName
                    if (msgs.HasNoErrors())
                    {
                        msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

                        SessionVariables.WebSession.LogActivity(
                            account: currentAccount,
                            actor: currentUser,
                            actionType: WebSessionActionType.Updated,
                            objectType: ObjectType.Activity,
                            objectId: activity.ID.Value,
                            objectText: activity.Name,
                            indirectObjectType: ObjectType.Account,
                            indirectObjectId: currentAccount.ID.Value,
                            indirectObjectText: currentAccount.Name
                        );
                    }
                    
                }
                else
                {
                    //create initial form for account
                    if (msgs.HasNoErrors() && !SessionVariables.AccountPackage.HasFeature(FeatureType.ManageForms))
                    {
                        var defaultForm = currentAccount.GetDefaultForm();
                        var newForm = defaultForm.Copy(currentUser.ID.Value, currentAccount.ID.Value, (name + " form").MaxChars(100, "..."), msgs);
                        if (msgs.HasNoErrors())
                        {
                            if (values.Any(m => m.Key == "form"))
                                values["form"] = newForm.ID.Value.ToString();
                            else
                                values.Add("form", newForm.ID.Value.ToString());
                        }
                    }

                    values.Add("registrationthankyouemail", "Thank you for registering.");
                    values.Add("applicationthankyouemail", "Thank you for applying.");
                    values.Add("interestthankyouemail", "Thank you for your interest. We will be in touch with you soon to follow up.");

                    activity = ActivityService.CreateNew(
                        loggedInUserId: currentUser.ID.Value,
                        account: currentAccount,
                        values: values,
                        msgs: msgs
                    );

                    // Show Success Message
                    // LOG: Someone Updated Activity ActivityName for Account AccountName
                    if (msgs.HasNoErrors())
                    {
                        msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

                        SessionVariables.WebSession.LogActivity(
                            account: currentAccount,
                            actor: currentUser,
                            actionType: WebSessionActionType.Created,
                            objectType: ObjectType.Activity,
                            objectId: activity.ID.Value,
                            objectText: activity.Name,
                            indirectObjectType: ObjectType.Account,
                            indirectObjectId: currentAccount.ID.Value,
                            indirectObjectText: currentAccount.Name
                        );
                    }
                }

                if (group.IsGuid())
                    activityGroup = currentAccount.GetActivityGroup(group.ToGuid(), msgs);                

                switch (cmd)
                {
                    case "next":
                        switch (activity.ActivityTypeId)
                        {
                            case ActivityType.LearningPod:
                            case ActivityType.Activity:
                                LoadEditSetupViewBag(activity, activityGroup, msgs);
                                ViewBag.ActiveMenu = "EditRestrictions";
                                return View("EditRestrictions", activity);

                            case ActivityType.FieldTrip:
                            case ActivityType.Team:
                                LoadEditScheduleViewBag(activity, activity.ActivityScheduleId?.ToString());
                                ViewBag.ActiveMenu = "EditSchedule";
                                return View("editschedule", activity);

                            case ActivityType.PermissionSlip:
                                LoadEditSetupViewBag(activity, activityGroup, msgs);
                                ViewBag.ActiveMenu = "EditDetails";
                                return View("EditDetails", activity);
                        }
                        break;
                }
            }

            LoadEditSetupViewBag(activity, activityGroup, msgs);
            ViewBag.ActiveMenu = "EditSetup";            
            return View(activity);
        }

        private void LoadEditSetupViewBag(Activity activity, ActivityGroup activityGroup, SystemMessages msgs)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var activeForm = activity.GetActiveActivityForm(msgs) ?? activityGroup?.GetDefaultActivityForm(msgs) ?? currentAccount.GetDefaultActivityForm(msgs);
            var periods = currentAccount.GetOpenAndUpcomingRegistrationPeriods(msgs);
            var activePeriod = activity.GetActiveRegistrationPeriod(msgs) ?? currentAccount.GetDefaultNewRegistrationPeriod();

            periods.Add(activePeriod);
            periods = periods.OrderBy(period => period.StartDateTime.Value).ToList();

            ViewBag.Messages = msgs;
            ViewBag.Group = activityGroup;
            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = currentAccount.GetForms(msgs).OrderBy(x => x.Name).ToList();
            ViewBag.Periods = periods;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.ActivePeriod = activePeriod;
            ViewBag.ActiveForm = activeForm;
            ViewBag.IsFullScreen = GetIsFullScreen(activity, activityGroup?.Uid.ToString());
        }

        private bool GetIsFullScreen(Activity activity, string groupUid = "")
        {
            ViewBag.FullScreenExitUrl = Url.Action("index", "activity", new { type = (int)activity.ActivityTypeId });

            if (groupUid.IsNotEmpty())
                ViewBag.FullScreenExitUrl = Url.Action("activities", "group", new { id = groupUid });
            else if (activity.ActivityGroupId.HasValue)
                ViewBag.FullScreenExitUrl = Url.Action("activities", "group", new { id = activity.ActivityGroupUid });

            return Request["isnew"] == "1" && !activity.IsPublished;
        }

        [HttpGet]
        public ActionResult EditRestrictions(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditRestrictions";
            ViewBag.IsFullScreen = GetIsFullScreen(activity);
            return View(activity);
        }

        [HttpPost]
        public ActionResult EditRestrictions(string id, string alloworganization, string restrictbygrade, string agestart, string ageend, string graderestrictions, string mingrade, string maxgrade, string genderrestrictions, string requiresparentalconsent, string cmd)
        {
            var msgs = new SystemMessages();
            Activity activity = null;
            var accountPackage = SessionVariables.AccountPackage;
            var currentUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            if (activity.IsNotEmpty())
            {
                if (!accountPackage.HasFeature(FeatureType.AllowStudentRegistration) && requiresparentalconsent.IsBlank())
                    requiresparentalconsent = "1";

                if (restrictbygrade.IsNotEmpty() && restrictbygrade.IsBoolean())
                {
                    if (restrictbygrade.ToBoolean())
                    {
                        agestart = "";
                        ageend = "";
                    }
                    else
                    {
                        mingrade = "";
                        maxgrade = "";
                    }
                }

                var values = new Dictionary<string, object>()
                {
                    { "alloworganization", alloworganization.IfBlank("0")},
                    { "agestart", agestart },
                    { "ageend", ageend },
                    { "gradestart", mingrade },
                    { "gradeend", maxgrade },
                    { "genderrestrictions", genderrestrictions},
                    { "requiresparentalconsent", requiresparentalconsent.IfBlank("0") }
                };

                activity = ActivityService.EditExisting(
                        loggedInUserId: currentUser.ID.Value,
                        account: currentAccount,
                        activityId: activity.ID.Value.ToString(),
                        values: values,
                        msgs: msgs
                    );
            }

            // Show Success Message
            // LOG: Someone Updated Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Updated,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = accountPackage;

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        ViewBag.ActiveMenu = "EditSetup";
                        LoadEditSetupViewBag(activity, activity.GetActivityGroup(msgs), msgs);
                        return View("EditSetup", activity);
                    case "next":
                        ViewBag.ActiveMenu = "EditSchedule";
                        var scheduleId = activity.ActivityScheduleId?.ToString();
                        LoadEditScheduleViewBag(activity, scheduleId);
                        return View("EditSchedule", activity);
                }
            }

            ViewBag.IsFullScreen = GetIsFullScreen(activity);
            ViewBag.ActiveMenu = "EditRestrictions";
            return View(activity);
        }

        [HttpGet]
        public ActionResult EditContact(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Instructors = currentAccount.GetUsers(msgs: msgs);
            ViewBag.ActiveMenu = "EditContact";
            ViewBag.IsFullScreen = GetIsFullScreen(activity);
            return View(activity);
        }

        [HttpPost]
        public ActionResult EditContact(string id, string leadertype, string contactname, string contactphone, string contactemail, string instructor, string instructorfirstname, string instructorlastname, string instructoremail, string instructorphone, string cmd, string provider, string providername, string providerfirstname, string providerlastname, string provideremail, string providerphone)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            Activity activity = null;
            var beforeProviderAccountId = new int?();

            var values = new Dictionary<string, object>()
            {
                { "leadertype", leadertype.IfBlank("0") },
                { "contactname", contactname},
                { "contactphone", contactphone},
                { "contactemail", contactemail},
                { "instructor", instructor },
                { "instructorfirstname", instructorfirstname },
                { "instructorlastname", instructorlastname },
                { "instructoremail", instructoremail },
                { "instructorphone", instructorphone },
                { "provider", provider },
                { "providername", providername },
                { "providerfirstname", providerfirstname },
                { "providerlastname", providerlastname },
                { "provideremail", provideremail },
                { "providerphone", providerphone }
            };

            activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            beforeProviderAccountId = activity.ProviderAccountId;

            if (activity.IsNotEmpty())
                activity = ActivityService.EditExisting(
                    loggedInUserId: currentUser.ID.Value,
                    account: currentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs);

            // Show Success Message
            // LOG: Someone Updated Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Updated,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Instructors = currentAccount.GetUsers(msgs: msgs);
            ViewBag.IsFullScreen = GetIsFullScreen(activity);

            if (msgs.HasNoErrors())
            {
                if (beforeProviderAccountId != activity.ProviderAccountId)
                    ActivityService.NotifyProviderOfNewActivity(activity, currentUser, msgs);

                switch (cmd)
                {
                    case "prev":
                        ViewBag.ActiveMenu = "EditDetails";
                        return View("EditDetails", activity);
                    case "next":
                        if (activity.ActivityTypeId.In(new ActivityType[] { ActivityType.Activity, ActivityType.LearningPod }))
                        {
                            ViewBag.ActiveMenu = "EditLocation";
                            return View("EditLocation", activity);
                        }
                        else
                        {
                            LoadEditCostViewBag(activity, msgs);
                            ViewBag.ActiveMenu = "EditCost";
                            return View("editcost", activity);
                        }
                }
            }

            ViewBag.ActiveMenu = "EditContact";
            return View(activity);
        }

        [HttpGet]
        public ActionResult EditDetails(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = currentAccount;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditDetails";
            ViewBag.IsFullScreen = GetIsFullScreen(activity);
            return View(activity);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditDetails(string id, string description, string details, string cmd)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            Activity activity = null;

            if (details.IsEmpty())
                msgs.Add($"{UILabel.ActivityLabel()} Details is required", MessageType.UserError);

            activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            if (activity.IsNotEmpty())
            {
                var values = new Dictionary<string, object>()
                {
                    { "details", details},
                    { "description", description}
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: currentUser.ID.Value,
                    account: currentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs
                );
            }

            // Show Success Message
            // LOG: Someone Updated Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Updated,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name,
                    msgs: msgs);
            }

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen(activity);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        ViewBag.ActiveMenu = "EditSchedule";
                        var scheduleId = activity.ActivityScheduleId?.ToString();
                        LoadEditScheduleViewBag(activity, scheduleId);
                        return View("EditSchedule", activity);
                    case "next":
                        ViewBag.ActiveMenu = "EditContact";
                        ViewBag.Instructors = currentAccount.GetUsers(msgs: msgs);
                        return View("EditContact", activity);
                }
            }

            ViewBag.ActiveMenu = "EditDetails";
            return View(activity);
        }

        private void LoadEditScheduleViewBag(Activity activity, string scheduleId)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            ViewBag.Account = currentAccount;
            ViewBag.ActivityId = activity.Uid.ToString();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditSchedule";
            ViewBag.Schedules = currentAccount.GetActivitySchedules(msgs);
            ViewBag.IsFullScreen = GetIsFullScreen(activity);

            ActivitySchedule schedule = null;

            // changing schedule
            if (scheduleId.IsNumber())
            {
                schedule = currentAccount.GetActivitySchedule(scheduleId.ToInteger(), msgs);
                if (schedule.AccountId != currentAccount.ID)
                    schedule = null;
            }

            if (schedule.IsEmpty())
                schedule = activity.GetSchedule();

            if (schedule.IsEmpty())
                schedule = ActivitySchedule.GetNewDefaultSchedule(activity.GetActiveRegistrationPeriod(msgs));

            ViewBag.Schedule = schedule;
            ViewBag.ShowScheduleList = SessionVariables.AccountPackage.HasFeature(FeatureType.ManageActivityGroups, msgs);
            ViewBag.Messages = msgs;
        }

        [HttpGet]
        public ActionResult EditSchedule(string id, string schedule)
        {
            var currentAccount = SessionVariables.CurrentAccount;

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            LoadEditScheduleViewBag(activity, schedule);
            return View(activity);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditSchedule(string id, string cmd, string schedule, string scheduledetails, string requiresalldays, string repeatsweekly, string startdate, string enddate, string dailystarttime, string dailyendtime, string isongoing, string hasschedule)
        {
            var msgs = new SystemMessages();
            var inSessionDates = new List<DateRange>();
            var currentUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;
            var currentAccountPackage = SessionVariables.AccountPackage;
            Activity activity = null;
            List<DateTime> outOfSessionDates = null;
            Dictionary<string, object> values = null;

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            if (activity.IsNotEmpty() && hasschedule.IsBoolean() && hasschedule.ToBoolean())
            {
                var filteredParams = Request.Params.Filter("schedulerepeat-");
                var days = ActivityScheduleUI.PrepDaysOfWeek(filteredParams);

                if (days.Count == 0 && repeatsweekly == "1")
                    msgs.Add("Please select at least one day", MessageType.UserError);

                if (msgs.HasNoErrors())
                {
                    inSessionDates = ActivityScheduleUI.PrepScheduleDates(repeatsweekly, days, startdate, enddate, dailystarttime, dailyendtime, Request.Params.Filter("meeting-open-"));
                    outOfSessionDates = ActivityScheduleUI.PrepScheduleDatesClosed(Request.Params.Filter("meeting-closed-"));

                    inSessionDates.ForEach(dateRange =>
                    {
                        if (dateRange.StartDate > dateRange.EndDate)
                            msgs.Add("Please ensure start dates and end times are earlier than the corresponding end date and end times", MessageType.UserError);
                    });
                }

                if (msgs.HasNoErrors())
                {
                    values = new Dictionary<string, object>()
                    {
                        { "scheduledetails", scheduledetails},
                        { "days", days.Select(x => (int)x).ToStrings().ToStringList() },
                        { "repeatsweekly", repeatsweekly.IfBlank("0") },
                        { "startdate", startdate },
                        { "enddate", enddate },
                        { "dailystarttime", dailystarttime },
                        { "dailyendtime", dailyendtime },
                        { "scheduledates", inSessionDates },
                        { "scheduledatesclosed", outOfSessionDates },
                        { "schedule", schedule },
                        { "isongoing", isongoing }
                    };

                    activity = ActivityService.EditExisting(
                        loggedInUserId: currentUser.ID.Value,
                        account: currentAccount,
                        activityId: activity.ID.Value.ToString(),
                        values: values,
                        msgs: msgs
                    );
                }                
            }

            // Show Success Message
            // LOG: Someone Updated Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Updated,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            ActivitySchedule sched = null;

            if (msgs.HasNoErrors())
                sched = activity.GetSchedule();
            else
                sched = ActivitySchedule.GetNewDefaultSchedule(inSessionDates);

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = currentAccountPackage;
            ViewBag.Schedule = sched;
            ViewBag.Schedules = currentAccount.GetActivitySchedules();
            ViewBag.ShowScheduleList = currentAccountPackage.HasFeature(FeatureType.ManageActivityGroups);
            ViewBag.IsFullScreen = GetIsFullScreen(activity);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        ViewBag.ActiveMenu = "EditRestrictions";
                        return View("EditRestrictions", activity);
                    case "next":
                        if (activity.ActivityTypeId != ActivityType.Team)
                        {
                            ViewBag.ActiveMenu = "EditDetails";
                            return View("EditDetails", activity);
                        }
                        else
                        {
                            ViewBag.ActiveMenu = "EditContact";
                            ViewBag.Instructors = currentAccount.GetUsers(msgs: msgs);
                            return View("editcontact", activity);
                        }
                }
            }

            if (ViewBag.Schedule == null)
                ViewBag.Schedule = ActivitySchedule.GetNewDefaultSchedule(inSessionDates);

            ViewBag.ActiveMenu = "EditSchedule";
            return View(activity);
        }

        [HttpGet]
        public ActionResult EditLocation(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditLocation";
            ViewBag.IsFullScreen = GetIsFullScreen(activity);

            return View(activity);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditLocation(string id, string location, string locationname, string locationdetails,string address1, string streetnumber, string streetname, string address2, string neighborhood, string city, string county, string state, 
            string postalcode, string country, string placeid, decimal latitude, decimal longitude, string mylocation, string locationtype, string cmd, string activitylocationtype, string virtuallocation, string hidepreciselocation = "0")
        {
            var currentAccount = SessionVariables.CurrentAccount;           
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (location.IsEmpty())
                msgs.Add("Location is required", MessageType.UserError);

            if (activity.IsNotEmpty() && msgs.HasNoErrors())
            {
                var values = new Dictionary<string, object>()
                {
                    { "location", location},
                    { "locationname", locationname},
                    { "locationdetails", locationdetails},
                    { "address1", address1},
                    { "streetnumber", streetnumber},
                    { "streetname", streetname},
                    { "address2", address2},
                    { "neighborhood", neighborhood},
                    { "city", city},
                    { "county", county},
                    { "state", state},
                    { "postalcode", postalcode},
                    { "country", country},
                    { "placeid", placeid},
                    { "latitude", latitude },
                    { "longitude", longitude },
                    { "mylocation", mylocation },
                    { "locationtype", locationtype },
                    { "activitylocationtype", activitylocationtype },
                    { "virtuallocation", virtuallocation },
                    { "hidepreciselocation", hidepreciselocation }
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: currentUser.ID.Value,
                    account: currentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs
                );
            }

            // Show Success Message
            // LOG: Someone Updated Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Updated,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen(activity);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        ViewBag.ActiveMenu = "EditContact";
                        ViewBag.Instructors = currentAccount.GetUsers(msgs: msgs);
                        return View("EditContact", activity);
                    case "next":
                        if (SessionVariables.AccountPackage.HasFeature(FeatureType.AcceptPayments))
                        {
                            LoadEditCostViewBag(activity, msgs);
                            return View("EditCost", activity);
                        }
                        else
                        {
                            ViewBag.ActiveMenu = "EditConfirmation";
                            return View("EditConfirmation", activity);
                        }
                }
            }

            ViewBag.ActiveMenu = "EditLocation";
            return View(activity);
        }

        private void LoadEditCostViewBag(Activity activity, SystemMessages msgs)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var canProcessPayments = currentAccount.CanProcessPayments(msgs);

            if (!canProcessPayments)
                activity.AllowToPayLater = true;

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.CanProcessPayments = canProcessPayments;
            ViewBag.PaymentPlan = activity.GetPaymentPlan();
            ViewBag.SubscriptionPlan = activity.GetSubscriptionPlan();
            ViewBag.PostPayPlan = activity.GetPostPayPlan();
            ViewBag.ActiveMenu = "EditCost";
            ViewBag.IsFullScreen = GetIsFullScreen(activity);
            ViewBag.hasSchedule = activity.HasSchedule;
        }

        [HttpGet]
        public ActionResult EditCost(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            LoadEditCostViewBag(activity, msgs);
            return View(activity);
        }

        [HttpPost]
        public ActionResult EditCost(string id, string cost, string costdescription, string acceptpaypal, string acceptcreditcard, string allowtopaylater, string cmd, 
            string hassubscriptionplan, string subscriptionplancost, string subscriptionplanperiod, string isongoingsubscription, string subscriptionPlanPeriodNumber, string subscriptionPaymentStartType, string subscriptionStartDate, string subscriptionEndDate,
            string haspaymentplan, string paymentplancost, string paymentplanperiod, string paymentplanperiodnumber, string paymentplanpaymentstarttype, string splitcostevenly, string regfeedownpayment, 
            string haspostpayplan, string postpaycost, string isparentpricepostpay, string isparentpricecost, string isparentpricepaymentplan, string isparentpricesubscription, string prepaidperiodspostpay, string postpayplanperiod)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (id.IsGuid())
            {
                if (!isparentpricecost.IsNumber())
                    isparentpricecost = "0";

                if (!isparentpricepostpay.IsNumber())
                    isparentpricepostpay = "0";

                if (!cost.IsNumber())
                    cost = "0";

                var values = new Dictionary<string, object>()
                {
                    { "cost", cost},
                    { "costdescription", costdescription},
                    { "acceptpaypal", acceptpaypal.IfBlank("0")},
                    { "acceptcreditcard", acceptcreditcard.IfBlank("0")},
                    { "allowtopaylater", allowtopaylater.IfBlank("0")},
                    { "hassubscriptionplan", hassubscriptionplan.IfBlank("0") },
                    { "subscriptionplancost", subscriptionplancost },
                    { "subscriptionplanperiod", subscriptionplanperiod },
                    { "subscriptionPlanPeriodNumber", subscriptionPlanPeriodNumber },
                    { "subscriptionPaymentStartType", subscriptionPaymentStartType },
                    { "subscriptionStartDate", subscriptionStartDate },
                    { "subscriptionEndDate", subscriptionEndDate },
                    { "isongoingsubscription", !activity.HasSchedule },
                    { "haspaymentplan", haspaymentplan.IfBlank("0") },
                    { "paymentplancost", paymentplancost },
                    { "paymentplanperiod", paymentplanperiod },
                    { "paymentplanperiodnumber", paymentplanperiodnumber },
                    { "paymentplanpaymentstarttype", paymentplanpaymentstarttype },
                    { "splitcostevenly", splitcostevenly.IsBoolean().ToString() },
                    { "regfeedownpayment", regfeedownpayment.IsBoolean().ToString() },
                    { "haspostpayplan", haspostpayplan.IfBlank("0") },
                    { "postpaycost", postpaycost },
                    { "isparentpricepostpay", isparentpricepostpay },
                    { "isparentpricecost", isparentpricecost },
                    { "isparentpricepaymentplan", isparentpricepaymentplan},
                    { "isparentpricesubscription", isparentpricesubscription },
                    { "prepaidperiodspostpay", prepaidperiodspostpay },
                    { "postpayplanperiod", postpayplanperiod },
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: currentUser.ID.Value,
                    account: currentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs
                );
            }

            // Show Success Message
            // LOG: Someone Updated Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Updated,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen(activity);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        ViewBag.ActiveMenu = "EditLocation";
                        return View("EditLocation", activity);
                    case "next":
                        ViewBag.ActiveMenu = "EditConfirmation";
                        return View("EditConfirmation", activity);
                    case "done":
                        return RedirectToAction("activities", "group", new { id = activity.ActivityGroupUid });
                    case "publish":
                        return RedirectToAction("publish", "activity", new { id = activity.Uid, isnew = Request["isnew"], source = Request["source"] });
                }
            }

            LoadEditCostViewBag(activity, msgs);
            return View(activity);
        }

        [HttpGet]
        public ActionResult EditConfirmation(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditConfirmation";
            ViewBag.IsFullScreen = GetIsFullScreen(activity);
            return View(activity);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditConfirmation(string id, string registrationthankyouemail, string applicationthankyouemail, string interestthankyouemail, string cmd)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (id.IsGuid())
            {
                var values = new Dictionary<string, object>()
                {
                    { "registrationthankyouemail", registrationthankyouemail },
                    { "interestthankyouemail", interestthankyouemail },
                    { "applicationthankyouemail", applicationthankyouemail }
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: currentUser.ID.Value,
                    account: currentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs
                );
            }

            // Show Success Message
            // LOG: Someone Updated Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Updated,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen(activity);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        if (activity.CanAcceptPayments())
                        {
                            LoadEditCostViewBag(activity, msgs);
                            return View("EditCost", activity);
                        }
                        else
                        {
                            ViewBag.ActiveMenu = "EditLocation";
                            return View("EditLocation", activity);
                        }
                    case "publish":
                        return Redirect(activity.ShortHomePageUrl);

                    default:
                        return RedirectToAction("requesttopublish", new { id = activity.Uid, isnew = Request["isnew"], source = Request["source"] });
                }
            }

            ViewBag.ActiveMenu = "EditConfirmation";
            return View(activity);
        }

        #endregion

        #region Copy, Delete, Archive, Restore
        [HttpPost]
        public ActionResult Copy(string id, string activitynamecopy, string usesameformcopy, string copywithregistrants, string type)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            var activityToCopy = currentAccount.GetActivity(id.ToGuid(), msgs);

            var activity = activityToCopy.Copy(
                            activitynamecopy,
                            true,
                            currentUser.ID.Value,
                            msgs);            

            // Show Success Message
            // LOG: Someone Updated Activity ActivityName for Account AccountName
            // Redirect to New Copied Activity Edit
            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Copied", MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Copied,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );

                ViewBag.Messages = msgs;
                return RedirectToAction("editsetup", new { id = activity.Uid, isnew = 1, source = Request["source"] });
            }

            ViewBag.Messages = msgs;
            return RedirectToAction("index", "activity", new { copyerror = true, type });
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (activity.IsNotEmpty())
                msgs.AddRange(activity.MarkAsDeleted(currentUser.ID.Value));            

            // Show Success Message
            // LOG: Someone has Deleted Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been deleted", activity.Name), MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                   account: currentAccount,
                   actor: currentUser,
                   actionType: WebSessionActionType.Deleted,
                   objectType: ObjectType.Activity,
                   objectId: activity.ID.Value,
                   objectText: activity.Name,
                   indirectObjectType: ObjectType.Account,
                   indirectObjectId: currentAccount.ID.Value,
                   indirectObjectText: currentAccount.Name
               );
            }
            
            ViewBag.Messages = msgs;

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "activity");
        }

        [HttpGet]
        public ActionResult Archive(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (activity.IsNotEmpty())
                msgs.AddRange(activity.MoveToArchive(currentUser.ID.Value));            

            // Show Success Message
            // LOG: Someone Archived Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {           
                msgs.Add(string.Format("{0} has been archived", activity.Name), MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Archived,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
               );
            }

            ViewBag.Messages = msgs;

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "activity");
        }

        [HttpGet]
        public ActionResult Restore(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (activity.IsNotEmpty())
                msgs.AddRange(activity.MoveFromArchive(currentUser.ID.Value));

            // Show Success Message
            // LOG: Someone Restored Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been restored", activity.Name), MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Restored,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
               );
            }

            ViewBag.Messages = msgs;

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "activity");
        }
        #endregion


        #region Attendance
        [HttpGet]
        public ActionResult Attendance(string id, string orderby, string direction, string pn, string rpp, string msg)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            if (msg.IsNotEmpty())
                msgs.Add(msg, msg.Contains("error") || msg.Contains("Error") ? MessageType.UserError : MessageType.Success);

            int pageNumber = 1;
            int recordsPerPage = 10;
            int totalAttendanceCount = 0;
            int totalPages = 0;
            string orderByParam = "Date";
            OrderByDirectionEnum orderbyDirection = OrderByDirectionEnum.Descending;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            if (orderby.IsNotBlank())
                orderByParam = orderby;
            if (direction.IsNotBlank())
                orderbyDirection = (OrderByDirectionEnum)Enum.Parse(typeof(OrderByDirectionEnum), direction);

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            List<Attendance> attendances = null;

            if (activity.IsNotEmpty())
            {
                attendances = activity.GetAttendance(orderByParam, orderbyDirection, start, length, msgs);
                totalAttendanceCount = activity.GetTotalAttendanceCount(msgs);

                if (totalAttendanceCount > 0)
                    totalPages = Math.Ceiling(totalAttendanceCount.ToDouble() / recordsPerPage).ToInteger();
            }

            ViewBag.Messages = msgs;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Attendances = attendances;

            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalAttendanceCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };

            return View(activity);
        }

        [HttpGet]
        public ActionResult TrackAttendance(string id, string att, string actinst)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var attendanceId = att.IsNumber() ? att.ToInteger() : new int?();
            var activityInstanceId = actinst.IsNumber() ? actinst.ToInteger() : new int?();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var attendance = activity.GetOrCreateNewAttendance(attendanceId, activityInstanceId, SessionVariables.CurrentUser, msgs);

            if (attendance.IsEmpty())
                return RedirectToAction("Attendance",new { id, msg = "Error: No classes have taken place yet"});
                
            ViewBag.Messages = msgs;
            ViewBag.AttendanceRegistrants = attendance.GetAttendanceRegistrants();
            ViewBag.Schedule = activity.GetSchedule();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            return View(attendance);
        }

        [HttpPost]
        public ActionResult TrackAttendance(string id, string att)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var presentStudents = new List<int>();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var attendance = AfterSchoolLibrary.Models.Attendance.Load(att.ToInteger(), msgs);

            attendance.GetAttendanceRegistrants(msgs).ForEach(attendanceRegistrant => {
                if (Request.Params[$"present-{attendanceRegistrant.ID.ToString()}"].IsNumber())
                    presentStudents.Add(attendanceRegistrant.ID.Value);
            });

            if (msgs.HasNoErrors())
                attendance.UpdateAttendance(attendance.ActivityInstanceId.Value, presentStudents, SessionVariables.CurrentUser, msgs);

            ViewBag.Messages = msgs;
            ViewBag.AttendanceRegistrants = attendance.GetAttendanceRegistrants();
            ViewBag.Schedule = activity.GetSchedule();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            return View(attendance);
        }

        // This returns all unbilled selected Attendances by Registrant
        private Dictionary<RegistrantActivity, List<Attendance>> GetAttendancesToBillPerRegistrantActivity(Account currentAccount, List<RegistrantActivity> registrantActivities, List<Attendance> attendances, BillingPeriodType billingPeriodType, SystemMessages msgs)
        {
            Dictionary<RegistrantActivity, List<Attendance>> registrantActivityAttendanceCounts = new Dictionary<RegistrantActivity, List<Attendance>>();
    
            registrantActivities.ForEach(registrantActivity =>
            {
                //.. special case: post pay plan with # of Pre-pay
                var activity = registrantActivity.GetActivity(msgs);
                var postPayPlan = activity.GetPostPayPlan(msgs);
                var attendancesToCharge = new List<Attendance>();

                switch (postPayPlan.BillingPeriodTypeId)
                {
                    case BillingPeriodType.Day:

                        attendancesToCharge = attendances.FindAll(attendance =>
                        {
                            var attendanceRegistrant = currentAccount.GetAttendanceRegistrant(activity, attendance.ID.Value, registrantActivity.RegistrantId.Value, msgs);
                            if (attendanceRegistrant.IsNotEmpty() && !attendanceRegistrant.IsAlreadyBilled)
                                return attendanceRegistrant.NumberPresent > 0;

                            return false;

                        }).ToList();

                        registrantActivityAttendanceCounts.Add(registrantActivity, attendancesToCharge);

                        break;

                    case BillingPeriodType.Week:
                       
                        attendances.ForEach(attendance =>
                        {
                            var currentWeekPresent = currentAccount.GetAttendanceRegistrantByDateRange(
                                                        activity, 
                                                        registrantActivity.RegistrantId.Value, 
                                                        new DateRange(
                                                            attendance.Date.Value.FirstDateOfWeek().Date,
                                                            attendance.Date.Value.LastDateOfWeek().Date.AddDays(1).AddMilliseconds(-1)
                                                        ) )
                                                        .Where(item => item.NumberPresent > 0)
                                                        .OrderBy(item => item.AttendanceDate.Value)
                                                        .ToList();

                            if (currentWeekPresent.Any() && !currentWeekPresent.Any(item => item.IsAlreadyBilled))
                            {
                                var firstAttendanceOfWeek = currentWeekPresent.First().GetAttendance(msgs);

                                var currentUser = SessionVariables.CurrentUser;
                                var attendanceRegistrant = currentAccount.GetAttendanceRegistrant(activity, attendance.ID.Value, registrantActivity.RegistrantId.Value, msgs);

                                if (attendanceRegistrant.IsNotEmpty() && firstAttendanceOfWeek.IsNotEmpty() && !attendanceRegistrant.IncludeNextAvailablePrePayment(currentUser, msgs)  && !attendancesToCharge.Any(item => item.ID == firstAttendanceOfWeek.ID))
                                    attendancesToCharge.Add(firstAttendanceOfWeek);
                            }

                        });

                        registrantActivityAttendanceCounts.Add(registrantActivity, attendancesToCharge);

                        break;
                }                

                
            });

            return registrantActivityAttendanceCounts;
        }

        // This returns billed Attendances and the count of Registrants Billed for billing period
        private Dictionary<Attendance, List<int>> GetNumberOfRegistrantsBilledPerAttendance(List<Attendance> attendances, BillingPeriodType billingPeriodType, SystemMessages msgs = null)
        {
            var numberOfRegistrantsBilledPerAttendance = new Dictionary<Attendance, List<int>>();

            attendances.ForEach(attendance =>
            {
                var registrantIdsBilled = AttendanceRegistrant.LoadByAttendanceId(attendance.ID.Value, msgs).Where(ar => ar.IsAlreadyBilled).Select(rb => rb.RegistrantId.Value).Distinct().ToList();

                switch (billingPeriodType)
                {
                    case BillingPeriodType.Day:
                        if (numberOfRegistrantsBilledPerAttendance.ContainsKey(attendance))
                        {
                            registrantIdsBilled.ForEach(rbId =>
                            {
                                if (!numberOfRegistrantsBilledPerAttendance[attendance].Contains(rbId))
                                    numberOfRegistrantsBilledPerAttendance[attendance].Add(rbId);
                            });
                        }
                        else
                            numberOfRegistrantsBilledPerAttendance.Add(attendance, registrantIdsBilled);
                        
                        break;

                    case BillingPeriodType.Week:
                        var attendanceIsInSameWeekAsExisitingAttendanceInList = numberOfRegistrantsBilledPerAttendance.FirstOrDefault(keyPair => keyPair.Key.Date.Value.DatesAreInSameWeek(attendance.Date.Value)).Key;
                        if (attendanceIsInSameWeekAsExisitingAttendanceInList.IsNotEmpty())
                        {
                            registrantIdsBilled.ForEach(rbId =>
                            {
                                if (!numberOfRegistrantsBilledPerAttendance[attendanceIsInSameWeekAsExisitingAttendanceInList].Contains(rbId))
                                    numberOfRegistrantsBilledPerAttendance[attendanceIsInSameWeekAsExisitingAttendanceInList].Add(rbId);
                            });
                        }
                        else
                            numberOfRegistrantsBilledPerAttendance.Add(attendance, registrantIdsBilled);

                        break;

                    default:
                        break;
                }

            });

            return numberOfRegistrantsBilledPerAttendance;
        }

            [HttpPost]
        public ActionResult ChargeAttendanceFee()
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            // this should return the amount to be charged for each registrant 
            var attendanceIds = Request.Form["AttendanceIds"];
            var activityId = Request.Form["Uid"];
            
            Activity activity = null;

            if (activityId.IsNotEmpty() && activityId.IsGuid())
                activity = currentAccount.GetActivity(activityId.ToGuid(), msgs);

            if (activity.IsNotEmpty() && (attendanceIds.IsNotEmpty() || attendanceIds.IsNotBlank()))
            {
                // get selected attendances
                var attendanceIdsList = attendanceIds.Split(',').Select(p => p.ToInteger()).ToList();
                var attendances = AfterSchoolLibrary.Models.Attendance.LoadByActivityId(activity.ID.Value, msgs);
                var selectedAttendances = attendances.Where(a => attendanceIdsList.Contains(a.ID.Value)).ToList();

                // sum up total cost of all attendances per registrant to display
                var registrantActivities = RegistrantActivity.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs);
                var billingPeriodType = activity.GetPostPayPlan(msgs).BillingPeriodTypeId;

                ViewBag.AttdanceIds = attendanceIds;
                ViewBag.BillingPeriodType = billingPeriodType;
                ViewBag.AttendancesToBillRegistrants = GetAttendancesToBillPerRegistrantActivity(currentAccount, registrantActivities, selectedAttendances, billingPeriodType, msgs);
                ViewBag.Messages = msgs;

                return View(activity);
            }
            return RedirectToAction("Attendance");
        }

        [HttpPost]
        public ActionResult ChargeAttendanceFeeDone()
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            // this should make the transaction purchase and activity payment records
            //and send charges for each registrant to stripe
            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var attendanceIds = Request.Form["AttendanceIds"];
            var activityId = Request.Form["Uid"];
            
            Activity activity = null;
            var defaultMsg = "An error occurred while processing payments. Please contact support, do not attempt to charge attendance again";
            var msg = defaultMsg;            

            if (activityId.IsNotEmpty() && activityId.IsGuid())
                activity = currentAccount.GetActivity(activityId.ToGuid(), msgs);

            if (activity.IsNotEmpty() && (attendanceIds.IsNotEmpty() || attendanceIds.IsNotBlank()))
            {
                // get selected attendances
                var attendanceIdsList = attendanceIds.Split(',').Select(p => p.ToInteger()).ToList();
                var attendances = AfterSchoolLibrary.Models.Attendance.LoadByActivityId(activity.ID.Value, msgs);
                var selectedAttendances = attendances.Where(a => attendanceIdsList.Contains(a.ID.Value)).ToList();

                // sum up total cost of all attendances per registrant to display
                var registrantActivities = RegistrantActivity.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs);
                var postPayPlan = activity.GetPostPayPlan(msgs);
                var billingPeriodType = postPayPlan?.BillingPeriodTypeId;
                var registrantActivityAttendances = GetAttendancesToBillPerRegistrantActivity(
                                                        currentAccount,
                                                        registrantActivities, 
                                                        selectedAttendances, 
                                                        billingPeriodType.HasValue ? billingPeriodType.Value : BillingPeriodType.NotSet, 
                                                        msgs);                

                foreach (var registrantActivityAttendance in registrantActivityAttendances)
                {
                    // one person multiple attendance dates
                    var registrantActivity = registrantActivityAttendance.Key;
                    var registration = registrantActivity.GetRegistration(msgs);
                    var costPerAttendance = registration.CostPerClass.IsNotEmpty() ? registration.CostPerClass.Value : 0;

                    if (costPerAttendance > 0)
                    {
                        var masterRegistrant = registration.GetMasterRegistrant(msgs);
                        var raAttendances = registrantActivityAttendance.Value;
                        var masterRegistrantAccount = masterRegistrant.GetAccount(msgs);
                        var description = $"{registrantActivity.GetActivity().Name.ToProperCase()} attendance Fee times {raAttendances.Count}";
                        var convenienceFeePayer = registration.GetConvenienceFeePayer();
                        var paymentTypeFeePayer = registration.GetPaymentTypeFeePayer();
                        var availablePrePayments = registration.GetAvailablePrePayments(activity, msgs);
                        var attendanceDaysToCharge = Math.Max(0, raAttendances.Count - availablePrePayments.Count);
                        CheckoutObject checkoutObject = null;
                        var coupon = CouponService.GetCoupon(registration, msgs);

                        if (attendanceDaysToCharge > 0)
                        {
                            BankAccountPurchaseObject bankAccountPurchaseObject = null;
                            CreditCardPurchaseObject creditCardPurchaseObject = null;

                            if (registration.PaymentTypeId == PaymentType.ACH)
                                bankAccountPurchaseObject = new BankAccountPurchaseObject { StripeSourceId = registration.StripeSourceId };
                            else
                                creditCardPurchaseObject = new CreditCardPurchaseObject { StripeSourceId = registration.StripeSourceId };

                            var overrideAmount = costPerAttendance * attendanceDaysToCharge;

                            if (postPayPlan.IsNotEmpty())
                            {
                                //.. if post pay plan, then we will have coupon value in registration to discount.
                                var couponDiscount = attendanceDaysToCharge * (registration.CouponValue.HasValue ? registration.CouponValue.Value : 0);
                                overrideAmount = (costPerAttendance * attendanceDaysToCharge) - couponDiscount;
                            }                          

                            checkoutObject = new CheckoutObject()
                            {
                                Coupon = coupon,
                                OverrideAmount = overrideAmount,
                                ConvenienceFeeOverrideAmount = registration.ConvenienceFeePaid.IfEmpty(0) * attendanceDaysToCharge,
                                PaymentTypeFeeOverrideAmount = registration.PaymentTypeFeePaid.IfEmpty(0) * attendanceDaysToCharge,
                                PayingAccount = masterRegistrantAccount,
                                BillingAccount = activity.GetAccount(msgs),
                                CreditCardPurchaseObject = creditCardPurchaseObject,
                                BankAccountPurchaseObject = bankAccountPurchaseObject,
                                PaymentType = bankAccountPurchaseObject.IsNotEmpty() ? PaymentType.ACH : PaymentType.CreditCard,
                                PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                                ConvenienceFeePayer = convenienceFeePayer,
                                CreditCardFeePayer = paymentTypeFeePayer,
                                MasterRegistrant = masterRegistrant,
                                Description = description,
                                SavePaymentInfo = false,
                                ParticipationObject = activity.GetParticipationObject(msgs),
                                Quantity = attendanceDaysToCharge,
                                Registration = registration,
                                PostPayPlan = postPayPlan,
                            };

                            if (msgs.HasNoErrors())
                                AccountService.ProcessPayment(
                                    checkoutObject,
                                    isAttendanceCharge: true,
                                    loggedInUser: currentUser,
                                    msgs: msgs);
                        }

                        if (msgs.HasNoErrors())
                            raAttendances.ForEach(raAttendance =>
                            {
                                var attendanceRegistrant = AttendanceRegistrant.LoadByAttendanceIdByRegistrantId(
                                                                raAttendance.ID.Value,
                                                                registrantActivity.GetRegistrant(msgs).ID.Value,
                                                                msgs)
                                                            .FirstOrDefault();

                                // add next available pre payment if available, if not then continue to set the checkoutobject transaction id
                                if (attendanceRegistrant.IsNotEmpty() 
                                    && !attendanceRegistrant.IncludeNextAvailablePrePayment(currentUser, msgs) 
                                    && (checkoutObject?.Transaction).IsNotEmpty() 
                                    && !attendanceRegistrant.IsAlreadyBilled)
                                {
                                    attendanceRegistrant.TransactionId = checkoutObject.Transaction.ID.Value;

                                    attendanceRegistrant
                                        .GetAttendanceRegistrantsInSameBillingPeriod(postPayPlan.BillingPeriodTypeId, msgs)
                                        .ForEach(ar => {
                                           ar.TransactionId = checkoutObject.Transaction.ID.Value;
                                           msgs.AddRange(ar.Save());
                                        });

                                    msgs.AddRange(attendanceRegistrant.Save());
                                }
                            });

                        if (msgs.HasErrors())
                        {
                            msg = $"An error occured while charging some parents, some parents may have already been processed and you will still receive a summary email if any have. Please contact support and do NOT attempt to charge for attendance again.";
                            break;
                        }
                    }
                }

                if (msgs.HasNoErrors() && msg == defaultMsg)
                {
                    selectedAttendances.ForEach(attendance =>
                    {
                        attendance.IsCharged = true;
                        msgs.AddRange(attendance.Save());
                    });

                    msg = "All accounts have been successfully charged.";
                }

                // Send summary email of charged AttendanceRegistrants to Activity Provider if there were any successful charges
                var registrantsBilledPerAttendance = GetNumberOfRegistrantsBilledPerAttendance(
                        selectedAttendances, 
                        billingPeriodType.HasValue ? billingPeriodType.Value : BillingPeriodType.NotSet,
                        msgs);
                var objRegistration = registrantActivities.First().GetRegistration(msgs);
                var costPerClass = objRegistration.CostPerClass.HasValue ? objRegistration.CostPerClass.Value : 0;
                var couponValue = objRegistration.CouponValue.HasValue ? objRegistration.CouponValue.Value : 0;

                if (registrantsBilledPerAttendance.Count > 0)
                    EmailHelper.SendAttendanceChargeConfirmation(registrantsBilledPerAttendance, activity, costPerClass, couponValue, null, SessionVariables.CurrentUser, false, msgs);
                    
            }

            return RedirectToAction("attendance", new { id = activityId, msg = msg });
        }
        #endregion

        [HttpGet]
        public ActionResult FinalizeRegistration(string id, string atype)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            activity.FinalizeRegistrations(currentUser, msgs);

            var msg = msgs.HasErrors() ? "Error Finalizing Registration" : "Registration Finalized";

            return RedirectToAction("registrants", new { id, atype, msg });
        }

        private void RegistrantsViewBag(Activity activity, string atype, string msg)
        {
            var msgs = new SystemMessages();
            var rat = RegistrationAssignmentType.NotSet;

            List<RegistrantActivity> ras = null;

            if (atype.IsNotEmpty())
            {
                rat = (RegistrationAssignmentType)atype.ToInteger();
                if (rat == RegistrationAssignmentType.Invited)
                    ras = activity.GetRegistrantActivities(true);
            }

            if (ras.IsEmpty())
                ras = activity.GetRegistrantActivities();

            if (rat != RegistrationAssignmentType.NotSet)
                ras = ras.Where(x => x.AssignmentTypeId == rat).OrderBy(x => x.SeatIndex).ToList();

            if (msg.IsNotEmpty())
                msgs.Add(msg, msg.ToLower().Contains("error") ? MessageType.UserError : MessageType.Success);

            ViewBag.Messages = msgs;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.Registrants = ras;
            ViewBag.AssignmentType = rat;
        }

        [HttpGet]
        public ActionResult Registrants(string id, string atype, string msg)
        {
            var currentAccount = SessionVariables.CurrentAccount;            
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            RegistrantsViewBag(activity, atype, msg);
            return View(activity);
        }

        [HttpGet]
        public ActionResult MoveRegistrant(string id, string registrant, string assignmenttype)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var ra = RegistrantActivity.LoadByParticipationObjectByRegistrant(
                activity.ID.Value, 
                ParticipationObjectType.Activity, 
                registrant.ToInteger(), 
                msgs);

            var newAssignmentType = (RegistrationAssignmentType)assignmenttype.ToInteger();

            if (ra.IsNotEmpty())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        ra.ChangeSeatAssignment(
                            newAssignmentType,
                            currentUser,
                            amsgs,
                            adc);
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );                
            }

            return RedirectToAction("registrants", new { id });
        }        

        [HttpGet]
        public ActionResult RegistrantSummary(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;            
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            BackgroundJob.Enqueue(() =>
                ActivityService.SendRegistrationSummaryReport(activity.ID.Value, UILabel.ActivityLabel(activity.ActivityTypeId, false), null)
            );

            msgs.Add("Your report will be sent shortly", MessageType.Success);

            ViewBag.Messages = msgs;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.Registrants = activity.GetRegistrantActivities();
            ViewBag.AssignmentType = RegistrationAssignmentType.NotSet;
            return View("Registrants", activity);
        }

        [HttpGet]
        public ActionResult RegistrantDownload(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var af = activity.GetActiveActivityForm();
            var questions = af.GetForm().GetAllQuestions();

            var registrantActivities = activity.GetRegistrantActivities();
            var table = new List<List<string>>();

            var row = new List<string>();

            row.Add("Date Registered");
            row.Add("Seat Assignment");

            if (activity.CostPerRegistrant > 0)
                row.Add("Amount Paid");

            foreach (var question in questions)
                row.Add(question.QuestionDataTypeId.ToString());

            table.Add(row);

            foreach (var registrantActivity in registrantActivities)
            {
                row = new List<string>();

                row.Add(registrantActivity.CreateDate.ToShortDateTimeString());
                row.Add(registrantActivity.AssignmentTypeId.ToString().SplitByUpperCase());

                if (activity.CostPerRegistrant > 0)
                    row.Add(registrantActivity.GetAmountPaid().ToCurrency());

                foreach (var question in questions)
                {
                    var answer = question.GetAnswer(registrantActivity.RegistrantId.Value);

                    if (answer.IsNotEmpty())
                        row.Add(answer.Text);
                    else
                        row.Add("");
                }

                table.Add(row);
            }

            string csv = CsvFile.Create(table);

            Response.Clear();
            Response.AddHeader("Content-Type", BasicLibrary.Utilities.GetMimeType("csv"));
            Response.AddHeader("Content-Disposition", "attachment; filename=registrants.csv");

            byte[] filebytes = System.Text.UTF8Encoding.UTF8.GetBytes(csv);
            Response.AddHeader("Content-Length", filebytes.Length.ToString());
            Response.BinaryWrite(filebytes);
            Response.End();

            ViewBag.Messages = msgs;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            return View("Registrants", activity);
        }

        [HttpPost]
        public JsonResult WithdrawRegistrant(string participationObjectId, string registrantUId, string type)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return Json(new { success = false, status = "error" }, JsonRequestBehavior.AllowGet);
            
            if (participationObjectId.IsEmpty() || registrantUId.IsEmpty() || type.IsEmpty())
                return Json(new { success = false, status = "error" }, JsonRequestBehavior.AllowGet);

            var registrant = currentAccount.GetRegistrant(registrantUId.ToGuid(), msgs);

            if (registrant.IsNotEmpty())
            {
                registrant.WithdrawRegistration(
                    participationObjectId: participationObjectId.ToInteger(),
                    participationObjectType: (ParticipationObjectType)Enum.Parse(typeof(ParticipationObjectType), type),
                    loggedInUser: SessionVariables.CurrentUser,
                    account: currentAccount,
                    msgs: msgs);
            }

            if (msgs.HasNoErrors())
                return Json(new { success = true, status = "ok" }, JsonRequestBehavior.AllowGet);
                
            //.. something went wrong, error occured
            return Json(new { success = false, status = "error", data = msgs.ToList().Select(msg => new { msg.Text }) }, JsonRequestBehavior.AllowGet);
        }

        private List<AttachedCoupon> GetAttachedCoupons(RegistrantActivity registrantActivity)
        {
            var masterRegistrant = registrantActivity.GetRegistrant().GetMasterRegistrant();
            return masterRegistrant?.GetAccount().GetAttachedCoupons() ?? new List<AttachedCoupon>();
        }

        [HttpGet]
        public ActionResult RegistrantDetail(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            var registrantActivity = RegistrantActivityService.GetRegistrantActivity(id.ToGuid(), msgs);
            var activity = currentAccount.GetActivity(registrantActivity.ParticipationObjectId.Value, msgs);

            ViewBag.Messages = msgs;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AttachedCoupons = GetAttachedCoupons(registrantActivity);
            return View(registrantActivity);
        }

        [HttpPost]
        public ActionResult AddPayment(string id, string datepaid, string amountpaid, string paymenttype)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var registrantActivity = RegistrantActivityService.GetRegistrantActivity(id.ToGuid(), msgs);
            var activity = registrantActivity.GetActivity();

            if (!datepaid.IsDateTime())
                msgs.Add("Date Paid must be a valid date", MessageType.UserError);

            if (!amountpaid.IsNumber() || amountpaid.ToDecimal() < 0)
                msgs.Add("Amount Paid must be a number greater than or equal to 0", MessageType.UserError);

            if (!paymenttype.IsNumber())
                msgs.Add("Please select a valid payment type", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                // the registrant that is only on the provider or school account
                var registrant = registrantActivity.GetRegistrant(msgs);
                
                // from the parent account, see if the registrant has a parent account
                var masterRegistrant = registrant.GetMasterRegistrant(msgs);

                // master registrant could be empty because a registrant can be created on the backend
                // so we may not have a purchaser account
                var purchaserAccount = masterRegistrant?.GetAccount(msgs);                

                AccountService.CreatePurchaseTransactionPayment(
                    checkoutObject: new CheckoutObject()
                    {
                        BillingAccount = currentAccount,
                        PayingAccount = purchaserAccount,
                        NumberOfSeats = 1,
                        ParticipationObject = registrantActivity.GetParticipationObject(msgs),
                        Registration = registrantActivity.GetRegistration(msgs),
                        MasterRegistrant = masterRegistrant,
                        PaymentType = (PaymentType)paymenttype.ToInteger(),
                        PaymentSourceType = PaymentSourceType.ManualProvider,
                        OverrideAmount = amountpaid.ToDecimal()
                    },
                    loggedInUser: SessionVariables.CurrentUser,
                    msgs: msgs
                    );  

                if (msgs.HasNoErrors())
                    msgs.Add("Payment has been added", MessageType.Success);
            }

            ViewBag.Messages = msgs;
            ViewBag.AttachedCoupons = GetAttachedCoupons(registrantActivity);
            ViewBag.IsActivityLeader = IsActivityLeader(activity);

            return View("registrantdetail", registrantActivity);
        }

        [HttpPost]
        public ActionResult AttachCoupon(string id, decimal couponAmount, CouponAmountType discountType, DateTime? expirationDate, CouponApplyType couponApplyType, StripeCouponDuration duration, int durationInMonths)
        {
            var msgs = new SystemMessages();
            var registrantActivity = RegistrantActivityService.GetRegistrantActivity(id.ToGuid(), msgs);
            var activity = registrantActivity.GetActivity();
            
            // check coupon's minimum amount
            if (couponAmount <= 0)
                msgs.Add("Coupon Amount must be a number greater than 0", MessageType.UserError);

            // check coupon's amount percentage 
            if (discountType == CouponAmountType.Percentage)
            {
                couponAmount /= 100;

                if (couponAmount > 1)
                    msgs.Add("Percentage must be a number less than or equal to 100%", MessageType.UserError);
            } else
            {
                // by default the maximum amount is the activity's cost
                var maxAmount = activity.CostPerRegistrant.Value;

                // check coupon's amount
                if (couponAmount > maxAmount)
                    msgs.Add($"Coupon Amount must be a number less than or equal to {maxAmount.ToCurrency()}", MessageType.UserError);
            }

            if (couponApplyType == CouponApplyType.SubscriptionPlan)
            {
                if (duration == StripeCouponDuration.Repeating && durationInMonths < 1)
                    msgs.Add("# of Times must be a number greater than 0", MessageType.UserError);
            }
           
            // check coupon's expiration date
            if (expirationDate.IsEmpty() || expirationDate <= DateTime.UtcNow.Date)
                msgs.Add("Expiration date must be in the future", MessageType.UserError);

            ViewBag.Messages = msgs;
            ViewBag.AttachedCoupons = GetAttachedCoupons(registrantActivity);
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            return View("RegistrantDetail", registrantActivity);
        }        

        [HttpGet]
        public ActionResult PublishDetail(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            ViewBag.Account = currentAccount;
            ViewBag.MustRequestToPublish = MustRequestToPublish(activity);
            ViewBag.ShowCopyButton = ShowCopyButtonOnPublishDetail(activity);
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.Messages = msgs;
            return View(activity);
        }

        [HttpGet]
        public ActionResult RequestToPublish(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var isNewActivity = Request["isnew"] == "1";

            activity.RequestToPublish(SessionVariables.CurrentUser, msgs);           

            if (msgs.HasNoErrors())
                if (isNewActivity)
                    msgs.Add("Your request to publish has been submitted", MessageType.Success);            
                else
                    msgs.Add("Activity saved successfully", MessageType.Success);

            ViewBag.Messages = msgs;
            ViewBag.MustRequestToPublish = MustRequestToPublish(activity);
            ViewBag.ShowCopyButton = ShowCopyButtonOnPublishDetail(activity);
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.IsFullScreen = isNewActivity;
            return View("publishdetail", activity);
        }

        private void BeforePublish(Activity activity, SystemMessages msgs)
        {
            var currentAccount = SessionVariables.CurrentAccount;            

            // check if provided by external provider
            var location = activity.GetLocation(msgs);
            var isLoggedInAccountOwnerAccount = currentAccount.ID == location?.HostAccountId;
            var activityIsHostedAtAnotherAccount = location?.HostAccountId != activity.AccountId;

            // activity is created in the activity provider's account
            if (isLoggedInAccountOwnerAccount && activityIsHostedAtAnotherAccount)
            {
                var visitingAccount = activity.GetAccount(msgs);

                if (!visitingAccount.IsApprovedAsVisitorTo(currentAccount, msgs))
                {
                    currentAccount.ApproveAccessToAccount(visitingAccount, SessionVariables.CurrentUser, msgs);

                    if (msgs.HasNoErrors())
                        currentAccount.SendApprovalNotificationToVisitor(visitingAccount, msgs);
                }
            }
        }

        [HttpGet]
        public ActionResult Publish(string id, bool isFromActivityHome = false)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var mustRequestToPublish = MustRequestToPublish(activity);

            if (mustRequestToPublish)
                msgs.Add("You do not have permissions to publish", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                BeforePublish(activity, msgs);
                activity.Publish(SessionVariables.CurrentUser, msgs);
            }

            if (msgs.HasNoErrors())
            {
                if (!activity.ActivityGroupId.HasValue)
                {
                    //.. send support email for AHQ just to notifiy the team a new profile has been completed
                    var body = $"<p>Account: {currentAccount.Name}<br />" +
                            $"Account Profile Url: {currentAccount.HomePageUrl}<br />" +
                            $"Activity: {activity.Name}<br />" +
                            $"Url: {activity.HomePageUrl}<br />" +
                            $"User: {currentUser.FirstName} {currentUser.LastName}<br />";

                var registrationDate = activity.GetActiveRegistrationPeriod();
                if (registrationDate.IsNotEmpty() && registrationDate.StartDateTime.IsNotEmpty())
                    body += $"Registration Start Date: {registrationDate.StartDateTime} <br />";

                if(activity.HasSchedule)
                    body += $"Activity Start Date: {activity.GetSchedule(msgs).StartDate.ToShortDateString()} <br />";

                    if (activity.AccountTimeZone.IsNotEmpty())
                        body += $"All times are in {activity.AccountTimeZone} <br />";

                    EmailHelper.SendEmail(
                        toEmail: AfterSchoolLibrary.Helpers.Email.AppCustomersEmailAddress,
                        subject: $"{currentAccount.Name} has published a new Activity: {activity.Name}",
                        body,
                        isAfterSchool: true);
                }

                // Show Succes Message
                // LOG: Someone Published Activity ActivityName for Account AccountName
                msgs.Add(string.Format("{0} has been published", activity.Name), MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Published,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
               );
            }

            ViewBag.Messages = msgs;
            ViewBag.IsFullScreen = Request["isnew"] == "1";
            ViewBag.MustRequestToPublish = mustRequestToPublish;
            ViewBag.ShowCopyButton = ShowCopyButtonOnPublishDetail(activity);
            ViewBag.IsActivityLeader = IsActivityLeader(activity);

            var user = currentAccount.GetAccountOwner(msgs);
            if (user.IsNotEmpty() && currentAccount.IsNotEmpty())
                SendGridHelper.AddOrUpdateContact(currentAccount, user, msgs);

            if (isFromActivityHome)
                return Redirect(activity.ShortHomePageUrl);

            return View("publishdetail", activity);
        }

        [HttpGet]
        public ActionResult Unpublish(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var mustRequestToPublish = MustRequestToPublish(activity);
            
            if (mustRequestToPublish)
                msgs.Add("You do not have permissions to unpublish", MessageType.UserError);

            if (msgs.HasNoErrors())
                activity.Unpublish(currentUser, msgs);

            // Show Success Message
            // LOG: Someon Unpublished Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been unpublished", activity.Name), MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Unpublished,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            ViewBag.Messages = msgs;
            ViewBag.MustRequestToPublish = mustRequestToPublish;
            ViewBag.ShowCopyButton = ShowCopyButtonOnPublishDetail(activity);
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            return View("publishdetail", activity);
        }

        [HttpGet]
        public ActionResult Form(string id, string qg)
        {
            var currentAccount = SessionVariables.CurrentAccount;

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var activityForm = activity.GetActiveActivityForm();

            var questionGroups = activityForm.GetForm().GetQuestionGroups();
            var individualGroups = questionGroups.Where(x => x.RegistrantTypeId == RegistrantType.Individual).ToList();
            var organizationGroups = questionGroups.Where(x => x.RegistrantTypeId == RegistrantType.Organization).ToList();

            FormQuestionGroup questionGroup = null;

            if (qg.IsNumber())
                questionGroup = FormQuestionGroup.Load(qg.ToInteger());
            else
                questionGroup = questionGroups.First();

            ViewBag.Messages = msgs;
            ViewBag.QuestionGroups = questionGroups;
            ViewBag.IndividualGroups = individualGroups;
            ViewBag.OrganizationGroups = organizationGroups;
            ViewBag.QuestionGroup = questionGroup;
            ViewBag.ActivityForm = activityForm;
            return View(activity);
        }

        [HttpPost]
        public ActionResult Form(string id, string qg, string name, string description, string cmd)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();            

            if (name.IsEmpty() && qg.IsBlank())
                msgs.Add("Name is required", MessageType.UserError);
            if (description.IsEmpty() && qg.IsBlank())
                msgs.Add("Description is required", MessageType.UserError);

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var activityForm = activity.GetActiveActivityForm();
            FormQuestionGroup questionGroup = null;

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        if (name.IsNotBlank()) activityForm.Name = name;
                        if (description.IsNotBlank()) activityForm.Description = description;

                        amsgs.AddRange(activityForm.Save(adc));

                        List<FormQuestion> questions = new List<FormQuestion>();

                        if (qg.IsNumber())
                        {
                            questions = FormQuestion.LoadByFormQuestionGroupId(qg.ToInteger(), dc: adc);
                            questionGroup = FormQuestionGroup.Load(qg.ToInteger(), dc: adc);
                        }

                        if (questionGroup.IsNotEmpty())
                        {
                            questionGroup.Name = Request.Params[string.Format("name-{0}", questionGroup.ID)];
                            questionGroup.LastModifiedBy = SessionVariables.CurrentUser.ID;
                            questionGroup.LastModifiedDate = DateTime.UtcNow;
                            amsgs.AddRange(questionGroup.Save(adc));
                        }

                        foreach (var question in questions)
                        {
                            if (!question.IsLocked)
                            {
                                question.Text = Request.Params[string.Format("text-{0}", question.ID)];

                                question.IsActive = false;
                                if (Request.Params[string.Format("active-{0}", question.ID)].IsBoolean())
                                    question.IsActive = true;

                                question.IsRequired = false;
                                if (Request.Params[string.Format("required-{0}", question.ID)].IsBoolean())
                                    question.IsRequired = true;

                                amsgs.AddRange(question.Save(adc));
                            }
                        }
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        msgs.Add("Registration Form Saved", MessageType.Success);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
            }

            var questionGroups = activityForm.GetForm().GetQuestionGroups();
            var individualGroups = questionGroups.Where(x => x.RegistrantTypeId == RegistrantType.Individual).ToList();
            var organizationGroups = questionGroups.Where(x => x.RegistrantTypeId == RegistrantType.Organization).ToList();
            var currentIndex = questionGroups.FindIndex(x => x.ID.Value == questionGroup.ID.Value);
            switch (cmd)
            {
                case "prev":
                    questionGroup = questionGroups[currentIndex - 1];
                    break;
                case "next":
                    questionGroup = questionGroups[currentIndex + 1];
                    break;
            }

            ViewBag.Messages = msgs;
            ViewBag.QuestionGroups = questionGroups;
            ViewBag.IndividualGroups = individualGroups;
            ViewBag.OrganizationGroups = organizationGroups;
            ViewBag.QuestionGroup = questionGroup;
            ViewBag.ActivityForm = activityForm;

            return View(activity);

        }

        [HttpGet]
        public ActionResult Messages(string id, string pn, string rpp)
        {
            var currentAccount = SessionVariables.CurrentAccount;            
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            var totalMessageCount = activity.GetMessagesCount(msgs);
            var activityMessages = activity.GetMessages(start, length, msgs);
            var totalPages = Math.Ceiling(totalMessageCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Messages = msgs;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActivityMessages = activityMessages;

            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalMessageCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };

            return View(activity);
        }

        [HttpGet]
        public ActionResult MessageDetail(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var am = currentAccount.GetActivityMessage(id.ToGuid(), msgs);
            var activity = currentAccount.GetActivity(am.ActivityId.Value, msgs);

            ViewBag.Messages = msgs;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.Registrants = am.GetActivityMessageRegistrants();

            return View(am);
        }

        [HttpGet]
        public ActionResult MessageAdd(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.Registrants = activity.GetRegistrants()
                                    .OrderBy(x => x.FirstName)
                                    .ToList();

            // If the user does have the messages feature, and the user tries to send a message, validate the user hasn't used up their quota of emails, and/or text messages, if they have exceeded their quota, show the upgrade.
            // If the user has hit or exceeded the email quota then show the upgrade modal
            ViewBag.EmailCount = currentAccount.GetEmailCountForCurrentMessagePeriod();
            ViewBag.TextCount = currentAccount.GetTextMessageCountForCurrentMessagePeriod();

            return View(activity);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult MessageAdd(string id, string deliveryMethod, string subject, string body, string sendtostaff, string sendtextmessages)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            if (subject.IsEmpty())
                msgs.Add("Message Reason is required", MessageType.UserError);
            if (body.IsEmpty())
                msgs.Add("Message Body is required", MessageType.UserError);

            ActivityMessage am = null;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (msgs.HasNoErrors())
            {
                var registrantIds = activity.GetRegistrants(msgs)
                                        .Where(x => Request[$"registrant-{x.ID.ToString()}"].IsNotBlank())
                                        .Select(x => x.ID.Value)
                                        .ToList();

                //.. email messaging
                if (sendtostaff.IsNotEmpty())
                {
                    // get current limit
                    var accountPackage = SessionVariables.AccountPackage;
                    var maxEmailCount = accountPackage.HasFeature(FeatureType.EmailMessaging) ? (int)accountPackage.GetFeature(FeatureType.EmailMessaging).Max.Value : 0;

                    // get current quota
                    var amr = ActivityMessageRegistrant.LoadByCreatedBy(SessionVariables.CurrentUser.ID.Value.ToString()).Where(m => m.CreateDate > DateTime.Now.AddMonths(-1).Date);
                    var usedEmailQuota = currentAccount.GetEmailCountForCurrentMessagePeriod();

                    //add additional registransts
                    usedEmailQuota = usedEmailQuota + registrantIds.Count;

                    // if exceeds, return error
                    if (usedEmailQuota > maxEmailCount)
                        msgs.Add("Email quota exceeded for current user.", MessageType.UserError);

                    if (msgs.HasNoErrors())
                    {
                        am = activity.AddMessage(
                            subject,
                            body,
                            registrantIds,
                            sendtostaff.IsBoolean(),
                            SessionVariables.CurrentUser.ID.Value,
                            msgs);

                        BackgroundJob.Enqueue(() => ActivityMessageService.Send(am.ID.Value, SessionVariables.CurrentUser.ID.Value, null, null));

                        msgs.Add("Your message will been sent to the selected registrants", MessageType.Success);
                        ViewBag.Registrants = am.GetActivityMessageRegistrants();
                        ViewBag.Messages = msgs;
                        ViewBag.IsActivityLeader = IsActivityLeader(activity);

                        return View("messagedetail", am.Reload());
                    }
                }

            }

            ViewBag.Messages = msgs;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);            
            ViewBag.Registrants = activity.GetRegistrants();
            return View(activity);
        }

        [HttpGet]
        public ActionResult Team(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Users = currentAccount.GetUsers(msgs: msgs);
            ViewBag.Messages = msgs;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            return View(activity);
        }

        [HttpPost]
        public ActionResult TeamSave(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();
            
            Activity activity = null;
            Account account = null;
            List<WebUser> users = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) => {

                    activity = currentAccount.GetActivity(id.ToGuid(), amsgs, adc);
                    account = activity.GetAccount(amsgs, adc);
                    users = account.GetUsers(msgs: amsgs, dc: adc);

                    users.ForEach(user =>
                    {
                        var doNotSkip = !(activity.CreatedBy.Value == user.ID.Value || currentUser.ID == user.ID);

                        if (doNotSkip)
                        {
                            var awu = user.GetActivityWebUser(activity, amsgs, adc);

                            if (Request["onteam-" + user.ID.Value.ToString()].IsNotEmpty())
                            {
                                if (awu.IsEmpty())
                                {
                                    awu = new ActivityWebUser();
                                    awu.ActivityId = activity.ID;
                                    awu.WebUserId = user.ID;
                                    awu.CreateDate = DateTime.UtcNow;
                                    awu.CreatedBy = currentUser.ID;
                                }

                                if (Request["isleader-" + user.ID.Value.ToString()].IsNotEmpty())
                                    awu.IsLeader = true;
                                else
                                    awu.IsLeader = false;

                                awu.LastModifiedDate = DateTime.UtcNow;
                                awu.LastModifiedBy = currentUser.ID;
                                amsgs.AddRange(awu.Save(adc));

                            }
                            else if (awu.IsNotEmpty())
                                amsgs.AddRange(awu.Delete(adc));
                        }
                    });
                },
                onSuccess: (SystemMessages smsgs) => {
                    msgs.AddRange(smsgs);
                    msgs.Add("The Activity Team has been updated", MessageType.Success);
                },
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
            );

            ViewBag.Messages = msgs;
            ViewBag.Users = users;
            return View("Team", activity);
        }

        [HttpGet]
        public ActionResult AddRegistration(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var activeForm = activity.GetActiveForm(msgs);
            var questions = activeForm.GetActiveQuestions(msgs);

            questions.ForEach(question => {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantGrade:
                    case QuestionDataType.RegistrantGender:
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                        question.IsRequired = true;
                        break;
                    default:
                        question.IsRequired = false;
                        break;
                }
            });

            ViewBag.QuestionViewModels = questions.OrderBy(question => question.DisplayOrder).Select(question => new FormQuestionViewModel(question)).ToList();
            ViewBag.Messages = msgs;

            return View(activity);
        }

        [HttpPost]
        public ActionResult AddRegistration(string id, string parentsig)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var questions = activity.GetActiveForm(msgs).GetActiveQuestions(msgs);

            questions.ForEach(question => {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantGrade:
                    case QuestionDataType.RegistrantGender:
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                        question.IsRequired = true;
                        break;
                    default:
                        question.IsRequired = false;
                        break;
                }
            });

            questions.ForEach(question => {
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);

                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantHomePhone:
                    case QuestionDataType.RegistrantWorkPhone:
                    case QuestionDataType.RegistrantMobilePhone:
                        if (answer.IsNotEmpty() && !answer.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                            msgs.Add($"{question.Text} must be a valid phone number", MessageType.UserError);
                        break;

                }
            });

            var qvms = questions.OrderBy(question => question.DisplayOrder).Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            if (msgs.HasNoErrors())
                currentAccount.CreateNewRegistrantAndRegistration(
                    participationObject: activity.GetParticipationObject(msgs),
                    questionsAndAnswers: qvms.Select(qvm => (qvm.Question, qvm.Value)).ToList(),
                    period: activity.GetCurrentRegistrationPeriod(msgs),
                    loggedInUser: SessionVariables.CurrentUser,
                    msgs: msgs);

            if (msgs.HasNoErrors())
                return RedirectToAction("registrants", new { id = activity.Uid });

            ViewBag.Messages = msgs;
            ViewBag.QuestionViewModels = qvms;

            return View(activity);
        }

        private bool IsActivityLeader(Activity activity)
        {
            if (SessionVariables.CurrentAccount.ID == activity.AccountId)
            {
                if (SessionVariables.AccountUserType.In(AccountService.AdminUserTypes))
                    return true;

                return SessionVariables.CurrentUser.IsActivityLeader(activity);
            }

            return false;
        }

        private bool MustRequestToPublish(Activity activity) => !SessionVariables.CurrentUser.CanPublishActivity(SessionVariables.CurrentAccount, activity);

        private bool IsExternallyProvided(Activity activity)
        {
            var location = activity.GetLocation();
            var hostAccount = location?.GetHostAccount();

            if (hostAccount.IsNotEmpty() && hostAccount.OwnerId.HasValue)
                return hostAccount.ID != activity.AccountId;

            return false;
        }

        private bool ShowCopyButtonOnPublishDetail(Activity activity) => activity.AccountId == SessionVariables.CurrentAccount.ID || !IsExternallyProvided(activity);

        [HttpGet]
        public virtual JsonResult GetCalculatedStartingAndEndDateForSubscription(string activityId, string billingPeriodType, string dayOfWeek, string dayOfMonth)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (!IsRequestFromApp() || activityId.IsEmpty() || currentAccount.IsEmpty())
                return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.AllowGet);
            
            var activity = currentAccount.GetActivity(activityId.ToInteger(), msgs);
            var dates = ActivityService.FinalizeStartAndEndDateForSubscription(activity, 
                (BillingPeriodType)billingPeriodType.ToInteger(), 
                dayOfWeek.IsNotEmpty() ? dayOfWeek.ToEnum<DayOfWeek>() : DayOfWeek.Sunday, 
                dayOfMonth.IsNotEmpty() ? dayOfMonth.ToDateTime() : DateTime.Now.AddDays(1));

            if (dates.paymentStart.IsNotEmpty() && dates.paymentStart.HasValue && dates.paymentEnd.IsNotEmpty() && dates.paymentEnd.HasValue)
                return Json(new { 
                    success = true, 
                    startDate = dates.paymentStart.Value.ToString(),
                    startDateFormated = dates.paymentStart.Value.ToString("dddd, MMMM d, yyyy"), 
                    endDate = dates.paymentEnd.Value.ToString(),
                    endDateFormated = dates.paymentEnd.Value.ToString("dddd, MMMM d, yyyy"),
                }, JsonRequestBehavior.AllowGet);

            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
    }
}
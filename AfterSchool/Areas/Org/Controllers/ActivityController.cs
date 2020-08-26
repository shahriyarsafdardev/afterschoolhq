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

namespace AfterSchool.Areas.Org.Controllers
{
    public class ActivityController : BaseLoggedInController
    {
        public ActivityController() : base(AccountService.ProviderAccountTypes.ToList())
        {

        }

        public ActionResult Summary(string id)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid());
            var actions = SessionAction.LoadByActivityId(
                activity.ID.Value,
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
            var msgs = new SystemMessages();
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
                activityGroup = ActivityGroup.LoadByUid(group.ToGuid(), msgs);
                totalActivityCount = activityGroup.GetActivityCount(ActivityType.Activity, isArchived, false, null, msgs);
                activities = activityGroup.GetActivities(ActivityType.Activity, isArchived, false, start, length, msgs: msgs);
                hasArchivedActivities = activityGroup.GetActivityCount(ActivityType.Activity, true, false, null, msgs) > 0;
            }
            else
            {
                var getUngroupActivitiesOnly = SessionVariables.CurrentAccount.AccountTypeId == AccountType.School;

                totalActivityCount = SessionVariables.CurrentAccount.GetActivityCount(
                    SessionVariables.AccountUserType, 
                    SessionVariables.CurrentUser, 
                    activityType, 
                    isArchived, 
                    false,
                    getUngroupActivitiesOnly, 
                    msgs);

                activities = SessionVariables.CurrentAccount.GetActivities(
                    SessionVariables.AccountUserType, 
                    SessionVariables.CurrentUser, 
                    activityType, 
                    isArchived, 
                    false,
                    getUngroupActivitiesOnly, 
                    start, 
                    length, 
                    msgs);

                hasArchivedActivities = SessionVariables.CurrentAccount.GetActivityCount(
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
            ViewBag.Messages = new SystemMessages();
            ViewBag.Groups = SessionVariables.CurrentAccount.GetSessions();
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

        [HttpGet]
        public ActionResult EditSetup(string id, string group, string type)
        {
            var msgs = new SystemMessages();
            ActivityGroup activityGroup = null;
            Activity activity;

            if (id.IsGuid())
            {
                activity = Activity.LoadByUid(id.ToGuid(), msgs);
                activityGroup = activity.GetActivityGroup(msgs);
            }
            else
            {
                activity = SessionVariables.CurrentAccount.GetDefaultNewActivity();
                activity.RegistrationModeId = RegistrationMode.Register;

                if (type.IsNotEmpty() && type.IsNumber())
                    activity.ActivityTypeId = type.ToInteger().ToEnum<ActivityType>();

                if (activity.ActivityTypeId == ActivityType.LearningPod)
                    activity.IsInviteOnly = true;

                //this means a new activity is  being added to an existing enrollment group
                if (group.IsGuid())
                {
                    activityGroup = ActivityGroup.LoadByUid(group.ToGuid(), msgs);
                    activity.RegistrationModeId = activityGroup.RegistrationModeId;
                }
            }

            LoadEditSetupViewBag(activity, activityGroup, msgs);
            ViewBag.ActiveMenu = "EditSetup";
            ViewBag.Group = activityGroup;
            
            return View(activity);
        }

        [HttpPost]
        public ActionResult EditSetup(string id, string name, string seatsavailable, string waitinglistseats, string registrationperiod, string registrationopendate, string registrationopentime, string registrationclosedate, string registrationclosetime, string timezone, string form, string regmode, string group, string cmd, string type, string inviteonly, string minseatsrequired)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            Activity activity;
            ActivityGroup activityGroup = null;

            var values = new Dictionary<string, object>()
            {
                { "name", name },
                { "seatsavailable", seatsavailable },
                { "waitinglistseats", waitinglistseats },
                { "registrationperiod", registrationperiod },
                { "registrationopendate", registrationopendate },
                { "registrationopentime", registrationopentime },
                { "registrationclosedate", registrationclosedate },
                { "registrationclosetime", registrationclosetime },
                { "timezone", timezone },
                { "form", form },
                { "regmode", regmode },
                { "group", group },
                { "type", type },
                { "inviteonly", inviteonly.IsBoolean().ToString() },
                { "minseatsrequired", minseatsrequired },
            };

            activity = Activity.LoadByUid(id.ToGuid());
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
            }
            else
            {
                //create initial form for account
                if (msgs.HasNoErrors() && !SessionVariables.AccountPackage.HasFeature(FeatureType.ManageForms))
                {
                    var defaultForm = currentAccount.GetDefaultForm();
                    var newForm = defaultForm.Copy(currentUser.ID.Value, currentAccount.ID.Value, (name + " form").MaxChars(100, "..."), msgs);
                    if (msgs.HasNoErrors())
                        values.Add("form", newForm.ID.Value.ToString());
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

                if (msgs.HasNoErrors())
                    UpdateSession(SessionActionType.ActivityCreate, currentAccount.ID, activity.ID);
            }

            if (group.IsGuid())
                activityGroup = ActivityGroup.LoadByUid(group.ToGuid());

            LoadEditSetupViewBag(activity, activityGroup, msgs);
            ViewBag.Group = activityGroup;

            if (msgs.HasNoErrors())
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "next":
                        switch(activity.ActivityTypeId)
                        {
                            case ActivityType.LearningPod:
                            case ActivityType.Activity:
                                ViewBag.ActiveMenu = "EditRestrictions";
                                return View("EditRestrictions", activity);
                            case ActivityType.FieldTrip:
                            case ActivityType.Team:
                                LoadEditScheduleViewBag(activity, activity.ActivityScheduleId?.ToString());
                                ViewBag.ActiveMenu = "EditSchedule";
                                return View("editschedule", activity);
                            case ActivityType.PermissionSlip:
                                ViewBag.ActiveMenu = "EditDetails";
                                return View("EditDetails", activity);

                        }
                        break;
                }
            }

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
            var activity = Activity.LoadByUid(id.ToGuid());

            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditRestrictions";
            ViewBag.IsFullScreen = GetIsFullScreen(activity);
            return View(activity);
        }

        [HttpPost]
        public ActionResult EditRestrictions(string id, string alloworganization, string agerestrictions, string agestart, string ageend, string graderestrictions, string mingrade, string maxgrade, string genderrestrictions, string requiresparentalconsent, string cmd)
        {
            var msgs = new SystemMessages();
            Activity activity = null;

            activity = Activity.LoadByUid(id.ToGuid());
            if (activity.IsNotEmpty())
            {
                var gradeIds = new List<int>();
                var grades = GradeLevel.Load();

                foreach (var grade in grades)
                {
                    if (Request["grade-" + grade.ID.ToString()].IsNotEmpty())
                    {
                        gradeIds.Add(grade.ID.Value);
                    }
                }

                var values = new Dictionary<string, object>()
                {
                    { "alloworganization", alloworganization.IfBlank("0")},
                    { "agestart", agerestrictions.IsNumber() ? agestart : "-1"},
                    { "ageend", agerestrictions.IsNumber() ? ageend : "-1"},
                    { "gradestart", graderestrictions.IsNumber() ? mingrade : "-1"},
                    { "gradeend", graderestrictions.IsNumber() ? maxgrade : "-1"},
                    { "genderrestrictions", genderrestrictions},
                    { "gradeIds", gradeIds.ToStrings().ToStringList() },
                    { "requiresparentalconsent", requiresparentalconsent.IfBlank("0") }
                };

                activity = ActivityService.EditExisting(
                        loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                        account: SessionVariables.CurrentAccount,
                        activityId: activity.ID.Value.ToString(),
                        values: values,
                        msgs: msgs
                    );
            }

            if (msgs.HasNoErrors())
                msgs.Add(string.Format("{0} Saved", UILabel.ActivityLabel(activity.ActivityTypeId)), MessageType.Success);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;

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
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Instructors = SessionVariables.CurrentAccount.GetUsers(msgs: msgs);
            ViewBag.ActiveMenu = "EditContact";
            ViewBag.IsFullScreen = GetIsFullScreen(activity);
            return View(activity);
        }

        [HttpPost]
        public ActionResult EditContact(string id, string leadertype, string contactname, string contactphone, string contactemail, string instructor, string instructorfirstname, string instructorlastname, string instructoremail, string instructorphone, string cmd, string provider, string providername, string providerfirstname, string providerlastname, string provideremail, string providerphone)
        {
            var msgs = new SystemMessages();
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

            activity = Activity.LoadByUid(id.ToGuid(), msgs);
            beforeProviderAccountId = activity.ProviderAccountId;

            if (activity.IsNotEmpty())
                activity = ActivityService.EditExisting(
                    loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                    account: SessionVariables.CurrentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs);

            if (msgs.HasNoErrors())
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Instructors = SessionVariables.CurrentAccount.GetUsers(msgs: msgs);
            ViewBag.IsFullScreen = GetIsFullScreen(activity);

            if (msgs.HasNoErrors())
            {
                if (msgs.HasNoErrors() && beforeProviderAccountId != activity.ProviderAccountId)
                    ActivityService.NotifyProviderOfNewActivity(activity, SessionVariables.CurrentUser, msgs);

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
            var activity = Activity.LoadByUid(id.ToGuid());

            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = SessionVariables.CurrentAccount;
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
            var msgs = new SystemMessages();
            Activity activity = null;

            if (details.IsEmpty())
                msgs.Add($"{UILabel.ActivityLabel()} Details is required", MessageType.UserError);

            activity = Activity.LoadByUid(id.ToGuid());
            if (activity.IsNotEmpty())
            {
                var values = new Dictionary<string, object>()
                {
                    { "details", details},
                    { "description", description}
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                    account: SessionVariables.CurrentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs
                );
            }

            if (msgs.HasNoErrors())
                msgs.Add(string.Format("{0} Saved", UILabel.ActivityLabel(activity.ActivityTypeId)), MessageType.Success);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
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
                        ViewBag.Instructors = SessionVariables.CurrentAccount.GetUsers(msgs: msgs);
                        return View("EditContact", activity);
                }
            }

            ViewBag.ActiveMenu = "EditDetails";
            return View(activity);
        }

        private void LoadEditScheduleViewBag(Activity activity, string scheduleId)
        {
            var msgs = new SystemMessages();
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.ActivityId = activity.Uid.ToString();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditSchedule";
            ViewBag.Schedules = SessionVariables.CurrentAccount.GetActivitySchedules(msgs);
            ViewBag.IsFullScreen = GetIsFullScreen(activity);

            ActivitySchedule schedule = null;

            // changing schedule
            if (scheduleId.IsNumber())
            {
                schedule = ActivitySchedule.Load(scheduleId.ToInteger(), msgs);
                if (schedule.AccountId != SessionVariables.CurrentAccount.ID)
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
            var activity = Activity.LoadByUid(id.ToGuid());

            LoadEditScheduleViewBag(activity, schedule);

            return View(activity);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditSchedule(string id, string cmd, string schedule, string scheduledetails, string requiresalldays, string repeatsweekly, string startdate, string enddate, string dailystarttime, string dailyendtime, string isongoing, string hasschedule)
        {
            var msgs = new SystemMessages();
            var inSessionDates = new List<DateRange>();
            Activity activity = null;
            List<DateTime> outOfSessionDates = null;
            Dictionary<string, object> values = null;

            activity = Activity.LoadByUid(id.ToGuid());
            if (activity.IsNotEmpty())
            {
                if (hasschedule.IsBoolean() && hasschedule.ToBoolean())
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
                            loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                            account: SessionVariables.CurrentAccount,
                            activityId: activity.ID.Value.ToString(),
                            values: values,
                            msgs: msgs
                        );
                    }
                }
            }

            if (msgs.HasNoErrors())
                msgs.Add(string.Format("{0} Schedule Saved", UILabel.ActivityLabel(activity.ActivityTypeId)), MessageType.Success);
            
            ActivitySchedule sched = null;

            if (msgs.HasNoErrors())
                sched = activity.GetSchedule();
            else
                sched = ActivitySchedule.GetNewDefaultSchedule(inSessionDates);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Schedule = sched;
            ViewBag.Schedules = SessionVariables.CurrentAccount.GetActivitySchedules();
            ViewBag.ShowScheduleList = SessionVariables.AccountPackage.HasFeature(FeatureType.ManageActivityGroups);
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
                            ViewBag.Instructors = SessionVariables.CurrentAccount.GetUsers(msgs: msgs);
                            return View("editcontact", activity);
                        }
                }
            }
            
            ViewBag.ActiveMenu = "EditSchedule";
            return View(activity);
        }

        [HttpGet]
        public ActionResult EditLocation(string id)
        {
            var activity = Activity.LoadByUid(id.ToGuid());

            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditLocation";
            ViewBag.IsFullScreen = GetIsFullScreen(activity);

            return View(activity);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditLocation(string id, string location, string locationname, string locationdetails,string address1, string streetnumber, string streetname, string address2, string neighborhood, string city, string county, string state, string postalcode, string country, string placeid, string mylocation, string locationtype, string cmd, string activitylocationtype, string virtuallocation, string hidepreciselocation = "0")
        {
            var msgs = new SystemMessages();
            Activity activity = Activity.LoadByUid(id.ToGuid());

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
                    { "mylocation", mylocation },
                    { "locationtype", locationtype },
                    { "activitylocationtype", activitylocationtype },
                    { "virtuallocation", virtuallocation },
                    { "hidepreciselocation", hidepreciselocation }
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                    account: SessionVariables.CurrentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs
                );
            }

            if (msgs.HasNoErrors())
                msgs.Add(string.Format("{0} Saved", UILabel.ActivityLabel(activity.ActivityTypeId)), MessageType.Success);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen(activity);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        ViewBag.ActiveMenu = "EditContact";
                        ViewBag.Instructors = SessionVariables.CurrentAccount.GetUsers(msgs: msgs);
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
            var canProcessPayments = SessionVariables.CurrentAccount.CanProcessPayments(msgs);

            if (!canProcessPayments)
                activity.AllowToPayLater = true;

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.CanProcessPayments = canProcessPayments;
            ViewBag.PaymentPlan = activity.GetPaymentPlan();
            ViewBag.SubscriptionPlan = activity.GetSubscriptionPlan();
            ViewBag.ActiveMenu = "EditCost";
            ViewBag.IsFullScreen = GetIsFullScreen(activity);
        }

        [HttpGet]
        public ActionResult EditCost(string id)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid(), msgs);
            LoadEditCostViewBag(activity, msgs);
            return View(activity);
        }

        [HttpPost]
        public ActionResult EditCost(string id, string cost, string costdescription, string acceptpaypal, string acceptcreditcard, string allowtopaylater, string cmd, string hassubscriptionplan, string subscriptionplancost, string subscriptionplanperiod, string haspaymentplan, string paymentplancost, string paymentplanperiod, string paymentplanperiodnumber, string paymentplanpaymentstarttype, string splitcostevenly, string regfeedownpayment)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid());

            if (id.IsGuid())
            {
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
                    { "haspaymentplan", haspaymentplan.IfBlank("0") },
                    { "paymentplancost", paymentplancost },
                    { "paymentplanperiod", paymentplanperiod },
                    { "paymentplanperiodnumber", paymentplanperiodnumber },
                    { "paymentplanpaymentstarttype", paymentplanpaymentstarttype },
                    { "splitcostevenly", splitcostevenly.IsBoolean().ToString() },
                    { "regfeedownpayment", regfeedownpayment.IsBoolean().ToString() },
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                    account: SessionVariables.CurrentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs
                );
            }

            if (msgs.HasNoErrors())
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
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
            var activity = Activity.LoadByUid(id.ToGuid());

            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = SessionVariables.CurrentAccount;
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
            var msgs = new SystemMessages();
            Activity activity = Activity.LoadByUid(id.ToGuid());

            if (id.IsGuid())
            {
                var values = new Dictionary<string, object>()
                {
                    { "registrationthankyouemail", registrationthankyouemail },
                    { "interestthankyouemail", interestthankyouemail },
                    { "applicationthankyouemail", applicationthankyouemail }
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                    account: SessionVariables.CurrentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs
                );
            }

            if (msgs.HasNoErrors())
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
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
                        if (!MustRequestToPublish(activity))
                            return RedirectToAction("publish", new { id = activity.Uid, isnew = Request["isnew"], source = Request["source"] });

                        return RedirectToAction("requesttopublish", new { id = activity.Uid, isnew = Request["isnew"], source = Request["source"] });
                }
            }

            ViewBag.ActiveMenu = "EditConfirmation";
            return View(activity);
        }

        [HttpPost]
        public ActionResult Copy(string id, string activitynamecopy, string usesameformcopy, string copywithregistrants, string type)
        {
            var msgs = new SystemMessages();
            var activityToCopy = Activity.LoadByUid(id.ToGuid());

            var activity = activityToCopy.Copy(
                            activitynamecopy,
                            true,
                            SessionVariables.CurrentUser.ID.Value,
                            msgs);

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
                return RedirectToAction("editsetup", new { id = activity.Uid, isnew = 1, source = Request["source"] });

            return RedirectToAction("index", "activity", new { copyerror = true, type });
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid());

            if (activity.IsNotEmpty())
                msgs.AddRange(activity.MarkAsDeleted(SessionVariables.CurrentUser.ID.Value));

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
                msgs.Add(string.Format("{0} has been deleted", activity.Name), MessageType.Success);

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "activity");
        }

        [HttpGet]
        public ActionResult Archive(string id)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid());

            if (activity.IsNotEmpty())
                msgs.AddRange(activity.MoveToArchive(SessionVariables.CurrentUser.ID.Value));

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
                msgs.Add(string.Format("{0} has been archived", activity.Name), MessageType.Success);

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "activity");
        }

        [HttpGet]
        public ActionResult Restore(string id)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid());

            if (activity.IsNotEmpty())
                msgs.AddRange(activity.MoveFromArchive(SessionVariables.CurrentUser.ID.Value));

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
                msgs.Add(string.Format("{0} has been restored", activity.Name), MessageType.Success);

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "activity");
        }

        [HttpGet]
        public ActionResult Attendance(string id)
        {
            var activity = Activity.LoadByUid(id.ToGuid());

            ViewBag.Messages = new SystemMessages();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            return View(activity);
        }

        [HttpGet]
        public ActionResult TrackAttendance(string id, string att)
        {
            var msgs = new SystemMessages();
            var attendanceId = att.IsNumber() ? att.ToInteger() : new int?();
            var activity = Activity.LoadByUid(id.ToGuid());

            var attendance = activity.GetOrCreateNewAttendance(attendanceId, SessionVariables.CurrentUser, msgs);


            ViewBag.Messages = msgs;
            ViewBag.AttendanceRegistrants = attendance.GetAttendanceRegistrants();
            ViewBag.Schedule = activity.GetSchedule();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            return View(attendance);
        }

        [HttpPost]
        public ActionResult TrackAttendance(string id, string att, string activityinstance)
        {
            var msgs = new SystemMessages();
            var presentStudents = new List<int>();
            var activity = Activity.LoadByUid(id.ToGuid());
            var attendance = AfterSchoolLibrary.Models.Attendance.Load(att.ToInteger(), msgs);

            if (!activityinstance.IsNumber())
                msgs.Add("Activity Date is required", MessageType.UserError);

            attendance.GetAttendanceRegistrants(msgs).ForEach(attendanceRegistrant => {
                if (Request.Params[$"present-{attendanceRegistrant.ID.ToString()}"].IsNumber())
                    presentStudents.Add(attendanceRegistrant.ID.Value);
            });

            if (msgs.HasNoErrors())
                attendance.UpdateAttendance(activityinstance.ToInteger(), presentStudents, SessionVariables.CurrentUser, msgs);


            ViewBag.Messages = msgs;
            ViewBag.AttendanceRegistrants = attendance.GetAttendanceRegistrants();
            ViewBag.Schedule = activity.GetSchedule();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            return View(attendance);
        }

        [HttpGet]
        public ActionResult FinalizeRegistration(string id, string atype)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid(), msgs);

            activity.FinalizeRegistrations(SessionVariables.CurrentUser, msgs);

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
            var activity = Activity.LoadByUid(id.ToGuid());
            RegistrantsViewBag(activity, atype, msg);
            return View(activity);
        }

        [HttpGet]
        public ActionResult MoveRegistrant(string id, string registrant, string assignmenttype)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid(), msgs);
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
                            SessionVariables.CurrentUser,
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
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid());

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
            var activity = Activity.LoadByUid(id.ToGuid());
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
            {
                row.Add(question.QuestionDataTypeId.ToString());
            }

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

            ViewBag.Messages = new SystemMessages();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            return View("Registrants", activity);
        }

        private List<AttachedCoupon> GetAttachedCoupons(RegistrantActivity registrantActivity)
        {
            var masterRegistrant = registrantActivity.GetRegistrant().GetMasterRegistrant();
            return masterRegistrant?.GetAccount().GetAttachedCoupons() ?? new List<AttachedCoupon>();
        }

        [HttpGet]
        public ActionResult RegistrantDetail(string id)
        {
            var registrantActivity = RegistrantActivity.LoadByUid(id.ToGuid());
            var activity = Activity.Load(registrantActivity.ParticipationObjectId.Value);

            ViewBag.Messages = new SystemMessages();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AttachedCoupons = GetAttachedCoupons(registrantActivity);
            return View(registrantActivity);
        }

        [HttpPost]
        public ActionResult AddPayment(string id, string datepaid, string amountpaid, string paymenttype)
        {
            var msgs = new SystemMessages();
            var registrantActivity = RegistrantActivity.LoadByUid(id.ToGuid());
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
                        BillingAccount = SessionVariables.CurrentAccount,
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
            var registrantActivity = RegistrantActivity.LoadByUid(id.ToGuid());
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

            if (msgs.HasNoErrors())
            {
                var registrantAccount = registrantActivity.GetRegistrant().GetMasterRegistrant()?.GetAccount();
                AttachedCoupon attachedCoupon = null;

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        // create new coupon
                       var coupon = SessionVariables.CurrentAccount.CreateCoupon(
                            amount: couponAmount,
                            expirationDate: expirationDate,
                            amountType: discountType,
                            applyType: couponApplyType,
                            durationInMonths: durationInMonths,
                            couponDuration: duration,
                            loggedInUser: SessionVariables.CurrentUser,
                            msgs: amsgs,
                            dc: adc);

                        // assign coupon to the activity (only works for this activity)
                        var poc = PurchaseableObjectCouponService.CreateNew(
                            purchaseableObject: activity.GetPurchaseableObject(), 
                            coupon: coupon, 
                            loggedInUser: SessionVariables.CurrentUser, 
                            msgs: amsgs, 
                            dc: adc);

                        attachedCoupon = registrantAccount.AttachCoupon(coupon, SessionVariables.CurrentUser, amsgs, adc);
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                   
                        attachedCoupon.Send();                   
                    },
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(msgs)
                );

            }

            ViewBag.Messages = msgs;
            ViewBag.AttachedCoupons = GetAttachedCoupons(registrantActivity);
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            return View("RegistrantDetail", registrantActivity);
        }        

        [HttpGet]
        public ActionResult PublishDetail(string id)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid(), msgs);
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.MustRequestToPublish = MustRequestToPublish(activity);
            ViewBag.ShowCopyButton = ShowCopyButtonOnPublishDetail(activity);
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.Messages = msgs;
            return View(activity);
        }

        [HttpGet]
        public ActionResult RequestToPublish(string id)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid());

            activity.RequestToPublish(SessionVariables.CurrentUser, msgs);

            if (msgs.HasNoErrors())
                msgs.Add("Your request to publish has been submitted", MessageType.Success);

            ViewBag.Messages = msgs;
            ViewBag.MustRequestToPublish = MustRequestToPublish(activity);
            ViewBag.ShowCopyButton = ShowCopyButtonOnPublishDetail(activity);
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.IsFullScreen = Request["isnew"] == "1";
            return View("publishdetail", activity);
        }

        private void BeforePublish(Activity activity, SystemMessages msgs)
        {
            // check if provided by external provider
            var location = activity.GetLocation(msgs);
            var isLoggedInAccountOwnerAccount = SessionVariables.CurrentAccount.ID == location?.HostAccountId;
            var activityIsHostedAtAnotherAccount = location?.HostAccountId != activity.AccountId;

            // activity is created in the activity provider's account
            if (isLoggedInAccountOwnerAccount && activityIsHostedAtAnotherAccount)
            {
                var visitingAccount = activity.GetAccount(msgs);

                if (!visitingAccount.IsApprovedAsVisitorTo(SessionVariables.CurrentAccount, msgs))
                {
                    SessionVariables.CurrentAccount.ApproveAccessToAccount(visitingAccount, SessionVariables.CurrentUser, msgs);

                    if (msgs.HasNoErrors())
                        SessionVariables.CurrentAccount.SendApprovalNotificationToVisitor(visitingAccount, msgs);
                }
            }
        }

        [HttpGet]
        public ActionResult Publish(string id)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid());
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
                msgs.Add(string.Format("{0} has been published", activity.Name), MessageType.Success);
                UpdateSession(SessionActionType.ActivityPublish, SessionVariables.CurrentAccount.ID, activity.ID);
            }

            ViewBag.Messages = msgs;
            ViewBag.IsFullScreen = Request["isnew"] == "1";
            ViewBag.MustRequestToPublish = mustRequestToPublish;
            ViewBag.ShowCopyButton = ShowCopyButtonOnPublishDetail(activity);
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            return View("publishdetail", activity);
        }

        [HttpGet]
        public ActionResult Unpublish(string id)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid());
            var mustRequestToPublish = MustRequestToPublish(activity);

            if (mustRequestToPublish)
                msgs.Add("You do not have permissions to unpublish", MessageType.UserError);

            if (msgs.HasNoErrors())
                activity.Unpublish(SessionVariables.CurrentUser, msgs);

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been unpublished", activity.Name), MessageType.Success);
                UpdateSession(SessionActionType.ActivityUnpublish, SessionVariables.CurrentAccount.ID, activity.ID);
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
            var activity = Activity.LoadByUid(id.ToGuid());
            var activityForm = activity.GetActiveActivityForm();

            var questionGroups = activityForm.GetForm().GetQuestionGroups();
            var individualGroups = questionGroups.Where(x => x.RegistrantTypeId == RegistrantType.Individual).ToList();
            var organizationGroups = questionGroups.Where(x => x.RegistrantTypeId == RegistrantType.Organization).ToList();

            FormQuestionGroup questionGroup = null;

            if (qg.IsNumber())
            {
                questionGroup = FormQuestionGroup.Load(qg.ToInteger());
            }
            else
            {
                questionGroup = questionGroups.First();
            }

            ViewBag.Messages = new SystemMessages();
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
            SystemMessages msgs = new SystemMessages();

            if (name.IsEmpty() && qg.IsBlank())
            {
                msgs.Add("Name is required", MessageType.UserError);
            }
            if (description.IsEmpty() && qg.IsBlank())
            {
                msgs.Add("Description is required", MessageType.UserError);
            }

            var activity = Activity.LoadByUid(id.ToGuid());
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
                                {
                                    question.IsActive = true;
                                }

                                question.IsRequired = false;
                                if (Request.Params[string.Format("required-{0}", question.ID)].IsBoolean())
                                {
                                    question.IsRequired = true;
                                }

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
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid(), msgs);

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
            var am = ActivityMessage.LoadByUid(id.ToGuid());
            var activity = Activity.Load(am.ActivityId.Value);
            ViewBag.Messages = new SystemMessages();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.Registrants = am.GetActivityMessageRegistrants();
            return View(am);
        }

        [HttpGet]
        public ActionResult MessageAdd(string id)
        {
            var activity = Activity.LoadByUid(id.ToGuid());

            ViewBag.Messages = new SystemMessages();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.Registrants = activity.GetRegistrants()
                                    .OrderBy(x => x.FirstName)
                                    .ToList();
            return View(activity);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult MessageAdd(string id, string deliveryMethod, string subject, string body, string sendtostaff)
        {
            var msgs = new SystemMessages();

            if (subject.IsEmpty())
                msgs.Add("Message Reason is required", MessageType.UserError);
            if (body.IsEmpty())
                msgs.Add("Message Body is required", MessageType.UserError);

            ActivityMessage am = null;
            var activity = Activity.LoadByUid(id.ToGuid());

            if (msgs.HasNoErrors())
            {
                var registrantIds = activity.GetRegistrants(msgs)
                                        .Where(x => Request[$"registrant-{x.ID.ToString()}"].IsNotBlank())
                                        .Select(x => x.ID.Value)
                                        .ToList();

                am = activity.AddMessage(
                        subject, 
                        body, 
                        registrantIds, 
                        sendtostaff.IsBoolean(), 
                        SessionVariables.CurrentUser.ID.Value, 
                        msgs);
                
                BackgroundJob.Enqueue(() => ActivityMessageService.Send(am.ID.Value, SessionVariables.CurrentUser.ID.Value, null, null));
            }

            ViewBag.Messages = msgs;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);

            if (msgs.HasNoErrors())
            {
                msgs.Add("Your message will been sent to the selected registrants", MessageType.Success);
                ViewBag.Registrants = am.GetActivityMessageRegistrants();
                ViewBag.Messages = msgs;
                ViewBag.IsActivityLeader = IsActivityLeader(activity);

                return View("messagedetail", am.Reload());
            }

            ViewBag.Messages = msgs;
            ViewBag.Registrants = activity.GetRegistrants();
            return View(activity);
        }

        [HttpGet]
        public ActionResult Team(string id)
        {
            var activity = Activity.LoadByUid(id.ToGuid());
            var msgs = new SystemMessages();

            ViewBag.Users = SessionVariables.CurrentAccount.GetUsers(msgs: msgs);
            ViewBag.Messages = msgs;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            return View(activity);
        }

        [HttpPost]
        public ActionResult TeamSave(string id)
        {
            var msgs = new SystemMessages();
            Activity activity = null;
            Account account = null;
            List<WebUser> users = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) => {

                    activity = Activity.LoadByUid(id.ToGuid(), amsgs, adc);
                    account = activity.GetAccount(amsgs, adc);
                    users = account.GetUsers(msgs: amsgs, dc: adc);

                    users.ForEach(user =>
                    {
                        var doNotSkip = !(activity.CreatedBy.Value == user.ID.Value || SessionVariables.CurrentUser.ID == user.ID);

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
                                    awu.CreatedBy = SessionVariables.CurrentUser.ID;
                                }

                                if (Request["isleader-" + user.ID.Value.ToString()].IsNotEmpty())
                                    awu.IsLeader = true;
                                else
                                    awu.IsLeader = false;

                                awu.LastModifiedDate = DateTime.UtcNow;
                                awu.LastModifiedBy = SessionVariables.CurrentUser.ID;
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
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid(), msgs);
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
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid(), msgs);
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
                SessionVariables.CurrentAccount.CreateNewRegistrantAndRegistration(
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

        private bool MustRequestToPublish(Activity activity)
        {
            return !SessionVariables.CurrentUser.CanPublishActivity(SessionVariables.CurrentAccount, activity);
        }

        private bool IsExternallyProvided(Activity activity)
        {
            var location = activity.GetLocation();
            var hostAccount = location?.GetHostAccount();

            if (hostAccount.IsNotEmpty() && hostAccount.OwnerId.HasValue)
                return hostAccount.ID != activity.AccountId;

            return false;
        }

        private bool ShowCopyButtonOnPublishDetail(Activity activity)
        {
            return activity.AccountId == SessionVariables.CurrentAccount.ID || !IsExternallyProvided(activity);
        }
    }
}
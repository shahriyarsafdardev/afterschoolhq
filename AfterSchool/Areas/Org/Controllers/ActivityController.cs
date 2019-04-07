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
        public ActivityController() : base(new List<AccountType> { AccountType.School, AccountType.ActivityProvider })
        {

        }

        // GET: Admin/Activity
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
        public ActionResult Index(string group, string pn, string rpp, string archive, string copyerror)
        {
            var msgs = new SystemMessages();
            int pageNumber = 1;
            int recordsPerPage = 10;
            bool isArchived = false;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            if (archive.IsBoolean())
                isArchived = archive.ToBoolean();

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
                totalActivityCount = SessionVariables.CurrentAccount.GetActivityCount(SessionVariables.AccountUserType, SessionVariables.LoggedInUser, ActivityType.Activity, isArchived, false, msgs);
                activities = SessionVariables.CurrentAccount.GetActivities(SessionVariables.AccountUserType, SessionVariables.LoggedInUser, ActivityType.Activity, isArchived, false, start, length, msgs);
                hasArchivedActivities = SessionVariables.CurrentAccount.GetActivityCount(SessionVariables.AccountUserType, SessionVariables.LoggedInUser, ActivityType.Activity, true, false, msgs) > 0;
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
        public ActionResult EditSetup(string id, string group)
        {
            var msgs = new SystemMessages();
            ActivityGroup activityGroup = null;
            Activity activity = null;

            if (id.IsGuid())
            {
                activity = Activity.LoadByUid(id.ToGuid(), msgs);
                activityGroup = activity.GetActivityGroup(msgs);
            }
            else
            {
                activity = SessionVariables.CurrentAccount.GetDefaultNewActivity();
                activity.RegistrationModeId = RegistrationMode.Register;

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
        public ActionResult EditSetup(string id, string name, string seatsavailable, string waitinglistseats, string registrationperiod, string registrationopendate, string registrationopentime, string registrationclosedate, string registrationclosetime, string timezone, string form, string regmode, string group, string cmd)
        {
            var msgs = new SystemMessages();
            Activity activity = null;
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
                { "group", group }
            };

            activity = Activity.LoadByUid(id.ToGuid());
            if (activity.IsNotEmpty())
            {
                activity = ActivityService.EditExisting(
                    loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
                    account: SessionVariables.CurrentAccount,
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
                    var defaultForm = SessionVariables.CurrentAccount.GetDefaultForm();
                    var newForm = defaultForm.Copy(SessionVariables.LoggedInUser.ID.Value, SessionVariables.CurrentAccount.ID.Value, (name + " form").MaxChars(100, "..."), msgs);
                    if (msgs.HasNoErrors())
                        values.Add("form", newForm.ID.Value.ToString());
                }

                values.Add("registrationthankyouemail", "Thank you for registering");
                values.Add("applicationthankyouemail", "Thank you for applying");

                activity = ActivityService.CreateNew(
                    loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
                    account: SessionVariables.CurrentAccount,
                    values: values,
                    msgs: msgs
                );

                if (msgs.HasNoErrors())
                    UpdateSession(SessionActionType.ActivityCreate, SessionVariables.CurrentAccount.ID, activity.ID);
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
                        ViewBag.ActiveMenu = "EditRestrictions";
                        return View("EditRestrictions", activity);
                }
            }

            ViewBag.ActiveMenu = "EditSetup";
            return View(activity);
        }

        private void LoadEditSetupViewBag(Activity activity, ActivityGroup activityGroup, SystemMessages msgs)
        {
            var activeForm = activity.GetActiveActivityForm(msgs) ?? activityGroup?.GetDefaultActivityForm(msgs) ?? SessionVariables.CurrentAccount.GetDefaultActivityForm(msgs);
            var periods = SessionVariables.CurrentAccount.GetOpenAndUpcomingRegistrationPeriods(msgs);
            var activePeriod = activity.GetActiveRegistrationPeriod(msgs) ?? SessionVariables.CurrentAccount.GetDefaultNewRegistrationPeriod();

            periods.Add(activePeriod);
            periods = periods.OrderBy(period => period.StartDateTime.Value).ToList();

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = SessionVariables.CurrentAccount.GetForms(msgs).OrderBy(x => x.Name).ToList();
            ViewBag.Periods = periods;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.ActivePeriod = activePeriod;
            ViewBag.ActiveForm = activeForm;
            ViewBag.IsFullScreen = GetIsFullScreen(activity, activityGroup?.Uid.ToString());
        }

        private bool GetIsFullScreen(Activity activity, string groupUid = "")
        {
            ViewBag.FullScreenExitUrl = Url.Action("index", "activity");

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
        public ActionResult EditRestrictions(string id, string alloworganization, string agerestrictions, string agestart, string ageend, string graderestrictions, string mingrade, string maxgrade, string genderrestrictions, string cmd)
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
                    { "gradeIds", gradeIds.ToArray().ToStringArray().ToStringList() }
                };

                activity = ActivityService.EditExisting(
                        loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
                        account: SessionVariables.CurrentAccount,
                        activityId: activity.ID.Value.ToString(),
                        values: values,
                        msgs: msgs
                    );
            }

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} Saved", UILabel.ActivityLabel(activity.ActivityTypeId)), MessageType.Success);
            }

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
        public ActionResult EditContact(string id, string leadertype, string contactname, string contactphone, string contactemail, string instructor, string instructorfirstname, string instructorlastname, string instructoremail, string instructorphone, string cmd)
        {
            var msgs = new SystemMessages();
            Activity activity = null;

            var values = new Dictionary<string, object>()
            {
                { "leadertype", leadertype },
                { "contactname", contactname},
                { "contactphone", contactphone},
                { "contactemail", contactemail},
                { "instructor", instructor },
                { "instructorfirstname", instructorfirstname },
                { "instructorlastname", instructorlastname },
                { "instructoremail", instructoremail },
                { "instructorphone", instructorphone }
            };

            activity = Activity.LoadByUid(id.ToGuid(), msgs);
            if (activity.IsNotEmpty())
            {
                activity = ActivityService.EditExisting(
                    loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
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
            ViewBag.Instructors = SessionVariables.CurrentAccount.GetUsers(msgs: msgs);
            ViewBag.IsFullScreen = GetIsFullScreen(activity);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        ViewBag.ActiveMenu = "EditDetails";
                        return View("EditDetails", activity);
                    case "next":
                        ViewBag.ActiveMenu = "EditLocation";
                        return View("EditLocation", activity);
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

            activity = Activity.LoadByUid(id.ToGuid());
            if (activity.IsNotEmpty())
            {
                var values = new Dictionary<string, object>()
                {
                    { "details", details},
                    { "description", description}
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
                    account: SessionVariables.CurrentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs
                );
            }

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} Saved", UILabel.ActivityLabel(activity.ActivityTypeId)), MessageType.Success);
            }

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;

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
                        ViewBag.IsFullScreen = GetIsFullScreen(activity);
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
        public ActionResult EditSchedule(string id, string cmd, string schedule, string scheduledetails, string requiresalldays, string repeatsweekly, string startdate, string enddate, string dailystarttime, string dailyendtime, string isongoing)
        {
            var msgs = new SystemMessages();
            Activity activity = null;
            var inSessionDates = new List<DateRange>();
            var outOfSessionDates = new List<DateTime>();

            activity = Activity.LoadByUid(id.ToGuid());
            if (activity.IsNotEmpty())
            {
                var values = new Dictionary<string, object>();

                var filteredParams = Request.Params.Filter("schedulerepeat-");
                var days = ActivityScheduleUI.PrepDaysOfWeek(filteredParams);
                inSessionDates = ActivityScheduleUI.PrepScheduleDates(repeatsweekly, days, startdate, enddate, dailystarttime, dailyendtime, Request.Params.Filter("meeting-open-"));
                outOfSessionDates = ActivityScheduleUI.PrepScheduleDatesClosed(Request.Params.Filter("meeting-closed-"));

                values = new Dictionary<string, object>()
                {
                    { "scheduledetails", scheduledetails},
                    { "days", days.Select(x => (int)x).ToArray().ToStringArray().ToStringList() },
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
                    loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
                    account: SessionVariables.CurrentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs
                );
            }

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} Schedule Saved", UILabel.ActivityLabel(activity.ActivityTypeId)), MessageType.Success);
            }

            var instances = new List<ActivityInstance>();
            ActivitySchedule sched = null;
            if (msgs.HasNoErrors())
            {
                sched = activity.GetSchedule();
            }
            else
            {
                sched = ActivitySchedule.GetNewDefaultSchedule(inSessionDates);
            }

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
                        ViewBag.ActiveMenu = "EditDetails";
                        return View("EditDetails", activity);
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
        public ActionResult EditLocation(string id, string location, string locationname, string locationdetails, string address1, string address2, string city, string state, string postalcode, string mylocation, string locationtype, string cmd)
        {
            var msgs = new SystemMessages();
            Activity activity = Activity.LoadByUid(id.ToGuid());

            //if (locationdetails.IsEmpty())
            //{
            //    msgs.Add("Please specificy location instructions", MessageType.UserError);
            //}

            if (activity.IsNotEmpty() && msgs.HasNoErrors())
            {
                var values = new Dictionary<string, object>()
                {
                    { "location", location},
                    { "locationname", locationname},
                    { "locationdetails", locationdetails},
                    { "address1", address1},
                    { "address2", address2},
                    { "city", city},
                    { "state", state},
                    { "postalcode", postalcode},
                    { "mylocation", mylocation },
                    { "locationtype", locationtype }
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
                    account: SessionVariables.CurrentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs
                );
            }

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} Saved", UILabel.ActivityLabel(activity.ActivityTypeId)), MessageType.Success);
            }

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
        public ActionResult EditCost(string id, string cost, string costdescription, string acceptpaypal, string acceptcreditcard, string allowtopaylater, string cmd, string hassubscriptionplan, string subscriptionplancost, string subscriptionplanperiod, string haspaymentplan, string paymentplancost, string paymentplanperiod, string paymentplanperiodnumber, string provider, string providername, string providerfirstname, string providerlastname, string provideremail, string providerphone)
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
                    { "provider", provider },
                    { "providername", providername },
                    { "providerfirstname", providerfirstname },
                    { "providerlastname", providerlastname },
                    { "provideremail", provideremail },
                    { "providerphone", providerphone }
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
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
                        return RedirectToAction("publish", "activity", new { id = activity.Uid, isnew = Request["isnew"] });
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
        public ActionResult EditConfirmation(string id, string registrationthankyouemail, string applicationthankyouemail, string cmd)
        {
            var msgs = new SystemMessages();
            Activity activity = Activity.LoadByUid(id.ToGuid());

            if (id.IsGuid())
            {
                var values = new Dictionary<string, object>()
                {
                    { "registrationthankyouemail", registrationthankyouemail },
                    { "applicationthankyouemail", applicationthankyouemail }
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
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
                        return RedirectToAction("publish", new { id = activity.Uid, isnew = Request["isnew"] });
                }
            }

            ViewBag.ActiveMenu = "EditConfirmation";
            return View(activity);
        }

        [HttpPost]
        public ActionResult Copy(string id, string activitynamecopy, string usesameformcopy, string copywithregistrants)
        {
            var msgs = new SystemMessages();
            var activityToCopy = Activity.LoadByUid(id.ToGuid());

            Activity activity = null;

            if (copywithregistrants.IsBoolean())
                activity = activityToCopy
                    .CopyWithRegistrants(activitynamecopy, true, SessionVariables.LoggedInUser.ID.Value, msgs);
            else
                activity = activityToCopy
                    .Copy(activitynamecopy, true, SessionVariables.LoggedInUser.ID.Value, msgs);

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
                return RedirectToAction("editsetup", new { id = activity.Uid, isnew = 1 });

            return RedirectToAction("index", "activity", new { copyerror = true });
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid());

            if (activity.IsNotEmpty())
            {
                msgs.AddRange(activity.MarkAsDeleted(SessionVariables.LoggedInUser.ID.Value));
            }

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been deleted", activity.Name), MessageType.Success);
            }

            if (Request.UrlReferrer.IsNotEmpty())
            {
                Response.Redirect(Request.UrlReferrer.ToString(), true);
            }

            return RedirectToAction("index", "activity");
        }

        [HttpGet]
        public ActionResult Archive(string id)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid());

            if (activity.IsNotEmpty())
            {
                msgs.AddRange(activity.MoveToArchive(SessionVariables.LoggedInUser.ID.Value));
            }

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been archived", activity.Name), MessageType.Success);
            }

            if (Request.UrlReferrer.IsNotEmpty())
            {
                Response.Redirect(Request.UrlReferrer.ToString(), true);
            }


            return RedirectToAction("index", "activity");
        }

        [HttpGet]
        public ActionResult Restore(string id)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid());

            if (activity.IsNotEmpty())
            {
                msgs.AddRange(activity.MoveFromArchive(SessionVariables.LoggedInUser.ID.Value));
            }

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been restored", activity.Name), MessageType.Success);
            }

            if (Request.UrlReferrer.IsNotEmpty())
            {
                Response.Redirect(Request.UrlReferrer.ToString(), true);
            }

            return RedirectToAction("index", "activity");
        }

        //[HttpPost]
        //public ActionResult NewOffering(string id, string activitynameoffer)
        //{
        //    var msgs = new SystemMessages();
        //    var activity = Activity.LoadByUid(id.ToGuid());
        //    var childActivity = activity.CreateChild(activitynameoffer, SessionVariables.LoggedInUser.ID.Value, msgs);

        //    ViewBag.Messages = msgs;
        //    if (msgs.HasNoErrors())
        //    {
        //        return RedirectToAction("editsetup", new { id = childActivity.ID });
        //    }
        //    else
        //    {
        //        return RedirectToAction("index", "activity");
        //    }
        //}

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

            var attendance = activity.GetOrCreateNewAttendance(attendanceId, SessionVariables.LoggedInUser, msgs);


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
                attendance.UpdateAttendance(activityinstance.ToInteger(), presentStudents, SessionVariables.LoggedInUser, msgs);


            ViewBag.Messages = msgs;
            ViewBag.AttendanceRegistrants = attendance.GetAttendanceRegistrants();
            ViewBag.Schedule = activity.GetSchedule();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            return View(attendance);
        }

        [HttpGet]
        public ActionResult Registrants(string id, string atype)
        {
            var activity = Activity.LoadByUid(id.ToGuid());
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

            ViewBag.Messages = new SystemMessages();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.Registrants = ras;
            ViewBag.AssignmentType = rat;
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
                            SessionVariables.LoggedInUser,
                            amsgs,
                            adc);
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    },
                    onException: (SystemMessages emsgs) =>
                    {
                        AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs);
                    }
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
            var registrantActivity = RegistrantActivity.Load(id.ToInteger());
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
            var registrantActivity = RegistrantActivity.Load(id.ToInteger());
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
                        PaymentSourceType = PaymentSourceType.Manual,
                        OverrideAmount = amountpaid.ToDecimal()
                    },
                    loggedInUser: SessionVariables.LoggedInUser,
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
            var registrantActivity = RegistrantActivity.Load(id.ToInteger());
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
                            loggedInUser: SessionVariables.LoggedInUser,
                            msgs: amsgs,
                            dc: adc);

                        // assign coupon to the activity (only works for this activity)
                        var poc = PurchaseableObjectCouponService.CreateNew(
                            purchaseableObject: activity.GetPurchaseableObject(), 
                            coupon: coupon, 
                            loggedInUser: SessionVariables.LoggedInUser, 
                            msgs: amsgs, 
                            dc: adc);

                        attachedCoupon = registrantAccount.AttachCoupon(coupon, SessionVariables.LoggedInUser, amsgs, adc);
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                   
                        attachedCoupon.Send();                   
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    },
                    onException: (SystemMessages emsgs) =>
                    {
                        msgs.AddRange(emsgs);
                    }
                );

            }

            ViewBag.Messages = msgs;
            ViewBag.AttachedCoupons = GetAttachedCoupons(registrantActivity);
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            return View("RegistrantDetail", registrantActivity);
        }

        [HttpGet]
        public ActionResult RegistrantImport(string id)
        {
            var activity = Activity.LoadByUid(id.ToGuid());
            ViewBag.Step = 1;
            ViewBag.StepLabel = "Upload";
            ViewBag.Messages = new SystemMessages();
            return View(activity);
        }

        [HttpPost]
        public ActionResult RegistrantImport(string id, HttpPostedFileBase importfile, string csv)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid());
            var af = activity.GetActiveActivityForm();
            string extension = "";
            var questions = af.GetForm().GetAllQuestions();

            if (csv.IsBlank())
            {
                if (importfile.IsEmpty())
                {
                    msgs.Add("Please upload a file", MessageType.UserError);
                }
                else if (Path.GetExtension(importfile.FileName).NotIn(new string[] { ".xls", ".csv", ".xlsx" }))
                {
                    msgs.Add("Please upload a valid file", MessageType.UserError);
                }
                else
                {
                    extension = Path.GetExtension(importfile.FileName);
                }

                if (msgs.HasNoErrors())
                {
                    ViewBag.Step = 2;
                    ViewBag.StepLabel = "Review";

                    //import
                    var sr = new StreamReader(importfile.InputStream);

                    List<string> headers = new List<string>();
                    List<string> headersWithData = new List<string>();
                    List<List<string>> rows = new List<List<string>>();
                    List<List<ImportField>> validatedRows = new List<List<ImportField>>();

                    if (extension == ".csv")
                    {
                        CsvHelper.CsvReader reader = new CsvHelper.CsvReader(sr);
                        reader.Configuration.Delimiter = ",";
                        reader.Configuration.HasHeaderRecord = true;
                        reader.Configuration.TrimOptions = CsvHelper.Configuration.TrimOptions.Trim;

                        while (reader.Read())
                        {
                            if (headers.Count == 0)
                            {
                                headers = reader.Context.HeaderRecord.ToList();
                            }

                            var row = new List<string>();
                            for (var i = 0; i < headers.Count; i++)
                            {
                                var header = headers[i];
                                var field = reader.GetField<string>(i);
                                if (header.ToLower().In(questions.Select(x => x.QuestionDataTypeId.ToString().ToLower())) && field.IsNotBlank())
                                {
                                    row.Add(field);
                                    if (!headersWithData.Contains(header))
                                    {
                                        headersWithData.Add(header);
                                    }
                                }
                            }

                            rows.Add(row);
                        }
                    }

                    foreach (var row in rows)
                    {
                        var validatedRow = new List<ImportField>();

                        for (var i = 0; i < headersWithData.Count; i++)
                        {
                            var validatedField = new ImportField();
                            validatedField.Value = row[i];

                            var field = row[i].ToLower();
                            var question = questions.Where(x => x.QuestionDataTypeId.ToString().ToLower() == headersWithData[i].ToLower()).First();

                            validatedField.Value = field;

                            if (field.Length > 1000)
                            {
                                validatedField.Messages.Add("Must be less than or equal to 1000 characters. Data will be truncated.", MessageType.Warning);
                            }

                            switch (question.QuestionTypeId)
                            {
                                case QuestionType.Date:

                                    if (field.IsNotBlank() && !field.IsDateTime())
                                    {
                                        validatedField.Messages.Add("Must be a valid date", MessageType.UserError);
                                    }

                                    break;

                                case QuestionType.Checkbox:

                                    if (field.IsNotBlank() && !field.IsBoolean())
                                    {
                                        validatedField.Messages.Add("Must be one of the following values: 0, 1, T, F, True, False", MessageType.UserError);
                                    }

                                    break;

                                case QuestionType.TextBox:
                                case QuestionType.TextArea:

                                    break;

                                case QuestionType.Email:

                                    if (field.IsNotBlank() && !field.IsMatch(BasicLibrary.Utilities.EmailRegEx))
                                    {
                                        validatedField.Messages.Add("Must be a valid email address", MessageType.UserError);
                                    }

                                    break;

                                case QuestionType.DropDownList:
                                case QuestionType.RadioList:

                                    if (field.IsNotBlank())
                                    {
                                        var options = question.Options.Split(',').Select(x => x.Trim().ToLower()).Where(x => x.Length > 0);
                                        
                                        if (field.NotIn(options))
                                        {
                                            validatedField.Messages.Add("Must be one of the following values: " + question.Options.Replace(",", ", "), MessageType.UserError);
                                        }
                                    }

                                    break;

                                case QuestionType.CheckboxList:

                                    if (field.IsNotBlank())
                                    {
                                        var options = question.Options.Split(',').Select(x => x.Trim().ToLower());
                                        var fieldvalues = field.Split(',').Select(x => x.Trim().ToLower()).Where(x => x.Length > 0);

                                        foreach (var fieldvalue in fieldvalues)
                                        {
                                            if (fieldvalue.NotIn(options))
                                            {
                                                validatedField.Messages.Add("Must be one of the following values: " + question.Options.Replace(",", ", "), MessageType.UserError);
                                            }
                                        }
                                    }

                                    break;
                            }

                            validatedRow.Add(validatedField);
                        }

                        validatedRows.Add(validatedRow);
                    }

                    var csvString = new System.Text.StringBuilder();
                    foreach (var header in headersWithData)
                    {
                        csvString.Append(header);
                        csvString.Append(",");
                    }
                    csvString.Append("\n");

                    foreach (var row in rows)
                    {
                        foreach (var field in row)
                        {
                            csvString.Append(field);
                            csvString.Append(",");
                        }
                        csvString.Append("\n");
                    }

                    ViewBag.Headers = headersWithData;
                    ViewBag.Rows = validatedRows;
                    ViewBag.Csv = csvString.ToString();
                }

            }
            else
            {
                ViewBag.Step = 3;
                ViewBag.StepLabel = "Import Complete";

                //import
                var sr = new StringReader(csv);

                List<string> headers = new List<string>();
                List<string> headersWithData = new List<string>();
                List<List<string>> rows = new List<List<string>>();
                List<List<ImportField>> validatedRows = new List<List<ImportField>>();

                //do import
                CsvHelper.CsvReader reader = new CsvHelper.CsvReader(sr);
                reader.Configuration.Delimiter = ",";
                reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.TrimOptions = CsvHelper.Configuration.TrimOptions.Trim;

                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) => {
                        while (reader.Read())
                        {
                            if (headers.Count == 0)
                            {
                                headers = reader.Context.HeaderRecord.ToList();
                            }

                            //assuming 1 seat per registrant line for now
                            var seatsRequested = 1;
                            var seatsLeft = activity.GetSeatsRemaining();
                            var seatCount = 0;
                            var waitingListSeatCount = 0;
                            var applicationSeatCount = 0;
                            var invitationSeatCount = 0;
                            var declinedSeatCount = 0;

                            if (seatsRequested > seatsLeft)
                            {
                                seatCount = 0;
                                waitingListSeatCount = 1;
                            }
                            else
                            {
                                seatCount = 1;
                                waitingListSeatCount = 0;
                            }

                            var hasSeatsAvailable = activity.GetRegistrantCount(amsgs, adc) < activity.SeatsAvailable;
                            var period = activity.GetCurrentRegistrationPeriod(amsgs, adc);

                            // TODO: need to verify if this really should be the master account. What is master
                            var registrant = SessionVariables.CurrentAccount.AddRegistrant(0, RegistrantType.Individual, true, amsgs, dc: adc);
                            var registration = registrant.AddRegistration(
                                SessionVariables.CurrentAccount.ID.Value,
                                activity.ActivityGroupId,
                                activity.AccountActivityGroupId,
                                period?.ID,
                                RegistrationCompletionStatus.RequiresParentSignature,
                                SessionVariables.LoggedInUser.ID.Value,
                                amsgs,
                                dc: adc);

                            var ra = registrant.AddOrUpdateParticipationObject(
                                registration.ID.Value, 
                                af.ActivityId.Value,
                                ParticipationObjectType.Activity,
                                RegistrationAssignmentType.Reserved,
                                period?.ID,
                                null,
                                SessionVariables.LoggedInUser.ID.Value,
                                amsgs, 
                                dc: adc);

                            for (var i = 0; i < headers.Count; i++)
                            {
                                var header = headers[i];
                                if (header.IsNotBlank())
                                {
                                    var field = reader.GetField<string>(i);
                                    var question = questions.Where(x => x.QuestionDataTypeId.ToString().ToLower() == headers[i].ToLower()).First();

                                    if (field.IsNotBlank())
                                    {
                                        var answer = registration.AddAnswer(field, question, registrant, 0, amsgs, dc: adc);
                                    }
                                }
                            }
                        }
                    },
                    onSuccess: (SystemMessages smsgs) => {
                        msgs.AddRange(smsgs);
                    },
                    onFailure: (SystemMessages fmsgs) => {
                        msgs.AddRange(fmsgs);
                    }
                );
            }

            ViewBag.Messages = msgs;
            return View(activity);
        }

        [HttpGet]
        public ActionResult GetImportSample(string id)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid());
            var af = activity.GetActiveActivityForm();

            var questions = af.GetForm().GetAllQuestions().OrderBy(x => (x.DisplayOrder.Value + 1) * (x.FormQuestionGroupDisplayOrder.Value + 1));

            var swriter = new StringWriter();
            var csvWriter = new CsvHelper.CsvWriter(swriter);
            csvWriter.Configuration.HasHeaderRecord = true;

            foreach (var question in questions)
            {
                csvWriter.WriteField(question.QuestionDataTypeId.ToString());
            }
            csvWriter.NextRecord();

            string csv = swriter.GetStringBuilder().ToString();

            Response.Clear();
            Response.Write(csv);
            Response.AddHeader("Content-Disposition", "attachment; filename=importsample.csv");
            Response.AddHeader("Content-Type", "text/csv");
            Response.AddHeader("Content-Length", System.Text.Encoding.UTF8.GetByteCount(csv).ToString());
            Response.End();

            return View();
        }

        [HttpGet]
        public ActionResult PublishDetail(string id)
        {
            var activity = Activity.LoadByUid(id.ToGuid());
            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = SessionVariables.CurrentAccount;
            return View(activity);
        }

        [HttpGet]
        public ActionResult Publish(string id)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid());

            activity.Publish(SessionVariables.LoggedInUser, msgs);

            if (msgs.HasNoErrors())
            {
                var activityId = activity.ID.Value;
                var loggedInUserId = SessionVariables.LoggedInUser.ID.Value;

                Hangfire.BackgroundJob.Enqueue(() => ActivityService.NotifyProviderOfNewActivity(activityId, loggedInUserId));

                msgs.Add(string.Format("{0} has been published", activity.Name), MessageType.Success);
                UpdateSession(SessionActionType.ActivityPublish, SessionVariables.CurrentAccount.ID, activity.ID);
            }

            ViewBag.Messages = msgs;
            ViewBag.IsFullScreen = Request["isnew"] == "1";
            return View("publishdetail", activity);
        }

        [HttpGet]
        public ActionResult Unpublish(string id)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUid(id.ToGuid());

            activity.Unpublish(SessionVariables.LoggedInUser, msgs);

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been unpublished", activity.Name), MessageType.Success);
                UpdateSession(SessionActionType.ActivityUnpublish, SessionVariables.CurrentAccount.ID, activity.ID);
            }

            ViewBag.Messages = msgs;
            return View("publishdetail", activity);
        }

        [HttpGet]
        public ActionResult Form(string id, string qg)
        {
            var activity = Activity.LoadByUid(id.ToGuid());
            var activityForm = activity.GetActiveActivityForm();

            //create form(s) if there is no form
            //if (activityForm.IsEmpty() && !activity.IsChildActivity)
            //{
            //    activity.CopyDefaultForm(SessionVariables.LoggedInUser.ID.Value);
            //    activityForm = activity.GetActiveActivityForm();

            //    List<Activity> children = activity.GetChildActivities();
            //    foreach (var child in children)
            //    {
            //        var af = child.AddFormAndActivitate(activityForm.ToForm(true), SessionVariables.LoggedInUser.ID.Value);
            //        af.IsPublished = activityForm.IsPublished;
            //        af.Save();
            //    }
            //}

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
                            questionGroup.LastModifiedBy = SessionVariables.LoggedInUser.ID;
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
        public ActionResult Messages(string id)
        {
            var activity = Activity.LoadByUid(id.ToGuid());
            ViewBag.Messages = new SystemMessages();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            return View(activity);
        }

        [HttpGet]
        public ActionResult MessageDetail(string id)
        {
            var am = ActivityMessage.Load(id.ToInteger());
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
        public ActionResult MessageAdd(string id,string deliveryMethod, string subject, string body)
        {
            var msgs = new SystemMessages();

            if (subject.IsEmpty())
                msgs.Add("Subject is required", MessageType.UserError);
            if (body.IsEmpty())
                msgs.Add("Body is required", MessageType.UserError);

            ActivityMessage am = null;
            var activity = Activity.LoadByUid(id.ToGuid());

            if (msgs.HasNoErrors())
            {
                var registrantIds = activity.GetRegistrants()
                    .Where(x => Request[$"registrant-{x.ID.ToString()}"].IsNotBlank())
                    .Select(x => x.ID.Value)
                    .ToList();
                am = activity.AddMessage(subject, body, registrantIds, SessionVariables.LoggedInUser.ID.Value, msgs);
                
                BackgroundJob.Enqueue(() => ActivityMessageService.Send(am.ID.Value, null));
            }

            ViewBag.Messages = msgs;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);

            if (msgs.HasNoErrors())
            {
                msgs.Add("Your message will been sent to the selected registrants", MessageType.Success);
                ViewBag.Registrants = am.GetActivityMessageRegistrants();
                ViewBag.Messages = msgs;
                ViewBag.IsActivityLeader = IsActivityLeader(activity);

                return View("messagedetail", am);
            }

            ViewBag.Messages = msgs;
            ViewBag.Registrants = activity.GetRegistrants();
            return View(activity);
        }

        [HttpGet]
        public ActionResult Team(string id)
        {
            var activity = Activity.LoadByUid(id.ToGuid());

            ViewBag.Messages = new SystemMessages();
            ViewBag.Users = SessionVariables.CurrentAccount.GetMembers().Select(x => x.ToWebUser()).ToList();
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

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) => {

                    activity = Activity.LoadByUid(id.ToGuid(), amsgs, adc);
                    account = activity.GetAccount(amsgs, adc);
                    users = account.GetMembers(adc).Select(x => x.ToWebUser()).ToList();

                    foreach (var user in users)
                    {
                        var doNotSkip = !(activity.CreatedBy.Value == user.ID.Value || SessionVariables.LoggedInUser.ID == user.ID);

                        if (doNotSkip)
                        {
                            var awu = user.GetActivityWebUser(activity, dc: adc);

                            if (Request["onteam-" + user.ID.Value.ToString()].IsNotEmpty())
                            {
                                if (awu.IsEmpty())
                                {
                                    awu = new ActivityWebUser();
                                    awu.ActivityId = activity.ID;
                                    awu.WebUserId = user.ID;
                                    awu.CreateDate = DateTime.UtcNow;
                                    awu.CreatedBy = SessionVariables.LoggedInUser.ID;
                                }

                                if (Request["isleader-" + user.ID.Value.ToString()].IsNotEmpty())
                                {
                                    awu.IsLeader = true;
                                }
                                else
                                {
                                    awu.IsLeader = false;
                                }

                                awu.LastModifiedDate = DateTime.UtcNow;
                                awu.LastModifiedBy = SessionVariables.LoggedInUser.ID;
                                amsgs.AddRange(awu.Save(adc));

                            }
                            else if (awu.IsNotEmpty())
                            {
                                amsgs.AddRange(awu.Delete(adc));
                            }
                        }
                    }
                },
                onSuccess: (SystemMessages smsgs) => {
                    msgs.AddRange(smsgs);
                    msgs.Add("The Activity Team has been updated", MessageType.Success);
                },
                onFailure: (SystemMessages fmsgs) => {
                    msgs.AddRange(fmsgs);
                }
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
                    loggedInUser: SessionVariables.LoggedInUser,
                    msgs: msgs);

            if (msgs.HasNoErrors())
                return RedirectToAction("registrants", new { id = activity.Uid });

            ViewBag.Messages = msgs;
            ViewBag.QuestionViewModels = qvms;

            return View(activity);
        }

        private bool IsActivityLeader(Activity activity)
        {
            return SessionVariables.AccountUserType == UserType.Administrator || SessionVariables.LoggedInUser.IsActivityLeader(activity);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Hangfire;
using AfterSchoolLibrary.Models;

namespace AfterSchool.Areas.Org.Controllers
{
    public class LeagueActivityController : BaseLoggedInController
    {
        public LeagueActivityController() : base(AccountService.ProviderAccountTypes.ToList())
        {

        }

        // GET: Admin/Activity
        public ActionResult Summary(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;

            var activity = currentAccount.GetActivity(id.ToGuid());
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
            ViewBag.Messages = msgs;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.Actions = actions;
            return View(activity);
        }

        [HttpGet]
        public ActionResult Index(string group, string pn, string rpp, string archive)
        {
            int pageNumber = 1;
            int recordsPerPage = 10;
            bool isArchived = false;
            var msgs = new SystemMessages();

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            if (archive.IsBoolean())
                isArchived = archive.ToInteger().ToBoolean();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            var activities = new List<Activity>();
            var totalActivityCount = 0;
            var hasArchivedActivities = false;

            ActivityGroup activityGroup = null;
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            if (group.IsGuid())
            {
                activityGroup = currentAccount.GetActivityGroup(group.ToGuid(), msgs);
                totalActivityCount = activityGroup.GetActivityCount(null, isArchived, false, null, msgs);
                activities = activityGroup.GetActivities(null, isArchived, false, start, length, false, msgs);
                hasArchivedActivities = activityGroup.GetActivityCount(null, true, false, null, msgs) > 0;
            }
            else
            {
                totalActivityCount = currentAccount.GetActivityCount(SessionVariables.AccountUserType, currentUser, null, isArchived, false, null, msgs);
                activities = currentAccount.GetActivities(SessionVariables.AccountUserType, currentUser, null, isArchived, false, null, start, length, msgs);
                hasArchivedActivities = currentAccount.GetActivityCount(SessionVariables.AccountUserType, currentUser, null, true, false, null, msgs) > 0;
            }

            var totalPages = Math.Ceiling(totalActivityCount.ToDouble() / recordsPerPage).ToInteger();

            var finalActivities = activities
                .OrderBy(x => x.Name)
                .ToList();

            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Messages = msgs;
            ViewBag.Groups = currentAccount.GetSessions(msgs);
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

        private void LoadEditSetupViewBag(string id, string group)
        {
            var activity = new Activity();
            var isLeader = true;
            var activePeriod = new RegistrationPeriod();
            ActivityGroup activityGroup = null;

            if (id.IsGuid())
            {
                activity = Activity.LoadByUid(id.ToGuid());
                isLeader = IsActivityLeader(activity);
                activePeriod = activity.GetActiveRegistrationPeriod();
                activityGroup = activity.GetActivityGroup();
            }
            else
            {
                activity = SessionVariables.CurrentAccount.GetDefaultNewActivity();

                //this means a new activity is not being added to an existing enrollment group
                if (!group.IsGuid())
                {
                    activePeriod = SessionVariables.CurrentAccount.GetDefaultNewRegistrationPeriod();
                }
                else
                {
                    activityGroup = ActivityGroup.LoadByUid(group.ToGuid());
                }
            }


            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.IsActivityLeader = isLeader;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditSetup";
            ViewBag.Forms = SessionVariables.CurrentAccount.GetForms().OrderBy(x => x.Name).ToList();
            ViewBag.ActiveForm = activity.GetActiveActivityForm();
            ViewBag.Periods = SessionVariables.CurrentAccount.GetRegistrationPeriods();
            ViewBag.ActivePeriod = activePeriod;
            ViewBag.Group = activityGroup;
        }

        [HttpGet]
        public ActionResult EditSetup(string id, string group)
        {
            var activity = new Activity();
            var isLeader = true;
            var activePeriod = new RegistrationPeriod();
            ActivityGroup activityGroup = null;
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;


            if (id.IsGuid())
            {
                activity = currentAccount.GetActivity(id.ToGuid(), msgs);
                isLeader = IsActivityLeader(activity);
                activePeriod = activity.GetActiveRegistrationPeriod(msgs);
                activityGroup = activity.GetActivityGroup(msgs);
            }
            else
            {
                activity = currentAccount.GetDefaultNewActivity();

                //this means a new activity is not being added to an existing enrollment group
                if (!group.IsGuid())
                    activePeriod = currentAccount.GetDefaultNewRegistrationPeriod();
                else
                    activityGroup = currentAccount.GetActivityGroup(group.ToGuid(), msgs);
            }

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.IsActivityLeader = isLeader;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditSetup";
            ViewBag.Forms = currentAccount.GetForms().OrderBy(x => x.Name).ToList();
            ViewBag.ActiveForm = activity.GetActiveActivityForm();
            ViewBag.Periods = currentAccount.GetRegistrationPeriods();
            ViewBag.ActivePeriod = activePeriod;
            ViewBag.Group = activityGroup;
            ViewBag.AccountActivityGroup = activityGroup.GetAccountActivityGroup(currentAccount);
            ViewBag.isExternalLeague = AccountActivityGroup.IsExternalLeague(currentAccount.ID.Value, activityGroup.ID.Value);

            return View(activity);
        }

        [HttpPost]
        public ActionResult EditSetup(string id, string description, string name, string seatsavailable, string waitinglistseats, string registrationperiod, string registrationopendate, string registrationopentime, string registrationclosedate, string registrationclosetime, string timezone, string form, string regmode, string group, string cmd)
        {
            var msgs = new SystemMessages();
            Activity activity = null;
            ActivityGroup activityGroup = null;
            var vb = ViewBag;
            string accountActivityGroup = null;

            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            if (group.IsGuid())
            {
                activityGroup = currentAccount.GetActivityGroup(group.ToGuid(), msgs);
                accountActivityGroup = activityGroup.GetAccountActivityGroup(currentAccount).ID.Value.ToString();
            }

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
                { "accountactivitygroup", accountActivityGroup },
                { "description", description }
            };

            if (id != "0") activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (activity.IsNotEmpty())
            {
                activity = ActivityService.EditExisting(
                    loggedInUserId: currentUser.ID.Value,
                    account: currentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs);

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

                activity = ActivityService.CreateNew(
                    loggedInUserId: currentUser.ID.Value,
                    account: currentAccount,
                    values: values,
                    msgs: msgs
                );

                if (msgs.HasNoErrors())
                {
                    // LOG: CurrentUser created Activity ActivityName for ActiivtyGroup ActivityGroupName

                    SessionVariables.WebSession.LogActivity(
                        account: currentAccount,
                        actor: currentUser,
                        actionType: WebSessionActionType.Created,
                        objectId: activity.ID.Value,
                        objectType: ObjectType.Activity,
                        objectText: activity.Name,
                        indirectObjectId: activityGroup?.ID.Value,
                        indirectObjectType: ObjectType.ActivityGroup,
                        indirectObjectText: activityGroup?.Name);
                }
            }



            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = currentAccount.GetForms().OrderBy(x => x.Name).ToList();
            ViewBag.Periods = currentAccount.GetRegistrationPeriods();
            ViewBag.Group = activityGroup;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.ActivePeriod = activity.GetActiveRegistrationPeriod() ?? currentAccount.GetDefaultNewRegistrationPeriod();
            ViewBag.ActiveForm = activity.GetActiveActivityForm();
            ViewBag.ActivityGroupType = ActivityGroupType.League;
            ViewBag.isExternalLeague = AccountActivityGroup.IsExternalLeague(currentAccount.ID.Value, activityGroup.ID.Value);

            if (msgs.HasNoErrors())
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "next":
                        ViewBag.ActiveMenu = "EditSchedule";
                        LoadEditScheduleViewBag(activity, null);
                        return View("EditSchedule", activity);
                }
            }

            ViewBag.ActiveMenu = "EditSetup";
            return View(activity);
        }

        [HttpGet]
        public ActionResult EditRestrictions(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditRestrictions";
            return View(activity);
        }

        [HttpPost]
        public ActionResult EditRestrictions(string id, string alloworganization, string agerestrictions, string agestart, string ageend, string graderestrictions, string gradestart, string gradeend, string genderrestrictions, string cmd)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

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
                    { "gradestart", graderestrictions.IsNumber() ? gradestart : "-1"},
                    { "gradeend", graderestrictions.IsNumber() ? gradeend : "-1"},
                    { "genderrestrictions", genderrestrictions},
                    { "gradeIds", gradeIds.ToStrings().ToStringList() }
                };

                activity = ActivityService.EditExisting(
                        loggedInUserId: currentUser.ID.Value,
                        account: currentAccount,
                        activityId: activity.ID.Value.ToString(),
                        values: values,
                        msgs: msgs);
            }

            if (msgs.HasNoErrors())
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        ViewBag.ActiveMenu = "EditSetup";
                        ViewBag.Forms = currentAccount.GetForms().OrderBy(x => x.Name).ToList();
                        ViewBag.IsActivityLeader = IsActivityLeader(activity);
                        ViewBag.ActiveForm = activity.GetActiveActivityForm();
                        return View("EditSetup", activity);
                    case "next":
                        ViewBag.ActiveMenu = "EditSchedule";
                        LoadEditScheduleViewBag(activity, null);
                        return View("EditSchedule", activity);
                }
            }

            ViewBag.ActiveMenu = "EditRestrictions";
            return View(activity);
        }

        [HttpGet]
        public ActionResult EditContact(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Instructors = currentAccount.GetUsers();
            ViewBag.ActiveMenu = "EditContact";

            return View(activity);
        }

        [HttpPost]
        public ActionResult EditContact(string id, string contactname, string contactphone, string contactemail, string instructor, string instructorfirstname, string instructorlastname, string instructoremail, string instructorphone, string cmd)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            var values = new Dictionary<string, object>()
            {
                { "contactname", contactname},
                { "contactphone", contactphone},
                { "contactemail", contactemail},
                { "instructor", instructor },
                { "instructorfirstname", instructorfirstname },
                { "instructorlastname", instructorlastname },
                { "instructoremail", instructoremail },
                { "instructorphone", instructorphone }
            };

            if (activity.IsNotEmpty())
                activity = ActivityService.EditExisting(
                    loggedInUserId: currentUser.ID.Value,
                    account: currentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs);

            if (msgs.HasNoErrors())
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        ViewBag.ActiveMenu = "EditSchedule";
                        LoadEditScheduleViewBag(activity, null);
                        return View("EditSchedule", activity);
                    case "next":                       
                        LoadEditStudentsViewBag(new StudentsViewBagObject(activity));
                        return View("EditStudents", activity);
                }
            }

            ViewBag.Instructors = currentAccount.GetUsers(msgs: msgs);
            ViewBag.ActiveMenu = "EditContact";
            return View(activity);
        }

        private void LoadEditStudentsViewBag(StudentsViewBagObject studentsViewBagObject)
        {
            // registrants already added to the team have a RegistrantActivity record
            var registrantActivities = studentsViewBagObject.Activity.GetRegistrantActivities();
            var registrantActivitiesRegIds = registrantActivities.Select(ra => ra.RegistrantId).ToList();
            //students not added to a team, we will re
            var availableRegistrants = new List<Registrant>();
            availableRegistrants.AddRange(studentsViewBagObject.Registrants);
            //students already added to a team
            var addedRegistrants = new List<Registrant>();

            foreach (var registrant in studentsViewBagObject.Registrants)
            {
                if (registrantActivitiesRegIds.Contains(registrant.ID))
                {
                    availableRegistrants.Remove(registrant); // remove students that are already added
                    addedRegistrants.Add(registrant);  // add students that have already been added
                }
            }

            ViewBag.Messages = studentsViewBagObject.Messages;
            ViewBag.Registrants = availableRegistrants;
            ViewBag.AddedRegistrants = addedRegistrants;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.Group = studentsViewBagObject.ActivityGroup;
            ViewBag.ActiveMenu = "EditStudents";
            ViewBag.isExternalLeague = AccountActivityGroup.IsExternalLeague(SessionVariables.CurrentAccount.ID.Value, studentsViewBagObject.ActivityGroup.ID.Value);            
        }

        [HttpGet]
        public ActionResult ManageStudents(string id)
        {
            var studentViewBagObject = new StudentsViewBagObject(id);
            LoadEditStudentsViewBag(studentViewBagObject);

            return View(studentViewBagObject.Activity);
        }

        [HttpPost]
        public ActionResult ManageStudents(string id, string cmd)
        {
            var studentViewBagObject = new StudentsViewBagObject(id);

            studentViewBagObject.Messages.AddRange(LeagueHelper.UpdateStudentsInTeam(studentViewBagObject.Activity, studentViewBagObject.Registrants, Request.Params.Filter("registrant-")));

            LoadEditStudentsViewBag(studentViewBagObject);

            return View(studentViewBagObject.Activity);
        }

        [HttpGet]
        public ActionResult EditStudents(string id)
        {
            var studentViewBagObject = new StudentsViewBagObject(id);

            LoadEditStudentsViewBag(studentViewBagObject);
            
            return View(studentViewBagObject.Activity);
        }

        [HttpPost]
        public ActionResult EditStudents(string id, string cmd)
        {
            var studentViewBagObject = new StudentsViewBagObject(id);

            studentViewBagObject.Messages.AddRange(LeagueHelper.UpdateStudentsInTeam(
                studentViewBagObject.Activity, 
                studentViewBagObject.Registrants, 
                Request.Params.Filter("registrant-")));

            LoadEditStudentsViewBag(studentViewBagObject);
            
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;

            if (studentViewBagObject.Messages.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        ViewBag.ActiveMenu = "EditContact";
                        return View("EditContact", studentViewBagObject.Activity);
                    case "next":
                        ViewBag.ActiveMenu = "EditStudents";
                        return View("EditStudents", studentViewBagObject.Activity);
                }
            }

            ViewBag.ActiveMenu = "EditStudents";
            return View(studentViewBagObject.Activity);
        }

        [HttpGet]
        public ActionResult EditDetails(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditDetails";
            return View(activity);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditDetails(string id, string details, string scheduledetails, string cmd)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (activity.IsNotEmpty())
            {
                var values = new Dictionary<string, object>()
                {
                    { "details", details},
                    { "scheduledetails", scheduledetails}
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: currentUser.ID.Value,
                    account: currentAccount,
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

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        ViewBag.ActiveMenu = "EditContact";

                        return View("EditContact", activity);
                 
                }
            }

            ViewBag.ActiveMenu = "EditDetails";
            return View(activity);
        }

        private void LoadEditScheduleViewBag(Activity activity, string scheduleId)
        {
            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.ActivityId = activity.ID.Value.ToString();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditSchedule";
            ViewBag.Schedules = SessionVariables.CurrentAccount.GetActivitySchedules();
            ViewBag.ActivityType = (int)activity.ActivityTypeId;

            ActivitySchedule schedule = null;

            // changing schedule
            if (scheduleId.IsNumber())
            {
                schedule = ActivitySchedule.Load(scheduleId.ToInteger());

                if (schedule.AccountId != SessionVariables.CurrentAccount.ID)
                    schedule = null;
            }

            if (schedule.IsEmpty())
                schedule = activity.GetSchedule();

            if (schedule.IsEmpty())
                schedule = ActivitySchedule.GetNewDefaultSchedule();

            ViewBag.Schedule = schedule;
            ViewBag.ShowScheduleList = SessionVariables.AccountPackage.HasFeature(FeatureType.ManageActivityGroups);
        }

        [HttpGet]
        public ActionResult EditSchedule(string id, string schedule)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            LoadEditScheduleViewBag(activity, schedule);

            return View(activity);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditSchedule(string id, string cmd, string schedule, string scheduledetails, string requiresalldays, string repeatsweekly, string startdate, string enddate, string dailystarttime, string dailyendtime, string isongoing)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var inSessionDates = new List<DateRange>();
            var outOfSessionDates = new List<DateTime>();

            if (activity.IsNotEmpty())
            {
                var filteredParams = Request.Params.Filter("schedulerepeat-");
                var days = ActivityScheduleUI.PrepDaysOfWeek(filteredParams);
                inSessionDates = ActivityScheduleUI.PrepScheduleDates(repeatsweekly, days, startdate, enddate, dailystarttime, dailyendtime, Request.Params.Filter("meeting-open-"));
                outOfSessionDates = ActivityScheduleUI.PrepScheduleDatesClosed(Request.Params.Filter("meeting-closed-"));

                var values = new Dictionary<string, object>()
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

            if (msgs.HasNoErrors())
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Schedule Saved", MessageType.Success);

            var instances = new List<ActivityInstance>();
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

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        ViewBag.ActiveMenu = "EditSetup";
                        ViewBag.Forms = SessionVariables.CurrentAccount.GetForms().OrderBy(x => x.Name).ToList();
                        ViewBag.IsActivityLeader = IsActivityLeader(activity);
                        ViewBag.ActiveForm = activity.GetActiveActivityForm();
                        return View("EditSetup", activity);
                    case "next":
                        //ViewBag.ActiveMenu = "EditDetails";
                        //return View("EditDetails", activity);
                        ViewBag.ActiveMenu = "EditContact";
                        ViewBag.Instructors = SessionVariables.CurrentAccount.GetUsers(msgs: msgs);
                        return View("EditContact", activity);
                }
            }

            ViewBag.ActiveMenu = "EditSchedule";
            return View(activity);
        }
        
        [HttpGet]
        public ActionResult EditLocation(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditLocation";

            return View(activity);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditLocation(string id, string location, string locationname, string locationdetails, string address1, string address2, string city, string state, string postalcode, string mylocation, string cmd)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (activity.IsNotEmpty())
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
                    { "mylocation", mylocation }
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: currentUser.ID.Value,
                    account: currentAccount,
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

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        ViewBag.ActiveMenu = "EditContact";
                        return View("EditContact", activity);
                    case "next":
                        if (SessionVariables.AccountPackage.HasFeature(FeatureType.AcceptPayments))
                        {
                            ViewBag.ActiveMenu = "EditCost";
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

        [HttpGet]
        public ActionResult EditCost(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditCost";
            return View(activity);
        }

        [HttpPost]
        public ActionResult EditCost(string id, string cost, string costdescription, string acceptpaypal, string acceptcreditcard, string allowtopaylater, string cmd)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (activity.IsNotEmpty())
            {
                var values = new Dictionary<string, object>()
                {
                    { "cost", cost},
                    { "costdescription", costdescription},
                    { "acceptpaypal", acceptpaypal.IfBlank("0")},
                    { "acceptcreditcard", acceptcreditcard.IfBlank("0")},
                    { "allowtopaylater", allowtopaylater.IfBlank("0")}
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: currentUser.ID.Value,
                    account: currentAccount,
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
                }
            }

            ViewBag.ActiveMenu = "EditCost";
            return View(activity);
        }

        [HttpGet]
        public ActionResult EditConfirmation(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditConfirmation";
            return View(activity);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditConfirmation(string id, string registrationthankyouemail, string applicationthankyouemail, string cmd)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (activity.IsNotEmpty())
            {
                var values = new Dictionary<string, object>()
                {
                    { "registrationthankyouemail", registrationthankyouemail },
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

            if (msgs.HasNoErrors())
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        if (activity.CanAcceptPayments())
                        {
                            ViewBag.ActiveMenu = "EditCost";
                            return View("EditCost", activity);
                        }
                        else
                        {
                            ViewBag.ActiveMenu = "EditLocation";
                            return View("EditLocation", activity);
                        }
                }
            }

            ViewBag.ActiveMenu = "EditConfirmation";
            return View(activity);
        }

        [HttpPost]
        public ActionResult Copy(string id, string activitynamecopy, string usesameformcopy)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs).Copy(activitynamecopy, true, currentUser.ID.Value, msgs);

            ViewBag.Messages = msgs;
            
            if (msgs.HasNoErrors())
                return RedirectToAction("editsetup", new { id = activity.Uid });

            return RedirectToAction("index", "activity");
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (activity.IsNotEmpty())
                msgs.AddRange(activity.MarkAsDeleted(currentUser.ID.Value));

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
                msgs.Add($"{activity.Name} has been deleted", MessageType.Success);

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "activity");
        }

        [HttpGet]
        public ActionResult Archive(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (activity.IsNotEmpty())
                msgs.AddRange(activity.MoveToArchive(currentUser.ID.Value));

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
                msgs.Add($"{activity.Name} has been archived", MessageType.Success);

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "activity");
        }

        [HttpGet]
        public ActionResult Restore(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (activity.IsNotEmpty())
                msgs.AddRange(activity.MoveFromArchive(currentUser.ID.Value));

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
                msgs.Add($"{activity.Name} has been restored", MessageType.Success);

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "activity");
        }

        [HttpGet]
        public ActionResult Attendance(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
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

            var attendance = activity.GetOrCreateNewAttendance(attendanceId, null, SessionVariables.CurrentUser, msgs);

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
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var presentStudents = new List<int>();
            var attendance = AfterSchoolLibrary.Models.Attendance.Load(att.ToInteger(), msgs);

            if (!activityinstance.IsNumber())
                msgs.Add("Activity Date is required", MessageType.UserError);

            attendance.GetAttendanceRegistrants(msgs).ForEach(attendanceRegistrant => {
                if (Request.Params[$"present-{attendanceRegistrant.ID.ToString()}"].IsNumber())
                    presentStudents.Add(attendanceRegistrant.ID.Value);
            });

            if (msgs.HasNoErrors())
                attendance.UpdateAttendance(activityinstance.ToInteger(), presentStudents, currentUser, msgs);


            ViewBag.Messages = msgs;
            ViewBag.AttendanceRegistrants = attendance.GetAttendanceRegistrants();
            ViewBag.Schedule = activity.GetSchedule();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            return View(attendance);
        }

        [HttpGet]
        public ActionResult Registrants(string id, string atype)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            List<RegistrantActivity> ras = null;
            var rat = RegistrationAssignmentType.NotSet;

            if (atype.IsNotEmpty())
            {
                rat = (RegistrationAssignmentType)atype.ToInteger();
                if(rat == RegistrationAssignmentType.Invited)
                    ras = activity.GetRegistrantActivities(true);
            }

            if (ras.IsEmpty())
                ras = activity.GetRegistrantActivities();

            if(rat != RegistrationAssignmentType.NotSet)
                ras = ras.Where(x => x.AssignmentTypeId == rat).OrderBy(x => x.SeatIndex).ToList();

            ViewBag.Messages = msgs;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.Registrants = ras;
            ViewBag.AssignmentType = rat;
            return View(activity);
        }

        [HttpGet]
        public ActionResult RegistrantSummary(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
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
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
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

        [HttpGet]
        public ActionResult RegistrantDetail(string id)
        {
            var ra = RegistrantActivity.LoadByUid(id.ToGuid());
            var activity = Activity.Load(ra.ParticipationObjectId.Value);

            ViewBag.Messages = new SystemMessages();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            return View(ra);
        }

        [HttpPost]
        public ActionResult RegistrantDetail(string id, string datepaid, string amountpaid, string paymenttype)
        {
            var msgs = new SystemMessages();
            var ra = RegistrantActivity.LoadByUid(id.ToGuid());
            var activity = ra.GetActivity();

            if (!datepaid.IsDateTime())
                msgs.Add("Date Paid must be a valid date", MessageType.UserError);

            if (!amountpaid.IsNumber() || amountpaid.ToDecimal() < 0)
                msgs.Add("Amount Paid must be a number greater than or equal to 0", MessageType.UserError);

            if (!paymenttype.IsNumber())
                msgs.Add("Please select a valid payment type", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var payment = new ActivityPayment()
                {
                    ParticipationObjectId = ra.ParticipationObjectId,
                    ParticipationObjectTypeId = ra.ParticipationObjectTypeId,
                    Amount = amountpaid.ToDecimal(),
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = SessionVariables.CurrentUser.ID,
                    LastModifiedBy = SessionVariables.CurrentUser.ID,
                    LastModifiedDate = DateTime.UtcNow,
                    RegistrantId = ra.RegistrantId,
                    AccountId = SessionVariables.CurrentAccount.ID
                };

                msgs.AddRange(payment.Save());

                if (msgs.HasNoErrors())
                    msgs.Add("Payment details have been updated", MessageType.Success);
            }

            ViewBag.Messages = msgs;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            return View(ra);
        }

        [HttpGet]
        public ActionResult PublishDetail(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = currentAccount;
            return View(activity);
        }

        [HttpGet]
        public ActionResult Publish(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            msgs.AddRange(activity.SetIsPublished(true, currentUser.ID.Value));            

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been published", activity.Name), MessageType.Success);

                // LOG: CurrentWebUser Published Activity ActivityName to Account AccountName's Profile

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Published,
                    objectId: activity.ID.Value,
                    objectType: ObjectType.Activity,
                    objectText: activity.Name,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectText: $"{currentAccount.Name}'s Profile");
            }

            ViewBag.Messages = msgs;
            return View("publishdetail", activity);
        }

        [HttpGet]
        public ActionResult Unpublish(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            msgs.AddRange(activity.SetIsPublished(false, currentUser.ID.Value));            

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been unpublished", activity.Name), MessageType.Success);

                // LOG: CurrentWebUser Unpublished Activity ActivityName from Account AccountName's Profile
                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Unpublished,
                    objectId: activity.ID.Value,
                    objectType: ObjectType.Activity,
                    objectText: activity.Name,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectText: $"{currentAccount.Name}'s Profile");
            }

            ViewBag.Messages = msgs;
            return View("publishdetail", activity);
        }

        [HttpGet]
        public ActionResult Form(string id, string qg)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
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

            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
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
                            questionGroup.LastModifiedBy = currentUser.ID;
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
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
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
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Messages = new SystemMessages();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.Registrants = activity.GetRegistrants()
                .Where(x => x.Email.IsNotBlank()) // why?  && !x.FirstRegistrantId.HasValue
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToList();
            return View(activity);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult MessageAdd(string id, string subject, string body, string sendtostaff)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (subject.IsEmpty())
                msgs.Add("Subject is required", MessageType.UserError);
            if (body.IsEmpty())
                msgs.Add("Body is required", MessageType.UserError);

            ActivityMessage am = null;

            if(msgs.HasNoErrors())
            {
                var registrantIds = activity.GetRegistrants()
                    .Where(x => Request[$"registrant-{x.ID.ToString()}"].IsNotBlank())
                    .Select(x => x.ID.Value)
                    .ToList();
                am = activity.AddMessage(subject, body, registrantIds, sendtostaff.IsBoolean(), currentUser.ID.Value, msgs);

                BackgroundJob.Enqueue(() => ActivityMessageService.Send(am.ID.Value, currentUser.ID.Value, null, null));
            }                

            ViewBag.Messages = msgs;
            ViewBag.IsActivityLeader = IsActivityLeader(activity);

            if (msgs.HasNoErrors())
            {
                msgs.Add("Your message will been sent to the selected registrants", MessageType.Success);
                ViewBag.Registrants = am.GetActivityMessageRegistrants();
                ViewBag.Messages = msgs;
                return View("messagedetail", am);
            }

            ViewBag.Messages = msgs;
            ViewBag.Registrants = activity.GetRegistrants();
            return View(activity);            
        }

        [HttpGet]
        public ActionResult Team(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            LoadTeamViewBag(activity);
            return View(activity);
        }

        private void LoadTeamViewBag(Activity activity)
        {
            var account = activity.GetAccount();

            ViewBag.Messages = new SystemMessages();
            ViewBag.Users = account.GetActiveMembers().Select(x => x.ToWebUser()).ToList();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
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

                    var currentAccount = SessionVariables.CurrentAccount;
                    var currentUser = SessionVariables.CurrentUser;
                    activity = currentAccount.GetActivity(id.ToGuid(), msgs);

                    account = activity.GetAccount(amsgs, adc);
                    users = account.GetActiveMembers(msgs: amsgs, dc: adc).Select(x => x.ToWebUser()).ToList();

                    foreach (var user in users)
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
                                {
                                    awu.IsLeader = true;
                                }
                                else
                                {
                                    awu.IsLeader = false;
                                }

                                awu.LastModifiedDate = DateTime.UtcNow;
                                awu.LastModifiedBy = currentUser.ID;
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
            LoadTeamViewBag(activity);
            return View("Team", activity);
        }


        private bool IsActivityLeader(Activity activity)
        {
            return SessionVariables.AccountUserType == UserType.Administrator || SessionVariables.CurrentUser.IsActivityLeader(activity);            
        }

        [HttpGet]
        public ActionResult Schedule(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var team = currentAccount.GetActivity(id.ToGuid(), msgs);
            var activityInstances = MatchService.GetTeamActivityInstances(team.ID.Value, msgs);

            ViewBag.Messages = msgs;
            ViewBag.ActivityInstances = activityInstances;

            return View(team);
        }

    }
}
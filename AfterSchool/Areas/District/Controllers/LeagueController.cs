using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchool.Models;
using BasicLibrary;
using AfterSchoolLibrary;
using System.Collections.Specialized;

namespace AfterSchool.Areas.District.Controllers
{   
    public class LeagueController : BaseLoggedInController
    {
        public LeagueController() : base(new List<AccountType> { AccountType.SchoolDistrict })
        {

        }

        // GET: District/League
        [HttpGet]
        public ActionResult Index(string pn, string rpp, string archive)
        {
            var groups = new List<ActivityGroup>();
            int pageNumber = 1;
            int recordsPerPage = 10;
            bool isArchived = false;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            if (archive.IsBoolean())
                isArchived = archive.ToInteger().ToBoolean();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;
            var totalCount = 0;
            var hasArchivedGroups = false;

            totalCount = SessionVariables.CurrentAccount.GetLeagueCount(isArchived);
            groups = SessionVariables.CurrentAccount.GetLeagues(isArchived, start, length).OrderByDescending(x => x.CreateDate).ToList();

            hasArchivedGroups = SessionVariables.CurrentAccount.GetLeagueCount(true) > 0;

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.GroupController = "league";
            ViewBag.Messages = new SystemMessages();
            ViewBag.HasArchivedGroups = hasArchivedGroups;
            ViewBag.ActivityGroupType = ActivityGroupType.League;
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };

            return View(groups);
        }
        [ValidateInput(false)]
        [HttpGet]
        public ActionResult Summary(string id)
        {
            var group = ActivityGroup.LoadByUid(id.ToGuid());
            var registrations = group.GetRegistrations();
            var actions = SessionAction.LoadByActivityGroupId(
                group.ID.Value,
                new List<SessionActionType>() {
                    SessionActionType.ActivityGroupCreate,
                    SessionActionType.ActivityGroupPublish,
                    SessionActionType.ActivityGroupUnpublish,
                },
                0,
                15);
            ViewBag.Messages = new SystemMessages();
            ViewBag.Actions = actions;
            ViewBag.Registrations = registrations;
            return View(group);
        }

        [ValidateInput(false)]
        // GET: District/School
        [HttpGet]
        public ActionResult Schools(string pn, string rpp)
        {
            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            int count = SessionVariables.CurrentAccount.GetVisitorAccountsCount();
            var visitorAccounts = SessionVariables.CurrentAccount.GetVisitorAccounts(start, length);
            var totalPages = Math.Ceiling(count.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Messages = new SystemMessages();
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = count,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };
            return View(visitorAccounts);
        }

        [HttpGet]
        public ActionResult EditSetup(string id, string group)
        {
            var activity = new Activity();
            var isLeader = true;
            var activePeriod = new RegistrationPeriod();
            ActivityGroup activityGroup = null;

            if (id.IsNumber() && id.ToInteger() > 0)
            {
                activity = Activity.Load(id.ToInteger());
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

            return View(activity);
        }

        [HttpPost]
        public ActionResult EditSetup(string id, string name, string seatsavailable, string waitinglistseats, string registrationperiod, string registrationopendate, string registrationopentime, string registrationclosedate, string registrationclosetime, string timezone, string form, string regmode, string group, string cmd)
        {
            var msgs = new SystemMessages();
            Activity activity = null;
            ActivityGroup activityGroup = null;

            var values = new NameValueCollection()
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

            if (id.IsNumber() && id.ToInteger() > 0)
            {
                activity = Activity.EditExisting(
                    loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
                    account: SessionVariables.CurrentAccount,
                    activityId: id,
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
                    var newForm = defaultForm.Copy(SessionVariables.LoggedInUser.ID.Value, SessionVariables.CurrentAccount.ID.Value, msgs);
                    if (msgs.HasNoErrors())
                    {
                        newForm.Name = (name + " form").MaxChars(100, "...");
                        msgs.AddRange(newForm.Save());
                        values.Add("form", newForm.ID.Value.ToString());
                        //activity.AddFormAndActivitate(newForm, SessionVariables.LoggedInUser.ID.Value, false, msgs);
                    }
                }

                activity = Activity.CreateNew(
                    loggedInUserId: SessionVariables.LoggedInUser.ID.Value,
                    account: SessionVariables.CurrentAccount,
                    values: values,
                    msgs: msgs
                );

                if (msgs.HasNoErrors())
                {
                    UpdateSession(SessionActionType.ActivityCreate, SessionVariables.CurrentAccount.ID, activity.ID);
                }
            }

            if (group.IsGuid())
            {
                activityGroup = ActivityGroup.LoadByUid(group.ToGuid());
            }

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = SessionVariables.CurrentAccount.GetForms().OrderBy(x => x.Name).ToList();
            ViewBag.Periods = SessionVariables.CurrentAccount.GetRegistrationPeriods();
            ViewBag.Group = activityGroup;
            ViewBag.ActivePeriod = activity.GetActiveRegistrationPeriod() ?? SessionVariables.CurrentAccount.GetDefaultNewRegistrationPeriod();
            ViewBag.ActiveForm = activity.GetActiveActivityForm();

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

        // GET: User
        [HttpGet]
        public ActionResult Teams(string pn, string rpp)
        {
            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            int userCount = SessionVariables.CurrentAccount.GetUserCount();
            var teams = SessionVariables.CurrentAccount.GetActivities(UserType.ActivityLeader, WebUser.Load(SessionVariables.CurrentAccount.OwnerId.Value), null, null, null, null,null);
            var totalPages = Math.Ceiling(userCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Messages = new SystemMessages();
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = userCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };

            return View(teams);
        }

        [ValidateInput(false)]
        [HttpGet]
        public ActionResult Edit(string id)
        {
            var league = new ActivityGroup()
            {
                ActivityGroupTypeId = ActivityGroupType.League,
                RegistrationModeId = RegistrationMode.Register,
            };

            if (id.IsGuid())
                league = ActivityGroup.LoadByUid(id.ToGuid());

            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = SessionVariables.CurrentAccount.GetForms();
            ViewBag.Grades = GradeLevel.Load();
            LoadEditScheduleViewBag(league, league.ActivityScheduleId.ToString());

            return View(league);
        }

        private void LoadEditScheduleViewBag(ActivityGroup activityGroup, string scheduleId)
        {
            /*
            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.ActivityId = activity.ID.Value.ToString();
            ViewBag.IsActivityLeader = IsActivityLeader(activity);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditSchedule";
            */

            ViewBag.Schedules = SessionVariables.CurrentAccount.GetActivitySchedules();

            ActivitySchedule schedule = null;

            // changing schedule
            if (scheduleId.IsNumber())
            {
                schedule = ActivitySchedule.Load(scheduleId.ToInteger());
                if (schedule.AccountId != SessionVariables.CurrentAccount.ID)
                {
                    schedule = null;
                }
            }
            if (schedule.IsEmpty())
            {
                schedule = activityGroup.GetSchedule();
            }
            if (schedule.IsEmpty())
            {
                schedule = ActivitySchedule.GetNewDefaultSchedule();
            }

            ViewBag.Schedule = schedule;
            ViewBag.ShowScheduleList = SessionVariables.AccountPackage.HasFeature(FeatureType.ManageActivityGroups);

        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Edit(string id, string name, string description, string regmode, string form, string isinviteonly, string genderrestrictions, string mingrade, string maxgrade, string schedule, string scheduledetails, string requiresalldays, string repeatsweekly, string startdate, string enddate, string dailystarttime, string dailyendtime)
        {
            var msgs = new SystemMessages();
            var scheduleDates = new List<string>();

            var values = new NameValueCollection()
            {
                { "name", name },
                { "description", description },
                { "regmode", ((int)RegistrationMode.Register).ToString() },
                { "form", form },
                { "isinviteonly", 0.ToString() },
                { "genderrestrictions", genderrestrictions },
                { "mingrade", mingrade },
                { "maxgrade", maxgrade }
            };

            if (SessionVariables.CurrentAccount.HasFeature(FeatureType.ManageActivityGroups) && schedule.IsNumber() && schedule.ToInteger() > 0)
            {
                values.Add("schedule", schedule);
            }
            else
            {
                var filteredParams = Request.Params.Filter("schedulerepeat-");
                var days = Models.ActivityScheduleUI.PrepDaysOfWeek(filteredParams);
                scheduleDates = Models.ActivityScheduleUI.PrepScheduleDates(repeatsweekly, days, startdate, enddate, dailystarttime, dailyendtime, Request.Params.Filter("meeting-"));

                values = new NameValueCollection();
                values.Add("scheduledetails", scheduledetails);
                values.Add("days", days.Select(x => (int)x).ToArray().ToStringArray().ToStringList());
                values.Add("repeatsweekly", repeatsweekly.IfBlank("0"));
                values.Add("startdate", startdate);
                values.Add("enddate", enddate);
                values.Add("dailystarttime", dailystarttime);
                values.Add("dailyendtime", dailyendtime);
                values.Add("scheduledates", scheduleDates.ToStringList());
                values.Add("schedule", schedule);
            }
                var league = ActivityGroup.SaveLeague(SessionVariables.LoggedInUser, SessionVariables.CurrentAccount, id, values, msgs);

            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityGroupLabel(league.ActivityGroupTypeId)} Saved", MessageType.Success);
                UpdateSession(sat: SessionActionType.ActivityGroupCreate, accountId: SessionVariables.CurrentAccount.ID, activityGroupId: league.ID);
            }

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = SessionVariables.CurrentAccount.GetForms();
            ViewBag.Grades = GradeLevel.Load();
            LoadEditScheduleViewBag(league, league.ActivityScheduleId.ToString());


            return View(league);
        }
    }
}
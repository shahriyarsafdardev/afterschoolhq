using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchool.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;
using System.Collections.Specialized;
using AfterSchoolLibrary.Services;
using Hangfire;

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

        public void PeriodViewBag(ActivityGroup group, SystemMessages msgs)
        {
            var defaultOpenDateTime = string.Format("{0} {1}", SessionVariables.CurrentAccount.ConvertToAccountTimeZone(DateTime.UtcNow).ToShortDateString(), "8:00 AM").ToDateTime();
            var defaultCloseDateTime = string.Format("{0} {1}", SessionVariables.CurrentAccount.ConvertToAccountTimeZone(DateTime.UtcNow).AddWeeks(1).ToShortDateString(), "12:00 AM").ToDateTime();

            ViewBag.Periods = RegistrationPeriod.LoadByActivityGroupId(group.ID.Value).OrderBy(x => x.StartDateTime.Value).ToList();
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.DefaultOpenDateTime = defaultOpenDateTime;
            ViewBag.DefaultCloseDateTime = defaultCloseDateTime;
            ViewBag.Messages = msgs;
        }

        [HttpGet]
        public ActionResult Periods(string id)
        {
            var account = SessionVariables.CurrentAccount;
            var group = ActivityGroup.LoadByUid(id.ToGuid());
            PeriodViewBag(group, new SystemMessages());
            return View(group);
        }

        [HttpPost]
        public ActionResult PeriodEdit(string id, string period, string opendate, string opentime, string closedate, string closetime)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var group = ActivityGroup.LoadByUid(id.ToGuid());
            var regperiod = new RegistrationPeriod();

            if (period.IsInteger())
            {
                regperiod = RegistrationPeriod.Load(period.ToInteger());
            }
            else
            {
                regperiod.CreatedBy = SessionVariables.LoggedInUser.ID.Value;
                regperiod.CreateDate = DateTime.UtcNow;
                regperiod.ActivityGroupId = group.ID;
                regperiod.AccountId = SessionVariables.CurrentAccount.ID;
            }

            regperiod.StartDateTime = string.Format("{0} {1}", opendate, opentime).ToDateTime();
            regperiod.EndDateTime = string.Format("{0} {1}", closedate, closetime).ToDateTime();
            regperiod.LastModifiedBy = SessionVariables.LoggedInUser.ID.Value;
            regperiod.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(regperiod.Save());

            PeriodViewBag(group, msgs);

            return View("periods", group);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult PeriodNotify(string id, string period, string emailmessage)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var group = ActivityGroup.LoadByUid(id.ToGuid());
            var regperiod = RegistrationPeriod.Load(period.ToInteger());
            var registrations = group.GetRegistrations(null, null, null);

            BackgroundJob.Enqueue(() =>
                RegistrationPeriod.NotifyRegistrants(
                    regperiod.ID.Value,
                    emailmessage,
                    registrations.Select(x => x.ID.Value).ToList(),
                    null)
            );

            msgs.Add("Your emails will be sent shortly", MessageType.Success);

            PeriodViewBag(group, msgs);

            return View("periods", group);
        }

        private ActivityGroup LoadSchoolsViewBag(string id, string pn, string rpp)
        {
            var msgs = new SystemMessages() { };
            var group = ActivityGroup.LoadByUid(id.ToGuid());

            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            int count = AccountActivityGroup.GetCount(
                new System.Collections.Hashtable()
                {
                    { "activitygroupid", group.ID.Value}
                },
                msgs);

            var invitedAccountActivityGroups = AccountActivityGroup.Load(
                new System.Collections.Hashtable()
                {
                    { "activitygroupid", group.ID.Value}
                },
                start,
                length,
                msgs);

            var totalPages = Math.Ceiling(count.ToDouble() / recordsPerPage).ToInteger();
            
            ViewBag.Messages = new SystemMessages();
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = count,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };

            ViewBag.InvitedAccountActivityGroups = invitedAccountActivityGroups;
            return group;
        }

        [ValidateInput(false)]
        // GET: District/School
        [HttpGet]
        public ActionResult Schools(string id,string pn, string rpp)
        {
            var group = LoadSchoolsViewBag(id, pn, rpp);
            return View(group);
        }

        [ValidateInput(false)]
        // GET: District/School
        [HttpGet]
        public ActionResult Schedule(string id, string pn, string rpp)
        {
            var group = ActivityGroup.LoadByUid(id.ToGuid());
            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            int count = SessionVariables.CurrentAccount.GetVisitorAccountsCount(AccountType.School);
            var visitorAccounts = SessionVariables.CurrentAccount.GetVisitorAccounts(AccountType.School, start, length);
            var totalPages = Math.Ceiling(count.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Messages = new SystemMessages();
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = count,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };
            return View(group);
        }

        // GET: User
        [HttpGet]
        public ActionResult Teams(string id, string pn, string rpp)
        {
            var group = ActivityGroup.LoadByUid(id.ToGuid());
            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            int userCount = SessionVariables.CurrentAccount.GetUserCount();
            var activityGroup = ActivityGroup.LoadByUid(id.ToGuid());
            var teams = activityGroup.GetLeagueTeams(SessionVariables.CurrentAccount, AccountType.School);
           // var teams = activityGroup.GetActivities();
          //  var teams = SessionVariables.CurrentAccount.GetActivities(UserType.ActivityLeader, WebUser.Load(SessionVariables.CurrentAccount.OwnerId.Value), null, null, null, null,null);
            var totalPages = Math.Ceiling(userCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Messages = new SystemMessages();
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = userCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };
            ViewBag.Teams = teams.Where(t => t.DateSubmittedToActivityGroup.IsNotEmpty()).ToList();
            ViewBag.Activities = group.GetActivities();
            return View(group);
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
        public ActionResult Edit(string id, string name, string description, string regmode, string form, string isinviteonly, string genderrestrictions, string mingrade, string maxgrade, string schedule, string scheduledetails, string requiresalldays, string repeatsweekly, string startdate, string enddate, string dailystarttime, string dailyendtime, string isongoing)
        {
            var msgs = new SystemMessages();
            var inSessionDates = new List<DateRange>();
            var outOfSessionDates = new List<DateTime>();

            var values = new Dictionary<string, object>()
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

            var filteredParams = Request.Params.Filter("schedulerepeat-");
            var days = Models.ActivityScheduleUI.PrepDaysOfWeek(filteredParams);
            inSessionDates = ActivityScheduleUI.PrepScheduleDates(repeatsweekly, days, startdate, enddate, dailystarttime, dailyendtime, Request.Params.Filter("meeting-open-"));
            outOfSessionDates = ActivityScheduleUI.PrepScheduleDatesClosed(Request.Params.Filter("meeting-closed-"));

            values.Add("scheduledetails", scheduledetails);
            values.Add("days", days.Select(x => (int)x).ToArray().ToStringArray().ToStringList());
            values.Add("repeatsweekly", repeatsweekly.IfBlank("0"));
            values.Add("startdate", startdate);
            values.Add("enddate", enddate);
            values.Add("dailystarttime", dailystarttime);
            values.Add("dailyendtime", dailyendtime);
            values.Add("scheduledates", inSessionDates);
            values.Add("scheduledatesclosed", outOfSessionDates);
            values.Add("schedule", schedule);
            values.Add("isongoing", isongoing);

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

        private List<Account> LoadInviteSchoolViewBag(ActivityGroup activityGroup, SystemMessages msgs)
        {
            var schools = SessionVariables.CurrentAccount.GetVisitorAccounts(AccountType.School, msgs);
            var accountActivityGroups = AccountActivityGroup.LoadByActivityGroupId(activityGroup.ID.Value, msgs);
            var accountIds = accountActivityGroups.Select(a => a.AccountId.Value);

            List<Account> notInivitedSchools = schools.Where(s => !accountIds.Contains(s.ID.Value)).ToList();

            var invitedSchools = new List<Account>();

            ViewBag.CurrentSchools = invitedSchools;
            ViewBag.SchoolsPerColumn = (schools.Count / 3) > (schools.Count % 3) ? (schools.Count / 3) : (schools.Count % 3);

            ViewBag.SchoolsToInvite = notInivitedSchools;
            ViewBag.RegistrationWindow = activityGroup.GetCurrentOrNextRegistrationPeriod(msgs);

            return notInivitedSchools;
        }

        [HttpGet]
        public ActionResult InviteSchool(string id)
        {
            var msgs = new SystemMessages();
            var activityGroup = ActivityGroup.LoadByUid(id.ToGuid());

            LoadInviteSchoolViewBag(activityGroup, msgs);

            ViewBag.Messages = msgs;

            return View(activityGroup);
        }

        [HttpPost]
        public ActionResult InviteSchool(string id, string invitemessage, string sendinvite)
        {
            var msgs = new SystemMessages();
            var activityGroup = ActivityGroup.LoadByUid(id.ToGuid());
            var schoolsToInvite = LoadInviteSchoolViewBag(activityGroup, msgs);
            var invitedSchools = new List<Account>();

            schoolsToInvite.ForEach(school => {
                if (Request[String.Format("school-{0}", school.ID.Value)].IsBoolean())
                    invitedSchools.Add(school);
            });

            AccountService.InviteSchoolsToLeague(invitedSchools, activityGroup, invitemessage, SessionVariables.LoggedInUser, msgs);            

            if (msgs.HasNoErrors() && sendinvite.IsBoolean())
            {
                var activityGroupId = activityGroup.ID.Value;
                var senderUserId = SessionVariables.LoggedInUser.ID.Value;

                invitedSchools.ForEach(school => {
                    var schoolId = school.ID.Value;

                    BackgroundJob.Enqueue(() =>
                        AccountService.SendLeagueInvite(schoolId, activityGroupId, senderUserId, invitemessage)
                    );
                });

                msgs.Add("Your invites will be sent shortly", MessageType.Success);
            }


            if (msgs.HasNoErrors())
                return RedirectToAction("schools", new { id = activityGroup.Uid });

            ViewBag.Messages = msgs;

            return View(activityGroup);
        }

    }
}
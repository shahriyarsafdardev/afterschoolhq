using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchool.Models;
using BasicLibrary;
using AfterSchoolLibrary;
using System.Collections.Specialized;
using Hangfire;

namespace AfterSchool.Areas.Org.Controllers
{
    public class LeagueController : BaseLoggedInController
    {
        public LeagueController() : base(new List<AccountType> { AccountType.School })
        {

        }

        private List<ActivityGroup> LoadIndexViewBag(string pn = "1", string rpp = "10" , string archive = "0")
        {
            var groups = new List<ActivityGroup>();  //internal Leagues
            var extAccountActivityGroups = new List<AccountActivityGroup>();
            var acceptedInvites = new List<ActivityGroup>();
            var pendingInvites = new List<ActivityGroup>();
            var internalGroups = new List<ActivityGroup>();
            var externalGroups = new List<ActivityGroup>();
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
            extAccountActivityGroups = AccountActivityGroup.LoadByAccountId(SessionVariables.CurrentAccount.ID.Value);

            acceptedInvites = extAccountActivityGroups.Where(aag => aag.InviteStatusId == InviteStatus.Accepted).Select(aag => aag.GetActivityGroup()).ToList();
            pendingInvites = extAccountActivityGroups.Where(aag => aag.InviteStatusId == InviteStatus.Invited).Select(aag => aag.GetActivityGroup()).ToList();

            hasArchivedGroups = SessionVariables.CurrentAccount.GetLeagueCount(true) > 0;

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();


            ViewBag.GroupController = "League";
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

            
            ViewBag.pendingInvites = pendingInvites;
            ViewBag.acceptedInvites = acceptedInvites;
            ViewBag.ExternalGroups = externalGroups;
            ViewBag.InternalGroups = internalGroups;
            
            return groups;
        }

        // GET: District/League
        [HttpGet]
        public ActionResult Index(string pn, string rpp, string archive)
        {
            var groups = LoadIndexViewBag(pn, rpp, archive);
            ViewBag.isExternalGroupsView = false;
            return View(groups);
        }

        [HttpGet]
        public ActionResult IndexExternal(string pn, string rpp, string archive)
        {
            var groups = LoadIndexViewBag(pn, rpp, archive);
            ViewBag.isExternalGroupsView = true;
            return View(groups);
        }

        [HttpGet]
        public ActionResult Summary(string id)
        {
            var group = ActivityGroup.LoadByUid(id.ToGuid());
            var accountActivityGroup = AccountActivityGroup.LoadByAccountIdByActivityGroupId(group.AccountId.Value, group.ID.Value);
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

        /// <summary>
        /// Controller for the " Reg Windows" view
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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


        /// <summary>
        ///  Controller for the "Students" registrations tab 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pn"></param>
        /// <param name="rpp"></param>
        /// <param name="emailed"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Registrations(string id, string pn, string rpp, string emailed)
        {
            var group = ActivityGroup.LoadByUid(id.ToGuid());
            var registrations = new List<Registration>();
            var msgs = new SystemMessages();

            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
            {
                pageNumber = pn.ToInteger();
            }
            if (rpp.IsNumber() && rpp.ToInteger() > 0)
            {
                recordsPerPage = rpp.ToInteger();
            }

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;
            var totalCount = 0;

            registrations = group.GetRegistrations(start, length);
            totalCount = group.GetRegistrationCount();

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            if (emailed.IsBoolean() && emailed.ToBoolean())
            {
                msgs.Add("An email will be sent shortly", MessageType.Success);
            }

            ViewBag.Messages = msgs;
            LoadRegistrationsViewBag(group, registrations);

            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };

            return View(group);
        }

        private void LoadRegistrationsViewBag(ActivityGroup group, List<Registration> registrations)
        {
            ViewBag.Registrations = registrations;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.Registrations = registrations;
            ViewBag.RegistrantActivities = group.GetRegistrantActivities(registrations);
            ViewBag.ReservedSeats = group.GetSeatCountByAssignmentType(RegistrationAssignmentType.Reserved);
            ViewBag.WaitingListSeats = group.GetSeatCountByAssignmentType(RegistrationAssignmentType.WaitingList);
            ViewBag.ApplicationSeats = group.GetSeatCountByAssignmentType(RegistrationAssignmentType.Application);
            ViewBag.ReturnUrl = Url.Action("registrations", "league", new { id = group.Uid });
        }

        // Teams in a league
        [HttpGet]
        public ActionResult LeagueActivities(string id, string pn, string rpp, string archive)
        {
            var group = ActivityGroup.LoadByUid(id.ToGuid());
            LoadLeagueActivityViewBag(group, id, pn, rpp, archive);
            return View(group);
        }

        private void LoadLeagueActivityViewBag(ActivityGroup group, string id, string pn, string rpp, string archive)
        {
            int pageNumber = 1;
            int recordsPerPage = 10;

            var isArchived = group.IsArchived;

            if (pn.IsNumber() && pn.ToInteger() > 0)
            {
                pageNumber = pn.ToInteger();
            }
            if (rpp.IsNumber() && rpp.ToInteger() > 0)
            {
                recordsPerPage = rpp.ToInteger();
            }
            if (archive.IsBoolean())
            {
                isArchived = archive.ToInteger().ToBoolean();
            }

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;
            var account = SessionVariables.CurrentAccount;
            var activities = group.GetActivities(isArchived, false, start, length);
            var totalCount = group.GetActivityCount(isArchived, false);
            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();
            var finalActivities = activities.OrderBy(x => x.Name).ToList();

            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Activities = finalActivities;
            ViewBag.NeedToUpgrade = finalActivities.Count >= SessionVariables.AccountPackage.GetFeature(FeatureType.Activities).GetMaxValue();
            ViewBag.Messages = new SystemMessages();
            ViewBag.HasArchivedActivities = group.GetActivityCount(true, false) > 0;

            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };
        }

        [HttpGet]
        public ActionResult Schedules(string id)
        {

            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUid(id.ToGuid());

            ViewBag.Messages = msgs;
            ViewBag.Schedules = group.GetSchedules();
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.Activities = group.GetActivities();

            return View(group);
        }

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

            return View(league);
        }

        [HttpPost]
        public ActionResult Edit(string id, string name, string description, string regmode, string form, string isinviteonly, string genderrestrictions, string mingrade, string maxgrade)
        {
            var msgs = new SystemMessages();

            var values = new Dictionary<string, object>()
            {
                { "name", name },
                { "description", description },
                { "regmode", regmode },
                { "form", form },
                { "isinviteonly", isinviteonly },
                { "genderrestrictions", genderrestrictions },
                { "mingrade", mingrade },
                { "maxgrade", maxgrade }
            };

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

            return View(league);
        }

        [HttpGet]
        public ActionResult SubmitToDistrict(string id, string pn, string rpp, string archive)
        {
            var msgs = new SystemMessages();

            var activity = Activity.LoadByUid(id.ToGuid());
            var timeSubmitted = DateTime.UtcNow;


            activity.DateSubmittedToActivityGroup = DateTime.UtcNow;
            activity.LastModifiedDate = DateTime.UtcNow;
            activity.LastModifiedBy = SessionVariables.LoggedInUser.ID.Value;
            msgs.AddRange(activity.Save());

            var group = activity.GetActivityGroup();
            LoadLeagueActivityViewBag(group, id, pn, rpp, archive);
            return View("LeagueActivities", group);
        }

        [HttpGet]
        public ActionResult UpdateInvitation(string id, InviteStatus inviteStatus)
        {
            var msgs = new SystemMessages();

            var accountActivityGroup = AccountActivityGroup.LoadByAccountIdByActivityGroupId(SessionVariables.CurrentAccount.ID.Value, id.ToInteger());
            msgs.AddRange(accountActivityGroup.UpdateInvitationStatus(inviteStatus, SessionVariables.CurrentAccount.ID.Value));

            var groups = LoadIndexViewBag();

            return View("Index",groups);
        }

        [HttpGet]
        public ActionResult PublishDetail(string id)
        {
            var activityGroup = ActivityGroup.LoadByUid(id.ToGuid());
            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = SessionVariables.CurrentAccount;
            return View(activityGroup);
        }

        [HttpGet]
        public ActionResult Publish(string id)
        {
            SystemMessages msgs = new SystemMessages();
            var activityGroup = ActivityGroup.LoadByUid(id.ToGuid());

            msgs.AddRange(activityGroup.SetIsPublished(true, SessionVariables.LoggedInUser.ID.Value));

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been published", activityGroup.Name), MessageType.Success);
                UpdateSession(SessionActionType.ActivityGroupPublish, SessionVariables.CurrentAccount.ID, activityGroup.ID);
            }

            ViewBag.Messages = msgs;
            return View("publishdetail", activityGroup);
        }

        [HttpGet]
        public ActionResult Unpublish(string id)
        {
            SystemMessages msgs = new SystemMessages();
            var activityGroup = ActivityGroup.LoadByUid(id.ToGuid());

            msgs.AddRange(activityGroup.SetIsPublished(false, SessionVariables.LoggedInUser.ID.Value));

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been unpublished", activityGroup.Name), MessageType.Success);
                UpdateSession(SessionActionType.ActivityGroupUnpublish, SessionVariables.CurrentAccount.ID, activityGroup.ID);
            }

            ViewBag.Messages = msgs;
            return View("publishdetail", activityGroup);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchool.Models;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using System.Collections.Specialized;
using Hangfire;
using BasicLibrary.DataAccess;

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
            ViewBag.ExternalGroups = acceptedInvites;
            ViewBag.InternalGroups = internalGroups;
            
            return groups;
        }

        // GET: District/League
        [HttpGet]
        public ActionResult Index(string pn, string rpp, string archive)
        {
            // var groups = LoadIndexViewBag(pn, rpp, archive);
            // ViewBag.isExternalGroupsView = false;
            return RedirectToAction("IndexExternal");
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
            var aag = group.GetAccountActivityGroup(SessionVariables.CurrentAccount, msgs);

            if (aag.IsNotEmpty())
            {
                ViewBag.AccountActivityGroup = aag;
                ViewBag.Periods = RegistrationPeriod.LoadByAccountActivityGroupId(aag.ID.Value).OrderBy(x => x.StartDateTime.Value).ToList();
            }
            else
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
            var aag = group.GetAccountActivityGroup(SessionVariables.CurrentAccount, msgs);

            if (period.IsInteger())
            {
                regperiod = RegistrationPeriod.Load(period.ToInteger());
            }
            else
            {
                if (aag.IsNotEmpty())
                    regperiod.AccountActivityGroupId = aag.ID;
                else
                    regperiod.ActivityGroupId = group.ID;

                regperiod.CreatedBy = SessionVariables.LoggedInUser.ID.Value;
                regperiod.CreateDate = DateTime.UtcNow;
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
            ViewBag.ApplicationSeats = group.GetSeatCountByAssignmentType(RegistrationAssignmentType.Applied);
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

            var accountAcitivtyGroup = AccountActivityGroup.LoadByAccountIdByActivityGroupId(SessionVariables.CurrentAccount.ID.Value, group.ID.Value);

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
        public ActionResult Schedule(string id)
        {
            var group = ActivityGroup.LoadByUid(id.ToGuid());

            var schedule = new ActivitySchedule();
            if (id.ToInteger() > 0)
            {
                schedule = ActivitySchedule.Load(id.ToInteger());
            }
            else
            {
                schedule = ActivitySchedule.GetNewDefaultSchedule();
            }
            ViewBag.Messages = new SystemMessages();
            ViewBag.ShowScheduleList = false;
            ViewBag.ActivityGroupUid = group;
            return View(schedule);
        
    }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            var msgs = new SystemMessages();

            var league = new ActivityGroup()
            {
                ActivityGroupTypeId = ActivityGroupType.League,
                RegistrationModeId = RegistrationMode.Apply,
            };

            if (id.IsGuid())
                league = ActivityGroup.LoadByUid(id.ToGuid(), msgs);

            var aag = league.GetAccountActivityGroup(SessionVariables.CurrentAccount, msgs);

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = SessionVariables.CurrentAccount.GetForms();
            ViewBag.Grades = GradeLevel.Load();
            ViewBag.AccountActivityGroup = aag;

            return View(league);
        }

        [HttpPost]
        public ActionResult Edit(string id, string name, string description, string regmode, string form, string isinviteonly, string genderrestrictions, string mingrade, string maxgrade, string contactname, string contactphone, string contactemail)
        {
            var msgs = new SystemMessages();
            var league = ActivityGroup.LoadByUid(id.ToGuid(), msgs);
            var aag = league.GetAccountActivityGroup(SessionVariables.CurrentAccount, msgs);

            if (aag.IsNotEmpty())
            {
                if (!form.IsNumber())
                    msgs.Add("Form is required", MessageType.UserError);

                if (contactname.IsEmpty())
                    msgs.Add("Contact name is required", MessageType.UserError);

                if (contactphone.IsEmpty())
                    msgs.Add("Contact phone is required", MessageType.UserError);
                else if (!contactphone.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                    msgs.Add("Please enter a valid phone number", MessageType.UserError);

                if (contactemail.IsEmpty())
                    msgs.Add("Contact email is required", MessageType.UserError);
                else if (!contactemail.IsRegExMatch(BasicLibrary.Utilities.EmailRegEx))
                    msgs.Add("Please enter a valid email", MessageType.UserError);

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        aag.ContactEmail = contactemail;
                        aag.ContactPhone = contactphone;
                        aag.ContactName = contactname;

                        if (form.ToString().ToInteger() == 0)
                        {
                            var defaultForm = SessionVariables.CurrentAccount.GetOrCreateDefaultForm(SessionVariables.LoggedInUser, amsgs, adc);
                            var newForm = defaultForm.Copy(
                                SessionVariables.LoggedInUser.ID.Value,
                                SessionVariables.CurrentAccount.ID.Value,
                                (league.Name + " Form").MaxChars(100, "..."),
                                amsgs,
                                adc);
                            aag.FormId = newForm.ID;
                        }
                        else
                        {
                            aag.FormId = form.ToString().ToInteger();
                        }

                        amsgs.AddRange(aag.Save(adc));
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        msgs.Add("Saved successfully", MessageType.Success);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
                
            }
            else
            {
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

                league = ActivityGroup.SaveLeague(SessionVariables.LoggedInUser, SessionVariables.CurrentAccount, id, values, msgs);

                if (msgs.HasNoErrors())
                {
                    msgs.Add($"{UILabel.ActivityGroupLabel(league.ActivityGroupTypeId)} Saved", MessageType.Success);
                    UpdateSession(sat: SessionActionType.ActivityGroupCreate, accountId: SessionVariables.CurrentAccount.ID, activityGroupId: league.ID);
                }
            }

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = SessionVariables.CurrentAccount.GetForms();
            ViewBag.Grades = GradeLevel.Load();
            ViewBag.AccountActivityGroup = aag;

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

            var accountActivityGroup = AccountActivityGroup.LoadByAccountIdByActivityGroupId(SessionVariables.CurrentAccount.ID.Value, id.ToInteger(), msgs);
            msgs.AddRange(accountActivityGroup.UpdateInvitationStatus(inviteStatus, SessionVariables.CurrentAccount.ID.Value));

            return RedirectToAction("IndexExternal", new { id });
        }

        [HttpGet]
        public ActionResult PublishDetail(string id)
        {
            var msgs = new SystemMessages();
            var activityGroup = ActivityGroup.LoadByUid(id.ToGuid(), msgs);
            var account = SessionVariables.CurrentAccount;
            var accountActivityGroup = activityGroup.GetAccountActivityGroup(SessionVariables.CurrentAccount, msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = account;
            ViewBag.AccountActivityGroup = accountActivityGroup;

            if (accountActivityGroup.IsNotEmpty()) 
                ViewBag.aagRegistrationURL = accountActivityGroup.GetHomePageUrl(SessionVariables.IsAfterSchool.Value);

            return View(activityGroup);
        }

        [HttpGet]
        public ActionResult Publish(string id)
        {
            SystemMessages msgs = new SystemMessages();
            var activityGroup = ActivityGroup.LoadByUid(id.ToGuid());
            var account = SessionVariables.CurrentAccount;
            var accountActivityGroup = activityGroup.GetAccountActivityGroup(SessionVariables.CurrentAccount, msgs);

            if (accountActivityGroup.IsNotEmpty())
            {
                msgs.AddRange(accountActivityGroup.SetIsPublished(true, SessionVariables.LoggedInUser.ID.Value));
                ViewBag.aagRegistrationURL = accountActivityGroup.GetHomePageUrl(SessionVariables.IsAfterSchool.Value);
            }
            else
                msgs.AddRange(activityGroup.SetIsPublished(true, SessionVariables.LoggedInUser.ID.Value));

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been published", activityGroup.Name), MessageType.Success);
                UpdateSession(SessionActionType.ActivityGroupPublish, SessionVariables.CurrentAccount.ID, activityGroup.ID);
            }

            ViewBag.AccountActivityGroup = accountActivityGroup;
            ViewBag.Messages = msgs;

            return View("publishdetail", activityGroup);
        }

        [HttpGet]
        public ActionResult Unpublish(string id)
        {
            var msgs = new SystemMessages();
            var activityGroup = ActivityGroup.LoadByUid(id.ToGuid());
            var account = SessionVariables.CurrentAccount;
            var accountActivityGroup = activityGroup.GetAccountActivityGroup(SessionVariables.CurrentAccount, msgs);

            if (accountActivityGroup.IsNotEmpty())
            {
                ViewBag.aagRegistrationURL = accountActivityGroup.GetHomePageUrl(SessionVariables.IsAfterSchool.Value);
                msgs.AddRange(accountActivityGroup.SetIsPublished(false, SessionVariables.LoggedInUser.ID.Value));
            }
            else
                msgs.AddRange(activityGroup.SetIsPublished(false, SessionVariables.LoggedInUser.ID.Value));

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been unpublished", activityGroup.Name), MessageType.Success);
                UpdateSession(SessionActionType.ActivityGroupUnpublish, SessionVariables.CurrentAccount.ID, activityGroup.ID);
            }

            ViewBag.AccountActivityGroup = accountActivityGroup;
            ViewBag.Messages = msgs;

            return View("publishdetail", activityGroup);
        }
    }
}
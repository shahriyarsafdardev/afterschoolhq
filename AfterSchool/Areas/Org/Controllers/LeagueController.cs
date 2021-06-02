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
using AfterSchoolLibrary.Models;

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

            acceptedInvites = extAccountActivityGroups
                .Where(aag => aag.InviteStatusId == InviteStatus.Accepted && !aag.ActivityGroupIsArchived)
                .Select(aag => aag.GetActivityGroup())
                .ToList();

            pendingInvites = extAccountActivityGroups
                .Where(aag => aag.InviteStatusId == InviteStatus.Invited && !aag.ActivityGroupIsArchived)
                .Select(aag => aag.GetActivityGroup())
                .ToList();

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
            var league = ActivityGroup.LoadByUid(id.ToGuid());
            var aag = AccountActivityGroup.LoadByAccountIdByActivityGroupId(SessionVariables.CurrentAccount.ID.Value, league.ID.Value);

            if (!aag.IsPublished)
                return RedirectToAction("publishwizard1", new { id = league.Uid });

            var registrations = aag.GetRegistrations();
            var actions = SessionAction.LoadByActivityGroupId(
                league.ID.Value,
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
            ViewBag.AccountActivityGroup = aag;
            return View(league);
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

                regperiod.CreatedBy = SessionVariables.CurrentUser.ID.Value;
                regperiod.CreateDate = DateTime.UtcNow;
                regperiod.AccountId = SessionVariables.CurrentAccount.ID;
            }

            regperiod.StartDateTime = $"{opendate} {opentime}".ToDateTime();
            regperiod.EndDateTime = $"{closedate} {closetime}".ToDateTime();
            regperiod.LastModifiedBy = SessionVariables.CurrentUser.ID.Value;
            regperiod.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(regperiod.Save());

            PeriodViewBag(group, msgs);

            return View("periods", group);
        }

        [HttpPost]
        public ActionResult TeamQuickAdd(string id, string teamname, string seatsavailable)
        {
            var msgs = new SystemMessages();
            var activityGroup = ActivityGroup.LoadByUid(id.ToGuid(), msgs);
            var aag = activityGroup.GetAccountActivityGroup(SessionVariables.CurrentAccount, msgs);
            Activity activity = null;
            var otherActivities = Activity.LoadByActivityGroupId(activityGroup.ID.Value);

            otherActivities.ForEach((oa) =>
            {
                if(oa.Name.IsSameAs(teamname))
                    msgs.Add("Team name already exists in this League, please use a different name", MessageType.UserError);
            });

            if (msgs.HasNoErrors())
            {
                activity = new Activity()
                {
                    Name = teamname.MaxChars(50),
                    SeatsAvailable = seatsavailable.ToInteger(),
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = SessionVariables.CurrentUser.ID,
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = SessionVariables.CurrentUser.ID,
                    AccountActivityGroupId = aag?.ID,
                    ActivityGroupId = activityGroup.ID,
                    AccountId = SessionVariables.CurrentAccount.ID,
                    ActivityTypeId = ActivityType.Team,
                    ContactEmail = aag.ContactEmail,
                    ContactName = aag.ContactName,
                    ContactPhone = aag.ContactPhone
                };
                msgs.AddRange(activity.Save());
            }
            
            if (msgs.HasNoErrors())
                msgs.AddRange(activity.SetupTeam(SessionVariables.CurrentAccount, SessionVariables.CurrentUser.ID.Value));

            var group = ActivityGroup.LoadByUid(id.ToGuid());
            LoadLeagueActivityViewBag(group, id, "", "", "", msgs);

            return View("LeagueActivities", group);
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
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUid(id.ToGuid());
            var aag = group.GetAccountActivityGroup(SessionVariables.CurrentAccount, msgs);
            var registrations = new List<Registration>();

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

            registrations = aag.GetRegistrations(start, length);
            totalCount = aag.GetRegistrationCount();

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            if (emailed.IsBoolean() && emailed.ToBoolean())
                msgs.Add("An email will be sent shortly", MessageType.Success);

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
            var aag = group.GetAccountActivityGroup(SessionVariables.CurrentAccount);

            ViewBag.AccountActivityGroup = aag;
            ViewBag.Registrations = registrations;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.Registrations = registrations;
            ViewBag.RegistrantActivities = group.GetRegistrantActivities(registrations);
            ViewBag.ReservedSeats = aag.IsEmpty() ? group.GetSeatCountByAssignmentTypes(Registration.ReservedStatuses) : aag.GetSeatCountByAssignmentTypes(Registration.ReservedStatuses);
            ViewBag.WaitingListSeats = aag.IsEmpty() ? group.GetSeatCountByAssignmentType(RegistrationAssignmentType.WaitingList) : aag.GetSeatCountByAssignmentType(RegistrationAssignmentType.WaitingList);
            ViewBag.ApplicationSeats = aag.IsEmpty() ? group.GetSeatCountByAssignmentType(RegistrationAssignmentType.Applied) : aag.GetSeatCountByAssignmentType(RegistrationAssignmentType.Applied);
            ViewBag.ReturnUrl = Url.Action("registrations", "league", new { id = group.Uid });
        }

        // Teams in a league
        [HttpGet]
        public ActionResult LeagueActivities(string id, string pn, string rpp, string archive, SystemMessages msgs = null)
        {
            var group = ActivityGroup.LoadByUid(id.ToGuid());
            LoadLeagueActivityViewBag(group, id, pn, rpp, archive, msgs);
            return View(group);
        }

        private void LoadLeagueActivityViewBag(ActivityGroup group, string id, string pn, string rpp, string archive, SystemMessages msgs = null)
        {
            msgs = msgs.IsNotEmpty() ? msgs : new SystemMessages();
            int pageNumber = 1;
            int recordsPerPage = 10;

            var isArchived = group.IsArchived;

            if (pn.IsNotEmpty() && pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();
            
            if (rpp.IsNotEmpty() && rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            if (archive.IsNotEmpty() && archive.IsBoolean())
                isArchived = archive.ToInteger().ToBoolean();

            var accountActivityGroup = group.GetAccountActivityGroup(SessionVariables.CurrentAccount);

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;
            var account = SessionVariables.CurrentAccount;
            var activities = accountActivityGroup.IsEmpty() ? group.GetActivities(null, isArchived, false, start, length) : accountActivityGroup.GetActivities(null, isArchived, false, start, length);
            var totalCount = accountActivityGroup.IsEmpty() ? group.GetActivityCount(null, isArchived, false, null) : accountActivityGroup.GetActivityCount(null, isArchived, false);
            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();
            var finalActivities = activities.OrderBy(x => x.Name).ToList();

            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Activities = finalActivities;
            ViewBag.NeedToUpgrade = finalActivities.Count >= SessionVariables.AccountPackage.GetFeature(FeatureType.Activities).GetMaxValue();
            ViewBag.Messages = msgs;
            ViewBag.HasArchivedActivities = accountActivityGroup.IsEmpty() ? group.GetActivityCount(null, true, false, null) > 0 : accountActivityGroup.GetActivityCount(null, true, false) > 0;

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
                schedule = ActivitySchedule.Load(id.ToInteger()); // this isnt right, you'll end up loading an activity group and an activity schedule by the same id??
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
                RegistrationModeId = RegistrationMode.Apply
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
            var currentAccount = SessionVariables.CurrentAccount;
            var currentWebUser = SessionVariables.CurrentUser;

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
                            var defaultForm = currentAccount.GetOrCreateDefaultForm(currentWebUser, amsgs, adc);
                            var newForm = defaultForm.Copy(
                                currentWebUser.ID.Value,
                                currentAccount.ID.Value,
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

                league = ActivityGroupService.SaveLeague(currentWebUser, currentAccount, id, values, msgs);

                if (msgs.HasNoErrors())
                {
                    msgs.Add($"{UILabel.ActivityGroupLabel(league.ActivityGroupTypeId)} Saved", MessageType.Success);

                    // LOG: CurrentUser Updated ActivityGroup ActivityGroupName for Account AccountName
                    currentAccount.LogActivity(
                        webSessionId: SessionVariables.WebSession.ID.Value,
                        actor: currentWebUser,
                        actionType: WebSessionActionType.Updated,
                        objectId: league.ID.Value,
                        objectType: ObjectType.ActivityGroup,
                        objectText: league.Name,
                        indirectObjectId: currentAccount.ID.Value,
                        indirectObjectType: ObjectType.Account,
                        indirectObjectText: currentAccount.Name);
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
            activity.LastModifiedBy = SessionVariables.CurrentUser.ID.Value;
            msgs.AddRange(activity.Save());


            if (msgs.HasNoErrors())
            {
                msgs.AddRange(SendSubmitOrWithdrawnEmail(activity, true));

            }

            var group = activity.GetActivityGroup();
            LoadLeagueActivityViewBag(group, id, pn, rpp, archive);
            return View("LeagueActivities", group);
        }

        [HttpGet]
        public ActionResult WithdrawFromDistrict(string id, string pn, string rpp, string archive)
        {
            var msgs = new SystemMessages();

            var activity = Activity.LoadByUid(id.ToGuid());

            activity.DateSubmittedToActivityGroup = null;
            activity.LastModifiedDate = DateTime.UtcNow;
            activity.LastModifiedBy = SessionVariables.CurrentUser.ID.Value;
            msgs.AddRange(activity.Save());

            if (msgs.HasNoErrors())
            {
                msgs.AddRange(SendSubmitOrWithdrawnEmail(activity, false));

            }

            var group = activity.GetActivityGroup();
            LoadLeagueActivityViewBag(group, id, pn, rpp, archive);
            return View("LeagueActivities", group);
        }

        private SystemMessages SendSubmitOrWithdrawnEmail(Activity activity, bool isSubmitting)
        {

            var account = SessionVariables.CurrentAccount;
            var activityGroup = activity.GetActivityGroup();
            var hostAccount = activityGroup.GetAccount();
            return account.SendSubmitTeamtoDistrictEmail(hostAccount, activity, activityGroup, isSubmitting);
        }

        [HttpGet]
        public ActionResult UpdateInvitation(string id, InviteStatus inviteStatus)
        {
            var msgs = new SystemMessages();
            var aag = AccountActivityGroup.LoadByAccountIdByActivityGroupId(SessionVariables.CurrentAccount.ID.Value, id.ToInteger(), msgs);
            var league = aag.GetActivityGroup(msgs);
            msgs.AddRange(aag.UpdateInvitationStatus(inviteStatus, SessionVariables.CurrentAccount.ID.Value));

            if (msgs.HasNoErrors())
            {
                msgs.AddRange(aag.SendInviteStatusUpdateEmail(inviteStatus, SessionVariables.CurrentAccount, SessionVariables.CurrentUser, msgs));

                if (inviteStatus == InviteStatus.Accepted)
                    return RedirectToAction("publishwizard1", new { id = league.Uid });
            }

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
            var currentAccount = SessionVariables.CurrentAccount;
            var currentWebUser = SessionVariables.CurrentUser;
            var accountActivityGroup = activityGroup.GetAccountActivityGroup(currentAccount, msgs);

            if (accountActivityGroup.IsNotEmpty())
            {
                msgs.AddRange(accountActivityGroup.SetIsPublished(true, currentWebUser.ID.Value));
                ViewBag.aagRegistrationURL = accountActivityGroup.GetHomePageUrl(SessionVariables.IsAfterSchool.Value);
            }
            else
                msgs.AddRange(activityGroup.SetIsPublished(true, currentWebUser.ID.Value));

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been published", activityGroup.Name), MessageType.Success);

                // LOG: CurrentWebUser published ActivityGroup ActivityGroupName to Account AccountName's Profile
                currentAccount.LogActivity(
                    webSessionId: SessionVariables.WebSession.ID.Value,
                    actor: currentWebUser,
                    actionType: WebSessionActionType.Published,
                    objectId: activityGroup.ID.Value,
                    objectType: ObjectType.ActivityGroup,
                    objectText: $"{currentAccount}'s Profile");
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
            var currentAccount = SessionVariables.CurrentAccount;
            var currentWebUser = SessionVariables.CurrentUser;
            var accountActivityGroup = activityGroup.GetAccountActivityGroup(currentAccount, msgs);

            if (accountActivityGroup.IsNotEmpty())
            {
                ViewBag.aagRegistrationURL = accountActivityGroup.GetHomePageUrl(SessionVariables.IsAfterSchool.Value);
                msgs.AddRange(accountActivityGroup.SetIsPublished(false, currentWebUser.ID.Value));
            }
            else
                msgs.AddRange(activityGroup.SetIsPublished(false, currentWebUser.ID.Value));

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been unpublished", activityGroup.Name), MessageType.Success);
                // LOG: CurrentWebUser unpublished ActivityGroup ActivityGroupName to Account AccountName's Profile
                currentAccount.LogActivity(
                    webSessionId: SessionVariables.WebSession.ID.Value,
                    actor: currentWebUser,
                    actionType: WebSessionActionType.Unpublished,
                    objectId: activityGroup.ID.Value,
                    objectType: ObjectType.ActivityGroup,
                    objectText: $"{currentAccount}'s Profile");
            }

            ViewBag.AccountActivityGroup = accountActivityGroup;
            ViewBag.Messages = msgs;

            return View("publishdetail", activityGroup);
        }

        private List<FormQuestion> GetAddRegistrationQuestions(Form form, SystemMessages msgs)
        {
            var questions = form.GetActiveQuestions(msgs);

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

            return questions;
        }

        [HttpGet]
        public ActionResult AddRegistration(string id)
        {
            var msgs = new SystemMessages();
            var league = ActivityGroup.LoadByUid(id.ToGuid(), msgs);
            var aag = league.GetAccountActivityGroup(SessionVariables.CurrentAccount, msgs);
            var form = aag.IsEmpty() ? league.GetForm(msgs) : aag.GetForm(msgs);
            var questions = GetAddRegistrationQuestions(form, msgs);

            ViewBag.QuestionViewModels = questions.OrderBy(question => question.DisplayOrder).Select(question => new FormQuestionViewModel(question)).ToList();
            ViewBag.Messages = msgs;
            ViewBag.AccountActivityGroup = aag;

            return View(league);
        }

        [HttpPost]
        public ActionResult AddRegistration(string id, string parentsig)
        {
            var msgs = new SystemMessages();
            var league = ActivityGroup.LoadByUid(id.ToGuid(), msgs);
            var aag = league.GetAccountActivityGroup(SessionVariables.CurrentAccount, msgs);
            var form = aag.IsEmpty() ? league.GetForm(msgs) : aag.GetForm(msgs);
            var questions = GetAddRegistrationQuestions(form, msgs);

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
                    participationObject: aag.GetParticipationObject(msgs),
                    questionsAndAnswers: qvms.Select(qvm => (qvm.Question, qvm.Value)).ToList(),
                    period: aag.GetCurrentRegistrationPeriod(msgs),
                    loggedInUser: SessionVariables.CurrentUser,
                    msgs: msgs);

            if (msgs.HasNoErrors())
                return RedirectToAction("registrations", new { id = league.Uid });

            ViewBag.Messages = msgs;
            ViewBag.QuestionViewModels = qvms;

            return View(league);
        }

        [HttpGet]
        public ActionResult PublishWizard1(string id)
        {
            var msgs = new SystemMessages();
            var league = ActivityGroup.LoadByUid(id.ToGuid());
            var aag = league.GetAccountActivityGroup(SessionVariables.CurrentAccount, msgs);

            if (aag.FormId.IsEmpty())
                aag.FormId = SessionVariables.CurrentAccount.GetForms().FirstOrDefault()?.ID;
            if (aag.ContactName.IsEmpty())
                aag.ContactName = SessionVariables.CurrentUser.DisplayName;
            if (aag.ContactPhone.IsEmpty())
                aag.ContactPhone = SessionVariables.CurrentUser.Phone;
            if (aag.ContactEmail.IsEmpty())
                aag.ContactEmail = SessionVariables.CurrentUser.Email;

            msgs.AddRange(aag.Save());
            
            ViewBag.AccountActivityGroup = aag;
            ViewBag.Messages = msgs;

            return View(league);
        }

        [HttpPost]
        public ActionResult PublishWizard1(string id, string contactname, string contactphone, string contactemail)
        {
            var msgs = new SystemMessages();
            var league = ActivityGroup.LoadByUid(id.ToGuid(), msgs);
            var aag = league.GetAccountActivityGroup(SessionVariables.CurrentAccount, msgs);

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

            aag.ContactEmail = contactemail;
            aag.ContactPhone = contactphone;
            aag.ContactName = contactname;
            msgs.AddRange(aag.Save());

            if (msgs.HasNoErrors())
                return RedirectToAction("publishwizard2", new { id = league.Uid });

            ViewBag.Messages = msgs;
            ViewBag.AccountActivityGroup = aag;

            return View(league);
        }

        [HttpGet]
        public ActionResult PublishWizard2(string id)
        {
            var msgs = new SystemMessages();
            var league = ActivityGroup.LoadByUid(id.ToGuid());
            var aag = league.GetAccountActivityGroup(SessionVariables.CurrentAccount, msgs);
            var period = new RegistrationPeriod()
            {
                StartDateTime = SessionVariables.CurrentAccount.ConvertToAccountTimeZone(DateTime.UtcNow.Date),
                EndDateTime = SessionVariables.CurrentAccount.ConvertToAccountTimeZone(DateTime.UtcNow.Date).AddWeeks(1)
            };

            ViewBag.Messages = msgs;
            ViewBag.Period = period;
            ViewBag.AccountActivityGroup = aag;

            return View(league);
        }

        [HttpPost]
        public ActionResult PublishWizard2(string id, string opendate, string opentime, string closedate, string closetime)
        {
            var msgs = new SystemMessages();
            var league = ActivityGroup.LoadByUid(id.ToGuid());
            var aag = league.GetAccountActivityGroup(SessionVariables.CurrentAccount, msgs);
            var period = aag.GetCurrentOrNextRegistrationPeriod() ?? new RegistrationPeriod()
            {
                StartDateTime = SessionVariables.CurrentAccount.ConvertToAccountTimeZone(DateTime.UtcNow.Date),
                EndDateTime = SessionVariables.CurrentAccount.ConvertToAccountTimeZone(DateTime.UtcNow.Date).AddWeeks(1)
            };

            if (opendate.IsEmpty())
                msgs.Add("Registration Open Date is required", MessageType.UserError);
            if (opentime.IsEmpty())
                msgs.Add("Registration Open Time is required", MessageType.UserError);
            if (closedate.IsEmpty())
                msgs.Add("Registration Close Date is required", MessageType.UserError);
            if (closetime.IsEmpty())
                msgs.Add("Registration Close Time is required", MessageType.UserError);

            if (!$"{opendate} {opentime}".IsDateTime())
                msgs.Add("Registration Open Date and Time must be a valid date & time", MessageType.UserError);
            if (!$"{closedate} {closetime}".IsDateTime())
                msgs.Add("Registration Close Date and Time must be a valid date & time", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        period = new RegistrationPeriod()
                        {
                            StartDateTime = $"{opendate} {opentime}".ToDateTime(),
                            EndDateTime = $"{closedate} {closetime}".ToDateTime(),
                            LastModifiedBy = SessionVariables.CurrentUser.ID,
                            LastModifiedDate = DateTime.UtcNow,
                            CreateDate = DateTime.UtcNow,
                            CreatedBy = SessionVariables.CurrentUser.ID,
                            AccountActivityGroupId = aag.ID,
                            AccountId = SessionVariables.CurrentAccount.ID
                        };
                        amsgs.AddRange(period.Save(adc));

                        amsgs.AddRange(aag.SetIsPublished(true, SessionVariables.CurrentUser.ID.Value, adc));
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("publishwizard3", new { id = league.Uid });

            ViewBag.Messages = msgs;
            ViewBag.Period = period;
            ViewBag.AccountActivityGroup = aag;

            return View(league);
        }

        [HttpGet]
        public ActionResult PublishWizard3(string id)
        {
            var msgs = new SystemMessages();
            var league = ActivityGroup.LoadByUid(id.ToGuid());
            var aag = league.GetAccountActivityGroup(SessionVariables.CurrentAccount, msgs);

            ViewBag.Messages = msgs;
            ViewBag.AccountActivityGroup = aag;

            return View(league);

        }

        private void LoadTeamStudentsViewBag(StudentsViewBagObject studentsViewBagObject)
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
            ViewBag.AccountActivityGroup = studentsViewBagObject.AccountActivityGroup;
            ViewBag.Activity = studentsViewBagObject.Activity;
            ViewBag.isExternalLeague = AccountActivityGroup.IsExternalLeague(SessionVariables.CurrentAccount.ID.Value, studentsViewBagObject.ActivityGroup.ID.Value);

        }

        [HttpGet]
        public ActionResult TeamStudents(string id, string team)
        {
            var studentViewBagObject = new StudentsViewBagObject(id, team);
            LoadTeamStudentsViewBag(studentViewBagObject);

            return View(studentViewBagObject.ActivityGroup);
        }

        [HttpPost]
        public ActionResult TeamStudents(string id, string team, string cmd)
        {
            var studentViewBagObject = new StudentsViewBagObject(id, team);

            studentViewBagObject.Messages.AddRange(LeagueHelper.UpdateStudentsInTeam(
                studentViewBagObject.Activity, 
                studentViewBagObject.Registrants, 
                Request.Params.Filter("registrant-")));

            LoadTeamStudentsViewBag(studentViewBagObject);

            return View(studentViewBagObject.ActivityGroup);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using Hangfire;
using System.Collections;
using System.Web.Script.Serialization;

namespace AfterSchool.Areas.Org.Controllers
{
    public class GroupController : BaseLoggedInController
    {
        public GroupController() : base(AccountService.ProviderAccountTypes.ToList())
        {

        }

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

            totalCount = SessionVariables.CurrentAccount.GetSessionCount(isArchived);
            groups = SessionVariables.CurrentAccount.GetSessions(isArchived, start, length).OrderByDescending(x => x.CreateDate).ToList();

            hasArchivedGroups = SessionVariables.CurrentAccount.GetSessionCount(true) > 0;

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.GroupController = "group";
            ViewBag.Messages = new SystemMessages();
            ViewBag.HasArchivedGroups = hasArchivedGroups;
            ViewBag.ActivityGroupType = ActivityGroupType.Session;
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };

            return View(groups);
        }

        [HttpGet]
        public ActionResult Summary(string id)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value, msgs);
            var registrations = group.GetRegistrations(msgs: msgs);
            var actions = SessionAction.LoadByActivityGroupId(
                group.ID.Value,
                new List<SessionActionType>() {
                    SessionActionType.ActivityGroupCreate,
                    SessionActionType.ActivityGroupPublish,
                    SessionActionType.ActivityGroupUnpublish,
                },
                0,
                15,
                msgs);
            ViewBag.Messages = new SystemMessages();
            ViewBag.Actions = actions;
            ViewBag.Registrations = registrations;
            return View(group);
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            var group = new ActivityGroup()
            {
                ID = new int?(),
                RegistrationModeId = RegistrationMode.Register,
                ActivityGroupTypeId = ActivityGroupType.Session
            };

            if (id.IsGuid())
                group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);

            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = SessionVariables.CurrentAccount.GetForms();
            // ViewBag.Grades = GradeLevel.Load();
            // LoadEditScheduleViewBag(group, group.ActivityScheduleId.ToString());
            return View(group);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Edit(string id, string name, string description, string regmode, string form, string isinviteonly, string genderrestrictions, string mingrade, string maxgrade, string schedule, string scheduledetails, string requiresalldays, string repeatsweekly, string startdate, string enddate, string dailystarttime, string dailyendtime, string isongoing, string contactname, string contactemail, string contactphone, string seatsavailable, string waitinglistseats)
        {
            var msgs = new SystemMessages();
            var currentWebUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;
            // var inSessionDates = new List<DateRange>();
            // var outOfSessionDates = new List<DateTime>();

            var values = new Dictionary<string, object>()
            {
                { "name", name },
                { "description", description },
                { "regmode", regmode.IsNotEmpty() ? regmode : ((int)RegistrationMode.Register).ToString() },
                { "form", form },
                { "isinviteonly", isinviteonly.IsBoolean() },
                // { "genderrestrictions", genderrestrictions },
                // { "mingrade", mingrade },
                // { "maxgrade", maxgrade },
                // { "contactname", contactname },
                // { "contactemail", contactemail },
                // { "contactphone", contactphone },
                // { "seatsavailable", seatsavailable },
                // { "waitinglistseats", waitinglistseats }
            };

            //var filteredParams = Request.Params.Filter("schedulerepeat-");
            //var days = Models.ActivityScheduleUI.PrepDaysOfWeek(filteredParams);
            //inSessionDates = ActivityScheduleUI.PrepScheduleDates(repeatsweekly, days, startdate, enddate, dailystarttime, dailyendtime, Request.Params.Filter("meeting-open-"));
            //outOfSessionDates = ActivityScheduleUI.PrepScheduleDatesClosed(Request.Params.Filter("meeting-closed-"));

            //values.Add("scheduledetails", scheduledetails);
            //values.Add("days", days.Select(x => (int)x).ToArray().ToStringArray().ToStringList());
            //values.Add("repeatsweekly", repeatsweekly.IfBlank("0"));
            //values.Add("startdate", startdate);
            //values.Add("enddate", enddate);
            //values.Add("dailystarttime", dailystarttime);
            //values.Add("dailyendtime", dailyendtime);
            //values.Add("scheduledates", inSessionDates);
            //values.Add("scheduledatesclosed", outOfSessionDates);
            //values.Add("schedule", schedule);
            //values.Add("isongoing", isongoing);

            var group = ActivityGroupService.SaveSession(currentWebUser, currentAccount, id, values, msgs);

            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityGroupLabel(group.ActivityGroupTypeId)} Saved", MessageType.Success);

                // LOG: CurrentUser Saved ActivityGroup ActivityGroupName for Account AccountName
                WebSessionUtilities.LogActivity(
                    webSessionId: SessionVariables.WebSession.ID.Value,
                    actor: currentWebUser,
                    actionType: WebSessionActionType.Updated,
                    objectId: group.ID.Value,
                    objectType: ObjectType.ActivityGroup,
                    objectText: group.Name,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectText: currentAccount.Name);
            }

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = SessionVariables.CurrentAccount.GetForms();
            // ViewBag.Grades = GradeLevel.Load();
            // LoadEditScheduleViewBag(group, group.ActivityScheduleId.ToString());
            return View(group);
        }

        [HttpGet]
        public ActionResult EmailContent(string id)
        {
            var group = new ActivityGroup();
            if (id.IsGuid())
            {
                group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);
            }
            else
            {
                group.ID = new int?();
                group.RegistrationModeId = RegistrationMode.Register;
            }
            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Properties = group.GetGroupProperties();
            return View(group);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EmailContent(string id, string landingpagetext, string registrationthankyouemail, string applicationthankyouemail, string noactivitiestext)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);

            group.AddOrUpdateGroupProperty(AccountPropertyType.LandingPageText, landingpagetext, SessionVariables.CurrentUser.ID.Value, msgs);
            group.AddOrUpdateGroupProperty(AccountPropertyType.RegistrationThankYouEmail, registrationthankyouemail, SessionVariables.CurrentUser.ID.Value, msgs);
            group.AddOrUpdateGroupProperty(AccountPropertyType.ApplicationThankYouEmail, applicationthankyouemail, SessionVariables.CurrentUser.ID.Value, msgs);
            group.AddOrUpdateGroupProperty(AccountPropertyType.NoActivitiesText, noactivitiestext, SessionVariables.CurrentUser.ID.Value, msgs);

            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityGroupLabel(group.ActivityGroupTypeId)} Saved", MessageType.Success);
            }

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Properties = group.GetGroupProperties();
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
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);
            PeriodViewBag(group, new SystemMessages());
            return View(group);
        }

        [HttpPost]
        public ActionResult PeriodEdit(string id, string period, string opendate, string opentime, string closedate, string closetime)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), account.ID.Value, msgs);
            var regperiod = new RegistrationPeriod();

            if (period.IsInteger())
            {
                regperiod = RegistrationPeriod.Load(period.ToInteger());                
            }
            else
            {
                regperiod.CreatedBy = SessionVariables.CurrentUser.ID.Value;
                regperiod.CreateDate = DateTime.UtcNow;
                regperiod.ActivityGroupId = group.ID;
                regperiod.AccountId = account.ID;
            }

            regperiod.StartDateTime = string.Format("{0} {1}", opendate, opentime).ToDateTime();
            regperiod.EndDateTime = string.Format("{0} {1}", closedate, closetime).ToDateTime();
            regperiod.LastModifiedBy = SessionVariables.CurrentUser.ID.Value;
            regperiod.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(regperiod.Save());

            PeriodViewBag(group, msgs);

            return View("periods", group);
        }

        [HttpGet]
        public ActionResult Activities(string id, string pn, string rpp, string archive)
        {
            int pageNumber = 1;
            int recordsPerPage = 10;
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);
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
            var activities = group.GetActivities(null, isArchived, false, start, length);
            var totalCount = group.GetActivityCount(null, isArchived, false, null);
            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();
            var finalActivities = activities.OrderBy(x => x.Name).ToList();

            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Activities = finalActivities;
            ViewBag.NeedToUpgrade = finalActivities.Count >= SessionVariables.AccountPackage.GetFeature(FeatureType.Activities).GetMaxValue();
            ViewBag.Messages = new SystemMessages();
            ViewBag.HasArchivedActivities = group.GetActivityCount(null, true, false, null) > 0;

            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };
            return View(group);
        }

        [HttpGet]
        public ActionResult Registrations(string id, string pn, string rpp, string emailed)
        {
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);
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
            ViewBag.ReservedSeats = group.GetSeatCountByAssignmentTypes(Registration.ReservedStatuses);
            ViewBag.WaitingListSeats = group.GetSeatCountByAssignmentType(RegistrationAssignmentType.WaitingList);
            ViewBag.ApplicationSeats = group.GetSeatCountByAssignmentType(RegistrationAssignmentType.Applied);
            ViewBag.ReturnUrl = Url.Action("registrations", "group", new { id = group.Uid });
        }
        
        public ActionResult NotifyAll(string id)
        {
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);
            var loggedInUserId = SessionVariables.CurrentUser.ID.Value;
            
            BackgroundJob.Enqueue(() => ActivityGroupService.SendLotteryResults(group.ID.Value, loggedInUserId, null, null));

            return RedirectToAction("registrations", new { id = group.Uid, emailed = 1 });
        }

        [HttpPost]
        public ActionResult EmailRegistrationReport(string id, string q, string activity, string emails, string filename, string reporttype, string question)
        {
            var msgs = new SystemMessages();
            var activityIds = new List<int>();
            var questionId = new int?();
            var account = SessionVariables.CurrentAccount;
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), account.ID.Value);
            var participationObjectType = ParticipationObjectType.Activity;

            if (activity.IsNumber())
                activityIds = activity.ToInteger().AsList();
            else
                activityIds = group.GetActivities(null, false, false, null, null).Select(x => x.ID.Value).ToList();

            if (question.IsNumber())
                questionId = question.ToInteger();

            BackgroundJob.Enqueue(() =>
                RegistrationService.EmailCSVReport(
                    account.ID.Value, 
                    q, 
                    questionId,
                    activityIds,
                    participationObjectType,
                    filename, 
                    (RegistrationReportType)reporttype.ToInteger(),
                    null,
                    RegistrationStatus.NotSet,
                    UILabel.ActivityLabel(group.ActivityGroupTypeId, false), 
                    emails,
                    null,
                    null)
            );

            ViewBag.Messages = msgs;

            return RedirectToAction("registrations", new { id = group.Uid, q = q, activity = activity, emailed = "1", question = question });
        }

        [HttpGet]
        public ActionResult Invited(string id, string pn, string rpp)
        {
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);
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

            var contacts = group.GetInvitedContacts(start, length);
            totalCount = group.GetInvitedContactCount();

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Messages = new SystemMessages();
            ViewBag.Contacts = contacts;
            ViewBag.Account = SessionVariables.CurrentAccount;

            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };

            return View(group);
        }

        [HttpGet]
        public ActionResult Messages(string id, string pn, string rpp)
        {
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);

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

            var contacts = group.GetInvitedContacts(start, length);
            totalCount = group.GetInvitedContactCount();

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };

            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            return View(group);
        }

        [HttpGet]
        public ActionResult MessageDetail(string id)
        {
            var msgs = new SystemMessages();
            var am = ActivityMessage.LoadByUid(id.ToGuid(), msgs);
            ViewBag.Messages = msgs;
            ViewBag.Registrants = am.GetActivityMessageRegistrants(msgs);
            return View(am);
        }

        [HttpGet]
        public ActionResult MessageAdd(string id)
        {
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);

            ViewBag.Messages = new SystemMessages();
            ViewBag.Registrants = group.GetRegistrants()
                .Where(x => x.Email.IsNotBlank())
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToList();

            return View(group);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult MessageAdd(string id, string deliveryMethod, string subject, string body, string sendtostaff)
        {
            var msgs = new SystemMessages();

            if (subject.IsEmpty())
                msgs.Add("Subject is required", MessageType.UserError);
            if (body.IsEmpty())
                msgs.Add("Body is required", MessageType.UserError);

            ActivityMessage am = null;
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);

            if (msgs.HasNoErrors())
            {
                var registrantIds = group.GetRegistrants()
                                        .Where(x => Request[$"registrant-{x.ID.ToString()}"].IsNotBlank())
                                        .Select(x => x.ID.Value)
                                        .ToList();

                am = group.AddMessage(
                        subject, 
                        body, 
                        registrantIds,
                        sendtostaff.IsBoolean(),
                        SessionVariables.CurrentUser.ID.Value, 
                        msgs);

                BackgroundJob.Enqueue(() => ActivityMessageService.Send(am.ID.Value, SessionVariables.CurrentUser.ID.Value, null, null));
            }

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
            {
                msgs.Add("Your message will been sent to the selected registrants", MessageType.Success);
                ViewBag.Registrants = am.GetActivityMessageRegistrants();
                ViewBag.Messages = msgs;
                return View("messagedetail", am);
            }

            ViewBag.Messages = msgs;
            ViewBag.Registrants = group.GetRegistrants();
            return View(group);
        }

        [HttpGet]
        public ActionResult MessagesResend(string id)
        {
            var msgs = new SystemMessages();
            var am = ActivityMessage.LoadByUid(id.ToGuid(), msgs);
            var group = am.GetActivityGroup(msgs);

            BackgroundJob.Enqueue(() => ActivityMessageService.Send(am.ID.Value, SessionVariables.CurrentUser.ID.Value, null, null));

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
            {
                msgs.Add("Your message will been sent to the selected registrants", MessageType.Success);
                ViewBag.Registrants = am.GetActivityMessageRegistrants();
                ViewBag.Messages = msgs;
                return View("messagedetail", am);
            }

            ViewBag.Messages = msgs;
            ViewBag.Registrants = group.GetRegistrants();
            return View(group);
        }        

        [HttpGet]
        public ActionResult PublishDetail(string id)
        {
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);
            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = SessionVariables.CurrentAccount;
            return View(group);
        }

        [HttpGet]
        public ActionResult Publish(string id)
        {
            SystemMessages msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);

            msgs.AddRange(group.SetIsPublished(true, SessionVariables.CurrentUser.ID.Value));

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been published", group.Name), MessageType.Success);
                var currentAccount = SessionVariables.CurrentAccount;
                // LOG: CurrentUSer published ActivityGroup ActivityGroupName from AccountAccountName's Profile
                WebSessionUtilities.LogActivity(
                    webSessionId: SessionVariables.WebSession.ID.Value,
                    actor: SessionVariables.CurrentUser,
                    actionType: WebSessionActionType.Published,
                    objectId: group.ID.Value,
                    objectType: ObjectType.ActivityGroup,
                    objectText: group.Name,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectText: $"{currentAccount}'s Profile");
            }

            ViewBag.Messages = msgs;
            return View("publishdetail", group);
        }

        [HttpGet]
        public ActionResult Unpublish(string id, string qg)
        {
            SystemMessages msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);

            msgs.AddRange(group.SetIsPublished(false, SessionVariables.CurrentUser.ID.Value));

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been unpublished", group.Name), MessageType.Success);
                var currentAccount = SessionVariables.CurrentAccount;
                // LOG: CurrentUSer unpublished ActivityGroup ActivityGroupName from AccountAccountName's Profile
                WebSessionUtilities.LogActivity(
                    webSessionId: SessionVariables.WebSession.ID.Value,
                    actor: SessionVariables.CurrentUser,
                    actionType: WebSessionActionType.Unpublished,
                    objectId: group.ID.Value,
                    objectType: ObjectType.ActivityGroup,
                    objectText: group.Name,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectText: $"{currentAccount}'s Profile");
            }

            ViewBag.Messages = msgs;
            return View("publishdetail", group);
        }

        [HttpGet]
        public ActionResult Archive(string id)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);

            if (group.IsNotEmpty())
                msgs.AddRange(group.MoveToArchive(SessionVariables.CurrentUser.ID.Value));

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
                msgs.Add($"{group.Name} has been archived", MessageType.Success);

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "group");
        }

        [HttpGet]
        public ActionResult Restore(string id)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);

            if (group.IsNotEmpty())
                group.MoveFromArchive(SessionVariables.CurrentUser.ID.Value, msgs);

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
                msgs.Add($"{group.Name} has been restored", MessageType.Success);

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "group");
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);

            if (group.IsNotEmpty())
                group.SetIsDeleted(SessionVariables.CurrentUser.ID.Value, true, msgs);

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
                msgs.Add($"{group.Name} was deleted at {SessionVariables.CurrentAccount.ConvertToAccountTimeZone(DateTime.UtcNow).ToLongDateTimeString()}", MessageType.Success);

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "group");
        }

        [HttpGet]
        public ActionResult Schedules(string id)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);

            ViewBag.Messages = msgs;
            ViewBag.Schedules = group.GetSchedules();
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.Activities = group.GetActivities();

            return View(group);
        }        

        [HttpPost]
        public ActionResult Copy(string id, string groupnamecopy, string usesameformcopy)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUid(id.ToGuid()).Copy(groupnamecopy, true, SessionVariables.CurrentUser.ID.Value, msgs);

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
                return RedirectToAction("edit", new { id = group.Uid });

            return RedirectToAction("index", "group");
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
            var session = ActivityGroup.LoadByUid(id.ToGuid(), msgs);
            var form = session.GetForm(msgs);
            var questions = GetAddRegistrationQuestions(form, msgs).OrderBy(question => question.DisplayOrder).ToList();

            ViewBag.QuestionViewModels = questions.Select(question => new FormQuestionViewModel(question)).ToList();
            ViewBag.Activities = session.GetActivities(ActivityType.Activity, false, false, null, null, false, msgs).Select(x => x.GetParticipationObject(msgs)).ToList();
            ViewBag.Messages = msgs;

            return View(session);
        }

        [HttpPost]
        public ActionResult AddRegistration(string id, string parentsig)
        {
            var msgs = new SystemMessages();
            var session = ActivityGroup.LoadByUid(id.ToGuid(), msgs);
            var form = session.GetForm(msgs);
            var questions = GetAddRegistrationQuestions(form, msgs);
            var activities = session.GetActivities(ActivityType.Activity, false, false, null, null, false, msgs);

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

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            // get activity preferences
            // NEED TO ADD / TEST multiple activities to a registration
            // ENSURE THEY ARE APPLICATIONS TO EACH OF THESE ACTIVITIES
            var activitiesAndPreferences = new List<(Activity activity, int preferenceNumber)>();

            for (int i = 1; i <= activities.Count; i++)
            {
                var postedValue = Request[$"activity-preference-{i}"];
                if (postedValue.IsGuid())
                {
                    var activity = Activity.LoadByUid(postedValue.ToGuid(), msgs);
                    activitiesAndPreferences.Add((activity, i));
                }

            }

            if (msgs.HasNoErrors())
                SessionVariables.CurrentAccount.CreateNewRegistrantAndRegistration(
                    participationObject: session.GetParticipationObject(msgs),
                    questionsAndAnswers: qvms.Select(qvm => (qvm.Question, qvm.Value)).ToList(),
                    activitiesAndPreferences: activitiesAndPreferences,
                    period: session.GetCurrentRegistrationPeriod(msgs),
                    loggedInUser: SessionVariables.CurrentUser,
                    msgs: msgs);

            if (msgs.HasNoErrors())
                return RedirectToAction("registrations", new { id = session.Uid });

            ViewBag.Messages = msgs;
            ViewBag.Activities = activities;
            ViewBag.QuestionViewModels = qvms;

            return View(session);
        }

        [HttpGet]
        public ActionResult Placement(string id, string step, string period)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);
            var nextStep = 1;

            if (step.IsNumber())
            {
                nextStep = step.ToInteger();
            }

            switch (nextStep)
            {
                case 1:
                    var periods = group.GetRegistrationPeriods(msgs).Where(x => x.GetSeatCountByAssignmentType(RegistrationAssignmentType.Applied) > 0).ToList();
                    ViewBag.Periods = periods;
                    break;
                case 2:
                    var registrations = group.GetRegistrations();
                    var ras = group.GetRegistrantActivities(registrations);
                    var registrantAssignments = group.RandomizePlacementAttempt(SessionVariables.CurrentUser, msgs);

                    var assignments = new List<int>();
                    var assignmentsWL = new List<int>();
                    registrantAssignments.ForEach(x => assignments.AddRange(x.Assignments));
                    registrantAssignments.ForEach(x => assignmentsWL.AddRange(x.WaitingListAssignments));

                    var assignedRegistrationActivities = ras.Where(x => assignments.Contains(x.ID.Value)).ToList();
                    var waitingListActivities = ras.Where(x => assignmentsWL.Contains(x.ID.Value)).ToList();

                    ViewBag.RegistrantAssignments = registrantAssignments;
                    ViewBag.RegistrantActivities = ras;
                    ViewBag.AssignedRegistrantActivities = assignedRegistrationActivities;
                    ViewBag.WaitingListActivities = waitingListActivities;
                    ViewBag.Activities = group.GetActivities(null, false, false, null, null, false).OrderBy(x => x.Name).ToList();
                    break;
            }

            ViewBag.Step = nextStep;
            ViewBag.Messages = msgs;
            return View(group);
        }

        [HttpGet]
        public ActionResult ResetPlacement(string id)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);

            if (group.RegistrationModeId == RegistrationMode.Apply)
            {
                msgs.AddRange(group.RandomizePlacementReset(SessionVariables.CurrentUser, msgs));

                if (msgs.HasNoErrors())
                    msgs.Add("All Registration Placement has been reset", MessageType.Success);
            }

            var periods = group.GetRegistrationPeriods().Where(x => x.GetSeatCountByAssignmentType(RegistrationAssignmentType.Applied) > 0).ToList();
            ViewBag.Periods = periods;

            ViewBag.Messages = msgs;
            ViewBag.Step = 1;
            return View("Placement", group);
        }

        [HttpPost]
        public ActionResult Placement(string id, string step, string period, string assignmentsJSON)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);
            var nextStep = 1;
            if (step.IsNumber())
            {
                nextStep = step.ToInteger();
            }

            var regperiod = period.IsNumber() ? RegistrationPeriod.Load(period.ToInteger()) : null;
            var registrantAssignments = new JavaScriptSerializer() { MaxJsonLength = 50000000 }.Deserialize<List<RegistrantAssignment>>(assignmentsJSON);

            group.RandomizePlacementApply(registrantAssignments, SessionVariables.CurrentUser, msgs);

            var registrations = group.GetRegistrations();
            var ras = group.GetRegistrantActivities(registrations);
            var assignments = new List<int>();
            var assignmentsWL = new List<int>();
            registrantAssignments.ForEach(x => assignments.AddRange(x.Assignments));
            registrantAssignments.ForEach(x => assignmentsWL.AddRange(x.WaitingListAssignments));

            var assignedRegistrationActivities = ras.Where(x => assignments.Contains(x.ID.Value)).ToList();
            var waitingListActivities = ras.Where(x => assignmentsWL.Contains(x.ID.Value)).ToList();

            ViewBag.RegistrantAssignments = registrantAssignments;
            ViewBag.RegistrantActivities = ras;
            ViewBag.AssignedRegistrantActivities = assignedRegistrationActivities;
            ViewBag.WaitingListActivities = waitingListActivities;
            ViewBag.Activities = group.GetActivities(null, false, false, null, null, false, msgs).OrderBy(x => x.Name).ToList();

            ViewBag.Step = nextStep;
            ViewBag.Messages = msgs;
            return View(group);
        }

        /* Sports */
        // GET: Org/Sports
        public ActionResult IndexLeague(string pn, string rpp, string archive)
        {
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
            var leagues = SessionVariables.CurrentAccount.GetLeagues(isArchived, start, length).OrderByDescending(x => x.CreateDate).ToList();

            hasArchivedGroups = SessionVariables.CurrentAccount.GetLeagueCount(true) > 0;

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.GroupController = "group";
            ViewBag.GroupAction = "indexleague";
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

            return View(leagues);
        }

        [HttpGet]
        public ActionResult EditSetup(string id)
        {
            var msgs = new SystemMessages();
            ActivityGroup league = null;

            if (id.IsGuid())
            {
                league = ActivityGroup.LoadByUid(id.ToGuid(), msgs);
            }
            else
            {
                league = SessionVariables.CurrentAccount.GetDefaultNewLeague();
                league.RegistrationModeId = RegistrationMode.Register;
                league.StartDate = DateTime.UtcNow.AddWeeks(4).FirstDateOfWeek();
                league.EndDate = league.StartDate.Value.AddWeeks(8).LastDateOfWeek();
            }

            LoadEditSetupViewBag(league, msgs);
            ViewBag.ActiveMenu = "EditSetup";

            return View(league);
        }

        [HttpPost]
        public ActionResult EditSetup(string id, string name, string seatsavailable, string waitinglistseats, string registrationperiod, string registrationopendate, string registrationopentime, string registrationclosedate, string registrationclosetime, string timezone, string form, string regmode, string isinviteonly, string startdate, string enddate, string cmd)
        {
            var msgs = new SystemMessages();
            var values = new Dictionary<string, object>()
            {
                { "name", name },
                { "seatsavailable", seatsavailable },
                { "waitinglistseats", waitinglistseats },
                { "timezone", timezone },
                { "form", form },
                { "regmode", regmode },
                { "isinviteonly", isinviteonly },
                { "startdate", startdate },
                { "enddate", enddate },
            };
            var league = SaveLeague(id, values, msgs);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "next":
                        return RedirectToAction("editrestrictions", new { id = league.Uid, isnew = Request["isnew"], saved = 1 });
                }
            }

            LoadEditSetupViewBag(league, msgs);
            ViewBag.ActiveMenu = "EditSetup";
            return View(league);
        }

        private void LoadEditSetupViewBag(ActivityGroup league, SystemMessages msgs)
        {
            var activeForm = league?.GetForm(msgs) ?? SessionVariables.CurrentAccount.GetDefaultForm(msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = SessionVariables.CurrentAccount.GetForms(msgs).OrderBy(x => x.Name).ToList();
            ViewBag.ActiveForm = activeForm;
            ViewBag.IsFullScreen = GetIsFullScreen(league);
        }

        private bool GetIsFullScreen(ActivityGroup group)
        {
            ViewBag.FullScreenExitUrl = Url.Action($"index{(group.ActivityGroupTypeId == ActivityGroupType.League ? "league" : "")}", "group");

            return Request["isnew"] == "1" && !group.IsPublished;
        }

        [HttpGet]
        public ActionResult EditRestrictions(string id)
        {
            var league = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);

            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditRestrictions";
            ViewBag.IsFullScreen = GetIsFullScreen(league);
            return View(league);
        }

        [HttpPost]
        public ActionResult EditRestrictions(string id, string alloworganization, string agerestrictions, string agestart, string ageend, string graderestrictions, string mingrade, string maxgrade, string genderrestrictions, string requiresparentalconsent, string cmd)
        {
            var msgs = new SystemMessages();
            var gradeIds = new List<int>();
            var grades = GradeLevel.Load();

            grades.ForEach(grade => {
                if (Request["grade-" + grade.ID.ToString()].IsNotEmpty())
                    gradeIds.Add(grade.ID.Value);
            });

            var values = new Dictionary<string, object>()
            {
                { "alloworganization", alloworganization.IfBlank("0")},
                { "agestart", agerestrictions.IsNumber() ? agestart : "-1"},
                { "ageend", agerestrictions.IsNumber() ? ageend : "-1"},
                { "mingrade", graderestrictions.IsNumber() ? mingrade : ""},
                { "maxgrade", graderestrictions.IsNumber() ? maxgrade : ""},
                { "genderrestrictions", genderrestrictions},
                { "gradeIds", gradeIds.ToStrings().ToStringList() },
                { "requiresparentalconsent", requiresparentalconsent.IfBlank("0") }
            };

            var league = SaveLeague(id, values, msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        return RedirectToAction("editsetup", new { id = league.Uid, isnew = Request["isnew"], saved = 1 });
                    case "next":
                        return RedirectToAction("editdetails", new { id = league.Uid, isnew = Request["isnew"], saved = 1 });
                }
            }

            ViewBag.IsFullScreen = GetIsFullScreen(league);
            ViewBag.ActiveMenu = "EditRestrictions";
            return View(league);
        }

        [HttpGet]
        public ActionResult EditContact(string id)
        {
            var msgs = new SystemMessages();
            var league = ActivityGroup.LoadByUid(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Instructors = SessionVariables.CurrentAccount.GetUsers(msgs: msgs);
            ViewBag.ActiveMenu = "EditContact";
            ViewBag.IsFullScreen = GetIsFullScreen(league);
            return View(league);
        }

        [HttpPost]
        public ActionResult EditContact(string id, string contactname, string contactphone, string contactemail, string cmd)
        {
            var msgs = new SystemMessages();
            var values = new Dictionary<string, object>()
            {
                { "contactname", contactname},
                { "contactphone", contactphone},
                { "contactemail", contactemail},
            };
            var league = SaveLeague(id, values, msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen(league);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        return RedirectToAction("editdetails", new { id = league.Uid, isnew = Request["isnew"], saved = 1 });
                    case "next":
                        return RedirectToAction("editcost", new { id = league.Uid, isnew = Request["isnew"], saved = 1 });
                }
            }

            ViewBag.ActiveMenu = "EditCost";
            return View(league);
        }

        [HttpGet]
        public ActionResult EditDetails(string id)
        {
            var league = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);

            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditDetails";
            ViewBag.IsFullScreen = GetIsFullScreen(league);
            return View(league);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditDetails(string id, string description, string details, string cmd)
        {
            var msgs = new SystemMessages();

            if (details.IsEmpty())
                msgs.Add($"{UILabel.ActivityGroupLabel(ActivityGroupType.League)} Details is required", MessageType.UserError);


            var values = new Dictionary<string, object>()
            {
                { "details", details},
                { "description", description}
            };

            var league = SaveLeague(id, values, msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen(league);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        return RedirectToAction("editrestrictions", new { id = league.Uid, isnew = Request["isnew"], saved = 1 });
                    case "next":
                        return RedirectToAction("editcontact", new { id = league.Uid, isnew = Request["isnew"], saved = 1 });
                }
            }

            ViewBag.ActiveMenu = "EditDetails";
            return View(league);
        }

        private void LoadEditCostViewBag(ActivityGroup league, SystemMessages msgs)
        {
            var canProcessPayments = SessionVariables.CurrentAccount.CanProcessPayments(msgs);

            if (!canProcessPayments)
                league.AllowToPayLater = true;

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.CanProcessPayments = canProcessPayments;
            ViewBag.PaymentPlan = league.GetPaymentPlan();
            ViewBag.ActiveMenu = "EditCost";
            ViewBag.IsFullScreen = GetIsFullScreen(league);
        }

        [HttpGet]
        public ActionResult EditCost(string id)
        {
            var msgs = new SystemMessages();
            var league = ActivityGroup.LoadByUid(id.ToGuid(), msgs);
            LoadEditCostViewBag(league, msgs);
            return View(league);
        }

        [HttpPost]
        public ActionResult EditCost(string id, string cost, string costdescription, string allowtopaylater, string cmd, string haspaymentplan, string paymentplancost, string paymentplanperiod, string paymentplanperiodnumber, string paymentplanpaymentstarttype)
        {
            var msgs = new SystemMessages();
            var values = new Dictionary<string, object>()
            {
                { "cost", cost},
                { "costdescription", costdescription},
                { "allowtopaylater", allowtopaylater.IfBlank("0")},
                { "haspaymentplan", haspaymentplan.IfBlank("0") },
                { "paymentplancost", paymentplancost },
                { "paymentplanperiod", paymentplanperiod },
                { "paymentplanperiodnumber", paymentplanperiodnumber },
                { "paymentplanpaymentstarttype", paymentplanpaymentstarttype }
            };
            var league = SaveLeague(id, values, msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen(league);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        return RedirectToAction("editcontact", new { id = league.Uid, isnew = Request["isnew"], saved = 1 });
                    case "next":
                        return RedirectToAction("editconfirmation", new { id = league.Uid, isnew = Request["isnew"], saved = 1 });
                    case "publish":
                        return RedirectToAction("publish", new { id = league.Uid, isnew = Request["isnew"], saved = 1 });
                }
            }

            LoadEditCostViewBag(league, msgs);

            return View(league);
        }

        [HttpGet]
        public ActionResult EditConfirmation(string id)
        {
            var league = ActivityGroup.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value);

            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditConfirmation";
            ViewBag.IsFullScreen = GetIsFullScreen(league);
            return View(league);
        }

        private ActivityGroup SaveLeague(string id, Dictionary<string, object> values, SystemMessages msgs)
        {
            var league = ActivityGroupService.SaveLeague(
                loggedInUser: SessionVariables.CurrentUser,
                account: SessionVariables.CurrentAccount,
                activityGroupUid: id,
                values: values,
                msgs: msgs
            );

            if (msgs.HasNoErrors())
                msgs.Add($"{UILabel.ActivityGroupLabel(league.ActivityGroupTypeId)} Saved", MessageType.Success);

            return league;
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditConfirmation(string id, string registrationthankyouemail, string applicationthankyouemail, string cmd)
        {
            var msgs = new SystemMessages();
            var values = new Dictionary<string, object>()
            {
                { "registrationthankyouemail", registrationthankyouemail },
                { "applicationthankyouemail", applicationthankyouemail }
            };
            var league = SaveLeague(id, values, msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen(league);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        if (SessionVariables.CurrentAccount.CanProcessPayments())
                            return RedirectToAction("editcost", new { id = league.Uid, isnew = Request["isnew"], saved = 1 });
                        else
                            return RedirectToAction("editcontact", new { id = league.Uid, isnew = Request["isnew"], saved = 1 });
                    case "publish":
                        return RedirectToAction("publish", new { id = league.Uid, isnew = Request["isnew"], saved = 1 });
                    case "signupperiods":
                        return RedirectToAction("periods", new { id = league.Uid, saved = 1 });
                }
            }

            ViewBag.ActiveMenu = "EditConfirmation";
            return View(league);
        }

        [HttpPost]
        public ActionResult TeamQuickAdd(string id, string teamname, string seatsavailable)
        {
            var msgs = new SystemMessages();
            var league = ActivityGroup.LoadByUid(id.ToGuid(), msgs);
            Activity activity = null;
            var otherActivities = Activity.LoadByActivityGroupId(league.ID.Value);

            otherActivities.ForEach((oa) =>
            {
                if (oa.Name.IsSameAs(teamname))
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
                    ActivityGroupId = league.ID,
                    AccountId = SessionVariables.CurrentAccount.ID,
                    ActivityTypeId = ActivityType.Team,
                    ContactEmail = league.ContactEmail,
                    ContactName = league.ContactName,
                    ContactPhone = league.ContactPhone,
                    GradeLevelEnd = league.MaxGradeLevelId,
                    GradeLevelStart = league.MinGradeLevelId,
                    RequiresParentalConsent = league.RequiresParentalConsent,
                    GenderRestriction = league.GenderRestriction,
                    IsPublished = true,
                };
                msgs.AddRange(activity.Save());
            }

            if (msgs.HasNoErrors())
                msgs.AddRange(activity.SetupTeam(SessionVariables.CurrentAccount, SessionVariables.CurrentUser.ID.Value));

            return RedirectToAction("activities", new { id = league.Uid, saved = 1 });
        }

    }
}
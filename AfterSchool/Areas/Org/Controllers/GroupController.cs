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
        public GroupController() : base(new List<AccountType>() { AccountType.ActivityProvider, AccountType.School })
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
            var group = ActivityGroup.LoadByUid(id.ToGuid(), msgs);
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
                group = ActivityGroup.LoadByUid(id.ToGuid());

            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = SessionVariables.CurrentAccount.GetForms();
            ViewBag.Grades = GradeLevel.Load();
            LoadEditScheduleViewBag(group, group.ActivityScheduleId.ToString());
            return View(group);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Edit(string id, string name, string description, string regmode, string form, string isinviteonly, string genderrestrictions, string mingrade, string maxgrade, string schedule, string scheduledetails, string requiresalldays, string repeatsweekly, string startdate, string enddate, string dailystarttime, string dailyendtime, string isongoing, string contactname, string contactemail, string contactphone, string seatsavailable, string waitinglistseats)
        {
            var msgs = new SystemMessages();
            var inSessionDates = new List<DateRange>();
            var outOfSessionDates = new List<DateTime>();

            var values = new Dictionary<string, object>()
            {
                { "name", name },
                { "description", description },
                { "regmode", regmode },
                { "form", form },
                { "isinviteonly", 0.ToString() },
                { "genderrestrictions", genderrestrictions },
                { "mingrade", mingrade },
                { "maxgrade", maxgrade },
                { "contactname", contactname },
                { "contactemail", contactemail },
                { "contactphone", contactphone },
                { "seatsavailable", seatsavailable },
                { "waitinglistseats", waitinglistseats }
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

            var group = ActivityGroupService.SaveSession(SessionVariables.CurrentUser, SessionVariables.CurrentAccount, id, values, msgs);

            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityGroupLabel(group.ActivityGroupTypeId)} Saved", MessageType.Success);

                if (id.IsEmpty())
                    UpdateSession(sat: SessionActionType.ActivityGroupCreate, accountId: SessionVariables.CurrentAccount.ID, activityGroupId: group.ID);
            }

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = SessionVariables.CurrentAccount.GetForms();
            ViewBag.Grades = GradeLevel.Load();
            LoadEditScheduleViewBag(group, group.ActivityScheduleId.ToString());
            return View(group);
        }

        [HttpGet]
        public ActionResult EmailContent(string id)
        {
            var group = new ActivityGroup();
            if (id.IsGuid())
            {
                group = ActivityGroup.LoadByUid(id.ToGuid());
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
            var group = ActivityGroup.LoadByUid(id.ToGuid());

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
                regperiod.CreatedBy = SessionVariables.CurrentUser.ID.Value;
                regperiod.CreateDate = DateTime.UtcNow;
                regperiod.ActivityGroupId = group.ID;
                regperiod.AccountId = SessionVariables.CurrentAccount.ID;
            }

            regperiod.StartDateTime = string.Format("{0} {1}", opendate, opentime).ToDateTime();
            regperiod.EndDateTime = string.Format("{0} {1}", closedate, closetime).ToDateTime();
            regperiod.LastModifiedBy = SessionVariables.CurrentUser.ID.Value;
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
                RegistrationPeriodService.NotifyRegistrants(
                    regperiod.ID.Value, 
                    emailmessage, 
                    registrations.Select(x => x.ID.Value).ToList(), 
                    null)
            );

            msgs.Add("Your emails will be sent shortly", MessageType.Success);

            PeriodViewBag(group, msgs);

            return View("periods", group);
        }

        [HttpGet]
        public ActionResult Activities(string id, string pn, string rpp, string archive)
        {
            int pageNumber = 1;
            int recordsPerPage = 10;
            var group = ActivityGroup.LoadByUid(id.ToGuid());
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
            ViewBag.ReservedSeats = group.GetSeatCountByAssignmentTypes(Registration.ReservedStatuses);
            ViewBag.WaitingListSeats = group.GetSeatCountByAssignmentType(RegistrationAssignmentType.WaitingList);
            ViewBag.ApplicationSeats = group.GetSeatCountByAssignmentType(RegistrationAssignmentType.Applied);
            ViewBag.ReturnUrl = Url.Action("registrations", "group", new { id = group.Uid });
        }
        
        public ActionResult NotifyAll(string id)
        {
            var group = ActivityGroup.LoadByUid(id.ToGuid());

            // TODO: Need to add functionality back
            // BackgroundJob.Enqueue(() => ActivityGroup.SendRegistrationConfirmations(group.ID.Value, null, null));

            var msgs = new SystemMessages();
            msgs.Add("Not Implemented", MessageType.UserError);

            return RedirectToAction("registrations", new { id = group.Uid, emailed = 1 });
        }

        [HttpPost]
        public ActionResult EmailRegistrationReport(string id, string q, string activity, string emails, string filename, string reporttype, string question)
        {
            var msgs = new SystemMessages();
            var activityIds = new List<int>();
            var questionId = new int?();
            var account = SessionVariables.CurrentAccount;
            var group = ActivityGroup.LoadByUid(id.ToGuid());
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
            var group = ActivityGroup.LoadByUid(id.ToGuid());
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
        public ActionResult Messages(string id)
        {
            var group = ActivityGroup.LoadByUid(id.ToGuid());
            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            return View(group);
        }

        [HttpGet]
        public ActionResult MessageDetail(string id)
        {
            var am = ActivityMessage.Load(id.ToInteger());
            ViewBag.Messages = new SystemMessages();
            ViewBag.Registrants = am.GetActivityMessageRegistrants();
            return View(am);
        }

        [HttpGet]
        public ActionResult MessageAdd(string id)
        {
            var group = ActivityGroup.LoadByUid(id.ToGuid());

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
        public ActionResult MessageAdd(string id, string subject, string body)
        {
            var msgs = new SystemMessages();

            if (subject.IsEmpty())
                msgs.Add("Subject is required", MessageType.UserError);
            if (body.IsEmpty())
                msgs.Add("Body is required", MessageType.UserError);

            ActivityMessage am = null;
            var group = ActivityGroup.LoadByUid(id.ToGuid());

            if (msgs.HasNoErrors())
            {
                var registrantIds = group.GetRegistrants()
                    .Where(x => Request[$"registrant-{x.ID.ToString()}"].IsNotBlank())
                    .Select(x => x.ID.Value)
                    .ToList();
                am = group.AddMessage(subject, body, registrantIds, SessionVariables.CurrentUser.ID.Value, msgs);

                BackgroundJob.Enqueue(() => ActivityMessageService.Send(am.ID.Value, null));
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
            var am = ActivityMessage.Load(id.ToInteger());
            var group = am.GetActivityGroup();

            BackgroundJob.Enqueue(() => ActivityMessageService.Send(am.ID.Value, null));

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
            var group = ActivityGroup.LoadByUid(id.ToGuid());
            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = SessionVariables.CurrentAccount;
            return View(group);
        }

        [HttpGet]
        public ActionResult Publish(string id)
        {
            SystemMessages msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUid(id.ToGuid());

            msgs.AddRange(group.SetIsPublished(true, SessionVariables.CurrentUser.ID.Value));

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been published", group.Name), MessageType.Success);
                UpdateSession(sat: SessionActionType.ActivityGroupPublish, accountId: SessionVariables.CurrentAccount.ID, activityGroupId: group.ID);
            }

            ViewBag.Messages = msgs;
            return View("publishdetail", group);
        }

        [HttpGet]
        public ActionResult Unpublish(string id, string qg)
        {
            SystemMessages msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUid(id.ToGuid());

            msgs.AddRange(group.SetIsPublished(false, SessionVariables.CurrentUser.ID.Value));

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been unpublished", group.Name), MessageType.Success);
                UpdateSession(sat: SessionActionType.ActivityGroupPublish, accountId: SessionVariables.CurrentAccount.ID, activityGroupId: group.ID);
            }

            ViewBag.Messages = msgs;
            return View("publishdetail", group);
        }

        [HttpGet]
        public ActionResult Archive(string id)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUid(id.ToGuid());

            if (group.IsNotEmpty())
            {
                msgs.AddRange(group.MoveToArchive(SessionVariables.CurrentUser.ID.Value));
            }

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been archived", group.Name), MessageType.Success);
            }

            if (Request.UrlReferrer.IsNotEmpty())
            {
                Response.Redirect(Request.UrlReferrer.ToString(), true);
            }

            return RedirectToAction("index", "group");
        }

        [HttpGet]
        public ActionResult Restore(string id)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUid(id.ToGuid());

            if (group.IsNotEmpty())
            {
                msgs.AddRange(group.MoveFromArchive(SessionVariables.CurrentUser.ID.Value));
            }

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been restored", group.Name), MessageType.Success);
            }

            if (Request.UrlReferrer.IsNotEmpty())
            {
                Response.Redirect(Request.UrlReferrer.ToString(), true);
            }

            return RedirectToAction("index", "group");
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

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditDetails(string id, string details, string scheduledetails, string cmd)
        {
            var msgs = new SystemMessages();
            Activity activity = null;

            if (id.IsNumber() && id.ToInteger() > 0)
            {
                var values = new Dictionary<string, object>()
                {
                    { "details", details},
                    { "scheduledetails", scheduledetails}
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: SessionVariables.CurrentUser.ID.Value,
                    account: SessionVariables.CurrentAccount,
                    activityId: id,
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
                        return View("EditSchedule", activity);
                    case "next":
                        ViewBag.ActiveMenu = "EditContact";
                        return View("EditContact", activity);
                }
            }

            ViewBag.ActiveMenu = "EditDetails";
            return View(activity);
        }

        private void LoadEditScheduleViewBag(ActivityGroup activityGroup, string scheduleId)
        {
            ViewBag.Schedules = SessionVariables.CurrentAccount.GetActivitySchedules();

            ActivitySchedule schedule = null;

            // changing schedule
            if (scheduleId.IsNumber())
            {
                schedule = ActivitySchedule.Load(scheduleId.ToInteger());
                if (schedule.AccountId != SessionVariables.CurrentAccount.ID)
                    schedule = null;
            }

            if (schedule.IsEmpty())
                schedule = activityGroup.GetSchedule();

            if (schedule.IsEmpty())
                schedule = ActivitySchedule.GetNewDefaultSchedule();

            ViewBag.Schedule = schedule;
            ViewBag.ActivityInstances = schedule.GetInstances();
            ViewBag.ShowScheduleList = SessionVariables.AccountPackage.HasFeature(FeatureType.ManageActivityGroups);
        }

        [HttpPost]
        public ActionResult Copy(string id, string groupnamecopy, string usesameformcopy)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUid(id.ToGuid()).Copy(groupnamecopy, true, SessionVariables.CurrentUser.ID.Value, msgs);

            ViewBag.Messages = msgs;
            if (msgs.HasNoErrors())
            {
                return RedirectToAction("edit", new { id = group.Uid });
            }
            else
            {
                return RedirectToAction("index", "group");
            }
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
            ViewBag.Activities = session.GetActivities(ActivityType.Activity, false, false, null, null, false, msgs);
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
            var group = ActivityGroup.LoadByUid(id.ToGuid());
            var nextStep = 1;

            if (step.IsNumber())
            {
                nextStep = step.ToInteger();
            }

            switch (nextStep)
            {
                case 1:
                    var periods = group.GetRegistrationPeriods().Where(x => x.GetSeatCountByAssignmentType(RegistrationAssignmentType.Applied) > 0).ToList();
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
            var group = ActivityGroup.LoadByUid(id.ToGuid());

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
            var group = ActivityGroup.LoadByUid(id.ToGuid());
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

    }
}
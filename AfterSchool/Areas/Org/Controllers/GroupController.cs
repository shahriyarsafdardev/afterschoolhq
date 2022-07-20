using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Web.Mvc;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using Hangfire;
using Extensions = AfterSchoolLibrary.Extensions;
using Utilities = BasicLibrary.Utilities;

namespace AfterSchool.Areas.Org.Controllers
{
    public class GroupController : BaseLoggedInController
    {
        public GroupController() : base(FormConstants.ProviderAccountTypes.ToList())
        {
        }

        [HttpGet]
        public ActionResult Index(string pn, string rpp, string archive)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var pageNumber = 1;
            var recordsPerPage = 10;
            var isArchived = false;

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

            totalCount = currentAccount.GetSessionCount(isArchived, msgs);
            var groups = currentAccount.GetSessions(isArchived, start, length, false, msgs)
                .OrderByDescending(x => x.CreateDate).ToList();
            hasArchivedGroups = currentAccount.GetSessionCount(true, msgs) > 0;
            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.GroupController = "group";
            ViewBag.Messages = msgs;
            ViewBag.HasArchivedGroups = hasArchivedGroups;
            ViewBag.ActivityGroupType = ActivityGroupType.Session;
            ViewBag.PagingModel = new PagingModel
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages,
            };

            return View(groups);
        }

        [HttpGet]
        public ActionResult Summary(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = currentAccount.GetActivityGroup(id.ToGuid(), msgs);
            var registrations = group.GetRegistrations(msgs: msgs);

            ViewBag.Messages = msgs;
            ViewBag.Registrations = registrations;
            return View(group);
        }

        [HttpGet]
        public ActionResult Edit(string id, string isnew)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            ActivityGroup group = null;

            if (id.IsNotGuid() && isnew.IsEmpty())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (isnew.IsNotEmpty() && isnew.ToBoolean())
                group = new ActivityGroup
                {
                    ID = new int?(),
                    RegistrationModeId = RegistrationMode.Register,
                    ActivityGroupTypeId = ActivityGroupType.Session,
                };
            else
                group = currentAccount.GetActivityGroup(id.ToGuid(), msgs);

            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = currentAccount.GetForms(msgs);
            ViewBag.Messages = msgs;

            return View(group);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Edit(string id, string name, string description, string regmode, string form,
            string isinviteonly, string genderrestrictions, string mingrade, string maxgrade, string schedule,
            string scheduledetails, string requiresalldays, string repeatsweekly, string startdate, string enddate,
            string dailystarttime, string dailyendtime, string isongoing, string contactname, string contactemail,
            string contactphone, string seatsavailable, string waitinglistseats)
        {
            var msgs = new SystemMessages();
            var currentWebUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;

            var values = new Dictionary<string, object>
            {
                {"name", name},
                {"description", description},
                {"regmode", regmode.IsNotEmpty() ? regmode : ((int) RegistrationMode.Register).ToString()},
                {"form", form},
                {"isinviteonly", isinviteonly.IsBoolean()},
            };

            var group = ActivityGroupService.SaveSession(currentWebUser, currentAccount, id, values, msgs);

            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityGroupLabel(group.ActivityGroupTypeId)} Saved", MessageType.Success);

                // LOG: CurrentUser Saved ActivityGroup ActivityGroupName for Account AccountName
                SessionVariables.WebSession.LogActivity(
                    currentAccount,
                    currentWebUser,
                    WebSessionActionType.Updated,
                    objectId: group.ID.Value,
                    objectType: ObjectType.ActivityGroup,
                    objectText: group.Name,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectText: currentAccount.Name);
            }

            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = currentAccount.GetForms(msgs);
            ViewBag.Messages = msgs;

            return View(group);
        }

        [HttpGet]
        public ActionResult EmailContent(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var group = new ActivityGroup();

            if (id.IsGuid())
            {
                group = currentAccount.GetActivityGroup(id.ToGuid(), msgs);
            }
            else
            {
                group.ID = new int?();
                group.RegistrationModeId = RegistrationMode.Register;
            }

            ViewBag.Properties = group.GetGroupProperties(msgs);
            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            return View(group);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EmailContent(string id, string landingpagetext, string registrationthankyouemail,
            string applicationthankyouemail, string noactivitiestext)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = currentAccount.GetActivityGroup(id.ToGuid(), msgs);

            group.AddOrUpdateGroupProperty(AccountPropertyType.LandingPageText, landingpagetext, currentUser.ID.Value,
                msgs);
            group.AddOrUpdateGroupProperty(AccountPropertyType.RegistrationThankYouEmail, registrationthankyouemail,
                currentUser.ID.Value, msgs);
            group.AddOrUpdateGroupProperty(AccountPropertyType.ApplicationThankYouEmail, applicationthankyouemail,
                currentUser.ID.Value, msgs);
            group.AddOrUpdateGroupProperty(AccountPropertyType.NoActivitiesText, noactivitiestext, currentUser.ID.Value,
                msgs);

            if (msgs.HasNoErrors())
                msgs.Add($"{UILabel.ActivityGroupLabel(group.ActivityGroupTypeId)} Saved", MessageType.Success);

            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Properties = group.GetGroupProperties(msgs);
            ViewBag.Messages = msgs;
            return View(group);
        }

        public void PeriodViewBag(ActivityGroup group, SystemMessages msgs)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var defaultOpenDateTime = string.Format("{0} {1}",
                currentAccount.ConvertToAccountTimeZone(DateTime.UtcNow).ToShortDateString(), "8:00 AM").ToDateTime();
            var defaultCloseDateTime = string.Format("{0} {1}",
                    currentAccount.ConvertToAccountTimeZone(DateTime.UtcNow).AddWeeks(1).ToShortDateString(),
                    "12:00 AM")
                .ToDateTime();

            ViewBag.Periods = currentAccount.GetRegistrationPeriodsByGroupId(group.ID.Value, msgs)
                .OrderBy(x => x.StartDateTime.Value).ToList();
            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.DefaultOpenDateTime = defaultOpenDateTime;
            ViewBag.DefaultCloseDateTime = defaultCloseDateTime;
        }

        [HttpGet]
        public ActionResult Periods(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = currentAccount.GetActivityGroup(id.ToGuid(), msgs);

            PeriodViewBag(group, msgs);

            ViewBag.Messages = msgs;

            return View(group);
        }

        [HttpPost]
        public ActionResult PeriodEdit(string id, string period, string opendate, string opentime, string closedate,
            string closetime)
        {
            var msgs = new SystemMessages();
            var currentUser = SessionVariables.CurrentUser;
            var account = SessionVariables.CurrentAccount;

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var group = account.GetActivityGroup(id.ToGuid(), msgs);

            RegistrationPeriodService.CreateOrUpdate(currentUser, account, group, period, opendate, closedate, opentime,
                closetime, msgs);

            PeriodViewBag(group, msgs);
            ViewBag.Messages = msgs;
            return View("periods", group);
        }

        [HttpGet]
        public ActionResult Activities(string id, string pn, string rpp, string archive)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var accountPackage = SessionVariables.AccountPackage;
            var pageNumber = 1;
            var recordsPerPage = 10;

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = account.GetActivityGroup(id.ToGuid(), msgs);
            var isArchived = group.IsArchived;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            if (archive.IsBoolean())
                isArchived = archive.ToInteger().ToBoolean();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            var activities = group.GetActivities(null, isArchived, false, start, length, false, msgs);
            var totalCount = group.GetActivityCount(null, isArchived, false, null, msgs);
            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();
            var finalActivities = activities.OrderBy(x => x.Name).ToList();

            ViewBag.AccountPackage = accountPackage;
            ViewBag.Activities = finalActivities;
            ViewBag.HasArchivedActivities = group.GetActivityCount(null, true, false, null, msgs) > 0;
            ViewBag.Messages = msgs;

            ViewBag.PagingModel = new PagingModel
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages,
            };
            return View(group);
        }

        [HttpGet]
        public ActionResult Registrations(string id, string pn, string rpp, string emailed)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = account.GetActivityGroup(id.ToGuid(), msgs);
            var pageNumber = 1;
            var recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;
            var totalCount = 0;

            var registrations = group.GetRegistrations(start, length, msgs);
            totalCount = group.GetRegistrationCount(msgs);

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            if (emailed.IsBoolean() && emailed.ToBoolean())
                msgs.Add("An email will be sent shortly", MessageType.Success);

            LoadRegistrationsViewBag(group, registrations, msgs);
            ViewBag.Messages = msgs;

            ViewBag.PagingModel = new PagingModel
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages,
            };

            return View(group);
        }

        private void LoadRegistrationsViewBag(ActivityGroup group, List<Registration> registrations,
            SystemMessages msgs)
        {
            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            ViewBag.Registrations = registrations;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.Registrations = registrations;
            ViewBag.RegistrantActivities = group.GetRegistrantActivities(registrations, msgs);
            ViewBag.ReservedSeats = group.GetSeatCountByAssignmentTypes(Registration.ReservedStatuses, msgs);
            ViewBag.WaitingListSeats = group.GetSeatCountByAssignmentType(RegistrationAssignmentType.WaitingList, msgs);
            ViewBag.ApplicationSeats = group.GetSeatCountByAssignmentType(RegistrationAssignmentType.Applied, msgs);
            ViewBag.ReturnUrl = Url.Action("registrations", "group", new {id = group.Uid});
        }

        public ActionResult NotifyAll(string id)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = account.GetActivityGroup(id.ToGuid(), msgs);
            var loggedInUserId = SessionVariables.CurrentUser.ID.Value;
            var groupId = group.ID.Value;

            // cannot pass objects into the BackgroundJob.Enqueue
            BackgroundJob.Enqueue(() => ActivityGroupService.SendLotteryResults(groupId, loggedInUserId, null, null));
            ViewBag.Messages = msgs;

            return RedirectToAction("registrations", new {id = group.Uid, emailed = 1});
        }

        [HttpPost]
        public ActionResult EmailRegistrationReport(string id, string q, string activity, string emails,
            string filename, string reporttype, string question, string volunteers)
        {
            var account = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var activityIds = new List<int>();
            var questionId = new int?();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = account.GetActivityGroup(id.ToGuid(), msgs);
            var participationObjectType = ParticipationObjectType.Activity;

            if (activity.IsNumber())
                activityIds = activity.ToInteger().AsList();
            else
                activityIds = group.GetActivities(null, false, false, null, null, false, msgs).Select(x => x.ID.Value)
                    .ToList();

            if (question.IsNumber())
                questionId = question.ToInteger();

            var getVolunteers = volunteers.IsBoolean() && volunteers.ToBoolean();
            BackgroundJob.Enqueue(() =>
                RegistrationService.EmailCsvReport(
                    account.ID.Value,
                    q,
                    questionId,
                    activityIds,
                    participationObjectType,
                    filename,
                    (RegistrationReportType) reporttype.ToInteger(),
                    null,
                    RegistrationStatus.NotSet,
                    UILabel.ActivityLabel(group.ActivityGroupTypeId, false),
                    emails,
                    null,
                    null,
                    getVolunteers,
                    null,
                    null)
            );

            ViewBag.Messages = msgs;

            return RedirectToAction("registrations", new
            {
                id = group.Uid, q, activity, emailed = "1",
                question,
            });
        }

        [HttpGet]
        public ActionResult Invited(string id, string pn, string rpp)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = account.GetActivityGroup(id.ToGuid(), msgs);
            var pageNumber = 1;
            var recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;
            var totalCount = 0;

            var contacts = group.GetInvitedContacts(start, length, msgs);
            totalCount = group.GetInvitedContactCount();

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Messages = msgs;
            ViewBag.Contacts = contacts;
            ViewBag.Account = account;

            ViewBag.PagingModel = new PagingModel
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages,
            };

            return View(group);
        }

        [HttpGet]
        public ActionResult Messages(string id, string pn, string rpp)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = account.GetActivityGroup(id.ToGuid(), msgs);

            var pageNumber = 1;
            var recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var totalCount = group.GetInvitedContactCount();
            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.PagingModel = new PagingModel
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages,
            };

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            return View(group);
        }

        [HttpGet]
        public ActionResult MessageDetail(string id)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var am = account.GetActivityMessage(id.ToGuid(), msgs);
            ViewBag.Registrants = am.GetActivityMessageRegistrants(msgs);
            ViewBag.Messages = msgs;
            return View(am);
        }

        [HttpGet]
        public ActionResult MessageAdd(string id)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = account.GetActivityGroup(id.ToGuid(), msgs);

            ViewBag.Registrants = group.GetRegistrants(msgs)
                .Where(x => x.Email.IsNotBlank())
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToList();
            ViewBag.Messages = msgs;

            return View(group);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult MessageAdd(string id, string deliveryMethod, string subject, string body,
            string sendtostaff)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (subject.IsEmpty())
                msgs.Add("Subject is required", MessageType.UserError);
            if (body.IsEmpty())
                msgs.Add("Body is required", MessageType.UserError);

            ActivityMessage am = null;

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = currentAccount.GetActivityGroup(id.ToGuid(), msgs);

            if (msgs.HasNoErrors())
            {
                var registrantIds = group.GetRegistrants(msgs)
                    .Where(x => Request[$"registrant-{x.ID.ToString()}"].IsNotBlank())
                    .Select(x => x.ID.Value)
                    .ToList();

                if (msgs.HasNoErrors())
                {
                    var willExceedQuota = currentAccount.WillExceedEmailQuota(SessionVariables.AccountPackage,
                        registrantIds.Count, currentUser, msgs);

                    if (!willExceedQuota)
                    {
                        am = group.AddMessage(
                            subject,
                            body,
                            registrantIds,
                            sendtostaff.IsBoolean() && sendtostaff.ToBoolean(),
                            currentUser.ID.Value,
                            msgs);

                        BackgroundJob.Enqueue(() =>
                            ActivityMessageService.Send(am.ID.Value, currentUser.ID.Value, null, null));

                        if (msgs.HasNoErrors())
                        {
                            msgs.Add("Your message will been sent to the selected registrants", MessageType.Success);
                            ViewBag.Registrants = am.GetActivityMessageRegistrants(msgs);
                            ViewBag.Messages = msgs;
                            return View("messagedetail", am);
                        }
                    }
                }
            }

            ViewBag.Registrants = group.GetRegistrants(msgs);
            ViewBag.Messages = msgs;
            return View(group);
        }

        [HttpGet]
        public ActionResult MessagesResend(string id)
        {
            var account = SessionVariables.CurrentAccount;

            if (id.IsNotGuid())
                return HttpNotFound();

            var msgs = new SystemMessages();

            var am = account.GetActivityMessage(id.ToGuid(), msgs);
            var group = am.GetActivityGroup(msgs);

            BackgroundJob.Enqueue(() => ActivityMessageService.Send(am.ID.Value, account.ID.Value, null, null));

            if (msgs.HasNoErrors())
            {
                msgs.Add("Your message will been sent to the selected registrants", MessageType.Success);
                ViewBag.Registrants = am.GetActivityMessageRegistrants(msgs);
                ViewBag.Messages = msgs;
                return View("messagedetail", am);
            }

            ViewBag.Registrants = group.GetRegistrants(msgs);
            ViewBag.Messages = msgs;
            return View(group);
        }

        [HttpGet]
        public ActionResult PublishDetail(string id)
        {
            var account = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = account.GetActivityGroup(id.ToGuid(), msgs);
            ViewBag.Messages = msgs;
            ViewBag.Account = account;
            return View(group);
        }

        [HttpGet]
        public ActionResult Publish(string id)
        {
            var account = SessionVariables.CurrentAccount;
            var currentuser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = account.GetActivityGroup(id.ToGuid(), msgs);

            msgs.AddRange(group.SetIsPublished(true, currentuser.ID.Value));

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been published", group.Name), MessageType.Success);

                // LOG: CurrentUSer published ActivityGroup ActivityGroupName from AccountAccountName's Profile
                SessionVariables.WebSession.LogActivity(
                    account,
                    currentuser,
                    WebSessionActionType.Published,
                    objectId: group.ID.Value,
                    objectType: ObjectType.ActivityGroup,
                    objectText: group.Name,
                    indirectObjectId: account.ID.Value,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectText: $"{account}'s Profile");
            }

            ViewBag.Messages = msgs;
            return View("publishdetail", group);
        }

        [HttpGet]
        public ActionResult Unpublish(string id, string qg)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentuser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = currentAccount.GetActivityGroup(id.ToGuid(), msgs);

            msgs.AddRange(group.SetIsPublished(false, currentuser.ID.Value));

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been unpublished", group.Name), MessageType.Success);

                // LOG: CurrentUSer unpublished ActivityGroup ActivityGroupName from AccountAccountName's Profile
                SessionVariables.WebSession.LogActivity(
                    currentAccount,
                    currentuser,
                    WebSessionActionType.Unpublished,
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
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = currentAccount.GetActivityGroup(id.ToGuid(), msgs);

            if (group.IsNotEmpty())
                msgs.AddRange(group.MoveToArchive(SessionVariables.CurrentUser.ID.Value));

            if (msgs.HasNoErrors())
                msgs.Add($"{group.Name} has been archived", MessageType.Success);

            ViewBag.Messages = msgs;

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "group");
        }

        [HttpGet]
        public ActionResult Restore(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = currentAccount.GetActivityGroup(id.ToGuid(), msgs);

            if (group.IsNotEmpty())
                group.MoveFromArchive(SessionVariables.CurrentUser.ID.Value, msgs);

            if (msgs.HasNoErrors())
                msgs.Add($"{group.Name} has been restored", MessageType.Success);

            ViewBag.Messages = msgs;

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "group");
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = currentAccount.GetActivityGroup(id.ToGuid(), msgs);

            if (group.IsNotEmpty())
                group.SetIsDeleted(SessionVariables.CurrentUser.ID.Value, true, msgs);

            if (msgs.HasNoErrors())
                msgs.Add(
                    $"{group.Name} was deleted at {currentAccount.ConvertToAccountTimeZone(DateTime.UtcNow).ToLongDateTimeString()}",
                    MessageType.Success);

            ViewBag.Messages = msgs;

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "group");
        }

        [HttpGet]
        public ActionResult Schedules(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = currentAccount.GetActivityGroup(id.ToGuid(), msgs);

            ViewBag.Schedules = group.GetSchedules(msgs);
            ViewBag.Account = currentAccount;
            ViewBag.Activities = group.GetActivities(msgs);
            ViewBag.Messages = msgs;

            return View(group);
        }

        [HttpPost]
        public ActionResult Copy(string id, string groupnamecopy, string usesameformcopy)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (id.IsNotGuid() && groupnamecopy.IsNotEmpty())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = currentAccount.GetActivityGroup(id.ToGuid(), msgs)?
                .Copy(groupnamecopy, true, SessionVariables.CurrentUser.ID.Value, msgs);

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
                return RedirectToAction("edit", new {id = group.Uid});

            return RedirectToAction("index", "group");
        }

        private List<FormQuestion> GetAddRegistrationQuestions(Form form, SystemMessages msgs)
        {
            var questions = form.GetActiveQuestions(msgs);

            questions.ForEach(question =>
            {
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
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var session = currentAccount.GetActivityGroup(id.ToGuid(), msgs);
            var form = session.GetForm(msgs);
            var questions = GetAddRegistrationQuestions(form, msgs).OrderBy(question => question.DisplayOrder).ToList();

            ViewBag.QuestionViewModels = questions.Select(question => new FormQuestionViewModel(question)).ToList();
            ViewBag.Activities = session.GetActivities(ActivityType.Activity, false, false, null, null, false, msgs)
                .Select(x => x.GetParticipationObject(msgs)).ToList();
            ViewBag.Messages = msgs;

            return View(session);
        }

        [HttpPost]
        public ActionResult AddRegistration(string id, string parentsig)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var session = currentAccount.GetActivityGroup(id.ToGuid(), msgs);
            var form = session.GetForm(msgs);
            var questions = GetAddRegistrationQuestions(form, msgs);
            var activities = session.GetActivities(ActivityType.Activity, false, false, null, null, false, msgs)
                .Select(x => x.GetParticipationObject(msgs)).ToList();

            questions.ForEach(question =>
            {
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);

                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                        if (answer.IsNotEmpty() && answer.Length < 3)
                            msgs.Add($"{question.Text} must be of 3 characters", MessageType.UserError);
                        break;
                    case QuestionDataType.RegistrantHomePhone:
                    case QuestionDataType.RegistrantWorkPhone:
                    case QuestionDataType.RegistrantMobilePhone:
                        if (answer.IsNotEmpty() && !answer.IsRegExMatch(Utilities.PhoneRegEx))
                            msgs.Add($"{question.Text} must be a valid phone number", MessageType.UserError);
                        break;
                }
            });

            var qvms = questions.Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            // get activity preferences
            // NEED TO ADD / TEST multiple activities to a registration
            // ENSURE THEY ARE APPLICATIONS TO EACH OF THESE ACTIVITIES
            var activitiesAndPreferences = new List<(Activity activity, int preferenceNumber)>();

            for (var i = 1; i <= activities.Count; i++)
            {
                var postedValue = Request[$"activity-preference-{i}"];
                var errorMessage = "All the Activtiy Preferences are required.";
                if (postedValue.IsEmpty() && !msgs.HasMessageContainingText(errorMessage))
                    msgs.Add(errorMessage, MessageType.UserError);

                if (postedValue.IsGuid())
                {
                    var activity = currentAccount.GetActivity(postedValue.ToGuid(), msgs);
                    activitiesAndPreferences.Add((activity, i));
                }
            }

            if (msgs.HasNoErrors())
                currentAccount.CreateNewRegistrantAndRegistration(
                    session.GetParticipationObject(msgs),
                    qvms.Select(qvm => (qvm.Question, qvm.Value)).ToList(),
                    activitiesAndPreferences,
                    session.GetCurrentRegistrationPeriod(msgs),
                    SessionVariables.CurrentUser,
                    msgs);

            if (msgs.HasNoErrors())
                return RedirectToAction("registrations", new {id = session.Uid});

            ViewBag.Messages = msgs;
            ViewBag.Activities = activities;
            ViewBag.QuestionViewModels = qvms;

            return View(session);
        }

        [HttpGet]
        public ActionResult Placement(string id, string step, string period)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = currentAccount.GetActivityGroup(id.ToGuid(), msgs);
            var nextStep = 1;

            if (step.IsNumber())
                nextStep = step.ToInteger();

            switch (nextStep)
            {
                case 1:
                    var periods = group.GetRegistrationPeriods(msgs).Where(x =>
                        x.GetSeatCountByAssignmentType(RegistrationAssignmentType.Applied) > 0).ToList();
                    ViewBag.Periods = periods;
                    break;
                case 2:
                    var registrations = group.GetRegistrations(null, null, msgs);
                    var ras = group.GetRegistrantActivities(registrations, msgs);
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
                    ViewBag.Activities = group.GetActivities(null, false, false, null, null, false, msgs)
                        .OrderBy(x => x.Name).ToList();
                    break;
            }

            ViewBag.Step = nextStep;
            ViewBag.Messages = msgs;
            return View(group);
        }

        [HttpGet]
        public ActionResult ResetPlacement(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = currentAccount.GetActivityGroup(id.ToGuid(), msgs);

            if (group.RegistrationModeId == RegistrationMode.Apply)
            {
                msgs.AddRange(group.RandomizePlacementReset(SessionVariables.CurrentUser, msgs));

                if (msgs.HasNoErrors())
                    msgs.Add("All Registration Placement has been reset", MessageType.Success);
            }

            var periods = group.GetRegistrationPeriods(msgs)
                .Where(x => x.GetSeatCountByAssignmentType(RegistrationAssignmentType.Applied) > 0).ToList();

            ViewBag.Periods = periods;
            ViewBag.Messages = msgs;
            ViewBag.Step = 1;
            return View("Placement", group);
        }

        [HttpPost]
        public ActionResult Placement(string id, string step, string period, string assignmentsJSON)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var group = currentAccount.GetActivityGroup(id.ToGuid(), msgs);
            var nextStep = 1;
            if (step.IsNumber())
                nextStep = step.ToInteger();

            var registrantAssignments = JsonSerializer.Deserialize<List<RegistrantAssignment>>(assignmentsJSON);

            group.RandomizePlacementApply(registrantAssignments, SessionVariables.CurrentUser, msgs);

            var registrations = group.GetRegistrations(msgs: msgs);
            var ras = group.GetRegistrantActivities(registrations, msgs);
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
            ViewBag.Activities = group.GetActivities(null, false, false, null, null, false, msgs).OrderBy(x => x.Name)
                .ToList();

            ViewBag.Step = nextStep;
            ViewBag.Messages = msgs;
            return View(group);
        }

        /* Sports */
        // GET: Org/Sports
        public ActionResult IndexLeague(string pn, string rpp, string archive)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var pageNumber = 1;
            var recordsPerPage = 10;
            var isArchived = false;

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

            totalCount = currentAccount.GetLeagueCount(isArchived, msgs);
            var leagues = currentAccount.GetLeagues(isArchived, start, length, msgs)
                .OrderByDescending(x => x.CreateDate).ToList();

            hasArchivedGroups = currentAccount.GetLeagueCount(true, msgs) > 0;

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.GroupController = "group";
            ViewBag.GroupAction = "indexleague";
            ViewBag.Messages = msgs;
            ViewBag.HasArchivedGroups = hasArchivedGroups;
            ViewBag.ActivityGroupType = ActivityGroupType.League;
            ViewBag.PagingModel = new PagingModel
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages,
            };

            return View(leagues);
        }

        [HttpGet]
        public ActionResult EditSetup(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            ActivityGroup league = null;

            if (id.IsGuid())
            {
                league = currentAccount.GetActivityGroup(id.ToGuid(), msgs);
            }
            else
            {
                league = currentAccount.GetDefaultNewLeague();
                league.RegistrationModeId = RegistrationMode.Register;
                league.StartDate = DateTime.UtcNow.AddWeeks(4).FirstDateOfWeek();
                league.EndDate = league.StartDate.Value.AddWeeks(8).LastDateOfWeek();
            }

            LoadEditSetupViewBag(league, msgs);
            ViewBag.ActiveMenu = "EditSetup";
            ViewBag.Messages = msgs;

            return View(league);
        }

        [HttpPost]
        public ActionResult EditSetup(string id, string name, string seatsavailable,
            string registrationperiod, string registrationopendate, string registrationopentime,
            string registrationclosedate, string registrationclosetime, string timezone, string form, string regmode,
            string isinviteonly, string startdate, string enddate, string cmd)
        {
            var msgs = new SystemMessages();
            var values = new Dictionary<string, object>
            {
                {"name", name},
                {"seatsavailable", seatsavailable},
                {"timezone", timezone},
                {"form", form},
                {"regmode", regmode},
                {"isinviteonly", isinviteonly},
                {"startdate", startdate},
                {"enddate", enddate},
            };

            Extensions.ValidateStartAndEndDate(startdate, enddate, msgs);

            var league = SaveLeague(id, values, msgs);

            if (msgs.HasNoErrors())
                switch (cmd)
                {
                    case "next":
                        return RedirectToAction("editrestrictions",
                            new {id = league.Uid, isnew = Request["isnew"], saved = 1});
                }

            LoadEditSetupViewBag(league, msgs);
            ViewBag.Messages = msgs;
            ViewBag.ActiveMenu = "EditSetup";
            return View(league);
        }

        private void LoadEditSetupViewBag(ActivityGroup league, SystemMessages msgs)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var activeForm = league?.GetForm(msgs) ?? currentAccount.GetDefaultStudentForm(msgs);

            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = currentAccount.GetForms(msgs).OrderBy(x => x.Name).ToList();
            ViewBag.ActiveForm = activeForm;
            ViewBag.IsFullScreen = GetIsFullScreen(league);
        }

        private bool GetIsFullScreen(ActivityGroup group)
        {
            ViewBag.FullScreenExitUrl =
                Url.Action($"index{(group.ActivityGroupTypeId == ActivityGroupType.League ? "league" : "")}", "group");

            return Request["isnew"] == "1" && !group.IsPublished;
        }

        [HttpGet]
        public ActionResult EditRestrictions(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var league = currentAccount.GetActivityGroup(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditRestrictions";
            ViewBag.IsFullScreen = GetIsFullScreen(league);
            return View(league);
        }

        [HttpPost]
        public ActionResult EditRestrictions(string id, string alloworganization, string agerestrictions,
            string agestart, string ageend, string graderestrictions, string mingrade, string maxgrade,
            string genderrestrictions, string requiresparentalconsent, string cmd)
        {
            var msgs = new SystemMessages();
            var gradeIds = new List<int>();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var grades = GradeLevel.Load(msgs: msgs);

            grades.ForEach(grade =>
            {
                if (Request["grade-" + grade.ID].IsNotEmpty())
                    gradeIds.Add(grade.ID.Value);
            });

            var values = new Dictionary<string, object>
            {
                {"alloworganization", alloworganization.IfBlank("0")},
                {"agestart", agerestrictions.IsNumber() ? agestart : "-1"},
                {"ageend", agerestrictions.IsNumber() ? ageend : "-1"},
                {"mingrade", graderestrictions.IsNumber() ? mingrade : ""},
                {"maxgrade", graderestrictions.IsNumber() ? maxgrade : ""},
                {"genderrestrictions", genderrestrictions},
                {"gradeIds", gradeIds.ToStrings().ToStringList()},
                {"requiresparentalconsent", requiresparentalconsent.IfBlank("0")},
            };

            var league = SaveLeague(id, values, msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;

            if (msgs.HasNoErrors())
                switch (cmd)
                {
                    case "prev":
                        return RedirectToAction("editsetup",
                            new {id = league.Uid, isnew = Request["isnew"], saved = 1});
                    case "next":
                        return RedirectToAction("editdetails",
                            new {id = league.Uid, isnew = Request["isnew"], saved = 1});
                }

            ViewBag.IsFullScreen = GetIsFullScreen(league);
            ViewBag.ActiveMenu = "EditRestrictions";
            return View(league);
        }

        [HttpGet]
        public ActionResult EditContact(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var league = currentAccount.GetActivityGroup(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Instructors = currentAccount.GetUsers(msgs: msgs);
            ViewBag.ActiveMenu = "EditContact";
            ViewBag.IsFullScreen = GetIsFullScreen(league);
            return View(league);
        }

        [HttpPost]
        public ActionResult EditContact(string id, string contactname, string contactphone, string contactemail,
            string cmd)
        {
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var values = new Dictionary<string, object>
            {
                {"contactname", contactname},
                {"contactphone", contactphone},
                {"contactemail", contactemail},
            };
            var league = SaveLeague(id, values, msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen(league);

            if (msgs.HasNoErrors())
                switch (cmd)
                {
                    case "prev":
                        return RedirectToAction("editdetails",
                            new {id = league.Uid, isnew = Request["isnew"], saved = 1});
                    case "next":
                        return RedirectToAction("editcost", new {id = league.Uid, isnew = Request["isnew"], saved = 1});
                }

            ViewBag.ActiveMenu = "EditCost";
            return View(league);
        }

        [HttpGet]
        public ActionResult EditDetails(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var league = currentAccount.GetActivityGroup(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
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

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (details.IsEmpty())
                msgs.Add($"{UILabel.ActivityGroupLabel(ActivityGroupType.League)} Details is required",
                    MessageType.UserError);

            var values = new Dictionary<string, object>
            {
                {"details", details},
                {"description", description},
            };

            var league = SaveLeague(id, values, msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen(league);

            if (msgs.HasNoErrors())
                switch (cmd)
                {
                    case "prev":
                        return RedirectToAction("editrestrictions",
                            new {id = league.Uid, isnew = Request["isnew"], saved = 1});
                    case "next":
                        return RedirectToAction("editcontact",
                            new {id = league.Uid, isnew = Request["isnew"], saved = 1});
                }

            ViewBag.ActiveMenu = "EditDetails";
            return View(league);
        }

        private void LoadEditCostViewBag(ActivityGroup league, SystemMessages msgs)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var canProcessPayments = currentAccount.CanProcessPayments(msgs);

            if (!canProcessPayments)
                league.AllowToPayLater = true;

            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.CanProcessPayments = canProcessPayments;
            ViewBag.PaymentPlan = league.GetPaymentPlan(msgs);
            ViewBag.ActiveMenu = "EditCost";
            ViewBag.IsFullScreen = GetIsFullScreen(league);
            ViewBag.Messages = msgs;
        }

        [HttpGet]
        public ActionResult EditCost(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var league = currentAccount.GetActivityGroup(id.ToGuid(), msgs);
            LoadEditCostViewBag(league, msgs);
            return View(league);
        }

        [HttpPost]
        public ActionResult EditCost(string id, string cost, string costdescription, string allowtopaylater, string cmd,
            string haspaymentplan, string paymentplancost, string paymentplanperiod, string paymentplanperiodnumber,
            string paymentplanpaymentstarttype)
        {
            var msgs = new SystemMessages();

            if (id.IsEmpty())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (cmd != "onlynext")
            {
                var values = new Dictionary<string, object>
                {
                    {"cost", cost},
                    {"costdescription", costdescription},
                    {"allowtopaylater", allowtopaylater.IfBlank("0")},
                    {"haspaymentplan", haspaymentplan.IfBlank("0")},
                    {"paymentplancost", paymentplancost},
                    {"paymentplanperiod", paymentplanperiod},
                    {"paymentplanperiodnumber", paymentplanperiodnumber},
                    {"paymentplanpaymentstarttype", paymentplanpaymentstarttype},
                };

                var league = SaveLeague(id, values, msgs);

                ViewBag.Messages = msgs;
                ViewBag.Account = SessionVariables.CurrentAccount;
                ViewBag.AccountPackage = SessionVariables.AccountPackage;
                ViewBag.IsFullScreen = GetIsFullScreen(league);

                if (msgs.HasNoErrors())
                    switch (cmd)
                    {
                        case "prev":
                            return RedirectToAction("editcontact",
                                new {id = league.Uid, isnew = Request["isnew"], saved = 1});
                        case "next":
                            return RedirectToAction("editconfirmation",
                                new {id = league.Uid, isnew = Request["isnew"], saved = 1});
                        case "publish":
                            return RedirectToAction("publish",
                                new {id = league.Uid, isnew = Request["isnew"], saved = 1});
                    }

                LoadEditCostViewBag(league, msgs);
                ViewBag.Messages = msgs;

                return View(league);
            }

            return RedirectToAction("editconfirmation", new {id, isnew = Request["isnew"], saved = 0});
        }

        [HttpGet]
        public ActionResult EditConfirmation(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var league = currentAccount.GetActivityGroup(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditConfirmation";
            ViewBag.IsFullScreen = GetIsFullScreen(league);
            return View(league);
        }

        private ActivityGroup SaveLeague(string id, Dictionary<string, object> values, SystemMessages msgs)
        {
            var league = ActivityGroupService.SaveLeague(
                SessionVariables.CurrentUser,
                SessionVariables.CurrentAccount,
                id,
                values,
                msgs
            );

            if (msgs.HasNoErrors())
                msgs.Add($"{UILabel.ActivityGroupLabel(league.ActivityGroupTypeId)} Saved", MessageType.Success);

            return league;
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditConfirmation(string id, string registrationthankyouemail,
            string applicationthankyouemail, string cmd)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var values = new Dictionary<string, object>
            {
                {"registrationthankyouemail", registrationthankyouemail},
                {"applicationthankyouemail", applicationthankyouemail},
            };

            var league = SaveLeague(id, values, msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen(league);

            if (msgs.HasNoErrors())
                switch (cmd)
                {
                    case "prev":
                        if (currentAccount.CanProcessPayments(msgs))
                            return RedirectToAction("editcost",
                                new {id = league.Uid, isnew = Request["isnew"], saved = 1});
                        return RedirectToAction("editcontact",
                            new {id = league.Uid, isnew = Request["isnew"], saved = 1});
                    case "publish":
                        return RedirectToAction("publish", new {id = league.Uid, isnew = Request["isnew"], saved = 1});
                    case "signupperiods":
                        return RedirectToAction("periods", new {id = league.Uid, saved = 1});
                }

            ViewBag.ActiveMenu = "EditConfirmation";
            return View(league);
        }

        [HttpPost]
        public ActionResult TeamQuickAdd(string id, string teamname, string seatsavailable)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var league = currentAccount.GetActivityGroup(id.ToGuid(), msgs);

            if (league.IsEmpty())
                msgs.Add("League is required", MessageType.UserError);

            Activity activity = null;

            if (msgs.HasNoErrors())
            {
                var otherActivities = currentAccount.LoadByActivitiesbyActivityGroupByAccountId(league.ID.Value, msgs);

                otherActivities.ForEach(oa =>
                {
                    if (oa.Name.IsSameAs(teamname))
                        msgs.Add("Team name already exists in this League, please use a different name",
                            MessageType.UserError);
                });
            }

            if (msgs.HasNoErrors())
            {
                activity = new Activity
                {
                    Name = teamname.MaxChars(50),
                    SeatsAvailable = seatsavailable.ToInteger(),
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = currentUser.ID,
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = currentUser.ID,
                    ActivityGroupId = league.ID,
                    AccountId = currentUser.ID,
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
                msgs.AddRange(activity.SetupTeam(currentAccount, currentUser.ID.Value));

            ViewBag.Messages = msgs;
            return RedirectToAction("activities", new {id = league.Uid, saved = 1});
        }
    }
}
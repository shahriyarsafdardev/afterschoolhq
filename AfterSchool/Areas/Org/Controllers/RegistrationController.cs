using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AfterSchool.Areas.Org.Assemblers.Registration;
using AfterSchool.Models;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Hangfire;
using Utilities = BasicLibrary.Utilities;

namespace AfterSchool.Areas.Org.Controllers
{
    public class RegistrationController : BaseLoggedInController
    {
        public RegistrationController() : base(FormConstants.OrganizationAccountTypes)
        {
        }

        // GET: Org/Registrant
        public ActionResult Index(string activity, string group, string pn, string rpp, string q, string emailed,
            string question, string regstatus, string at, string volunteers)
        {
            var msgs = new SystemMessages();
            var pageNumber = 1;
            var recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var getVolunteers = volunteers.IsBoolean() && volunteers.ToBoolean();
            var currentAccount = SessionVariables.CurrentAccount;

            var currentUser = SessionVariables.CurrentUser;
            var start = recordsPerPage * ((int?) pageNumber - 1);
            var activityIds = new List<int>();
            var registrationAssignmentTypes = new List<RegistrationAssignmentType>();

            if (activity.IsNotEmpty())
            {
                activityIds = activity.Split(',').Select(x => x.ToInteger()).ToList();
            }

            if (at.IsNumber())
                registrationAssignmentTypes.Add((RegistrationAssignmentType) at.ToInteger());

            var registrationStatus = int.TryParse(regstatus, out var parsed)
                ? (RegistrationStatus) parsed
                : RegistrationStatus.NotSet;

            var registrations1 = currentAccount.SearchRegistrations(q, null, activityIds,
                ParticipationObjectType.Activity, registrationStatus, registrationAssignmentTypes, start, recordsPerPage, getVolunteers, msgs);

            var totalCount1 = currentAccount.SearchRegistrationCount(q, null, activityIds,
                ParticipationObjectType.Activity, registrationStatus, registrationAssignmentTypes, getVolunteers, msgs);

            var activities = currentAccount
                .GetActivities(UserType.Administrator, currentUser, null, null, false, false, null, msgs: msgs)
                .OrderBy(x => x.Name).ToList();

            if (getVolunteers)
            {
                activities = activities.Where(a => a.ActivityTypeId == ActivityType.VolunteerOpportunity).ToList();
            }
            else
            {
                activities = activities.Where(a => a.ActivityTypeId != ActivityType.VolunteerOpportunity).ToList();
            }

            var groups = getVolunteers ? new List<ActivityGroup>() : currentAccount.GetActivityGroups(msgs);

            var (registrations, totalCount) = ((List<Registration>, int)) (registrations: registrations1, totalCount: totalCount1);

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            if (emailed.IsBoolean() && emailed.ToBoolean())
                msgs.Add("An email will be sent shortly", MessageType.Success);

            var isStudentCRMEnabled = SessionVariables.CurrentAccount.HasFeature(FeatureType.StudentCRM);
            var studentCRMPackage = PackageService.GetPackageWithFeatureLowestPrice(FeatureType.StudentCRM, SessionVariables.AccountPackage);
            var pagingModel = new PagingModel
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages,
            };

            var result = new RegistrationIndexAssembler().AssembleRegistrationIndexResult(isStudentCRMEnabled, studentCRMPackage, registrations, msgs,
                pagingModel, false, activities, groups, new List<FormQuestion>(), new List<FormQuestionGroup>(), false, currentAccount);
            return View(result);
        }

        [HttpPost]
        public ActionResult EmailReport(string id, string q, string activity, string group, string emails,
            string filename, string reporttype, string question, string regstatus, string at, string volunteers)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;

            RegistrationAssignmentType? rat = null;
            var registrationStatus = RegistrationStatus.NotSet;
            var questionId = new int?();
            var activityIds = new List<int>();
            var groupIds = new List<int>();

            if (activity.IsNotEmpty())
                activityIds = activity.Split(',').Select(x => x.ToInteger()).ToList();

            if (group.IsNotEmpty())
            {
                groupIds = group.Split(',').Select(x => x.ToInteger()).ToList();
                var groupList = currentAccount.GetActivityGroups(groupIds, msgs);
                groupList.ForEach(x =>
                {
                    activityIds.AddRange(x.GetActivities(null, false, false, null, null, msgs: msgs)
                        .Select(y => y.ID.Value));
                });
            }

            if (at.IsNumber())
                rat = (RegistrationAssignmentType) at.ToInteger();

            if (question.IsNumber())
                questionId = question.ToInteger();

            if (regstatus.IsNumber())
                registrationStatus = (RegistrationStatus) regstatus.ToInteger();

            var getVolunteers = volunteers.IsBoolean() && volunteers.ToBoolean();
            BackgroundJob.Enqueue(() =>
                RegistrationService.EmailCsvReport(
                    currentAccount.ID.Value,
                    q,
                    questionId,
                    activityIds,
                    ParticipationObjectType.Activity,
                    filename,
                    (RegistrationReportType) reporttype.ToInteger(),
                    rat.HasValue ? rat.Value.AsList() : null,
                    registrationStatus,
                    UILabel.ActivityLabel(ActivityType.Activity, false),
                    emails,
                    null,
                    null,
                    getVolunteers,
                    null,
                    null)
            );

            ViewBag.Messages = msgs;

            return RedirectToAction("index", new {volunteers = volunteers, q, activity, emailed = "1", question, group, at, regstatus});
        }

        [HttpGet]
        public ActionResult Edit(string id, string r, string returnUrl)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var registration = currentAccount.GetRegistration(id.ToGuid(), msgs);

            LoadRegistrationViewBag(registration, r, msgs);

            if (registration.IsEmpty())
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            ViewBag.ReturnURl = returnUrl;
            return View(registration);
        }

        private List<FormQuestionViewModel> UpdateLoadQuestions(Registration registration,
            NameValueCollection formAnswers, SystemMessages msgs)
        {
            var answers = registration.GetAnswers(msgs);
            var questionIds = answers.Select(answer => answer.FormQuestionId.Value).ToList();
            var questions = FormQuestion.LoadList(questionIds, msgs);

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

            List<FormQuestionViewModel> qvms = null;
            questions = questions.OrderBy(question => question.DisplayOrder).ToList();

            if (formAnswers.IsNotEmpty())
                qvms = questions.Select(question => new FormQuestionViewModel(question, formAnswers)).ToList();
            else
                qvms = questions.Select(question => new FormQuestionViewModel(
                        question,
                        answers.Find(x => x.FormQuestionId == question.ID)))
                    .ToList();

            ViewBag.QuestionViewModels = qvms;
            ViewBag.Messages = msgs;

            return qvms;
        }

        [HttpGet]
        public ActionResult Update(string id, string returnUrl)
        {
            var currentAccount = SessionVariables.CurrentAccount;

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var msgs = new SystemMessages();
            var registration = currentAccount.GetRegistration(id.ToGuid(), msgs);

            LoadRegistrationViewBag(registration, id, msgs);

            UpdateLoadQuestions(registration, null, msgs);

            ViewBag.ReturnURl = returnUrl;

            return View(registration);
        }

        [HttpPost]
        public ActionResult Update(string id, string nothing, string returnUrl)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var msgs = new SystemMessages();
            var registration = currentAccount.GetRegistration(id.ToGuid(), msgs);
            var submittedRecord = registration.GetRegistrant(msgs);
            var qvms = UpdateLoadQuestions(registration, Request.Params, msgs);

            // validation
            qvms.ForEach(qvm =>
            {
                if (qvm.Question.IsRequired && qvm.Value.IsEmpty())
                    msgs.Add($"{qvm.Question.Text} is required", MessageType.UserError);

                switch (qvm.Question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantMobilePhone:
                    case QuestionDataType.RegistrantHomePhone:
                    case QuestionDataType.RegistrantWorkPhone:
                        if (qvm.Value.IsNotEmpty() && !qvm.Value.IsRegExMatch(Utilities.PhoneRegEx))
                            msgs.Add($"{qvm.Question.Text} must be a valid phone number", MessageType.UserError);
                        break;
                    case QuestionDataType.RegistrantEmail:
                    case QuestionDataType.Parent1Email:
                    case QuestionDataType.Parent2Email:
                        if (qvm.Value.IsNotEmpty() && !qvm.Value.IsRegExMatch(Utilities.EmailRegEx))
                            msgs.Add($"{qvm.Question.Text} must be a valid email", MessageType.UserError);
                        break;
                }
            });

            // save answers
            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        qvms.ForEach(qvm =>
                        {
                            if (qvm.Question.QuestionTypeId == QuestionType.FileUpload)
                            {
                                if (Request.Files[qvm.Name].IsNotEmpty() && Request.Files[qvm.Name].ContentLength > 0)
                                {
                                    var currentFileSizeInBytes = Request.Files[qvm.Name].ContentLength;
                                    //5 MB = 5242880 Bytes
                                    //File uploaded here should be less than of 5MB
                                    if (currentFileSizeInBytes <= 5242880)
                                        submittedRecord.AddOrUpdateAnswer(
                                            file: Request.Files[qvm.Name].InputStream,
                                            filename: Request.Files[qvm.Name].FileName,
                                            question: qvm.Question,
                                            registrationId: registration.ID,
                                            loggedInUser: currentUser,
                                            msgs: amsgs,
                                            dc: adc);
                                    else
                                        msgs.Add("Upload a file less than or equal to 5 megabytes",
                                            MessageType.UserError);
                                }
                            }
                            else
                                submittedRecord.AddOrUpdateAnswer(
                                    answerText: qvm.Value.IfEmpty("").Trim(),
                                    question: qvm.Question,
                                    registrationId: registration.ID,
                                    loggedInUser: currentUser,
                                    msgs: amsgs,
                                    dc: adc);
                        });

                        msgs.AddRange(amsgs);
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("edit", new {id = registration.Uid, returnUrl});

            ViewBag.Messages = msgs;
            ViewBag.ReturnURl = returnUrl;

            return View(registration);
        }

        [HttpGet]
        public ActionResult DownloadFile(string id, string r, string file, string returnUrl)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var msgs = new SystemMessages();
            var registration = currentAccount.GetRegistration(id.ToGuid(), msgs);
            LoadRegistrationViewBag(registration, r, msgs);

            var answers = (List<FormQuestionAnswer>) ViewBag.Answers;
            var answer = answers.Find(item => item.ID.ToString() == file);

            if (answer.IsNotEmpty())
            {
                var link = answer.GetFileDownloadLink();

                if (link.IsNotEmpty())
                    return Redirect(answer.GetFileDownloadLink());
            }

            ViewBag.Messages = msgs;
            return RedirectToAction("Edit", new {id, r, returnUrl});
        }

        [HttpPost]
        public ActionResult Merge(string id, string r, string returnUrl, string mergeintoreg)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            if (r.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var msgs = new SystemMessages();
            var registration = currentAccount.GetRegistration(r.ToGuid(), msgs);

            if (mergeintoreg.IsGuid())
            {
                var mergeInto = currentAccount.GetRegistration(mergeintoreg.ToGuid(), msgs);
                mergeInto.MergeInto(registration, currentUser.ID.Value, msgs);
                mergeInto = mergeInto.Reload(msgs);
                LoadRegistrationViewBag(mergeInto, mergeintoreg, msgs);

                msgs.Add("Your registrations were successfully merged", MessageType.Success);
            }
            else
                msgs.Add("Invalid registration", MessageType.UserError);

            ViewBag.Messages = msgs;

            return View("edit", registration);
        }

        [HttpPost]
        public ActionResult MoveSeat(string id, string ra, string to, string r)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            if (id.IsNotGuid() || r.IsNotGuid() || !ra.IsInteger())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var registration = currentAccount.GetRegistration(id.ToGuid(), msgs);
            var selectedRegistration = currentAccount.GetRegistration(r.ToGuid(), msgs);
            var registrantActivity = RegistrantActivity.Load(ra.ToInteger(), msgs);
            var ratTo = RegistrationAssignmentType.NotSet;

            if (to.IsNotEmpty())
                ratTo = (RegistrationAssignmentType) to.ToInteger();

            if (ratTo != RegistrationAssignmentType.NotSet)
                registrantActivity.ChangeSeatAssignment(ratTo, currentUser, msgs);

            if (msgs.HasNoErrors())
            {
                selectedRegistration.WasUpdated(msgs);

                var participationObject = registrantActivity.GetParticipationObject(msgs);
                var registrant = registrantActivity.GetRegistrant(msgs);

                // LOG: Someone Updated RegistrantActivity {registrant.Name} assignment type to {ratTo} for Activty ActivityName
                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Updated,
                    objectId: registrantActivity.ID,
                    objectType: ObjectType.RegistrantActivity,
                    objectText: $"{registrant.GetFirstNameLastInitial()}'s assignment type to {ratTo}",
                    indirectObjectId: participationObject.ObjectID,
                    indirectObjectType: participationObject.ObjectType.ToObjectType(),
                    indirectObjectText: participationObject.Name);

                msgs.Add(string.Format("{0} has been moved to {1} for {2}",
                        registrant.Name, ratTo.ToString().SplitByUpperCase(),
                        participationObject.Name),
                    MessageType.Success);
            }

            LoadRegistrationViewBag(registration, r, msgs);

            if (registration.IsEmpty())
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            return View("edit", registration);
        }

        [HttpGet]
        public ActionResult RemoveActivity(string id, string ra, string r)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;

            if (id.IsNotGuid() || r.IsNotGuid() || !ra.IsInteger())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var registration = currentAccount.GetRegistration(id.ToGuid(), msgs);
            var selectedRegistration = currentAccount.GetRegistration(r.ToGuid(), msgs);
            var registrantActivity = RegistrantActivity.Load(ra.ToInteger(), msgs);
            var activity = registrantActivity.GetActivity(msgs);

            selectedRegistration.RemoveParticipation(registrantActivity.ParticipationObjectId.Value,
                registrantActivity.ParticipationObjectTypeId, msgs);

            if (msgs.HasNoErrors())
                msgs.Add($"Removed {UILabel.ActivityLabel(activity.ActivityTypeId)}: {activity.Name}",
                    MessageType.Success);

            LoadRegistrationViewBag(registration, r, msgs);

            if (registration.IsEmpty())
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            return View("edit", registration);
        }

        [HttpPost]
        public ActionResult AddActivity(string id, string activity, string activityassignment, string r)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            if (id.IsNotGuid() || r.IsNotGuid() || activity.IsNotGuid() || !activityassignment.IsInteger())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var registration = currentAccount.GetRegistration(id.ToGuid(), msgs);
            var selectedRegistration = currentAccount.GetRegistration(r.ToGuid(), msgs);
            var act = currentAccount.GetActivity(activity.ToGuid(), msgs);
            var registrant = selectedRegistration.GetRegistrant(msgs);
            var regperiod = act.GetActiveRegistrationPeriod(msgs);
            var assignmentType = (RegistrationAssignmentType) activityassignment.ToInteger();
            var choiceNumber = 1;
            var registrationPeriodId = new int?();
            if (regperiod.IsNotEmpty())
                registrationPeriodId = regperiod.ID;

            registrant.AddOrUpdateParticipationObject(
                selectedRegistration.ID.Value,
                act.ID.Value,
                ParticipationObjectType.Activity,
                assignmentType,
                choiceNumber,
                registrationPeriodId,
                act.ActivityGroupId,
                currentUser.ID.Value,
                msgs);

            if (msgs.HasNoErrors())
            {
                selectedRegistration.WasUpdated(msgs);

                // notify parent
                var masterRegistrant = registrant.GetMasterRegistrant(msgs);

                if (masterRegistrant.IsNotEmpty())
                    selectedRegistration.SendConfirmation(
                        new CheckoutObject
                        {
                            BillingAccount = currentAccount,
                            PayingAccount = masterRegistrant.GetAccount(msgs),
                            MasterRegistrant = masterRegistrant,
                            NumberOfSeats = 1,
                            ParticipationObject = act.GetParticipationObject(msgs),
                            Registration = selectedRegistration,
                        },
                        null, currentUser, true, null, msgs);

                // LOG:  CurrentWebUser added Registrant RegistrantName to Activity ActivityName
                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Added,
                    objectId: registrant.ID.Value,
                    objectType: ObjectType.Registrant,
                    objectText: registrant.GetFirstNameLastInitial(),
                    indirectObjectId: act.ID.Value,
                    indirectObjectType: ObjectType.Activity,
                    indirectObjectText: act.Name,
                    msgs: msgs);

                msgs.Add($"{act.Name} has been added for {registrant.Name}", MessageType.Success);
            }

            LoadRegistrationViewBag(registration, r, msgs);

            if (registration.IsEmpty())
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            return View("edit", registration);
        }

        [HttpGet]
        public ActionResult GoToEdit(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var registration = currentAccount.GetRegistration(id.ToGuid(), msgs);
            LoadRegistrationViewBag(registration, msgs: msgs);
            SessionVariables.Cart = null;
            return Redirect($"/reg/edit/{id}");
        }

        [HttpGet]
        public ActionResult Notify(string id, string r)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;

            if (id.IsNotGuid() || r.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var registration = currentAccount.GetRegistration(id.ToGuid(), msgs);

            // TODO: Need to figure out
            // var selectedRegistration = Registration.LoadByUidByAccountId(r.ToGuid(), SessionVariables.CurrentAccount.ID.Value, msgs);
            // BackgroundJob.Enqueue(() => Registration.SendConfirmation(selectedRegistration.ID.Value, true, null));

            msgs.Add("Not Implemented", MessageType.UserError);
            // msgs.Add(string.Format("{0} will be notified shortly", selectedRegistration.ToRegistrant().Name), MessageType.Success);

            LoadRegistrationViewBag(registration, r, msgs);

            if (registration.IsEmpty())
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            return View("Edit", registration);
        }

        [HttpGet]
        public ActionResult Delete(string id, string r, string returnurl)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;

            if (id.IsNotGuid() || r.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var registration = currentAccount.GetRegistration(id.ToGuid(), msgs);
            var selectedRegistration = currentAccount.GetRegistration(r.ToGuid(), msgs);
            // Get the uid of the activity before deleting the registration
            var activityUid = registration.GetFirstParticipationObject(msgs).Uid;

            selectedRegistration.CancelAll(msgs);

            if (msgs.HasNoErrors())
            {
                msgs.Add("Registration Deleted", MessageType.Success);
                TempData["msgs"] = msgs;

                if (returnurl.IsNotEmpty() && returnurl.Contains("registrant") && activityUid.IsNotEmpty())
                    return RedirectToAction("registrants", "activity", new {id = activityUid, atype = ""});

                if (returnurl.IsNotEmpty())
                    return Redirect(returnurl);

                return RedirectToAction("index");
            }

            LoadRegistrationViewBag(registration, r, msgs);

            if (registration.IsEmpty())
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            return View("Edit", registration);
        }

        [HttpGet]
        public ActionResult RegistrationEdit(string id)
        {
            return View();
        }

        private void LoadRegistrationViewBag(Registration registration, string selectedRegistration = null,
            SystemMessages msgs = null)
        {
            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            var registrant = registration.GetRegistrant(msgs);

            if (registration.AccountId != currentAccount.ID)
                registration = null;

            var allRegistrations = registration.GetRelatedRegistrations(msgs);
            allRegistrations.Add(registration);
            allRegistrations = allRegistrations.OrderByDescending(x => x.ID).ToList();

            if (selectedRegistration.IsGuid())
                registration = allRegistrations.First(x => x.Uid == selectedRegistration.ToGuid());

            if (registration.IsEmpty())
                return;

            var ras = registration.GetRegistrantActivities(msgs);
            var rasActivityTypes = ras.Where(y => y.ParticipationObjectTypeId == ParticipationObjectType.Activity);
            ViewBag.Registrant = registrant;
            ViewBag.Account = currentAccount;
            ViewBag.Answers = registration.GetAnswers(msgs);
            ViewBag.RegistrantActivities = ras;
            ViewBag.Registrations = allRegistrations;
            ViewBag.SelectedRegistration = registration;

            if (registration.IsNotEmpty())
                ViewBag.CanMergeWith = allRegistrations.Where(x =>
                    x.RegistrationPeriodId == registration.RegistrationPeriodId && x.ID != registration.ID).ToList();

            List<Activity> activities = null;

            if (registration.ActivityGroupId.HasValue)
                activities = registration.GetActivityGroup(msgs).GetActivities(
                    activityType: null,
                    isArchived: false,
                    isDeleted: false,
                    start: null,
                    length: null,
                    forceReload: false,
                    msgs: msgs);
            else
                activities = currentAccount.GetActivities(
                    userType: UserType.Administrator,
                    user: currentUser,
                    null,
                    activityType: null,
                    isArchived: false,
                    isDeleted: false,
                    ungroupedOnly: null,
                    start: null,
                    length: null,
                    msgs: msgs);

            ViewBag.AvailableActivities = activities
                .Where(x => x.ID.Value.NotIn(rasActivityTypes.Select(y => y.ParticipationObjectId.Value)))
                .OrderBy(x => x.Name)
                .ToList();

            ViewBag.Messages = msgs;

            if (Request["returnurl"].IsNotEmpty())
                ViewBag.ListUrl = Request["returnurl"];
            else
                ViewBag.ListUrl = Url.Action("index", "registration");
        }

        private (List<Registration>, int) LoadSearch(string activity, string group, string q,
            string registrationStatusNumber, string at, bool isReadOnly, bool getVolunteers, SystemMessages msgs = null,
            int? pageNumber = null, int? recordsPerPage = null)
        {
            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var selectedGroups = new List<ActivityGroup>();
            var selectedActivities = new List<Activity>();
            var start = recordsPerPage * (pageNumber - 1);
            var activityIds = new List<int>();
            var registrationAssignmentTypes = new List<RegistrationAssignmentType>();

            if (activity.IsNotEmpty())
            {
                activityIds = activity.Split(',').Select(x => x.ToInteger()).ToList();
                selectedActivities = currentAccount.GetActivities(activityIds, msgs);
            }

            if (group.IsNotEmpty())
            {
                var groupIds = group.Split(',').Select(x => x.ToInteger()).ToList();
                selectedGroups = currentAccount.GetActivityGroups(groupIds, msgs);
                selectedGroups.ForEach(x => { activityIds.AddRange(x.GetActivities(null, false, false, null, null).Select(y => y.ID.Value)); });
            }

            if (at.IsNumber())
                registrationAssignmentTypes.Add((RegistrationAssignmentType) at.ToInteger());

            var registrationStatus = int.TryParse(registrationStatusNumber, out var parsed)
                ? (RegistrationStatus) parsed
                : RegistrationStatus.NotSet;

            var registrations = currentAccount.SearchRegistrations(q, null, activityIds,
                ParticipationObjectType.Activity, registrationStatus, registrationAssignmentTypes, start, recordsPerPage, getVolunteers, msgs);

            var totalCount = currentAccount.SearchRegistrationCount(q, null, activityIds,
                ParticipationObjectType.Activity, registrationStatus, registrationAssignmentTypes, getVolunteers, msgs);

            var activities = currentAccount
                .GetActivities(UserType.Administrator, currentUser, null, null, false, false, null, msgs: msgs)
                .OrderBy(x => x.Name).ToList();

            activities = getVolunteers
                ? activities.Where(a => a.ActivityTypeId == ActivityType.VolunteerOpportunity).ToList()
                : activities.Where(a => a.ActivityTypeId != ActivityType.VolunteerOpportunity).ToList();

            var groups = currentAccount.GetActivityGroups(msgs);

            ViewBag.Activities = activities;
            ViewBag.Groups = groups;
            ViewBag.SelectedActivities = selectedActivities;
            ViewBag.SelectedGroups = selectedGroups;
            ViewBag.SearchBarReadOnly = isReadOnly;

            return (registrations, totalCount);
        }

        [HttpGet]
        public ActionResult Messages(string pn, string rpp, string volunteers)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            var activityMessages = currentAccount.GetActivityMessages(start, length, msgs);

            var totalCount = currentAccount.GetActivityMessageCount(msgs);
            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();
            var getVolunteers = volunteers.IsBoolean() && volunteers.ToBoolean();
            LoadSearch(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, false, getVolunteers, msgs);

            ViewBag.Messages = msgs;
            ViewBag.PagingModel = new PagingModel
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages,
            };
            return View(activityMessages);
        }

        [HttpGet]
        public ActionResult MessageDetail(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var am = currentAccount.GetActivityMessage(id.ToGuid(), msgs);
            ViewBag.Registrants = am.GetActivityMessageRegistrants(msgs);
            ViewBag.Messages = msgs;
            return View(am);
        }

        [HttpGet]
        public ActionResult MessageAdd(string activity, string group, string pn, string rpp, string q, string emailed,
            string question, string regstatus, string at, string volunteers)
        {
            var msgs = new SystemMessages();
            var getVolunteers = volunteers.IsBoolean() && volunteers.ToBoolean();
            var (registrations, totalCount) = LoadSearch(activity, group, q, regstatus, at, true, getVolunteers, msgs);

            ViewBag.Registrations = registrations;
            ViewBag.TotalCount = totalCount;
            ViewBag.Messages = msgs;

            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult MessageAdd(string id, string subject, string body, string sendtostaff, string activity,
            string group, string pn, string rpp, string q, string emailed, string question, string regstatus, string at, string volunteers)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            if (subject.IsEmpty())
                msgs.Add("Message Reason is required", MessageType.UserError);
            if (body.IsEmpty())
                msgs.Add("Message Body is required", MessageType.UserError);

            ActivityMessage am = null;
            var getVolunteers = volunteers.IsBoolean() && volunteers.ToBoolean();
            var (registrations, totalCount) = LoadSearch(activity, group, q, regstatus, at, true, getVolunteers, msgs);

            if (msgs.HasNoErrors())
            {
                var registrantIds = registrations.Select(registration => registration.RegistrantId.Value).ToList();

                am = currentAccount.AddMessage(
                    subject,
                    body,
                    registrantIds,
                    sendtostaff.IsBoolean(),
                    currentUser.ID.Value,
                    msgs);

                BackgroundJob.Enqueue(() => ActivityMessageService.Send(am.ID.Value, currentUser.ID.Value, null, null));
            }

            ViewBag.Registrations = registrations;
            ViewBag.TotalCount = totalCount;

            if (msgs.HasNoErrors())
            {
                msgs.Add("Your message will been sent to the selected registrants", MessageType.Success);
                ViewBag.Registrants = am.GetActivityMessageRegistrants(msgs);
                ViewBag.Messages = msgs;
                ViewBag.Volunteers = getVolunteers;

                return View("messagedetail", am.Reload(msgs));
            }

            ViewBag.Messages = msgs;
            return View(activity);
        }

        [HttpGet]
        public ActionResult MessagesResend(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            if (id.IsNotGuid())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var am = currentAccount.GetActivityMessage(id.ToGuid(), msgs);

            BackgroundJob.Enqueue(() => ActivityMessageService.Send(am.ID.Value, currentUser.ID.Value, null, null));

            ViewBag.Registrants = am.GetActivityMessageRegistrants(msgs);

            if (msgs.HasNoErrors())
            {
                msgs.Add("Your message will been sent to the selected registrants", MessageType.Success);
                ViewBag.Messages = msgs;
                return View("messagedetail", am);
            }

            ViewBag.Messages = msgs;
            return View("messagedetail", am);
        }

        [HttpGet]
        public ActionResult Surveys(string id, string r)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var registration = currentAccount.GetRegistration(id.ToGuid(), msgs);
            var registrant = Registrant.Load(registration.RegistrantId.Value);

            LoadRegistrationViewBag(registration, r, msgs);

            if (registration.IsEmpty())
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            ViewBag.SurveyResponses = SurveyResponse.LoadByRegistrantId(registrant.ID.Value, msgs)
                .OrderByDescending(a => a.CreateDate).ToList();

            ViewBag.Messages = msgs;
            return View(registration);
        }
    }
}
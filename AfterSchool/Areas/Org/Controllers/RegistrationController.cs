using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using Hangfire;
using System.Collections.Specialized;
using BasicLibrary.DataAccess;

namespace AfterSchool.Areas.Org.Controllers
{
    public class RegistrationController : BaseLoggedInController
    {
        public RegistrationController() : base(AccountService.ProviderAccountTypes.ToList())
        {

        }

        // GET: Org/Registrant
        public ActionResult Index(string activity, string group, string pn, string rpp, string q, string emailed, string question, string regstatus, string at)
        {
            var msgs = new SystemMessages();
            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var (registrations, totalCount) = LoadSearch(activity, group, pageNumber, recordsPerPage, q, emailed, question, regstatus, at, false);
            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            if (emailed.IsBoolean() && emailed.ToBoolean())
                msgs.Add("An email will be sent shortly", MessageType.Success);
            
            ViewBag.Messages = msgs;
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };
            return View(registrations);
        }

        [HttpPost]
        public ActionResult EmailReport(string id, string q, string activity, string group, string emails, string filename, string reporttype, string question, string regstatus, string at)
        {
            var msgs = new SystemMessages();
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
                var groupList = ActivityGroup.LoadList(groupIds, msgs);
                groupList.ForEach(x => {
                    activityIds.AddRange(x.GetActivities(null, false, false, null, null).Select(y => y.ID.Value));
                });
            }

            if (at.IsNumber())
                rat = (RegistrationAssignmentType)at.ToInteger();

            if (question.IsNumber())
                questionId = question.ToInteger();
            
            if (regstatus.IsNumber())
                registrationStatus = (RegistrationStatus)regstatus.ToInteger();

            var account = SessionVariables.CurrentAccount;

            BackgroundJob.Enqueue(() =>
                RegistrationService.EmailCSVReport(
                    account.ID.Value,
                    q,
                    questionId,
                    activityIds,
                    ParticipationObjectType.Activity,
                    filename,
                    (RegistrationReportType)reporttype.ToInteger(),
                    rat.HasValue ? rat.Value.AsList() : null,
                    registrationStatus,
                    UILabel.ActivityLabel(ActivityType.Activity, false),
                    emails,
                    null,
                    null)
            );

            ViewBag.Messages = msgs;
            
            return RedirectToAction("index", new { q, activity, emailed = "1", question, group, at, regstatus });
        }

        [HttpGet]
        public ActionResult Edit(string id, string r, string returnUrl)
        {
            var registration = Registration.LoadByUid(id.ToGuid());
            LoadRegistrationViewBag(registration, r);

            return View(registration);
        }

        private List<FormQuestionViewModel> UpdateLoadQuestions(Registration registration, NameValueCollection formAnswers, SystemMessages msgs)
        {
            var answers = registration.GetAnswers(msgs);
            var questionIds = answers.Select(answer => answer.FormQuestionId.Value).ToList();
            var questions = FormQuestion.LoadList(questionIds, msgs);

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

            return qvms;
        }

        [HttpGet]
        public ActionResult Update(string id)
        {
            var msgs = new SystemMessages();
            var registration = Registration.LoadByUid(id.ToGuid(), msgs);

            UpdateLoadQuestions(registration, null, msgs);

            ViewBag.Messages = msgs;

            return View(registration);
        }

        [HttpPost]
        public ActionResult Update(string id, string nothing)
        {
            var msgs = new SystemMessages();
            var registration = Registration.LoadByUid(id.ToGuid(), msgs);
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
                        if (qvm.Value.IsNotEmpty() && !qvm.Value.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                            msgs.Add($"{qvm.Question.Text} must be a valid phone number", MessageType.UserError);
                        break;
                    case QuestionDataType.RegistrantEmail:
                    case QuestionDataType.Parent1Email:
                    case QuestionDataType.Parent2Email:
                        if (qvm.Value.IsNotEmpty() && !qvm.Value.IsRegExMatch(BasicLibrary.Utilities.EmailRegEx))
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
                                            loggedInUser: SessionVariables.CurrentUser,
                                            msgs: amsgs,
                                            dc: adc);
                                    else
                                        msgs.Add("Upload a file less than or equal to 5 megabytes", MessageType.UserError);
                                }
                            }
                            else
                                submittedRecord.AddOrUpdateAnswer(
                                    answerText: qvm.Value.IfEmpty("").Trim(),
                                    question: qvm.Question,
                                    registrationId: registration.ID,
                                    loggedInUser: SessionVariables.CurrentUser,
                                    msgs: amsgs,
                                    dc: adc);
                        });
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("edit", new { id = registration.Uid });

            ViewBag.Messages = msgs;

            return View(registration);
        }

        [HttpGet]
        public ActionResult DownloadFile(string id, string r, string file, string returnUrl)
        {
            var registration = Registration.LoadByUid(id.ToGuid());
            LoadRegistrationViewBag(registration, r);

            var answers = (List<FormQuestionAnswer>)ViewBag.Answers;
            var answer = answers.Find(item => item.ID.ToString() == file);

            if (answer.IsNotEmpty())
            {
                var link = answer.GetFileDownloadLink();

                if (link.IsNotEmpty())
                    return Redirect(answer.GetFileDownloadLink());
            }

            return RedirectToAction("Edit", new { id, r, returnUrl });
        }

        [HttpPost]
        public ActionResult Merge(string id, string r, string returnUrl, string mergeintoreg)
        {
            var msgs = new SystemMessages();
            var registration = Registration.LoadByUid(r.ToGuid()); // load selected registration

            if (mergeintoreg.IsGuid())
            {
                var mergeInto = Registration.LoadByUid(mergeintoreg.ToGuid());
                mergeInto.MergeInto(registration, SessionVariables.CurrentUser.ID.Value, msgs);
                mergeInto = mergeInto.Reload();
                LoadRegistrationViewBag(mergeInto, mergeintoreg);

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
            var registration = Registration.LoadByUid(id.ToGuid());
            var selectedRegistration = Registration.LoadByUid(r.ToGuid());           

            var registrantActivity = RegistrantActivity.Load(ra.ToInteger());
            var ratTo = RegistrationAssignmentType.NotSet;

            if (to.IsNotEmpty())
                ratTo = (RegistrationAssignmentType)to.ToInteger();

            if(ratTo != RegistrationAssignmentType.NotSet)
                registrantActivity.ChangeSeatAssignment(ratTo, SessionVariables.CurrentUser, msgs);

            if (msgs.HasNoErrors())
            {
                selectedRegistration.WasUpdated(msgs);

                // LOG: Someone Updated RegistrantActivity {registrant.Name} assignment type to {ratTo} for Activty ActivityName
                WebSessionUtilities.LogActivity(
                    webSessionId: SessionVariables.WebSession.ID.Value,
                    actor: SessionVariables.CurrentUser,
                    actionType: WebSessionActionType.Updated,
                    objectId: registrantActivity.ID,
                    objectType: ObjectType.RegistrantActivity,
                    objectText: $"{registration.GetRegistrant().GetFirstNameLastInitial()}'s assignment type to {ratTo}",
                    indirectObjectId: registrantActivity.GetActivityId(),
                    indirectObjectType: ObjectType.Activity,
                    indirectObjectText: registrantActivity.GetActivity().Name);

                msgs.Add(string.Format("{0} has been moved to {1} for {2}", selectedRegistration.ToRegistrant().Name, ratTo.ToString().SplitByUpperCase(), registrantActivity.GetParticipationObject(msgs).Name), MessageType.Success);
            }

            LoadRegistrationViewBag(registration, r);
            ViewBag.Messages = msgs;

            
            return View("edit", registration);
        }

        [HttpGet]
        public ActionResult RemoveActivity(string id, string ra, string r)
        {
            var msgs = new SystemMessages();
            var registration = Registration.LoadByUid(id.ToGuid());
            var selectedRegistration = Registration.LoadByUid(r.ToGuid());
            var registrantActivity = RegistrantActivity.Load(ra.ToInteger(), msgs);
            var activity = registrantActivity.GetActivity(msgs);

            selectedRegistration.RemoveParticipation(registrantActivity.ParticipationObjectId.Value, registrantActivity.ParticipationObjectTypeId, msgs);

            if (msgs.HasNoErrors())
                msgs.Add($"Removed {UILabel.ActivityLabel(activity.ActivityTypeId)}: {activity.Name}", MessageType.Success);

            LoadRegistrationViewBag(registration, r);
            ViewBag.Messages = msgs;

            return View("edit", registration);
        }

        [HttpPost]
        public ActionResult AddActivity(string id, string activity, string activityassignment, string r)
        {
            var msgs = new SystemMessages();
            var registration = Registration.LoadByUid(id.ToGuid(), msgs);
            var selectedRegistration = Registration.LoadByUid(r.ToGuid(), msgs);
            var act = Activity.LoadByUid(activity.ToGuid(), msgs);
            var registrant = selectedRegistration.GetRegistrant(msgs);
            var regperiod = act.GetActiveRegistrationPeriod(msgs);  
            var assignmentType = (RegistrationAssignmentType)activityassignment.ToInteger();
            
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
                SessionVariables.CurrentUser.ID.Value, 
                msgs);

            if (msgs.HasNoErrors())
            {
                selectedRegistration.WasUpdated(msgs);

                // notify parent
                var masterRegistrant = registrant.GetMasterRegistrant(msgs);

                if (masterRegistrant.IsNotEmpty())
                    selectedRegistration.SendConfirmation(
                        new CheckoutObject()
                        {
                            BillingAccount = SessionVariables.CurrentAccount,
                            PayingAccount = masterRegistrant.GetAccount(msgs),
                            MasterRegistrant = masterRegistrant,
                            NumberOfSeats = 1,
                            ParticipationObject = act.GetParticipationObject(msgs),
                            Registration = selectedRegistration,
                        },
                        null,
                        SessionVariables.CurrentUser,
                        true,
                        msgs);

                // LOG:  CurrentWebUser added Registrant RegistrantName to Activity ActivityName
                WebSessionUtilities.LogActivity(
                    webSessionId: SessionVariables.WebSession.ID.Value,
                    actor: SessionVariables.CurrentUser,
                    actionType: WebSessionActionType.Added,
                    objectId: registrant.ID.Value,
                    objectType: ObjectType.Registrant,
                    objectText: registrant.GetFirstNameLastInitial(),
                    indirectObjectId: act.ID.Value,
                    indirectObjectType: ObjectType.Activity,
                    indirectObjectText: act.Name);

                msgs.Add($"{act.Name} has been added for {registrant.Name}", MessageType.Success);
            }

            LoadRegistrationViewBag(registration, r);
            ViewBag.Messages = msgs;

            return View("edit", registration);
        }

        [HttpGet]
        public ActionResult GoToEdit(string id)
        {
            var registration = Registration.LoadByUid(id.ToGuid());
            LoadRegistrationViewBag(registration);

            SessionVariables.Cart = null;

            return Redirect("/reg/edit/" + id);

            //return View("edit", registration);
        }

        [HttpGet]
        public ActionResult Notify(string id, string r)
        {
            var msgs = new SystemMessages();
            var registration = Registration.LoadByUid(id.ToGuid());
            var selectedRegistration = Registration.LoadByUid(r.ToGuid());

            // TODO: Need to figure out
            // BackgroundJob.Enqueue(() => Registration.SendConfirmation(selectedRegistration.ID.Value, true, null));

            msgs.Add("Not Implemented", MessageType.UserError);
            // msgs.Add(string.Format("{0} will be notified shortly", selectedRegistration.ToRegistrant().Name), MessageType.Success);

            LoadRegistrationViewBag(registration, r);
            ViewBag.Messages = msgs;
            return View("Edit", registration);
        }

        [HttpGet]
        public ActionResult Delete(string id, string r, string returnurl)
        {
            var registration = Registration.LoadByUid(id.ToGuid());
            var selectedRegistration = Registration.LoadByUid(r.ToGuid());
            var msgs = new SystemMessages();

            if (returnurl.IsNotEmpty())
            {
                var ra = selectedRegistration.GetRegistrantActivities(msgs).FirstOrDefault();
                if (ra.IsNotEmpty())
                {
                    var activity = ra.GetActivity(msgs);

                    if (activity.IsNotEmpty())
                        returnurl = Url.Action("registrants", "activity", new { id = activity.Uid });
                }
            }

            selectedRegistration.CancelAll(msgs);

            if (msgs.HasNoErrors())
            {
                if (returnurl.IsNotEmpty())
                    return Redirect(returnurl);

                return RedirectToAction("index");
            }

            ViewBag.Messages = msgs;
            LoadRegistrationViewBag(registration, r);
            return View("Edit", registration);
        }

        [HttpGet]
        public ActionResult RegistrationEdit(string id)
        {
            return View();
        }

        private void LoadRegistrationViewBag(Registration registration, string selectedRegistration = null)
        {
            var registrant = registration.GetRegistrant();
            if (registration.AccountId != SessionVariables.CurrentAccount.ID)
                registration = null;

            var allRegistrations = registration.GetRelatedRegistrations();
            allRegistrations.Add(registration);
            allRegistrations = allRegistrations.OrderByDescending(x => x.ID).ToList();

            if(selectedRegistration.IsGuid())
                registration = allRegistrations.First(x => x.Uid == selectedRegistration.ToGuid());
            else
                registration = allRegistrations.First();

            var ras = registration.GetRegistrantActivities();
            var rasActivityTypes = ras.Where(y => y.ParticipationObjectTypeId == ParticipationObjectType.Activity);
            ViewBag.Messages = new SystemMessages();
            ViewBag.Registrant = registrant;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.Answers = registration.GetAnswers();
            ViewBag.RegistrantActivities = ras;
            ViewBag.Registrations = allRegistrations;
            ViewBag.SelectedRegistration = registration;

            if (registration.IsNotEmpty())
                ViewBag.CanMergeWith = allRegistrations.Where(x => x.RegistrationPeriodId == registration.RegistrationPeriodId && x.ID != registration.ID).ToList();

            List<Activity> activities = null;

            if (registration.ActivityGroupId.HasValue)
                activities = registration.GetActivityGroup().GetActivities(
                    activityType: null, 
                    isArchived: false, 
                    isDeleted: false, 
                    start: null, 
                    length: null, 
                    forceReload: false);
            else
                activities = SessionVariables.CurrentAccount.GetActivities(
                    userType: UserType.Administrator, 
                    user: SessionVariables.CurrentUser, 
                    activityType: null, 
                    isArchived: false, 
                    isDeleted: false, 
                    ungroupedOnly: null,
                    start: null, 
                    length: null);

            ViewBag.AvailableActivities = activities
                                            .Where(x => x.ID.Value.NotIn(rasActivityTypes.Select(y => y.ParticipationObjectId.Value)))
                                            .OrderBy(x => x.Name)
                                            .ToList();

            if (Request["returnurl"].IsNotEmpty())
                ViewBag.ListUrl = Request["returnurl"];
            else
                ViewBag.ListUrl = Url.Action("index", "registration");
        }

        private (List<Registration>, int) LoadSearch(string activity, string group, int? pageNumber, int? recordsPerPage, string q, string emailed, string question, string regstatus, string at, bool isReadOnly)
        {
            var msgs = new SystemMessages();
            List<Registration> registrations;
            var activities = new List<Activity>();
            var groups = new List<ActivityGroup>();
            var selectedGroups = new List<ActivityGroup>();
            var selectedActivities = new List<Activity>();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;
            var totalCount = 0;
            var activityIds = new List<int>();
            List<int> groupIds;
            var questionId = new int?();
            var rats = new List<RegistrationAssignmentType>();
            var registrationStatus = RegistrationStatus.NotSet;

            if (activity.IsNotEmpty())
            {
                activityIds = activity.Split(',').Select(x => x.ToInteger()).ToList();
                selectedActivities = Activity.LoadList(activityIds);
            }

            if (group.IsNotEmpty())
            {
                groupIds = group.Split(',').Select(x => x.ToInteger()).ToList();
                selectedGroups = ActivityGroup.LoadList(groupIds, msgs);
                selectedGroups.ForEach(x => {
                    activityIds.AddRange(x.GetActivities(null, false, false, null, null).Select(y => y.ID.Value));
                });
            }

            if (at.IsNumber())
                rats.Add((RegistrationAssignmentType)at.ToInteger());

            if (regstatus.IsNumber())
                registrationStatus = (RegistrationStatus)regstatus.ToInteger();

            //if (question.IsNumber())
            //    questionId = question.ToInteger();

            registrations = SessionVariables.CurrentAccount.SearchRegistrations(
                q,
                questionId,
                activityIds,
                ParticipationObjectType.Activity,
                registrationStatus,
                rats,
                start,
                length,
                msgs);

            totalCount = SessionVariables.CurrentAccount.SearchRegistrationCount(
                q,
                questionId,
                activityIds,
                ParticipationObjectType.Activity,
                registrationStatus,
                rats,
                msgs);

            activities = SessionVariables.CurrentAccount.GetActivities(
                UserType.Administrator,
                SessionVariables.CurrentUser,
                null,
                false,
                false,
                null)
                .OrderBy(x => x.Name)
                .ToList();

            groups = SessionVariables.CurrentAccount.GetActivityGroups(msgs);

            //get all questions
            //var activityForms = SessionVariables.CurrentAccount.GetActivityForms().Where(x => x.IsActive).ToList();
            //var questions = FormQuestion.LoadByFormIdsByIsActive(activityForms.Select(x => x.FormId.Value).ToList(), true);
            //var finalQuestions = new List<FormQuestion>();
            //var finalQuestionGroups = new List<FormQuestionGroup>();

            //finalQuestions.AddRange(questions.Where(x => x.QuestionDataTypeId != QuestionDataType.NotSet).DistinctBy(x => x.QuestionDataTypeId));
            //finalQuestions.AddRange(questions.Where(x => x.QuestionDataTypeId == QuestionDataType.NotSet).DistinctBy(x => new { x.Text, x.FormQuestionGroupName }));

            //var questionGroups = questions.Select(x => x.ToFormQuestionGroup()).ToList();
            //finalQuestionGroups.AddRange(questionGroups.Where(x => x.QuestionGroupDataTypeId != QuestionGroupDataType.NotSet).DistinctBy(x => x.QuestionGroupDataTypeId));
            //finalQuestionGroups.AddRange(questionGroups.Where(x => x.QuestionGroupDataTypeId == QuestionGroupDataType.NotSet).DistinctBy(x => x.Name));

            //ViewBag.Questions = finalQuestions;
            //ViewBag.QuestionGroups = finalQuestionGroups;

            ViewBag.Activities = activities;
            ViewBag.Groups = groups;
            ViewBag.SelectedActivities = selectedActivities;
            ViewBag.SelectedGroups = selectedGroups;
            ViewBag.SearchBarReadOnly = isReadOnly;

            return (registrations, totalCount);
        }

        [HttpGet]
        public ActionResult Messages(string pn, string rpp)
        {
            var msgs = new SystemMessages();
            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            var activityMessages = SessionVariables.CurrentAccount.GetActivityMessages(start, length, msgs);
            var totalCount = SessionVariables.CurrentAccount.GetActivityMessageCount(msgs);
            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();
            var (registrations, totalRegistrationCount) = LoadSearch("", "", null, null, "", "", "", "", "", false);

            ViewBag.Messages = msgs;
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };
            return View(activityMessages);
        }

        [HttpGet]
        public ActionResult MessageDetail(string id)
        {
            var am = ActivityMessage.LoadByUid(id.ToGuid());
            ViewBag.Messages = new SystemMessages();
            ViewBag.Registrants = am.GetActivityMessageRegistrants();
            return View(am);
        }

        [HttpGet]
        public ActionResult MessageAdd(string activity, string group, string pn, string rpp, string q, string emailed, string question, string regstatus, string at)
        {
            var msgs = new SystemMessages();
            var (registrations, totalCount) = LoadSearch(activity, group, null, null, q, emailed, question, regstatus, at, true);

            ViewBag.Registrations = registrations;
            ViewBag.TotalCount = totalCount;
            ViewBag.Messages = msgs;

            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult MessageAdd(string id, string subject, string body, string sendtostaff, string activity, string group, string pn, string rpp, string q, string emailed, string question, string regstatus, string at)
        {
            var msgs = new SystemMessages();

            if (subject.IsEmpty())
                msgs.Add("Message Reason is required", MessageType.UserError);
            if (body.IsEmpty())
                msgs.Add("Message Body is required", MessageType.UserError);

            ActivityMessage am = null;
            var (registrations, totalCount) = LoadSearch(activity, group, null, null, q, emailed, question, regstatus, at, true);

            if (msgs.HasNoErrors())
            {
                var registrantIds = registrations.Select(registration => registration.RegistrantId.Value).ToList();

                am = SessionVariables.CurrentAccount.AddMessage(
                        subject,
                        body,
                        registrantIds,
                        sendtostaff.IsBoolean(),
                        SessionVariables.CurrentUser.ID.Value,
                        msgs);

                BackgroundJob.Enqueue(() => ActivityMessageService.Send(am.ID.Value, SessionVariables.CurrentUser.ID.Value, null, null));
            }

            ViewBag.Registrations = registrations;
            ViewBag.TotalCount = totalCount;
            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
            {
                msgs.Add("Your message will been sent to the selected registrants", MessageType.Success);
                ViewBag.Registrants = am.GetActivityMessageRegistrants();

                return View("messagedetail", am.Reload());
            }

            return View(activity);
        }

        [HttpGet]
        public ActionResult MessagesResend(string id)
        {
            var msgs = new SystemMessages();
            var am = ActivityMessage.LoadByUid(id.ToGuid(), msgs);

            BackgroundJob.Enqueue(() => ActivityMessageService.Send(am.ID.Value, SessionVariables.CurrentUser.ID.Value, null, null));

            ViewBag.Messages = msgs;
            ViewBag.Registrants = am.GetActivityMessageRegistrants();

            if (msgs.HasNoErrors())
            {
                msgs.Add("Your message will been sent to the selected registrants", MessageType.Success);
                return View("messagedetail", am);
            }

            return View("messagedetail", am);
        }
    }
}
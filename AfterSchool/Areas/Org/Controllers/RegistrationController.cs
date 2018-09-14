using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchool.Models;
using System.Threading.Tasks;
using Hangfire;

namespace AfterSchool.Areas.Org.Controllers
{
    public class RegistrationController : BaseLoggedInController
    {
        public RegistrationController() : base(new List<AccountType> { AccountType.School, AccountType.ActivityProvider })
        {

        }

        // GET: Org/Registrant
        public ActionResult Index(string activity, string group, string pn, string rpp, string q, string emailed, string question, string regstatus, string at)
        {
            var msgs = new SystemMessages();
            var registrations = new List<Registration>();
            var activities = new List<Activity>();
            var groups = new List<ActivityGroup>();
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
            var activityIds = new List<int>();
            var groupIds = new List<int>();
            var questionId = new int?();
            var rat = RegistrationAssignmentType.NotSet;
            var registrationStatus = RegistrationStatus.NotSet;

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

            if (regstatus.IsNumber())
                registrationStatus = (RegistrationStatus)regstatus.ToInteger();

            if (question.IsNumber())
                questionId = question.ToInteger();

            registrations = SessionVariables.CurrentAccount.SearchRegistrations(
                q, 
                questionId, 
                activityIds,
                ParticipationObjectType.Activity,
                registrationStatus, 
                rat, 
                start, 
                length);

            totalCount = SessionVariables.CurrentAccount.SearchRegistrationCount(
                q, 
                questionId,
                activityIds,
                ParticipationObjectType.Activity,
                registrationStatus, 
                rat);                 

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();
            activities = SessionVariables.CurrentAccount.GetActivities(
                UserType.Administrator, 
                SessionVariables.LoggedInUser,
                null,
                false, 
                false)
                .OrderBy(x => x.Name)
                .ToList();

            groups = SessionVariables.CurrentAccount.GetSessions();

            if (emailed.IsBoolean() && emailed.ToBoolean())
                msgs.Add("An email will be sent shortly", MessageType.Success);

            //get all questions
            var activityForms = SessionVariables.CurrentAccount.GetActivityForms().Where(x => x.IsActive).ToList();
            var questions = FormQuestion.LoadByFormIdsByIsActive(activityForms.Select(x => x.FormId.Value).ToList(), true);
            var finalQuestions = new List<FormQuestion>();
            var finalQuestionGroups = new List<FormQuestionGroup>();

            finalQuestions.AddRange(questions.Where(x => x.QuestionDataTypeId != QuestionDataType.NotSet).DistinctBy(x => x.QuestionDataTypeId));
            finalQuestions.AddRange(questions.Where(x => x.QuestionDataTypeId == QuestionDataType.NotSet).DistinctBy(x => new { x.Text, x.FormQuestionGroupName }));

            var questionGroups = questions.Select(x => x.ToFormQuestionGroup()).ToList();
            finalQuestionGroups.AddRange(questionGroups.Where(x => x.QuestionGroupDataTypeId != QuestionGroupDataType.NotSet).DistinctBy(x => x.QuestionGroupDataTypeId));
            finalQuestionGroups.AddRange(questionGroups.Where(x => x.QuestionGroupDataTypeId == QuestionGroupDataType.NotSet).DistinctBy(x => x.Name));

            ViewBag.Questions = finalQuestions;
            ViewBag.QuestionGroups = finalQuestionGroups;      
            ViewBag.Messages = msgs;
            ViewBag.Activities = activities;
            ViewBag.Groups = groups;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.ActivityGroupType = ActivityGroupType.Session;
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
            var rat = RegistrationAssignmentType.NotSet;
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
                Registration.EmailCSVReport(
                    account.ID.Value,
                    q,
                    questionId,
                    activityIds,
                    ParticipationObjectType.Activity,
                    filename,
                    (RegistrationReportType)reporttype.ToInteger(),
                    RegistrationAssignmentType.Reserved,
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


        [HttpPost]
        public ActionResult Merge(string id, string r, string returnUrl, string mergeintoreg)
        {
            var msgs = new SystemMessages();
            var registration = Registration.LoadByUid(r.ToGuid()); // load selected registration

            if (mergeintoreg.IsGuid())
            {
                var mergeInto = Registration.LoadByUid(mergeintoreg.ToGuid());
                msgs.AddRange(mergeInto.MergeInto(registration, SessionVariables.LoggedInUser.ID.Value));
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
                msgs.AddRange(registrantActivity.ChangeSeatAssignment(ratTo, SessionVariables.LoggedInUser.ID.Value));

            if (msgs.HasNoErrors())
            {
                var sat = SessionActionType.NotSet;
                switch (ratTo)
                {
                    case RegistrationAssignmentType.Applied:
                        sat = SessionActionType.ActivityMoveToApplication;
                        break;
                    case RegistrationAssignmentType.Reserved:
                        sat = SessionActionType.ActivityMoveToReserved;
                        break;
                    case RegistrationAssignmentType.WaitingList:
                        sat = SessionActionType.ActivityMoveToWaitingList;
                        break;
                }


                msgs.AddRange(selectedRegistration.WasUpdated());

                UpdateSession(sat, SessionVariables.CurrentAccount.ID, registrantActivity.GetActivityId(), null, selectedRegistration.RegistrantId, selectedRegistration.ID, selectedRegistration.RegistrationPeriodId, registrantActivity.GetActivityGroupId());

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

            msgs.AddRange(selectedRegistration.RemoveParticipation(registrantActivity.ParticipationObjectId.Value, registrantActivity.ParticipationObjectTypeId));

            if (msgs.HasNoErrors())
            {
                var act = RegistrantActivity.Load(ra.ToInteger());

                msgs.AddRange(selectedRegistration.WasUpdated());

                UpdateSession(SessionActionType.RegistrationRemoveActivity, SessionVariables.CurrentAccount.ID, act.ID, null, selectedRegistration.RegistrantId, selectedRegistration.ID, selectedRegistration.RegistrationPeriodId);

                msgs.Add($"Removed {UILabel.ActivityLabel(activity.ActivityTypeId)}: {activity.Name}", MessageType.Success);
            }

            LoadRegistrationViewBag(registration, r);
            ViewBag.Messages = msgs;

            return View("edit", registration);
        }

        [HttpPost]
        public ActionResult AddActivity(string id, string activity, string activityassignment, string preferencenumber, string r)
        {
            var registration = Registration.LoadByUid(id.ToGuid());
            var selectedRegistration = Registration.LoadByUid(r.ToGuid());
            var ras = selectedRegistration.GetRegistrantActivities();
            var act = Activity.Load(activity.ToInteger());
            var registrant = selectedRegistration.GetRegistrant();
            var regperiod = act.GetActiveRegistrationPeriod();            
            var msgs = new SystemMessages();
            var assignmentType = (RegistrationAssignmentType)activityassignment.ToInteger();
            
            var choiceNumber = 1;
            var registrationPeriodId = new int?();
            if (regperiod.IsNotEmpty())
                registrationPeriodId = regperiod.ID;

            registrant.AddOrUpdateParticipationObject(
                selectedRegistration.ID.Value, 
                activity.ToInteger(), 
                ParticipationObjectType.Activity, 
                assignmentType, 
                choiceNumber, 
                registrationPeriodId, 
                act.ActivityGroupId, 
                SessionVariables.LoggedInUser.ID.Value, 
                msgs);

            if (msgs.HasNoErrors())
            {
                msgs.AddRange(selectedRegistration.WasUpdated());

                UpdateSession(SessionActionType.RegistrationAddActivity, SessionVariables.CurrentAccount.ID, act.ID, null, selectedRegistration.RegistrantId, selectedRegistration.ID, selectedRegistration.RegistrationPeriodId);

                msgs.Add("Added New Activity: " + act.Name, MessageType.Success);
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
        public ActionResult Delete(string id, string r)
        {
            var registration = Registration.LoadByUid(id.ToGuid());
            var selectedRegistration = Registration.LoadByUid(r.ToGuid());
            var msgs = new SystemMessages();

            msgs = selectedRegistration.CancelAll();

            if (msgs.HasNoErrors())
            {
                return RedirectToAction("index");
            }
            else
            {
                ViewBag.Messages = msgs;
                LoadRegistrationViewBag(registration, r);
                return View("Edit", registration);
            }
        }

        [HttpGet]
        public ActionResult RegistrationEdit(string id)
        {
            return View();
        }

        public void LoadRegistrationViewBag(Registration registration, string selectedRegistration = null)
        {
            var registrant = registration.GetRegistrant();
            if (registration.AccountId != SessionVariables.CurrentAccount.ID)
                registration = null;

            var allRegistrations = registration.GetRelatedRegistrations();
            allRegistrations.Add(registration);
            allRegistrations = allRegistrations.OrderByDescending(x => x.ID).ToList();

            if(selectedRegistration.IsGuid())
                registration = allRegistrations.Where(x => x.Uid == selectedRegistration.ToGuid()).First();
            else
                registration = allRegistrations.First();

            var registrantActivities = registration.GetRegistrantActivities();
            ViewBag.Messages = new SystemMessages();
            ViewBag.Registrant = registrant;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.Answers = registration.GetAnswers();
            ViewBag.RegistrantActivities = registrantActivities;
            ViewBag.Registrations = allRegistrations;
            ViewBag.SelectedRegistration = registration;

            if (registration.IsNotEmpty())
                ViewBag.CanMergeWith = allRegistrations.Where(x => x.RegistrationPeriodId == registration.RegistrationPeriodId && x.ID != registration.ID).ToList();

            var activities = new List<Activity>();

            if (registration.ActivityGroupId.HasValue)
                activities = registration.GetActivityGroup().GetActivities(null, false, false, null, null, false);
            else
                activities = SessionVariables.CurrentAccount.GetActivities(UserType.Administrator, SessionVariables.LoggedInUser, null, false, false, null, null);

            ViewBag.Activities = activities.Where(x =>
                    x.ID.Value.NotIn(registrantActivities.Where(y => y.ParticipationObjectTypeId == ParticipationObjectType.Activity)
                    .Select(y => y.ParticipationObjectId.Value))
                )
                .OrderBy(x => x.Name)
                .ToList();

            if (Request["returnurl"].IsNotEmpty())
                ViewBag.ListUrl = Request["returnurl"];
            else
                ViewBag.ListUrl = Url.Action("index", "registration");
        }
    }
}
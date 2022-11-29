using AfterSchool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AfterSchool.Areas.District.Models;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Areas.District.Controllers
{
    public class VolunteerController : BaseLoggedInController
    {
        public VolunteerController() : base(new List<AccountType> { AccountType.SchoolDistrict })
        {

        }

        // GET: District/Profile
        public ActionResult Index()
        {
            var volunteerViewModel = new List<VolunteerViewModel>();
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var registeredVolunteerRegistrants = Registrant.LoadMasterRegistrantsByAccountHostAndPersonType((int)currentAccount.ID, RegistrantPersonType.Volunteer , msgs);
            
            if (registeredVolunteerRegistrants.IsNotEmpty())
            {
                foreach (var registrant in registeredVolunteerRegistrants)
                {
                    var volunteerApproval = VolunteerApproval.LoadByWebUserId((int)registrant.WebUserId,msgs);
                    
                    if(volunteerApproval.IsNotEmpty())
                        volunteerViewModel.Add(VolunteerAssembler.VolunteerAssemblerResult(registrant, volunteerApproval));
                    
                }                
            }
            
            ViewBag.Messages = msgs;
            return View(volunteerViewModel);
        }
        
        public ActionResult BackgroundCheck()
        {
            List<Account> account = new List<Account>();
            var msgs = new SystemMessages();
            ViewBag.Messages = msgs;
            return View(account);
        }
        
        public ActionResult Details(int id, string r, string returnUrl)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var registration = Registration.LoadByRegistrantId(id, msgs).FirstOrDefault();

            LoadRegistrationViewBag(registration, r, msgs);

            if (registration.IsEmpty())
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);

            ViewBag.ReturnURl = returnUrl;
            return View(registration);
        }
        
        public ActionResult Todo(int taskType = (int)TaskStatus.ReviewApplicant , int applicationStatus = (int)ApplicationStatus.New)
        {
            var TodoViewModelList = new List<TodoViewModel>();
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = TempData["msgs"].IsNotEmpty() ? (SystemMessages) TempData["msgs"] : new SystemMessages();
            var volunteerApprovals = VolunteerApproval.Load();
            var volunteerTasks = VolunteerTask.LoadByTaskTypeAndAppStatus((TaskStatus) taskType, (ApplicationStatus) applicationStatus, volunteerApprovals, currentAccount, msgs);
            
            if (volunteerTasks.IsNotEmpty())
            {
                volunteerTasks.ForEach(task =>
                {
                    var webUser = WebUser.LoadbyId((int)task.WebUserId);
                    if(task.IsNotEmpty())
                        TodoViewModelList.Add(TodoAssembler.TodoAssemblerResult(webUser, task));
                });

            }

            ViewBag.SelectedTaskType = (TaskStatus) taskType;
            ViewBag.SelectedApplicationStatus = (ApplicationStatus)applicationStatus;
            ViewBag.Messages = msgs;
            return View(TodoViewModelList);
        }

        [HttpPost]
        public ActionResult AddRemoveApproval(string webUserIDs , string operation , string reason , DateTime expirationDate)
        {
            var msgs = new SystemMessages();
            var currentUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;
            
            if(webUserIDs.IsEmpty())
                msgs.Add("No volunteer was selected",MessageType.UserError);
            
            if(reason.IsEmpty())
                msgs.Add("Reason was not provided",MessageType.UserError);
            
            if(expirationDate.IsEmpty())
                msgs.Add("Expiration date was not selected",MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        foreach (var webUserID in webUserIDs.Split(','))
                        {
                            var volunteerTask = VolunteerTask.LoadByWebUser(webUserID.ToInteger());
                            
                            VolunteerTaskService.EditTask(volunteerTask,
                                amsgs, adc);

                            VolunteerApprovalService.AddApproval(Convert.ToInt32(webUserID),
                                currentAccount.ID,
                                (operation.Equals("approve") ? ApprovalStatus.Approved : ApprovalStatus.Denied),
                                reason,
                                expirationDate,
                                currentUser,
                                amsgs,adc
                            );

                        }
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        smsgs.Add("Done",MessageType.Success);
                        msgs.AddRange(smsgs);
                    },
                    onFailure: (SystemMessages fmsgs) => { msgs.AddRange(fmsgs); },
                    onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                );
            }

            TempData["msgs"] = msgs;
            
            return RedirectToAction("Todo");
        }
        
        private void LoadRegistrationViewBag(Registration registration, string selectedRegistration = null,
            SystemMessages msgs = null)
        {
            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            var registrant = registration.GetRegistrant(msgs);

            // if (registration.AccountId != currentAccount.ID)
            //     registration = null;

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

    }
}
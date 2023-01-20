using AfterSchool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using AfterSchool.Areas.District.Models;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.FeatureFlag;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Integrations;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Hangfire;
using Email = BasicLibrary.Email;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;
using Model = AfterSchoolLibrary.Models;

namespace AfterSchool.Areas.District.Controllers
{
    public class VolunteerController : BaseLoggedInController
    {
        private readonly IBackgroundCheckWorkflow backgroundCheckWorkflow;
        private readonly IFeatureFlagRepository featureFlagRepository;
        public VolunteerController(IFeatureFlagRepository featureFlagRepository, IBackgroundCheckWorkflow backgroundCheckWorkflow) : base(new List<AccountType> { AccountType.SchoolDistrict })
        {
            this.featureFlagRepository = featureFlagRepository;
            this.backgroundCheckWorkflow = backgroundCheckWorkflow;
        }

        // GET: District/Profile
        public ActionResult Index(string pn, string rpp, string search, int type = (int)ApprovalStatus.Approved)
        {
            
            var pageNumber = 1;
            var recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;
            
            var volunteerViewModel = new List<VolunteerViewModel>();
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var count = Registrant.GetCountApprovedMasterRegistrantsByAccountHostAndPersonType((int)currentAccount.ID, RegistrantPersonType.Volunteer, search, type, msgs);
            var registeredVolunteerRegistrants = Registrant.LoadApprovedMasterRegistrantsByAccountHostAndPersonType((int)currentAccount.ID, RegistrantPersonType.Volunteer, search,type, start, length, msgs);
            
            if (registeredVolunteerRegistrants.IsNotEmpty())
            {
                foreach (var registrant in registeredVolunteerRegistrants)
                {
                    var volunteerApproval = VolunteerApproval.LoadByWebUserId((int)registrant.WebUserId,msgs).FirstOrDefault();
                    
                    volunteerViewModel.Add(VolunteerAssembler.VolunteerAssemblerResult(registrant, volunteerApproval));
                    
                }                
            }
            
            var totalPages = Math.Ceiling(count.ToDouble() / recordsPerPage).ToInteger();
            
            HandleTempMessages(msgs);
            
            ViewBag.Messages = msgs;
            ViewBag.SelectedType = (ApprovalStatus) type;
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = count,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages,
            };
            return View(volunteerViewModel);
        }
        
        public ActionResult BackgroundCheck(string pn, string rpp, string search)
        {
            
            var pageNumber = 1;
            var recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;
            
            var backgroundViewModel = new List<BackgroundViewModel>();
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var count = Registrant.GetCountBackgroundMasterRegistrantsByAccountHostAndPersonType((int)currentAccount.ID, RegistrantPersonType.Volunteer, search, msgs);
            var registeredVolunteerRegistrants = Registrant.LoadABackgroundMasterRegistrantsByAccountHostAndPersonType((int)currentAccount.ID, RegistrantPersonType.Volunteer, search, start, length, msgs);

            if (registeredVolunteerRegistrants.IsNotEmpty())
            {
                foreach (var registrant in registeredVolunteerRegistrants)
                {
                    var backgroundCheck = Model.BackgroundCheck.LoadByWebUserId((int)registrant.WebUserId).FirstOrDefault();
                    
                    if(backgroundCheck.IsNotEmpty())
                        backgroundViewModel.Add(BackgroundAssembler.BackgroundAssemblerResult(registrant, backgroundCheck));
                        
                }
            }

            var totalPages = Math.Ceiling(count.ToDouble() / recordsPerPage).ToInteger();
            
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = count,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages,
            };
            
            ViewBag.Messages = msgs;
            return View(backgroundViewModel);
        }
        
        [HttpPost]
        public async Task<ActionResult> InitiateBackgroundCheck(string webUserIDs)
        {
            var msgs = new SystemMessages();
            BackgroundCheckRequestResult backgroundCheckRequestResult = new BackgroundCheckRequestResult();

            try
            {
                var currentAccount = SessionVariables.CurrentAccount;
            
                if(webUserIDs.IsEmpty())
                    msgs.Add("No volunteer was selected",MessageType.UserError);

                if (msgs.HasNoErrors())
                {
                    foreach (var webUserID in webUserIDs.Split(','))
                    {
                        var webUser = WebUser.LoadbyId(webUserID.ToInteger(), msgs);
                        var volunteerTask = VolunteerTask.LoadByWebUser(webUserID.ToInteger(),msgs);

                        backgroundCheckRequestResult = await CheckIfBackgroundCheckIsEnabledAndRequestUrl(msgs, webUser, (int)currentAccount.ID);

                        if (msgs.HasNoErrors())
                        {
                            VolunteerTaskService.EditTask(volunteerTask, msgs);
                            
                            SendLinkToVolunteer(backgroundCheckRequestResult.BackgroundCheckUrl, webUser, msgs);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(ex);
                msgs.Add(ex.Message, MessageType.Error, ex);
                msgs.AddRange(backgroundCheckRequestResult.msg);
            }
            
            HandleTempMessages(msgs);

            return RedirectToAction("Todo");
        }

        public ActionResult SendMessage(string email, string subject, string body)
        {
            var msgs = new SystemMessages();
            
            if(body.Length > 1000)
                msgs.Add("Message is greater than 1000 characters",MessageType.Error);
            
            if(msgs.HasNoErrors())
                EmailHelper.SendEmail(email, subject, body, true);
            
            HandleTempMessages(msgs);
            
            return RedirectToAction("Index");
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
        
        public ActionResult Todo(string pn, string rpp, int? taskType, int? applicationStatus, string search = "")
        {
            
            var pageNumber = 1;
            var recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;
            
            var TodoViewModelList = new List<TodoViewModel>();
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            HandleTempMessages(msgs);
            //var volunteerApprovals = VolunteerApproval.Load();
            var count = VolunteerTask.GetCountTypeAndAppStatus(taskType, applicationStatus, search, currentAccount, msgs);
            var volunteerTasks = VolunteerTask.LoadByTaskTypeAndAppStatus(taskType, applicationStatus, search, currentAccount, start, length, msgs);

            if (volunteerTasks.IsNotEmpty())
            {
                volunteerTasks.ForEach(task =>
                {
                    var webUser = WebUser.LoadbyId((int)task.WebUserId);
                    if(task.IsNotEmpty())
                        TodoViewModelList.Add(TodoAssembler.TodoAssemblerResult(webUser, task));
                });

            }
            
            var totalPages = Math.Ceiling(count.ToDouble() / recordsPerPage).ToInteger();
            
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = count,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages,
            };
            
            ViewBag.SelectedTaskType = taskType;
            ViewBag.SelectedApplicationStatus = applicationStatus;
            ViewBag.Messages = msgs;
            return View(TodoViewModelList);
        }

        [HttpPost]
        public ActionResult AddRemoveApproval(string webUserIDs , string operation , string reason , DateTime expirationDate,string returnUrl = null)
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
            
            if((expirationDate - DateTime.UtcNow).TotalDays < 30)
                msgs.Add("Expiration date should be greater than a month",MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        foreach (var webUserID in webUserIDs.Split(','))
                        {
                            var volunteerTask = VolunteerTask.LoadByWebUser(webUserID.ToInteger(), amsgs , adc);

                            if (volunteerTask.IsNotEmpty())
                            {
                                VolunteerTaskService.EditTask(volunteerTask,
                                    amsgs, adc);
                            }

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

            HandleTempMessages(msgs);
            
            if(returnUrl.IsNotEmpty())
                return new RedirectResult(returnUrl);
            
            return RedirectToAction("Todo");
        }

        public ActionResult ExportVolunteer(string search, int type = (int)ApprovalStatus.Approved)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;

            RegistrationService.EmailCsvReportDistrict(
                currentAccount.ID.Value,
                search,
                UILabel.ActivityLabel(ActivityType.Activity, false),
                currentAccount.Email,
                null,
                null,
                type,
                msgs,
                null);

            if (msgs.HasErrors())
            {
                msgs.Add("Email will be sent shortly",MessageType.Success);
            }
            
            HandleTempMessages(msgs);
            
            return RedirectToAction("Index" , new {search,type});
        }
        
        private void LoadRegistrationViewBag(Registration registration, string selectedRegistration = null,
            SystemMessages msgs = null)
        {
            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            var registrant = registration.GetRegistrant(msgs);

            var allRegistrations = registration.GetRelatedRegistrationsDistrict(msgs);
            allRegistrations.Add(registration);
            allRegistrations = allRegistrations.OrderBy(x => x.ID).ToList();

            if (selectedRegistration.IsGuid())
                registration = allRegistrations.First(x => x.Uid == selectedRegistration.ToGuid());

            if (registration.IsEmpty())
                return;
            
            var volunteerApprovalStatus = VolunteerApproval.LoadByWebUserId((int)registrant.WebUserId,msgs).FirstOrDefault()?.ApprovalStatusId;
            ViewBag.Registrant = registrant;
            ViewBag.Account = currentAccount;
            ViewBag.Answers = registration.GetAnswers(msgs);
            ViewBag.Registrations = allRegistrations;
            ViewBag.SelectedRegistration = registration;
            ViewBag.SelectedType = volunteerApprovalStatus.IsNotEmpty() ? volunteerApprovalStatus :  ApprovalStatus.NotSet;
            ViewBag.Messages = msgs;

            if (Request["returnurl"].IsNotEmpty())
                ViewBag.ListUrl = Request["returnurl"];
            else
                ViewBag.ListUrl = Url.Action("index", "registration");
        }
        
        private async Task<BackgroundCheckRequestResult> CheckIfBackgroundCheckIsEnabledAndRequestUrl(SystemMessages msgs, WebUser webUser, int accountId)
        {
            if (msgs.HasErrors() || !featureFlagRepository.IsEnabled("background-check")) return null;
            var backgroundCheckResult = await backgroundCheckWorkflow.RequestBackgroundCheck(webUser, accountId, msgs);
            return backgroundCheckResult;
        }

        private void SendLinkToVolunteer(string link, WebUser webUser, SystemMessages msgs)
        {
            EmailHelper.InitiateBackgroundCheckMail(webUser, link, msgs);
        }
        
    }
}
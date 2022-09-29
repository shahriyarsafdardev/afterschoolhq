using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchool.Models;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using ByteSizeLib;
using Stripe;
using AccountService = AfterSchoolLibrary.Services.AccountService;
using Email = AfterSchoolLibrary.Helpers.Email;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Areas.Org.Controllers
{
    public class AccountController : BaseLoggedInController
    {
        public AccountController() : base(FormConstants.OrganizationAccountTypes)
        {
        }

        private bool GetIsFullScreen() =>
            Request["isnew"] == "1" || Request["isfullscreen"] == "1";

        [HttpGet]
        public ActionResult GetStartedPayments(string id)
        {
            var msgs = new SystemMessages();
            HandleTempMessages(msgs);
            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = true;
            return View(SessionVariables.CurrentAccount);
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(SessionVariables.CurrentAccount);
        }

        [HttpPost]
        public ActionResult Edit(string id, string name, string email, string phone, string uniqueName,
            string timeZone, string stripeCustomerId, string stripeSubscriptionId, string startnewtrial, string resyncfromstripe, 
            string resyncpackagefeatures, string endtrial)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var accountPackage = SessionVariables.AccountPackage;
            var isNewCustomer = false;
            var isNewSubscription = false;
            var isSuperAdmin = SessionVariables.OriginalLoggedInUser.IsNotEmpty();

            if (name.IsEmpty())
                msgs.Add("Organization Name is required", MessageType.UserError);

            if (msgs.HasNoErrors())
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        currentAccount.Name = name;
                        currentAccount.Phone = phone;
                        currentAccount.TimeZone = timeZone;
                        currentAccount.Email = email;

                        if (!currentAccount.UniqueName.IsSameAs(uniqueName))
                        {
                            if (currentAccount.UniqueName.IsEmpty() && uniqueName.IsEmpty())
                                currentAccount.UniqueName = AccountService.GenerateUniqueName(name, currentAccount.ID, amsgs, adc);
                            else
                                currentAccount.UniqueName = AccountService.GenerateUniqueName(uniqueName, currentAccount.ID, amsgs, adc);
                        }

                        currentAccount.LastModifiedDate = DateTime.UtcNow;
                        currentAccount.LastModifiedBy = currentUser.ID;

                        if (isSuperAdmin && !currentAccount.StripeCustomerId.IsSameAs(stripeCustomerId))
                        {
                            currentAccount.StripeCustomerId = stripeCustomerId;
                            currentAccount.HasDefaultPaymentMethod = false; //reset
                            isNewCustomer = true;
                        }
                        
                        amsgs.AddRange(currentAccount.Save(adc));

                        if (amsgs.HasNoErrors() && isSuperAdmin && !accountPackage.StripeSubscriptionId.IsSameAs(stripeCustomerId))
                        {
                            accountPackage.StripeSubscriptionId = stripeSubscriptionId;
                            amsgs.AddRange(accountPackage.Save(adc));
                            isNewSubscription = true;
                        }

                        if (isNewCustomer || isNewSubscription || resyncfromstripe.IsBoolean())
                        {
                            currentAccount.CheckIfSubscriberAccountHasPaymentMethod(null, amsgs, adc);
                            currentAccount.CheckIfSubscriberAccountSubscriptionChanged(startnewtrial.IsBoolean(), currentUser, amsgs, adc);
                        }

                        if (resyncpackagefeatures.IsBoolean())
                        {
                            accountPackage = currentAccount.GetActiveAccountPackage(amsgs, adc);
                            accountPackage.GetPackage(amsgs, adc).CopyFeaturesToAccountPackage(accountPackage, currentUser, amsgs, adc);
                        }

                        if (endtrial.IsBoolean())
                            accountPackage.EndTrialNow(amsgs, adc);
                        
                        // ..update profile title if account name has been changed 
                        // Should we do this?
                        if (amsgs.HasNoErrors() && name.IsNotEmpty() && currentAccount.Name != name)
                        {
                            var profile = currentAccount.GetAccountProfile(amsgs, adc);

                            if (profile.IsNotEmpty())
                            {
                                profile.Title = name;
                                amsgs.AddRange(profile.Save(adc));
                            }
                        }
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);

                        if (msgs.HasNoErrors())
                            msgs.Add("Saved Successfully", MessageType.Success);
                    },
                    onFailure: (SystemMessages fmsgs) => { msgs.AddRange(fmsgs); },
                    onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                );

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(currentAccount);
        }

        [HttpGet]
        public ActionResult Location()
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Location = currentAccount.GetDefaultLocation(msgs);
            ViewBag.IsFullScreen = GetIsFullScreen();
            ViewBag.Messages = msgs;

            return View(currentAccount);
        }

        [HttpPost]
        public ActionResult Location(string id, string location, string locationname, string address1, string address2,
            string city, string state, string postalcode, string country, string mylocation, string locationtype)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            var locationType = LocationType.NotSet;
            var hostAccountId = new int?();

            if (locationtype.IsNumber())
                locationType = (LocationType) locationtype.ToInteger();

            if (mylocation.IsBoolean() && mylocation.ToBoolean())
                hostAccountId = currentAccount.ID;

            if (location.IsNumber())
            {
                var currentLocation = AfterSchoolLibrary.Models.Location.Load(location.ToInteger(), msgs);

                if (currentLocation.IsOwnedByOrIsNotOwnedAtAll(currentAccount))
                {
                    currentLocation.LocationTypeId = locationType;
                    currentLocation.HostAccountId = hostAccountId;
                    currentLocation.LastModifiedBy = currentUser.ID;
                    currentLocation.LastModifiedDate = DateTime.UtcNow;
                    msgs.AddRange(currentLocation.Save());
                }

                currentAccount.SetDefaultLocation(currentLocation, currentUser, msgs);
            }
            else
            {
                var newLocation = LocationService.CreateNewWithValidation(
                    locationName: locationname,
                    address1: address1,
                    address2: address2,
                    city: city,
                    state: state,
                    postalCode: postalcode,
                    country: "US",
                    locationType: locationType,
                    hostAccountId: hostAccountId,
                    visitorAccount: currentAccount,
                    loggedInUser: currentUser,
                    msgs: msgs);

                if (msgs.HasNoErrors())
                    currentAccount.SetDefaultLocation(newLocation, currentUser, msgs);
            }


            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Location = currentAccount.GetDefaultLocation(msgs);
            ViewBag.IsFullScreen = GetIsFullScreen();
            ViewBag.Messages = msgs;

            return View(currentAccount);
        }

        [HttpGet]
        public ActionResult PaymentsOptions(string showPaymentNotificationPopup)
        {
            var account = SessionVariables.CurrentAccount;

            ViewBag.Account = account;
            ViewBag.ShowPaymentNotificationPopup = showPaymentNotificationPopup;
            ViewBag.IsFullScreen = GetIsFullScreen();
            ViewBag.SignupType = Request["signuptype"].IsNotEmpty() && Request["signuptype"].IsInteger()
                ? (SignUpType) Request["signuptype"].ToInteger()
                : SignUpType.NotSet;
            ViewBag.Messages = new SystemMessages();
            ViewBag.SupportEmail = Email.AppSupportEmailAddress;

            return View();
        }

        [HttpGet]
        public ActionResult CheckSetup()
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var model = currentAccount.GetCheckPaymentAccount(msgs) ?? new CheckPaymentAccount();

            ViewBag.Account = currentAccount;
            ViewBag.IsFullScreen = GetIsFullScreen();
            ViewBag.Messages = msgs;

            return View(model);
        }

        [HttpPost]
        public ActionResult CheckSetup(string entitytype, string name, string address1, string address2, string city,
            string state, string postalcode)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (!entitytype.IsNumber())
                msgs.Add("Select whether we're sending payments to an Individual or a Company", MessageType.UserError);

            if (name.IsEmpty())
                msgs.Add("Payee Name is required", MessageType.UserError);

            if (address1.IsEmpty())
                msgs.Add("Address 1 is required", MessageType.UserError);

            if (city.IsEmpty())
                msgs.Add("City is required", MessageType.UserError);

            if (state.IsEmpty())
                msgs.Add("State is required", MessageType.UserError);

            if (postalcode.IsEmpty())
                msgs.Add("Postal Code is required", MessageType.UserError);

            if (msgs.HasNoErrors())
                currentAccount.CreateOrUpdateCheckPaymentAccount(
                    entitytype.ToEnum<LegalEntityType>(),
                    name,
                    address1,
                    address2,
                    city,
                    state,
                    postalcode,
                    SessionVariables.CurrentUser,
                    msgs);

            if (msgs.HasNoErrors() && Request["isaccountredirect"] == "1")
            {
                TempData["IsStripeIntegrated"] = true;
                return Redirect(currentAccount.ShortHomePageUrl);
            }

            if (msgs.HasNoErrors() && Request["isnew"] == "1")
                return RedirectToAction("paymentsdone", new {isnew = 1});

            var model = currentAccount.GetCheckPaymentAccount(msgs) ?? new CheckPaymentAccount();

            ViewBag.Account = currentAccount;
            ViewBag.IsFullScreen = GetIsFullScreen();
            ViewBag.Messages = msgs;

            return View(model);
        }

        [HttpGet]
        public ActionResult PaymentsDone(string id)
        {
            var account = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            ViewBag.HasActivities = account.HasActivities(msgs);
            ViewBag.Account = account;
            ViewBag.IsFullScreen = GetIsFullScreen();

            //.. send support email to AHQ just to notifiy the team that a payment has been added/updated.
            var body = $"<p>Account: {account.Name}<br />" +
                       $"Account Profile Url: {account.HomePageUrl}<br />" +
                       $"User: {currentUser.FirstName} {currentUser.LastName}<br />";

            EmailHelper.SendEmail(
                toEmail: Email.AppCustomersEmailAddress,
                subject: $"{account.Name} has connected their stripe account",
                body,
                isAfterSchool: true);

            ViewBag.Messages = msgs;
            return View();
        }

        [HttpGet]
        public ActionResult Features()
        {
            var account = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            ViewBag.Features = account.GetAccountPackageFeatures(msgs);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Messages = msgs;

            return View(account);
        }

        [HttpPost]
        public ActionResult Features(string x)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var accountPackage = currentAccount.GetActiveAccountPackage(msgs);

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    foreach (FeatureType type in Enum.GetValues(typeof(FeatureType)))
                    {
                        if (Request[type.ToString()].IsNotEmpty())
                        {
                            var accountFeature = AccountPackageFeature.LoadByAccountPackageIdByFeatureTypeId(
                                                    accountPackage.ID.Value,
                                                    type, 
                                                    msgs: amsgs, 
                                                    dc: adc);
                            
                            if (accountFeature.IsEmpty())
                                accountFeature = new AccountPackageFeature
                                {
                                    AccountPackageId = accountPackage.ID,
                                    CreateDate = DateTime.UtcNow,
                                    CreatedBy = currentUser.ID,
                                    FeatureTypeId = type
                                };

                            accountFeature.Max = Request[type.ToString()].ToDecimal();
                            accountFeature.LastModifiedBy = currentUser.ID;
                            accountFeature.LastModifiedDate = DateTime.UtcNow;
                            amsgs.AddRange(accountFeature.Save(adc));
                            
                            if (accountFeature.FeatureTypeId == FeatureType.TrialDays &&
                                accountPackage.TrialStartDate.HasValue)
                            {
                                accountPackage.UpdateTrialDaysWithStripe(accountFeature.Max.Value.ToInteger(), amsgs, adc);
                            }
                            else if (accountFeature.FeatureTypeId == FeatureType.ManualStripePayouts &&
                                     currentAccount.StripeUserId.IsNotEmpty())
                            {
                                var manualStripePayouts =
                                    accountPackage.HasFeature(featureType: FeatureType.ManualStripePayouts, msgs: amsgs,
                                        dc: adc);
                                
                                new StripeHelper().ToggleStripePayoutForConnectedAccount(
                                    customAccountsStripeUserId: currentAccount.StripeUserId, 
                                    accountName: currentAccount.Name,
                                    payoutIntervalType: manualStripePayouts ? PayoutIntervalType.Manual : PayoutIntervalType.Daily, 
                                    msgs: amsgs);
                            }
                            else if (accountFeature.FeatureTypeId == FeatureType.ManageVolunteerOpportunities)
                            {
                                currentAccount.EnsureDefaultVolunteerForm(currentUser, amsgs, adc);
                            }
                        }
                    }
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                    msgs.Add("Saved Successfully", MessageType.Success);
                    SessionVariables.AccountPackage = currentAccount.GetActiveAccountPackage();
                },
                onFailure: (SystemMessages fmsgs) => { msgs.AddRange(fmsgs); }
            );

            ViewBag.Features = currentAccount.GetAccountPackageFeatures(msgs);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Messages = msgs;

            return View(currentAccount);
        }

        [HttpGet]
        public ActionResult Team()
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var accountPackage = SessionVariables.AccountPackage;
            var members = currentAccount.GetUsers(msgs: msgs);

            ViewBag.AccountPackage = accountPackage;
            ViewBag.Members = members;
            ViewBag.NeedToUpgrade =
                members.Count >= accountPackage.GetFeature(FeatureType.TeamAccounts, msgs).GetMaxValue();
            ViewBag.Messages = msgs;
            return View(currentAccount);
        }

        [HttpGet]
        public ActionResult TeamInvite()
        {
            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsLocationBasedProvider = FormConstants.LocationProviderAccountTypes
                .Contains(SessionVariables.CurrentAccount?.AccountTypeId ?? AccountType.NotSet);
            return View(SessionVariables.CurrentAccount);
        }

        [HttpPost]
        public ActionResult TeamInvite(string usertype, string emails, string message)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var accountPackage = SessionVariables.AccountPackage;

            if (usertype.IsEmpty())
                msgs.Add("Account type is required", MessageType.UserError);

            if (emails.IsEmpty())
                msgs.Add("Please enter at least one email address", MessageType.UserError);
            else
                emails.Split(',')
                    .Select(x => x.Trim())
                    .Where(email => !email.IsMatch(BasicLibrary.Utilities.EmailRegEx))
                    .ToList()
                    .ForEach(email => msgs.Add(string.Format("{0} is not a valid email address", email),
                        MessageType.UserError));

            if (msgs.HasNoErrors())
                emails.Split(',')
                    .Select(x => x.Trim())
                    .ToList()
                    .ForEach(email =>
                        account.InviteUser(
                            "",
                            "",
                            email,
                            "",
                            Request.Params["message"],
                            (UserType) usertype.ToInteger(),
                            currentUser,
                            msgs));

            if (msgs.HasNoErrors())
                msgs.Add("All invites have been sent", MessageType.Success);

            var members = account.GetUsers(msgs: msgs);

            ViewBag.AccountPackage = accountPackage;
            ViewBag.Members = members;
            ViewBag.NeedToUpgrade =
                members.Count >= accountPackage.GetFeature(FeatureType.TeamAccounts, msgs).GetMaxValue();
            ViewBag.Messages = msgs;

            return View("Team", account);
        }

        /***
         * NOTE: Needs Refactoring to change from userid to a more abstract value
         */
        public ActionResult UpdateUserType(string userid, string newusertypeid)
        {
            var msgs = new SystemMessages();
            var currentUser = SessionVariables.CurrentUser;

            if (userid.IsEmpty() || userid.Trim() == "0")
                msgs.Add("User id cannot be empty", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var user = WebUser.Load(userid.ToInteger(), msgs);
                var awu = AccountWebUser.LoadByWebUserIdByIsDeleted(user.ID.Value, false, msgs).FirstOrDefault();

                if (awu.IsNotEmpty())
                    awu.UpdateUserType(currentUser, newusertypeid, msgs);
            }

            return Json(new {status = (msgs.HasNoErrors() ? "ok" : "error")}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TeamRemoveMember(string id)
        {
            var msgs = new SystemMessages();
            var currentUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;
            var accountUserType = SessionVariables.AccountUserType;
            var user = WebUser.LoadByUid(id.ToGuid(), msgs);
            var awu = currentAccount.GetAccountWebUser(user.ID.Value, msgs);

            if (awu.IsNotEmpty() && currentUser.HasAccessTo(currentAccount, msgs) &&
                accountUserType.In(FormConstants.AdminUserTypes))
                awu.MarkAsDeleted(currentUser, msgs);

            ViewBag.Messages = msgs;
            return RedirectToAction("Team");
        }

        [HttpGet]
        public ActionResult Visitors()
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;

            ViewBag.RequestingVisitors = account.GetRequestedVisitorAccounts(null, msgs);
            ViewBag.ApprovedVisitors = account.GetApprovedVisitorAccounts(msgs);
            ViewBag.Messages = msgs;

            return View(account);
        }

        [HttpGet]
        public ActionResult ApproveVisitor(string id)
        {
            var msgs = new SystemMessages();
            var accountRelationship = SessionVariables.CurrentAccount.GetAccountRelationship(id.ToInteger(), msgs);
            msgs.AddRange(
                accountRelationship.UpdateApprovalStatus(ApprovalStatus.Approved, SessionVariables.CurrentUser));

            ViewBag.Messages = msgs;
            return RedirectToAction("visitors");
        }

        [HttpGet]
        public ActionResult DenyVisitor(string id)
        {
            var msgs = new SystemMessages();
            var accountRelationship = SessionVariables.CurrentAccount.GetAccountRelationship(id.ToInteger(), msgs);
            msgs.AddRange(
                accountRelationship.UpdateApprovalStatus(ApprovalStatus.Denied, SessionVariables.CurrentUser));

            ViewBag.Messages = msgs;
            return RedirectToAction("visitors");
        }

        [HttpGet]
        public ActionResult Forms(string fromSurvey, string pn, string rpp, string q, string type)
        {
            var msgs = new SystemMessages();
            
            var pageNumber = 1;
            var recordsPerPage = 10;
            var currentAccount = SessionVariables.CurrentAccount;
            var formType = FormType.NotSet;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            if (type.IsNotEmpty() && type.IsNumber())
                formType = type.ToInteger().ToEnum<FormType>();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;
            
            var forms = currentAccount.GetForms(formType, q, start, length, msgs);
            var formCount = currentAccount.GetFormCount(formType, q, msgs);
            var totalPages = Math.Ceiling(formCount.ToDouble() / recordsPerPage).ToInteger();
            
            ViewBag.PagingModel = new PagingModel
            {
                TotalCount = formCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages,
            };
            ViewBag.Messages = msgs;
            ViewBag.FromSurvey = fromSurvey;
            return View(forms);
        }

        [HttpGet]
        public ActionResult FormEdit(string id, string qg)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            HandleTempMessages(msgs);

            if (id.IsEmpty())
                return HttpNotFound();

            var form = currentAccount.GetForm(id.ToInteger(), msgs);
            var questionGroups = form.GetQuestionGroups(msgs);
            var individualGroups = questionGroups
                .Where(x => x.RegistrantTypeId == RegistrantType.Individual)
                .OrderBy(x => x.DisplayOrder).ToList();

            //make sure only question groups with a setting of individual or organization are added to the final count
            questionGroups.Clear();
            questionGroups.AddRange(individualGroups);

            FormQuestionGroup questionGroup;

            if (qg.IsNumber())
            {
                if (qg.ToInteger() == 0)
                    questionGroup = new FormQuestionGroup
                    {
                        QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
                        FormId = form.ID,
                        RegistrantTypeId = RegistrantType.Individual,
                        ID = 0,
                    };
                else
                    questionGroup = FormQuestionGroup.Load(qg.ToInteger(), msgs);
            }
            else
                questionGroup = null;

            ViewBag.Messages = msgs;
            ViewBag.QuestionGroups = questionGroups;
            ViewBag.IndividualGroups = individualGroups;
            ViewBag.QuestionGroup = questionGroup;
            return View(form);
        }

        [HttpPost]
        public ActionResult FormEdit(string id, string copyfrom, string qg, string name, string description, string cmd,
            string formtype)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (id.IsEmpty() && copyfrom.IsEmpty())
                return HttpNotFound();

            Form form = null;
            FormQuestionGroup questionGroup = null;
            FormType formTypeId = (FormType)formtype.ToInteger();

            if (name.IsEmpty() && qg.IsBlank())
                msgs.Add("Name is required", MessageType.UserError);

            if (id.ToInteger() == 0)
            {
                if (copyfrom.IsNumber())
                {
                    var formToCopy = currentAccount.GetForm(copyfrom.ToInteger(), msgs);
                    
                    if (formToCopy.IsNotEmpty())
                        form = formToCopy.Copy(currentUser.ID.Value, currentAccount.ID.Value, formToCopy.FormTypeId, null, msgs);
                }
                else
                    form = currentAccount.GetOrCreateDefaultForm(formTypeId, currentUser, msgs)
                                            .Copy(currentUser.ID.Value, currentAccount.ID.Value,
                                                formTypeId, null, msgs);
            }
            else
                form = currentAccount.GetForm(id.ToInteger(), msgs);

            var questions = new List<FormQuestion>();
            var questionGroups = new List<FormQuestionGroup>();
            var individualGroups = new List<FormQuestionGroup>();

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        if (name.IsNotBlank()) form.Name = name;
                        form.Description = description;
                        if (formtype.IsNotBlank()) form.FormTypeId = (FormType) formtype.ToInteger();

                        amsgs.AddRange(form.Save(adc));

                        if (qg.IsNumber())
                        {
                            // create or update question groups
                            if (qg.ToInteger() == 0)
                            {
                                var sectionName = Request.Params["name-0"];
                                var questionGroupExists = FormQuestionGroup.Exists(
                                    criterias: new Hashtable
                                        {{"Name", sectionName}, {"FormId", form.ID}, {"IsDeleted", false}},
                                    msgs: amsgs,
                                    dc: adc);

                                if (!questionGroupExists)
                                {
                                    questionGroup = new FormQuestionGroup
                                    {
                                        CreateDate = DateTime.UtcNow,
                                        CreatedBy = currentUser.ID,
                                        LastModifiedDate = DateTime.UtcNow,
                                        LastModifiedBy = currentUser.ID,
                                        DisplayOrder = form.GetQuestionGroupCount(amsgs, adc) + 1,
                                        FormId = form.ID,
                                        QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
                                        RegistrantTypeId = RegistrantType.Individual,
                                        Name = sectionName,
                                        IsDeleted = false,
                                        IsActive = true,
                                    };
                                    amsgs.AddRange(questionGroup.Save(adc));
                                    questions = new List<FormQuestion>();
                                }
                                else
                                {
                                    questionGroup = FormQuestionGroup
                                        .Load(
                                            criterias: new Hashtable
                                                {{"Name", sectionName}, {"FormId", form.ID}, {"IsDeleted", false}},
                                            msgs: amsgs, dc: adc)?.FirstOrDefault();
                                }
                            }
                            else
                            {
                                questionGroup = FormQuestionGroup.Load(qg.ToInteger(), amsgs, adc);
                                questions = questionGroup.GetAllQuestions(amsgs, adc);

                                if (questionGroup.QuestionGroupDataTypeId == QuestionGroupDataType.NotSet)
                                {
                                    questionGroup.Name = Request.Params[$"name-{questionGroup.ID}"];
                                    questionGroup.LastModifiedBy = currentUser.ID;
                                    questionGroup.LastModifiedDate = DateTime.UtcNow;
                                    amsgs.AddRange(questionGroup.Save(adc));
                                }
                            }

                            // save existing questions
                            questions.ForEach(question =>
                            {
                                var text = Request.Params[$"text-{question.ID}"];
                                var desc = Request.Params[$"description-{question.ID}"];
                                var options = Request.Params[$"options-text-{question.ID}"];
                                var link = Request.Params[$"link-text-{question.ID}"];
                                var isActive = Request.Params[$"active-{question.ID}"].IsBoolean();
                                var isRequired = Request.Params[$"required-{question.ID}"].IsBoolean();
                                var isAnswerDescriptionEnabled =
                                    Request.Params[$"answerDescription-{question.ID}"].IsBoolean();

                                // this extra conditions are to prevent null entry in db from refresh-post calls
                                if (!question.IsLocked &&
                                    (text.IsNotEmpty() || desc.IsNotEmpty() || options.IsNotEmpty() ||
                                     link.IsNotEmpty() ||
                                     question.IsActive != isActive || question.IsRequired != isRequired ||
                                     question.IsAnswerDescriptionEnabled != isAnswerDescriptionEnabled))
                                {
                                    question.IsActive = isActive;
                                    question.IsRequired = isRequired;
                                    question.IsAnswerDescriptionEnabled = isAnswerDescriptionEnabled;

                                    if (question.QuestionDataTypeId == QuestionDataType.NotSet)
                                    {
                                        question.Text = text;
                                        question.Description = desc;
                                        question.Options = options;
                                        question.Link = link;
                                        question.DisplayOrder =
                                            Request.Params[$"displayOrderId-{question.ID}"].ToInteger();
                                        question.QuestionTypeId =
                                            (QuestionType) Request.Params[$"questiontype-{question.ID}"].ToInteger();
                                    }

                                    question.LastModifiedBy = currentUser.ID;
                                    question.LastModifiedDate = DateTime.UtcNow;
                                    amsgs.AddRange(question.Save(adc));
                                }
                            });

                            if (Request.Form["newquestionsIds"].Length > 0)
                            {
                                SaveNewQuestions(form.ID, currentUser.ID, questionGroup.ID, amsgs, adc);
                            }
                        }
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        msgs.Add("Form Saved", MessageType.Success);
                    },
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                );
            }

            questionGroups = form.GetQuestionGroups(msgs);
            individualGroups = questionGroups
                .Where(x => x.RegistrantTypeId == RegistrantType.Individual)
                .OrderBy(x => x.DisplayOrder)
                .ToList();

            //make sure only question groups with a setting of individual or organization are added to the final count
            questionGroups.Clear();
            questionGroups.AddRange(individualGroups);

            var currentIndex = -1;

            if (questionGroup.IsNotEmpty())
                currentIndex = questionGroups.FindIndex(x => x.ID.Value == questionGroup.ID.Value);

            switch (cmd)
            {
                case "prev":
                    if (currentIndex > 0) questionGroup = questionGroups[currentIndex - 1];
                    break;
                case "next":
                    if (currentIndex < questionGroups.Count - 1) questionGroup = questionGroups[currentIndex + 1];
                    break;
            }

            ViewBag.QuestionGroups = questionGroups;
            ViewBag.IndividualGroups = individualGroups;
            ViewBag.QuestionGroup = questionGroup;
            ViewBag.Messages = msgs;
            return View(form);
        }

        private void SaveNewQuestions(int? formId, int? currentUserId, int? formQuestionGroupId, SystemMessages msgs, DatabaseConnection dc)
        {
            var questionIds = Request.Form["newquestionsIds"];
            var questionsList = questionIds.Split(',');
            foreach (var question in questionsList)
            {
                var text = Request.Params[$"new-text-{question}"];

                var formQuestion = new FormQuestion
                {
                    FormId = formId,
                    CreateDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    CreatedBy = currentUserId,
                    LastModifiedBy = currentUserId,
                    DisplayOrder = Request.Params[$"new-sort-{question}"].ToInteger(),
                    FormQuestionGroupId = formQuestionGroupId,
                    IsTemplate = false,
                    QuestionDataTypeId = QuestionDataType.NotSet,
                    QuestionTypeId = (QuestionType) Request.Params[$"new-questiontype-{question}"].ToInteger(),
                    Text = text,
                    Options = Request.Params[$"new-options-text-{question}"],
                    Link = Request.Params[$"new-link-text-{question}"],
                    Description = Request.Params[$"new-description-{question}"],
                    IsActive = true,
                    IsDeleted = false,
                    IsRequired = Request.Params[$"new-required-{question}"].IsBoolean(),
                    IsAnswerDescriptionEnabled = Request.Params[$"new-description-enabled-{question}"].IsBoolean(),
                };
                msgs.AddRange(formQuestion.Save(dc));
            }
        }

        /***
         * NOTE: should be called DeleteFormQuestion
         */
        [HttpGet]
        public ActionResult Delete(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var questionToDelete = FormQuestion.Load(id.ToInteger(), msgs);
            questionToDelete.IsDeleted = true;
            msgs.AddRange(questionToDelete.Save());

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been deleted", questionToDelete.Text), MessageType.Success);
                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Deleted,
                    objectType: ObjectType.FormQuestion,
                    objectId: id.ToInteger(),
                    objectText: questionToDelete.Text,
                    indirectObjectType: ObjectType.FormQuestion,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            ViewBag.Messages = msgs;
            SetTempMessages(msgs);

            return RedirectToAction("FormEdit",
                new {id = questionToDelete.FormId, qg = questionToDelete.FormQuestionGroupId});
        }

        [HttpGet]
        public ActionResult DeleteSection(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var sectionToDelete = FormQuestionGroup.Load(id: id.ToInteger(), msgs: msgs);
            sectionToDelete.IsDeleted = true;
            msgs.AddRange(sectionToDelete.Save());

            //lets inactive all the questions for this section, so later when retrieving questions, we don't have to check for removed sections
            var questionsInThisSection =
                FormQuestion.LoadByFormQuestionGroupId(FormQuestionGroupId: sectionToDelete.ID.Value, msgs: msgs);
            foreach (var item in questionsInThisSection)
            {
                item.IsDeleted = true;
                msgs.AddRange(item.Save());
            }

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been deleted with all it's questions.", sectionToDelete.Name),
                    MessageType.Success);
                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Deleted,
                    objectType: ObjectType.FormQuestionGroup,
                    objectId: id.ToInteger(),
                    objectText: sectionToDelete.Name,
                    indirectObjectType: ObjectType.FormQuestionGroup,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            ViewBag.Messages = msgs;
            SetTempMessages(msgs);

            return RedirectToAction("FormEdit", new {id = sectionToDelete.FormId, qg = ""});
        }

        private AccountProfile StyleViewBag(SystemMessages msgs)
        {
            var account = SessionVariables.CurrentAccount;
            var profile = account.GetOrCreateAccountProfile(SessionVariables.CurrentUser, msgs);
            var awards = account.GetAccountProperties(AccountPropertyType.Award, msgs);
            var certifications = account.GetAccountProperties(AccountPropertyType.Certification, msgs);
            var photos = account.GetAccountProperties(AccountPropertyType.Photo, msgs);
            var classifications = account.GetAccountProperties(AccountPropertyType.Classifications, msgs);
            var softSkills = account.GetAccountProperties(AccountPropertyType.SoftSkills, msgs);
            var academicSubjects = account.GetAccountProperties(AccountPropertyType.AcademicSubjects, msgs);
            var teachingMethods = account.GetAccountProperties(AccountPropertyType.TeachingMethods, msgs);
            var servicesOffered = account.GetAccountProperties(AccountPropertyType.ServiceOffered, msgs);
            var timesOffered = account.GetAccountProperties(AccountPropertyType.TimeOffered, msgs);
            var pricingOptions = account.GetAccountProperties(AccountPropertyType.PricingOption, msgs);
            var specialNeedsOfferings = account.GetAccountProperties(AccountPropertyType.SpecialNeedsOffering, msgs);

            while (awards.Count < 5)
                awards.Add(new AccountProperty());

            while (certifications.Count < 5)
                certifications.Add(new AccountProperty());

            while (photos.Count < 6)
                photos.Add(new AccountProperty());

            var ah = Utilities.GetNewAmazonHelper();

            if (profile.Logo.IsNotBlank())
                ViewBag.LogoUrl = ah.GetFileLink(profile.Logo);

            if (profile.HeaderImage.IsNotBlank())
                ViewBag.HeaderImageUrl = ah.GetFileLink(profile.HeaderImage);

            photos.ForEach(photo => photo.Value = ah.GetFileLink(photo.Value));

            ViewBag.Account = account;
            ViewBag.Location = profile.GetLocation(msgs);

            ViewBag.Grades = GradeLevel.Load(msgs: msgs);
            ViewBag.GradeStart = profile.GetGradeLevelStart(msgs);
            ViewBag.GradeEnd = profile.GetGradeLevelEnd(msgs);
            ViewBag.AgeStart = profile.GetAgeStart();
            ViewBag.AgeEnd = profile.GetAgeEnd();
            ViewBag.Awards = awards;
            ViewBag.Certifications = certifications;
            ViewBag.Photos = photos;
            ViewBag.ServicesOffered = servicesOffered;
            ViewBag.TimesOffered = timesOffered;
            ViewBag.PricingOptions = pricingOptions;
            ViewBag.SpecialNeedsOfferings = specialNeedsOfferings;
            ViewBag.Classifications = classifications;
            ViewBag.SoftSkills = softSkills;
            ViewBag.AcademicSubjects = academicSubjects;
            ViewBag.TeachingMethods = teachingMethods;
            ViewBag.ProgramsAllDayDuringWorkWeek =
                profile.GetProperties(AccountPropertyType.ProgramsAllDayDuringWorkWeek, msgs);
            ViewBag.IsFullScreen = GetIsFullScreen();
            ViewBag.Messages = msgs;
            ViewBag.CostLow = profile.CostLow.HasValue ? profile.CostLow.Value.ToString("0.00") : "";
            ViewBag.CostHigh = profile.CostHigh.HasValue ? profile.CostHigh.Value.ToString("0.00") : "";
            ViewBag.IsIsypAccount = AccountService.CheckIfIsypAccount(account, msgs);

            return profile;
        }

        [HttpGet]
        public ActionResult Style()
        {
            return View(StyleViewBag(new SystemMessages()));
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Style(string id, string logo, string headerimage, string about, string title,
            string headline,
            string primaryColor, string collectregistration, string registrationurl, string buttontext, string website,
            string mingrade,
            string maxgrade, string agestart, string ageend, string restrictbygrade, string costlow, string costhigh,
            string twitter,
            string facebook, string instagram, string linkedin, string youtube, string calendar,
            string genderrestriction,
            string programCategories, string softskills, string academicsubjects, string teachingmethods,
            string programsalldayduringworkweek, string profileemail, string profilephone, string locationname,
            string streetnumber,
            string streetname, string address2, string neighborhood, string city, string state, string postalcode,
            string county,
            string country, string placeid, decimal? latitude, decimal? longitude, string opencovid,
            string elearningcenter,
            string virtualprogram, string ahqapproved, string videoUrl, IEnumerable<HttpPostedFileBase> photos)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var profile = account.GetAccountProfile(msgs);
            var allowedFiles = new string[] {".png", ".jpg", ".jpeg"};

            if (logo.IsNotEmpty())
            {
                if (Request.Files["logo"].ContentLength == 0 ||
                    !Request.Files["logo"].FileName.ToLower().Contains(allowedFiles) ||
                    Request.Files["logo"].ContentLength > Utilities.ImageUploadMaxBytes)
                    msgs.Add("Please upload a valid jpg or png file, must be less than 4MB", MessageType.UserError);
            }

            if (headerimage.IsNotEmpty())
            {
                if (Request.Files["headerimage"].ContentLength == 0 ||
                    !Request.Files["headerimage"].FileName.ToLower().Contains(allowedFiles) ||
                    Request.Files["headerimage"].ContentLength > Utilities.ImageUploadMaxBytes)
                    msgs.Add("Please upload a valid jpg or png file", MessageType.UserError);
            }

            if (collectregistration == "1")
            {
                registrationurl = "";
                buttontext = "";
            }

            if (website.IsNotEmpty() && !website.IsRegExMatch(BasicLibrary.Utilities.WebsiteRegEx))
                msgs.Add("Please enter a valid website address", MessageType.UserError);

            if (registrationurl.IsNotEmpty() && !registrationurl.IsRegExMatch(BasicLibrary.Utilities.WebsiteRegEx))
                msgs.Add("Please enter a valid registration url address", MessageType.UserError);

            if (facebook.IsNotEmpty() && !facebook.IsRegExMatch(BasicLibrary.Utilities.WebsiteRegEx))
                msgs.Add("Please enter a valid Facebook profile address", MessageType.UserError);

            if (linkedin.IsNotEmpty() && !linkedin.IsRegExMatch(BasicLibrary.Utilities.WebsiteRegEx))
                msgs.Add("Please enter a valid LinkedIn profile address", MessageType.UserError);

            if (youtube.IsNotEmpty() && !youtube.IsRegExMatch(BasicLibrary.Utilities.WebsiteRegEx))
                msgs.Add("Please enter a valid YouTube URL", MessageType.UserError);

            if (calendar.IsNotEmpty() && !calendar.IsRegExMatch(BasicLibrary.Utilities.WebsiteRegEx))
                msgs.Add("Please enter a valid calendar URL", MessageType.UserError);

            if (videoUrl.IsNotEmpty() && !videoUrl.IsRegExMatch(BasicLibrary.Utilities.ProfileVideoSpecificUrlRegEx))
                msgs.Add("Please enter a valid Video(YouTube) URL", MessageType.UserError);

            if (locationname.IsEmpty())
                msgs.Add("Location is a required field.", MessageType.UserError);

            if (latitude.IsEmpty() || longitude.IsEmpty())
                msgs.Add("An error occurred saving location data, please check what was entered in the location field",
                    MessageType.UserError);

            //server side validation for photos count, which should not be more then 6
            var properties = profile.GetProperties(AccountPropertyType.Photo, msgs);
            var savedPhotos = properties != null ? properties.Count : 0;
            var incomingPhotos = photos != null ? photos.Count(m => m != null) : 0;

            if (savedPhotos + incomingPhotos > 6)
                msgs.Add("Only 6 photos allowed to store in gallery.", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                if (logo.IsNotEmpty())
                    profile.UploadLogo(
                        account,
                        Request.Files["logo"].InputStream,
                        Request.Files["logo"].FileName,
                        currentUser,
                        msgs);

                if (headerimage.IsNotEmpty())
                    profile.UploadHeaderImage(
                        account,
                        Request.Files["headerimage"].InputStream,
                        Request.Files["headerimage"].FileName,
                        currentUser,
                        msgs);

                if (photos != null)
                {
                    foreach (var photo in photos)
                    {
                        if (photo.IsNotEmpty() && photo.FileName.IsNotBlank())
                        {
                            if (photo.FileName.ToLower().Contains(allowedFiles) &&
                                photo.ContentLength <= Utilities.ImageUploadMaxBytes)
                            {
                                profile.UploadGalleryPhoto(
                                    account,
                                    photo.InputStream,
                                    photo.FileName,
                                    photoId: "",
                                    currentUser,
                                    msgs);
                            }
                            else
                                msgs.Add("Please upload a valid jpg or png file that is less than 4MB",
                                    MessageType.UserError);
                        }
                    }
                }

                if (SessionVariables.AccountUserType == UserType.TechSupport)
                    ahqapproved = ahqapproved.IsNumber() ? "1" : "0";

                if (restrictbygrade.IsNotEmpty() && restrictbygrade.IsBoolean())
                {
                    if (restrictbygrade.ToBoolean())
                    {
                        agestart = "";
                        ageend = "";
                    }
                    else
                    {
                        mingrade = "";
                        maxgrade = "";
                    }
                }

                profile.UpdateProfile(
                    title,
                    headline,
                    about,
                    registrationurl,
                    buttontext,
                    website,
                    virtualprogram.IsNumber() && virtualprogram.ToBoolean(),
                    mingrade,
                    maxgrade,
                    agestart,
                    ageend,
                    costlow,
                    costhigh,
                    twitter,
                    facebook,
                    instagram,
                    linkedin,
                    youtube,
                    calendar,
                    genderrestriction,
                    profileemail,
                    profilephone,
                    Request.Params.Filter("award-id-"),
                    Request.Params.Filter("award-new-"),
                    Request.Params.Filter("certification-id-"),
                    Request.Params.Filter("certification-new-"),
                    Request.Params.Filter("services-offered-"),
                    Request.Params.Filter("times-offered-"),
                    Request.Params.Filter("pricing-options-"),
                    Request.Params.Filter("special-needs-"),
                    programCategories,
                    softskills,
                    academicsubjects,
                    teachingmethods,
                    programsalldayduringworkweek,
                    ahqapproved,
                    videoUrl,
                    currentUser,
                    msgs);

                profile.AddNewLocation(
                    locationname,
                    streetnumber,
                    streetname,
                    address2,
                    neighborhood,
                    city,
                    county,
                    state,
                    postalcode,
                    country,
                    placeid,
                    latitude.Value,
                    longitude.Value,
                    currentUser,
                    msgs);
            }

            if (msgs.HasNoErrors())
                msgs.Add("Saved Successfully", MessageType.Success);

            return View(StyleViewBag(msgs));
        }

        [HttpPost]
        public virtual JsonResult RemoveAccountProperty(int id)
        {
            try
            {
                if (id > 0)
                {
                    var msgs = new SystemMessages();
                    var account = SessionVariables.CurrentAccount;
                    var profile = account.GetAccountProfile(msgs);
                    profile.RemoveProperty(id, msgs);
                    return Json(new {IsSuccess = true});
                }

                //if we reach here, something gone wrong. 
                return Json(new {IsSuccess = false});
            }
            catch (Exception)
            {
                return Json(new {IsSuccess = false});
            }
        }

        [HttpGet]
        public ActionResult GetFeatures(int id)
        {
            var msgs = new SystemMessages();

            if (id.IsEmpty())
                msgs.Add("Package not found.", MessageType.UserError);

            var features = PackageFeature.LoadByPackageId(id, msgs);

            var model = PackageFeatureMapper.FilterAndMapPackageFeatures(features.Select(x => (x.FeatureTypeId, x.GetMaxValue())));

            if (model.Any())
                return Json(new {success = true, status = "ok", data = model}, JsonRequestBehavior.AllowGet);

            return Json(new {success = false, status = "error", data = msgs.ToList().Select(msg => new {msg.Text})},
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Billing()
        {
            var currentAccount = SessionVariables.CurrentAccount;
            ViewBag.Messages = new SystemMessages();;
            return View(currentAccount);
        }

        public ActionResult OAuthStripe()
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            //.. user is redirected from onbording
            //.. used session becuase stripe is not allowing multiple redirection at the moment
            var id = Session["activityUid"];

            if (Request["error"].IsEmpty())
                currentAccount.CompleteStripeConnect(Request["code"], SessionVariables.CurrentUser, msgs);
            else
                msgs.Add("It appears you have canceled your request to connect your Stripe account.",
                    MessageType.Notice);

            if (msgs.HasNoErrors())
            {
                if (id.IsNotEmpty())
                {
                    Session["activityUid"] = null;
                    TempData["IsStripeIntegrated"] = true;

                    var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

                    if (activity.IsNotEmpty() && activity.ShortHomePageUrl.IsNotEmpty())
                        return Redirect(activity.ShortHomePageUrl);
                    else
                        return Redirect(currentAccount.ShortHomePageUrl);
                }

                return RedirectToAction("paymentsdone",
                    new {source = Request["state"], isfullscreen = Request["isfullscreen"]});
            }

            ViewBag.Account = currentAccount;
            ViewBag.IsFullScreen = GetIsFullScreen();
            ViewBag.Messages = msgs;

            return View("PaymentsOptions");
        }

        public ActionResult DisconnectStripe()
        {
            var msgs = new SystemMessages();

            SessionVariables.CurrentAccount.DisconnectStripeConnect(SessionVariables.CurrentUser, msgs);

            ViewBag.Messages = msgs;

            return RedirectToAction("PaymentsOptions");
        }

        [HttpGet]
        public ActionResult SelectPlan()
        {
            var msgs = new SystemMessages();
            var currentPackage = SessionVariables.AccountPackage;
            var currentAccount = SessionVariables.CurrentAccount;
            
            HandleTempMessages(msgs);
            
            ViewBag.CurrentPackage = currentPackage;
            ViewBag.Packages = Package.LoadByIsActive(true, msgs);
            ViewBag.SetupIntentClientSecret = StripeHelper.GetStripeHelperSubscriberAccount().CreateSetupIntent(currentAccount.StripeCustomerId, msgs).ClientSecret;
            ViewBag.Messages = msgs;

            return View(currentAccount);
        }

        [HttpGet]
        public ActionResult DowngradeToFree(string newPlan)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var accountPackage = SessionVariables.AccountPackage;
            
            currentAccount.SwitchPackagesWithStripe
            (
                Package.LoadByUid(newPlan.ToGuid(), msgs),
                BillingPeriodType.Month,
                true,
                accountPackage.StripeSubscriptionId,
                currentUser,
                msgs
            );

            if (msgs.HasErrors())
            {
                SetTempMessages(msgs);
                return RedirectToAction("SelectPlan");
            }

            return RedirectToAction("SelectPlanDone");
        }

        public ActionResult SelectPlanPurchaseAfterTrialEnd(Guid newplan, BillingPeriodType period)
        {
            var msgs = new SystemMessages();
            HandleTempMessages(msgs);

            var currentAccount = SessionVariables.CurrentAccount;
            var currentAccountPackage = SessionVariables.AccountPackage;
            var newPackage = Package.LoadByUid(newplan, msgs);

            var session = StripeHelper.GetStripeHelperSubscriberAccount().GetCheckoutSession(
                currentAccount.StripeCustomerId,
                newPackage.GetStripePriceId(period),
                $"{Utilities.BaseUrl()}{Url.Action("AfterStripeUpdate", "Account", new {Area = "org"})}" +
                $"?newplan={newplan}&period={(int)period}&returnurl={Url.Action("SelectPlanDone", "Account", new {Area = "org"})}",
                $"{Utilities.BaseUrl()}/{Url.Action("SelectPlan", "Account", new {Area = "org"})}",
                currentAccount.ReferrerAccountId.HasValue ? StripeHelper.ReferralCouponId : null,
                msgs);

            // have to cancel the existing subscription in stripe or else it will auto charge on the existing trial
            var stripeSubscription = currentAccountPackage.GetStripeSubscription(msgs);
            if (stripeSubscription.IsNotEmpty() && !stripeSubscription.CanceledAt.HasValue)
                StripeHelper.GetStripeHelperSubscriberAccount().CancelSubscription(currentAccountPackage.StripeSubscriptionId, false, msgs);
            
            if (msgs.HasNoErrors() && session.IsNotEmpty())
                return new RedirectResult(session.Url);

            SetTempMessages(msgs);
            return RedirectToAction("SelectPlan");
        }

        [HttpGet]
        public ActionResult SelectPlanDone()
        {
            var msgs = new SystemMessages(); 
            ViewBag.CurrentPackage = SessionVariables.AccountPackage;
            ViewBag.Messages = msgs;
            return View(SessionVariables.CurrentAccount);
        }

        public ActionResult AfterStripeUpdate(string id, string newPlan, string period, string returnUrl)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var accountPackage = SessionVariables.AccountPackage;

            Database.ExecuteTransaction(
                action: (amsgs, adc) =>
                {
                    // if coming from select plan, need to potentially change subscription if changed on select plan screen
                    var newPlanSelected = newPlan.IsGuid() && period.IsInteger();
                    var isPlanDifferent = newPlanSelected && (accountPackage.PackageUid != newPlan.ToGuid() || accountPackage.BillingPeriodTypeId != (BillingPeriodType) period.ToInteger());
                    if (accountPackage.IsDuringTrial && newPlanSelected)
                    {
                        if (isPlanDifferent)
                            currentAccount.SwitchPackagesWithStripe
                            (
                                Package.LoadByUid(newPlan.ToGuid(), amsgs, adc),
                                (BillingPeriodType) period.ToInteger(),
                                true,
                                accountPackage.StripeSubscriptionId,
                                currentUser,
                                amsgs,
                                adc
                            );
                        else if (currentAccount.ReferrerAccountId.HasValue)
                            StripeHelper.GetStripeHelperSubscriberAccount().UpdateSubscription(accountPackage.StripeSubscriptionId, null,
                                null, null, null, null, StripeHelper.ReferralCouponId, amsgs);
                    }

                    // check if finally added payment
                    currentAccount.CheckIfSubscriberAccountHasPaymentMethod(null, amsgs, adc);
                    currentAccount.CheckIfSubscriberAccountSubscriptionChanged(false, currentUser, amsgs, adc);
                },
                onSuccess: (smsgs) => msgs.AddRange(smsgs),
                onFailure: (fmsgs) => msgs.AddRange(fmsgs),
                onException: (emsgs) => Utilities.SystemErrorLog(emsgs)
            );

            if (msgs.HasNoErrors() && returnUrl.IsNotEmpty())
                return new RedirectResult(returnUrl);
            
            if (msgs.HasErrors())
                SetTempMessages(msgs);

            return RedirectToAction("Billing");
        }

        public static class PackageFeatureMapper
        {
            public static IEnumerable<PackageFeaturesPlanListing> FilterAndMapPackageFeatures(IEnumerable<(FeatureType type, decimal value)> features)
            {
                var featureTypesNotToIncludeInFeatureListing = new List<FeatureType>
                {
                    FeatureType.NotSet,
                    FeatureType.CustomUrl,
                    FeatureType.ManageExternalLeagues,
                    FeatureType.ManageLeagues,
                    FeatureType.ManageLocations,
                    FeatureType.Participants,
                    FeatureType.PaymentsPercentageFee,
                    FeatureType.PaymentsPercentageFlatFee,
                    FeatureType.TrialDays,
                    FeatureType.UseStripeConnectedAccount,
                    FeatureType.ViewInsights,
                };

                bool FeatureTypeHasNotBeenDeprecated(FeatureType featureType)
                {
                    return Enum.GetName(typeof(FeatureType), featureType) != null;
                }

                return features.Where(x => x.type.NotIn(featureTypesNotToIncludeInFeatureListing))
                    .Where(x => FeatureTypeHasNotBeenDeprecated(x.type))
                    .Select(x => new PackageFeaturesPlanListing((Enum.GetName(typeof(FeatureType), x.type)).ToString().SplitByUpperCase(), x.value));
            }
        }

        public class PackageFeaturesPlanListing
        {
            public PackageFeaturesPlanListing(string type, decimal value)
            {
                this.type = type;
                this.value = value;
            }

            public string type { get; }
            public decimal value { get; }
        }
        
    }
}
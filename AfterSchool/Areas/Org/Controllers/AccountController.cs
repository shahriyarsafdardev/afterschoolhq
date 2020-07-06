using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;
using AfterSchool.Models;
using AfterSchoolLibrary.Services;
using Hangfire;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Billing;

namespace AfterSchool.Areas.Org.Controllers
{
    public class AccountController : BaseLoggedInController
    {
        public AccountController() : base(new List<AccountType> { AccountType.School, AccountType.ActivityProvider })
        {

        }

        private bool GetIsFullScreen()
        {
            return Request["isnew"] == "1" || Request["isfullscreen"] == "1";
        }

        [HttpGet]
        public ActionResult GetStartedPayments(string id)
        {
            ViewBag.Messages = new SystemMessages();
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
        public ActionResult Edit(string id, string name, string email, string phone, string uniquename, string isactive, string timezone)
        {
            var msgs = new SystemMessages();
            var a = SessionVariables.CurrentAccount;                                

            if (name.IsEmpty())
                msgs.Add("Organization Name is required", MessageType.UserError);

            a.Name = name;
            a.Phone = phone;
            a.UniqueName = uniquename;
            a.TimeZone = timezone;
            a.Email = email;

            if (msgs.HasNoErrors())
            {
                if (a.UniqueName.IsEmpty() && uniquename.IsEmpty())
                    a.UniqueName = AccountService.GenerateUniqueName(name, a.ID);
                else
                    a.UniqueName = AccountService.GenerateUniqueName(uniquename, a.ID);

                a.LastModifiedDate = DateTime.UtcNow;
                a.LastModifiedBy = SessionVariables.CurrentUser.ID;
                msgs.AddRange(a.SaveWithStripe());

                if (msgs.HasNoErrors())
                    msgs.Add("Saved Successfully", MessageType.Success);
            }

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(a);
        }

        [HttpGet]
        public ActionResult Location()
        {
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Messages = new SystemMessages();
            ViewBag.Location = SessionVariables.CurrentAccount.GetDefaultLocation();
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(SessionVariables.CurrentAccount);
        }

        [HttpPost]
        public ActionResult Location(string id, string location, string locationname, string address1, string address2, string city, string state, string postalcode, string country, string mylocation, string locationtype)
        {
            var msgs = new SystemMessages();
            var a = SessionVariables.CurrentAccount;

            var locationType = LocationType.NotSet;
            var hostAccountId = new int?();

            if (locationtype.IsNumber())
                locationType = (LocationType)locationtype.ToInteger();

            if (mylocation.IsBoolean() && mylocation.ToBoolean())
                hostAccountId = SessionVariables.CurrentAccount.ID;

            if (location.IsNumber())
            {
                var currentLocation = AfterSchoolLibrary.Models.Location.Load(location.ToInteger(), msgs);

                if (currentLocation.IsOwnedByOrIsNotOwnedAtAll(SessionVariables.CurrentAccount))
                {
                    currentLocation.LocationTypeId = locationType;
                    currentLocation.HostAccountId = hostAccountId;
                    currentLocation.LastModifiedBy = SessionVariables.CurrentUser.ID;
                    currentLocation.LastModifiedDate = DateTime.UtcNow;
                    msgs.AddRange(currentLocation.Save());
                }

                SessionVariables.CurrentAccount.SetDefaultLocation(location.ToInteger(), SessionVariables.CurrentUser.ID.Value);
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
                    visitorAccount: SessionVariables.CurrentAccount,
                    loggedInUser: SessionVariables.CurrentUser,
                    msgs: msgs
                    );

                if (msgs.HasNoErrors())
                    SessionVariables.CurrentAccount.SetDefaultLocation(newLocation.ID.Value, SessionVariables.CurrentUser.ID.Value);
            }            

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Location = SessionVariables.CurrentAccount.GetDefaultLocation();
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(a);
        }     

        [HttpGet]
        public ActionResult PaymentsOptions()
        {
            var account = SessionVariables.CurrentAccount;

            // if (account.HasStripeConnectedAccount && account.StripePayoutsEnabled)
            //     return RedirectToAction("paymentsentity");

            ViewBag.Account = account;
            ViewBag.IsFullScreen = GetIsFullScreen();
            ViewBag.Messages = new SystemMessages();
            return View();
        }

        [HttpGet]
        public ActionResult CheckSetup()
        {
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.IsFullScreen = GetIsFullScreen();
            ViewBag.Messages = new SystemMessages();
            return View(SessionVariables.CurrentAccount.GetCheckPaymentAccount() ?? new CheckPaymentAccount());
        }

        [HttpPost]
        public ActionResult CheckSetup(string entitytype, string name, string address1, string address2, string city, string state, string postalcode)
        {
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
                SessionVariables.CurrentAccount.CreateOrUpdateCheckPaymentAccount(
                    entitytype.ToEnum<LegalEntityType>(),
                    name,
                    address1,
                    address2,
                    city,
                    state,
                    postalcode,
                    SessionVariables.CurrentUser,
                    msgs);

            if (msgs.HasNoErrors() && Request["isnew"] == "1")
                return RedirectToAction("paymentsdone", new { isnew = 1 });

            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.IsFullScreen = GetIsFullScreen();
            ViewBag.Messages = msgs;
            return View(SessionVariables.CurrentAccount.GetCheckPaymentAccount() ?? new CheckPaymentAccount());
        }


        [HttpGet]
        public ActionResult BankSetup()
        {            
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.IsFullScreen = GetIsFullScreen();
            ViewBag.Messages = new SystemMessages();
            return View(BankInfoViewModel.FromBankPaymentAccount(SessionVariables.CurrentAccount.GetBankPaymentAccount() ?? new BankPaymentAccount()));
        }

        [HttpPost]
        public ActionResult BankSetup(BankInfoViewModel bankInfoVM)
        {
            var msgs = new SystemMessages();

            if (!bankInfoVM.BankAccountType.HasValue || bankInfoVM.BankAccountType.Value == BankAccountType.NotSet)
                msgs.Add("Select the bank account type", MessageType.UserError);

            if (bankInfoVM.BankName.IsEmpty())
                msgs.Add("Bank Name is required", MessageType.UserError);

            if (bankInfoVM.AccountHoldersName.IsEmpty())
                msgs.Add("Account holder's name is required", MessageType.UserError);

            if (bankInfoVM.RoutingNumber1.IsEmpty() || bankInfoVM.RoutingNumber1.Trim().Length != 9)
                msgs.Add("Routing Number 1 is required", MessageType.UserError);

            if (bankInfoVM.RoutingNumber2.IsEmpty() || bankInfoVM.RoutingNumber2.Trim().Length != 9)
                msgs.Add("Routing Number 2 is required", MessageType.UserError);

            if (bankInfoVM.RoutingNumber1 != bankInfoVM.RoutingNumber2)
                msgs.Add("Routing Numbers must match", MessageType.UserError);

            if (bankInfoVM.AccountNumber1.IsEmpty())
                msgs.Add("Account Number 1 is required", MessageType.UserError);

            if (bankInfoVM.AccountNumber2.IsEmpty())
                msgs.Add("Account Number 2 is required", MessageType.UserError);

            if (bankInfoVM.AccountNumber1 != bankInfoVM.AccountNumber2)
                msgs.Add("Account Numbers must match", MessageType.UserError);

            if (bankInfoVM.BankCountry.IsEmpty())
                msgs.Add("Bank Country is required", MessageType.UserError);

            if (msgs.HasNoErrors())
                SessionVariables.CurrentAccount.CreateOrUpdateBankPaymentAccount(
                    bankInfoVM.BankName,
                    bankInfoVM.BankAccountType.Value,
                    bankInfoVM.RoutingNumber1,
                    bankInfoVM.AccountNumber1,
                    bankInfoVM.AccountHoldersName,
                    bankInfoVM.BankCountry,
                    SessionVariables.CurrentUser,
                    msgs);

            if (msgs.HasNoErrors() && Request["isnew"] == "1")
                return RedirectToAction("paymentsdone", new { isnew = 1 });

            if (msgs.HasNoErrors())
                bankInfoVM = BankInfoViewModel.FromBankPaymentAccount(SessionVariables.CurrentAccount.GetBankPaymentAccount() ?? new BankPaymentAccount());

            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.IsFullScreen = GetIsFullScreen();
            ViewBag.Messages = msgs;
            return View(bankInfoVM);
        }

        [HttpGet]
        public ActionResult PaymentsEntity()
        {
            var msgs = new SystemMessages();
            var paymentsEntityVM = PaymentsEntityViewModel.LoadFromStripeAccount(SessionVariables.CurrentAccount.GetStripeCustomAccount(msgs));

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(paymentsEntityVM);
        }

        private void ValidateFile(System.Web.HttpPostedFileBase file, SystemMessages msgs)
        {
            if (file.IsEmpty())
                return;

            if (file.ContentLength == 0 || (file.ContentType != "image/jpeg" && file.ContentType != "image/png"))
                msgs.Add("A valid Driver's License or Passport is required as either a jpg or a png", MessageType.UserError);
            else if (ByteSizeLib.ByteSize.FromBytes(file.ContentLength).MegaBytes > 8)
                msgs.Add("Your document must be less than 8 MBs", MessageType.UserError);
        }

        [HttpPost]
        public ActionResult PaymentsEntity(PaymentsEntityViewModel paymentsEntityVM)
        {
            var msgs = new SystemMessages();
            var stripeAccount = SessionVariables.CurrentAccount.GetStripeCustomAccount(msgs);

            if (paymentsEntityVM.BusinessType == LegalEntityType.NotSet)
                msgs.Add("Please select your business type", MessageType.UserError);

            if (paymentsEntityVM.BusinessType == LegalEntityType.Company)
            {
                if (paymentsEntityVM.BusinessName.IsEmpty())
                    msgs.Add("Legal Business Name is required", MessageType.UserError);

                if (paymentsEntityVM.EIN.IsEmpty())
                    msgs.Add("Employer Identification Number is required", MessageType.UserError);

                if (paymentsEntityVM.BusinessPhone.IsEmpty())
                    msgs.Add("Business Phone is required", MessageType.UserError);
            }

            if (paymentsEntityVM.FirstName.IsEmpty())
                msgs.Add("First Name is required", MessageType.UserError);

            if (paymentsEntityVM.LastName.IsEmpty())
                msgs.Add("Last Name is required", MessageType.UserError);

            if (!paymentsEntityVM.DateOfBirth.HasValue)
                msgs.Add("Date of Birth is required and must be a valid date", MessageType.UserError);

            if (paymentsEntityVM.SSNLast4.IsEmpty() || !paymentsEntityVM.SSNLast4.IsNumber())
                msgs.Add("SSN(last 4) is required", MessageType.UserError);            

            if (paymentsEntityVM.PersonalEmail.IsEmpty())
                msgs.Add("Email is required", MessageType.UserError);

            if (paymentsEntityVM.PersonalPhone.IsEmpty())
                msgs.Add("Phone is required", MessageType.UserError);

            if (paymentsEntityVM.Address1.IsEmpty())
                msgs.Add("Address 1 is required", MessageType.UserError);

            if (paymentsEntityVM.City.IsEmpty())
                msgs.Add("City is required", MessageType.UserError);

            if (paymentsEntityVM.State.IsEmpty())
                msgs.Add("State is required", MessageType.UserError);

            if (paymentsEntityVM.PostalCode.IsEmpty())
                msgs.Add("Postal Code is required", MessageType.UserError);

            if (!paymentsEntityVM.TOSAccept)
                msgs.Add("You must agree to the terms of service", MessageType.UserError);

            if (paymentsEntityVM.BusinessUrl.IsEmpty() && paymentsEntityVM.ProductDescription.IsEmpty())
                msgs.Add("You must enter either a website or a Service Description", MessageType.UserError);

            if (PaymentsEntityViewModel.LoadFromStripeAccount(stripeAccount).RequiresFullSSN.IsTrue() && paymentsEntityVM.SSN.IsEmpty())
                msgs.Add("SSN is required", MessageType.UserError);

            if (PaymentsEntityViewModel.LoadFromStripeAccount(stripeAccount).RequiresDocument.IsTrue())
            {
                if (Request.Files.Count < 2)
                    msgs.Add("Please upload the front and back of your identification", MessageType.UserError);

                ValidateFile(Request.Files[0], msgs);
                ValidateFile(Request.Files[1], msgs);
            }

            if (paymentsEntityVM.TOSAccept)
            {
                paymentsEntityVM.TOSAcceptanceDate = DateTime.UtcNow;
                paymentsEntityVM.TOSAcceptanceIP = Request.UserHostAddress;
                paymentsEntityVM.TOSAcceptanceUserAgent = Request.UserAgent;
            }

            if (msgs.HasNoErrors())
            {
                if (PaymentsEntityViewModel.LoadFromStripeAccount(stripeAccount).RequiresDocument.IsTrue())
                {
                    var documentFront = StripeHelper.UploadFile(
                        Request.Files[0].InputStream,
                        Request.Files[0].FileName,
                        StripeFilePurpose.IdentityDocument,
                        msgs);

                    var documentBack = StripeHelper.UploadFile(
                        Request.Files[1].InputStream,
                        Request.Files[1].FileName,
                        StripeFilePurpose.IdentityDocument,
                        msgs);

                    paymentsEntityVM.DocumentFrontId = documentFront.Id;
                    paymentsEntityVM.DocumentBackId = documentBack.Id;
                }

                if (SessionVariables.CurrentAccount.StripeUserId.IsEmpty())
                    SessionVariables.CurrentAccount.CreateStripeCustomAccount(
                        paymentsEntityVM.GetStripeAccountCreateOptions(SessionVariables.CurrentAccount),
                        msgs);
                else
                    SessionVariables.CurrentAccount.UpdateStripeCustomAccount(
                        paymentsEntityVM.GetStripeAccountUpdateOptions(SessionVariables.CurrentAccount),
                        msgs);
            }

            if (msgs.HasNoErrors())
            {
                msgs.Add("Your update information has been submitted.", MessageType.Success);

                //if (startingStatus != stripeAccount.VerificationStatusId && stripeAccount.VerificationStatusId == StripeVerificationStatus.Verified)
                //    BackgroundJob.Enqueue(() => SessionVariables.CurrentAccount.SendStripeAccountVerified(null));

                if (paymentsEntityVM.BusinessType == LegalEntityType.Company && stripeAccount.ChargesEnabled)
                    return RedirectToAction("paymentsbank", "account", new { isnew = Request["isnew"] });
            }
           
            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(PaymentsEntityViewModel.LoadFromStripeAccount(SessionVariables.CurrentAccount.GetStripeCustomAccount(msgs)));
        }

        [HttpGet]
        public ActionResult PaymentsBank()
        {
            var msgs = new SystemMessages();
            var paymentsEntityVM = PaymentsEntityViewModel.LoadFromStripeAccount(SessionVariables.CurrentAccount.GetStripeCustomAccount(msgs));

            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.Messages = msgs;
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(paymentsEntityVM);
        }

        [HttpPost]
        public ActionResult PaymentsBank(PaymentsEntityViewModel paymentsEntityVM)
        {
            var msgs = new SystemMessages();
            
            if (paymentsEntityVM.BankRoutingNumber1.IsEmpty())
                msgs.Add("Routing Number is required", MessageType.UserError);
            if (paymentsEntityVM.BankRoutingNumber2.IsEmpty())
                msgs.Add("Confirm Routing Number is required", MessageType.UserError);
            if (paymentsEntityVM.BankRoutingNumber1 != paymentsEntityVM.BankRoutingNumber2)
                msgs.Add("Routing Numbers must match", MessageType.UserError);

            if (paymentsEntityVM.BankAccountNumber1.IsEmpty())
                msgs.Add("Account Number is required", MessageType.UserError);
            if (paymentsEntityVM.BankAccountNumber2.IsEmpty())
                msgs.Add("Confirm Account Number is required", MessageType.UserError);
            if (paymentsEntityVM.BankAccountNumber1 != paymentsEntityVM.BankAccountNumber2)
                msgs.Add("Account Numbers must match", MessageType.UserError);

            if (msgs.HasNoErrors())
                SessionVariables.CurrentAccount.UpdateStripeCustomAccount(
                        paymentsEntityVM.GetStripeAccountUpdateOptions(SessionVariables.CurrentAccount),
                        msgs);

            if (msgs.HasNoErrors() && Request["isnew"] == "1")
                return RedirectToAction("PaymentsDone");

            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.Messages = msgs;
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(paymentsEntityVM);
        }

        [HttpGet]
        public ActionResult PaymentsDone(string id)
        {
            var msgs = new SystemMessages();

            ViewBag.HasActivities = SessionVariables.CurrentAccount.HasActivities(msgs);
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.Messages = msgs;
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View();
        }

        [HttpGet]
        public ActionResult Features()
        {
            Account a = SessionVariables.CurrentAccount;
            ViewBag.Messages = new SystemMessages();
            ViewBag.Features = a.GetAccountPackageFeatures();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            return View(a);
        }

        [HttpPost]
        public ActionResult Features(string x)
        {
            var msgs = new SystemMessages();
            var a = SessionVariables.CurrentAccount;
            var accountPackage = a.GetActiveAccountPackage();

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) => {
                    foreach (FeatureType type in System.Enum.GetValues(typeof(FeatureType)))
                    {
                        if (Request[type.ToString()].IsNotEmpty())
                        {
                            var accountFeature = AccountPackageFeature.LoadByAccountPackageIdByFeatureTypeId(accountPackage.ID.Value, type, dc: adc);
                            if (accountFeature.IsEmpty())
                            {
                                accountFeature = new AccountPackageFeature();
                                accountFeature.AccountPackageId = accountPackage.ID;
                                accountFeature.CreateDate = DateTime.UtcNow;
                                accountFeature.CreatedBy = SessionVariables.CurrentUser.ID;
                                accountFeature.FeatureTypeId = type;
                            }

                            accountFeature.Max = Request[type.ToString()].ToDecimal();
                            accountFeature.LastModifiedBy = SessionVariables.CurrentUser.ID;
                            accountFeature.LastModifiedDate = DateTime.UtcNow;
                            msgs.AddRange(accountFeature.Save(adc));

                            if (accountFeature.FeatureTypeId == FeatureType.TrialDays && accountPackage.TrialStartDate.HasValue)
                                amsgs.AddRange(accountPackage.UpdateTrialDays(accountFeature.Max.Value.ToInteger(), adc));
                        }
                    }
                },
                onSuccess: (SystemMessages smsgs) => {
                    msgs.AddRange(smsgs);
                    msgs.Add("Saved Successfully", MessageType.Success);
                    SessionVariables.AccountPackage = a.GetActiveAccountPackage();
                },
                onFailure: (SystemMessages fmsgs) => {
                    msgs.AddRange(fmsgs);
                }
            );            

            ViewBag.Messages = msgs;
            ViewBag.Features = a.GetAccountPackageFeatures();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            return View(a);
        }

        [HttpGet]
        public ActionResult Team()
        {
            var msgs = new SystemMessages();
            var members = SessionVariables.CurrentAccount.GetUsers(msgs: msgs);

            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Members = members;
            ViewBag.NeedToUpgrade = members.Count >= SessionVariables.AccountPackage.GetFeature(FeatureType.TeamAccounts, msgs).GetMaxValue();
            ViewBag.Messages = msgs;
            return View(SessionVariables.CurrentAccount);
        }

        [HttpGet]
        public ActionResult TeamInvite()
        {
            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            return View(SessionVariables.CurrentAccount);
        }

        [HttpPost]
        public ActionResult TeamInvite(string usertype, string emails, string message)
        {
            SystemMessages msgs = new SystemMessages();

            if (usertype.IsEmpty())
                msgs.Add("Account type is required", MessageType.UserError);

            if (emails.IsEmpty())
                msgs.Add("Please enter at least one email address", MessageType.UserError);
            else
                emails.Split(',')
                    .Select(x => x.Trim())
                    .Where(email => !email.IsMatch(BasicLibrary.Utilities.EmailRegEx))
                    .ToList()
                    .ForEach(email => msgs.Add(string.Format("{0} is not a valid email address", email), MessageType.UserError));

            if (msgs.HasNoErrors())
                emails.Split(',')
                    .Select(x => x.Trim())
                    .ToList()
                    .ForEach(email => 
                        SessionVariables.CurrentAccount.InviteUser(
                            "", 
                            "", 
                            email, 
                            "",
                            Request.Params["message"], 
                            (UserType)usertype.ToInteger(), 
                            SessionVariables.CurrentUser,
                            msgs)
                     );

            if (msgs.HasNoErrors())
                msgs.Add("All invites have been sent", MessageType.Success);

            var members = SessionVariables.CurrentAccount.GetUsers(msgs: msgs);
            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Members = members;
            ViewBag.NeedToUpgrade = members.Count >= SessionVariables.AccountPackage.GetFeature(FeatureType.TeamAccounts).GetMaxValue();
            return View("Team", SessionVariables.CurrentAccount);
        }

        [HttpGet]
        public ActionResult Visitors()
        {
            var msgs = new SystemMessages();

            ViewBag.RequestingVisitors = SessionVariables.CurrentAccount.GetRequestedVisitorAccounts(null, msgs);
            ViewBag.ApprovedVisitors = SessionVariables.CurrentAccount.GetApprovedVisitorAccounts(msgs);
            ViewBag.Messages = msgs;

            return View(SessionVariables.CurrentAccount);
        }

        [HttpGet]
        public ActionResult ApproveVisitor(string id)
        {
            var msgs = new SystemMessages();
            var accountRelationship = AccountRelationship.Load(id.ToInteger(), msgs);
            msgs.AddRange(accountRelationship.UpdateApprovalStatus(ApprovalStatus.Approved, SessionVariables.CurrentUser));

            return RedirectToAction("visitors");
        }

        [HttpGet]
        public ActionResult DenyVisitor(string id)
        {
            var msgs = new SystemMessages();
            var accountRelationship = AccountRelationship.Load(id.ToInteger(), msgs);
            msgs.AddRange(accountRelationship.UpdateApprovalStatus(ApprovalStatus.Denied, SessionVariables.CurrentUser));

            return RedirectToAction("visitors");
        }

        [HttpGet]
        public ActionResult Forms()
        {
            ViewBag.Messages = new SystemMessages();
            return View(SessionVariables.CurrentAccount.GetForms());
        }

        [HttpGet]
        public ActionResult FormEdit(string id, string qg)
        {
            var form = Form.Load(id.ToInteger());
            var questionGroups = form.GetQuestionGroups();
            var individualGroups = questionGroups
                .Where(x => x.RegistrantTypeId == RegistrantType.Individual)
                .OrderBy(x => x.DisplayOrder).ToList();
            var organizationGroups = questionGroups
                .Where(x => x.RegistrantTypeId == RegistrantType.Organization)
                .OrderBy(x => x.DisplayOrder).ToList();

            //temporarily disable organization groups
            organizationGroups = new List<FormQuestionGroup>();

            //make sure only question groups with a setting of individual or organization are added to the final count
            questionGroups.Clear();
            questionGroups.AddRange(individualGroups);
            questionGroups.AddRange(organizationGroups);

            FormQuestionGroup questionGroup = null;

            if (qg.IsNumber())
            {
                if (qg.ToInteger() == 0)
                    questionGroup = new FormQuestionGroup()
                    {
                        QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
                        FormId = form.ID,
                        RegistrantTypeId = RegistrantType.Individual,
                        ID = 0
                    };
                else
                    questionGroup = FormQuestionGroup.Load(qg.ToInteger());
            }
            else
            {
                questionGroup = null;
            }

            ViewBag.Messages = new SystemMessages();
            ViewBag.QuestionGroups = questionGroups;
            ViewBag.IndividualGroups = individualGroups;
            ViewBag.OrganizationGroups = organizationGroups;
            ViewBag.QuestionGroup = questionGroup;
            return View(form);
        }

        [HttpPost]
        public ActionResult FormEdit(string id, string copyfrom, string qg, string name, string description, string cmd)
        {
            var msgs = new SystemMessages();
            Form form = null;
            FormQuestionGroup questionGroup = null;

            if (name.IsEmpty() && qg.IsBlank())
                msgs.Add("Name is required", MessageType.UserError);

            if (description.IsEmpty() && qg.IsBlank() && false)
                msgs.Add("Description is required", MessageType.UserError);


            if (id.ToInteger() == 0)
            {
                if (copyfrom.IsNumber())
                    form = Form.Load(copyfrom.ToInteger(), msgs)
                        .Copy(SessionVariables.CurrentUser.ID.Value, SessionVariables.CurrentAccount.ID.Value, null, msgs);
                else
                    form = SessionVariables.CurrentAccount
                        .GetDefaultForm(msgs)
                        .Copy(SessionVariables.CurrentUser.ID.Value, SessionVariables.CurrentAccount.ID.Value, null, msgs);
            }
            else
            {
                form = Form.Load(id.ToInteger(), msgs);
            }


            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        if (name.IsNotBlank()) form.Name = name;
                        if (description.IsNotBlank()) form.Description = description;

                        amsgs.AddRange(form.Save(adc));

                        var questions = new List<FormQuestion>();

                        if (qg.IsNumber())
                        {
                            // create or update question groups
                            if (qg.ToInteger() == 0)
                            {
                                questionGroup = new FormQuestionGroup()
                                {
                                    CreateDate = DateTime.UtcNow,
                                    CreatedBy = SessionVariables.CurrentUser.ID,
                                    LastModifiedDate = DateTime.UtcNow,
                                    LastModifiedBy = SessionVariables.CurrentUser.ID,
                                    DisplayOrder = form.GetQuestionGroupCount(amsgs, adc) + 1,
                                    FormId = form.ID,
                                    QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,                                   
                                    RegistrantTypeId = RegistrantType.Individual,
                                    Name = Request.Params["name-0"]
                                };
                                amsgs.AddRange(questionGroup.Save(adc));
                                questions = new List<FormQuestion>();
                            }
                            else
                            {
                                questionGroup = FormQuestionGroup.Load(qg.ToInteger(), amsgs, adc);
                                questions = questionGroup.GetAllQuestions(amsgs, adc);

                                if (questionGroup.QuestionGroupDataTypeId == QuestionGroupDataType.NotSet)
                                {
                                    questionGroup.Name = Request.Params[$"name-{questionGroup.ID}"];
                                    questionGroup.LastModifiedBy = SessionVariables.CurrentUser.ID;
                                    questionGroup.LastModifiedDate = DateTime.UtcNow;
                                    amsgs.AddRange(questionGroup.Save(adc));
                                }
                            }

                            // save existing questions
                            questions.ForEach(question =>
                            {
                                if (!question.IsLocked)
                                {
                                    question.IsActive = Request.Params[$"active-{question.ID}"].IsBoolean();
                                    question.IsRequired = Request.Params[$"required-{question.ID}"].IsBoolean();

                                    if (question.QuestionDataTypeId == QuestionDataType.NotSet)
                                    {
                                        question.Text = Request.Params[$"text-{question.ID}"];
                                        // for Text Block
                                        question.Description = Request.Params[$"description-{question.ID}"];
                                        question.Options = Request.Params[$"options-text-{question.ID}"];
                                        question.Link = Request.Params[$"link-text-{question.ID}"];

                                        question.QuestionTypeId = (QuestionType)Request.Params[$"questiontype-{question.ID}"].ToInteger();

                                    }

                                    question.LastModifiedBy = SessionVariables.CurrentUser.ID;
                                    question.LastModifiedDate = DateTime.UtcNow;
                                    amsgs.AddRange(question.Save(adc));
                                }

                            });

                            // save new questions
                            if (Request.Form["newquestioncount"].IsNumber() && Request.Form["newquestioncount"].ToInteger() > 0)
                            {
                                var questionCount = Request.Form["newquestioncount"].ToInteger();

                                for (var i = 0; i < questionCount; i++)
                                {
                                    var question = new FormQuestion
                                    {
                                        FormId = form.ID,
                                        CreateDate = DateTime.UtcNow,
                                        LastModifiedDate = DateTime.UtcNow,
                                        CreatedBy = SessionVariables.CurrentUser.ID,
                                        LastModifiedBy = SessionVariables.CurrentUser.ID,
                                        DisplayOrder = questionGroup.GetQuestionCount(amsgs, adc) + 1,
                                        FormQuestionGroupId = questionGroup.ID,
                                        IsTemplate = false,
                                        QuestionDataTypeId = QuestionDataType.NotSet,
                                        QuestionTypeId = (QuestionType)Request.Params[string.Format("new-questiontype-{0}", i)].ToInteger(),
                                        Text = Request.Params[string.Format("new-text-{0}", i)],
                                        Options = Request.Params[string.Format("new-options-text-{0}", i)],
                                        Link = Request.Params[string.Format("new-link-text-{0}", i)],
                                        Description = Request.Params[string.Format("new-description-{0}", i)],
                                        IsActive = Request.Params[string.Format("new-active-{0}", i)].IsBoolean(),
                                        IsRequired = Request.Params[string.Format("new-required-{0}", i)].IsBoolean()
                                    };
                                    amsgs.AddRange(question.Save(adc));
                                }
                            }
                        }
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        msgs.Add("Form Saved", MessageType.Success);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
            }

            var questionGroups = form.GetQuestionGroups();
            var individualGroups = questionGroups
                .Where(x => x.RegistrantTypeId == RegistrantType.Individual)
                .OrderBy(x => x.DisplayOrder)
                .ToList();
            var organizationGroups = questionGroups
                .Where(x => x.RegistrantTypeId == RegistrantType.Organization)
                .OrderBy(x => x.DisplayOrder)
                .ToList();

            //temporarily disable organization groups
            organizationGroups = new List<FormQuestionGroup>();

            //make sure only question groups with a setting of individual or organization are added to the final count
            questionGroups.Clear();
            questionGroups.AddRange(individualGroups);
            questionGroups.AddRange(organizationGroups);

            var currentIndex = -1;

            if (questionGroup.IsNotEmpty())
            {
                currentIndex = questionGroups.FindIndex(x => x.ID.Value == questionGroup.ID.Value);
            }

            switch (cmd)
            {
                case "prev":
                    if (currentIndex > 0) questionGroup = questionGroups[currentIndex - 1];
                    break;
                case "next":
                    if (currentIndex < questionGroups.Count - 1) questionGroup = questionGroups[currentIndex + 1];
                    break;
            }

            ViewBag.Messages = msgs;
            ViewBag.QuestionGroups = questionGroups;
            ViewBag.IndividualGroups = individualGroups;
            ViewBag.OrganizationGroups = organizationGroups;
            ViewBag.QuestionGroup = questionGroup;
            return View(form);
        }

        private AccountProfile StyleViewBag(SystemMessages msgs)
        {
            var profile = SessionVariables.CurrentAccount.GetOrCreateAccountProfile(SessionVariables.LoggedInUser, msgs);

            var awards = SessionVariables.CurrentAccount.GetAccountProperties(AccountPropertyType.Award, msgs);
            var certifications = SessionVariables.CurrentAccount.GetAccountProperties(AccountPropertyType.Certification, msgs);
            var photos = SessionVariables.CurrentAccount.GetAccountProperties(AccountPropertyType.Photo, msgs);
            var classifications = SessionVariables.CurrentAccount.GetAccountProperties(AccountPropertyType.Classifications);
            var softSkills = SessionVariables.CurrentAccount.GetAccountProperties(AccountPropertyType.SoftSkills);
            var academicSubjects = SessionVariables.CurrentAccount.GetAccountProperties(AccountPropertyType.AcademicSubjects);
            var teachingMethods = SessionVariables.CurrentAccount.GetAccountProperties(AccountPropertyType.TeachingMethods);
            var servicesOffered = SessionVariables.CurrentAccount.GetAccountProperties(AccountPropertyType.ServiceOffered);
            var timesOffered = SessionVariables.CurrentAccount.GetAccountProperties(AccountPropertyType.TimeOffered);
            var pricingOptions = SessionVariables.CurrentAccount.GetAccountProperties(AccountPropertyType.PricingOption);

            while (awards.Count < 5)
                awards.Add(new AccountProperty());

            while (certifications.Count < 5)
                certifications.Add(new AccountProperty());

            while (photos.Count < 6)
                photos.Add(new AccountProperty());

            var ah = AfterSchoolLibrary.Helpers.Utilities.GetNewAmazonHelper();

            if (profile.Logo.IsNotBlank())
                ViewBag.LogoUrl = ah.GetFileLink(profile.Logo, true, null, true);

            if (profile.HeaderImage.IsNotBlank())
                ViewBag.HeaderImageUrl = ah.GetFileLink(profile.HeaderImage, true, null, true);

            photos.ForEach(photo => photo.Value = ah.GetFileLink(photo.Value, true, null, true));

            ViewBag.Messages = msgs;
            ViewBag.Grades = GradeLevel.Load();
            ViewBag.Awards = awards;
            ViewBag.Certifications = certifications;
            ViewBag.Photos = photos;
            ViewBag.ServicesOffered = servicesOffered;
            ViewBag.TimesOffered = timesOffered;
            ViewBag.PricingOptions = pricingOptions;
            ViewBag.Classifications = classifications;
            ViewBag.SoftSkills = softSkills;
            ViewBag.AcademicSubjects = academicSubjects;
            ViewBag.TeachingMethods = teachingMethods;
            ViewBag.IsFullScreen = GetIsFullScreen();

            return profile;
        }

        [HttpGet]
        public ActionResult Style()
        {
            var msgs = new SystemMessages();
            return View(StyleViewBag(msgs));
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Style(string id, string logo, string headerimage, string about, string headline, string primaryColor, string website, string mingrade, string maxgrade, string twitter, string facebook, string instagram, string linkedin, string genderrestriction, string classifications, string softskills, string academicsubjects, string teachingmethods)
        {
            var msgs = new SystemMessages();
            var profile = SessionVariables.CurrentAccount.GetAccountProfile(msgs);
            var allowedFiles = new string[] { ".png", ".jpg", ".jpeg" };
            var maxSizeInBytes = 1024 * 1024 * 2;

            if (logo.IsNotEmpty())
            {
                if (Request.Files["logo"].ContentLength == 0 || 
                    !Request.Files["logo"].FileName.ToLower().Contains(allowedFiles) ||
                    Request.Files["logo"].ContentLength > maxSizeInBytes)
                    msgs.Add("Please upload a valid jpg or png file", MessageType.UserError);
            }

            if (headerimage.IsNotEmpty())
            {
                if (Request.Files["headerimage"].ContentLength == 0 || 
                    !Request.Files["headerimage"].FileName.ToLower().Contains(allowedFiles) ||
                    Request.Files["headerimage"].ContentLength > maxSizeInBytes)
                    msgs.Add("Please upload a valid jpg or png file", MessageType.UserError);
            }

            if (website.IsNotEmpty() && !website.IsRegExMatch(BasicLibrary.Utilities.WebsiteRegEx))
                msgs.Add("Please enter a valid website address", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                if (logo.IsNotEmpty())
                    profile.UploadLogo(
                        SessionVariables.CurrentAccount,
                        Request.Files["logo"].InputStream,
                        Request.Files["logo"].FileName,
                        SessionVariables.CurrentUser,
                        msgs);

                if (headerimage.IsNotEmpty())
                    profile.UploadHeaderImage(
                        SessionVariables.CurrentAccount,
                        Request.Files["headerimage"].InputStream,
                        Request.Files["headerimage"].FileName,
                        SessionVariables.CurrentUser,
                        msgs);

                
                foreach (var key in Request.Files.AllKeys.Where(key => key.Contains("photo-")))
                {
                    var file = Request.Files[key];
                    if (file.IsNotEmpty() && file.FileName.IsNotBlank())
                    {
                        if (file.FileName.Contains(allowedFiles) && file.ContentLength <= maxSizeInBytes)
                        {
                            var photoId = "";

                            if (!key.Contains("new"))
                                photoId = key.Split('-')[2];

                            profile.UploadGalleryPhoto(
                                    SessionVariables.CurrentAccount,
                                    Request.Files[key].InputStream,
                                    Request.Files[key].FileName,
                                    photoId,
                                    SessionVariables.CurrentUser,
                                    msgs);
                        }
                        else
                        {
                            msgs.Add("Please upload a valid jpg or png file", MessageType.UserError);
                        }
                    }
                }

                profile.UpdateProfile(
                    headline,
                    about,
                    primaryColor,
                    website,
                    mingrade,
                    maxgrade,
                    twitter,
                    facebook,
                    instagram,
                    linkedin,
                    genderrestriction,
                    Request.Params.Filter("award-id-"),
                    Request.Params.Filter("award-new-"),
                    Request.Params.Filter("certification-id-"),
                    Request.Params.Filter("certification-new-"),
                    Request.Params.Filter("services-offered-"),
                    Request.Params.Filter("times-offered-"),
                    Request.Params.Filter("pricing-options-"),
                    classifications,
                    softskills,
                    academicsubjects,
                    teachingmethods,
                    SessionVariables.CurrentUser,
                    msgs);
            }

            if (msgs.HasNoErrors())
                msgs.Add("Saved Successfully", MessageType.Success);

            return View(StyleViewBag(msgs));
        }

        //[HttpGet]
        //public ActionResult Integrations()
        //{
        //    ViewBag.AccountPackage = SessionVariables.AccountPackage;
        //    ViewBag.Messages = new SystemMessages();
        //    ViewBag.FacebookAdPixel = SessionVariables.CurrentAccount.GetAccountProperty(AccountPropertyType.FacebookAdPixel);
        //    return View(SessionVariables.CurrentAccount);
        //}

        //[ValidateInput(false)]
        //[HttpPost]
        //public ActionResult Integrations(string id, string facebookadpixelscript)
        //{
        //    var msgs = new SystemMessages();
        //    var a = SessionVariables.CurrentAccount;

        //    var facebookAdPixel = a.GetAccountProperty(AccountPropertyType.FacebookAdPixel);

        //    if (facebookAdPixel.IsEmpty())
        //    {
        //        facebookAdPixel = new AccountProperty() { Value = facebookadpixelscript };
        //        facebookAdPixel.AccountPropertyTypeId = AccountPropertyType.FacebookAdPixel;
        //        facebookAdPixel.CreateDate = DateTime.UtcNow;
        //        facebookAdPixel.CreatedBy = SessionVariables.LoggedInUser.ID;
        //        facebookAdPixel.AccountId = a.ID;
        //    }
        //    else
        //    {
        //        facebookAdPixel.Value = facebookadpixelscript;
        //    }

        //    if (msgs.HasNoErrors())
        //    {
        //        Database.ExecuteTransaction(
        //            action: (SystemMessages amsgs, DatabaseConnection adc) => {

        //                a.LastModifiedDate = DateTime.UtcNow;
        //                a.LastModifiedBy = SessionVariables.LoggedInUser.ID;
        //                amsgs.AddRange(a.Save(adc));

        //                if (amsgs.HasNoErrors())
        //                {
        //                    facebookAdPixel.LastModifiedDate = DateTime.UtcNow;
        //                    facebookAdPixel.LastModifiedBy = SessionVariables.LoggedInUser.ID;
        //                    amsgs.AddRange(facebookAdPixel.Save(adc));
        //                }
        //            },
        //            onSuccess: (SystemMessages smsgs) => {
        //                msgs.AddRange(smsgs);
        //                msgs.Add("Saved Successfully", MessageType.Success);
        //            },
        //            onFailure: (SystemMessages fmsgs) => {
        //                msgs.AddRange(fmsgs);
        //            }
        //        );

        //    }

        //    ViewBag.Messages = msgs;
        //    ViewBag.AccountPackage = SessionVariables.AccountPackage;
        //    ViewBag.FacebookAdPixel = facebookAdPixel;
        //    return View(a);
        //}

        //[HttpGet]
        //public ActionResult Detail()
        //{
        //    ViewBag.AccountPackage = SessionVariables.AccountPackage;
        //    ViewBag.Messages = new SystemMessages();
        //    return View(SessionVariables.CurrentAccount);
        //}

        //[HttpGet]
        //public ActionResult Billing()
        //{
        //    Account a = SessionVariables.CurrentAccount;
        //    ViewBag.BillingHistory = a.GetStripeBillingHistory();
        //    ViewBag.RefundHistory = a.GetStripeRefundHistory();
        //    // ViewBag.Cards = a.GetStripeCards();
        //    ViewBag.Messages = new SystemMessages();
        //    return View(a);
        //}

        //[HttpGet]
        //public ActionResult NewBilling(string id)
        //{
        //    var msgs = new SystemMessages();

        //    if (SessionVariables.AccountPackage.IsEmpty())
        //    {
        //        var accountPackages = SessionVariables.CurrentAccount.GetAccountPackages(msgs).Where(ap => ap.IsActive);

        //        if (!accountPackages.Any())
        //        {
        //            var defaultPackage = Package.Load(msgs: msgs).First();
        //            SessionVariables.AccountPackage = SessionVariables.CurrentAccount.AddPackage(defaultPackage.ID.Value, BillingPeriodType.Month, SessionVariables.LoggedInUser, msgs);
        //        }
        //        else
        //        {
        //            SessionVariables.AccountPackage = accountPackages.Where(ap => ap.IsActive).First();
        //        }

        //        return LoginRedirect();
        //    }

        //    var package = id.IsGuid() ? Package.LoadByUid(id.ToGuid()) : SessionVariables.AccountPackage?.ToPackage();

        //    ViewBag.Package = package;
        //    ViewBag.Packages = Package.LoadByIsActiveByIsAfterSchool(true, SessionVariables.IsAfterSchool.Value);
        //    ViewBag.Messages = new SystemMessages();
        //    return View(SessionVariables.CurrentAccount);
        //}

        //[HttpPost]
        //public ActionResult NewPayment(string selectedpackage, string stripetoken)
        //{
        //    var msgs = new SystemMessages();

        //    Package package = null;
        //    var a = SessionVariables.CurrentAccount;
        //    var isFromTrialAccount = SessionVariables.AccountPackage?.IsDuringTrial ?? false;

        //    Database.ExecuteTransaction(
        //        action: (SystemMessages amsgs, DatabaseConnection adc) => {

        //            var selectedPackageArr = selectedpackage.Split('_');
        //            var billingPeriod = (BillingPeriodType)selectedPackageArr[0].ToInteger();

        //            package = Package.LoadByUid(selectedPackageArr[1].ToGuid(), amsgs, adc);

        //            // amsgs.AddRange(a.AddCreditCard(stripetoken, true, SessionVariables.LoggedInUser, adc));

        //            if (amsgs.HasNoErrors())
        //                a.AddPackage(package.ID.Value, billingPeriod, SessionVariables.LoggedInUser, amsgs, adc);

        //            SessionVariables.AccountPackage = a.GetActiveAccountPackage(amsgs, adc);
        //        },
        //        onSuccess: (SystemMessages smsgs) => {
        //            msgs.AddRange(smsgs);

        //            msgs.Clear(MessageType.Success);
        //            msgs.Add(string.Format("The '{0} Package' has been added to your account", package.Name), MessageType.Success);

        //            if (isFromTrialAccount)
        //                SessionVariables.CurrentAccount.SendWelcomeEmailPurchase();
        //        },
        //        onFailure: (SystemMessages fmsgs) => {
        //            msgs.AddRange(fmsgs);
        //        }
        //    );

        //    ViewBag.Messages = msgs;

        //    if (msgs.HasNoErrors())
        //    {
        //        ViewBag.BillingHistory = a.GetStripeBillingHistory();
        //        ViewBag.RefundHistory = a.GetStripeRefundHistory();
        //        // ViewBag.Cards = a.GetStripeCards();
        //        return View("Billing", a);
        //    }
        //    else
        //    {
        //        ViewBag.Package = package;
        //        ViewBag.Packages = Package.LoadByIsActiveByIsAfterSchool(true, SessionVariables.IsAfterSchool.Value);
        //        return View("NewBilling", a);
        //    }
        //}

        //[HttpPost]
        //public ActionResult AddCard(string id, string stripetoken, string makeprimary)
        //{
        //    SystemMessages msgs = new SystemMessages();

        //    var a = SessionVariables.CurrentAccount;
        //    //msgs.AddRange(a.AddCreditCard(stripetoken, !makeprimary.IsEmpty(), SessionVariables.LoggedInUser));
        //    //ViewBag.Cards = a.GetStripeCards();
        //    ViewBag.Messages = msgs;
        //    return View("Billing", a);
        //}

        //[HttpGet]
        //public ActionResult Packages()
        //{
        //    Account a = SessionVariables.CurrentAccount;
        //    ViewBag.Packages = Package.LoadByIsActive(true);
        //    ViewBag.AccountPackage = a.GetActiveAccountPackage();
        //    ViewBag.Messages = new SystemMessages();
        //    return View(a);
        //}

        //[HttpGet]
        //public ActionResult SelectPackage(string id)
        //{
        //    SystemMessages msgs = new SystemMessages();

        //    Account a = SessionVariables.CurrentAccount;
        //    a.AddPackage(id.ToInteger(), BillingPeriodType.Month, SessionVariables.LoggedInUser, msgs);

        //    SessionVariables.AccountPackage = a.GetActiveAccountPackage();

        //    ViewBag.Packages = Package.LoadByIsActive(true);
        //    ViewBag.AccountPackage = SessionVariables.AccountPackage;
        //    ViewBag.Messages = msgs;
        //    return View("Packages", a);
        //}

        public ActionResult OAuthStripe()
        {
            var msgs = new SystemMessages();

            if (Request["error"].IsEmpty())
                SessionVariables.CurrentAccount.CompleteStripeConnect(Request["code"], msgs);
            else
                msgs.Add("It appears you have canceled your request to connect your Stripe account.", MessageType.Notice);

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
                return RedirectToAction("paymentsdone", new { source = Request["state"], isfullscreen = Request["isfullscreen"] });

            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.IsFullScreen = GetIsFullScreen();

            return View("PaymentsOptions");
        }

        public ActionResult DisconnectStripe()
        {
            var msgs = new SystemMessages();

            SessionVariables.CurrentAccount.DisconnectStripeConnect(msgs);

            return RedirectToAction("PaymentsOptions");
        }
    }
}
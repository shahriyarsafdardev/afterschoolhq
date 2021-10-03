using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;
using AfterSchool.Models;
using AfterSchoolLibrary.Services;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Billing;
using System.Web;
using AfterSchoolLibrary.Helpers;

namespace AfterSchool.Areas.Org.Controllers
{
    public class AccountController : BaseLoggedInController
    {
        public AccountController() : base(AccountService.ProviderAccountTypes.ToList())
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
        public ActionResult Edit(string id, string name, string email, string phone, string uniquename, string isactive, string timezone)
        {
            var msgs = new SystemMessages();
            var a = SessionVariables.CurrentAccount;

            if (name.IsEmpty())
                msgs.Add("Organization Name is required", MessageType.UserError);

            Database.ExecuteTransaction(
               action: (SystemMessages amsgs, DatabaseConnection adc) =>
               {
                   // ..update profile title if account name has been changed 
                   if (name.IsNotEmpty() && a.Name != name)
                   {
                       var profile = a.GetAccountProfile(amsgs, adc);

                       if (profile.IsNotEmpty())
                       {
                           profile.Title = name;
                           amsgs.AddRange(profile.Save(adc));
                       }
                   }

                   a.Name = name;
                   a.Phone = phone;
                   a.UniqueName = uniquename;
                   a.TimeZone = timezone;
                   a.Email = email;

                   if (amsgs.HasNoErrors())
                   {
                       if (a.UniqueName.IsEmpty() && uniquename.IsEmpty())
                           a.UniqueName = AccountService.GenerateUniqueName(name, a.ID, amsgs, adc);
                       else
                           a.UniqueName = AccountService.GenerateUniqueName(uniquename, a.ID, amsgs, adc);

                       a.LastModifiedDate = DateTime.UtcNow;
                       a.LastModifiedBy = SessionVariables.CurrentUser.ID;
                       amsgs.AddRange(a.SaveWithStripe(adc));
                   }
               },
               onSuccess: (SystemMessages smsgs) =>
               {
                   msgs.AddRange(smsgs);

                   if (msgs.HasNoErrors())
                       msgs.Add("Saved Successfully", MessageType.Success);
               },
               onFailure: (SystemMessages fmsgs) =>
               {
                   msgs.AddRange(fmsgs);
               },
               onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
           );

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(a);
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
        public ActionResult Location(string id, string location, string locationname, string address1, string address2, string city, string state, string postalcode, string country, string mylocation, string locationtype)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            var locationType = LocationType.NotSet;
            var hostAccountId = new int?();

            if (locationtype.IsNumber())
                locationType = (LocationType)locationtype.ToInteger();

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
            ViewBag.SignupType = Request["signuptype"].IsNotEmpty() && Request["signuptype"].IsInteger() ? (SignUpType)Request["signuptype"].ToInteger() : SignUpType.NotSet;
            ViewBag.Messages = new SystemMessages();
            ViewBag.SupportEmail = AfterSchoolLibrary.Helpers.Email.AppSupportEmailAddress;

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
        public ActionResult CheckSetup(string entitytype, string name, string address1, string address2, string city, string state, string postalcode)
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
                return RedirectToAction("paymentsdone", new { isnew = 1 });

            var model = currentAccount.GetCheckPaymentAccount(msgs) ?? new CheckPaymentAccount();

            ViewBag.Account = currentAccount;
            ViewBag.IsFullScreen = GetIsFullScreen();
            ViewBag.Messages = msgs;

            return View(model);
        }


        [HttpGet]
        public ActionResult BankSetup()
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var model = BankInfoViewModel.FromBankPaymentAccount(currentAccount.GetBankPaymentAccount(msgs) ?? new BankPaymentAccount());

            ViewBag.Account = currentAccount;
            ViewBag.IsFullScreen = GetIsFullScreen();
            ViewBag.Messages = msgs;

            return View(model);
        }

        [HttpPost]
        public ActionResult BankSetup(BankInfoViewModel bankInfoVM)
        {
            var currentAccount = SessionVariables.CurrentAccount;
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
                currentAccount.CreateOrUpdateBankPaymentAccount(
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
                bankInfoVM = BankInfoViewModel.FromBankPaymentAccount(currentAccount.GetBankPaymentAccount(msgs) ?? new BankPaymentAccount());

            ViewBag.Account = currentAccount;
            ViewBag.IsFullScreen = GetIsFullScreen();
            ViewBag.Messages = msgs;

            return View(bankInfoVM);
        }

        [HttpGet]
        public ActionResult PaymentsEntity()
        {
            //.. this view is depreciated and paymentsoptions is the right view
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var paymentsEntityVM = PaymentsEntityViewModel.LoadFromStripeAccount(currentAccount.GetStripeCustomAccount(msgs));

            ViewBag.Messages = msgs;
            ViewBag.Account = currentAccount;
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
            var account = SessionVariables.CurrentAccount;
            var stripeAccount = account.GetStripeCustomAccount(msgs);

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

                if (account.StripeUserId.IsEmpty())
                    account.CreateStripeCustomAccount(
                        paymentsEntityVM.GetStripeAccountCreateOptions(account),
                        msgs);
                else
                    account.UpdateStripeCustomAccount(
                        paymentsEntityVM.GetStripeAccountUpdateOptions(account),
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

            var model = PaymentsEntityViewModel.LoadFromStripeAccount(account.GetStripeCustomAccount(msgs));

            ViewBag.Messages = msgs;
            ViewBag.Account = account;
            ViewBag.IsFullScreen = GetIsFullScreen();

            return View(model);
        }

        [HttpGet]
        public ActionResult PaymentsBank()
        {
            var account = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var paymentsEntityVM = PaymentsEntityViewModel.LoadFromStripeAccount(account.GetStripeCustomAccount(msgs));

            ViewBag.Account = account;
            ViewBag.Messages = msgs;
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(paymentsEntityVM);
        }

        [HttpPost]
        public ActionResult PaymentsBank(PaymentsEntityViewModel paymentsEntityVM)
        {
            var account = SessionVariables.CurrentAccount;
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
                account.UpdateStripeCustomAccount(
                        paymentsEntityVM.GetStripeAccountUpdateOptions(account),
                        msgs);

            if (msgs.HasNoErrors() && Request["isnew"] == "1")
                return RedirectToAction("PaymentsDone");

            ViewBag.Account = account;
            ViewBag.Messages = msgs;
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(paymentsEntityVM);
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

            var user = account.GetAccountOwner(msgs);

            //.. send support email to AHQ just to notifiy the team that a payment has been added/updated.
            var body = $"<p>Account: {account.Name}<br />" +
                    $"Account Profile Url: {account.HomePageUrl}<br />" +
                    $"User: {currentUser.FirstName} {currentUser.LastName}<br />";

            EmailHelper.SendEmail(
                toEmail: AfterSchoolLibrary.Helpers.Email.AppCustomersEmailAddress,
                subject: $"{account.Name} has connected their stripe account",
                body,
                isAfterSchool: true);

            if (user.IsNotEmpty() && account.IsNotEmpty())
                SendGridHelper.AddOrUpdateContact(account, user, msgs);

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
            var a = SessionVariables.CurrentAccount;
            var currentuser = SessionVariables.CurrentUser;
            var accountPackage = a.GetActiveAccountPackage(msgs);

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    foreach (FeatureType type in System.Enum.GetValues(typeof(FeatureType)))
                    {
                        if (Request[type.ToString()].IsNotEmpty())
                        {
                            var accountFeature = AccountPackageFeature.LoadByAccountPackageIdByFeatureTypeId(accountPackage.ID.Value, type, msgs: amsgs, dc: adc);
                            if (accountFeature.IsEmpty())
                            {
                                accountFeature = new AccountPackageFeature();
                                accountFeature.AccountPackageId = accountPackage.ID;
                                accountFeature.CreateDate = DateTime.UtcNow;
                                accountFeature.CreatedBy = currentuser.ID;
                                accountFeature.FeatureTypeId = type;
                            }

                            accountFeature.Max = Request[type.ToString()].ToDecimal();
                            accountFeature.LastModifiedBy = currentuser.ID;
                            accountFeature.LastModifiedDate = DateTime.UtcNow;
                            amsgs.AddRange(accountFeature.Save(adc));

                            if (accountFeature.FeatureTypeId == FeatureType.TrialDays && accountPackage.TrialStartDate.HasValue)
                                amsgs.AddRange(accountPackage.UpdateTrialDays(accountFeature.Max.Value.ToInteger(), adc));
                        }
                    }
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                    msgs.Add("Saved Successfully", MessageType.Success);
                    SessionVariables.AccountPackage = a.GetActiveAccountPackage();
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                }
            );

            ViewBag.Features = a.GetAccountPackageFeatures(msgs);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Messages = msgs;

            return View(a);
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
            ViewBag.NeedToUpgrade = members.Count >= accountPackage.GetFeature(FeatureType.TeamAccounts, msgs).GetMaxValue();
            ViewBag.Messages = msgs;
            return View(currentAccount);
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
                    .ForEach(email => msgs.Add(string.Format("{0} is not a valid email address", email), MessageType.UserError));

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
                            (UserType)usertype.ToInteger(),
                            currentUser,
                            msgs));

            if (msgs.HasNoErrors())
                msgs.Add("All invites have been sent", MessageType.Success);

            var members = account.GetUsers(msgs: msgs);

            ViewBag.AccountPackage = accountPackage;
            ViewBag.Members = members;
            ViewBag.NeedToUpgrade = members.Count >= accountPackage.GetFeature(FeatureType.TeamAccounts, msgs).GetMaxValue();
            ViewBag.Messages = msgs;

            return View("Team", account);
        }

        public ActionResult TeamRemoveMember(string id)
        {
            var msgs = new SystemMessages();
            var currentUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;
            var accountUserType = SessionVariables.AccountUserType;
            var user = WebUser.LoadByUid(id.ToGuid(), msgs);
            var awu = currentAccount.GetAccountWebUser(user.ID.Value, msgs);

            if (awu.IsNotEmpty() && currentUser.HasAccessTo(currentAccount, msgs) && accountUserType.In(AccountService.AdminUserTypes))
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
            msgs.AddRange(accountRelationship.UpdateApprovalStatus(ApprovalStatus.Approved, SessionVariables.CurrentUser));

            ViewBag.Messages = msgs;
            return RedirectToAction("visitors");
        }

        [HttpGet]
        public ActionResult DenyVisitor(string id)
        {
            var msgs = new SystemMessages();
            var accountRelationship = SessionVariables.CurrentAccount.GetAccountRelationship(id.ToInteger(), msgs);
            msgs.AddRange(accountRelationship.UpdateApprovalStatus(ApprovalStatus.Denied, SessionVariables.CurrentUser));

            ViewBag.Messages = msgs;
            return RedirectToAction("visitors");
        }

        [HttpGet]
        public ActionResult Forms(string fromSurvey)
        {
            var msgs = new SystemMessages();
            var forms = SessionVariables.CurrentAccount.GetForms(msgs);

            ViewBag.Messages = msgs;
            ViewBag.FromSurvey = fromSurvey;
            return View(forms);
        }

        [HttpGet]
        public ActionResult FormEdit(string id, string qg)
        {
            var currentAccount = SessionVariables.CurrentAccount;

            if (id.IsEmpty())
                return HttpNotFound();

            var msgs = TempData["Messages"] as SystemMessages;
            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            var form = currentAccount.GetForm(id.ToInteger(), msgs);
            var questionGroups = form.GetActiveQuestionGroups(msgs);
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
                    questionGroup = FormQuestionGroup.Load(qg.ToInteger(), msgs);
            }
            else
                questionGroup = null;

            ViewBag.Messages = msgs;
            ViewBag.QuestionGroups = questionGroups;
            ViewBag.IndividualGroups = individualGroups;
            ViewBag.OrganizationGroups = organizationGroups;
            ViewBag.QuestionGroup = questionGroup;
            return View(form);
        }

        [HttpPost]
        public ActionResult FormEdit(string id, string copyfrom, string qg, string name, string description, string cmd, string formtype)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (id.IsEmpty() && copyfrom.IsEmpty())
                return HttpNotFound();

            Form form = null;
            FormQuestionGroup questionGroup = null;

                if (name.IsEmpty() && qg.IsBlank())
                    msgs.Add("Name is required", MessageType.UserError);

                if (id.ToInteger() == 0)
                {
                    if ((FormType)formtype.ToInteger() != FormType.Survey)
                    {
                        if (copyfrom.IsNumber())
                            form = currentAccount.GetForm(copyfrom.ToInteger(), msgs)?
                                .Copy(currentUser.ID.Value, currentAccount.ID.Value, (FormType)formtype.ToInteger(), null, msgs, null);
                        else
                            form = currentAccount.GetDefaultForm(msgs)?
                                .Copy(currentUser.ID.Value, currentAccount.ID.Value, (FormType)formtype.ToInteger(), null, msgs, null);
                    }
                    else
                    {
                        form = FormService.CreateBlankForm(form, currentUser.ID.Value, currentAccount.ID.Value, name, description, msgs);
                        ViewBag.QuestionGroups = new List<FormQuestionGroup>(); // to avoid errors
                    }
                }
                else
                    form = currentAccount.GetForm(id.ToInteger(), msgs);

            var questions = new List<FormQuestion>();
            var questionGroups = new List<FormQuestionGroup>();
            var individualGroups = new List<FormQuestionGroup>();
            var organizationGroups = new List<FormQuestionGroup>();

            if (msgs.HasNoErrors() && ((FormType)formtype.ToInteger() != FormType.Survey))
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        if (name.IsNotBlank()) form.Name = name;
                        if (description.IsNotBlank()) form.Description = description;
                        if (formtype.IsNotBlank()) form.FormTypeId = (FormType)formtype.ToInteger();

                        amsgs.AddRange(form.Save(adc));

                        if (qg.IsNumber())
                        {
                            // create or update question groups
                            if (qg.ToInteger() == 0)
                            {
                                questionGroup = new FormQuestionGroup()
                                {
                                    CreateDate = DateTime.UtcNow,
                                    CreatedBy = currentUser.ID,
                                    LastModifiedDate = DateTime.UtcNow,
                                    LastModifiedBy = currentUser.ID,
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
                                    questionGroup.LastModifiedBy = currentUser.ID;
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
                                    question.IsAnswerDescriptionEnabled = Request.Params[$"answerDescription-{question.ID}"].IsBoolean();

                                    if (question.QuestionDataTypeId == QuestionDataType.NotSet)
                                    {
                                        question.Text = Request.Params[$"text-{question.ID}"];
                                        // for Text Block
                                        question.Description = Request.Params[$"description-{question.ID}"];
                                        question.Options = Request.Params[$"options-text-{question.ID}"];
                                        question.Link = Request.Params[$"link-text-{question.ID}"];
                                        question.DisplayOrder = Request.Params[$"displayOrderId-{question.ID}"].ToInteger();
                                        question.QuestionTypeId = (QuestionType)Request.Params[$"questiontype-{question.ID}"].ToInteger();
                                    }

                                    question.LastModifiedBy = currentUser.ID;
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
                                        CreatedBy = currentUser.ID,
                                        LastModifiedBy = currentUser.ID,
                                        DisplayOrder = Request.Params[string.Format("new-sort-{0}", i)].ToInteger(),
                                        //questionGroup.GetQuestionCount(amsgs, adc) + 1,
                                        FormQuestionGroupId = questionGroup.ID,
                                        IsTemplate = false,
                                        QuestionDataTypeId = QuestionDataType.NotSet,
                                        QuestionTypeId = (QuestionType)Request.Params[string.Format("new-questiontype-{0}", i)].ToInteger(),
                                        Text = Request.Params[string.Format("new-text-{0}", i)],
                                        Options = Request.Params[string.Format("new-options-text-{0}", i)],
                                        Link = Request.Params[string.Format("new-link-text-{0}", i)],
                                        Description = Request.Params[string.Format("new-description-{0}", i)],
                                        IsActive = Request.Params[string.Format("new-active-{0}", i)].IsBoolean(),
                                        IsRequired = Request.Params[string.Format("new-required-{0}", i)].IsBoolean(),
                                        IsAnswerDescriptionEnabled = Request.Params[string.Format("new-description-enabled-{0}", i)].IsBoolean()
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

            if ((FormType)formtype.ToInteger() != FormType.Survey)
            {
                questionGroups = form.GetQuestionGroups(msgs);
                individualGroups = questionGroups
                    .Where(x => x.RegistrantTypeId == RegistrantType.Individual)
                    .OrderBy(x => x.DisplayOrder)
                    .ToList();
                organizationGroups = questionGroups
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
            }
            ViewBag.QuestionGroups = questionGroups;
            ViewBag.IndividualGroups = individualGroups;
            ViewBag.OrganizationGroups = organizationGroups;
            ViewBag.QuestionGroup = questionGroup;
            ViewBag.Messages = msgs;
            return View(form);
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var questionToDelete = FormQuestion.Load(id.ToInteger(), msgs);
            questionToDelete.IsActive = false;
            msgs.AddRange(questionToDelete.Save());
            
            // Show Success Message
            // LOG: Someone has Deleted Question.
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
                   indirectObjectType: ObjectType.FormQuestion,  // Need to know about indirectObjectType
                   indirectObjectId: currentAccount.ID.Value,
                   indirectObjectText: currentAccount.Name
               );
            }

            ViewBag.Messages = msgs;
            TempData["Messages"] = msgs;

            return RedirectToAction("FormEdit", new { id = questionToDelete.FormId, qg = "" });
        }

        [HttpGet]
        public ActionResult DeleteSection(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var sectionToDelete = FormQuestionGroup.Load(id.ToInteger(), msgs);
            sectionToDelete.IsActive = false;
            msgs.AddRange(sectionToDelete.Save());

            // Show Success Message
            // LOG: Someone has Deleted Question.
            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been deleted.", sectionToDelete.Name), MessageType.Success);
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
            TempData["Messages"] = msgs;

            return RedirectToAction("FormEdit", new { id = sectionToDelete.FormId, qg = ""});
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

            var ah = AfterSchoolLibrary.Helpers.Utilities.GetNewAmazonHelper();

            if (profile.Logo.IsNotBlank())
                ViewBag.LogoUrl = ah.GetFileLink(profile.Logo, true, null, true);

            if (profile.HeaderImage.IsNotBlank())
                ViewBag.HeaderImageUrl = ah.GetFileLink(profile.HeaderImage, true, null, true);

            photos.ForEach(photo => photo.Value = ah.GetFileLink(photo.Value, true, null, true));

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
            ViewBag.IsFullScreen = GetIsFullScreen();
            ViewBag.Messages = msgs;

            return profile;
        }

        [HttpGet]
        public ActionResult Style()
        {
            return View(StyleViewBag(new SystemMessages()));
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Style(string id, string logo, string headerimage, string about, string title, string headline, string primaryColor, string website, string mingrade, string maxgrade,
            string agestart, string ageend, string restrictbygrade, string twitter, string facebook, string instagram, string linkedin, string youtube, string calendar, string genderrestriction,
            string programCategories, string softskills, string academicsubjects, string teachingmethods, string profileemail, string profilephone,
            string locationname, string streetnumber, string streetname, string address2, string neighborhood, string city, string state, string postalcode, string county, string country, string placeid, decimal latitude, decimal longitude,
            string opencovid, string elearningcenter, string virtualprogram, string ahqapproved, string videoUrl, IEnumerable<HttpPostedFileBase> photos)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var profile = account.GetAccountProfile(msgs);
            var allowedFiles = new string[] { ".png", ".jpg", ".jpeg" };

            if (logo.IsNotEmpty())
            {
                if (Request.Files["logo"].ContentLength == 0 ||
                    !Request.Files["logo"].FileName.ToLower().Contains(allowedFiles) ||
                    Request.Files["logo"].ContentLength > AfterSchoolLibrary.Helpers.Utilities.ImageUploadMaxBytes)
                    msgs.Add("Please upload a valid jpg or png file, must be less than 4MB", MessageType.UserError);
            }

            if (headerimage.IsNotEmpty())
            {
                if (Request.Files["headerimage"].ContentLength == 0 ||
                    !Request.Files["headerimage"].FileName.ToLower().Contains(allowedFiles) ||
                    Request.Files["headerimage"].ContentLength > AfterSchoolLibrary.Helpers.Utilities.ImageUploadMaxBytes)
                    msgs.Add("Please upload a valid jpg or png file", MessageType.UserError);
            }

            if (website.IsNotEmpty() && !website.IsRegExMatch(BasicLibrary.Utilities.WebsiteRegEx))
                msgs.Add("Please enter a valid website address", MessageType.UserError);

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
                            if (photo.FileName.ToLower().Contains(allowedFiles) && photo.ContentLength <= AfterSchoolLibrary.Helpers.Utilities.ImageUploadMaxBytes)
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
                                msgs.Add("Please upload a valid jpg or png file that is less than 4MB", MessageType.UserError);
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
                    website,
                    opencovid.IsNumber() && opencovid.ToBoolean(),
                    elearningcenter.IsNumber() && elearningcenter.ToBoolean(),
                    virtualprogram.IsNumber() && virtualprogram.ToBoolean(),
                    mingrade,
                    maxgrade,
                    agestart,
                    ageend,
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
                    latitude,
                    longitude,
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
                    return Json(new { IsSuccess = true });
                }

                //if we reach here, something gone wrong. 
                return Json(new { IsSuccess = false });
            }
            catch (Exception ex)
            {
                return Json(new { IsSuccess = false });
            }
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

        [HttpGet]
        public ActionResult Billing()
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var packages = currentAccount.GetAccountPackages(msgs);

            if (packages.Count <= 0 || !packages.Any(m => !m.IsFree))
                return RedirectToAction("NewBilling");

            ViewBag.BillingHistory = currentAccount.GetStripeBillingHistory(msgs);
            ViewBag.RefundHistory = currentAccount.GetStripeRefundHistory(msgs);
            ViewBag.Cards = currentAccount.GetStripeCards(msgs);
            ViewBag.Messages = msgs;

            return View(currentAccount);
        }

        [HttpGet]
        public ActionResult NewBilling(string id, string ReturnUrl, string featureId)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var accountPackage = SessionVariables.AccountPackage;
            var msgs = new SystemMessages();

            if (accountPackage.IsEmpty())
            {
                var accountPackages = currentAccount.GetAccountPackages(msgs).Where(ap => ap.IsActive);

                if (!accountPackages.Any())
                {
                    var defaultPackage = Package.Load(msgs: msgs).First();
                    SessionVariables.AccountPackage = currentAccount.AddPackage(defaultPackage.ID.Value, BillingPeriodType.Month, SessionVariables.LoggedInUser, msgs);
                }
                else
                    SessionVariables.AccountPackage = accountPackages.First(ap => ap.IsActive);

                return LoginRedirect();
            }

            if (ReturnUrl.IsNotEmpty())
                ViewBag.ReturnUrl = ReturnUrl;

            var package = id.IsGuid() ? Package.LoadByUid(id.ToGuid(), msgs) : accountPackage?.ToPackage();

            Package suggestedPackage = null;

            if (featureId.IsNotEmpty() && featureId.IsInteger())
                suggestedPackage = Package.Load(featureId.ToInteger(), msgs);

            ViewBag.SuggestedPackage = suggestedPackage;
            ViewBag.Package = package;
            ViewBag.Packages = Package.LoadByIsActiveByIsAfterSchool(true, SessionVariables.IsAfterSchool.Value, msgs);
            ViewBag.Messages = msgs;
            return View(currentAccount);
        }

        [HttpPost]
        public ActionResult GetFeatures(int id)
        {
            var msgs = new SystemMessages();

            if (id.IsEmpty())
                msgs.Add("Package not found.", MessageType.UserError);

            var features = PackageFeature.LoadByPackageId(id, msgs);

            var featureTypeNotIn = new List<FeatureType>() {
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
                FeatureType.ViewInsights
            };

            var model = features.Where(m => m.FeatureTypeId.NotIn(featureTypeNotIn)).Select(m => new
            {
                type = (Enum.GetName(typeof(FeatureType), m.FeatureTypeId)).ToString().SplitByUpperCase(),
                value = m.GetMaxValue()
            });

            if (model.Any())
                return Json(new { success = true, status = "ok", data = model }, JsonRequestBehavior.AllowGet);

            return Json(new { success = false, status = "error", data = msgs.ToList().Select(msg => new { msg.Text }) }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult NewPayment(string selectedpackage, string stripetoken, string cardId)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var accountPackage = SessionVariables.AccountPackage;
            var msgs = new SystemMessages();

            Package package = null;
            AccountPackage newlyAddedPackage = null;
            Stripe.Customer customer = null;

            var isFromTrialAccount = accountPackage?.IsDuringTrial ?? false;

            if (stripetoken.IsEmail() || cardId.IsEmpty())
                msgs.Add("Please select valid card to proceed further.", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        var selectedPackageArr = selectedpackage.Split('_');
                        var billingPeriod = (BillingPeriodType)selectedPackageArr[0].ToInteger();

                        package = Package.LoadByUid(selectedPackageArr[1].ToGuid(), amsgs, adc);

                        customer = currentAccount.CreateStripeCustomer(amsgs, adc);

                        if (stripetoken.IsNotEmpty())
                            currentAccount.CreateStripeCardFromToken(stripetoken, customer, amsgs, adc);
                        else if (cardId.IsNotEmpty())
                            currentAccount.UpdateStripeCard(cardId, customer, amsgs, adc);

                        if (amsgs.HasNoErrors())
                            newlyAddedPackage = currentAccount.AddPackage(package.ID.Value, billingPeriod, SessionVariables.LoggedInUser, amsgs, adc);
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.Clear(MessageType.Success);
                        msgs.Add(string.Format("The '{0} Package' has been added to your account", package.Name), MessageType.Success);

                        if (isFromTrialAccount)
                            currentAccount.SendWelcomeEmailPurchase(currentAccount.GetAccountOwner(smsgs));

                        msgs.AddRange(smsgs);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
            }

            if (newlyAddedPackage.IsNotEmpty())
                SessionVariables.AccountPackage = currentAccount.GetAccountPackage(newlyAddedPackage.Uid, msgs);

            if (msgs.HasNoErrors())
            {
                ViewBag.BillingHistory = currentAccount.GetStripeBillingHistory(msgs);
                ViewBag.RefundHistory = currentAccount.GetStripeRefundHistory(msgs);
                ViewBag.Cards = currentAccount.GetStripeCards(msgs);
                ViewBag.Messages = msgs;
                return View("Billing", currentAccount);
            }
            else
            {
                ViewBag.Package = package;
                ViewBag.Packages = Package.LoadByIsActiveByIsAfterSchool(true, SessionVariables.IsAfterSchool.Value, msgs);
                ViewBag.Messages = msgs;
                return View("NewBilling", currentAccount);
            }
        }

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

        [HttpGet]
        public ActionResult Packages()
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;

            ViewBag.Packages = Package.LoadByIsActive(true, msgs);
            ViewBag.AccountPackage = currentAccount.GetActiveAccountPackage(msgs);
            ViewBag.Messages = msgs;
            return View(currentAccount);
        }

        [HttpGet]
        public ActionResult SelectPackage(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;

            currentAccount.AddPackage(id.ToInteger(), BillingPeriodType.Month, SessionVariables.LoggedInUser, msgs);

            SessionVariables.AccountPackage = currentAccount.GetActiveAccountPackage(msgs);

            ViewBag.Packages = Package.LoadByIsActive(true, msgs);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Messages = msgs;
            return View("Packages", currentAccount);
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
                msgs.Add("It appears you have canceled your request to connect your Stripe account.", MessageType.Notice);

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

                return RedirectToAction("paymentsdone", new { source = Request["state"], isfullscreen = Request["isfullscreen"] });
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

        public ActionResult SetDefaultCard(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;

            if (id.IsBlank())
                msgs.Add("Card not found.", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var customer = currentAccount.GetStripeCustomer(msgs);
                currentAccount.UpdateStripeCard(id, customer, msgs);
            }

            ViewBag.BillingHistory = currentAccount.GetStripeBillingHistory(msgs);
            ViewBag.RefundHistory = currentAccount.GetStripeRefundHistory(msgs);
            ViewBag.Cards = currentAccount.GetStripeCards(msgs);
            ViewBag.Messages = msgs;

            return View("Billing", currentAccount);
        }
    }
}
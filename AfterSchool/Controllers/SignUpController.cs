using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using AfterSchool.Helpers;
using AfterSchool.Models;
using AfterSchoolLibrary;
using AfterSchoolLibrary.FeatureFlag;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Hangfire;
using Email = AfterSchoolLibrary.Helpers.Email;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Controllers
{
    public class SignUpController : BaseController
    {
        private readonly IFeatureFlagRepository featureFlagRepository;

        public SignUpController(IFeatureFlagRepository featureFlagRepository)
        {
            this.featureFlagRepository = featureFlagRepository;
        }

        [HttpGet]
        public ActionResult Index(string invite, bool isrequestonlyaccess = false)
        {
            if (invite.IsBlank())
            {
                return RedirectToAction("Welcome");
            }

            var signupIndexVM = new SignUpIndexViewModel()
            {
                IsRequestOnlyAccess = isrequestonlyaccess,
                InviteCodeToShare = invite,
            };

            ViewBag.Messages = new SystemMessages();
            return View(signupIndexVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(SignUpIndexViewModel model)
        {
            var msgs = new SystemMessages();
            Account invitingAccount = null;

            if (model.FirstName.IsEmpty())
                msgs.Add("First Name is required", MessageType.UserError);

            if (model.LastName.IsEmpty())
                msgs.Add("Last Name is required", MessageType.UserError);

            if (!model.Email.IsEmail())
                msgs.Add("Please enter a valid email address", MessageType.UserError);
            else if (WebUser.ExistsByEmail(model.Email, msgs))
                msgs.Add("This email has already been registered", MessageType.UserError);

            if (model.Password1.IsEmpty() || model.Password2.IsEmpty() || (model.Password1 != model.Password2))
                msgs.Add("Passwords are required and must match", MessageType.UserError);
            else if (model.Password1.Length < 8)
                msgs.Add("Password must be at least 8 characters", MessageType.UserError);
            if (Utilities.IsProduction || Utilities.IsStaging)
                msgs.AddRange(RecaptchaHelper.Validate(Request["g-recaptcha-response"]));

            if (model.InviteCodeToShare.IsNotEmpty())
            {
                if (!model.InviteCodeToShare.IsGuid())
                    msgs.Add("Invalid invitation code", MessageType.UserError);

                invitingAccount = Account.LoadByInviteCodeToShare(model.InviteCodeToShare.ToGuid());
                if (invitingAccount.IsEmpty())
                    msgs.Add("Invalid invitation code", MessageType.UserError);
            }

            if (msgs.HasNoErrors())
            {
                WebUser user = null;
                Account account = null;

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        user = WebUserService.CreateNewWithNoPhone(
                            firstName: model.FirstName,
                            lastName: model.LastName,
                            email: model.Email,
                            password: model.Password2,
                            isActive: true,
                            isRegistered: true,
                            isVerified: false,
                            loggedInUser: null,
                            howHearAboutUs: model.HowHearAboutUs,
                            msgs: amsgs,
                            dc: adc);

                        account = user.AddNewAccountWithDefaultPackage(
                            orgName: model.OrgName,
                            accountType: model.OrgType.HasValue ? model.OrgType.Value : AccountType.NotSet,
                            loggedInUser: user,
                            msgs: amsgs,
                            dc: adc);

                        if (invitingAccount.IsNotEmpty())
                        {
                            if (model.IsRequestOnlyAccess)
                                account.RequestAccessToAccount(invitingAccount, HostPermissionType.ApproveOnly, user,
                                    msgs, adc);
                            else
                                invitingAccount.ApproveAccessToAccount(account, user, amsgs, adc);
                        }
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        LoadSessionInfo(user, account, msgs: msgs);

                        if (user.IsNotEmpty() && account.IsNotEmpty())
                            SendGridHelper.AddOrUpdateContact(account, user, msgs);

                        BackgroundJob.Enqueue(() => WebUserService.SendVerificationEmail(user, null, true));
                        BackgroundJob.Enqueue(() =>
                            WebUserService.SendWelcomePurchase(user.FirstName, user.Email, account.IsAfterSchool));
                    },
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => msgs.AddRange(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("index", "onboarding", new {area = "org"});

            ViewBag.Messages = msgs;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IndexNew(SignUpIndexViewModel model)
        {
            var msgs = new SystemMessages();
            Account invitingAccount = null;

            if (model.FirstName.IsEmpty())
                msgs.Add("First Name is required", MessageType.UserError);

            if (model.LastName.IsEmpty())
                msgs.Add("Last Name is required", MessageType.UserError);

            if (!model.Email.IsEmail())
                msgs.Add("Please enter a valid email address", MessageType.UserError);
            else if (WebUser.ExistsByEmail(model.Email, msgs))
                msgs.Add("This email has already been registered", MessageType.UserError);

            if (model.Password1.IsEmpty() || model.Password2.IsEmpty() || (model.Password1 != model.Password2))
                msgs.Add("Passwords are required and must match", MessageType.UserError);
            else if (model.Password1.Length < 8)
                msgs.Add("Password must be at least 8 characters", MessageType.UserError);


            if (Utilities.IsProduction || Utilities.IsStaging)
                msgs.AddRange(RecaptchaHelper.Validate(Request["g-recaptcha-response"]));

            if (model.InviteCodeToShare.IsNotEmpty())
            {
                if (!model.InviteCodeToShare.IsGuid())
                    msgs.Add("Invalid invitation code", MessageType.UserError);

                invitingAccount = Account.LoadByInviteCodeToShare(model.InviteCodeToShare.ToGuid());
                if (invitingAccount.IsEmpty())
                    msgs.Add("Invalid invitation code", MessageType.UserError);
            }

            if (msgs.HasNoErrors())
            {
                WebUser user = null;
                Account account = null;

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        user = WebUserService.CreateNewWithNoPhone(
                            firstName: model.FirstName,
                            lastName: model.LastName,
                            email: model.Email,
                            password: model.Password2,
                            isActive: true,
                            isRegistered: true,
                            isVerified: false,
                            loggedInUser: null,
                            howHearAboutUs: model.HowHearAboutUs,
                            msgs: amsgs,
                            dc: adc);

                        if (model.SignupType.IsNotEmpty() && model.SignupType == SignUpType.Teacher1099)
                            model.OrgName = $"{model.FirstName} {model.LastName} Teacher";

                        account = user.AddNewAccountWithDefaultPackageWithStripe(
                            orgName: model.OrgName,
                            accountType: model.OrgType.HasValue ? model.OrgType.Value : AccountType.NotSet,
                            loggedInUser: user,
                            msgs: amsgs,
                            dc: adc);

                        if (user.IsNotEmpty() && account.IsNotEmpty() && amsgs.HasNoErrors())
                        {
                            var webSession = SessionVariables.WebSession;

                            // LOG: WebUserName Created Account Account
                            webSession.LogActivity(
                                account: account,
                                actor: user,
                                actionType: WebSessionActionType.Created,
                                objectType: ObjectType.Account,
                                objectId: account.ID.Value,
                                objectText: account.Name
                            );

                            // LOG: WebUserName SignedUp
                            webSession.LogActivity(
                                account: account,
                                actor: user,
                                actionType: WebSessionActionType.SignedUp,
                                objectType: ObjectType.WebUser,
                                objectId: user.ID.Value,
                                objectText: user.DisplayName
                            );
                        }

                        if (invitingAccount.IsNotEmpty())
                        {
                            if (model.IsRequestOnlyAccess)
                                account.RequestAccessToAccount(invitingAccount, HostPermissionType.ApproveOnly, user,
                                    msgs, adc);
                            else
                                invitingAccount.ApproveAccessToAccount(account, user, amsgs, adc);
                        }
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        LoadSessionInfo(user, account, msgs: msgs);

                        if (user.IsNotEmpty() && account.IsNotEmpty())
                            SendGridHelper.AddOrUpdateContact(account, user, msgs);

                        BackgroundJob.Enqueue(() => WebUserService.SendVerificationEmail(user, null, true));
                        BackgroundJob.Enqueue(() =>
                            WebUserService.SendWelcomePurchase(user.FirstName, user.Email, account.IsAfterSchool));
                    },
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => msgs.AddRange(emsgs)
                );
            }

            if (msgs.HasNoErrors())
            {
                if (model.SignupType == SignUpType.Teacher1099)
                    return RedirectToAction("paymentsoptions", "account",
                        new {area = "org", isfullscreen = 1, signuptype = (int) model.SignupType});

                return featureFlagRepository.IsEnabled("free-trial")
                    ? RedirectToAction("freetrial", "onboarding", new {area = "org"})
                    : RedirectToAction("index", "onboarding", new {area = "org", showGettingStarted = true});
            }


            ViewBag.Messages = msgs;

            return View(model);
        }

        public ActionResult McCoyRegistration()
        {
            var mcCoyCodeToShare = ConfigurationManager.AppSettings["MccoyInviteCodeToShare"];
            if (mcCoyCodeToShare.IsGuid())
            {
                var mccoyAccount = Account.LoadByInviteCodeToShare(mcCoyCodeToShare.ToGuid());

                if (mccoyAccount.IsNotEmpty())
                    return RedirectToAction("Welcome", new {invite = mcCoyCodeToShare, isrequestonlyaccess = true});
            }

            return HttpNotFound();
        }

        public ActionResult ISYPRegistration()
        {
            var isypCodeToShare = ConfigurationManager.AppSettings["ISYPInviteCodeToShare"];
            if (isypCodeToShare.IsGuid())
            {
                var isypAccount = Account.LoadByInviteCodeToShare(isypCodeToShare.ToGuid());

                if (isypAccount.IsNotEmpty())
                    return RedirectToAction("Welcome", new {invite = isypCodeToShare, isrequestonlyaccess = true});
            }

            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult New(string id, string a)
        {
            // if logged in while signing up a new account logout to prevent confusion
            Logout(Request.Url.ToString());

            var msgs = new SystemMessages();
            WebUser wu = null;

            if (!id.IsGuid() || !a.IsGuid())
                msgs.Add("Invalid invitation code", MessageType.UserError);
            else
            {
                wu = WebUser.LoadByUid(id.ToGuid(), null);
                if (wu.IsEmpty())
                    msgs.Add("Invalid invitation code", MessageType.UserError);
                else if (wu.IsActive)
                {
                    var account = Account.LoadByUid(a.ToGuid(), msgs);
                    AccountWebUser awu = null;

                    if (account.IsEmpty())
                        msgs.Add("Invalid invitation code", MessageType.UserError);

                    if (msgs.HasNoErrors())
                    {
                        awu = AccountWebUser.LoadByAccountIdByWebUserId(account.ID.Value, wu.ID.Value, msgs);

                        if (awu.IsEmpty())
                            msgs.Add("Invalid invitation code", MessageType.UserError);
                    }

                    if (msgs.HasNoErrors())
                    {
                        awu.Activate(wu, msgs);
                        account.SetIsActive(true, wu, msgs);
                    }

                    if (msgs.HasNoErrors())
                        return Redirect(Utilities.LoginUrl());

                    msgs.Add("An error occurred. Please contact support", MessageType.UserError);
                }
            }

            ViewBag.Messages = msgs;
            ViewBag.Uid = id;
            ViewBag.AccountUid = a;
            return View(wu);
        }

        [HttpPost]
        public ActionResult New(string id, string a, string firstname, string lastname, string phone, string email,
            string password1, string password2)
        {
            var msgs = new SystemMessages();
            WebUser user = null;
            Account account = null;

            if (!id.IsGuid())
                msgs.Add("Invalid invitation code", MessageType.UserError);
            else
            {
                user = WebUser.LoadByUid(id.ToGuid());

                if (user.IsEmpty())
                    msgs.Add("Invalid invitation code", MessageType.UserError);
                else if (user.IsActive)
                    msgs.Add("Account has already been created. Please proceed to the login page",
                        MessageType.UserError);
            }

            if (!a.IsGuid())
                msgs.Add("Invalid invitation code", MessageType.UserError);
            else
            {
                account = Account.LoadByUid(a.ToGuid());

                if (account.IsEmpty())
                    msgs.Add("Invalid invitation code", MessageType.UserError);
            }

            if (firstname.IsEmpty())
                msgs.Add("First Name is required", MessageType.UserError);
            if (lastname.IsEmpty())
                msgs.Add("Last Name is required", MessageType.UserError);

            if (phone.IsEmpty())
                msgs.Add("Phone is required", MessageType.UserError);
            if (email.IsEmpty())
                msgs.Add("Email is required", MessageType.UserError);
            else if (!email.IsEmail())
                msgs.Add("Please enter a valid email address", MessageType.UserError);
            else if (!user.CanEmailBeUsed(email, msgs))
                msgs.Add("This email has already been registered", MessageType.UserError);

            if (password1.IsEmpty() || password2.IsEmpty() || (password1 != password2))
                msgs.Add("Passwords are required and must match", MessageType.UserError);
            else if (password1.Length < 8)
                msgs.Add("Password must be at least 8 characters", MessageType.UserError);

            if (msgs.HasNoErrors())
                user.CompleteSignUp(account, firstname, lastname, email, password1, phone, null, msgs);

            if (msgs.HasNoErrors() && !user.VerifiedAccount)
            {
                var userId = user.ID.Value;
                BackgroundJob.Enqueue(() => WebUserService.SendVerificationEmail(userId));
            }

            if (msgs.HasNoErrors())
                LoadSessionInfo(user, msgs: msgs);

            if (msgs.HasNoErrors())
                return LoginRedirect();

            ViewBag.Messages = msgs;
            ViewBag.AccountUid = a;
            ViewBag.Uid = id;
            return View(user);
        }

        [HttpGet]
        public ActionResult Verify(string id, string redirectUrl)
        {
            var msgs = new SystemMessages();

            if (id.IsEmpty() || !id.IsGuid())
                msgs.Add("Invalid verification code", MessageType.UserError);
            else if (!WebUser.ExistsByUid(id.ToGuid(), msgs))
                msgs.Add("Invalid verification code", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var user = WebUser.LoadByUid(id.ToGuid(), msgs);
                msgs.AddRange(user.MarkAsVerified());

                if (msgs.HasNoErrors())
                {
                    msgs.Add("Your account has been verified", MessageType.Success);
                    
                    // this is only here if the user is already logged in, there's a chance the user won't be logged in
                    var currentAccount = SessionVariables.CurrentAccount;
                    var currentUser = SessionVariables.CurrentUser;

                    // if logged in reload with new verified value
                    if (currentUser.IsNotEmpty() && currentUser.Uid == id.ToGuid())
                        SessionVariables.CurrentUser = WebUser.Load(currentUser.ID.Value);

                    // if not logged in you need to redirect to login afterwards.
                    if (currentAccount.IsEmpty() || currentUser.IsEmpty())
                    {
                        ViewBag.Url = AfterSchoolLibrary.Helpers.Utilities.LoginUrl(redirectUrl);
                    }
                    else if (redirectUrl.IsNotEmpty())
                    {
                        ViewBag.URL = redirectUrl;
                    }
                    else
                    {
                        // NOTE: this code should go here, but it is not important right now, we need to use hubspot anyway moving forward
                        SendGridHelper.AddOrUpdateContact(currentAccount, user, msgs);
                        
                        switch (currentAccount.OnboardingStepDone)
                        {
                            case OnboardingStepType.AccountType:
                                ViewBag.URL = Url.Action("ProgramInfo", "Onboarding", new {area = "org"});
                                break;
                            case OnboardingStepType.SocialLinks:
                                ViewBag.URL = Url.Action("Done", "Onboarding", new {area = "org"});
                                break;
                            case OnboardingStepType.ProgramInfo:
                                ViewBag.URL = Url.Action("AdditionalInfo", "Onboarding", new {area = "org"});
                                break;

                            case OnboardingStepType.AdditionalInfo:
                                ViewBag.URL = Url.Action("ContactInfo", "Onboarding", new {area = "org"});
                                break;
                            case OnboardingStepType.ContactInfo:
                                ViewBag.URL = Url.Action("Logo", "Onboarding", new {area = "org"});
                                break;

                            case OnboardingStepType.LogoAndHeader:
                                ViewBag.URL = Url.Action("ServicesOffered", "Onboarding", new {area = "org"});
                                break;

                            case OnboardingStepType.ServicesOffered:
                                ViewBag.URL = Url.Action("Links", "Onboarding", new {area = "org"});
                                break;
                            case OnboardingStepType.All:
                                ViewBag.URL = Url.Action("Account", "R", new {accountname = currentAccount.UniqueName});
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            ViewBag.Messages = msgs;
            return View();
        }

        public ActionResult Save()
        {
            SystemMessages msgs = new SystemMessages();
            WebUser wu = null;

            if (Request.Form["firstname"].IsEmpty())
                msgs.Add("First Name is required", MessageType.UserError);

            if (Request.Form["lastname"].IsEmpty())
                msgs.Add("Last Name is required", MessageType.UserError);

            if (Request.Form["email"].IsEmpty())
                msgs.Add("Email is required", MessageType.UserError);
            else if (!Request.Form["email"].IsEmail())
                msgs.Add("Email is invalid", MessageType.UserError);

            if (Request.Form["username"].IsEmpty())
                msgs.Add("Username is required", MessageType.UserError);
            else if (!Request.QueryString["i"].IsGuid())
            {
                wu = WebUser.LoadByUsername(Request.Form["username"], null);
                if (wu.IsNotEmpty())
                    msgs.Add("Username is already in use", MessageType.UserError);

                wu = WebUser.LoadByEmail(Request.Form["email"], null);
                if (wu.IsNotEmpty())
                    msgs.Add("Email is already in use", MessageType.UserError);
            }

            if (Request.Form["password"].IsEmpty())
                msgs.Add("Password is required", MessageType.UserError);

            if (Request.Form["password2"].IsEmpty())
                msgs.Add("Verify Password is required", MessageType.UserError);

            if (Request.Form["password"].IsNotEmpty() && Request.Form["password"] != Request.Form["password2"])
                msgs.Add("Passwords do not match", MessageType.UserError);


            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        if (Request.QueryString["i"].IsGuid())
                            wu = WebUser.LoadByUidByIsActive(Request.QueryString["i"].ToGuid(), false, dc: adc);
                        else
                            wu = new WebUser();

                        wu.FirstName = Request.Form["firstname"];
                        wu.LastName = Request.Form["lastname"];
                        wu.Email = Request.Form["email"];
                        wu.Username = Request.Form["username"];
                        wu.Password = WebUserService.EncryptPassword(Request.Form["password"]);
                        wu.TimeZoneId = Request.Form["timezone"].ToDouble();
                        wu.IsActive = true;
                        wu.LastModifiedDate = wu.CreateDate = DateTime.Now;
                        wu.LastModifiedBy = wu.CreatedBy = 0;

                        if (!Request.Files["uploadfile"].IsEmpty())
                        {
                            HttpPostedFileBase fileUpload = Request.Files["uploadfile"];
                            if (fileUpload.ContentLength > 0)
                            {
                                string fileName = fileUpload.FileName;
                                string fileExt = BasicLibrary.Utilities.GetFileExtension(fileName);
                                string uploadPath = Request.PhysicalApplicationPath + "Uploads\\" + wu.Username + "\\";
                                string fullFilePath = uploadPath + "profile-orig." + fileExt;
                                string thumbnailFilePath = uploadPath + "profile-thumb-300.jpg";

                                if (Directory.Exists(uploadPath) == false)
                                    Directory.CreateDirectory(uploadPath);
                                if (System.IO.File.Exists(fullFilePath) == false) System.IO.File.Delete(fullFilePath);
                                if (System.IO.File.Exists(thumbnailFilePath) == false)
                                    System.IO.File.Delete(thumbnailFilePath);

                                fileUpload.SaveAs(fullFilePath);

                                RebornImage.CreateThumbnailMaxDimension(300, fullFilePath, thumbnailFilePath,
                                    ImageFormat.Jpeg);

                                wu.ImageUrl = "/user/image/" + wu.Username + "/";
                            }
                        }

                        amsgs.AddRange(wu.Save(adc));
                    },
                    onSuccess: (SystemMessages smsgs) => { msgs.AddRange(smsgs); },
                    onFailure: (SystemMessages fmsgs) => { msgs.AddRange(fmsgs); }
                );
            }

            if (msgs.HasAny(SystemMessages.ErrorTypes) == false)
            {
                Response.Redirect("/signup/confirm");
            }

            ViewData["msgs"] = msgs;

            return View();
        }

        [HttpPost]
        public JsonResult IsEmailAlreadyExists(string emailId)
        {
            var msgs = new SystemMessages();
            WebUser wu = WebUser.LoadByEmail(emailId, msgs);
            if (msgs.HasNoErrors())
            {
                if (wu.IsNotEmpty())
                    return Json(new {success = false, error = false});
                else
                    return Json(new {success = true, error = false});
            }
            else
            {
                return Json(new {success = false, error = true});
            }
        }

        [HttpGet]
        public ActionResult Confirm(string id)
        {
            var msgs = new SystemMessages();
            WebUser user = null;
            if (id.IsGuid())
            {
                user = WebUser.LoadByUid(id.ToGuid());
                if (user.IsRegistered)
                    user = null;
            }

            if (user.IsEmpty())
                msgs.Add("Invalid id", MessageType.UserError);

            ViewBag.Messages = msgs;
            return View(user);
        }

        [HttpPost]
        public ActionResult Confirm(string id, string firstname, string lastname, string email, string password1,
            string password2, string returnurl)
        {
            var msgs = new SystemMessages();

            if (!id.IsGuid())
                msgs.Add("Invalid id", MessageType.UserError);

            if (firstname.IsEmpty())
                msgs.Add("First Name is required", MessageType.UserError);

            if (lastname.IsEmpty())
                msgs.Add("Last Name is required", MessageType.UserError);

            if (email.IsEmpty() || !email.IsEmail())
                msgs.Add("Email is required", MessageType.UserError);

            if (password1.IsEmpty())
                msgs.Add("Password is required", MessageType.UserError);

            if (password2.IsEmpty())
                msgs.Add("Verify Password is required", MessageType.UserError);

            if (password1 != password2)
                msgs.Add("Passwords must match", MessageType.UserError);

            var user = WebUser.LoadByUid(id.ToGuid());
            if (user.IsNotEmpty())
            {
                user.FirstName = firstname;
                user.LastName = lastname;
                user.Email = email;
            }
            else
            {
                msgs.Add("Invalid id", MessageType.UserError);
            }

            if (msgs.HasNoErrors())
            {
                user.ConfirmAccount(password1, msgs);
                SessionVariables.CurrentUser = user;

                LoadSessionInfo(user, msgs);

                return HandleRedirectAfterLogin(returnurl);
            }

            return View(user);
        }

        [HttpGet]
        public ActionResult Invite(string id)
        {
            var msgs = new SystemMessages();

            if (!id.IsGuid())
                msgs.Add("Invalid invite", MessageType.UserError);

            var invitingAccount = Account.LoadByInviteCodeToShare(id.ToGuid(), msgs);

            if (invitingAccount.IsEmpty())
                msgs.Add("Invalid invite code", MessageType.UserError);

            var accountTypesToAccept = FormConstants.ProviderAccountTypes;

            // auto accept invite of already logged in
            if (SessionVariables.CurrentUser.IsNotEmpty() &&
                SessionVariables.CurrentAccount.AccountTypeId.In(accountTypesToAccept) &&
                SessionVariables.CurrentAccount.ID != invitingAccount.ID)
            {
                invitingAccount.ApproveAccessToAccount(SessionVariables.CurrentAccount, SessionVariables.CurrentUser,
                    msgs);

                return LoginRedirect();
            }

            ViewBag.Messages = msgs;
            ViewBag.User = new WebUser();

            return View(invitingAccount);
        }

        [HttpPost]
        public ActionResult Invite(string id, string firstname, string lastname, string email, string phone,
            string password1, string password2)
        {
            var msgs = new SystemMessages();
            var user = new WebUser
            {
                FirstName = firstname,
                LastName = lastname,
                Email = email,
                Phone = phone,
            };

            if (!id.IsGuid())
                msgs.Add("Invalid invitation code", MessageType.UserError);

            var invitingAccount = Account.LoadByInviteCodeToShare(id.ToGuid());
            if (invitingAccount.IsEmpty())
                msgs.Add("Invalid invitation code", MessageType.UserError);

            if (firstname.IsEmpty())
                msgs.Add("First Name is required", MessageType.UserError);

            if (lastname.IsEmpty())
                msgs.Add("Last Name is required", MessageType.UserError);

            if (email.IsEmpty())
                msgs.Add("Email is required", MessageType.UserError);
            else if (!email.IsMatch(BasicLibrary.Utilities.EmailRegEx))
                msgs.Add("Please enter a valid email address", MessageType.UserError);
            else if (WebUser.ExistsByEmail(email, msgs))
                msgs.Add("This email has already been registered. Login if you already have an account.",
                    MessageType.UserError);

            if (phone.IsEmpty())
                msgs.Add("Phone is required", MessageType.UserError);
            else if (!phone.IsMatch(BasicLibrary.Utilities.PhoneRegEx))
                msgs.Add("Please enter a valid phone number", MessageType.UserError);
            else if (WebUser.ExistsByPhone(phone, msgs))
                msgs.Add("This phone has already been registered. Login if you already have an account.",
                    MessageType.UserError);

            if (password1.IsEmpty() || password2.IsEmpty() || (password1 != password2))
                msgs.Add("Passwords are required and must match", MessageType.UserError);
            else if (password1.Length < 8)
                msgs.Add("Password must be at least 8 characters", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                // create a new user, then take then to create a real provider account
                user = WebUserService.CreateNew(firstname, lastname, email, phone, password1, true, true, false, false,
                    null);

                if (msgs.HasNoErrors() && !user.VerifiedAccount)
                    msgs.AddRange(user.SendVerificationEmail());
            }

            if (msgs.HasNoErrors())
                LoadSessionInfo(user, msgs: msgs);

            if (msgs.HasNoErrors())
                return RedirectToAction("createaccount", new {invitecode = invitingAccount.InviteCodeToShare});

            ViewBag.Messages = msgs;
            ViewBag.User = user;

            return View(invitingAccount);
        }

        [HttpGet]
        public ActionResult CreateAccount(string invitecode)
        {
            var msgs = new SystemMessages();

            if (invitecode.IsGuid())
                ViewBag.InvitingAccount = Account.LoadByInviteCodeToShare(invitecode.ToGuid(), msgs);

            ViewBag.Messages = msgs;
            ViewBag.User = SessionVariables.CurrentUser;
            ViewBag.InviteCode = invitecode;

            return View(new Account()
                {Email = SessionVariables.CurrentUser.Email, Phone = SessionVariables.CurrentUser.Phone});
        }

        [HttpPost]
        public ActionResult CreateAccount(string invitecode, string orgname, string email, string phone,
            string accounttype)
        {
            var msgs = new SystemMessages();
            Account invitingAccount = null;
            var newAccount = new Account()
            {
                Name = orgname,
                Phone = phone,
                Email = email,
            };

            if (orgname.IsEmpty())
                msgs.Add("Organization Name is required", MessageType.UserError);

            if (email.IsEmpty())
                msgs.Add("Email is required", MessageType.UserError);
            else if (!email.IsRegExMatch(BasicLibrary.Utilities.EmailRegEx))
                msgs.Add("Email must be valid (ex: name@domain.com)", MessageType.UserError);

            if (phone.IsEmpty())
                msgs.Add("Phone is required", MessageType.UserError);
            else if (!phone.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                msgs.Add("Phone must be a valid phone number", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        newAccount = AccountService.CreateNew(
                            orgName: orgname,
                            email: email,
                            phone: phone,
                            isActive: true,
                            accountType: (AccountType) accounttype.ToInteger(),
                            accountOwner: SessionVariables.CurrentUser,
                            packageId: Package.LoadByIsActiveByIsAfterSchool(true, true, amsgs, adc).Last().ID,
                            packageBillingPeriod: BillingPeriodType.Month,
                            timeZone: null,
                            loggedInUser: SessionVariables.CurrentUser,
                            isAfterSchool: true,
                            createWithStripe: true,
                            msgs: amsgs,
                            dc: adc);

                        newAccount.AddUser(SessionVariables.CurrentUser, UserType.Owner, SessionVariables.CurrentUser,
                            amsgs, adc);

                        if (invitecode.IsGuid())
                        {
                            invitingAccount = Account.LoadByInviteCodeToShare(invitecode.ToGuid(), amsgs, adc);
                            invitingAccount.ApproveAccessToAccount(newAccount, SessionVariables.CurrentUser, amsgs,
                                adc);
                        }
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        LoadSessionInfo(SessionVariables.CurrentUser, newAccount);
                    },
                    onFailure: (SystemMessages fmsgs) => { msgs.AddRange(fmsgs); }
                );
            }

            if (msgs.HasNoErrors())
                return LoginRedirect();

            ViewBag.Messages = msgs;
            ViewBag.User = SessionVariables.CurrentUser;
            ViewBag.InviteCode = invitecode;

            return View(newAccount);
        }

        [HttpGet]
        public ActionResult Claim(string invite)
        {
            var msgs = new SystemMessages();

            if (!invite.IsGuid())
                msgs.Add("Invalid invitation code", MessageType.UserError);

            var invitingAccount = Account.LoadByInviteCodeToShare(invite.ToGuid());

            var signupIndexVM = new SignUpIndexViewModel()
            {
                InviteCodeToShare = invite,
                Account = invitingAccount,
                IsClaimProcess = true,
            };

            ViewBag.Messages = new SystemMessages();
            return View("User", signupIndexVM);
        }

        [HttpGet]
        public ActionResult User(string invite)
        {
            var msgs = new SystemMessages();

            if (!invite.IsGuid())
                msgs.Add("Invalid invitation code", MessageType.UserError);

            var invitingAccount = Account.LoadByInviteCodeToShare(invite.ToGuid());

            var signupIndexVM = new SignUpIndexViewModel()
            {
                InviteCodeToShare = invite,
                Account = invitingAccount,
            };

            ViewBag.Messages = new SystemMessages();
            return View(signupIndexVM);
        }

        [HttpPost]
        public ActionResult User(SignUpIndexViewModel model)
        {
            var msgs = new SystemMessages();
            Account invitingAccount = null;

            if (model.FirstName.IsEmpty())
                msgs.Add("First Name is required", MessageType.UserError);

            if (model.LastName.IsEmpty())
                msgs.Add("Last Name is required", MessageType.UserError);

            if (!model.Email.IsEmail())
                msgs.Add("Please enter a valid email address", MessageType.UserError);
            else if (WebUser.ExistsByEmail(model.Email, msgs))
                msgs.Add("This email has already been registered", MessageType.UserError);

            if (model.Password1.IsEmpty() || model.Password2.IsEmpty() || (model.Password1 != model.Password2))
                msgs.Add("Passwords are required and must match", MessageType.UserError);
            else if (model.Password1.Length < 8)
                msgs.Add("Password must be at least 8 characters", MessageType.UserError);

            if (!model.InviteCodeToShare.IsGuid())
                msgs.Add("Invalid invitation code", MessageType.UserError);

            // msgs.AddRange(RecaptchaHelper.Validate(Request["g-recaptcha-response"]));

            invitingAccount = Account.LoadByInviteCodeToShare(model.InviteCodeToShare.ToGuid());
            if (invitingAccount.IsEmpty())
                msgs.Add("Invalid invitation code", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                WebUser user = null;

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        user = WebUserService.CreateNewWithNoPhone(
                            firstName: model.FirstName,
                            lastName: model.LastName,
                            email: model.Email,
                            password: model.Password2,
                            isActive: true,
                            isRegistered: true,
                            isVerified: false,
                            loggedInUser: new WebUser {ID = 0},
                            howHearAboutUs: model.HowHearAboutUs,
                            msgs: amsgs,
                            dc: adc);

                        invitingAccount.AddUser(user, UserType.Administrator, new WebUser {ID = 0}, amsgs, adc);
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        LoadSessionInfo(user, invitingAccount, msgs: msgs);

                        var userId = user.ID.Value;

                        BackgroundJob.Enqueue(() => WebUserService.SendVerificationEmail(userId));
                    },
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => msgs.AddRange(emsgs)
                );
            }

            if (msgs.HasNoErrors())
            {
                if (model.IsClaimProcess)
                    return Redirect($"{invitingAccount.HomePageUrl}?edit=1");

                return RedirectToAction("index", "home", new {area = "org"});
            }

            ViewBag.Messages = msgs;

            return View(model);
        }

        /* APIS */
        [HttpPost]
        public ActionResult Personal(string firstname, string lastname, string email, string password, string remember,
            string howhearaboutus)
        {
            if (!IsRequestFromApp())
                return Json(new {status = "error", messages = new SystemMessages()}, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();

            if (firstname.IsBlank())
                msgs.Add("First Name is required", MessageType.UserError);

            if (lastname.IsBlank())
                msgs.Add("Last Name is required", MessageType.UserError);

            if (email.IsBlank() || !email.IsRegExMatch(BasicLibrary.Utilities.EmailRegEx))
                msgs.Add("Email is required and must be a valid email address", MessageType.UserError);

            if (password.IsEmpty())
                msgs.Add("Password is required", MessageType.UserError);
            else if (password.Length < 8)
                msgs.Add("Password must be at least 8 characters", MessageType.UserError);

            WebUser user = null;
            Account account = null;

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        user = WebUserService.CreateNewWithNoPhone(
                            firstName: firstname,
                            lastName: lastname,
                            email: email,
                            password: password,
                            isActive: true,
                            isRegistered: true,
                            isVerified: false,
                            loggedInUser: null,
                            howHearAboutUs: howhearaboutus,
                            msgs: amsgs,
                            dc: adc);

                        if (user.IsNotEmpty())
                            account = user.AddNewRegistrantAccount(userType: UserType.Owner, isActive: true,
                                isAfterSchool: true, loggedInUser: user, msgs: amsgs, dc: adc);
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);

                        if (user.IsNotEmpty())
                        {
                            if (account.IsNotEmpty())
                                LoadSessionInfo(user: user, account: account, doRememberMe: remember.ToBoolean(),
                                    msgs: msgs);

                            var userId = user.ID.Value;
                            BackgroundJob.Enqueue(() => WebUserService.SendVerificationEmail(userId));
                        }
                    },
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                );

                if (msgs.HasNoErrors())
                    return Json(new {status = "ok", isNewAccount = true}, JsonRequestBehavior.AllowGet);
            }

            return Json(new {status = "error", messages = msgs.ToList().Select(msg => new {msg.Text})},
                JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Welcome(string invite, string isrequestonlyaccess, string signuptype)
        {
            Account invitingAccount = null;

            if (invite.IsGuid())
            {
                invitingAccount = Account.LoadByInviteCodeToShare(invite.ToGuid());

                var currentAccount = SessionVariables.CurrentAccount;
                var currentUser = SessionVariables.CurrentUser;

                if (currentAccount.IsNotEmpty() && currentUser.IsNotEmpty())
                {
                    if (currentUser.HasMultipleAccounts() || currentAccount.AccountTypeId != AccountType.Registrant)
                        return RedirectToAction("index", "login", new {returnurl = Request["returnurl"], invite});
                }
            }

            var model = new SignUpIndexViewModel()
            {
                IsRequestOnlyAccess = isrequestonlyaccess.IsBoolean() && isrequestonlyaccess.ToBoolean(),
                InviteCodeToShare = invite.IsNotEmpty() ? invite : "",
                SignupType = signuptype.IsNotEmpty() && signuptype.IsInteger()
                    ? (SignUpType) signuptype.ToInteger()
                    : SignUpType.NotSet,
                InvitingAccount = invitingAccount,
            };
            return View(model);
        }

        /// <summary>
        ///     we already have this method in service controller, but I created a serate one so if needed, we can change it
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual JsonResult GetData(string search)
        {
            if (!IsRequestFromApp())
                return Json(new {status = "error", messages = new SystemMessages()}, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();
            var accounts =
                Account.FullTextSearch(search.Trim(), FormConstants.ProviderAccountTypes.ToList(), 0, 10, msgs);

            return Json(new
            {
                success = true,
                status = "ok",
                accounts = accounts.Select(account => new OrgDropDown()
                {
                    Id = account.Uid,
                    Slug = account.HomePageUrl,
                    Text = GetAccountName(account),
                    IsClaimed = account.IsClaimed,
                }),
            }, JsonRequestBehavior.AllowGet);
        }

        public static string GetAccountName(Account account)
        {
            var cityState = account.GetDefaultLocation()?.FormatCityStateZipCode();
            if (cityState.IsNotBlank())
            {
                //ex.Darye's Enrichment Club - Indianapolis, IN
                return $"{account.Name} - {cityState}";
            }
            else
            {
                return account.Name;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual JsonResult SaveClaims(string firstName, string lastName, string email, string phone,
            string accountId)
        {
            var msgs = new SystemMessages();

            if (!accountId.IsGuid())
                msgs.Add("Organization not found.", MessageType.UserError);

            if (firstName.IsBlank())
                msgs.Add("First Name is required", MessageType.UserError);

            if (email.IsBlank())
                msgs.Add("Email is required", MessageType.UserError);
            else if (!email.IsEmail())
                msgs.Add("Please enter a valid email address", MessageType.UserError);

            // get organization/account here
            var account = Account.LoadByUid(accountId.ToGuid(), msgs);

            if (account.IsEmpty())
                msgs.Add("Organization not found.", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                try
                {
                    // account is claimed, send the owner of the profile an email with the details of the request
                    if (account.IsClaimed)
                    {
                        EmailHelper.SendProfileClaimedEmail(account, firstName, lastName, email, msgs);
                        return Json(new {success = true, status = "ok", data = true}, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var userEmail = new MailAddress(email);
                        var userEmailDomain = userEmail.Host;
                        var user = WebUserService.StartUserAccount(firstName, lastName, email, msgs);

                        // if the email is the exact match of what the Account.Email is, OR
                        // if account has ClaimEmailDomain and user email's domain matches with that, send email with instructions on how to login 
                        // else if email is not recognized, send email to team member
                        if (email.IsSameAs(account.Email) ||
                            (account.ClaimEmailDomain.IsNotBlank() && userEmailDomain.IsNotBlank() &&
                             account.ClaimEmailDomain.IsSameAs(userEmailDomain)))
                        {
                            EmailHelper.SendClaimProfileEmail(account, user.Uid, email, msgs);
                            return Json(new {success = true, status = "ok"}, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            // email domain is not recognized, send email with instructions to organization member's email so that user can gain accss.
                            // the user will need to start at the sign up page, it should already have the account preset so that the user starts with the account already associated.
                            EmailHelper.SendNonRecognizedProfileClaimEmail(account, user, msgs);

                            //Note: masking the email with a fixed number of characters will make it difficult to guess the email and is slightly more secure.
                            var maskedEmail =
                                $"{account.Email[0]}****{account.Email.Substring(account.Email.IndexOf('@') - 2)}";
                            return Json(new {success = true, status = "ok", data = maskedEmail},
                                JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json(new {success = false, status = "error", data = ex.Message},
                        JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new {success = false, status = "error", data = msgs.ToList().Select(msg => new {msg.Text})},
                JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public virtual JsonResult ContactSupport(string firstName, string lastName, string email, string phone,
            string accountId)
        {
            var msgs = new SystemMessages();

            if (!accountId.IsGuid())
                msgs.Add("Organization not found.", MessageType.UserError);

            if (firstName.IsBlank())
                msgs.Add("First Name is required", MessageType.UserError);

            if (email.IsBlank())
                msgs.Add("Email is required", MessageType.UserError);
            else if (!email.IsEmail())
                msgs.Add("Please enter a valid email address", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                try
                {
                    var account = Account.LoadByUid(accountId.ToGuid(), msgs);

                    EmailHelper.SendEmail(Email.AppSupportEmailAddress,
                        $"AHQ Profile Claim Form - {account.Name}",
                        $"<p>Organization: {account.Name}<br />Name: {firstName} {lastName}<br />Email: {email}<br />Phone: {phone}<br /></p>",
                        true);

                    return Json(new {success = true, status = "ok"}, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new {success = false, status = "error", data = ex.Message},
                        JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new {success = false, status = "error", data = msgs.ToList().Select(msg => new {msg.Text})},
                JsonRequestBehavior.AllowGet);
        }
    }
}
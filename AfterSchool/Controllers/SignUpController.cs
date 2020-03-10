using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary.DataAccess;
using Hangfire;
using System.Collections.Generic;

namespace AfterSchool.Controllers
{
    public class SignUpController : BaseController
    {
        [HttpGet]
        public ActionResult Index(string invite)
        {
            return Redirect("https://myahq.com/transform/");

            var signupIndexVM = new SignUpIndexViewModel()
            {
                FirstName = CookieVariables.DemoFirstName,
                Email = CookieVariables.DemoEmailAddress,
                OrgName = CookieVariables.DemoOrgName,
                InviteCodeToShare = invite
            };

            ViewBag.Messages = new SystemMessages();
            return View(signupIndexVM);
        }

        [HttpPost]
        public ActionResult Index(SignUpIndexViewModel model)
        {
            var msgs = new SystemMessages();
            Account invitingAccount = null;

            if (model.OrgName.IsEmpty())
                msgs.Add("Organization / Program Name is required", MessageType.UserError);

            if (!model.OrgType.HasValue)
                msgs.Add("Organization Type is required", MessageType.UserError);

            if (!model.OrgPhone.IsPhone())
                msgs.Add("Organization Phone is required", MessageType.UserError);

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
                            msgs: amsgs,
                            dc: adc);

                        if (user.IsNotEmpty())
                            account = user.AddNewAccount(
                                orgName: model.OrgName,
                                phone: model.OrgPhone,
                                isActive: true,
                                accountType: model.OrgType.Value,
                                addDefaultPackage: true,
                                timeZone: model.TimeZone,
                                isAfterSchool: true,
                                loggedInUser: user,
                                msgs: amsgs,
                                dc: adc);

                        if (invitingAccount.IsNotEmpty() && account.IsNotEmpty())
                            invitingAccount.ApproveAccessToAccount(account, user, amsgs, adc);

                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        LoadSessionInfo(user, account, msgs: msgs);

                        var userId = user.ID.Value;
                        var accountId = account.ID.Value;

                        BackgroundJob.Enqueue(() => AccountService.SendWelcomeEmailPurchase(accountId));
                        BackgroundJob.Enqueue(() => WebUserService.SendVerificationEmail(userId));
                    },
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => msgs.AddRange(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return LoginRedirect();

            ViewBag.Messages = msgs;

            return View(model);
        }

        [HttpGet]
        public ActionResult New(string id, string a)
        {
            // if logged in while signing up a new account logout to prevent confusion
            Logout();

            var msgs = new SystemMessages();
            WebUser wu = null;

            if(!id.IsGuid() || !a.IsGuid())
                msgs.Add("Invalid invitation code", MessageType.UserError);
            else
            {
                wu = WebUser.LoadByUid(id.ToGuid(), null);
                if(wu.IsEmpty())
                    msgs.Add("Invalid invitation code", MessageType.UserError);
                else if(wu.IsActive)
                    msgs.Add("Account has already been created. Please proceed to the login page", MessageType.UserError);
            }
            
            ViewBag.Messages = msgs;
            ViewBag.Uid = id;
            ViewBag.AccountUid = a;
            return View(wu);
        }

        [HttpPost]
        public ActionResult New(string id, string a, string firstname, string lastname, string email, string password1, string password2)
        {
            var msgs = new SystemMessages();
            WebUser user = null;
            Account account = null;

            if(!id.IsGuid())
                msgs.Add("Invalid invitation code", MessageType.UserError);
            else
            {
                user = WebUser.LoadByUid(id.ToGuid(), null);

                if (user.IsEmpty())
                    msgs.Add("Invalid invitation code", MessageType.UserError);
                else if (user.IsActive)
                    msgs.Add("Account has already been created. Please proceed to the login page", MessageType.UserError);
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

            if(email.IsEmpty())
                msgs.Add("Email is required", MessageType.UserError);
            else if(!email.IsMatch(BasicLibrary.Utilities.EmailRegEx))
                msgs.Add("Please enter a valid email address", MessageType.UserError);
            else if(!user.CanEmailBeUsed(email, msgs))
                msgs.Add("This email has already been registered", MessageType.UserError);

            if (password1.IsEmpty() || password2.IsEmpty() || (password1 != password2))
                msgs.Add("Passwords are required and must match", MessageType.UserError);
            else if (password1.Length < 8)
                msgs.Add("Password must be at least 8 characters", MessageType.UserError);

            if (msgs.HasNoErrors())
                user.CompleteSignUp(account, firstname, lastname, email, password1, null, msgs);

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
        public ActionResult Verify(string id)
        {
            var msgs = new SystemMessages();

            if (!id.IsGuid())
            {
                msgs.Add("Invalid verification code", MessageType.UserError);
            }
            else if (!WebUser.ExistsByUid(id.ToGuid(), msgs))
            { 
                msgs.Add("Invalid verification code", MessageType.UserError);
            }

            if(msgs.HasNoErrors())
            {
                msgs.AddRange(WebUser.LoadByUid(id.ToGuid()).MarkAsVerified());

                if(msgs.HasNoErrors())
                {
                    msgs.Add("Your account has been verified", MessageType.Success);

                    // if logged in reload with new verified value
                    if(SessionVariables.CurrentUser.IsNotEmpty() && SessionVariables.CurrentUser.Uid == id.ToGuid())
                    {
                        SessionVariables.CurrentUser = WebUser.Load(SessionVariables.CurrentUser.ID.Value);
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
            else if(!Request.QueryString["i"].IsGuid())
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
                    action: (SystemMessages amsgs, BasicLibrary.DataAccess.DatabaseConnection adc) =>
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

                                if (System.IO.Directory.Exists(uploadPath) == false) System.IO.Directory.CreateDirectory(uploadPath);
                                if (System.IO.File.Exists(fullFilePath) == false) System.IO.File.Delete(fullFilePath);
                                if (System.IO.File.Exists(thumbnailFilePath) == false) System.IO.File.Delete(thumbnailFilePath);

                                fileUpload.SaveAs(fullFilePath);

                                RebornImage.CreateThumbnailMaxDimension(300, fullFilePath, thumbnailFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);

                                wu.ImageUrl = "/user/image/" + wu.Username + "/";
                            }
                        }

                        amsgs.AddRange(wu.Save(adc));
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
            }

            if (msgs.HasAny(SystemMessages.ErrorTypes) == false)
            {
                Response.Redirect("/signup/confirm");
            }

            ViewData["msgs"] = msgs;

            return View();
        }

        [HttpGet]
        public ActionResult Confirm(string id)
        {
            var msgs = new SystemMessages();
            WebUser user = null;
            if (id.IsGuid())
            {
                user = WebUser.LoadByUid(id.ToGuid());
                if(user.IsRegistered)
                {
                    user = null;
                }
            }

            if (user.IsEmpty())
            {
                msgs.Add("Invalid id", MessageType.UserError);
            }

            ViewBag.Messages = msgs;
            return View(user);
        }

        [HttpPost]
        public ActionResult Confirm(string id, string firstname, string lastname, string email, string password1, string password2)
        {
            var msgs = new SystemMessages();

            if (!id.IsGuid())
            {
                msgs.Add("Invalid id", MessageType.UserError);
            }
            if (firstname.IsEmpty())
            {
                msgs.Add("First Name is required", MessageType.UserError);
            }
            if (lastname.IsEmpty())
            {
                msgs.Add("First Name is required", MessageType.UserError);
            }
            if (email.IsEmpty() || !email.IsMatch(BasicLibrary.Utilities.EmailRegEx))
            {
                msgs.Add("Email is required", MessageType.UserError);
            }
            if (password1.IsEmpty())
            {
                msgs.Add("Password is required", MessageType.UserError);
            }
            if (password2.IsEmpty())
            {
                msgs.Add("Verify Password is required", MessageType.UserError);
            }
            if (password1 != password2)
            {
                msgs.Add("Passwords must match", MessageType.UserError);
            }

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
                user.IsRegistered = true;
                user.IsActive = true;
                user.Password = WebUserService.EncryptPassword(password1);
                msgs.AddRange(user.Save());

                user.SendVerificationEmail(SessionVariables.IsAfterSchool.Value);

                SessionVariables.CurrentUser = user;

                return RedirectToAction("index", "home", new { area = "user" });
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

            var accountTypesToAccept = new List<AccountType>() { AccountType.ActivityProvider, AccountType.School };           

            // auto accept invite of already logged in
            if (SessionVariables.CurrentUser.IsNotEmpty() && SessionVariables.CurrentAccount.AccountTypeId.In(accountTypesToAccept) && SessionVariables.CurrentAccount.ID != invitingAccount.ID)
            {
                invitingAccount.ApproveAccessToAccount(SessionVariables.CurrentAccount, SessionVariables.CurrentUser, msgs);

                return LoginRedirect();
            }

            ViewBag.Messages = msgs;
            ViewBag.User = new WebUser();

            return View(invitingAccount);
        }

        [HttpPost]
        public ActionResult Invite(string id, string firstname, string lastname, string email, string phone, string password1, string password2)
        {
            var msgs = new SystemMessages();
            var user = new WebUser
            {
                FirstName = firstname,
                LastName = lastname,
                Email = email,
                Phone = phone
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
                msgs.Add("This email has already been registered. Login if you already have an account.", MessageType.UserError);

            if (phone.IsEmpty())
                msgs.Add("Phone is required", MessageType.UserError);
            else if (!phone.IsMatch(BasicLibrary.Utilities.PhoneRegEx))
                msgs.Add("Please enter a valid phone number", MessageType.UserError);
            else if (WebUser.ExistsByPhone(phone, msgs))
                msgs.Add("This phone has already been registered. Login if you already have an account.", MessageType.UserError);

            if (password1.IsEmpty() || password2.IsEmpty() || (password1 != password2))
                msgs.Add("Passwords are required and must match", MessageType.UserError);
            else if (password1.Length < 8)
                msgs.Add("Password must be at least 8 characters", MessageType.UserError);            

            if (msgs.HasNoErrors())
            {
                // create a new user, then take then to create a real provider account
                user = WebUserService.CreateNew(firstname, lastname, email, phone, password1, true, true, false, false, null);

                if (msgs.HasNoErrors() && !user.VerifiedAccount)
                    msgs.AddRange(user.SendVerificationEmail(SessionVariables.IsAfterSchool.Value));
            }

            if (msgs.HasNoErrors())
                LoadSessionInfo(user, msgs: msgs);

            if (msgs.HasNoErrors())
                return RedirectToAction("createaccount", new { invitecode = invitingAccount.InviteCodeToShare });

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

            return View(new Account() { Email = SessionVariables.CurrentUser.Email, Phone = SessionVariables.CurrentUser.Phone });
        }

        [HttpPost]
        public ActionResult CreateAccount(string invitecode, string orgname, string email, string phone, string accounttype)
        {
            var msgs = new SystemMessages();
            Account invitingAccount = null;
            var newAccount = new Account()
            {
                Name = orgname,
                Phone = phone,
                Email = email
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
                            accountType: (AccountType)accounttype.ToInteger(),
                            accountOwner: SessionVariables.CurrentUser,
                            packageId: Package.LoadByIsActiveByIsAfterSchool(true, true, amsgs, adc).Last().ID,
                            packageBillingPeriod: BillingPeriodType.Month,
                            timeZone: null,
                            loggedInUser: SessionVariables.CurrentUser,
                            isAfterSchool: true,
                            createWithStripe: true,
                            msgs: amsgs, 
                            dc: adc);

                        newAccount.AddUser(SessionVariables.CurrentUser, UserType.Owner, SessionVariables.CurrentUser, amsgs, adc);

                        if (invitecode.IsGuid())
                        {
                            invitingAccount = Account.LoadByInviteCodeToShare(invitecode.ToGuid(), amsgs, adc);
                            invitingAccount.ApproveAccessToAccount(newAccount, SessionVariables.CurrentUser, amsgs, adc);
                        }   
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        LoadSessionInfo(SessionVariables.CurrentUser, newAccount);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
            }

            if (msgs.HasNoErrors())
                return LoginRedirect();

            ViewBag.Messages = msgs;
            ViewBag.User = SessionVariables.CurrentUser;
            ViewBag.InviteCode = invitecode;

            return View(newAccount);
        }
    }
}

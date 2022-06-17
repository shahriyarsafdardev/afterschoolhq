using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using BasicLibrary.DataAccess;

namespace AfterSchool.Areas.Admin.Controllers
{
    public class UserController : BaseLoggedInController
    {
        public UserController() : base(new List<AccountType> { AccountType.AfterSchool })
        {

        }

        // GET: User
        [HttpGet]
        public ActionResult Index(string pn, string rpp, string search)
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

            int userCount = WebUser.GetSearchCount(search, msgs);
            var users = WebUser.Search(search, start, length);
            var totalPages = Math.Ceiling(userCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Messages = msgs;
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = userCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };

            return View(users);
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            ViewBag.Messages = new SystemMessages();

            if (Request["returnurl"].IsNotEmpty())
                ViewBag.ReturnUrl = Request["returnurl"];

            if (id.IsEmpty())
                return View(new WebUser() { FirstName = "New", LastName = "User" });

            return View(WebUser.LoadByUid(id.ToGuid()));
        }

        [HttpPost]
        public ActionResult Edit(string id, string firstname, string lastname, string email, string phone, string password1, string password2, string isactive, string sysadmin)
        {
            var msgs = new SystemMessages();
            WebUser user = null; 

            if (id.IsGuid())
                user = WebUser.LoadByUid(id.ToGuid()); 

            if (!email.IsEmail())
                msgs.Add("Email must be a valid email address", MessageType.UserError);
            else if (user.IsNotEmpty() && !user.Email.IsSameAs(email) && WebUser.ExistsByEmail(email, msgs))
                msgs.Add("Email is already in use", MessageType.UserError);

            if (password1 != password2)
                msgs.Add("Password do not match", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        if (user.IsEmpty())
                            user = new WebUser()
                            {
                                CreateDate = DateTime.UtcNow,
                                CreatedBy = SessionVariables.CurrentUser.ID,
                                FirstName = firstname,
                                LastName = lastname,
                                Email = email,
                                Phone = phone,
                                LastModifiedDate = DateTime.UtcNow,
                                LastModifiedBy = SessionVariables.CurrentUser.ID,
                                IsActive = !isactive.IsEmpty(),
                            };

                        if (password1.IsNotEmpty() && password2.IsNotEmpty())
                            user.Password = WebUserService.EncryptPassword(password1);

                        msgs.AddRange(user.Save(adc));

                        if (sysadmin.IsNumber())
                            AccountService.GetAfterSchoolAccount(amsgs, adc)
                                .AddUser(
                                    user,
                                    UserType.Administrator,
                                    SessionVariables.CurrentUser,
                                    amsgs,
                                    adc);
                    },
                    onSuccess: (SystemMessages smsgs) => 
                    {
                        msgs.AddRange(smsgs);
                        msgs.Add("Save successful", MessageType.Success);

                        if (sysadmin.IsNumber())
                            EmailHelper.SendEmailToSupport(
                                $"ALERT: A new Super Admin Account has been created: {user.Email}",
                                $"<p>{user.DisplayName}<br/>{user.Email}<br />{user.Phone}</p>"
                            );
                    },
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            ViewBag.Messages = msgs;
            return View(user);
        }

        [HttpGet]
        public ActionResult MyProfile()
        {
            return RedirectToAction("edit", new { id = SessionVariables.CurrentUser.Uid.ToString() });
        }

        [HttpGet]
        public ActionResult Invite(string id, string returnUrl)
        {
            ViewBag.AccountId = id;
            ViewBag.Messages = new SystemMessages();
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Invite(string id, string emails, string message)
        {
            SystemMessages msgs = new SystemMessages();

            if (emails.IsEmpty())
            {
                msgs.Add("Please enter at least one email address", MessageType.UserError);
            }
            else
            {
                foreach(string email in emails.Split(',').Select(x=>x.Trim()))
                {
                    if(!email.IsMatch(BasicLibrary.Utilities.EmailRegEx))
                    {
                        msgs.Add(string.Format("{0} is not a valid email address", email), MessageType.UserError);
                    }
                }
            }
            
            if(msgs.HasNoErrors())
            {
                Account a = null;
                UserType ut;

                if (!id.IsNumber())
                {
                    a = SessionVariables.CurrentAccount;
                    ut = UserType.SuperAdmin;
                }
                else
                {
                    a = Account.Load(id.ToInteger());
                    ut = UserType.Administrator;
                }

                foreach (string email in emails.Split(',').Select(x => x.Trim()))
                {
                    a.InviteUser("", "", email, null, Request.Params["message"], ut, SessionVariables.CurrentUser, msgs);
                }
            }

            if (msgs.HasNoErrors())
            {
                msgs.Add("All invites have been sent", MessageType.Success);

                if (Request["returnurl"].IsNotBlank())
                {
                    Response.Redirect(Request["returnurl"], true);
                }
            }

            ViewBag.Messages = msgs;
            return View();
        }


        [HttpGet]
        public ActionResult Impersonate(string id, string uid)
        {
            var msgs = new SystemMessages();
            WebUser user = null;

            if (uid.IsEmpty() && id.IsNumber())
                user = Account.Load(id.ToInteger()).GetAccountOwner();
            else if(uid.IsGuid())
                user = WebUser.LoadByUid(uid.ToGuid());
            else
                msgs.Add("Invalid params", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                SessionVariables.OriginalLoggedInUser = SessionVariables.CurrentUser;
                LoadSessionInfo(user, null, false, msgs);
            }

            if (msgs.HasNoErrors())
            {
                if (SessionVariables.CurrentAccount.IsEmpty())
                    return RedirectToAction("select", "login", new { Area = "" });

                return LoginRedirect();
            }
            else
            {
                SessionVariables.OriginalLoggedInUser = null;
                ViewBag.Messages = msgs;

                if (id.IsNotEmpty())
                    return RedirectToAction("Team", "Account", new { id });
                else
                    return RedirectToAction("Index", "User");
            }
        }

    }
}
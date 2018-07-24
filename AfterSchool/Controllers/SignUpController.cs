using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using BasicLibrary;
using AfterSchool.Models;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;

namespace AfterSchool.Controllers
{
    public class SignUpController : BaseController
    {
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
            {
                msgs.Add("Invalid invitation code", MessageType.UserError);
            }
            else
            {
                user = WebUser.LoadByUid(id.ToGuid(), null);
                if (user.IsEmpty())
                {
                    msgs.Add("Invalid invitation code", MessageType.UserError);
                }
                else if (user.IsActive)
                {
                    msgs.Add("Account has already been created. Please proceed to the login page", MessageType.UserError);
                }
            }

            if (!a.IsGuid())
            {
                msgs.Add("Invalid invitation code", MessageType.UserError);
            }
            else
            {
                account = Account.LoadByUid(a.ToGuid());
                if (account.IsEmpty())
                {
                    msgs.Add("Invalid invitation code", MessageType.UserError);
                }
            }

            if (firstname.IsEmpty())
            {
                msgs.Add("First Name is required", MessageType.UserError);
            }
            if (lastname.IsEmpty())
            {
                msgs.Add("Last Name is required", MessageType.UserError);
            }
            if(email.IsEmpty())
            {
                msgs.Add("Email is required", MessageType.UserError);
            }
            else if(!email.IsMatch(BasicLibrary.Utilities.EmailRegEx))
            {
                msgs.Add("Please enter a valid email address", MessageType.UserError);
            }
            else if(!user.CanEmailBeUsed(email))
            {
                msgs.Add("This email has already been registered", MessageType.UserError);
            }
            if (password1.IsEmpty() || password2.IsEmpty() || (password1 != password2))
            {
                msgs.Add("Passwords are required and must match", MessageType.UserError);
            }
            else if (password1.Length < 8)
            {
                msgs.Add("Password must be at least 8 characters", MessageType.UserError);
            }           

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, BasicLibrary.DataAccess.DatabaseConnection adc) =>
                    {
                        msgs.AddRange(user.CompleteSignUp(account, firstname, lastname, email, password1, null, adc));
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        if (!user.VerifiedAccount)
                            msgs.AddRange(user.SendVerificationEmail(SessionVariables.IsAfterSchool.Value));
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
            }

            if (msgs.HasNoErrors())
                msgs.AddRange(LoadSessionInfo(user));

            if (msgs.HasNoErrors())
            {
                return LoginRedirect();
            }
            else
            {
                ViewBag.Messages = msgs;
                ViewBag.AccountUid = a;
                ViewBag.Uid = id;
                return View(user);
            }
        }

        [HttpGet]
        public ActionResult Verify(string id)
        {
            var msgs = new SystemMessages();

            if (!id.IsGuid())
            {
                msgs.Add("Invalid verification code", MessageType.UserError);
            }
            else if (!WebUser.ExistsByUid(id.ToGuid()))
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
                    if(SessionVariables.LoggedInUser.IsNotEmpty() && SessionVariables.LoggedInUser.Uid == id.ToGuid())
                    {
                        SessionVariables.LoggedInUser = WebUser.Load(SessionVariables.LoggedInUser.ID.Value);
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
            {
                msgs.Add("First Name is required", MessageType.UserError);
            }
            if (Request.Form["lastname"].IsEmpty())
            {
                msgs.Add("Last Name is required", MessageType.UserError);
            }
            if (Request.Form["email"].IsEmpty())
            {
                msgs.Add("Email is required", MessageType.UserError);
            }
            else if (Request.Form["email"].IsMatch(BasicLibrary.Utilities.EmailRegEx) == false)
            {
                msgs.Add("Email is invalid", MessageType.UserError);
            }
            if (Request.Form["username"].IsEmpty())
            {
                msgs.Add("Username is required", MessageType.UserError);
            }
            else if(Request.QueryString["i"].IsGuid() == false)
            {
                wu = WebUser.LoadByUsername(Request.Form["username"], null);
                if (wu.IsEmpty() == false)
                {
                    msgs.Add("Username is already in use", MessageType.UserError);
                }

                wu = WebUser.LoadByEmail(Request.Form["email"], null);
                if (wu.IsEmpty() == false)
                {
                    msgs.Add("Email is already in use", MessageType.UserError);
                }
            }
            if (Request.Form["password"].IsEmpty())
            {
                msgs.Add("Password is required", MessageType.UserError);
            }
            if (Request.Form["password2"].IsEmpty())
            {
                msgs.Add("Verify Password is required", MessageType.UserError);
            }
            if (Request.Form["password"].IsEmpty() == false && Request.Form["password"] != Request.Form["password2"])
            {
                msgs.Add("Passwords do not match", MessageType.UserError);
            }


            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, BasicLibrary.DataAccess.DatabaseConnection adc) =>
                    {
                        if (Request.QueryString["i"].IsGuid())
                        {
                            wu = WebUser.LoadByUidByIsActive(Request.QueryString["i"].ToGuid(), false, dc: adc);
                        }
                        else
                        {
                            wu = new WebUser();
                        }
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

                SessionVariables.LoggedInUser = user;

                return RedirectToAction("index", "home", new { area = "user" });
            }

            return View(user);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchool.Models;
using AfterSchool.Helpers;

namespace AfterSchool.Controllers
{
    public class PricingController : Controller
    {
        /*
        // GET: Pricing
        [HttpGet]
        public ActionResult Index()
        {
            return View(Package.LoadByIsActive(true));
        }
        */

        [HttpGet]
        public ActionResult SelectPackage(string id, string trial)
        {
            ViewBag.Messages = new SystemMessages();
            ViewBag.Trial = trial.IsBoolean() ? trial.ToBoolean() : false;
            return View(Package.LoadByUid(id.ToGuid()));
        }

        [HttpPost]
        public ActionResult CreateAccount(string id, string firstname, string lastname, string company, string phone, string email, string password1, string password2, string terms, string trial)
        {
            var msgs = new SystemMessages();

            if(firstname.IsEmpty())
            {
                msgs.Add("First Name is required", MessageType.UserError);
            }
            if (lastname.IsEmpty())
            {
                msgs.Add("Last Name is required", MessageType.UserError);
            }
            if (company.IsEmpty())
            {
                msgs.Add("Organization is required", MessageType.UserError);
            }
            if (phone.IsEmpty())
            {
                msgs.Add("Phone is required", MessageType.UserError);
            }
            if (email.IsEmpty())
            {
                msgs.Add("Email is required", MessageType.UserError);
            }
            else if(!email.IsMatch(BasicLibrary.Utilities.EmailRegEx))
            {
                msgs.Add("Email must valid", MessageType.UserError);
            }
            else if (WebUser.ExistsByEmail(email))
            {
                msgs.Add("Email address is already associated with an account", MessageType.UserError);
            }
            if (password1.IsEmpty() || password2.IsEmpty() || (password1 != password2))
            {
                msgs.Add("Passwords are required and must match", MessageType.UserError);
            }
            else if(password1.Length < 6)
            {
                msgs.Add("Password must be at least 6 characters", MessageType.UserError);
            }
            if(terms.IsEmpty())
            {
                msgs.Add("Please agree to the terms and conditions", MessageType.UserError);
            }

            msgs.AddRange(RecaptchaHelper.Validate(Request.Params["g-recaptcha-response"]));

            var isTrial = trial.IsBoolean() ? trial.ToBoolean() : false;

            var package = Package.LoadByUid(id.ToGuid());
            var selectedBillingPeriodType = BillingPeriodType.Month;
            AccountPackage accountPackage = null;

            if (msgs.HasNoErrors())
            {
                Account account = null;
                WebUser user = null;

                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, BasicLibrary.DataAccess.DatabaseConnection adc) =>
                    {
                        account = AccountService.NewAccount(
                            company: company,
                            email: email,
                            phone: phone,
                            isActive: true,
                            loggedInUserId: 0,
                            isAfterSchool: AfterSchool.Models.Utilities.IsAfterSchool(),
                            msgs: amsgs,
                            dc: adc
                            );

                        if (amsgs.HasNoErrors())
                        {
                            user = account.AddUser(
                                firstName: firstname,
                                lastName: lastname,
                                email: email,
                                password: password1,
                                isActive: true,
                                isRegistered: true,
                                isVerified: false,
                                ut: UserType.Owner,
                                loggedInUserId: 0,
                                msgs: amsgs,
                                dc: adc
                                );
                        }

                        if (amsgs.HasNoErrors())
                        {
                            if (package.IsFree(selectedBillingPeriodType) || package.HasFeature(FeatureType.TrialDays))
                            {
                                accountPackage = account.AddPackage(package.ID.Value, selectedBillingPeriodType, user, amsgs, adc);
                            }
                        }
                        if (amsgs.HasNoErrors())
                        {
                            //create default form
                            var form = account.GetDefaultForm(adc);
                            var defaultForm = form.Copy(user.ID.Value, account.ID.Value, amsgs, adc);
                        }
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);

                        SessionVariables.LoggedInUser = user;
                        SessionVariables.CurrentAccount = account;
                        SessionVariables.AccountUserType = UserType.Owner;
                        SessionVariables.AccountPackage = account.GetActiveAccountPackage();

                        if (isTrial)
                        {
                            SessionVariables.CurrentAccount.SendWelcomeEmailTrial();
                        }
                        else
                        {
                            SessionVariables.CurrentAccount.SendWelcomeEmailPurchase();
                        }
                        SessionVariables.LoggedInUser.SendVerificationEmail(SessionVariables.IsAfterSchool.Value);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );                
            }

            ViewBag.Messages = msgs;
            ViewBag.Trial = isTrial;

            if (msgs.HasNoErrors())
            {
                if(package.IsFree(selectedBillingPeriodType) || isTrial)
                {
                    return RedirectToAction("index", "home", new { area = "org" });
                }
                else
                {
                    return RedirectToAction("newbilling", "account", new { area = "org", id = package.Uid.ToString() });
                }
            }
            else
            {
                return View("SelectPackage", package);
            }
        }
    }
}
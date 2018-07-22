using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;
using AfterSchool.Models;

namespace AfterSchool.Areas.Org.Controllers
{
    public class AccountController : BaseLoggedInController
    {
        public AccountController() : base(new List<AccountType> { AccountType.School, AccountType.ActivityProvider })
        {

        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            return View(SessionVariables.CurrentAccount);
        }

        [HttpPost]
        public ActionResult Edit(string id, string name, string email, string phone, string uniquename, string isactive, string timezone)
        {
            SystemMessages msgs = new SystemMessages();
            Account a = SessionVariables.CurrentAccount;                                

            if (name.IsEmpty())
            {
                msgs.Add("Organization Name is required", MessageType.UserError);
            }

            a.Name = name;
            a.Phone = phone;
            a.UniqueName = uniquename;
            a.TimeZone = timezone;
            a.Email = email;

            if (msgs.HasNoErrors())
            {
                if (a.UniqueName.IsEmpty() && uniquename.IsEmpty())
                {
                    a.UniqueName = AccountService.GenerateUniqueName(name, a.ID);
                }
                else
                {
                    a.UniqueName = AccountService.GenerateUniqueName(uniquename, a.ID);
                }

                a.LastModifiedDate = DateTime.UtcNow;
                a.LastModifiedBy = SessionVariables.LoggedInUser.ID;
                msgs.AddRange(a.SaveWithStripe());

                if (msgs.HasNoErrors())
                {
                    msgs.Add("Saved Successfully", MessageType.Success);
                }
            }

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            return View(a);
        }

        [HttpGet]
        public ActionResult Location()
        {
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Messages = new SystemMessages();
            return View(SessionVariables.CurrentAccount);
        }

        [HttpPost]
        public ActionResult Location(string id, string address1, string address2, string city, string state, string postalcode, string country)
        {
            var msgs = new SystemMessages();
            var a = SessionVariables.CurrentAccount;

            a.Address1 = address1;
            a.Address2 = address2;
            a.City = city;
            a.State = state;
            a.PostalCode = postalcode;
            a.Country = country;

            if (msgs.HasNoErrors())
            {
                a.LastModifiedDate = DateTime.UtcNow;
                a.LastModifiedBy = SessionVariables.LoggedInUser.ID;
                msgs.AddRange(a.Save());

                if (msgs.HasNoErrors())
                {
                    msgs.Add("Saved Successfully", MessageType.Success);
                }
            }

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            return View(a);
        }

        [HttpGet]
        public ActionResult Style()
        {
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Messages = new SystemMessages();
            ViewBag.LandingPage = SessionVariables.CurrentAccount.GetAccountProperty(AccountPropertyType.LandingPageText);
            ViewBag.RegThankYou = SessionVariables.CurrentAccount.GetAccountProperty(AccountPropertyType.RegistrationThankYouEmail);
            return View(SessionVariables.CurrentAccount);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Style(string id, string logourl, string landingpagetext, string regthankyoutext)
        {
            var msgs = new SystemMessages();
            var a = SessionVariables.CurrentAccount;

            a.LogoUrl = logourl;

            var landingPage = a.GetAccountProperty(AccountPropertyType.LandingPageText);
            var thankyou = a.GetAccountProperty(AccountPropertyType.RegistrationThankYouEmail);

            if (landingPage.IsEmpty())
            {
                landingPage = new AccountProperty() { Value = landingpagetext };
                landingPage.AccountPropertyTypeId = AccountPropertyType.LandingPageText;
                landingPage.CreateDate = DateTime.UtcNow;
                landingPage.CreatedBy = SessionVariables.LoggedInUser.ID;
                landingPage.AccountId = a.ID;
            }
            else
            {
                landingPage.Value = landingpagetext;
            }

            if (thankyou.IsEmpty())
            {
                thankyou = new AccountProperty() { Value = regthankyoutext };
                thankyou.AccountPropertyTypeId = AccountPropertyType.RegistrationThankYouEmail;
                thankyou.CreateDate = DateTime.UtcNow;
                thankyou.CreatedBy = SessionVariables.LoggedInUser.ID;
                thankyou.AccountId = a.ID;
            }
            else
            {
                thankyou.Value = regthankyoutext;
            }

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) => {
                        a.LastModifiedDate = DateTime.UtcNow;
                        a.LastModifiedBy = SessionVariables.LoggedInUser.ID;
                        amsgs.AddRange(a.Save(adc));

                        if (amsgs.HasNoErrors())
                        {
                            landingPage.LastModifiedDate = DateTime.UtcNow;
                            landingPage.LastModifiedBy = SessionVariables.LoggedInUser.ID;
                            amsgs.AddRange(landingPage.Save(adc));
                        }

                        if (amsgs.HasNoErrors())
                        {
                            thankyou.LastModifiedDate = DateTime.UtcNow;
                            thankyou.LastModifiedBy = SessionVariables.LoggedInUser.ID;
                            amsgs.AddRange(thankyou.Save(adc));
                        }
                    },
                    onSuccess: (SystemMessages smsgs) => {
                        msgs.AddRange(smsgs);
                        msgs.Add("Saved Successfully", MessageType.Success);
                    },
                    onFailure: (SystemMessages fmsgs) => {
                        msgs.AddRange(fmsgs);
                    }
                );
                
            }

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.LandingPage = landingPage;
            ViewBag.RegThankYou = thankyou;
            return View(a);
        }

        [HttpGet]
        public ActionResult Integrations()
        {
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Messages = new SystemMessages();
            ViewBag.FacebookAdPixel = SessionVariables.CurrentAccount.GetAccountProperty(AccountPropertyType.FacebookAdPixel);
            return View(SessionVariables.CurrentAccount);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Integrations(string id, string facebookadpixelscript)
        {
            var msgs = new SystemMessages();
            var a = SessionVariables.CurrentAccount;

            var facebookAdPixel = a.GetAccountProperty(AccountPropertyType.FacebookAdPixel);

            if (facebookAdPixel.IsEmpty())
            {
                facebookAdPixel = new AccountProperty() { Value = facebookadpixelscript };
                facebookAdPixel.AccountPropertyTypeId = AccountPropertyType.FacebookAdPixel;
                facebookAdPixel.CreateDate = DateTime.UtcNow;
                facebookAdPixel.CreatedBy = SessionVariables.LoggedInUser.ID;
                facebookAdPixel.AccountId = a.ID;
            }
            else
            {
                facebookAdPixel.Value = facebookadpixelscript;
            }

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) => {

                        a.LastModifiedDate = DateTime.UtcNow;
                        a.LastModifiedBy = SessionVariables.LoggedInUser.ID;
                        amsgs.AddRange(a.Save(adc));

                        if (amsgs.HasNoErrors())
                        {
                            facebookAdPixel.LastModifiedDate = DateTime.UtcNow;
                            facebookAdPixel.LastModifiedBy = SessionVariables.LoggedInUser.ID;
                            amsgs.AddRange(facebookAdPixel.Save(adc));
                        }
                    },
                    onSuccess: (SystemMessages smsgs) => {
                        msgs.AddRange(smsgs);
                        msgs.Add("Saved Successfully", MessageType.Success);
                    },
                    onFailure: (SystemMessages fmsgs) => {
                        msgs.AddRange(fmsgs);
                    }
                );

            }

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.FacebookAdPixel = facebookAdPixel;
            return View(a);
        }

        [HttpGet]
        public ActionResult Detail()
        {
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Messages = new SystemMessages();
            return View(SessionVariables.CurrentAccount);
        }

        [HttpGet]
        public ActionResult Billing()
        {
            Account a = SessionVariables.CurrentAccount;
            ViewBag.BillingHistory = a.GetStripeBillingHistory();
            ViewBag.RefundHistory = a.GetStripeRefundHistory();
            ViewBag.Cards = a.GetStripeCards();
            ViewBag.Messages = new SystemMessages();
            return View(a);
        }

        [HttpGet]
        public ActionResult NewBilling(string id)
        {
            var account = SessionVariables.CurrentAccount;
            var package = id.IsGuid() ? Package.LoadByUid(id.ToGuid()) : SessionVariables.AccountPackage?.ToPackage();

            ViewBag.Package = package;
            ViewBag.Packages = Package.LoadByIsActiveByIsAfterSchool(true, SessionVariables.IsAfterSchool.Value);
            ViewBag.Messages = new SystemMessages();
            return View(account);
        }

        [HttpPost]
        public ActionResult NewPayment(string selectedpackage, string stripetoken)
        {
            var msgs = new SystemMessages();

            Package package = null;
            var a = SessionVariables.CurrentAccount;
            var isFromTrialAccount = SessionVariables.AccountPackage?.IsDuringTrial ?? false;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) => {

                    var selectedPackageArr = selectedpackage.Split('_');
                    var billingPeriod = (BillingPeriodType)selectedPackageArr[0].ToInteger();

                    package = Package.LoadByUid(selectedPackageArr[1].ToGuid(), amsgs, adc);

                    amsgs.AddRange(a.AddCreditCard(stripetoken, true, SessionVariables.LoggedInUser, adc));
                    if (amsgs.HasNoErrors())
                        a.AddPackage(package.ID.Value, billingPeriod, SessionVariables.LoggedInUser, amsgs, adc);

                    SessionVariables.AccountPackage = a.GetActiveAccountPackage(amsgs, adc);
                },
                onSuccess: (SystemMessages smsgs) => {
                    msgs.AddRange(smsgs);

                    msgs.Clear(MessageType.Success);
                    msgs.Add(string.Format("The '{0} Package' has been added to your account", package.Name), MessageType.Success);

                    if (isFromTrialAccount)
                        SessionVariables.CurrentAccount.SendWelcomeEmailPurchase();
                },
                onFailure: (SystemMessages fmsgs) => {
                    msgs.AddRange(fmsgs);
                }
            );            

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
            {
                ViewBag.BillingHistory = a.GetStripeBillingHistory();
                ViewBag.RefundHistory = a.GetStripeRefundHistory();
                ViewBag.Cards = a.GetStripeCards();
                return View("Billing", a);
            }
            else
            {
                ViewBag.Package = package;
                ViewBag.Packages = Package.LoadByIsActiveByIsAfterSchool(true, SessionVariables.IsAfterSchool.Value);
                return View("NewBilling", a);
            }
        }

        [HttpPost]
        public ActionResult AddCard(string id, string stripetoken, string makeprimary)
        {
            SystemMessages msgs = new SystemMessages();

            var a = SessionVariables.CurrentAccount;
            msgs.AddRange(a.AddCreditCard(stripetoken, !makeprimary.IsEmpty(), SessionVariables.LoggedInUser));
            ViewBag.Cards = a.GetStripeCards();
            ViewBag.Messages = msgs;
            return View("Billing", a);
        }

        [HttpGet]
        public ActionResult Packages()
        {
            Account a = SessionVariables.CurrentAccount;
            ViewBag.Packages = Package.LoadByIsActive(true);
            ViewBag.AccountPackage = a.GetActiveAccountPackage();
            ViewBag.Messages = new SystemMessages();
            return View(a);
        }

        [HttpGet]
        public ActionResult SelectPackage(string id)
        {
            SystemMessages msgs = new SystemMessages();

            Account a = SessionVariables.CurrentAccount;
            a.AddPackage(id.ToInteger(), BillingPeriodType.Month, SessionVariables.LoggedInUser, msgs);

            SessionVariables.AccountPackage = a.GetActiveAccountPackage();

            ViewBag.Packages = Package.LoadByIsActive(true);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Messages = msgs;
            return View("Packages", a);
        }

        [HttpGet]
        public ActionResult EntityPayments()
        {
            Account a = SessionVariables.CurrentAccount;
            ViewBag.Messages = new SystemMessages();
            return View(a);
        }

        [HttpPost]
        public ActionResult EntityPayments(string paypalemail)
        {
            var msgs = new SystemMessages();

            SessionVariables.CurrentAccount.PayPalEmailAddress = paypalemail;
            msgs.AddRange(SessionVariables.CurrentAccount.Save());

            if(msgs.HasNoErrors())
                msgs.Add("PayPal information saved", MessageType.Success);

            ViewBag.Messages = msgs;
            return View(SessionVariables.CurrentAccount);
        }

        [HttpGet]
        public ActionResult BankInformationPayments()
        {
            Account a = SessionVariables.CurrentAccount;
            ViewBag.Messages = new SystemMessages();
            return View(a);
        }

        [HttpPost]
        public ActionResult BankInformationPayments(string paypalemail)
        {
            var msgs = new SystemMessages();

            SessionVariables.CurrentAccount.PayPalEmailAddress = paypalemail;
            msgs.AddRange(SessionVariables.CurrentAccount.Save());

            if (msgs.HasNoErrors())
                msgs.Add("PayPal information saved", MessageType.Success);

            ViewBag.Messages = msgs;
            return View(SessionVariables.CurrentAccount);
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
                                accountFeature.CreatedBy = SessionVariables.LoggedInUser.ID;
                                accountFeature.FeatureTypeId = type;
                            }

                            accountFeature.Max = Request[type.ToString()].ToInteger();
                            accountFeature.LastModifiedBy = SessionVariables.LoggedInUser.ID;
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

        public ActionResult OAuthStripe()
        {
            var msgs = new SystemMessages();

            if (Request["error"].IsEmpty())
            {
                try
                {
                    string scope = Request["scope"];
                    string code = Request["code"];
                    string state = Request["state"];

                    RestClient rc = new RestClient("https://connect.stripe.com/oauth/token");
                    rc.RequestMethod = RestClientRequestMethod.POST;
                    rc.Parameters.Add("client_secret", AfterSchoolLibrary.Billing.StripeHelper.SecretKey);
                    rc.Parameters.Add("code", code);
                    rc.Parameters.Add("grant_type", "authorization_code");
                    rc.Parameters.Add("client_id", AfterSchoolLibrary.Billing.StripeHelper.PlatformClientId);
                    rc.ContentType = RestClientContentTypeEnum.URLEncoded;
                    string responseFromServer = rc.DoRequest();

                    dynamic results = responseFromServer.FromJSON();

                    string token_type = results["token_type"];
                    string stripe_publishable_key = results["stripe_publishable_key"];
                    bool livemode = results["livemode"];
                    string stripe_user_id = results["stripe_user_id"];
                    string refresh_token = results["refresh_token"];
                    string access_token = results["access_token"];
                    scope = results["scope"];

                    SessionVariables.CurrentAccount.StripePublishableKey = stripe_publishable_key;
                    SessionVariables.CurrentAccount.StripeAccessToken = access_token;
                    SessionVariables.CurrentAccount.StripeRefreshToken = refresh_token;
                    SessionVariables.CurrentAccount.StripeUserId = stripe_user_id;
                    msgs.AddRange(SessionVariables.CurrentAccount.Save());

                    if(msgs.HasNoErrors())
                    {
                        msgs.Add("Your account has been connected to Stripe", MessageType.Success);
                    }
                    else
                    {
                        SessionVariables.CurrentAccount.StripePublishableKey = "";
                        SessionVariables.CurrentAccount.StripeAccessToken = "";
                        SessionVariables.CurrentAccount.StripeRefreshToken = "";
                        SessionVariables.CurrentAccount.StripeUserId = "";
                    }

                }
                catch (Exception ex)
                {
                    msgs.Add("An error occurred while connecting your Stripe account.", MessageType.Error, ex);
                }
            }
            else
            {
                msgs.Add("It appears you have canceled your request to connect your Stripe account.", MessageType.Notice);
            }

            ViewBag.Messages = msgs;

            return View("Payments", SessionVariables.CurrentAccount);
        }

        [HttpGet]
        public ActionResult Team(string id)
        {
            var members = SessionVariables.CurrentAccount.GetMembers().Select(x => x.ToWebUser()).ToList();
            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Members = members;
            ViewBag.NeedToUpgrade = members.Count >= SessionVariables.AccountPackage.GetFeature(FeatureType.TeamAccounts).GetMaxValue();
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
                    .ForEach(email => msgs.AddRange(
                        SessionVariables.CurrentAccount.InviteUser(
                            "", 
                            "", 
                            email, 
                            Request.Params["message"], 
                            (UserType)usertype.ToInteger(), 
                            SessionVariables.LoggedInUser)
                     ));

            if (msgs.HasNoErrors())
                msgs.Add("All invites have been sent", MessageType.Success);

            var members = SessionVariables.CurrentAccount.GetMembers().Select(x => x.ToWebUser()).ToList();
            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Members = members;
            ViewBag.NeedToUpgrade = members.Count >= SessionVariables.AccountPackage.GetFeature(FeatureType.TeamAccounts).GetMaxValue();
            return View("Team", SessionVariables.CurrentAccount);
        }
    }
}
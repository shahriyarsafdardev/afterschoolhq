using System;
using System.Linq;
using System.Web.Mvc;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Helpers;
using BasicLibrary;
using AfterSchool.Models;
using AfterSchool.Helpers;
using AfterSchoolLibrary.Services;
using AfterSchoolLibrary;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web.UI;

namespace AfterSchool.Controllers
{
    public class ServicesController : BaseController
    {
        // GET: Services

        #region Checkout

        [HttpGet]
        public ActionResult CheckEligibility(string uid)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();
            var registrant = Registrant.LoadByUid(uid.ToGuid(), msgs);

            if (SessionVariables.Cart.IsNotEmpty())
            {
                var cartItems = SessionVariables.Cart.GetCartItems(msgs);
                var participationObjects = cartItems.Select(item => item.GetParticipationObject(msgs)).ToList();
                var ineligibleActivities = new List<string>();

                participationObjects.ForEach(participationObject => {
                    if ( participationObject.RestrictByGrade() && !participationObject.IsStudentGradeInRange(registrant, msgs))
                        ineligibleActivities.Add($"Grades {participationObject.Name}: {participationObject.GetMinGradeLevel(msgs).Abbreviation} - {participationObject.GetMaxGradeLevel(msgs).Abbreviation}");

                    if (!participationObject.RestrictByGrade() && !participationObject.IsStudentAgeInRange(registrant, msgs))
                        ineligibleActivities.Add($"Ages {participationObject.Name}: {participationObject.AgeStart} - {participationObject.AgeEnd}");
                });

                if (msgs.HasNoErrors())
                    return Json(new
                    {
                        status = "ok",
                        studentName = registrant.DisplayName,
                        ineligibleActivities
                    }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckAccountStatus(string email)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();

            if (!email.IsEmail())
                msgs.Add("Please enter a valid email address", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var isInUse = WebUser.ExistsByEmail(email, msgs);
                var isRegistered = false;

                if (isInUse)
                    isRegistered = WebUser.LoadByEmail(email, msgs).IsRegistered;

                return Json(new
                {
                    status = "ok",
                    isInUse,
                    isRegistered
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendAccountConfirmationEmail(string email, string redirectUrl)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();
            WebUser user = null;

            if (!email.IsEmail())
                msgs.Add("Invalid Request", MessageType.UserError);

            if (!WebUser.ExistsByEmail(email, msgs))
                msgs.Add("Invalid Request", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                user = WebUser.LoadByEmail(email, msgs);

                if (user.IsRegistered)
                    msgs.Add("Invalid Request", MessageType.UserError);
            }

            if (msgs.HasNoErrors())
            {
                EmailHelper.SendAccountConfirmationEmail(user, redirectUrl, msgs);

                if (msgs.HasNoErrors())
                    return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCreditCardFees(string id, string amount)
        {

            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();
            Account account = null;

            if (!id.IsGuid())
                msgs.Add("Invalid request", MessageType.UserError);

            if (!amount.IsNumber())
                msgs.Add("Invalid request", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                account = Account.LoadByUid(id.ToGuid(), msgs);

                if (account.IsEmpty())
                    msgs.Add("Invalid request", MessageType.UserError);
            }

            if (msgs.HasNoErrors())
            {
                var fee = new FeeCalculationObject(amount.ToDecimal(), account, msgs).GetCreditCardFee();
                return Json(new { status = "ok", creditCardFee = fee, creditCardFeeFormatted = fee.ToCurrency() }, JsonRequestBehavior.AllowGet);

            }

            return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCartItem(string uid)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();

            if (SessionVariables.Cart.IsNotEmpty())
            {
                var activity = Activity.LoadByUid(uid.ToGuid(), msgs);

                if (msgs.HasNoErrors())
                    return Json(new
                    {
                        status = "ok",
                        cart = GetCartItemObject(activity.GetParticipationObject(msgs))
                    }, JsonRequestBehavior.AllowGet);
            }


            return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.AllowGet);
        }

        public dynamic GetCartItemObject(ParticipationObject participationObject)
        {
            return new
            {
                uid = participationObject.Uid,
                name = participationObject.Name,
                days = ParticipationObjectUI.PrintDaysOfWeekList(participationObject),
                minGradeText = participationObject.GetMinGradeLevel()?.Name,
                maxGradeText = participationObject.GetMaxGradeLevel()?.Name
            };
        }

        [HttpPost]
        public ActionResult AddToCart(string uid)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();

            if (SessionVariables.Cart.IsEmpty())
                SessionVariables.Cart = Cart.GetNewCart(loggedInUser: SessionVariables.CurrentUser, msgs: msgs);

            var participationObject = Activity.LoadByUid(uid.ToGuid(), msgs).GetParticipationObject(msgs);

            SessionVariables.Cart.AddCartItem(participationObject, msgs);

            if (msgs.HasNoErrors())
                return Json(new {
                    status = "ok",
                    cart = GetCartItemObject(participationObject)
                }, JsonRequestBehavior.AllowGet);

            return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RemoveFromCart(string uid)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();

            if (SessionVariables.Cart.IsNotEmpty())
                SessionVariables.Cart.RemoveCartItem(Activity.LoadByUid(uid.ToGuid(), msgs), msgs);

            if (msgs.HasNoErrors())
                return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);

            return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.AllowGet);
        }

        #endregion 

        #region DemoOrgName

        public ActionResult SetDemoOrgName(string orgname, string firstname, string email)
        {
            var msgs = new SystemMessages();

            if (orgname.IsBlank())
                msgs.Add("Organization / Program name is required", MessageType.UserError);

            if (firstname.IsBlank())
                msgs.Add("First Name is required", MessageType.UserError);

            if (!email.IsEmail())
                msgs.Add("Email is required", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var uniqueName = AccountService.GenerateUniqueName(orgname, null, msgs);
                CookieVariables.DemoOrgName = orgname;
                CookieVariables.DemoUniqueOrgName = uniqueName;
                CookieVariables.DemoFirstName = firstname;
                CookieVariables.DemoEmailAddress = email;

                SendGridHelper.AddContactFromDemo(
                    orgname,
                    firstname,
                    email,
                    msgs);

                return Json(new { status = "ok", demoUrl = $"/{uniqueName}" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult HasDemoOrgName()
        {
            return Json(new { status = "ok", hasDemoOrgName = CookieVariables.DemoOrgName.IsNotBlank() }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Profile

        [HttpGet]
        public ActionResult GetProfiles(int? count)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();           

            var profiles = AccountProfile.Search(
                            accountTypes: AccountService.CustomerAccountTypes,
                            isOpenDuringCovid: null,
                            isELearningCenter: null,
                            neighborhoods: null,
                            cities: null,
                            states: null,
                            start: 0,
                            length: count,
                            getRandomSample: true,
                            msgs: msgs);

            var profilesObj = new List<object>();

            var html = "";

            profiles.ForEach(profile =>
            {
                var result = ViewEngines.Engines.FindPartialView(this.ControllerContext, "~/Views/Shared/Registration/_BrowseProvider.cshtml");

                if (result.View != null)
                {
                    this.ViewData.Model = profile;
                    var sb = new StringBuilder();
                    using (StringWriter sw = new StringWriter(sb))
                    {
                        using (HtmlTextWriter output = new HtmlTextWriter(sw))
                        {
                            var viewContext = new ViewContext(this.ControllerContext, result.View, this.ViewData, new TempDataDictionary(), output);
                            result.View.Render(viewContext, output);
                        }
                    }

                    html += sb.ToString();
                }
            });


            //profiles.ForEach(profile => {
            //    var account = profile.GetAccount(msgs);
            //    var photos = profile.GetPhotos(msgs);
            //    var classifications = profile.GetProperties(AccountPropertyType.Classifications, msgs);
            //    var gradeLevelStart = profile.GetGradeLevelStart(msgs).Name;
            //    var gradeLevelEnd = profile.GetGradeLevelEnd(msgs).Name;
            //    var location = profile.GetLocation(msgs);
            //    var locationString = location?.FormatCityStateZipCode(false);
            //    var followerCount = account.GetSubscriberCount(msgs);
            //    var headerImageUrl = profile.GetHeaderImageUrl();

            //    if (photos.Any() && headerImageUrl.IsBlank())
            //    {
            //        var ah = AfterSchoolLibrary.Helpers.Utilities.GetNewAmazonHelper();
            //        headerImageUrl = ah.GetFileLink(photos.First().Value, true, null, true);
            //    }

            //    if (headerImageUrl.IsBlank() && location.IsNotEmpty())
            //        headerImageUrl = LocationService.GetGoogleMapsImageUrl(location, 1500, 600);

            //    var classificationsObj = new List<object>();
            //    classifications.ForEach(classification => {
            //        classificationsObj.Add(new
            //        {
            //            Label = classification.Value,
            //        });
            //    });

            //    profilesObj.Add(new {
            //        HomePageUrl = account.ShortHomePageUrl,
            //        AccountName = account.Name,
            //        Title = profile.Title,
            //        Headline = profile.Headline,
            //        IsOpenDuringCovid = profile.IsOpenDuringCovid,
            //        IsELearningCenter = profile.IsELearningCenter,
            //        HeaderImageUrl = headerImageUrl,
            //        Followers = followerCount,
            //        Location = locationString,
            //        GradeLevelStart = gradeLevelStart,
            //        GradeLevelEnd = gradeLevelEnd,
            //        Classifications = classificationsObj,
            //    });
            //});

            return Json(new { status = "ok", profiles = profilesObj, html }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ClaimProfile(string profile, string firstname, string lastname, string email, string phone, string g_recaptcha_response)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();

            if (!profile.IsGuid())
                msgs.Add("Invalid request", MessageType.UserError);

            if (firstname.IsEmpty())
                msgs.Add("First Name is required", MessageType.UserError);

            if (lastname.IsEmpty())
                msgs.Add("Last Name is required", MessageType.UserError);

            if (!email.IsEmail())
                msgs.Add("Email is required", MessageType.UserError);

            if (!phone.IsPhone())
                msgs.Add("Valid phone is required", MessageType.UserError);

            msgs.AddRange(RecaptchaHelper.Validate(g_recaptcha_response));

            Account account = null;

            if (msgs.HasNoErrors())
                account = Account.LoadByUid(profile.ToGuid(), msgs);

            if (msgs.HasNoErrors() && account.IsNotEmpty() && !account.IsClaimed)
            {
                EmailHelper.SendEmail(
                    "sales@afterschoolhq.com",
                    $"AHQ Profile Claim Form - {account.Name}",
                    $"<p>Organization: {account.Name}<br />Name: {firstname} {lastname}<br />Email: {email}<br />Phone: {phone}<br /></p>",
                    true);

                EmailHelper.SendClaimEmail(account, msgs);

                return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ContactProvider(string profile, string firstname, string lastname, string email, string phone, string g_recaptcha_response)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();

            if (!profile.IsGuid())
                msgs.Add("Invalid request", MessageType.UserError);

            if (firstname.IsEmpty())
                msgs.Add("First Name is required", MessageType.UserError);

            if (lastname.IsEmpty())
                msgs.Add("Last Name is required", MessageType.UserError);

            if (!email.IsEmail())
                msgs.Add("Email is required", MessageType.UserError);

            if (phone.IsNotEmpty() && !phone.IsPhone())
                msgs.Add("Valid phone is required", MessageType.UserError);

            msgs.AddRange(RecaptchaHelper.Validate(g_recaptcha_response));

            Account account = null;

            if (msgs.HasNoErrors())
                account = Account.LoadByUid(profile.ToGuid(), msgs);

            if (msgs.HasNoErrors() && account.IsNotEmpty())
            {
                var contactExists = account.DoesContactExist(
                                        firstname.Trim(),
                                        lastname.Trim(),
                                        email.Trim(),
                                        msgs);
                if (!contactExists)
                {
                    var contact = account.AddContact(
                        firstname,
                        lastname,
                        email,
                        phone,
                        SessionVariables.CurrentUser,
                        msgs);

                    if (msgs.HasNoErrors())
                    {
                        // LOG: Someone Contacted Account AccountName
                        WebSessionUtilities.LogActivity(
                            actor: SessionVariables.CurrentUser, 
                            webSessionId: SessionVariables.WebSession.ID.Value,
                            actionType: WebSessionActionType.Contacted,
                            objectType: ObjectType.Account,
                            objectId: account.ID.Value, 
                            objectText: account.Name
                        );
                        
                        EmailHelper.SendContactProviderEmail(account, contact, msgs);
                    }
                }

                return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Follow(string id)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();
            Account account = null;

            if (SessionVariables.CurrentUser.IsEmpty())
                return new HttpUnauthorizedResult();

            if (!id.IsGuid())
                msgs.Add("Invalid Request", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                account = Account.LoadByUid(id.ToGuid(), msgs);

                if (account.IsEmpty())
                    msgs.Add("Invalid request", MessageType.UserError);
            }

            if (msgs.HasNoErrors())
            {
                account.AddOrUpdateSubscriber(SessionVariables.CurrentUser, SubscriberStatus.Subscribed, SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                {
                    // LOG: Someone Followed Account AccountName
                    WebSessionUtilities.LogActivity(
                        webSessionId: SessionVariables.WebSession.ID.Value,
                        actor: SessionVariables.CurrentUser,
                        actionType: WebSessionActionType.Followed,
                        objectType: ObjectType.Account,
                        objectId: account.ID.Value,
                        objectText: account.Name
                    );

                    return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult Unfollow(string id)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();
            Account account = null;

            if (SessionVariables.CurrentUser.IsEmpty())
                return new HttpUnauthorizedResult();

            if (!id.IsGuid())
                msgs.Add("Invalid Request", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                account = Account.LoadByUid(id.ToGuid(), msgs);

                if (account.IsEmpty())
                    msgs.Add("Invalid request", MessageType.UserError);
            }

            if (msgs.HasNoErrors())
            {
                account.AddOrUpdateSubscriber(SessionVariables.CurrentUser, SubscriberStatus.Unsubscribed, SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                {
                    // LOG: Someone Unfollowed Account AccountName
                    WebSessionUtilities.LogActivity(
                        webSessionId: SessionVariables.WebSession.ID.Value,
                        actor: SessionVariables.CurrentUser,
                        actionType: WebSessionActionType.Unfollowed,
                        objectType: ObjectType.Account,
                        objectId: account.ID.Value,
                        objectText: account.Name
                    );
                    return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.AllowGet);

        }

        #endregion

        public ActionResult LeadForm(string org, string name, string email, string phone, string comments)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();

            if (org.IsEmpty())
                msgs.Add("Invalid Request", MessageType.UserError);

            if (name.IsEmpty())
                msgs.Add("Name is required", MessageType.UserError);

            if (!email.IsEmail())
                msgs.Add("Email is required", MessageType.UserError);

            if (!phone.IsPhone())
                msgs.Add("Valid phone is required", MessageType.UserError);
            
            if (msgs.HasNoErrors())
            {
                EmailHelper.SendEmail(
                    "sales@afterschoolhq.com",
                    $"AHQ Lead Form - {org}",
                    $"<p>Organization: {org}<br />Name: {name}<br />Email: {email}<br />Phone: {phone}<br />Best time to contact: {comments}</p>",
                    true);

                return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchLocations(string search, string type)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();
            var locations = Location.Search(search.Trim(), (LocationType)type.ToInteger(), 0, 10, msgs);

            return Json(new { 
                status = "ok",
                locations = locations.Select(location => new
                {
                    location.ID,
                    location.Name,
                    location.Address1,
                    location.Address2,
                    location.City,
                    location.State,
                    location.PostalCode,
                    FullAddress = location.FormatAddress(useLineBreaks: false),
                    location.HostAccountId,
                    LocationType = location.LocationTypeId.ToString()
                })
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchProviders(string search)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();
            var accounts = Account.FullTextSearch(search.Trim(), AccountService.ProviderAccountTypes.ToList(), 0, 10, msgs);

            return Json(new
            {
                status = "ok",
                providers = accounts.Select(account => new
                {
                    account.ID,
                    account.Name,
                    account.Address1,
                    account.Address2,
                    account.City,
                    account.State,
                    account.PostalCode,
                    FullLabel = account.FormatAddress(useLineBreaks: false)
                })
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ValidateCoupon(string couponcode, string paymentoption)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;

            SubscriptionPlan paymentPlan = null;

            var participationObject = SessionVariables.Cart.GetCartItems(msgs)
                .FirstOrDefault()?
                .GetParticipationObject(msgs);

            if (paymentoption.IsNotEmpty() && paymentoption.IsNumber())
                paymentPlan = SubscriptionPlan.Load(paymentoption.ToInteger(), msgs);

            var subscriptionPlan = participationObject.SubscriptionPlans.FirstOrDefault();
            var postPayPlan = participationObject.PostPayPlans.FirstOrDefault();

            var coupon = currentAccount.ValidateCoupon(participationObject, couponcode, msgs);
            var isPostPayPlan = postPayPlan.IsNotEmpty();
            var amount = isPostPayPlan ? participationObject.GetCheckoutCostWithConvenienceFee() :  participationObject.Cost.Value;
            
            var feeCalcObject = new FeeCalculationObject(amount, participationObject.OwnerAccount, msgs);

            // These amounts will either be the regular price or the discounted price, but we always return a value to update the UI
            var calculatedAmountACH = isPostPayPlan ? amount : feeCalcObject.GetFinalTotalCost(PaymentType.ACH);
            var calculatedAmountCC = isPostPayPlan ? amount + participationObject.GetCheckoutPostPayCreditCardFee() : feeCalcObject.GetFinalTotalCost(PaymentType.CreditCard);
            var calculatedCCFee = isPostPayPlan ? participationObject.GetCheckoutPostPayCreditCardFee() : feeCalcObject.GetCreditCardFee();           

            int numberInMonths = 0;
            decimal subscriptionPlanAmount = 0;
            decimal postPayPlanAmount = 0;
            var discountString = "";

            // If coupon is valid we update the amount shown to the user here
            if (coupon.IsNotEmpty() && coupon.ID.IsNotEmpty() && coupon.DoesApplyToRegistrationFee())
            {
                var couponValue = coupon.GetValue(amount);
                var discountedFeeCalcObject = new FeeCalculationObject(amount - couponValue, participationObject.OwnerAccount, msgs);

                //.. if post pay pre payment, we'll have already applied all the charges upfornt when adding activity. so final cost will be row cost only.               
                calculatedAmountACH = isPostPayPlan ? discountedFeeCalcObject.GetFinalTotalCostForPostPay(PaymentType.ACH) : discountedFeeCalcObject.GetFinalTotalCost(PaymentType.ACH);
                calculatedAmountCC = isPostPayPlan ? discountedFeeCalcObject.GetFinalTotalCostForPostPay(PaymentType.CreditCard) : discountedFeeCalcObject.GetFinalTotalCost(PaymentType.CreditCard);
                calculatedCCFee = discountedFeeCalcObject.GetCreditCardFee();
                discountString = coupon.GetDiscountString();
            }

            if (paymentPlan.IsNotEmpty())
            {
                amount = paymentPlan.CostPerPeriod.Value;
                numberInMonths = paymentPlan.TotalPeriods.Value;
            }

            if (subscriptionPlan.IsNotEmpty())
                subscriptionPlanAmount = subscriptionPlan.CostPerPeriod.Value;

            if (isPostPayPlan)
                postPayPlanAmount = postPayPlan.CostPerPeriod.Value;

            var status = "error";

            if (msgs.HasNoErrors())
            {
                status = "ok";
                msgs.Add("Coupon has successfuly been applied", MessageType.Success);
            }
                   
            return Json(new { status, coupon, amount, calculatedAmountCC, calculatedAmountACH, calculatedCCFee, numberInMonths, subscriptionPlanAmount, postPayPlanAmount, discountString, messages = msgs }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SubmitContactForm(string name, string fromEmail, string pageName, string message, string phone, string MainSubject, string g_recaptcha_response)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();
            string toEmail = "";
            if (name.IsBlank())
                msgs.Add("Please enter your name", MessageType.UserError);

            if (!fromEmail.IsMatch(BasicLibrary.Utilities.EmailRegEx))
                msgs.Add("Please enter your email", MessageType.UserError);

            if (message.IsBlank())
                msgs.Add("Please enter a message", MessageType.UserError);


            if (phone.IsNotEmpty() && !phone.IsPhone())
                msgs.Add("Phone number is not in a valid format", MessageType.UserError);

            if (pageName != "search")
            {
                msgs.AddRange(RecaptchaHelper.Validate(g_recaptcha_response));
            }

            switch(pageName)
            {
                case "contact":
                    toEmail = AfterSchoolLibrary.Helpers.Email.AppSalesEmailAddress;
                    break;
                case "search":
                    toEmail = AfterSchoolLibrary.Helpers.Email.AppSupportEmailAddress;
                    break;
            }

            if (msgs.HasNoErrors())
                msgs.AddRange(EmailHelper.SendEmail(
                    toEmail: toEmail,
                    subject: MainSubject.IsNotEmpty()? MainSubject: $"WEBSITE FORM FROM: {name}",
                    body: string.Format("<p>from: <a href=\"mailto:{0}\">{0}</a></p><p>{1}</p>", fromEmail, message),
                    isAfterSchool: true));

            var status = msgs.HasNoErrors() ? "ok" : "error";

            return Json(new { status, messages = msgs }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetIPSSchools()
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();
            var district = Account.LoadByAccountTypeId(AccountType.SchoolDistrict, msgs).FirstOrDefault();

            if (district.IsEmpty())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var schools = district.GetVisitorAccounts(AccountType.School.AsList(), msgs).Where(school => !school.IsTest && school.IsActive).OrderBy(x => x.Name);
            var status = msgs.HasNoErrors() ? "ok" : "error";
            var schoolList = schools.Select(school => 
                                    new 
                                    { 
                                        school.Name, 
                                        Link = school.HomePageUrl, 
                                        Address = school.GetHostedLocations(msgs).Any() ? school.GetHostedLocations(msgs).First().FormatAddressWithoutName(false) : ""
                                    }
                                );

            return Json(new 
            { 
                status, 
                messages = msgs, 
                schools = schoolList
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult IsInUse(string email)
        {
            if (!IsRequestFromApp())
                return Json(new { status = "error", messages = new SystemMessages() }, JsonRequestBehavior.AllowGet);

            var msgs = new SystemMessages();

            if (email.IsEmpty() || !email.IsRegExMatch(BasicLibrary.Utilities.EmailRegEx))
                msgs.Add("Invalid Request", MessageType.UserError);

            if (msgs.HasNoErrors())
                return Json(new { status = "ok", isInUse = WebUser.ExistsByEmail(email, msgs) }, JsonRequestBehavior.AllowGet);

            return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.AllowGet);
        }
    }
}

using System;
using System.Linq;
using System.Web.Mvc;
using AfterSchool.Models;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.FeatureFlag;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Hangfire;
using Email = AfterSchoolLibrary.Helpers.Email;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Areas.Org.Controllers
{
    public class OnboardingController : BaseLoggedInController
    {
        private readonly IFeatureFlagRepository featureFlagRepository;

        public OnboardingController(IFeatureFlagRepository featureFlagRepository) : base(
            FormConstants.OrganizationAccountTypesAndNotSet.ToList())
        {
            this.featureFlagRepository = featureFlagRepository;
        }

        // GET: Org/Onboarding
        [HttpGet]
        public ActionResult Index(bool showGettingStarted = false)
        {
            if (featureFlagRepository.IsEnabled("free-trial"))
                return RedirectToAction("FreeTrial");

            if (showGettingStarted)
                return RedirectToAction("GettingStarted");

            return RedirectToAction("ProgramInfo");
        }

        [HttpGet]
        public ActionResult StartSubscription()
        {
            var msgs = new SystemMessages();
            HandleTempMessages(msgs);
            ViewBag.Messages = msgs;
            return View();
        }

        [HttpGet]
        public ActionResult ContinueToPayment()
        {
            var msgs = new SystemMessages();
            HandleTempMessages(msgs);

            var currentAccount = SessionVariables.CurrentAccount;
            var currentAccountPackage = SessionVariables.AccountPackage;

            var session = StripeHelper.GetStripeHelperSubscriberAccount().GetCheckoutSession(
                currentAccount.StripeCustomerId,
                currentAccountPackage.GetStripePriceId(msgs),
                $"{Utilities.BaseUrl()}{Url.Action("AfterStripeUpdate", "Account", new {Area = "org"})}" +
                $"?returnurl={Url.Action("SubscriptionSuccess", "Onboarding", new {Area = "org"})}",
                $"{Utilities.BaseUrl()}/{Url.Action("SubscriptionFail", "Onboarding", new {Area = "org"})}",
                msgs);

            if (msgs.HasNoErrors() && session.IsNotEmpty())
                return new RedirectResult(session.Url);
            
            ViewBag.Messages = msgs;
            return View("StartSubscription");
        }

        [HttpGet]
        public ActionResult SubscriptionSuccess()
        {
            var msgs = new SystemMessages();
            var currentPackage = SessionVariables.AccountPackage;
            var currentAccount = SessionVariables.CurrentAccount;
            HandleTempMessages(msgs);

            currentAccount.UpdateOnboardingStep(OnboardingStepType.AccountType, msgs);
            
            ViewBag.RenewalDate = currentPackage.GetRenewalDate().ToLongDateString();
            ViewBag.RenewalAmount = currentPackage.Price.ToCurrency();
            ViewBag.Messages = msgs;
            return View();
        }

        [HttpGet]
        public ActionResult SubscriptionFail()
        {
            var msgs = new SystemMessages();
            HandleTempMessages(msgs);
            ViewBag.Messages = msgs;
            return View();
        }

        [HttpGet]
        public ActionResult AccountType()
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            HandleTempMessages(msgs);

            ViewBag.Messages = msgs;
            ViewBag.CurrentUser = currentUser;

            return View(account);
        }

        [HttpGet]
        public ActionResult SetAccountType(AccountType type)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;

            currentAccount.AccountTypeId = type;
            currentAccount.OnboardingStepDone = OnboardingStepType.AccountType;
            msgs.AddRange(currentAccount.Save());

            return RedirectToAction("ProgramInfo");
        }

        [HttpGet]
        public ActionResult GettingStarted()
        {
            var msgs = new SystemMessages();
            HandleTempMessages(msgs);
            ViewBag.Messages = msgs;
            return View();
        }

        [HttpGet]
        public ActionResult ProgramInfo()
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var profile = currentAccount.GetOrCreateAccountProfile(currentUser, msgs);
            var headline = profile.IsNotEmpty() ? profile.Headline : "";
            var genderrestrictions =
                profile.IsNotEmpty() ? (int) profile.GenderRestriction : (int) GenderRestrictionType.None;

            HandleTempMessages(msgs);

            ViewBag.Headline = headline;
            ViewBag.GenderRestriction = genderrestrictions;
            ViewBag.Messages = msgs;
            ViewBag.Title = currentAccount.Name;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;

            return View(currentAccount);
        }

        [HttpPost]
        public ActionResult ProgramInfo(string title, string headline, string genderrestrictions, string timeZone)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var profile = currentAccount.GetOrCreateAccountProfile(currentUser, msgs);

            HandleTempMessages(msgs);

            if (title.IsEmpty())
                msgs.Add("Program Name is required", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        currentAccount.Name = title.MaxChars(50);
                        currentAccount.UniqueName = AccountService.GenerateUniqueName(title, currentAccount.ID, amsgs, adc);
                        currentAccount.OnboardingStepDone = OnboardingStepType.ProgramInfo;
                        currentAccount.TimeZone = timeZone.IsNotEmpty() ? timeZone : null;
                        amsgs.AddRange(currentAccount.Save(adc));

                        profile.UpdateProfileProgramInfo(
                            title: title,
                            headline: headline,
                            genderrestriction: genderrestrictions,
                            loggedInUser: currentUser,
                            msgs: amsgs,
                            dc: adc);
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("AdditionalInfo");


            ViewBag.Messages = msgs;
            ViewBag.Title = Request["title"] ?? currentAccount.Name;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;

            return View(currentAccount);
        }

        [HttpGet]
        public ActionResult AdditionalInfo()
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var profile = currentAccount.GetOrCreateAccountProfile(currentUser, msgs);
            var grades = GradeLevel.Load(msgs: msgs);
            var minGradeId = grades.First().ID.Value;
            var gradeStart = profile.GetGradeLevelStart();
            var maxGradeId = grades.Last().ID.Value;
            var gradeEnd = profile.GetGradeLevelEnd();
            var ageStart = profile.GetAgeStart();
            var ageEnd = profile.GetAgeEnd();

            HandleTempMessages(msgs);

            ViewBag.IsNewProfile = profile.CostLow.IsEmpty() && profile.CostHigh.IsEmpty();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Grades = grades;
            ViewBag.Messages = msgs;
            ViewBag.IsVirtualProgram = profile.IsNotEmpty() && profile.IsVirtualProgram;
            ViewBag.IsRestrictByGrade = profile.IsRestrictByGrade();
            ViewBag.GradeStart = gradeStart.IsNotEmpty() ? gradeStart.ID : minGradeId;
            ViewBag.GradeEnd = gradeEnd.IsNotEmpty() ? gradeEnd.ID : maxGradeId;
            ViewBag.AgeStart = ageStart.HasValue ? ageStart.Value : 0;
            ViewBag.AgeEnd = ageEnd.HasValue ? ageEnd.Value : 18;
            ViewBag.CostLow = profile.CostLow.HasValue ? profile.CostLow.Value.ToString("0.00") : "";
            ViewBag.CostHigh = profile.CostHigh.HasValue ? profile.CostHigh.Value.ToString("0.00") : "";

            return View(currentAccount);
        }

        [HttpPost]
        public ActionResult AdditionalInfo(string mingrade, string maxgrade, string agestart, string ageend,
            string isvirtualprogram, string costlow, string costhigh)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var profile = currentAccount.GetOrCreateAccountProfile(currentUser, msgs);

            HandleTempMessages(msgs);

            var isVirtualProgram = isvirtualprogram.IsBoolean() && isvirtualprogram.ToBoolean();

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        currentAccount.UpdateOnboardingStep(OnboardingStepType.AdditionalInfo, amsgs, adc);

                        profile.UpdateProfileAdditionalInfo(
                            mingrade: mingrade,
                            maxgrade: maxgrade,
                            agestart: agestart,
                            ageend: ageend,
                            isvirtualprogram: isVirtualProgram,
                            loggedInUser: currentUser,
                            costhigh: costhigh,
                            costlow: costlow,
                            msgs: amsgs,
                            dc: adc);
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("ContactInfo");

            var grades = GradeLevel.Load(msgs: msgs);

            ViewBag.Grades = grades;
            ViewBag.MinGradeId = Request["mingrade"] ?? grades.First().ID.ToString();
            ViewBag.MaxGradeId = Request["maxgrade"] ?? grades.Last().ID.ToString();
            ViewBag.IsVirtualProgram = isVirtualProgram;
            ViewBag.CostLow = costlow;
            ViewBag.CostHigh = costhigh;
            ViewBag.IsNewProfile = false;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;

            return View(currentAccount);
        }

        [HttpGet]
        public ActionResult ContactInfo()
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var profile = currentAccount.GetAccountProfile(msgs);
            var location = profile.GetLocation();

            HandleTempMessages(msgs);

            ViewBag.Location = location ?? new Location();
            ViewBag.Email = profile.ProfileEmail.IsNotEmpty() ? profile.ProfileEmail : currentAccount.Email;
            ViewBag.Phone = profile.ProfilePhone.IsNotEmpty() ? profile.ProfilePhone : currentAccount.Phone;
            ViewBag.IsProfileEmail = profile.ProfileEmail.IsNotEmpty();
            ViewBag.IsProfilePhone = profile.ProfilePhone.IsNotEmpty();
            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            return View(currentAccount);
        }

        [HttpPost]
        public ActionResult ContactInfo(string email, string publicemail, string phone, string publicphone,
            string locationname,
            string streetnumber, string streetname, string address2, string neighborhood, string city, string state,
            string postalcode,
            string county, string country, string placeid, string latitude, string longitude)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var profile = currentAccount.GetOrCreateAccountProfile(currentUser, msgs);

            HandleTempMessages(msgs);

            if (email.IsEmpty())
                msgs.Add("Program Email is required", MessageType.UserError);

            if (phone.IsNotEmpty() && !phone.IsPhone())
                msgs.Add("Program Phone is not in a valid format", MessageType.UserError);

            if (placeid.IsEmpty())
                msgs.Add("Program Location is required", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var extractedPhoneNumber =
                    new String(phone.Where(Char.IsDigit).ToArray()); //we will store plain phone number in database

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        currentAccount.Email = email;
                        currentAccount.Phone = extractedPhoneNumber;
                        currentAccount.OnboardingStepDone = OnboardingStepType.ContactInfo;
                        amsgs.AddRange(currentAccount.Save(adc));

                        profile.UpdateProfileContactinfo(
                            profileemail: publicemail.IsBoolean() ? email : null,
                            profilephone: publicphone.IsBoolean() ? extractedPhoneNumber : null,
                            loggedInUser: currentUser,
                            msgs: msgs,
                            dc: adc
                        );

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
                            Convert.ToDecimal(latitude),
                            Convert.ToDecimal(longitude),
                            currentUser,
                            amsgs,
                            adc);
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("Logo");

            var location = profile.GetLocation(msgs);

            if (location.IsEmpty())
                location = new Location
                {
                    GooglePlaceId = placeid, StreetNumber = streetnumber, StreetName = streetname, Address2 = address2,
                    City = city, State = state, PostalCode = postalcode, Country = country, County = county,
                    Neighborhood = neighborhood, Name = locationname, Longitude = Convert.ToDecimal(longitude),
                    Latitude = Convert.ToDecimal(latitude),
                };

            ViewBag.Location = location;
            ViewBag.Email = Request["email"] ?? currentUser.Email;
            ViewBag.Phone = Request["phone"] ?? currentUser.Phone;
            ViewBag.IsProfileEmail = publicemail.IsBoolean() && publicemail.ToBoolean();
            ViewBag.IsProfilePhone = publicphone.IsBoolean() && publicphone.ToBoolean();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;

            ViewBag.Messages = msgs;
            return View(currentAccount);
        }

        [HttpGet]
        public ActionResult Logo()
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var profile = currentAccount.GetOrCreateAccountProfile(SessionVariables.CurrentUser, msgs);
            var ah = Utilities.GetNewAmazonHelper();

            HandleTempMessages(msgs);

            if (profile.Logo.IsNotBlank())
                ViewBag.LogoUrl = ah.GetFileLink(profile.Logo);

            if (profile.HeaderImage.IsNotBlank())
                ViewBag.HeaderImageUrl = ah.GetFileLink(profile.HeaderImage);

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            return View(currentAccount);
        }

        [HttpPost]
        public ActionResult Logo(string logo, string headerimage)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var profile = currentAccount.GetOrCreateAccountProfile(SessionVariables.CurrentUser, msgs);
            var allowedFiles = new string[] {".png", ".jpg", ".jpeg"};
            var accountPackage = SessionVariables.AccountPackage;

            HandleTempMessages(msgs);

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
                    msgs.Add("Please upload a valid jpg or png file, must be less than 4MB", MessageType.UserError);
            }

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

                currentAccount.UpdateOnboardingStep(OnboardingStepType.LogoAndHeader, msgs);
            }

            if (msgs.HasNoErrors())
                return RedirectToAction(accountPackage.IsFree ? "ServicesOffered" : "Links");

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = accountPackage;
            return View(currentAccount);
        }

        [HttpGet]
        public ActionResult ServicesOffered()
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var profile = currentAccount.GetOrCreateAccountProfile(SessionVariables.CurrentUser, msgs);

            HandleTempMessages(msgs);

            ViewBag.ServiceOffered = profile.GetProperties(AccountPropertyType.ServiceOffered, msgs);
            ViewBag.TimeOffered = profile.GetProperties(AccountPropertyType.TimeOffered, msgs);
            ViewBag.PricingOption = profile.GetProperties(AccountPropertyType.PricingOption, msgs);
            ViewBag.SpecialNeedsOffering = profile.GetProperties(AccountPropertyType.SpecialNeedsOffering, msgs);
            ViewBag.TeachingMethods = profile.GetProperties(AccountPropertyType.TeachingMethods, msgs);
            ViewBag.AcademicSubjects = profile.GetProperties(AccountPropertyType.AcademicSubjects, msgs);
            ViewBag.SoftSkills = profile.GetProperties(AccountPropertyType.SoftSkills, msgs);
            ViewBag.ProgramCategories = profile.GetProperties(AccountPropertyType.Classifications, msgs);
            ViewBag.ProgramsAllDayDuringWorkWeek =
                profile.GetProperties(AccountPropertyType.ProgramsAllDayDuringWorkWeek, msgs);

            ViewBag.Messages = msgs;

            return View(currentAccount);
        }

        [HttpPost]
        public ActionResult ServicesOffered(string programCategories, string softskills, string academicsubjects,
            string teachingmethods, string programsalldayduringworkweek)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var profile = currentAccount.GetOrCreateAccountProfile(currentUser, msgs);

            HandleTempMessages(msgs);

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        currentAccount.UpdateOnboardingStep(OnboardingStepType.ServicesOffered, amsgs, adc);

                        profile.UpdateProfileServicesOffered(
                            servicesOffered: Request.Params.Filter("services-offered-"),
                            timesOffered: Request.Params.Filter("times-offered-"),
                            pricingOptions: Request.Params.Filter("pricing-options-"),
                            specialNeedsOfferings: Request.Params.Filter("special-needs-"),
                            classifications: programCategories,
                            softskills: softskills,
                            academicsubjects: academicsubjects,
                            teachingmethods: teachingmethods,
                            programsalldayduringworkweek: programsalldayduringworkweek,
                            loggedInUser: currentUser,
                            msgs: amsgs,
                            dc: adc);
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("Links");

            ViewBag.ServiceOffered = profile.GetProperties(AccountPropertyType.ServiceOffered, msgs);
            ViewBag.TimeOffered = profile.GetProperties(AccountPropertyType.TimeOffered, msgs);
            ViewBag.PricingOption = profile.GetProperties(AccountPropertyType.PricingOption, msgs);
            ViewBag.SpecialNeedsOffering = profile.GetProperties(AccountPropertyType.SpecialNeedsOffering, msgs);
            ViewBag.TeachingMethods = profile.GetProperties(AccountPropertyType.TeachingMethods, msgs);
            ViewBag.AcademicSubjects = profile.GetProperties(AccountPropertyType.AcademicSubjects, msgs);
            ViewBag.Softskills = profile.GetProperties(AccountPropertyType.SoftSkills, msgs);
            ViewBag.ProgramCategories = profile.GetProperties(AccountPropertyType.Classifications, msgs);
            ViewBag.ProgramsAllDayDuringWorkWeek =
                profile.GetProperties(AccountPropertyType.ProgramsAllDayDuringWorkWeek, msgs);
            ViewBag.Messages = msgs;

            return View(currentAccount);
        }


        [HttpGet]
        public ActionResult Links()
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            
            HandleTempMessages(msgs);

            ViewBag.IsIsypAccount = AccountService.CheckIfIsypAccount(currentAccount, msgs);
            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Profile = currentAccount.GetOrCreateAccountProfile(SessionVariables.CurrentUser, msgs);
            return View(currentAccount);
        }


        [HttpPost]
        public ActionResult Links(string website, string twitter, string facebook, string instagram, string linkedin,
            string youtube,
            string calendar, string registrationurl, string buttontext, string collectregistration)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var profile = currentAccount.GetOrCreateAccountProfile(currentUser, msgs);

            HandleTempMessages(msgs);

            if (collectregistration == "1")
            {
                registrationurl = "";
                buttontext = "";
            }

            if (registrationurl.IsNotEmpty() && !registrationurl.IsRegExMatch(BasicLibrary.Utilities.WebsiteRegEx))
                msgs.Add("Please enter a valid registration url address", MessageType.UserError);

            if (website.IsNotEmpty() && !website.IsRegExMatch(BasicLibrary.Utilities.WebsiteRegEx))
                msgs.Add("Please enter a valid website address", MessageType.UserError);

            if (facebook.IsNotEmpty() && !facebook.IsRegExMatch(BasicLibrary.Utilities.WebsiteRegEx))
                msgs.Add("Please enter a valid Facebook profile address", MessageType.UserError);

            if (linkedin.IsNotEmpty() && !linkedin.IsRegExMatch(BasicLibrary.Utilities.WebsiteRegEx))
                msgs.Add("Please enter a valid LinkedIn profile address", MessageType.UserError);

            if (youtube.IsNotEmpty() && !youtube.IsRegExMatch(BasicLibrary.Utilities.WebsiteRegEx))
                msgs.Add("Please enter a valid YouTube Url", MessageType.UserError);

            if (calendar.IsNotEmpty() && !calendar.IsRegExMatch(BasicLibrary.Utilities.WebsiteRegEx))
                msgs.Add("Please enter a valid calendar URL", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        currentAccount.OnboardingStepDone = OnboardingStepType.SocialLinks;
                        currentAccount.Website = website;
                        amsgs.AddRange(currentAccount.Save(adc));

                        profile.UpdateProfileLinks(
                            website: website,
                            twitter: twitter,
                            facebook: facebook,
                            instagram: instagram,
                            linkedin: linkedin,
                            youtube: youtube,
                            calendar: calendar,
                            registrationurl: registrationurl,
                            buttontext: buttontext,
                            loggedInUser: currentUser,
                            msgs: amsgs,
                            dc: adc);
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
            {
                if (currentUser.Email.Contains("@afterschoolhq.com"))
                    return RedirectToAction("Done");
                
                return RedirectToAction("Done", new {accounttype = currentAccount.AccountTypeId.ToString()});
            }

            ViewBag.IsIsypAccount = AccountService.CheckIfIsypAccount(currentAccount, msgs);
            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            return View(currentAccount);
        }

        [HttpGet]
        public ActionResult RedirectToDone()
        {
            if (SessionVariables.CurrentUser.Email.Contains("@afterschoolhq.com"))
                return RedirectToAction("Done");
                
            return RedirectToAction("Done", new {accounttype = SessionVariables.CurrentAccount.AccountTypeId.ToString()});
        }

        [HttpGet]
        public ActionResult Done()
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var profile = currentAccount.GetOrCreateAccountProfile(currentUser, msgs);
            var hostAccounts = currentAccount.GetHostAccounts();
            var hostAccountString = string.Join(",",
                hostAccounts?.Select(hostAccount => hostAccount.Name) ?? Array.Empty<string>());

            HandleTempMessages(msgs);

            currentAccount.UpdateOnboardingStep(OnboardingStepType.All, msgs);

            if (msgs.HasNoErrors() && currentAccount.IsNotEmpty() && currentUser.IsNotEmpty())
            {
                //.. send support email to AHQ just to notify the team that a profile completed.
                var location = profile.GetLocation() ?? currentAccount.GetDefaultLocation();

                BackgroundJob.Enqueue(() =>
                    EmailHelper.SendEmail(
                        Email.AppCustomersEmailAddress,
                        $"AHQ Profile Completed - {currentAccount.Name}",
                        $"<p>Organization: {currentAccount.Name}<br />" +
                        $"Main Contact Name: {currentUser.DisplayName}<br />" +
                        $"Email: {currentUser.Email}<br />" +
                        $"Profile Link: {currentAccount.HomePageUrl}<br />" +
                        $"How did you hear about us: {currentUser.HowHearAboutUs} <br />" +
                        $"Location: {location.ToString()}<br />" +
                        $"Host Account(s): {hostAccountString}" +
                        $"</p>",
                        true)
                );
            }

            ViewBag.HostAccounts = hostAccounts;
            ViewBag.HasActivities = currentAccount.HasActivities(msgs);
            ViewBag.Messages = msgs;

            return View(currentAccount);
        }

        [HttpGet]
        public ActionResult FreeTrial()
        {
            var account = SessionVariables.CurrentAccount;
            return View(account);
        }
    }
}
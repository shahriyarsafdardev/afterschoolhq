using AfterSchool.Models;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace AfterSchool.Areas.Org.Controllers
{
    public class OnboardingController : BaseLoggedInController
    {
        public OnboardingController() : base(AccountService.ProviderAccountTypesAndNotSet.ToList())
        {

        }

        // GET: Org/Onboarding
        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("AccountType");
        }

        [HttpGet]
        public ActionResult AccountType()
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            ViewBag.Messages = msgs;
            ViewBag.CurrentUser = currentUser;

            return View(account);
        }

        [HttpGet]
        public ActionResult SetAccountType(AccountType type)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;

            account.AccountTypeId = type;
            account.OnboardingStepDone = OnboardingStepType.AccountType;
            msgs.AddRange(account.Save());

            return RedirectToAction("ProgramInfo");
        }

        [HttpGet]
        public ActionResult ProgramInfo()
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var grades = GradeLevel.Load(msgs: msgs);

            ViewBag.Grades = grades;
            ViewBag.MinGradeId = grades.First().ID.ToString();
            ViewBag.MaxGradeId = grades.Last().ID.ToString();
            ViewBag.Messages = msgs;
            ViewBag.Title = account.Name;
            ViewBag.OpenCovid = true;
            ViewBag.ELC = false;

            //TODO: need to review that is this the right place, I mean the get call of ProgramInfo
            //need to review the change in fields if any.
            var user = account.GetAccountOwner(msgs);

            if (user.IsNotEmpty() && account.IsNotEmpty())
            {
                SendGridHelper.AddOrUpdateContact(account, user, msgs);
            }

            return View(account);
        }

        [HttpPost]
        public ActionResult ProgramInfo(string title, string headline, string mingrade, string maxgrade, string genderrestriction, string opencovid, string elearningcenter)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var profile = account.GetOrCreateAccountProfile(currentUser, msgs);

            if (title.IsEmpty())
                msgs.Add("Program Name is required", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        account.Name = title;
                        account.UniqueName = AccountService.GenerateUniqueName(title, account.ID, amsgs, adc);
                        account.OnboardingStepDone = OnboardingStepType.ProgramInfo;
                        amsgs.AddRange(account.Save(adc));

                        profile.UpdateProfile(
                            title: title,
                            headline: headline,
                            mingrade: mingrade,
                            maxgrade: maxgrade,
                            genderrestriction: genderrestriction,
                            opencovid: opencovid.IsNumber() ? opencovid : "0",
                            elearningcenter: elearningcenter.IsNumber() ? opencovid : "0",
                            loggedInUser: currentUser,
                            msgs: amsgs,
                            dc: adc);
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("ContactInfo");

            var grades = GradeLevel.Load(msgs: msgs);

            ViewBag.Grades = grades;
            ViewBag.MinGradeId = Request["mingrade"] ?? grades.First().ID.ToString();
            ViewBag.MaxGradeId = Request["maxgrade"] ?? grades.Last().ID.ToString();
            ViewBag.Messages = msgs;
            ViewBag.Title = Request["title"] ?? account.Name;
            ViewBag.OpenCovid = opencovid.IsNumber();
            ViewBag.ELC = elearningcenter.IsNumber();

            return View(account);
        }

        [HttpGet]
        public ActionResult ContactInfo()
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            ViewBag.Email = currentUser.Email;
            ViewBag.Phone = currentUser.Phone;
            ViewBag.Location = new Location();
            ViewBag.Messages = msgs;
            return View(account);
        }

        [HttpPost]
        public ActionResult ContactInfo(string email, string publicemail, string phone, string publicphone, string locationname, string streetnumber, string streetname, string address2, string neighborhood, string city, string state, string postalcode, string county, string country, string placeid)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var profile = account.GetOrCreateAccountProfile(SessionVariables.CurrentUser, msgs);
            var currentUser = SessionVariables.CurrentUser;

            if (email.IsEmpty())
                msgs.Add("Program Email is required", MessageType.UserError);

            if (phone.IsNotEmpty() && !phone.IsPhone())
                msgs.Add("Program Phone is not in a valid format", MessageType.UserError);

            if (placeid.IsEmpty())
                msgs.Add("Program Location is required", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var extractedPhoneNumber = new String(phone.Where(Char.IsDigit).ToArray()); //we will store plain phone number in database

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        account.Email = email;
                        account.Phone = extractedPhoneNumber;
                        account.OnboardingStepDone = OnboardingStepType.ContactInfo;
                        amsgs.AddRange(account.Save(adc));

                        profile.UpdateProfile(
                            profileemail: publicemail.IsBoolean() ? email : null,
                            profilephone: publicphone.IsBoolean() ? extractedPhoneNumber : null,
                            loggedInUser: currentUser,
                            msgs: amsgs,
                            dc: adc);

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
                            currentUser,
                            amsgs,
                            adc);
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("Logo");

            var location = profile.GetLocation(msgs);

            if (location.IsEmpty())
                location = new Location() { GooglePlaceId = placeid, StreetNumber = streetnumber, StreetName = streetname, Address2 = address2, City = city, State = state, PostalCode = postalcode, Country = country, County = county, Neighborhood = neighborhood, Name = locationname };

            ViewBag.Location = location;
            ViewBag.Email = Request["email"] ?? currentUser.Email;
            ViewBag.Phone = Request["phone"] ?? currentUser.Phone;

            ViewBag.Messages = msgs;
            return View(account);
        }

        [HttpGet]
        public ActionResult Logo()
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;

            ViewBag.Messages = msgs;
            return View(account);
        }

        [HttpPost]
        public ActionResult Logo(string logo, string headerimage)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var profile = account.GetOrCreateAccountProfile(SessionVariables.CurrentUser, msgs);
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
                
                account.OnboardingStepDone = OnboardingStepType.LogoAndHeader;
                msgs.AddRange(account.Save());               
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("ServicesOffered");

            ViewBag.Messages = msgs;
            return View(account);
        }

        [HttpGet]
        public ActionResult ServicesOffered()
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;

            ViewBag.Messages = msgs;
            return View(account);
        }

        [HttpPost]
        public ActionResult ServicesOffered(string programCategories, string softskills, string academicsubjects, string teachingmethods)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var profile = account.GetOrCreateAccountProfile(SessionVariables.CurrentUser, msgs);

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        account.OnboardingStepDone = OnboardingStepType.ServicesOffered;
                        amsgs.AddRange(account.Save(adc));

                        profile.UpdateProfile(
                            servicesOffered: Request.Params.Filter("services-offered-"),
                            timesOffered: Request.Params.Filter("times-offered-"),
                            pricingOptions: Request.Params.Filter("pricing-options-"),
                            specialNeedsOfferings: Request.Params.Filter("special-needs-"),
                            classifications: programCategories,
                            softskills: softskills,
                            academicsubjects: academicsubjects,
                            teachingmethods: teachingmethods,
                            loggedInUser: SessionVariables.CurrentUser,
                            msgs: amsgs,
                            dc: adc);
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("Links");

            ViewBag.Messages = msgs;
            return View(account);
        }

        [HttpGet]
        public ActionResult Links()
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;

            ViewBag.Messages = msgs;
            return View(account);
        }

        [HttpPost]
        public ActionResult Links(string website, string twitter, string facebook, string instagram, string linkedin, string youtube, string calendar)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var profile = account.GetOrCreateAccountProfile(SessionVariables.CurrentUser, msgs);

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
                        account.OnboardingStepDone = OnboardingStepType.SocialLinks;
                        account.Website = website;
                        amsgs.AddRange(account.Save(adc));

                        profile.UpdateProfile(
                            website: website,
                            twitter: twitter,
                            facebook: facebook,
                            instagram: instagram,
                            linkedin: linkedin,
                            youtube: youtube,
                            calendar: calendar,
                            loggedInUser: SessionVariables.CurrentUser,
                            msgs: amsgs,
                            dc: adc);
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("Done");

            ViewBag.Messages = msgs;
            return View(account);
        }

        [HttpGet]
        public ActionResult Done()
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            account.OnboardingStepDone = OnboardingStepType.All;
            msgs.AddRange(account.Save());

            if (msgs.HasNoErrors() && account.IsNotEmpty() && currentUser.IsNotEmpty())
            {
                EmailHelper.SendEmail(
                    AfterSchoolLibrary.Helpers.Email.AppSupportEmailAddress,
                    $"AHQ Profile Completed - {account.Name}",
                    $"<p>Organization: {account.Name}<br />Name: {currentUser.FirstName} {currentUser.LastName}<br />Email: {currentUser.Email}<br />Profile Link: {account.HomePageUrl}</p>",
                    true);

                //user has completed all the profile steps, send the contact to send grid.
                var user = account.GetAccountOwner(msgs);

                if (user.IsNotEmpty() && account.IsNotEmpty())
                {
                    SendGridHelper.AddOrUpdateContact(account, user, msgs);
                }
            }

            ViewBag.Messages = msgs;

            return View(account);
        }
    }
}
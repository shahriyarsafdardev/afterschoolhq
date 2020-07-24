using AfterSchool.Models;
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
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;

            ViewBag.Messages = msgs;
            ViewBag.CurrentUser = SessionVariables.CurrentUser;

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

            return View(account);
        }

        [HttpPost]
        public ActionResult ProgramInfo(string title, string headline, string mingrade, string maxgrade, string genderrestriction)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var profile = account.GetOrCreateAccountProfile(SessionVariables.CurrentUser, msgs);

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
                return RedirectToAction("ContactInfo");

            var grades = GradeLevel.Load(msgs: msgs);

            ViewBag.Grades = grades;
            ViewBag.MinGradeId = Request["mingrade"] ?? grades.First().ID.ToString();
            ViewBag.MaxGradeId = Request["maxgrade"] ?? grades.Last().ID.ToString();
            ViewBag.Messages = msgs;

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
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        account.Email = email;
                        account.Phone = phone;
                        account.OnboardingStepDone = OnboardingStepType.ContactInfo;
                        amsgs.AddRange(account.Save(adc));

                        profile.UpdateProfile(
                            profileemail: publicemail.ToBoolean() ? email : null,
                            profilephone: publicphone.ToBoolean() ? phone : null,
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
        public ActionResult ServicesOffered(string classifications, string softskills, string academicsubjects, string teachingmethods)
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
                            classifications: classifications,
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
        public ActionResult Links(string website, string twitter, string facebook, string instagram, string linkedin, string youtube)
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
                msgs.Add("Please enter a valid YouTube profile address", MessageType.UserError);

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
            return View(SessionVariables.CurrentAccount);
        }
    }
}
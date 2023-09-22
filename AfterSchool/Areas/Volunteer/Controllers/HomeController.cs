using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;

namespace AfterSchool.Areas.Volunteer.Controllers
{
    public class HomeController : BaseLoggedInController
    {
        public HomeController() : base(new List<AccountType> {AccountType.Volunteer})
        {
        }

        // GET: Volunteer/Home
        public ActionResult Index()
        {
            var msgs = new SystemMessages();
            var currentUser = SessionVariables.CurrentUser;
            var masterRegistrant = currentUser.GetMasterVolunteerRegistrant(msgs);
            var registrants = new List<Registrant> {masterRegistrant};
            var backgroundCheck = BackgroundCheck.LoadByWebUserId(currentUser.ID.Value, true).FirstOrDefault();
            var bgStatus = backgroundCheck?.BackgroundCheckStatusId;
            var registrantModel = registrants.PrepareRegistrantDetails(masterRegistrant, msgs);
            
            ViewBag.ActivityInstances = registrantModel.ActivityInstances;
            ViewBag.ActivitiesRequiringAttention = registrantModel.RegistrantActivities;
            ViewBag.Providers = registrantModel.Providers;
            ViewBag.Messages = msgs;
            ViewBag.IsSterlingSetupRequired = bgStatus.IsNotEmpty() && bgStatus == BackgroundCheckStatus.EmailSentToVolunteer;
            ViewBag.SterlingLink = backgroundCheck?.SterlingInviteUrl;
            
            return View(registrants);
        }
    }
}
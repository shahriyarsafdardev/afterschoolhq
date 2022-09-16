using System.Collections.Generic;
using System.Web.Mvc;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;

namespace AfterSchool.Areas.Volunteer.Controllers
{
    public class HomeController : BaseLoggedInController
    {
        public HomeController() : base(new List<AccountType> {AccountType.Volunteer,AccountType.Registrant})
        {
        }

        // GET: Volunteer/Home
        public ActionResult Index()
        {
            var msgs = new SystemMessages();

            var masterRegistrant = SessionVariables.CurrentUser.GetMasterVolunteerRegistrant(msgs);
            var registrants = new List<Registrant> {masterRegistrant};

            var registrantModel = registrants.PrepareRegistrantDetails(masterRegistrant, msgs);

            ViewBag.ActivityInstances = registrantModel.ActivityInstances;
            ViewBag.ActivitiesRequiringAttention = registrantModel.RegistrantActivities;
            ViewBag.Providers = registrantModel.Providers;
            ViewBag.Messages = msgs;

            return View(registrants);
        }
    }
}
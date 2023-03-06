using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AfterSchool.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;

namespace AfterSchool.Areas.User.Controllers
{
    public class HomeController : BaseLoggedInController
    {
        public HomeController() : base(new List<AccountType> {AccountType.Registrant})
        {
        }

        // GET: User/Home
        public ActionResult Index()
        {
            var msgs = new SystemMessages();
            //var masterRegistrant = SessionVariables.CurrentUser.GetMasterParentOrStudentRegistrant(msgs);
            var masterRegistrant = SessionVariables.CurrentUser.GetMasterParentOrStudentOrAdultRegistrant(msgs);
            var registrants = SessionVariables.CurrentUser.GetStudentRegistrants(msgs);

            if (registrants.All(registrant => registrant.ID != masterRegistrant.ID) && masterRegistrant.IsNotEmpty())
                registrants.Add(masterRegistrant);

            var registrantModel = registrants.PrepareRegistrantDetails(masterRegistrant, msgs);

            ViewBag.ActivityInstances = registrantModel.ActivityInstances;
            ViewBag.ActivitiesRequiringAttention = registrantModel.RegistrantActivities;
            ViewBag.Providers = registrantModel.Providers;
            ViewBag.Messages = msgs;

            return View(registrants);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AfterSchoolLibrary.Models;
using AfterSchool.Models;

namespace AfterSchool.Controllers
{
    public class HomeController : BaseController
    {
        // GET: Home
        public ActionResult Index()
        {
            if (Utilities.IsAfterSchool())
            {
                return Redirect("/");
            }

            var packages = Package.LoadByIsActiveByIsAfterSchool(true, false);
            ViewBag.Packages = packages;
            ViewBag.FeatureTypes = new List<FeatureType>() { FeatureType.AcceptPayments, FeatureType.Activities, FeatureType.TeamAccounts };

            return View();
        }
        public ActionResult ASHQ()
        {
            Utilities.SetAfterSchool(true);
            var package = Package.LoadByIsActiveByIsAfterSchool(true, true).FirstOrDefault();
            ViewBag.Package = package;
            return View();
        }
    }
}
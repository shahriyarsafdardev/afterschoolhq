using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchool.Models;

namespace AfterSchool.Controllers
{
    public class HomeController : BaseController
    {
        // GET: Home
        public ActionResult Index()
        {
            if (AfterSchool.Models.Utilities.IsAfterSchool())
            {
                return Redirect("/public/");
            }
            else
            {
                var packages = Package.LoadByIsActiveByIsAfterSchool(true, false);
                ViewBag.Packages = packages;
                ViewBag.FeatureTypes = new List<FeatureType>() { FeatureType.AcceptPayments, FeatureType.Activities, FeatureType.TeamAccounts };
            }

            return View();
        }
        public ActionResult ASHQ()
        {
            AfterSchool.Models.Utilities.SetAfterSchool(true);
            var package = Package.LoadByIsActiveByIsAfterSchool(true, true).FirstOrDefault();
            ViewBag.Package = package;
            return View();
        }
    }
}
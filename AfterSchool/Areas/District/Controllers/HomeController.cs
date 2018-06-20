using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchool.Models;

namespace AfterSchool.Areas.District.Controllers
{
    public class HomeController : BaseLoggedInController
    {
        // GET: District/Home
        public ActionResult Index()
        {
            return View();
        }
    }
}
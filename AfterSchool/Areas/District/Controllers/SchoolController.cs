using AfterSchool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AfterSchool.Areas.District.Controllers
{
    public class SchoolController : BaseLoggedInController
    {
        // GET: District/School
        public ActionResult Index()
        {
            return View();
        }
    }
}
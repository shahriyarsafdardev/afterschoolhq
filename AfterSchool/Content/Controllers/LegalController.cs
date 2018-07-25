using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AfterSchool.Controllers
{
    public class LegalController : Controller
    {
        // GET: Legal
        public ActionResult Tos()
        {
            return View();
        }

        public ActionResult Privacy()
        {
            return View();
        }

        public ActionResult Copyright()
        {
            return View();
        }
    }
}
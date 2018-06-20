using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchool.Models;
using BasicLibrary;

namespace AfterSchool.Areas.District.Controllers
{
    public class SportController : BaseLoggedInController
    {
        // GET: District/Sport
        public ActionResult Summary()
        {
            ViewBag.Messages = new SystemMessages();
            return View();
        }
    }
}
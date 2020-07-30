using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AfterSchool.Controllers
{
    public class IndianapolisController : Controller
    {
        // GET: Indianapolis
        public ActionResult Index()
        {
            return RedirectToAction("index", "search", new { city = "indianapolis", state = "indiana" });
        }
    }
}
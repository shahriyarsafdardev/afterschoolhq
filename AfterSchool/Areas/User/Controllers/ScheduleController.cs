using AfterSchool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AfterSchool.Areas.User.Controllers
{
    public class ScheduleController : BaseLoggedInController
    {
        public ScheduleController() : base(new List<AccountType>() { AccountType.Registrant })
        {

        }

        // GET: User/Schedule
        public ActionResult Index()
        {
            return View();
        }
    }
}
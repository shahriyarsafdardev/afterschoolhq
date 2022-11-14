using AfterSchool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Areas.District.Controllers
{
    public class VolunteerController : BaseLoggedInController
    {
        public VolunteerController() : base(new List<AccountType> { AccountType.SchoolDistrict })
        {

        }

        // GET: District/Profile
        public ActionResult Index()
        {
            List<Account> account = new List<Account>();
            var msgs = new SystemMessages();
            ViewBag.Messages = msgs;
            return View(account);
        }
        
        public ActionResult BackgroundCheck()
        {
            List<Account> account = new List<Account>();
            var msgs = new SystemMessages();
            ViewBag.Messages = msgs;
            return View(account);
        }
        
        public ActionResult Todo()
        {
            List<Account> account = new List<Account>();
            var msgs = new SystemMessages();
            ViewBag.Messages = msgs;
            return View(account);
        }
    }
}
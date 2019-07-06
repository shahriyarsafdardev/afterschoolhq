using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AfterSchool.Areas.User.Controllers
{
    public class FamilyController : BaseLoggedInController
    {
        public FamilyController() : base(new List<AccountType>() { AccountType.Registrant })
        {

        }

        // GET: User/Family
        public ActionResult Index()
        {
            var msgs = new SystemMessages();
            var registrants = SessionVariables.CurrentUser.GetAllRegistrants(msgs);

            return View(registrants);
        }
    }
}
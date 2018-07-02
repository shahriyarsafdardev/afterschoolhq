using AfterSchool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AfterSchool.Areas.District.Controllers
{
    public class ProfileController : BaseLoggedInController
    {
        public ProfileController() : base(new List<AccountType> { AccountType.SchoolDistrict })
        {

        }

        // GET: District/Profile
        public ActionResult Index()
        {
            return View();
        }
    }
}
using AfterSchool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AfterSchool.Areas.District.Controllers
{
    public class MessageController : BaseLoggedInController
    {
        public MessageController() : base(new List<AccountType> { AccountType.SchoolDistrict })
        {

        }

        // GET: District/Message
        public ActionResult Index()
        {
            return View();
        }
    }
}
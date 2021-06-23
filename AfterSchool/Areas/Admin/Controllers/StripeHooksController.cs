using AfterSchool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AfterSchool.Areas.Admin.Controllers
{
    public class StripeHooksController : BaseLoggedInController
    {
        public StripeHooksController() : base(new List<AccountType> { AccountType.AfterSchool })
        {

        }

        // GET: Admin/StripeHooks
        public ActionResult Index()
        {
            return View();
        }
    }
}
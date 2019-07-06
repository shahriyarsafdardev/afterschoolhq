using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;

namespace AfterSchool.Areas.User.Controllers
{
    public class HomeController : BaseLoggedInController
    {
        public HomeController() : base(new List<AccountType>() { AccountType.Registrant })
        {

        }

        // GET: User/Home
        public ActionResult Index()
        {
            var msgs = new SystemMessages();            
            var registrants = SessionVariables.CurrentAccount.GetChildrenRegistrants(msgs)
                                .OrderBy(x => x.FirstName)
                                .ThenBy(x => x.LastName)
                                .ToList();            

            ViewBag.Messages = msgs;

            return View(registrants);
        }
    }
}
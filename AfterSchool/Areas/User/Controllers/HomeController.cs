using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;
using AfterSchool.Models;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace AfterSchool.Areas.User.Controllers
{
    public class HomeController : BaseLoggedInController
    {
        public HomeController() : base(null)
        {

        }

        // GET: User/Home
        public ActionResult Index()
        {
            var registrants = Registrant.LoadByWebUserIdByIsMaster(SessionVariables.LoggedInUser.ID.Value, true)
                                .OrderBy(x => x.FirstName)
                                .ThenBy(x => x.LastName).ToList();
            return View(registrants);
        }
    }
}
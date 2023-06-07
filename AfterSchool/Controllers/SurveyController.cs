using System.Web.Mvc;
using AfterSchool.Models;
using BasicLibrary;
using LibraryUtilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Controllers
{
    public class SurveyController : BaseController
    {
        [HttpGet]
        public ActionResult Index(string id)
        {
            if (!id.IsGuid()) return Redirect($"~/404.html");
            return Redirect($"~/reactapp/survey/{id.ToGuid()}");
        }   
    }
}
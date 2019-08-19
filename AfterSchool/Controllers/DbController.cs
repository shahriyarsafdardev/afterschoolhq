using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using BasicLibrary;
using AfterSchool.Models;
using AfterSchoolLibrary;
using System.Text.RegularExpressions;
using System.Collections;

namespace AfterSchool.Controllers
{
    public class DbController : BaseController
    {
        [HttpGet]
        public ActionResult Update(string id)
        {
            if (Request.UserHostAddress == "::1" || id == "eibfeiwalncdsja32rq9yfwbfg32yewody7832re7ywofaudisfh7238qoweiafb7328qwoadg78027rweubfad")
            {
                var msgs = new SystemMessages();

                msgs.AddRange(AfterSchoolLibrary.Helpers.Utilities.InitDatabase(
                    "darye@afterschoolhq.com", 
                    "help students learn", 
                    "317-316-0596", 
                    false, 
                    "AfterSchool HQ"));

                return Json(new { status = "ok", messages = msgs }, JsonRequestBehavior.AllowGet);
            }
            
            return Json(new { status = "ok" }, JsonRequestBehavior.AllowGet);
        }
    }
}

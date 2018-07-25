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
        public ActionResult Update(string id)
        {

            var msgs = new SystemMessages();
            msgs.AddRange(AfterSchoolLibrary.Utilities.InitDatabase("darye@afterschool.com", "help students learn", "317-316-0596", false, "AfterSchool HQ"));
            
            Response.Clear();
            Response.Write(msgs.RenderMessageList(MessageOutputType.Text));
            Response.End();

            return View();
        }
    }
}

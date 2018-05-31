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
        public ActionResult Update()
        {
            SystemMessages msgs = new SystemMessages();
            msgs.AddRange(AfterSchoolLibrary.Utilities.InitDatabase("darye@reborncode.com", "aDm1n1906", false, "EnrollHQ"));
            
            Response.Clear();
            Response.Write(msgs.RenderMessageList(MessageOutputType.Text));
            Response.End();

            return View();
        }
    }
}

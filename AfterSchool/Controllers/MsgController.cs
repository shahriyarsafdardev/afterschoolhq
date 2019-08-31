using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;

namespace AfterSchool.Controllers
{
    public class MsgController : BaseController
    {
        // GET: Msg
        public ActionResult Index(string id)
        {
            if (id.IsGuid())
            {
                var am = ActivityMessage.LoadByUid(id.ToGuid());
                
                if (am.IsNotEmpty())
                    return View(am);
            }

            return View("Invalid");
        }
    }
}
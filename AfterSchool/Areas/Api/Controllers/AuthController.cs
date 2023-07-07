using AfterSchool.Areas.Api.Models;
using AfterSchool.Models;
using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AfterSchool.Areas.Api.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet]
        public ActionResult GetCurrentUser()
        {
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;

                if (currentUser.IsEmpty())
                {
                    msgs.Add("User Not Found", MessageType.ApiError);
                    throw new Exception("Unauthorized");
                }

                return Json(
                new ApiResponse(msgs,
                    new WebUserDto(currentUser)),
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);
                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }
    }
}
using AfterSchoolLibrary.Services;
using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AfterSchool.Areas.Admin.Controllers
{
    public class ScheduleTasksController : Controller
    {
        // GET: Admin/ScheduleTasks
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RunCronJob()
        {
            var msgs = new SystemMessages();
            msgs.AddRange(AccountService.PreparePayouts());

            if (msgs.HasNoErrors())
                return Json(new { success = true, status = "ok", messages = msgs.ToList().Select(msg => new { msg.Text }) }, JsonRequestBehavior.AllowGet);

            return Json(new { success = false, status = "error", messages = msgs.ToList().Select(msg => new { msg.Text }) }, JsonRequestBehavior.AllowGet);
        }
    }
}
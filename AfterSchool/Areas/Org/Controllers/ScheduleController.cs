using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchool.Models;
using System.Web.Script.Serialization;

namespace AfterSchool.Areas.Org.Controllers
{
    public class ScheduleController : Controller
    {

        [HttpGet]
        public ActionResult Copy(string id, string activity)
        {
            var schedule = ActivitySchedule.Load(id.ToInteger());
            var copy = schedule.GetCopy();

            ViewBag.Messages = new SystemMessages();
            ViewBag.ShowScheduleList = false;
            ViewBag.ActivityId = activity;
            return View("edit", copy);
        }

        [HttpGet]
        public ActionResult Edit(string id, string activity, string group)
        {
            var schedule = new ActivitySchedule();
            if (id.ToInteger() > 0)
            {
                schedule = ActivitySchedule.Load(id.ToInteger());
            }
            else
            {
                schedule = ActivitySchedule.GetNewDefaultSchedule();
            }
            ViewBag.Messages = new SystemMessages();
            ViewBag.ShowScheduleList = false;
            ViewBag.ActivityId = activity;
            ViewBag.ActivityGroupUid = group;
            return View(schedule);
        }

        [HttpGet]
        public ActionResult EditForActivity(string id, string activity)
        {
            var schedule = new ActivitySchedule();
            var act = Activity.Load(activity.ToInteger());

            if (id.ToInteger() > 0)
            {
                schedule = ActivitySchedule.Load(id.ToInteger());
            }
            else
            {
                schedule = ActivitySchedule.GetNewDefaultSchedule();
            }

            ViewBag.Messages = new SystemMessages();
            ViewBag.ShowScheduleList = true;
            ViewBag.Schedules = ActivitySchedule.LoadByAccountId(act.AccountId.Value);
            ViewBag.ActivityId = activity;
            return View(schedule);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Edit(string id, string name, string scheduledetails, string repeatsweekly, string startdate, string enddate, string dailystarttime, string dailyendtime, string scheduledates, string activity, string group)
        {
            var msgs = new SystemMessages();
            var schedule = new ActivitySchedule();

            var scheduleRepeat = Request.Form.Filter("schedulerepeat-");
            var meetingInfo = Request.Form.Filter("meeting-");
            var days = ActivityScheduleUI.PrepDaysOfWeek(scheduleRepeat);
            var scheduleDates = ActivityScheduleUI.PrepScheduleDates(repeatsweekly, days, startdate, enddate, dailystarttime, dailyendtime, meetingInfo);

            if (id.ToInteger() > 0)
            {
                schedule = ActivitySchedule.EditExisting(id, name, scheduledetails, repeatsweekly.IsBoolean() ? repeatsweekly.ToBoolean() : false, startdate.ToDateTime(), enddate.ToDateTime(), dailystarttime.ToDateTime(), dailyendtime.ToDateTime(), scheduleDates.ToStringList(), SessionVariables.LoggedInUser.ID.Value, msgs);
            }
            else
            {
                var groupId = group.IsGuid() ? ActivityGroup.LoadByUid(group.ToGuid()).ID : null;

                schedule = ActivitySchedule.CreateNew(name, scheduledetails, repeatsweekly.IsBoolean() ? repeatsweekly.ToBoolean() : false, startdate.ToDateTime(), enddate.ToDateTime(), dailystarttime.ToDateTime(), dailyendtime.ToDateTime(), scheduleDates.ToStringList(), SessionVariables.CurrentAccount.ID.Value, SessionVariables.LoggedInUser.ID.Value, groupId, msgs);
            }

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} Schedule Saved", UILabel.ActivityLabel()), MessageType.Success);
            }

            ViewBag.Messages = msgs;
            ViewBag.ShowScheduleList = false;
            ViewBag.ActivityId = activity;
            ViewBag.ActivityGroupUid = group;

            if (msgs.HasNoErrors())
            {
                if (activity.IsNumber())
                {
                    return RedirectToAction("editschedule", "activity", new { id = activity, schedule = schedule.ID });
                }
                else if (group.IsGuid())
                {
                    return RedirectToAction("schedules", "group", new { id = group });
                }
            }

            return View(schedule);
        }

    }
}
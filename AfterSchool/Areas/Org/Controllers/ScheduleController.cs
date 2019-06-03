using System.Collections.Generic;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;

namespace AfterSchool.Areas.Org.Controllers
{
    public class ScheduleController : BaseLoggedInController
    {
        public ScheduleController() : base(new List<AccountType> { AccountType.School, AccountType.ActivityProvider })
        {

        }

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
            var act = Activity.LoadByUid(activity.ToGuid());

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
        public ActionResult Edit(string id, string name, string scheduledetails, string repeatsweekly, string startdate, string enddate, string dailystarttime, string dailyendtime, string scheduledates, string isongoing, string activity, string group)
        {
            var msgs = new SystemMessages();
            var schedule = new ActivitySchedule();

            var scheduleRepeat = Request.Form.Filter("schedulerepeat-");
            var meetingOpenInfo = Request.Form.Filter("meeting-open-");
            var meetingClosedInfo = Request.Form.Filter("meeting-closed-");
            var days = ActivityScheduleUI.PrepDaysOfWeek(scheduleRepeat);
            var scheduleDates = ActivityScheduleUI.PrepScheduleDates(repeatsweekly, days, startdate, enddate, dailystarttime, dailyendtime, meetingOpenInfo);
            var scheduleDatesClosed = ActivityScheduleUI.PrepScheduleDatesClosed(meetingClosedInfo);

            if (id.ToInteger() > 0)
            {
                schedule = ActivitySchedule.EditExisting(id, name, scheduledetails, repeatsweekly.IsBoolean() ? repeatsweekly.ToBoolean() : false, isongoing.IsBoolean() ? isongoing.ToBoolean() : false, startdate.ToDateTime(), enddate.ToDateTime(), dailystarttime.ToDateTime(), dailyendtime.ToDateTime(), scheduleDates, scheduleDatesClosed, SessionVariables.CurrentUser.ID.Value, msgs);
            }
            else
            {
                var groupId = group.IsGuid() ? ActivityGroup.LoadByUid(group.ToGuid()).ID : null;

                schedule = ActivityScheduleService.CreateNew(name, scheduledetails, repeatsweekly.IsBoolean() ? repeatsweekly.ToBoolean() : false, isongoing.IsBoolean() ? isongoing.ToBoolean() : false, startdate.ToDateTime(), enddate.ToDateTime(), dailystarttime.ToDateTime(), dailyendtime.ToDateTime(), scheduleDates, scheduleDatesClosed, SessionVariables.CurrentAccount.ID.Value, groupId, SessionVariables.CurrentUser.ID.Value, msgs);
            }

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} Schedule Saved", UILabel.ActivityLabel(ActivityType.Activity)), MessageType.Success);
            }

            ViewBag.Messages = msgs;
            ViewBag.ShowScheduleList = false;
            ViewBag.ActivityId = activity;
            ViewBag.ActivityGroupUid = group;

            if (msgs.HasNoErrors())
            {
                if (activity.IsGuid())
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
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using System.Linq;

namespace AfterSchool.Areas.Org.Controllers
{
    public class ScheduleController : BaseLoggedInController
    {
        public ScheduleController() : base(AccountService.ProviderAccountTypes.ToList())
        {

        }

        [HttpGet]
        public ActionResult Copy(string id, string activity)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            
            if (currentAccount.IsEmpty() ||!id.IsInteger())
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var schedule = currentAccount.GetActivitySchedule(id.ToInteger(), msgs);
            var copy = schedule.GetCopy(msgs);

            ViewBag.Messages = msgs;
            ViewBag.ShowScheduleList = false;
            ViewBag.ActivityId = activity;
            return View("edit", copy);
        }

        [HttpGet]
        public ActionResult Edit(string id, string activity, string group)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty() || !id.IsInteger())
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var schedule = new ActivitySchedule();
            if (id.ToInteger() > 0)
                schedule = currentAccount.GetActivitySchedule(id.ToInteger(), msgs);
            else
                schedule = ActivitySchedule.GetNewDefaultSchedule();

            ViewBag.Messages = msgs;
            ViewBag.ShowScheduleList = false;
            ViewBag.ActivityId = activity;
            ViewBag.ActivityGroupUid = group;
            return View(schedule);
        }

        [HttpGet]
        public ActionResult EditForActivity(string id, string activity)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty() || !id.IsInteger() || activity.IsNotGuid())
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var schedule = new ActivitySchedule();
            if (id.ToInteger() > 0)
                schedule = currentAccount.GetActivitySchedule(id.ToInteger(), msgs);
            else
                schedule = ActivitySchedule.GetNewDefaultSchedule();

            ViewBag.Schedules = currentAccount.GetActivitySchedules(msgs);
            ViewBag.Messages = msgs;
            ViewBag.ShowScheduleList = true;            
            ViewBag.ActivityId = activity;
            return View(schedule);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Edit(string id, string schedule, string name, string scheduledetails, string repeatsweekly, string startdate, string enddate, string dailystarttime, string dailyendtime, string scheduledates, string isongoing, string activity, string group)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            if (schedule.IsNotEmpty())
                id = schedule;

            var activitySchedule = new ActivitySchedule();
            var scheduleRepeat = Request.Form.Filter("schedulerepeat-");
            var meetingOpenInfo = Request.Form.Filter("meeting-open-");
            var meetingClosedInfo = Request.Form.Filter("meeting-closed-");
            var days = ActivityScheduleUI.PrepDaysOfWeek(scheduleRepeat);
            var scheduleDates = ActivityScheduleUI.PrepScheduleDates(repeatsweekly, days, startdate, enddate, dailystarttime, dailyendtime, meetingOpenInfo);
            var scheduleDatesClosed = ActivityScheduleUI.PrepScheduleDatesClosed(meetingClosedInfo);

            if (id.ToInteger() > 0)
                activitySchedule = ActivitySchedule.EditExisting(id, name, scheduledetails, repeatsweekly.IsBoolean() && repeatsweekly.ToBoolean(), isongoing.IsBoolean() && isongoing.ToBoolean(), startdate.ToDateTime(), enddate.ToDateTime(), dailystarttime.ToDateTime(), dailyendtime.ToDateTime(), scheduleDates, scheduleDatesClosed, currentUser.ID.Value, msgs);
            else
            {
                var groupId = group.IsGuid() ? currentAccount.GetActivityGroup(group.ToGuid(), msgs).ID : null;
                activitySchedule = ActivityScheduleService.CreateNew(name, scheduledetails, repeatsweekly.IsBoolean() && repeatsweekly.ToBoolean(), isongoing.IsBoolean() && isongoing.ToBoolean(), startdate.ToDateTime(), enddate.ToDateTime(), dailystarttime.ToDateTime(), dailyendtime.ToDateTime(), scheduleDates, scheduleDatesClosed, currentAccount.ID.Value, groupId, currentUser.ID.Value, msgs);
            }

            if (msgs.HasNoErrors())
                msgs.Add(string.Format("{0} Schedule Saved", UILabel.ActivityLabel(ActivityType.Activity)), MessageType.Success);

            ViewBag.Messages = msgs;
            ViewBag.ShowScheduleList = false;
            ViewBag.ActivityId = activity;
            ViewBag.ActivityGroupUid = group;

            if (msgs.HasNoErrors())
            {
                if (activity.IsGuid())
                    return RedirectToAction("editschedule", "activity", new { id = activity, schedule = activitySchedule.ID });
                else if (group.IsGuid())
                    return RedirectToAction("schedules", "group", new { id = group });
            }

            return View(activitySchedule);
        }
    }
}
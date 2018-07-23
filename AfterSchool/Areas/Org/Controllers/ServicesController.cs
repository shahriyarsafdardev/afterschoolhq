using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchool.Models;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using BasicLibrary.DataAccess;

namespace AfterSchool.Areas.Org.Controllers
{
    public class ServicesController : BaseLoggedInController
    {
        public ServicesController() : base(new List<AccountType> { AccountType.School, AccountType.ActivityProvider })
        {

        }

        [HttpGet]
        public ActionResult GetScheduleActivities(string id)
        {
            var msgs = new SystemMessages();
            Response.Clear();

            if (id.IsNumber())
            {
                var schedule = ActivitySchedule.Load(id.ToInteger());
                var activities = schedule.GetActivities();

                WriteOutput(new { status = "ok", activities = activities.Select(x => new { x.ID, x.Name }) }, ResponseType.JSON);

            }
            else
            {
                msgs.Add("Invalid data", MessageType.ApiUsageError);
            }

            if (msgs.HasErrors())
            {
                HandleErrors(msgs, ResponseType.JSON);
            }
            Response.End();

            return View();
        }

        [HttpPost]
        public ActionResult SetScheduleActivities(string id)
        {
            var msgs = new SystemMessages();
            Response.Clear();

            if (id.IsNumber())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        var schedule = ActivitySchedule.Load(id.ToInteger(), dc: adc);
                        var currentActivities = schedule.GetActivities(adc);
                        var activities = schedule.GetActivityGroup(adc).GetActivities(adc);

                        // remove
                        foreach (var item in currentActivities)
                        {
                            if (!Request["activity-" + item.ID].IsNumber())
                            {
                                item.ActivityScheduleId = null;
                                amsgs.AddRange(item.Save(adc));
                            }
                        }

                        // add
                        foreach (var item in activities)
                        {
                            if (Request["activity-" + item.ID].IsNumber())
                            {
                                item.ActivityScheduleId = schedule.ID;
                                amsgs.AddRange(item.Save(adc));
                            }
                        }
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        WriteOutput(new { status = "ok" }, ResponseType.JSON);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
            }
            else
            {
                msgs.Add("Invalid data", MessageType.ApiUsageError);
            }

            if (msgs.HasErrors())
            {
                HandleErrors(msgs, ResponseType.JSON);
            }
            Response.End();

            return View();
        }

        [HttpGet]
        public ActionResult UpdateSchedule(string id, string schedule)
        {
            var msgs = new SystemMessages();
            Response.Clear();

            if (id.IsGuid() && schedule.IsNumber())
            {
                var activity = Activity.LoadByUid(id.ToGuid());
                var sched = ActivitySchedule.Load(schedule.ToInteger());

                if (activity.AccountId != SessionVariables.CurrentAccount.ID || sched.AccountId != SessionVariables.CurrentAccount.ID)
                {
                    msgs.Add("Invalid data", MessageType.ApiUsageError);
                }

                if (msgs.HasNoErrors())
                {
                    activity.ActivityScheduleId = sched.ID;
                    msgs.AddRange(activity.Save());
                } 
            }
            else
            {
                msgs.Add("Invalid data", MessageType.ApiUsageError);
            }

            if (msgs.HasNoErrors())
            {
                WriteOutput(new { status = "ok" }, ResponseType.JSON);
            }
            else
            {
                HandleErrors(msgs, ResponseType.JSON);
            }
            Response.End();

            return View();
        }

        [HttpGet]
        public ActionResult AddContactToList(string list, string contact)
        {
            var msgs = new SystemMessages();
            Response.Clear();

            if (!list.IsInteger() || !contact.IsGuid())
            {
                msgs.Add("Invalid data", MessageType.ApiUsageError);
            }

            if (msgs.HasNoErrors())
            {
                var contactList = ContactList.Load(list.ToInteger());
                var registrant = Registrant.LoadByUid(contact.ToGuid());

                if (contactList.AccountId == SessionVariables.CurrentAccount.ID.Value)
                {
                    msgs.AddRange(contactList.AddContact(registrant.ID.Value, SessionVariables.LoggedInUser.ID.Value));
                    if (msgs.HasNoErrors())
                    {
                        WriteOutput(new { status = "ok", contactName = registrant.DisplayName, listName = contactList.Name }, ResponseType.JSON);
                    }
                }
                else
                {
                    msgs.Add("Invalid data", MessageType.ApiUsageError);
                }
            }

            if (msgs.HasErrors())
            {
                HandleErrors(msgs, ResponseType.JSON);
            }
            Response.End();

            return View();
        }

        [HttpGet]
        public ActionResult RemoveFromContactList(string list, string contact)
        {
            var msgs = new SystemMessages();
            Response.Clear();

            if (!list.IsInteger() || !contact.IsGuid())
            {
                msgs.Add("Invalid data", MessageType.ApiUsageError);
            }

            if (msgs.HasNoErrors())
            {
                var contactList = ContactList.Load(list.ToInteger());
                var registrant = Registrant.LoadByUid(contact.ToGuid());

                if (contactList.AccountId == SessionVariables.CurrentAccount.ID.Value)
                {
                    msgs.AddRange(contactList.RemoveContact(registrant.ID.Value, SessionVariables.LoggedInUser.ID.Value));
                    if (msgs.HasNoErrors())
                    {
                        WriteOutput(new { status = "ok", contactName = registrant.DisplayName, listName = contactList.Name }, ResponseType.JSON);
                    }
                }
                else
                {
                    msgs.Add("Invalid data", MessageType.ApiUsageError);
                }
            }

            if (msgs.HasErrors())
            {
                HandleErrors(msgs, ResponseType.JSON);
            }
            Response.End();

            return View();
        }

        [HttpGet]
        public ActionResult DeleteContact(string id)
        {
            Response.Clear();
            var msgs = new SystemMessages();
            if (id.IsGuid())
            {
                Registrant contact = null;

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        contact = Registrant.LoadByUid(id.ToGuid(), dc: adc);
                        if (contact.IsNotEmpty() && contact.AccountId == SessionVariables.CurrentAccount.ID && contact.IsContactOnly)
                        {
                            var rags = RegistrantActivityGroup.LoadByRegistrantId(contact.ID.Value, dc: adc);
                            foreach(var rag in rags)
                            {
                                amsgs.AddRange(rag.Delete(adc));
                            }

                            var ras = RegistrantActivity.LoadByRegistrantId(contact.ID.Value, dc: adc);
                            foreach(var ra in ras)
                            {
                                amsgs.AddRange(ra.Delete(adc));
                            }

                            amsgs.AddRange(contact.Delete(adc));
                        }
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        WriteOutput(new { status = "ok", contactName = contact.DisplayName }, ResponseType.JSON);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
                
            }
            else
            {
                msgs.Add("Invalid data", MessageType.ApiUsageError);
            }

            if (msgs.HasErrors())
            {
                HandleErrors(msgs, ResponseType.JSON);
            }

            Response.End();
            return View();
        }

        // GET: Org/Services
        public ActionResult GetAnswerOptions(string id)
        {
            var question = FormQuestion.Load(id.ToInteger());
            var list = new List<string>();

            if (question.IsNotEmpty() && question.Options.IsNotEmpty())
            {
                list = question.Options.Split(',').ToList();
            }

            Response.Clear();
            WriteOutput(new { status = "ok", answers = list }, ResponseType.JSON);
            Response.End();

            return View();
        }

        public ActionResult GetActivityDays(string id)
        {
            var activity = Activity.LoadByUid(id.ToGuid());
            Response.Clear();
            WriteOutput(new { status = "ok", days = activity.GetDaysOfTheWeek().Select(x => new { Value = (int)x, Text = x.ToString() }) }, ResponseType.JSON);
            Response.End();

            return View();
        }

        public ActionResult GetRegistrationMaxChoiceNumbersForDay(string id, string day)
        {
            var ras = RegistrantActivity.LoadByRegistrationId(id.ToInteger());
            var dow = (DayOfWeekASL)day.ToInteger();

            Response.Clear();
            WriteOutput(new { status = "ok", MaxChoices = ras.Where(x => x.Day == dow).Max(x => x.PreferenceNumber.Value) }, ResponseType.JSON);
            Response.End();

            return View();
        }

        public ActionResult SendVerificationEmail()
        {
            var msgs = SessionVariables.LoggedInUser.SendVerificationEmail(SessionVariables.IsAfterSchool.Value);

            Response.Clear();
            WriteOutput(new { status = "ok", messages = msgs }, ResponseType.JSON);
            Response.End();

            return View();
        }

        public ActionResult SearchLocations(string search)
        {
            var msgs = new SystemMessages();
            var locations = Location.Search(search.Trim(), 0, 10, msgs);

            Response.Clear();
            WriteOutput(new
            {
                status = "ok",
                locations = locations.Select(location => new
                {
                    location.ID,
                    location.Name,
                    location.Address1,
                    location.Address2,
                    location.City,
                    location.State,
                    location.PostalCode,
                    FullAddress = location.FormatAddress(useLineBreaks: false),
                    location.HostAccountId
                })
            }, 
            ResponseType.JSON);
            Response.End();           

            return View();
        }
    }
}
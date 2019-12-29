using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using Hangfire;
using System.Collections;
using System.Web.Script.Serialization;

namespace AfterSchool.Areas.Org.Controllers
{
    public class SportsController : BaseLoggedInController
    {
        public SportsController() : base(new List<AccountType>() { AccountType.ActivityProvider, AccountType.School })
        {

        }

        

        [HttpGet]
        public ActionResult Summary(string id)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUid(id.ToGuid(), msgs);
            var registrations = group.GetRegistrations(msgs: msgs);
            var actions = SessionAction.LoadByActivityGroupId(
                group.ID.Value,
                new List<SessionActionType>() {
                    SessionActionType.ActivityGroupCreate,
                    SessionActionType.ActivityGroupPublish,
                    SessionActionType.ActivityGroupUnpublish,
                },
                0,
                15,
                msgs);
            ViewBag.Messages = new SystemMessages();
            ViewBag.Actions = actions;
            ViewBag.Registrations = registrations;
            return View(group);
        }

        [HttpGet]
        public ActionResult Edit()
        {
            return RedirectToAction("EditSetup");
        }

    }
}
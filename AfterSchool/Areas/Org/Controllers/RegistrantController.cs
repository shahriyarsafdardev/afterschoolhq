using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchool.Models;

namespace AfterSchool.Areas.Org.Controllers
{
    public class RegistrantController : BaseLoggedInController
    {
        // GET: Org/Registrant
        public ActionResult Index(string activity, string pn, string rpp)
        {
            var registrants = new List<RegistrantActivity>();
            var activities = new List<Activity>();
            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
            {
                pageNumber = pn.ToInteger();
            }
            if (rpp.IsNumber() && rpp.ToInteger() > 0)
            {
                recordsPerPage = rpp.ToInteger();
            }

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;
            var totalCount = 0;

            if (activity.IsNumber())
            {
                totalCount = SessionVariables.CurrentAccount.GetRegistrantActivityCountByActivityId(activity.ToInteger());
                registrants = SessionVariables.CurrentAccount.GetRegistrantActivitiesByActivityId(activity.ToInteger(), start, length);               
            }
            else
            {
                totalCount = SessionVariables.CurrentAccount.GetRegistrantActivityCount();
                registrants = SessionVariables.CurrentAccount.GetRegistrantActivities(start, length);
            }

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();
            registrants = registrants.OrderBy(x => x.RegistrantName).ToList();
            activities = SessionVariables.CurrentAccount.GetActivities(UserType.Administrator, SessionVariables.LoggedInUser, false, false, null, null).OrderBy(x => x.Name).ToList();

            ViewBag.Messages = new SystemMessages();
            ViewBag.Activities = activities;
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };
            return View(registrants);
        }

        public ActionResult Edit(string id)
        {
            var registrant = RegistrantActivity.Load(id.ToInteger());
            if(registrant.ActivityAccountId != SessionVariables.CurrentAccount.ID)
            {
                registrant = null;
            }
            ViewBag.Messages = new SystemMessages();
            return View(registrant);
        }
    }
}
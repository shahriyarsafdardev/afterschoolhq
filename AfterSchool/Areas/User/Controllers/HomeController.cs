using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using System;

namespace AfterSchool.Areas.User.Controllers
{
    public class HomeController : BaseLoggedInController
    {
        public HomeController() : base(new List<AccountType>() { AccountType.Registrant })
        {

        }

        // GET: User/Home
        public ActionResult Index()
        {
            var msgs = new SystemMessages();
            var masterRegistrant = SessionVariables.CurrentUser.GetMasterRegistrant(msgs);
            var registrants = SessionVariables.CurrentUser.GetChildrenRegistrants(msgs);
            var activityInstances = new List<(
                ActivityInstance ActivityInstance, 
                Registrant Registrant, 
                int? ParticipationObjectId, 
                ParticipationObjectType ParticipationObjectType)>();
            var registrantActivities = new List<RegistrantActivity>();
            var assignmentTypesNeedingAttention = new List<RegistrationAssignmentType>()
                {
                    RegistrationAssignmentType.ReservedPendingParentalConsent,
                    RegistrationAssignmentType.ReservedPendingPayment
                };

            if (!registrants.Any(registrant => registrant.ID == masterRegistrant.ID))
                registrants.Add(masterRegistrant);

            registrants.ForEach(registrant => {
                registrant.GetSubmittedRecords(msgs).ForEach(submittedRecord => {

                    var ras = submittedRecord.GetRegistrantActivities(msgs);

                    ras.ForEach(ra => {
                        var instances = ra.GetSchedule(msgs).GetInstances(msgs: msgs);
                        activityInstances.AddRange(instances.Select(instance => (instance, registrant, ra.ParticipationObjectId, ra.ParticipationObjectTypeId)));
                    });

                    registrantActivities.AddRange(ras);
                });
            });

            ViewBag.ActivityInstances = activityInstances
                .Where(item => item.ActivityInstance.StartDateTime.Value.Date >= DateTime.UtcNow.Date)
                .OrderBy(item => item.ActivityInstance.StartDateTime.Value)
                .Take(4)
                .ToList();
            ViewBag.ActivitiesRequiringAttention = registrantActivities.Where(registrantActivity => registrantActivity.AssignmentTypeId.In(assignmentTypesNeedingAttention)).ToList();
            ViewBag.Messages = msgs;

            return View(registrants);
        }
    }
}
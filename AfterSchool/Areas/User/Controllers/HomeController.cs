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
            var providers = new List<Account>();

            if (!registrants.Any(registrant => registrant.ID == masterRegistrant.ID) && masterRegistrant.IsNotEmpty())
                registrants.Add(masterRegistrant);

            registrants.ForEach(registrant => {

                if (registrant.SchoolLocationId.HasValue)
                {
                    var account = registrant.GetSchoolLocation(msgs)?.GetHostAccount(msgs);                   
                    if(account.IsNotEmpty() && !providers.Any(provider => provider.ID == account.ID))
                        providers.Add(account);
                }

                registrant.GetSubmittedRecords(msgs).ForEach(submittedRecord => {

                    var ras = submittedRecord.GetRegistrantActivities(Registration.ReservedStatuses, msgs);
                    var account = submittedRecord.GetAccount(msgs);

                    ras.ForEach(ra => {
                        var instances = ra.GetSchedule(msgs)?.GetInSessionInstances(msgs: msgs);

                        var participantObject = new ParticipationObject(ra.ParticipationObjectId.Value, ra.ParticipationObjectTypeId, msgs);

                        if (instances.IsNotEmpty())
                        {
                            var activityInstanceSeats = ActivityInstanceSeats.LoadByRegistrantActivityId(ra.ID.Value, msgs);
                            if (participantObject.CanRegisterPerClass)
                                instances.RemoveAll(instance => instance.ID.NotIn(activityInstanceSeats.Select(activityInstanceSeat => activityInstanceSeat.ActivityInstanceId)));

                            activityInstances.AddRange(instances.Select(instance => (
                                instance,
                                registrant,
                                ra.ParticipationObjectId,
                                ra.ParticipationObjectTypeId)));
                        }
                    });

                    if (masterRegistrant.IsParentOrGuardian)
                        registrantActivities.AddRange(ras);

                    if (!providers.Any(provider => provider.ID == account.ID))
                        providers.Add(account);
                });
            });

            ViewBag.ActivityInstances = activityInstances
                .Where(item => item.ActivityInstance.StartDateTime.Value.Date >= DateTime.UtcNow.Date)
                .OrderBy(item => item.ActivityInstance.StartDateTime.Value)
                .Take(10)
                .ToList();

            ViewBag.ActivitiesRequiringAttention = registrantActivities.Where(ra =>
                    (ra.AssignmentTypeId.In(assignmentTypesNeedingAttention) ||
                    (ra.AssignmentTypeId == RegistrationAssignmentType.Applied && ra.GetRegistration().ESignature.IsEmpty())) &&
                    !ra.HasPlan()
                ).ToList();

            ViewBag.Providers = providers;
            ViewBag.Messages = msgs;

            return View(registrants);
        }
    }
}
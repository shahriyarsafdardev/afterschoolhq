using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AfterSchool.Areas.User.Models;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;

namespace AfterSchool.Areas.User.Controllers
{
    public class ActivityController : BaseLoggedInController
    {
        public ActivityController() : base(new List<AccountType> {AccountType.Registrant})
        {
        }

        // GET: User/Activity
        public ActionResult Index()
        {
            var msgs = new SystemMessages();
            var registrants = SessionVariables.CurrentUser.GetStudentRegistrants(msgs);
            var registeredActivities = new List<RegistrantActivityViewModel>();

            registrants.ForEach(registrant =>
            {
                var participationViewModels = new List<ParticipationViewModel>();

                registrant.GetSubmittedRecords(msgs).ForEach(submittedRecord =>
                {
                    var ras = submittedRecord.GetRegistrantActivities(msgs)
                        .Where(ra => ra.ParticipationObjectTypeId != ParticipationObjectType.ActivityGroup)
                        .ToList();

                    ras.ForEach(ra =>
                    {
                        var participationObject = ra.GetParticipationObject(msgs);
                        var isPast = participationObject.IsOutOfSession(msgs) || ra.AssignmentTypeId == RegistrationAssignmentType.Withdrawn;

                        if (isPast)
                            participationViewModels.Add(new ParticipationViewModel(participationObject, ra, submittedRecord, RegistrationStatus.Past));
                        else if (participationObject.IsInSession(msgs))
                            participationViewModels.Add(new ParticipationViewModel(participationObject, ra, submittedRecord, RegistrationStatus.Current));
                        else if (participationObject.WillBeInSession(msgs))
                            participationViewModels.Add(new ParticipationViewModel(participationObject, ra, submittedRecord, RegistrationStatus.Future));
                    });
                });

                registeredActivities.Add(new RegistrantActivityViewModel(registrant, participationViewModels));
            });

            return View(registeredActivities);
        }

        [HttpGet]
        public ActionResult Withdraw(string id, string po, string type, string returnurl)
        {
            var msgs = new SystemMessages();
            var registrant = Registrant.LoadByUid(id.ToGuid());

            registrant.WithdrawRegistration(
                po.ToInteger(),
                (ParticipationObjectType) type.ToInteger(),
                SessionVariables.CurrentUser,
                SessionVariables.CurrentAccount,
                true,
                isParent: true,
                msgs: msgs);

            if (returnurl.IsNotBlank())
                return Redirect(returnurl);

            return RedirectToAction("Index");
        }
    }
}
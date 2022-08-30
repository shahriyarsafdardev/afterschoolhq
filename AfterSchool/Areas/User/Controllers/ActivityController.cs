using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using AfterSchool.Areas.User.Models;
using System.Linq;

namespace AfterSchool.Areas.User.Controllers
{
    public class ActivityController : BaseLoggedInController
    {
        public ActivityController() : base(new List<AccountType> { AccountType.Registrant, AccountType.Volunteer })
        {

        }

        // GET: User/Activity
        public ActionResult Index()
        {
            var msgs = new SystemMessages();
            var isVolunteer = SessionVariables.CurrentAccount.AccountTypeId == AccountType.Volunteer;
            var registrants = isVolunteer? SessionVariables.CurrentUser.GetVolunteerRegistrants(msgs) : SessionVariables.CurrentUser.GetStudentRegistrants(msgs);
            var studentActivities = new List<RegistrantActivityViewModel>();

            registrants.ForEach(registrant =>
            {
                var studentPOs = new List<ParticipationViewModel>();

                registrant.GetSubmittedRecords(msgs).ForEach(submittedRecord =>
                {
                    var ras = submittedRecord.GetRegistrantActivities(msgs)
                                .Where(ra => ra.ParticipationObjectTypeId != ParticipationObjectType.ActivityGroup)
                                .ToList();                    

                    ras.ForEach(ra => {
                        var participationObject = ra.GetParticipationObject(msgs);
                        var isPast = participationObject.IsOutOfSession(msgs) || ra.AssignmentTypeId == RegistrationAssignmentType.Withdrawn;

                        if (isPast)
                            studentPOs.Add(new ParticipationViewModel(participationObject, ra, submittedRecord, RegistrationStatus.Past));
                        else if (participationObject.IsInSession(msgs))
                            studentPOs.Add(new ParticipationViewModel(participationObject, ra, submittedRecord, RegistrationStatus.Current));
                        else if (participationObject.WillBeInSession(msgs))
                            studentPOs.Add(new ParticipationViewModel(participationObject, ra, submittedRecord, RegistrationStatus.Future));
                    });
                    
                });

                studentActivities.Add(new RegistrantActivityViewModel(registrant, studentPOs));
            });

            return View(studentActivities);
        }

        [HttpGet]
        public ActionResult Withdraw(string id, string po, string type, string returnurl)
        {
            var msgs = new SystemMessages();
            var registrant = Registrant.LoadByUid(id.ToGuid());

            registrant.WithdrawRegistration(
                participationObjectId: po.ToInteger(), 
                participationObjectType: (ParticipationObjectType)type.ToInteger(), 
                loggedInUser: SessionVariables.CurrentUser,
                account: SessionVariables.CurrentAccount,
                isCancelPayment: true,
                isParent: true,
                msgs: msgs);

            if (returnurl.IsNotBlank())
                return Redirect(returnurl);

            return RedirectToAction("Index");
        }
    }
}
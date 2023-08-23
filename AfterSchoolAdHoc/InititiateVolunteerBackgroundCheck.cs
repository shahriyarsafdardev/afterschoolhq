using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Services;

namespace AfterSchoolAdHoc;
internal class InititiateVolunteerBackgroundCheck
{
    public static void Run(SystemMessages msgs, DatabaseConnection dc)
    {
        var accountId = 6127; //196;
        var createdDate = DateTime.UtcNow.ToString("07/01/2023").ToDateTime();
        var volunteerRegistrants = Registrant.GetAllSelfRegisteredVolunteers(accountId, (int) RegistrantPersonType.Volunteer, 
            createdDate, false, msgs, dc);
        
        volunteerRegistrants.ForEach(registrant =>
        {
            var registration = registrant.GetRegistrations(msgs,dc).FirstOrDefault();
            var participationObject = registration.GetFirstParticipationObject(msgs, dc);
            var webUser = registrant.GetWebUser(msgs, dc);

            // if (registration.IsNotEmpty() && participationObject.IsNotEmpty() && participationObject.OwnerAccount.FiscalAccountId.IsNotEmpty()
            //     && !registration.ESignature.Equals("IMPORTED"))
            //     InitateBackgroundCheck(webUser, participationObject,msgs);
        });
    }
}
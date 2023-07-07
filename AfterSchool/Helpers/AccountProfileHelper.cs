using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;

namespace AfterSchool.Helpers
{
    public class AccountProfileHelper
    {
        public static string GetHeaderImageURL(ParticipationObject participationObject)
        {
            var msgs = new SystemMessages();
            var accountProfile = participationObject.OwnerAccount.GetAccountProfile(msgs);
            return accountProfile.GetHeaderImageUrl(msgs);
        }
    }
}
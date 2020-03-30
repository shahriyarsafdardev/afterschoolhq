using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;

namespace AfterSchool.Models
{
    public class RegistrationHeaderViewModel
    {
        public string Headline { get; set; }
        public string Title { get; set; }
        public string LogoUrl { get; set; }
        public string HeaderUrl { get; set; }
        public bool IsDemo { get; set; }
        public string SessionName { get; set; }
        public string PrimaryColor { get; set; }
        public Account Account { get; set; }
        public AccountProfile AccountProfile { get; set; }
        public ParticipationObject ParticipationObject { get; set; }

        public int Followers { get; set; }

        public RegistrationHeaderViewModel()
        {

        }

        public RegistrationHeaderViewModel(ParticipationObject participationObject, AccountProfile profile)
        {
            ParticipationObject = participationObject;
            Account = participationObject.OwnerAccount;
            AccountProfile = profile;

            HeaderUrl = profile?.GetHeaderImageUrl();
            LogoUrl = profile?.GetLogoUrl();
            Title = ParticipationObject.Name;
            Headline = ParticipationObject.Description;
            IsDemo = !Account.ID.HasValue;
            PrimaryColor = AccountProfile?.PrimaryColor;
            Followers = Account.GetSubscriberCount();
        }

        public RegistrationHeaderViewModel(ParticipationObject participationObject, bool IsDuringRegistration)
        {
            ParticipationObject = participationObject;
            Account = participationObject.OwnerAccount;
            AccountProfile = Account.GetAccountProfile();

            HeaderUrl = AccountProfile?.GetHeaderImageUrl();
            LogoUrl = AccountProfile?.GetLogoUrl();
            Title = ParticipationObject.Name;
            PrimaryColor = AccountProfile?.PrimaryColor;
            Followers = Account.GetSubscriberCount();

            if (IsDuringRegistration)
                Headline = "Registration";

            IsDemo = !Account.ID.HasValue;
        }

        public RegistrationHeaderViewModel(ParticipationObject participationObject, string title, string headline)
        {
            ParticipationObject = participationObject;
            Account = participationObject.OwnerAccount;
            AccountProfile = Account.GetAccountProfile();

            HeaderUrl = AccountProfile?.GetHeaderImageUrl();
            LogoUrl = AccountProfile?.GetLogoUrl();
            Title = title;
            Headline = headline;
            IsDemo = !Account.ID.HasValue;
            PrimaryColor = AccountProfile?.PrimaryColor;
            Followers = Account.GetSubscriberCount();
        }

        public RegistrationHeaderViewModel(Account account, AccountProfile profile)
        {
            Account = account;
            AccountProfile = profile;

            HeaderUrl = profile?.GetHeaderImageUrl();
            LogoUrl = profile?.GetLogoUrl();
            Title = account.Name;
            Headline = profile?.Headline;
            IsDemo = !account.ID.HasValue;
            PrimaryColor = profile?.PrimaryColor;
            Followers = Account.GetSubscriberCount();
        }

        public RegistrationHeaderViewModel(Account account, AccountProfile profile, ParticipationObject session)
        {
            ParticipationObject = session;
            Account = account;
            AccountProfile = profile;

            HeaderUrl = profile?.GetHeaderImageUrl();
            LogoUrl = profile?.GetLogoUrl();
            Title = account.Name;
            Headline = profile?.Headline;
            IsDemo = !account.ID.HasValue;
            PrimaryColor = profile?.PrimaryColor;
            Followers = Account.GetSubscriberCount();

            if (session.IsNotEmpty())
                SessionName = session.Name;

        }
    }
}
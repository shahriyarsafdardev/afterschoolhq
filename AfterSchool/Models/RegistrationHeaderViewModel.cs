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

        public RegistrationHeaderViewModel()
        {

        }

        public RegistrationHeaderViewModel(ParticipationObject participationObject, AccountProfile profile)
        {
            var account = participationObject.OwnerAccount;

            HeaderUrl = profile?.GetHeaderImageUrl();
            LogoUrl = profile?.GetLogoUrl();
            Title = participationObject.Name;
            Headline = participationObject.Description;
            IsDemo = !account.ID.HasValue;
            PrimaryColor = profile?.PrimaryColor;
        }

        public RegistrationHeaderViewModel(ParticipationObject participationObject, bool IsDuringRegistration)
        {
            var account = participationObject.OwnerAccount;
            var profile = account.GetAccountProfile();

            HeaderUrl = profile?.GetHeaderImageUrl();
            LogoUrl = profile?.GetLogoUrl();
            Title = participationObject.Name;
            PrimaryColor = profile?.PrimaryColor;

            if (IsDuringRegistration)
                Headline = "Registration";

            IsDemo = !account.ID.HasValue;
        }

        public RegistrationHeaderViewModel(ParticipationObject participationObject, string title, string headline)
        {
            var account = participationObject.OwnerAccount;
            var profile = account.GetAccountProfile();

            HeaderUrl = profile?.GetHeaderImageUrl();
            LogoUrl = profile?.GetLogoUrl();
            Title = title;
            Headline = headline;
            IsDemo = !account.ID.HasValue;
            PrimaryColor = profile?.PrimaryColor;
        }

        public RegistrationHeaderViewModel(Account account, AccountProfile profile)
        {
            HeaderUrl = profile?.GetHeaderImageUrl();
            LogoUrl = profile?.GetLogoUrl();
            Title = account.Name;
            Headline = profile?.Headline;
            IsDemo = !account.ID.HasValue;
            PrimaryColor = profile?.PrimaryColor;
        }

        public RegistrationHeaderViewModel(Account account, AccountProfile profile, ParticipationObject session)
        {
            HeaderUrl = profile?.GetHeaderImageUrl();
            LogoUrl = profile?.GetLogoUrl();
            Title = account.Name;
            Headline = profile?.Headline;
            IsDemo = !account.ID.HasValue;
            PrimaryColor = profile?.PrimaryColor;

            if (session.IsNotEmpty())
                SessionName = session.Name;

        }

        public RegistrationHeaderViewModel(string title, string headline, string logoUrl, string headerUrl, string sessionName, string primaryColor, bool isDemo)
        {
            Headline = title;
            Title = headline;
            LogoUrl = logoUrl;
            HeaderUrl = headerUrl;
            IsDemo = isDemo;
            SessionName = sessionName;
            PrimaryColor = primaryColor;
        }

      
    }
}
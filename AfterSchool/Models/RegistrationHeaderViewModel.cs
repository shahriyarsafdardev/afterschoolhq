using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;

namespace AfterSchool.Models
{
    public class RegistrationHeaderViewModel
    {
        public string Headline { get; set; }
        public string Title { get; set; }
        public string LogoUrl { get; set; }
        public string HeaderUrl { get; set; }

        public RegistrationHeaderViewModel()
        {

        }

        public RegistrationHeaderViewModel(AccountProfile accountProfile)
        {
            Headline = accountProfile.Headline;
            Title = accountProfile.GetAccount().Name;
            LogoUrl = accountProfile.GetLogoUrl();
            HeaderUrl = accountProfile.GetHeaderImageUrl();
        }

        public RegistrationHeaderViewModel(Account account)
        {
            var profile = account.GetAccountProfile();
            Headline = profile.Headline;
            Title = account.Name;
            LogoUrl = profile.GetLogoUrl();
            HeaderUrl = profile.GetHeaderImageUrl();
        }

        public RegistrationHeaderViewModel(string title, string headline, string logoUrl, string headerUrl)
        {
            Headline = title;
            Title = headline;
            LogoUrl = logoUrl;
            HeaderUrl = headerUrl;
        }
    }
}
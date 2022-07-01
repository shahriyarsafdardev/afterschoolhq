using System.Collections.Generic;
using System.Linq;
using AfterSchoolLibrary.Models;
using BasicLibrary;

namespace AfterSchool.Assemblers
{
    public class BrowseResult
    {
        public Account Account { get; set; }
        public AccountProfile AccountProfile { get; set; }
        public bool CleverSsoEnabled { get; set; }
        public List<Account> FeaturedProviders { get; set; }
        public List<GradeLevel> Grades { get; set; }
        public List<Location> Locations { get; set; }
        public List<ParticipationObject> RegistrationClosed { set; get; }
        public List<ParticipationObject> RegistrationOpened { get; set; }
        public List<ParticipationObject> RegistrationWillOpen { get; set; }
        public ParticipationObject Session { get; set; }
        public SystemMessages Messages { get; set; }
    }

    public class BrowseAssembler
    {
        public BrowseResult AssembleBrowseResult(Account account, AccountProfile accountProfile, bool cleverSsoEnabled, List<Account> featuredProviders,
            List<GradeLevel> grades, List<Location> locations, List<ParticipationObject> registrationClosed, List<ParticipationObject> registrationOpened,
            List<ParticipationObject> registrationWillOpen, ParticipationObject session, SystemMessages messages)
        {
            return new BrowseResult
            {
                Account = account,
                AccountProfile = accountProfile,
                CleverSsoEnabled = cleverSsoEnabled,
                FeaturedProviders = featuredProviders,
                Grades = grades,
                Locations = locations.DistinctBy(location => location.ID).ToList(),
                RegistrationClosed = registrationClosed,
                RegistrationOpened = registrationOpened,
                RegistrationWillOpen = registrationWillOpen,
                Session = session,
                Messages = messages,
            };
        }
    }
}
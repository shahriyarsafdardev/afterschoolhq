using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class SeasonFactory
    {
        public static Season Create(ActivityGroup League, int ByesPerTeam, ActivitySchedule Rounds, SystemMessages msgs, DatabaseConnection dc)
        {
            //make new season and add all necciary
            var season = new Season() { ActivityGroup = League, RoundCount = Rounds.GetInstances(false, msgs, dc).Count, totalByes = ByesPerTeam};
            var season2 = season.GenerateSeason(Rounds, League, ByesPerTeam, msgs, dc);
            return season2;
        }
    }
}

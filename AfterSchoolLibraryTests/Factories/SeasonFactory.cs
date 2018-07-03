using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;

namespace AfterSchoolLibraryTests.Factories
{
    public class SeasonFactory
    {
        
        public static Season Create(ActivityGroup League, int ByesPerTeam, ActivitySchedule Rounds, SystemMessages msgs, DatabaseConnection dc)
        {
            //make new season and add all necciary feilds
            var season = new Season() { ActivityGroup = League, RoundCount = Rounds.GetInstances(false, dc).Count };
            var season2 = season.GenerateSeason(Rounds, League, ByesPerTeam, dc);
            return season2;
        }
        
    }
}

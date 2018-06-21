using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfterSchoolLibrary
{

    [Serializable]
    public class SeasonSheduler
    {
        // The league
        public ActivityGroup ActivityGroup { get; set; }
        // The teams
        private List<Activity> Activities { get; set; }
        // 
        private List<ActivityAndSeatCount> ActivityAndSeatCounts { get; set; }
        private List<Registrant> Registrants { get; set; }

        public Season GenerateShedule(ActivitySchedule shedule, ActivityGroup Leauge, DatabaseConnection dc)
        {
            return new Season();
        }

        /*
        public bool HasPlayed(this Activity, Activity team)
        {
            // need a has played feild in Activity
            return true;
        }
        */

    }
    public class Season //just a list of a list of matches
    {
        public int HomeTeamID { get; set; }
        public int AwayTeamID { get; set; }
        public int LocationID { get; set; }
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public int ActivityInstanceID { get; set; }

        /*
        public List<Match> NewRound(ActivityGroup League, bool isBye, DatabaseConnection dc)
        {
            var teams = League.GetActivities(dc);
            teams.Shuffle();
            if (isBye)
            {
                GiveBye(teams);
            }



            for (int i = 0;  1< teams.Count - 1; i++)
            {
                var team = teams[i];
                if(teams[i+1]. have not played){
                    if(team. home games > team . away games){
                        if( teams[i+1]. HomePageUrl games)
                    }

                }
            }
            

            // Have I played the next member of the list?

            // home>away? then play away find someone who home<away

            //else -> defaults to playing at home

            return ;


        }
        */

        public void GiveBye(List<Activity> Teams)
        {
            foreach (Activity team in Teams)
            {
                /*
                if (!team had bye){
                    var t = team;
                    Teams.Remove(team);
                    return;
                }
                */
            }
            return;
        }
    }
}


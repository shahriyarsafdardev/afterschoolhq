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
        public KeyValuePair<int, int> totalGamesCount { get; set; }
        public KeyValuePair<int, int> byes { get; set; }
        public KeyValuePair<int, int> homeGamesCount { get; set; }


        public class Season
        {
            // The league
            public ActivityGroup ActivityGroup { get; set; }
            // The teams
            private List<Activity> Activities { get; set; }
            public int totalGamesCount { get; set; }
            public int rounds { get; set; }
            public int byesPerTeam { get; set; }

            public int getFraction()
            {
                return Math.Ceiling(byesPerTeam.ToDecimal() / rounds.ToDecimal()).ToInteger();
            }

           public List<int> MiddleOutSort (int rounds)
            {
                var frac = Math.Ceiling(byesPerTeam.ToDecimal() / rounds.ToDecimal()).ToInteger();
                var mid = Math.Ceiling(frac / 2.ToDecimal()).ToInteger();
                var list = new List<int>();
                for (int i = 0; i < frac; i++)
                {
                    if(i%2 == 0)
                    {
                        list.Add(mid - i);
                    }
                    if(i%2 == 1)
                    {
                        list.Add(mid + i);
                    }
                }
                return list;
            }

            public List<int> WeeklyByeCountCalculator (int fraction, int teams)
            {
                // int weeksOfHeavyByes = fraction - (fraction % teams);
                // var HeavybyeGamesCount = Math.Ceiling(teams / fraction.ToDecimal()).ToInteger();

                var list = new List<int> ();

                //round up to the next even number in order to handle an extream nuber of byes in a short timeframe
                while(teams != 0 && fraction != 0)
                {
                    var WeeklyByes = Math.Ceiling(teams / fraction.ToDecimal()).ToInteger();
                    if(WeeklyByes % 2 != 0)
                    {
                        WeeklyByes++;
                    }
                    list.Add(WeeklyByes);
                    teams = teams - WeeklyByes;
                    fraction--;
                }
                return list;
            }


            public Season GenerateShedule(ActivitySchedule shedule, ActivityGroup Leauge, int numberOfByes, DatabaseConnection dc)
            {
                for( int i = 0; i < rounds; i++)
                {
                    Round.NewRound(ActivityGroup,)
                }
                return new Season();
            }
        }


        public class Round //just a list of Matches
        {
            // times of each round
            public int ActivityInstanceID { get; set; }
            //a list of matches for this round
            public List<TeamMatch>

            public static List<TeamMatch> NewRound(ActivityGroup League, int byes, DatabaseConnection dc)
            {
                var teams = League.GetActivities(dc);
                teams.Shuffle();

                // will remove a team from teams if they have not had a bye yet 
                for( int i = 0; i < byes; i++)
                {
                    GiveBye(teams);
                }

                var round = new List<TeamMatch>();

                for (int i = 0; i < teams.Count - 1; i++)
                {
                    for (int k = 1; k < teams.Count - 1; i++)
                    {
                        // if all teams have played, reset

                        var team = teams[i];
                        if (teams[i + 1].have not played){
                            if (team.home games > team.away games){
                                if (teams[i + k].games) {
                            new TeamMatch() { HomeTeamId = teams[i].AccountId, AwayTeamId= teams[i + k].AccountId };
                                }
                            }
                        }

                return round;

            }
            public void GiveBye(List<Activity> Teams)
            {
                foreach (Activity team in Teams)
                {
                    
                    if (!team.had bye){
                        Teams.Remove(team);
                        return;
                    }

                }
                return;
            }
        }

    }



using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfterSchoolLibrary
{

        public class Season
        {
            // The league
            public ActivityGroup ActivityGroup { get; set; }
            // The teams
            private List<Activity> Activities { get; set; }
            public int totalByes { get; set; }
            public int RoundCount { get; set; }
            public List<KeyValuePair<int, int>> byesPerTeam { get; set; }
            public List<KeyValuePair<int, int>> HomeCountPerTeam { get; set; }
            public List<KeyValuePair<int, int>> AwayCountPerTeam { get; set; }
            public List<List<TeamMatch>> Rounds { get; set; }


        public Season GenerateSeason(ActivitySchedule schedule, ActivityGroup league, int ByeCountPerTeam, SystemMessages msgs, DatabaseConnection dc)
        {
            var roundDates = schedule.GetInstances(false, msgs, dc);
            var season = new Season() { ActivityGroup = league, byesPerTeam = new List<KeyValuePair<int, int>>() };
            var ByesInWeeks = WeeklyByeCountCalculator(roundDates.Count, league.GetActivityCount(ActivityType.Team, null, null, null, msgs, dc) * ByeCountPerTeam);
            var datesForByes = MiddleOutSort(roundDates.Count);
            var teams = league.GetActivities(msgs, dc);
            var byeCountPerWeekOrderedList = new List<KeyValuePair<int, int>>();
            season.Rounds = new List<List<TeamMatch>>();
            this.Rounds = new List<List<TeamMatch>>();

            //initializes the bye counter
            foreach (Activity team in teams)
            {
                season.byesPerTeam.Add(new KeyValuePair<int, int>(0, team.ID.Value));
            }

            this.byesPerTeam = season.byesPerTeam;

            if (ByeCountPerTeam != 0)
            {
                // divides the shedule so that every team has had a bye in the given fraction of the season
                var fraction = roundDates.Count / ByeCountPerTeam;
                //calculates the number of byes per week in decending order
                var ByeCountPerWeekUnordered = WeeklyByeCountCalculator(fraction, teams.Count);
                // orders the weeks from the middle out
                var Coorespondingweeks = MiddleOutSort(roundDates.Count);
                // the week is the key, the number of byes is the value

                for (int i = 0; i < Coorespondingweeks.Count; i++)
                {
                    var ByeCountPerWeekOrdered = new KeyValuePair<int, int>(Coorespondingweeks[i], ByeCountPerWeekUnordered[i]);
                    byeCountPerWeekOrderedList.Add(ByeCountPerWeekOrdered);
                }

            }


            for (int i = 0; i < roundDates.Count; i++)
            {
                {
                    var newRound = Round.NewRound(this, teams, roundDates[i], byeCountPerWeekOrderedList[i].Value, dc);
                    season.Rounds.Add(newRound.Matches);
                }

            }

            return season;
        }

            public List<int> MiddleOutSort(int rounds)
            {
                var frac = Math.Ceiling(RoundCount.ToDecimal()).ToInteger() / totalByes.ToDecimal();
                var mid = Math.Ceiling(frac / 2.ToDecimal()).ToInteger();
                if(mid % 2 == 0)
            {
                mid++;
            }
                var list = new List<int>();
                if(frac % 2 == 1)
            {
                list.Add(mid);
                for (int i = 1; i < frac - mid + 1; i++)
                {
                    list.Add(mid + i);
                    list.Add(mid - i);
                }
            }

            else
            {
                list.Add(mid);
                for (int i = 1; i < frac - mid + 1; i++)
                {
                    list.Add(mid - i);
                    list.Add(mid + i);
                    var a = i;
                }
                list.Add(1);

            }
                return list;
            }

            public List<int> WeeklyByeCountCalculator(int fraction, int teams)
            {
                var byeList = new List<int>();

                if (teams % 2 == 0)
                {
                    //round up to the next even number in order to handle an extream number of byes in a short timeframe
                    while (teams != 0 && fraction != 0)
                    {
                        var WeeklyByes = Math.Ceiling(teams / fraction.ToDecimal()).ToInteger();
                        if (WeeklyByes % 2 != 0)
                        {
                            WeeklyByes++;
                        }
                        byeList.Add(WeeklyByes);
                        teams = teams - WeeklyByes;
                        fraction--;
                    }
                    return byeList;
                }
                if (teams % 2 != 0)
                {
                    if (teams <= fraction)
                    {
                        while (teams != 0)
                        {
                            byeList.Add(1);
                            teams--;
                        }
                    }
                    if (teams > fraction)
                    {
                        while (teams != 0 && fraction != 0)
                        {
                            var WeeklyByes = Math.Ceiling(teams / fraction.ToDecimal()).ToInteger();
                            byeList.Add(WeeklyByes);
                            teams = teams - WeeklyByes;

                        }
                    }
                }
                return byeList;
            }

        public class Round //just a list of Matches
        {
            // Number of byes in a round
            public int ByeCount { get; set; }
            // times of each round
            public int ActivityInstanceID { get; set; }
            //a list of matches for this round
            public List<TeamMatch> Matches { get; set; }
            public List<Activity> Teams { get; set; }

            public static Round NewRound(Season season, List<Activity> Teams, ActivityInstance activityInstance, int byes, DatabaseConnection dc)
            {
                Teams.Shuffle();
                var round = new Round() { ByeCount = byes, Teams = Teams, ActivityInstanceID = activityInstance.ID.Value  };
                // will remove a team from teams if they have not had a bye yet 

                List<Activity> byeteams =  new List<Activity>() { };

                for (int i = 0; i < byes; i++)
                {
                    if (Teams.Count % 2 == 1)
                    {
                        var byeTeam = GiveBye(Teams, season);
                        byeteams.Add(byeTeam);
                    }
                    else
                    {
                        var byeTeam1 = GiveBye(Teams, season);
                        var byeTeam2 = GiveBye(Teams, season);
                        byeteams.Add(byeTeam1);
                        byeteams.Add(byeTeam2);

                    }
                }

                round.Matches = new List<TeamMatch>();

                for (int i = 0; i < (Teams.Count / 2); i++)
                {
                    int k = i + 1; 
                    while ( k < Teams.Count)
                    {
                        if(round.Matches.Count == Teams.Count / 2)
                        {
                            break;
                        }
                        // if all teams have played, reset

                        var team = Teams[i];
                        var opponent = Teams[k];

                        var x = TeamMatch.LoadByHomeAndAwayID(team.ID.Value, opponent.ID.Value, null, dc);
                        //checks if the teams have played eachother
                        if (TeamMatch.LoadByHomeAndAwayID(team.ID.Value, opponent.ID.Value,null,dc).Count() == 0)
                        {
                            //checks to see if team should play at home or away
                            if (TeamMatch.LoadByHomeID(team.ID.Value, null, dc).Count <= TeamMatch.LoadByAwayID(team.ID.Value, null, dc).Count)
                            {
                                var match = new TeamMatch() { HomeTeamId = Teams[i].AccountId, AwayTeamId = Teams[i + k].AccountId };
                                round.Matches.Add(match);
                                match.Save(dc);
                            }
                            else
                            {
                                var match = new TeamMatch() { AwayTeamId = Teams[i].AccountId, HomeTeamId = Teams[i + k].AccountId };
                                match.Save(dc);
                                round.Matches.Add(match);
                            }
                        }
                        k++;
                    }
                }
                season.Rounds.Add(round.Matches);

                // adds back the teams that were removed to give a bye
                foreach(Activity team in byeteams)
                {
                    Teams.Add(team);
                }
                return round;
            }
            public static Activity GiveBye(List<Activity> Teams, Season season)
            {
                foreach (Activity team in Teams)
                {
                    var id = team.ID.Value;
                    foreach (KeyValuePair<int, int> pair in season.byesPerTeam)
                    {
                        if (pair.Value == id)
                        {
                            if (pair.Key == 0)
                            {
                                var newByeCount = new KeyValuePair<int, int>(pair.Key + 1, pair.Value);
                                season.byesPerTeam.Remove(pair);
                                Teams.Remove(team);
                                return team;
                            }
                        }
                    }

                }
                return null;
            }
        }

    }
}



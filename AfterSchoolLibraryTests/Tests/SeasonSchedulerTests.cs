using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibraryTests
{
    public class SeasonShedulerTest : BaseTest
    {
        public SeasonShedulerTest(ITestOutputHelper output) : base(output)
        {
        }


        [Fact]
        public void MiddleOutSortOddTest()
        {
            var season = new Season() { };
            season.totalByes = 1;
            season.RoundCount = 5;
            var List = new List<int>();
            List.Add(3);
            List.Add(4);
            List.Add(2);
            List.Add(5);
            List.Add(1);
            Assert.Equal(List, season.MiddleOutSort(5));
        }

        [Fact]
        public void MiddleOutSortEvenTest()
        {
            var season = new Season() { };
            season.totalByes = 1;
            season.RoundCount = 4;
            var List = new List<int>();
            List.Add(3);
            List.Add(2);
            List.Add(4);
            List.Add(1);
            Assert.Equal(List, season.MiddleOutSort(4));

        }

        [Fact]
        public void WeeklyByeCountCalculatorTest()
        {
            var season = new Season() { };
            var List = new List<int>();
            List.Add(1);
            List.Add(1);
            List.Add(1);
            Assert.Equal(List, season.WeeklyByeCountCalculator(4, 3));
        }

        [Fact]
        public void WeeklyByeCountCalculatorTest2()
        {
            var season = new Season() { };
            var List = new List<int>();
            List.Add(4);
            List.Add(4);
            List.Add(2);
            Assert.Equal(List, season.WeeklyByeCountCalculator(3, 10));
        }

        // 3 teams one bye means that one match will be played
        [Fact]
        public void NewRoundTest()
        {

            var season = new Season() {byesPerTeam = new List<KeyValuePair<int, int>>()};
            var teams = new List<Activity>();
            var webUser = Factories.WebUserFactory.Create(true, msgs, dc);
            var account = Factories.AccountFactory.Create(webUser, msgs, dc);
            var League = Factories.ActivityGroupFactory.Create(account, ActivityGroupType.League, msgs, dc);
            var rounds = Factories.ActivityScheduleFactory.Create(account, msgs, dc);


            for(int i = 0; i < 4; i++)
            {
                var team = Factories.ActivityFactory.Create(account, League, msgs, dc);
                teams.Add(team);
                season.byesPerTeam.Add(new KeyValuePair<int, int>(team.ID.Value, 0));
                var shedule = Factories.ActivityInstanceFactory.Create(team,rounds, msgs, dc);
                var start = new DateTime(1, 1, 1);
                var end = new DateTime(2, 2, 2);
                rounds.AddInstance(start, end, webUser.ID.Value, msgs, dc);
            }

            var roundInstances = rounds.GetInstances(false, msgs, dc);
            var round = roundInstances[0];

            Assert.Single(Season.Round.NewRound(season, teams, round, 1, dc).Matches);
        }

        [Fact]
        public void NewSeasonTest()
        {
            var teams = new List<Activity>();
            var webUser = Factories.WebUserFactory.Create(true, msgs, dc);
            var account = Factories.AccountFactory.Create(webUser, msgs, dc);
            var League = Factories.ActivityGroupFactory.Create(account, ActivityGroupType.League, msgs, dc);
            var rounds = Factories.ActivityScheduleFactory.Create(account, msgs, dc);
            var byesPerTeam = 1;

            //sets up the shedule
            for(int i = 0; i < 5; i++)
            {
                var start = new DateTime(1, 1, 1);
                var end = new DateTime(2, 2, 2);
                rounds.AddInstance(start, end, webUser.ID.Value, msgs, dc);
            }
            // adds teams to the shedule
            for (int i = 0; i < 5; i++)
            {
                var team = Factories.ActivityFactory.Create(account, League, msgs, dc);
                var shedule = Factories.ActivityInstanceFactory.Create(team, rounds, msgs, dc);
            }

            var season = Factories.SeasonFactory.Create(League, byesPerTeam, rounds, msgs, dc);
            var byeCheck = season.byesPerTeam;
            var homeCheck = season.HomeCountPerTeam;
            var awayCheck = season.AwayCountPerTeam;

            Assert.Equal(5, season.Rounds.Count);
        }

    }
}

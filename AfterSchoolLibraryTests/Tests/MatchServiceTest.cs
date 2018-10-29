using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;
using AfterSchoolLibraryTests.Factories;
using System.Linq;
using System.Collections.Generic;

namespace AfterSchoolLibraryTests.Tests
{
    public class MatchServiceTest : BaseTest
    {
        private WebUser AccountOwner { get; set; }
        private Account HostAccount { get; set; }

        public MatchServiceTest(ITestOutputHelper output) : base(output)
        {
            AccountOwner = WebUserFactory.Create(msgs, dc);
            HostAccount = AccountFactory.Create(AccountOwner, msgs, dc);
        }

        [Fact]
        public void CreateOrUpdateTeamMissingTest()
        {
            var visitorAccountUser = WebUserFactory.Create(msgs, dc);
            var visitorAccount = AccountFactory.Create(visitorAccountUser, msgs, dc);
            var league = ActivityGroupFactory.CreateWithSchedule(HostAccount, ActivityGroupType.League, msgs, dc);
            var team = ActivityFactory.Create(visitorAccount, league, msgs, dc);
            league.Reload();
            var dateRange = new DateRange(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
            var schedule = league.GetSchedule(msgs: msgs, dc: dc);
            var activityInstance = league.GetSchedule().AddInstance(dateRange, InstanceType.InSession, AccountOwner.ID.Value, msgs, dc);

            var match = MatchFactory.Create(
                    HostAccount,
                    AccountOwner,
                    msgs,
                    dc,
                    league: league,
                    team: team,
                    activityInstance: activityInstance);

            Assert.True(Match.Load(match.ID.Value, msgs, dc).IsNotEmpty());
            Assert.True(Match.Load(match.ID.Value, msgs, dc).ID.Value == match.ID.Value);
        }

       [Theory]
       [InlineData(5)]
       public void LoadByTeamIdTest(int numberToCreate)
        {
            var visitorAccountUser = WebUserFactory.Create(msgs, dc);
            var visitorAccount = AccountFactory.Create(visitorAccountUser, msgs, dc);
            var league = ActivityGroupFactory.CreateWithSchedule(HostAccount, ActivityGroupType.League, msgs, dc);
            var team = ActivityFactory.Create(visitorAccount, league, msgs, dc);
            var matches = new List<Match>();

            for (int i = 0; i < numberToCreate; i++)
            {
                matches.Add(MatchFactory.Create(
                    HostAccount,
                    AccountOwner,
                    msgs,
                    dc,
                    league: league,
                    team: team,
                    activityInstance: null));
            }

            var matchesIds = matches.Select(m => m.ID.Value);
            var loadedMatches = Match.LoadByTeamId(team.ID.Value, msgs, dc).ToList();
            var loadedMatchesIds = loadedMatches.Select(m => m.ID.Value);

            Assert.True(matches.Count == 5);
            Assert.True(loadedMatches.Count == 5);
            Assert.True(loadedMatchesIds.SequenceEqual(matchesIds));
        }

        [Theory]
        [InlineData(5)]
        public void LoadByActivityInstanceIdTest(int numberToCreate)
        {
            var visitorAccountUser = WebUserFactory.Create(msgs, dc);
            var visitorAccount = AccountFactory.Create(visitorAccountUser, msgs, dc);
            var league = ActivityGroupFactory.CreateWithSchedule(HostAccount, ActivityGroupType.League, msgs, dc);
            league.Reload();
            var dateRange = new DateRange(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
            var schedule = league.GetSchedule(msgs:msgs, dc: dc);
            var activityInstance = league.GetSchedule().AddInstance(dateRange, InstanceType.InSession, AccountOwner.ID.Value, msgs, dc);
            var matches = new List<Match>();

            for (int i = 0; i < numberToCreate; i++)
            {
                matches.Add(MatchFactory.Create(
                    HostAccount, 
                    AccountOwner,
                    msgs, 
                    dc, 
                    league: league,
                    team: null, 
                    activityInstance: activityInstance));
            }

            var matchesIds = matches.Select(m => m.ID.Value);
            var loadedMatches = Match.LoadByActivityInstanceId(activityInstance.ID.Value, msgs, dc).ToList();
            var loadedMatchesIds = loadedMatches.Select(m => m.ID.Value);

            Assert.True(matches.Count == 5);
            Assert.True(loadedMatches.Count == 5);
            Assert.True(loadedMatchesIds.SequenceEqual(matchesIds));
        }

        [Fact]
        public void LoadByTeamIdByActivityInstanceIdTest()
        {
            var visitorAccountUser = WebUserFactory.Create(msgs, dc);
            var visitorAccount = AccountFactory.Create(visitorAccountUser, msgs, dc);
            var league = ActivityGroupFactory.CreateWithSchedule(HostAccount, ActivityGroupType.League, msgs, dc);
            var team = ActivityFactory.Create(visitorAccount, league, msgs, dc);
            league.Reload();
            var dateRange = new DateRange(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
            var schedule = league.GetSchedule(msgs: msgs, dc: dc);
            var activityInstance = league.GetSchedule().AddInstance(dateRange, InstanceType.InSession, AccountOwner.ID.Value, msgs, dc);

            var match = MatchFactory.Create(
                    HostAccount,
                    AccountOwner,
                    msgs,
                    dc,
                    league: league,
                    team: team,
                    activityInstance: activityInstance);

            var loadedMatch = Match.LoadByTeamIdByActivityInstanceId(team.ID.Value, activityInstance.ID.Value, msgs, dc);

            Assert.True(loadedMatch.ID.Value == match.ID.Value);
        }

        [Fact]
        public void ExistsTest()
        {
            var visitorAccountUser = WebUserFactory.Create(msgs, dc);
            var visitorAccount = AccountFactory.Create(visitorAccountUser, msgs, dc);
            var league = ActivityGroupFactory.CreateWithSchedule(HostAccount, ActivityGroupType.League, msgs, dc);
            var team = ActivityFactory.Create(visitorAccount, league, msgs, dc);
            league.Reload();
            var dateRange = new DateRange(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
            var schedule = league.GetSchedule(msgs: msgs, dc: dc);
            var activityInstance = league.GetSchedule().AddInstance(dateRange, InstanceType.InSession, AccountOwner.ID.Value, msgs, dc);

            var match = MatchFactory.Create(
                    HostAccount,
                    AccountOwner,
                    msgs,
                    dc,
                    league: league,
                    team: team,
                    activityInstance: activityInstance);

            Assert.True(Match.Exists(team.ID.Value, activityInstance.ID.Value, msgs, dc));
        }

    }
}

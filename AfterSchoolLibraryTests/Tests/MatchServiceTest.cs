using System;
using Xunit;
using Xunit.Abstractions;
using AfterSchoolLibrary;
using BasicLibrary;
using AfterSchoolLibraryTests.Factories;
using System.Linq;
using System.Collections.Generic;
using AfterSchoolLibrary.Services;

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
        public void CreateOrUpdateMatchMissingTeamTest()
        {
            var errorMessage = "Error Saving Match, Missing Team on Match";
            var visitorAccountUser = WebUserFactory.Create(msgs, dc);
            var visitorAccount = AccountFactory.Create(visitorAccountUser, msgs, dc);
            var league = ActivityGroupFactory.CreateWithSchedule(HostAccount, ActivityGroupType.League, msgs, dc);
            var team = ActivityFactory.Create(visitorAccount, league, msgs, dc);
            league.Reload();
            var dateRange = new DateRange(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
            var schedule = league.GetSchedule(msgs: msgs, dc: dc);
            var activityInstance = league.GetSchedule().AddInstance(dateRange, InstanceType.InSession, AccountOwner.ID.Value, msgs, dc);

            var match = MatchService.CreateOrUpdate(
                                      currentUser: AccountOwner,
                                      teamId: null,
                                      activityInstanceId: activityInstance.ID.Value,
                                      match: null,
                                      score: 0,
                                      teamPlace: 0,
                                      msgs: msgs,
                                      dc: dc
                                      );
            
            Assert.True(match.IsEmpty());
            Assert.True(msgs.HasMessageContainingText(errorMessage.ToLower()));
        }

        [Fact]
        public void CreateOrUpdateMatchMissingActivityInstanceTest()
        {
            var errorMessage = "Error Saving Match, Missing ActivityInstance";
            var visitorAccountUser = WebUserFactory.Create(msgs, dc);
            var visitorAccount = AccountFactory.Create(visitorAccountUser, msgs, dc);
            var league = ActivityGroupFactory.CreateWithSchedule(HostAccount, ActivityGroupType.League, msgs, dc);
            var team = ActivityFactory.Create(visitorAccount, league, msgs, dc);
            league.Reload();
            var dateRange = new DateRange(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
            var schedule = league.GetSchedule(msgs: msgs, dc: dc);
            var activityInstance = league.GetSchedule().AddInstance(dateRange, InstanceType.InSession, AccountOwner.ID.Value, msgs, dc);

            var match = MatchService.CreateOrUpdate(
                                      currentUser: AccountOwner,
                                      teamId: team.ID.Value,
                                      activityInstanceId: null,
                                      match: null,
                                      score: 0,
                                      teamPlace: 0,
                                      msgs: msgs,
                                      dc: dc
                                      );

            Assert.True(match.IsEmpty());
            Assert.True(msgs.HasMessageContainingText(errorMessage.ToLower()));
        }

        [Fact]
        public void CreateOrUpdateMatchInvalidActivityInstanceTest()
        {
            var errorMessage = "Error Saving Match, Invalid ActivityInstance";
            var visitorAccountUser = WebUserFactory.Create(msgs, dc);
            var visitorAccount = AccountFactory.Create(visitorAccountUser, msgs, dc);
            var league = ActivityGroupFactory.CreateWithSchedule(HostAccount, ActivityGroupType.League, msgs, dc);
            var team = ActivityFactory.Create(visitorAccount, league, msgs, dc);
            league.Reload();
            var dateRange = new DateRange(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
            var schedule = league.GetSchedule(msgs: msgs, dc: dc);
            var activityInstance = league.GetSchedule().AddInstance(dateRange, InstanceType.InSession, AccountOwner.ID.Value, msgs, dc);

            var match = MatchService.CreateOrUpdate(
                                      currentUser: AccountOwner,
                                      teamId: team.ID.Value,
                                      activityInstanceId: -1,
                                      match: null,
                                      score: 0,
                                      teamPlace: 0,
                                      msgs: msgs,
                                      dc: dc
                                      );

            Assert.True(match.IsEmpty());
            Assert.True(msgs.HasMessageContainingText(errorMessage.ToLower()));
        }

        [Fact]
        public void CreateOrUpdateMatchTeamHasMatchActivityInstanceTest()
        {
            var errorMessage = "Error Saving Match, team already has a match with this activity instance";
            var visitorAccountUser = WebUserFactory.Create(msgs, dc);
            var visitorAccount = AccountFactory.Create(visitorAccountUser, msgs, dc);
            var league = ActivityGroupFactory.CreateWithSchedule(HostAccount, ActivityGroupType.League, msgs, dc);
            var team = ActivityFactory.Create(visitorAccount, league, msgs, dc);
            league.Reload();
            var dateRange = new DateRange(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
            var schedule = league.GetSchedule(msgs: msgs, dc: dc);
            var activityInstance = league.GetSchedule().AddInstance(dateRange, InstanceType.InSession, AccountOwner.ID.Value, msgs, dc);

            var match = MatchService.CreateOrUpdate(
                                      currentUser: AccountOwner,
                                      teamId: team.ID.Value,
                                      activityInstanceId: activityInstance.ID.Value,
                                      match: null,
                                      score: 0,
                                      teamPlace: 0,
                                      msgs: msgs,
                                      dc: dc
                                      );

            var match2 = MatchService.CreateOrUpdate(
                                      currentUser: AccountOwner,
                                      teamId: team.ID.Value,
                                      activityInstanceId: activityInstance.ID.Value,
                                      match: null,
                                      score: 0,
                                      teamPlace: 0,
                                      msgs: msgs,
                                      dc: dc
                                      );

            Assert.True(match2.IsEmpty());
            Assert.True(msgs.HasMessageContainingText(errorMessage.ToLower()));
        }
    }
}

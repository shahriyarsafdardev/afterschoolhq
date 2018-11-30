using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using System.Linq;
using System;

namespace AfterSchoolLibraryTests.Factories
{
    public class MatchFactory
    {
        public static Match Create(WebUser user, Activity team, ActivityInstance activityInstance, SystemMessages msgs, DatabaseConnection dc, Match exisitingMatch = null, int score = 0, int teamPlace = 0)
        {
           var match = MatchService.CreateOrUpdate(
                                      currentUser: user,
                                      teamId: team.ID.Value,
                                      activityInstanceId: activityInstance.ID.Value,
                                      match: exisitingMatch,
                                      score: score,
                                      teamPlace: teamPlace,
                                      msgs: msgs,
                                      dc: dc
                                      );
          
            msgs.AddRange(match.Save(dc));
            return match;
        }

        public static Match Create(Account hostAccount, WebUser accountOwner, SystemMessages msgs, DatabaseConnection dc, ActivityGroup league = null, Activity team = null, ActivityInstance activityInstance = null)
        {
            var vistorAccountUser = WebUserFactory.Create(msgs, dc);
            var visitorAccount = AccountFactory.Create(vistorAccountUser, msgs, dc);
            
            if (league.IsEmpty())
            {
                var gradeLevels = GradeLevel.Load(msgs: msgs, dc: dc);
                var schedule = ActivityScheduleFactory.Create(hostAccount, msgs, dc);
                league = ActivityGroupFactory.Create(
                    hostAccount,
                    ActivityGroupType.League,
                    schedule,
                    GenderRestrictionType.None,
                    gradeLevels.First().ID,
                    gradeLevels.Last().ID,
                    35,
                    CostUnitType.PerActivity,
                    true,
                    msgs,
                    dc);
            }

            var participationObject = league.ToParticipationObject(msgs, dc);

            if(team.IsEmpty())
                team = ActivityFactory.Create(visitorAccount, league, msgs, dc);

            if (activityInstance.IsEmpty())
            {
                var dateRange = new DateRange(DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
                activityInstance = league.GetSchedule().AddInstance(dateRange, InstanceType.InSession, accountOwner.ID.Value, msgs, dc);
            }
            

            return MatchFactory.Create(vistorAccountUser, team, activityInstance, msgs, dc); 
        }
    }
}

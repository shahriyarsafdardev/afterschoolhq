using System;
using System.Collections.Generic;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Services
{
    public static class MatchService
    {

        public static Match CreateOrUpdate(WebUser currentUser, int teamId, int activityInstanceId, Match match = null, decimal score = 0, int teamPlace = 0, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
           
            if (teamId.IsEmpty() )
                msgs.Add("Missing Team on Match", MessageType.Error);

            // vactivityInstance
            if (activityInstanceId.IsEmpty())
                msgs.Add("Error Saving Match, invalid activityInstance ", MessageType.Error);

            // validate activityInstanceId
            if (ActivityInstance.Load(activityInstanceId, msgs, dc).IsEmpty())
                msgs.Add("Error Saving Match, invalid activityInstance ", MessageType.Error);

            // check that this team doesnt already have a match on this activity instance
            var matches = Match.LoadByActivityInstanceId(activityInstanceId, msgs, dc);
            matches.ForEach(m =>
            {
                if(m.TeamId == teamId)
                    msgs.Add("Error Saving Match, team already has a match with this activity instance", MessageType.Error);
            });

            if (msgs.HasErrors())
                return null;
            
            return CreateOrUpdateMatch(currentUser, teamId, activityInstanceId, match, score, teamPlace, msgs, dc: dc);
        }

        /// <summary>
        /// Create or Update a Match
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="teamId"></param>
        /// <param name="activityInstanceId"></param>
        /// <param name="match"></param>
        /// <param name="score"></param>
        /// <param name="teamPlace"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        private static Match CreateOrUpdateMatch(WebUser currentUser, int teamId, int activityInstanceId, Match match = null, decimal score = 0, int teamPlace = 0, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
           
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    if (match.IsEmpty())
                    {
                        match = new Match
                        {
                            CreateDate = DateTime.UtcNow,
                            CreatedBy = currentUser.ID.Value,
                            TeamId = teamId,
                            ActivityInstanceId = activityInstanceId
                        };
                    }

                    match.LastModifiedDate = DateTime.UtcNow;
                    match.LastModifiedBy = currentUser.ID.Value;
                    match.TeamPlace = teamPlace;
                    match.Score = score;

                    msgs.AddRange(match.Save(adc));
                    
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.Add("Error saving Match", MessageType.Error);
                },
                dc: dc
            );

            return match;
        }
    }

}

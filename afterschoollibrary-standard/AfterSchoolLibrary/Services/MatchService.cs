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
           
            if (teamId.IsEmpty())
            {
                // not creating an account, current account has no password

                msgs.Add("Missing Team on Match", MessageType.Error);
                return null;
            }

            // vactivityInstacne
            if (activityInstanceId.IsEmpty())
            {
                msgs.Add("Error Saving Match, invalid activityInstance ", MessageType.Error);
                return null;
            }

            //validate activityInstanceId
            if (ActivityInstance.Load(activityInstanceId, msgs, dc).IsEmpty())
            {
                msgs.Add("Error Saving Match, invalid activityInstance ", MessageType.Error);
                return null;
            } 
            
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
                    if (amsgs.HasNoErrors())
                    {
                        if (match.IsEmpty())
                        {
                            match = new Match
                            {
                                CreateDate = DateTime.UtcNow,
                                CreatedBy = currentUser.ID.Value,
                                TeamId = teamId,
                                ActivityInstanceId = activityInstanceId,
                                Score = score,
                                TeamPlace = teamPlace
                            };
                        }

                        match.TeamId = teamId;
                        match.ActivityInstanceId = activityInstanceId;
                        match.LastModifiedDate = DateTime.UtcNow;
                        match.LastModifiedBy = currentUser.ID.Value;
                        match.TeamPlace = teamPlace;
                        match.Score = score;

                        msgs.AddRange(match.Save(adc));
                    }
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.Add("Match Saved", MessageType.Success);
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

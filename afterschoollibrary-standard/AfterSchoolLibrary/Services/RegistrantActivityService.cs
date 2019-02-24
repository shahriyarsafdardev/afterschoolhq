using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AfterSchoolLibrary.Services
{
    public static class RegistrantActivityService
    {
        public static ParticipationObject GetParticipationObject(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            switch (registrantActivity.ParticipationObjectTypeId)
            {
                case ParticipationObjectType.AccountActivityGroup:
                    return AccountActivityGroup.Load(registrantActivity.ParticipationObjectId.Value, msgs, dc).GetParticipationObject(msgs, dc);
                case ParticipationObjectType.Activity:
                    return Activity.Load(registrantActivity.ParticipationObjectId.Value, msgs, dc).GetParticipationObject(msgs, dc);
                case ParticipationObjectType.ActivityGroup:
                    return ActivityGroup.Load(registrantActivity.ParticipationObjectId.Value, msgs, dc).GetParticipationObject(msgs, dc);
            }

            return null;
        }
        public static Activity GetActivity(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (registrantActivity.ParticipationObjectTypeId == ParticipationObjectType.Activity)
                return Activity.Load(registrantActivity.ParticipationObjectId.Value, msgs, dc);

            return null;
        }

        public static ActivityGroup GetActivityGroup(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (registrantActivity.ParticipationObjectTypeId == ParticipationObjectType.ActivityGroup)
                return ActivityGroup.Load(registrantActivity.ParticipationObjectId.Value, msgs, dc);

            return null;
        }

        public static AccountActivityGroup GetAccountActivityGroup(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (registrantActivity.ParticipationObjectTypeId == ParticipationObjectType.AccountActivityGroup)
                return AccountActivityGroup.Load(registrantActivity.ParticipationObjectId.Value, msgs, dc);

            return null;
        }

        private static List<ActivityInstance> GetScheduleInstances(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var instances = new List<ActivityInstance>();
            ActivitySchedule schedule = null;
            switch (registrantActivity.ParticipationObjectTypeId)
            {
                case ParticipationObjectType.Activity:
                    schedule = registrantActivity.GetActivity(msgs, dc).GetSchedule(msgs: msgs, dc: dc);
                    if (schedule.IsNotEmpty())
                        instances.AddRange(schedule.GetInstances(msgs: msgs, dc: dc));
                    break;
                case ParticipationObjectType.ActivityGroup:
                    schedule = registrantActivity.GetActivityGroup(msgs, dc).GetSchedule(msgs: msgs, dc: dc);
                    if (schedule.IsNotEmpty())
                        instances.AddRange(schedule.GetInstances(msgs: msgs, dc: dc));
                    break;
                case ParticipationObjectType.AccountActivityGroup:
                    var aag = registrantActivity.GetAccountActivityGroup(msgs, dc);
                    schedule = aag.GetActivityGroup(msgs, dc).GetSchedule(msgs: msgs, dc: dc);
                    if (schedule.IsNotEmpty())
                        instances.AddRange(schedule.GetInstances(msgs: msgs, dc: dc));
                    break;
            }
            return instances;
        }

        public static bool IsInSession(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var now = DateTime.UtcNow;
            var instances = registrantActivity.GetScheduleInstances(msgs, dc);
            if (instances.Any())
                return instances.First().StartDateTime <= now && now <= instances.Last().EndDateTime;

            return false;
        }

        public static bool WasInSession(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var now = DateTime.UtcNow;
            var instances = registrantActivity.GetScheduleInstances(msgs, dc);
            if (instances.Any())
                return instances.Last().EndDateTime < now;

            return false;
        }

        public static bool WillBeInSession(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var now = DateTime.UtcNow;
            var instances = registrantActivity.GetScheduleInstances(msgs, dc);
            if (instances.Any())
                return now < instances.First().StartDateTime;

            return false;
        }

        public static bool IsOutOfSession(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var now = DateTime.UtcNow;
            var instances = registrantActivity.GetScheduleInstances(msgs, dc);
            if (instances.Any())
                return now > instances.Last().StartDateTime;

            return true;
        }

        public static ActivitySchedule GetSchedule(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            switch (registrantActivity.ParticipationObjectTypeId)
            {
                case ParticipationObjectType.AccountActivityGroup:
                    var aag = registrantActivity.GetAccountActivityGroup(msgs, dc);
                    return aag.GetActivityGroup(msgs, dc).GetSchedule(msgs: msgs, dc: dc);
                case ParticipationObjectType.Activity:
                    return registrantActivity.GetActivity(msgs, dc).GetSchedule(msgs: msgs, dc: dc);
                case ParticipationObjectType.ActivityGroup:
                    return registrantActivity.GetActivityGroup(msgs, dc).GetSchedule(msgs: msgs, dc: dc);
            }

            return null;
        }
    }
}

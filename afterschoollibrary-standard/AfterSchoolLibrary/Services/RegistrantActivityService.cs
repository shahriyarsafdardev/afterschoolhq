using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class RegistrantActivityService
    {
        public static ParticipationObject GetParticipationObject(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            switch (registrantActivity.ParticipationObjectTypeId)
            {
                case ParticipationObjectType.AccountActivityGroup:
                    return AccountActivityGroup.Load(registrantActivity.ParticipationObjectId.Value, msgs, dc).ToParticipationObject(msgs, dc);
                case ParticipationObjectType.Activity:
                    return Activity.Load(registrantActivity.ParticipationObjectId.Value, msgs, dc).ToParticipationObject(msgs, dc);
                case ParticipationObjectType.ActivityGroup:
                    return ActivityGroup.Load(registrantActivity.ParticipationObjectId.Value, msgs, dc).ToParticipationObject(msgs, dc);
            }

            return null;
        }
    }
}

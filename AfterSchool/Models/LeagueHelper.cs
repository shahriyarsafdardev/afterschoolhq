using AfterSchoolLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace AfterSchool.Models
{
    public static class LeagueHelper
    {
        public static SystemMessages UpdateStudentsInTeam(Activity activity, List<Registrant> registrants, NameValueCollection Request)
        {
            var msgs = new SystemMessages();
            var activityGroup = activity.GetActivityGroup();
            var accountActivityGroup = activityGroup.GetAccountActivityGroup(SessionVariables.CurrentAccount, msgs);
            var activities = accountActivityGroup.IsEmpty() ? activityGroup.GetActivities() : accountActivityGroup.GetActivities();
            activities = activities.Where(a => a.ID.Value != activity.ID.Value).ToList();            

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    registrants.ForEach(registrant =>
                    {
                        var registration = RegistrantActivity.LoadByParticipationObjectByRegistrant(
                            accountActivityGroup.ID.Value, 
                            ParticipationObjectType.AccountActivityGroup, 
                            registrant.ID.Value, 
                            amsgs, 
                            adc)
                            .GetRegistration(amsgs, adc);

                        var ra = RegistrantActivity.LoadByParticipationObjectByRegistrant(
                                    activity.ID.Value, 
                                    ParticipationObjectType.Activity, 
                                    registrant.ID.Value, 
                                    amsgs, 
                                    adc);

                        if (Request[$"registrant-{registrant.ID}"].IsBoolean() && ra.IsEmpty())
                        {
                            //check if students is registered for another team on the league
                            activities.ForEach(otherTeam =>
                            {
                                if (RegistrantActivity.LoadByParticipationObjectByRegistrant(otherTeam.ID.Value, ParticipationObjectType.Activity, registrant.ID.Value, amsgs, adc).IsNotEmpty())
                                    registration.RemoveParticipation(otherTeam.ID.Value, ParticipationObjectType.Activity, amsgs, adc);
                            });

                            //check if the student is not already registered for this team
                            if (RegistrantActivity.LoadByParticipationObjectByRegistrant(activity.ID.Value, ParticipationObjectType.Activity, registrant.ID.Value, amsgs, adc).IsEmpty())
                            {
                                // manages seats and create registrantactivity record
                                var participationObject = activity.GetParticipationObject(amsgs, adc);

                                registration.ProcessRegistrationAssignment(
                                        participationObject: participationObject,
                                        masterRegistrant: null,
                                        registrant: registrant,
                                        seatsRequested: 1,
                                        activityGroupId: activity.ActivityGroupId,
                                        periodId: activity.GetCurrentRegistrationPeriod(amsgs, adc)?.ID,
                                        amountCharged: participationObject.Cost,
                                        loggedInUser: SessionVariables.CurrentUser,
                                        msgs: amsgs,
                                        dc: adc);
                            }
                        }
                        else if (!Request[$"registrant-{registrant.ID}"].IsBoolean() && ra.IsNotEmpty())
                        {
                            ra.GetRegistration(amsgs, adc).RemoveParticipation(activity.ID.Value, ParticipationObjectType.Activity, amsgs, adc);
                        }
                    });

                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                    msgs.Add("Your new team registrations have been added", MessageType.Success);
                },
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
            );

            if (msgs.HasNoErrors())
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

            return msgs;
        }
    }

    class StudentsViewBagObject
    {
        public SystemMessages Messages { get; set; }
        public Activity Activity { get; set; }
        public ActivityGroup ActivityGroup { get; set; }
        public AccountActivityGroup AccountActivityGroup { get; set; }
        public List<Registrant> Registrants { get; set; }

        public StudentsViewBagObject(string activityUid)
        {
            var currentAccount = SessionVariables.CurrentAccount;

            Messages = new SystemMessages();
            Activity = currentAccount.GetActivity(activityUid.ToGuid(), Messages);
            ActivityGroup = Activity.GetActivityGroup(Messages);
            AccountActivityGroup = ActivityGroup.GetAccountActivityGroup(currentAccount, Messages);
            Registrants = AccountActivityGroup.IsEmpty() ? ActivityGroup.GetRegistrants(msgs: Messages) : AccountActivityGroup.GetRegistrants(Messages);
        }

        public StudentsViewBagObject(Activity activity)
        {
            Messages = new SystemMessages();
            Activity = activity;
            ActivityGroup = Activity.GetActivityGroup(Messages);
            AccountActivityGroup = ActivityGroup.GetAccountActivityGroup(SessionVariables.CurrentAccount, Messages);
            Registrants = AccountActivityGroup.IsEmpty() ? ActivityGroup.GetRegistrants(msgs: Messages) : AccountActivityGroup.GetRegistrants(Messages);
        }

        public StudentsViewBagObject(string activityGroupUid, string activityUid)
        {
            var currentAccount = SessionVariables.CurrentAccount;

            Messages = new SystemMessages();
            Activity = currentAccount.GetActivity(activityUid.ToGuid(), Messages);
            ActivityGroup = currentAccount.GetActivityGroup(activityGroupUid.ToGuid(), Messages);
            AccountActivityGroup = ActivityGroup.GetAccountActivityGroup(currentAccount, Messages);
            Registrants = AccountActivityGroup.IsEmpty() ? ActivityGroup.GetRegistrants(msgs: Messages) : AccountActivityGroup.GetRegistrants(Messages);
        }
    }
}
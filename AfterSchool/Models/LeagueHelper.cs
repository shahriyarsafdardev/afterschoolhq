using AfterSchoolLibrary;
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
    public class LeagueHelper
    {
        public static SystemMessages UpdateStudentsInTeam(Activity activity, List<Registrant> registrants, NameValueCollection Request)
        {
            var msgs = new SystemMessages();
            var updatedRegistrants = new List<Registrant>();
            var activityGroup = activity.GetActivityGroup();
            var accountActivityGroup = activityGroup.GetAccountActivityGroup(SessionVariables.CurrentAccount, msgs);
            // var activities = activityGroup.GetActivities();
            var activities = accountActivityGroup.IsEmpty() ? activityGroup.GetActivities() : accountActivityGroup.GetActivities();
            activities = activities.Where(a => a.ID.Value != activity.ID.Value).ToList();

            var period = activity.GetCurrentRegistrationPeriod();

            // var registrants = LoadEditStudentsViewBag(activity);

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

                        var ra = RegistrantActivity.LoadByParticipationObjectByRegistrant(activity.ID.Value, ParticipationObjectType.Activity, registrant.ID.Value, amsgs, adc);

                        if (Request[$"registrant-{registrant.ID}"].IsBoolean() && ra.IsEmpty())
                        {
                            //check if students is registered for another team on the league
                            foreach (var otherTeam in activities)
                            {
                                if (RegistrantActivity.LoadByParticipationObjectByRegistrant(otherTeam.ID.Value, ParticipationObjectType.Activity, registrant.ID.Value, amsgs, adc).IsNotEmpty())
                                {
                                    //var otherRegAct = RegistrantActivity.LoadByParticipationObjectByRegistrant(otherTeam.ID.Value, ParticipationObjectType.Activity, registrant.ID.Value, amsgs, adc);
                                    //var otherRegistration = otherRegAct.GetRegistration(amsgs, adc);
                                    amsgs.AddRange(registration.RemoveParticipation(otherTeam.ID.Value, ParticipationObjectType.Activity, adc));
                                }

                            }

                            //check if the student is not already registered for this team
                            if (RegistrantActivity.LoadByParticipationObjectByRegistrant(activity.ID.Value, ParticipationObjectType.Activity, registrant.ID.Value, amsgs, adc).IsEmpty())
                            {
                                //var registration = registrant.AddRegistration(
                                //    SessionVariables.CurrentAccount.ID.Value,
                                //    activity.ActivityGroupId,
                                //    period?.ID,
                                //    RegistrationCompletionStatus.Completed,
                                //    SessionVariables.LoggedInUser.ID.Value,
                                //    amsgs,
                                //    adc);

                                // manages seats and create registrantactivity record
                                Registration.ProcessRegistrationAssignment(
                                        registration: registration,
                                        participationObject: activity.ToParticipationObject(amsgs, adc),
                                        registrant: registrant,
                                        seatsRequested: 1,
                                        activityGroupId: activity.ActivityGroupId,
                                        periodId: activity.GetCurrentRegistrationPeriod(amsgs, adc)?.ID,
                                        loggedInUser: SessionVariables.LoggedInUser,
                                        msgs: amsgs,
                                        dc: adc);
                            }


                        }
                        else if (!Request[$"registrant-{registrant.ID}"].IsBoolean() && ra.IsNotEmpty())
                        {
                            amsgs.AddRange(ra.GetRegistration(amsgs, adc).RemoveParticipation(activity.ID.Value, ParticipationObjectType.Activity, adc));
                        }
                    });

                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                    msgs.Add("Your new team registrations have been added", MessageType.Success);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                }
            );

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} Saved", UILabel.ActivityLabel(activity.ActivityTypeId)), MessageType.Success);
            }

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
            Messages = new SystemMessages();
            Activity = Activity.LoadByUid(activityUid.ToGuid(), Messages);
            ActivityGroup = Activity.GetActivityGroup(Messages);
            AccountActivityGroup = ActivityGroup.GetAccountActivityGroup(SessionVariables.CurrentAccount, Messages);
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
            Messages = new SystemMessages();
            Activity = Activity.LoadByUid(activityUid.ToGuid(), Messages);
            ActivityGroup = ActivityGroup.LoadByUid(activityGroupUid.ToGuid(), Messages);
            AccountActivityGroup = ActivityGroup.GetAccountActivityGroup(SessionVariables.CurrentAccount, Messages);
            Registrants = AccountActivityGroup.IsEmpty() ? ActivityGroup.GetRegistrants(msgs: Messages) : AccountActivityGroup.GetRegistrants(Messages);
        }
    }
}
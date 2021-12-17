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
        public static RegistrantActivity GetRegistrantActivity(Guid Uid, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByUid(Uid, msgs, dc);
        }

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

        public static bool HasPlan(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantSubscriptionPlan.ExistsByParticipationObjectByRegistrants(
                participationObjectId: registrantActivity.ParticipationObjectId.Value,
                participationObjectType: registrantActivity.ParticipationObjectTypeId,
                registrantIds: registrantActivity.RegistrantId.Value.AsList(),
                msgs: msgs,
                dc: dc);
        }

        public static bool HasPaymentPlan(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantSubscriptionPlan.ExistsByParticipationObjectByRegistrantsByPlanType(
                participationObjectId: registrantActivity.ParticipationObjectId.Value,
                participationObjectType: registrantActivity.ParticipationObjectTypeId,
                registrantIds: registrantActivity.RegistrantId.Value.AsList(),
                SubscriptionPlanType.PaymentPlan,
                true,
                msgs: msgs,
                dc: dc);

        }

        public static List<FormQuestionAnswer> GetAnswers(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return registrantActivity.GetRegistration(msgs, dc).GetAnswers(msgs, dc);
        }

        public static bool IsRegisteredForInstance(this RegistrantActivity registrantActivity, ActivityInstance instance, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return registrantActivity.IsRegisteredForInstance(instance.ID.Value, msgs, dc);
        }

        public static bool IsRegisteredForInstance(this RegistrantActivity registrantActivity, int instanceId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var aiseats = ActivityInstanceSeats.LoadByRegistrantActivityIdByActivityInstanceId(registrantActivity.ID.Value, instanceId, msgs, dc);

            return aiseats.IsNotEmpty() && aiseats.NumberOfSeats.HasValue && aiseats.NumberOfSeats.Value > 0;
        }

        public static List<ActivityInstanceSeats> GetInstanceSeats(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityInstanceSeats.LoadByRegistrantActivityId(registrantActivity.ID.Value, msgs, dc);
        }

        public static ActivityInstanceSeats GetInstanceSeats(this RegistrantActivity registrantActivity, ActivityInstance instance, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityInstanceSeats.LoadByRegistrantActivityIdByActivityInstanceId(registrantActivity.ID.Value, instance.ID.Value, msgs, dc);
        }

        public static Registrant GetRegistrant(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.Load(registrantActivity.RegistrantId.Value, msgs, dc);
        }

        public static Registrant GetMasterRegistrant(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return registrantActivity.GetRegistrant(msgs, dc)?.GetMasterRegistrant(msgs, dc);
        }

        public static bool HasParentRegistrantId(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return registrantActivity.GetMasterRegistrant(msgs, dc).IsNotEmpty();
        }

        public static int? GetParentRegistrantId(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return registrantActivity.GetMasterRegistrant(msgs, dc)?.ID;            
        }

        public static Registration GetRegistration(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (registrantActivity.RegistrationId.HasValue)
                return Registration.Load(registrantActivity.RegistrationId.Value, msgs, dc);

            return null;
        }

        public static void CancelRegistration(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    registrantActivity.AdjustPreferencesDuringRemoval(null, amsgs, adc);
                    registrantActivity.AdjustSeatIndexesDuringRemoval(amsgs, adc);
                    amsgs.AddRange(registrantActivity.Delete(adc));
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        public static void ChangeSeatAssignment(this RegistrantActivity registrantActivity, RegistrationAssignmentType to, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (registrantActivity.AssignmentTypeId == to)
                return;

            //if you move it before the change is made it will adjust everyone correctly. Has to be done before the change is saved.
            //need a better function that can allow setting which seat index parameter needs to be handled
            registrantActivity.AdjustSeatIndexesDuringRemoval(msgs, dc);

            registrantActivity.SeatIndex = RegistrantActivity.GetSeatCountByAssignmentType(
                assignmentType: to,
                participationObjectId: registrantActivity.ParticipationObjectId,
                participationObjectType: registrantActivity.ParticipationObjectTypeId,
                msgs: msgs,
                dc: dc) + 1;

            registrantActivity.AssignmentTypeId = to;
            registrantActivity.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            registrantActivity.LastModifiedDate = DateTime.UtcNow;

            msgs.AddRange(registrantActivity.Save(dc));
        }

        public static void AdjustSeatIndexesDuringRemoval(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            // get all all registrant activities where the activity id is the same, 
            // the day is the same, and only update people with the same assignment type, 
            // UPDATE: No longer the same day because registration is now according to a schedule, not a day
            // I think they need to be in the same registration period if it exists   
            // TODO: Review if I need to check the same registration period, probably more related to the activity group if anything?

            var otherPeoplesActivities = RegistrantActivity.LoadByParticipationObject(
                    registrantActivity.ParticipationObjectId.Value,
                    registrantActivity.ParticipationObjectTypeId,
                    msgs,
                    dc
                )
                .Where(ra => ra.RegistrantId != registrantActivity.RegistrantId)
                .ToList();

            registrantActivity.AdjustSeatIndexes(otherPeoplesActivities, -1, msgs, dc);
        }

        public static void AdjustSeatIndexes(this RegistrantActivity registrantActivity, List<RegistrantActivity> ras, int factor, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            ras.Where(x => x.AssignmentTypeId == registrantActivity.AssignmentTypeId && x.SeatIndex > registrantActivity.SeatIndex)
                .ToList()
                .ForEach(item => item.AdjustSeatIndex(1 * factor, msgs, dc));
        }

        public static void AdjustSeatIndex(this RegistrantActivity registrantActivity, int amount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            registrantActivity.SeatIndex += amount;
            msgs.AddRange(registrantActivity.Save(dc));
        }

        public static void AdjustPreferenceNumber(this RegistrantActivity registrantActivity, int amount, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            registrantActivity.PreferenceNumber -= amount;
            msgs.AddRange(registrantActivity.Save(dc));
        }

        public static void AdjustPreferencesDuringRemoval(this RegistrantActivity registrantActivity, List<RegistrantActivity> registrantActivities = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (registrantActivities.IsEmpty())
            {
                // has to be for the same registration period and same account
                // need a better function to filter out registration period in the query
                registrantActivities = RegistrantActivity
                                        .LoadByRegistrantId(registrantActivity.RegistrantId.Value, dc: dc)
                                        .Where(x => x.RegistrationPeriodId == registrantActivity.RegistrationPeriodId)
                                        .ToList();
            }

            // lower choices really mean higher preference numbers. 
            // Like choice #1 is a higher choice than choice #3, 
            // but of course in math we know that 1 is lower than 3.
            registrantActivities
                .Where(x => x.PreferenceNumber.HasValue && x.PreferenceNumber.Value > registrantActivity.PreferenceNumber.Value)
                .ToList()
                .ForEach(ra => ra.AdjustPreferenceNumber(-1, msgs, dc));
        }

        public static decimal GetAmountPaid(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityPayment.GetTotalPaidByParticipationObjectByRegistrant(
                registrantActivity.ParticipationObjectId.Value,
                registrantActivity.ParticipationObjectTypeId,
                registrantActivity.RegistrantId.Value,
                false,
                msgs,
                dc);
        }

        public static decimal GetAmountPaidByParent(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityPayment.GetAmountPaidByParentByParticipationObjectByRegistrant(
                registrantActivity.ParticipationObjectId.Value,
                registrantActivity.ParticipationObjectTypeId,
                registrantActivity.RegistrantId.Value,
                false,
                msgs,
                dc);
        }

        public static bool GetIsProRatedFlagByParent(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityPayment.GetIsProRatedFlagByParentByParticipationObjectByRegistrant(
                registrantActivity.ParticipationObjectId.Value,
                registrantActivity.ParticipationObjectTypeId,
                registrantActivity.RegistrantId.Value,
                false,
                msgs,
                dc);
        }

        public static SystemMessages SendPaymentReminder(this RegistrantActivity registrantActivity)
        {
            return Helpers.EmailHelper.SendPaymentReminder(registrantActivity);
        }

        public static Account GetActivityAccount(this RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return registrantActivity.GetParticipationObject().OwnerAccount;
        }

        public static string GetRegistrantName(this RegistrantActivity registrantActivity, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (loggedInUser.GetUserType(registrantActivity.GetActivityAccount(msgs, dc), msgs, dc) != UserType.TechSupport)
                return registrantActivity.RegistrantName.IfBlank(registrantActivity.RegistrantEmail);

            return registrantActivity.RegistrantUid.ToString();
        }

        public static bool IsAssigned(this RegistrantActivity registrantActivity, RegistrationAssignmentType registrationAssignmentType)
        {
            return registrantActivity.AssignmentTypeId == registrationAssignmentType;
        }

        public static bool IsRegistrantActivityStatusAccessible(this RegistrantActivity registrantActivity)
        {
            return registrantActivity.AssignmentTypeId.NotIn(new List<RegistrationAssignmentType>() {
                RegistrationAssignmentType.NotSet,
                RegistrationAssignmentType.Withdrawn,
                RegistrationAssignmentType.Declined,
                RegistrationAssignmentType.Invited });
        }

        public static bool IsWithdrawEnabled(this RegistrantActivity registrantActivity)
        {
            return registrantActivity.AssignmentTypeId.NotIn(new List<RegistrationAssignmentType>() {
                RegistrationAssignmentType.NotSet,
                RegistrationAssignmentType.Withdrawn,
                RegistrationAssignmentType.Declined
            });
        }

        public static bool IsSendInvoiceEnabled(this RegistrantActivity registrantActivity)
        {
            return registrantActivity.AssignmentTypeId.In(new List<RegistrationAssignmentType>() {
                RegistrationAssignmentType.Reserved,
                RegistrationAssignmentType.ReservedPendingParentalConsent,
                RegistrationAssignmentType.ReservedPendingPayment,
                RegistrationAssignmentType.Applied,
            });
        }

        private static RegistrationAssignmentType[] ReservedRegistrantAssignmentTypes
        {
            get
            {
                return new RegistrationAssignmentType[]
                {
                    RegistrationAssignmentType.Reserved,
                    RegistrationAssignmentType.ReservedPendingParentalConsent,
                    RegistrationAssignmentType.ReservedPendingPayment,
                };
            }
        }

        public static bool IsReservedRegistrationAssignmentType(this RegistrantActivity registrantActivity)
        {
            return registrantActivity.AssignmentTypeId.In(ReservedRegistrantAssignmentTypes);
        }
    }
}

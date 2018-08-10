using System;
using System.Linq;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections.Generic;

namespace AfterSchoolLibrary
{
    public class ParticipationObject
    {
        public object Object { get; set; }
        public int? ObjectID { get; set; }
        public ParticipationObjectType ObjectType { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public ActivitySchedule Schedule { get; set; }
        public Location Location { get; set; }
        public GenderRestrictionType GenderRestriction { get; set; }
        public int? MinGradeId { get; set; }
        public int? MaxGradeId { get; set; }
        public decimal? Cost { get; set; }
        public CostUnitType? CostUnitType { get; set; }
        public RegistrationPeriod RegistrationPeriod { get; set; }
        public List<SubscriptionPlan> PaymentPlans { get; set; }
        public List<SubscriptionPlan> SubscriptionPlans { get; set; }
        public string UniqueName { get; set; }
        public RegistrationMode RegistrationMode { get; set; }
        public Form Form { get; set; }
        public Account OwnerAccount { get; set; }
        public bool AllowToPayLater { get; set; }
        public bool IsFree { get; set; }
        public Guid Uid { get; set; }
        public int? SeatsRemaining { get; set; }
        public int? WaitingListSeatsRemaining { get; set; }
        public string Description { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string CostDescription { get; set; }
        public RegistrationMode RegistrationModeId { get; set; }

        private ActivityType? ActivityType { get; set; }
        private ActivityGroupType? ActivityGroupType { get; set; }       

        public ParticipationObject(Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Object = activity;
            ObjectID = activity.ID;
            ObjectType = ParticipationObjectType.Activity;
            Uid = activity.Uid;
            Name = activity.Name;
            Details = activity.Details;
            Schedule = activity.GetSchedule(false, msgs, dc);
            Location = activity.GetLocation(msgs, dc);
            GenderRestriction = activity.GenderRestriction;
            ActivityType = activity.ActivityTypeId;
            UniqueName = activity.UniqueName;
            RegistrationMode = activity.RegistrationModeId;
            SeatsRemaining = activity.GetSeatsRemaining(msgs, dc);
            WaitingListSeatsRemaining = activity.GetWaitingListSeatsRemaining(msgs, dc);
            Description = activity.Description;
            ContactName = activity.ContactName;
            ContactEmail = activity.ContactEmail;
            ContactPhone = activity.ContactPhone;
            CostDescription = activity.CostDescription;
            RegistrationModeId = activity.RegistrationModeId;
           
            if (activity.GradeLevelStart.HasValue && activity.GradeLevelEnd.HasValue)
            {
                MinGradeId = activity.GradeLevelStart;
                MaxGradeId = activity.GradeLevelEnd;
            }
            else
            {
                MinGradeId = GradeLevel.Load(msgs: msgs, dc: dc).First().ID;
                MaxGradeId = GradeLevel.Load(msgs: msgs, dc: dc).Last().ID;
            }

            Cost = activity.CostPerRegistrant;
            CostUnitType = global::CostUnitType.PerRegistrant;
            AllowToPayLater = activity.AllowToPayLater;
            RegistrationPeriod = activity.GetActiveRegistrationPeriod(msgs, dc);
            Form = activity.GetActiveActivityForm(msgs, dc)?.ToForm();
            OwnerAccount = activity.GetAccount(msgs, dc);

            PaymentPlans = activity.GetPaymentPlans(msgs, dc);
            SubscriptionPlans = activity.GetSubscriptionPlans(msgs, dc);
            IsFree = (!activity.CostPerRegistrant.HasValue || activity.CostPerRegistrant.Value == 0.ToDecimal()) && SubscriptionPlans.Count == 0 && PaymentPlans.Count == 0;
        }

        public ParticipationObject(ActivityGroup activityGroup, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Object = activityGroup;
            ObjectID = activityGroup.ID;
            ObjectType = ParticipationObjectType.ActivityGroup;
            Uid = activityGroup.Uid;
            Name = activityGroup.Name;
            Details = activityGroup.Description;
            Schedule = activityGroup.GetSchedule(false, msgs, dc);
            GenderRestriction = activityGroup.GenderRestriction;
            MinGradeId = activityGroup.MinGradeLevelId;
            MaxGradeId = activityGroup.MaxGradeLevelId;
            Cost = activityGroup.Cost;
            CostUnitType = activityGroup.CostUnitTypeId;
            ActivityGroupType = activityGroup.ActivityGroupTypeId;
            RegistrationPeriod = activityGroup.GetCurrentOrNextRegistrationPeriod(msgs, dc);
            UniqueName = activityGroup.UniqueName;
            RegistrationMode = activityGroup.RegistrationModeId;
            Form = activityGroup.GetForm(msgs, dc);
            OwnerAccount = activityGroup.GetAccount(msgs, dc);
            // Description = activityGroup.Description;
            Location = OwnerAccount.GetDefaultLocation(msgs, dc);
            // TODO: PayLater = activityGroup.AllowToPayLater;
            RegistrationModeId = activityGroup.RegistrationModeId;

            PaymentPlans = activityGroup.GetPaymentPlans(msgs, dc);
            SubscriptionPlans = activityGroup.GetSubscriptionPlans(msgs, dc);
            IsFree = (!activityGroup.Cost.HasValue || activityGroup.Cost.Value == 0.ToDecimal()) && SubscriptionPlans.Count == 0 && PaymentPlans.Count == 0;
        }

        public ParticipationObject(AccountActivityGroup aag, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Object = aag;
            var activityGroup = aag.GetActivityGroup(msgs, dc);
            ObjectID = aag.ID;
            ObjectType = ParticipationObjectType.AccountActivityGroup;
            Uid = activityGroup.Uid;
            Name = activityGroup.Name;
            Details = activityGroup.Description;
            Schedule = activityGroup.GetSchedule(false, msgs, dc);
            GenderRestriction = activityGroup.GenderRestriction;
            MinGradeId = activityGroup.MinGradeLevelId;
            MaxGradeId = activityGroup.MaxGradeLevelId;
            Cost = aag.Cost;
            CostUnitType = aag.CostUnitTypeId;
            // TODO: PayLater = activityGroup.AllowToPayLater;
            ActivityGroupType = activityGroup.ActivityGroupTypeId;
            RegistrationPeriod = aag.GetCurrentOrNextRegistrationPeriod(msgs, dc);
            UniqueName = activityGroup.UniqueName;
            RegistrationMode = activityGroup.RegistrationModeId;
            Form = activityGroup.GetForm(msgs, dc);
            OwnerAccount = aag.GetAccount(msgs, dc);
            ContactName = aag.ContactName;
            ContactEmail = aag.ContactEmail;
            ContactPhone = aag.ContactPhone;
            // Description = activityGroup.Description;
            Location = OwnerAccount.GetDefaultLocation(msgs, dc);
            RegistrationModeId = RegistrationMode.Apply; // aag.RegistrationModeId;

            PaymentPlans = activityGroup.GetPaymentPlans(msgs, dc);
            SubscriptionPlans = activityGroup.GetSubscriptionPlans(msgs, dc);

            IsFree = (!aag.Cost.HasValue || aag.Cost.Value == 0.ToDecimal()) && SubscriptionPlans.Count == 0 && PaymentPlans.Count == 0;
        }

        public string GetHomePageUrl(bool isAfterSchool = true)
        {
            switch (this.ObjectType)
            {
                case ParticipationObjectType.Activity:
                    return ((Activity)this.Object).GetHomePageUrl(isAfterSchool);
                case ParticipationObjectType.ActivityGroup:
                    return ((ActivityGroup)this.Object).GetHomePageUrl(isAfterSchool);
                case ParticipationObjectType.AccountActivityGroup:
                    return ((AccountActivityGroup)this.Object).GetHomePageUrl(isAfterSchool);
            }
            return "";
        }

        public string GetRegistrationPageUrl(bool isAfterSchool = true)
        {
            return $"{GetHomePageUrl(isAfterSchool)}/register";
        }

        public bool IsSport()
        {
            if (this.ObjectType == ParticipationObjectType.Activity)
                return this.ActivityType.Value == global::ActivityType.Team;
            else
                return this.ActivityGroupType == global::ActivityGroupType.League;
        }

        public Tuple<GradeLevel, GradeLevel> GetMinAndMaxGradeLevels(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (MinGradeId.HasValue && MaxGradeId.HasValue)
                return Tuple.Create(GetMinGradeLevel(msgs, dc), GetMaxGradeLevel(msgs, dc));

            return null;
        }            

        public GradeLevel GetMinGradeLevel(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (MinGradeId.HasValue)
                return GradeLevel.Load(MinGradeId.Value, msgs, dc);

            return null;
        }

        public GradeLevel GetMaxGradeLevel(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (MaxGradeId.HasValue)
                return GradeLevel.Load(MaxGradeId.Value, msgs, dc);

            return null;
        }

        /// <summary>
        /// if seats remaining is null meaning no seats are set (could be a league or unlimited access) 
        /// or seats remaining > 0
        /// </summary>
        /// <returns></returns>
        public bool HasSeatsAvailable()
        {
            return !SeatsRemaining.HasValue || SeatsRemaining.Value > 0;
        }
        
        /// <summary>
        /// if waiting list has a value and the value is greater than zero
        /// </summary>
        /// <returns></returns>
        public bool HasWaitingListSeatsAvailable()
        {
            return WaitingListSeatsRemaining.HasValue && WaitingListSeatsRemaining.Value > 0;
        }

        public string GetRegistrationThankYou(RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (registrantActivity.AssignmentTypeId == RegistrationAssignmentType.Applied)
            {
                if (ObjectType == ParticipationObjectType.Activity)
                    return ((Activity)Object).GetActivityProperty(AccountPropertyType.ApplicationThankYouEmail, dc)?.Value;
                else
                    return ((ActivityGroup)Object).GetGroupProperty(AccountPropertyType.ApplicationThankYouEmail, dc)?.Value;
            }
            else
            {
                if (ObjectType == ParticipationObjectType.Activity)
                    return ((Activity)Object).GetActivityProperty(AccountPropertyType.RegistrationThankYouEmail, dc)?.Value;
                else
                    return ((ActivityGroup)Object).GetGroupProperty(AccountPropertyType.RegistrationThankYouEmail, dc)?.Value;
            }
        }

        public int GetRegistrantCountByAssignmentType(RegistrationAssignmentType assignmentType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByAssignmentType(
                assignmentType: assignmentType,
                participationObjectId: this.ObjectID,
                participationObjectType: this.ObjectType,
                msgs: msgs,
                dc: dc);
        }

        public RegistrationPeriod GetCurrentRegistrationPeriod(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (IsActivity())
                return GetActivity().GetCurrentRegistrationPeriod(msgs, dc);

            if (IsActivityGroup())
                return GetActivityGroup().GetCurrentRegistrationPeriod(msgs, dc);

            if (IsAccountActivityGroup())
                return GetAccountActivityGroup().GetCurrentRegistrationPeriod(msgs, dc);

            return null;
        }

        public RegistrationPeriod GetCurrentOrNextRegistrationPeriod(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (IsActivity())
                return GetActivity().GetCurrentOrNextRegistrationPeriod(msgs, dc);

            if (IsActivityGroup())
                return GetActivityGroup().GetCurrentOrNextRegistrationPeriod(msgs, dc);

            if (IsAccountActivityGroup())
                return GetAccountActivityGroup().GetCurrentOrNextRegistrationPeriod(msgs, dc);

            return null;
        }

        public bool IsActivity()
        {
            return ObjectType == ParticipationObjectType.Activity;
        }

        public bool IsAccountActivityGroup()
        {
            return ObjectType == ParticipationObjectType.AccountActivityGroup;
        }

        public bool IsActivityGroup()
        {
            return ObjectType == ParticipationObjectType.ActivityGroup;
        }

        public ActivityGroup GetActivityGroup()
        {
            return (ActivityGroup)this.Object;
        }

        public AccountActivityGroup GetAccountActivityGroup()
        {
            return (AccountActivityGroup)this.Object;
        }

        public Activity GetActivity()
        {
            return (Activity)this.Object;
        }
    }
}

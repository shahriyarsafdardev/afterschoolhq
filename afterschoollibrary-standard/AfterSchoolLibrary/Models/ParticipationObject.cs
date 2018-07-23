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
        public List<PaymentPlan> PaymentPlans { get; set; }
        public List<SubscriptionPlan> SubscriptionPlans { get; set; }
        public string UniqueName { get; set; }
        public RegistrationMode RegistrationMode { get; set; }
        public Form Form { get; set; }
        public Account OwnerAccount { get; set; }
        public bool AllowToPayLater { get; set; }
        public bool IsFree { get; set; }

        private ActivityType? ActivityType { get; set; }
        private ActivityGroupType? ActivityGroupType { get; set; }

        public ParticipationObject(Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Object = activity;
            ObjectID = activity.ID;
            ObjectType = ParticipationObjectType.Activity;
            Name = activity.Name;
            Details = activity.Details;
            Schedule = activity.GetSchedule(false, msgs, dc);
            Location = activity.GetLocation(msgs, dc);
            GenderRestriction = activity.GenderRestriction;
            ActivityType = activity.ActivityTypeId;
            UniqueName = activity.UniqueName;
            RegistrationMode = activity.RegistrationModeId;
           
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
            IsFree = !activity.CostPerRegistrant.HasValue || activity.CostPerRegistrant.Value == 0.ToDecimal();
        }

        public ParticipationObject(ActivityGroup activityGroup, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Object = activityGroup;
            ObjectID = activityGroup.ID;
            ObjectType = ParticipationObjectType.ActivityGroup;
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
            // TODO: PayLater = activityGroup.AllowToPayLater;

            PaymentPlans = activityGroup.GetPaymentPlans(msgs, dc);
            SubscriptionPlans = activityGroup.GetSubscriptionPlans(msgs, dc);
            IsFree = !activityGroup.Cost.HasValue || activityGroup.Cost.Value == 0.ToDecimal();
        }

        public ParticipationObject(AccountActivityGroup aag, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Object = aag;
            var activityGroup = aag.GetActivityGroup(msgs, dc);
            ObjectID = activityGroup.ID;
            ObjectType = ParticipationObjectType.ActivityGroup;
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

            PaymentPlans = activityGroup.GetPaymentPlans(msgs, dc);
            SubscriptionPlans = activityGroup.GetSubscriptionPlans(msgs, dc);

            IsFree = !aag.Cost.HasValue || aag.Cost.Value == 0.ToDecimal();
        }

        public string GetHomePageUrl()
        {
            switch (this.ObjectType)
            {
                case ParticipationObjectType.Activity:
                    return ((Activity)this.Object).GetHomePageUrl();
                case ParticipationObjectType.ActivityGroup:
                    return ((ActivityGroup)this.Object).GetHomePageUrl();
            }
            return "";
        }

        public string GetRegistrationPageUrl()
        {
            switch (this.ObjectType)
            {
                case ParticipationObjectType.Activity:
                    return ((Activity)this.Object).GetRegistrationPageUrl();
                case ParticipationObjectType.ActivityGroup:
                    return ((ActivityGroup)this.Object).GetHomePageUrl();
            }
            return "";
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
    }
}

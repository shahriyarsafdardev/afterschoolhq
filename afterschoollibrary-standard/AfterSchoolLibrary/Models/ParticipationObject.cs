using System;
using System.Linq;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections.Generic;

namespace AfterSchoolLibrary.Models
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
        public bool IsTuitionFree { get; set; }
        public Guid Uid { get; set; }
        public int? SeatsRemaining { get; set; }
        public int? WaitingListSeatsRemaining { get; set; }
        public string Description { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string CostDescription { get; set; }
        public string LocationDetails { get; set; }

        private ActivityType? ActivityType { get; set; }
        private ActivityGroupType? ActivityGroupType { get; set; }

        public ParticipationObject(int participationObjectId, ParticipationObjectType participationObjectType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            switch(participationObjectType)
            {
                case ParticipationObjectType.AccountActivityGroup:
                    InitAccountActivityGroup(AccountActivityGroup.Load(participationObjectId, msgs, dc), msgs, dc);
                    break;
                case ParticipationObjectType.Activity:
                    InitActivity(Activity.Load(participationObjectId, msgs, dc), msgs, dc);
                    break;
                case ParticipationObjectType.ActivityGroup:
                    InitActivityGroup(ActivityGroup.Load(participationObjectId, msgs, dc), msgs, dc);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void InitActivity(Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Object = activity;
            ObjectID = activity.ID;
            ObjectType = ParticipationObjectType.Activity;
            Uid = activity.Uid;
            Name = activity.Name;
            Details = activity.Details;
            Schedule = activity.GetSchedule(msgs, dc);
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
            LocationDetails = activity.LocationDetails;

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
            Form = activity.GetActiveActivityForm(msgs, dc)?.GetForm();
            OwnerAccount = activity.GetAccount(msgs, dc);

            PaymentPlans = activity.GetActivePaymentPlans(msgs, dc);
            SubscriptionPlans = activity.GetActiveSubscriptionPlans(msgs, dc);
            IsTuitionFree = (!activity.CostPerRegistrant.HasValue || activity.CostPerRegistrant.Value == 0.ToDecimal());
            IsFree = IsTuitionFree && SubscriptionPlans.Count == 0 && PaymentPlans.Count == 0;
        }

        private void InitActivityGroup(ActivityGroup activityGroup, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Object = activityGroup;
            ObjectID = activityGroup.ID;
            ObjectType = ParticipationObjectType.ActivityGroup;
            Uid = activityGroup.Uid;
            Name = activityGroup.Name;
            Details = activityGroup.Description;
            Schedule = activityGroup.GetSchedule(msgs, dc);
            GenderRestriction = activityGroup.GenderRestriction;
            MinGradeId = activityGroup.MinGradeLevelId;
            MaxGradeId = activityGroup.MaxGradeLevelId;
            Cost = activityGroup.Cost;
            CostUnitType = activityGroup.CostUnitTypeId;
            ActivityGroupType = activityGroup.ActivityGroupTypeId;
            RegistrationPeriod = activityGroup.GetLatestRegistrationPeriod(msgs, dc);
            UniqueName = activityGroup.UniqueName;
            RegistrationMode = activityGroup.RegistrationModeId;
            Form = activityGroup.GetForm(msgs, dc);
            OwnerAccount = activityGroup.GetAccount(msgs, dc);
            ContactEmail = activityGroup.ContactEmail;
            ContactName = activityGroup.ContactName;
            ContactPhone = activityGroup.ContactPhone;
            // Description = activityGroup.Description;
            Location = OwnerAccount.GetDefaultLocation(msgs, dc);
            // TODO: PayLater = activityGroup.AllowToPayLater;
            SeatsRemaining = activityGroup.GetSeatsRemaining(msgs, dc);
            WaitingListSeatsRemaining = activityGroup.GetWaitingListSeatsRemaining(msgs, dc);

            PaymentPlans = activityGroup.GetPaymentPlans(msgs, dc);
            SubscriptionPlans = activityGroup.GetSubscriptionPlans(msgs, dc);
            IsTuitionFree = (!activityGroup.Cost.HasValue || activityGroup.Cost.Value == 0.ToDecimal());
            IsFree = IsTuitionFree && SubscriptionPlans.Count == 0 && PaymentPlans.Count == 0;
        }

        private void InitAccountActivityGroup(AccountActivityGroup aag, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Object = aag;
            var activityGroup = aag.GetActivityGroup(msgs, dc);
            ObjectID = aag.ID;
            ObjectType = ParticipationObjectType.AccountActivityGroup;
            Uid = activityGroup.Uid;
            Name = activityGroup.Name;
            Details = activityGroup.Description;
            Schedule = activityGroup.GetSchedule(msgs, dc);
            GenderRestriction = activityGroup.GenderRestriction;
            MinGradeId = activityGroup.MinGradeLevelId;
            MaxGradeId = activityGroup.MaxGradeLevelId;
            Cost = aag.Cost;
            CostUnitType = aag.CostUnitTypeId;
            // TODO: PayLater = activityGroup.AllowToPayLater;
            ActivityGroupType = activityGroup.ActivityGroupTypeId;
            RegistrationPeriod = aag.GetLatestRegistrationPeriod(msgs, dc);
            UniqueName = activityGroup.UniqueName;
            Form = aag.GetForm(msgs, dc);
            OwnerAccount = aag.GetAccount(msgs, dc);
            ContactName = aag.ContactName;
            ContactEmail = aag.ContactEmail;
            ContactPhone = aag.ContactPhone;
            // Description = activityGroup.Description;
            Location = OwnerAccount.GetDefaultLocation(msgs, dc);
            RegistrationMode = RegistrationMode.Apply; // aag.RegistrationModeId;

            PaymentPlans = activityGroup.GetPaymentPlans(msgs, dc);
            SubscriptionPlans = activityGroup.GetSubscriptionPlans(msgs, dc);

            IsTuitionFree = (!aag.Cost.HasValue || aag.Cost.Value == 0.ToDecimal());
            IsFree = IsTuitionFree && SubscriptionPlans.Count == 0 && PaymentPlans.Count == 0;
        }

        public ParticipationObject(Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            InitActivity(activity, msgs, dc);
        }

        public ParticipationObject(ActivityGroup activityGroup, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            InitActivityGroup(activityGroup, msgs, dc);
        }

        public ParticipationObject(AccountActivityGroup aag, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            InitAccountActivityGroup(aag, msgs, dc);
        }

        public bool IsNotFreeAndCanAcceptPayments
        {
            get
            {
                return !IsFree && OwnerAccount.CanProcessPayments();
            } 
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
                if (IsActivity())
                    return this.GetActivity().GetActivityProperty(AccountPropertyType.ApplicationThankYouEmail, msgs, dc)?.Value;
                else if (IsActivityGroup())
                    return this.GetActivityGroup().GetGroupProperty(AccountPropertyType.ApplicationThankYouEmail, msgs, dc)?.Value;
                else if (IsAccountActivityGroup())
                    return this.GetAccountActivityGroup()
                        .GetActivityGroup(msgs, dc)
                        .GetGroupProperty(AccountPropertyType.ApplicationThankYouEmail, msgs, dc)?
                        .Value;
            }
            else
            {
                if (ObjectType == ParticipationObjectType.Activity)
                    return this.GetActivity().GetActivityProperty(AccountPropertyType.RegistrationThankYouEmail, msgs, dc)?.Value;
                else if (IsActivityGroup())
                    return this.GetActivityGroup().GetGroupProperty(AccountPropertyType.RegistrationThankYouEmail, msgs, dc)?.Value;
                else if (IsAccountActivityGroup())                
                    return this.GetAccountActivityGroup()
                        .GetActivityGroup(msgs, dc)
                        .GetGroupProperty(AccountPropertyType.RegistrationThankYouEmail, msgs, dc)?
                        .Value;
            }

            return "";
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

        public int GetRegistrantCountByAssignmentTypes(List<RegistrationAssignmentType> assignmentTypes, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByAssignmentTypes(
                assignmentTypes: assignmentTypes,
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

        public RegistrationPeriod GetLatestRegistrationPeriod(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (IsActivity())
                return GetActivity().GetLatestRegistrationPeriod(msgs, dc);

            if (IsActivityGroup())
                return GetActivityGroup().GetLatestRegistrationPeriod(msgs, dc);

            if (IsAccountActivityGroup())
                return GetAccountActivityGroup().GetLatestRegistrationPeriod(msgs, dc);

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

        public int? GetActivityGroupId()
        {
            return IsActivityGroup() ? ObjectID : null;
        }

        public int? GetAccountActivityGroupId()
        {
            return IsAccountActivityGroup() ? ObjectID : null;
        }

        public int? GetActivityId()
        {
            return IsActivity() ? ObjectID : null;
        }

        public decimal GetAmountPaidByRegistrant(int registrantId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return ActivityPayment.GetTotalPaidByParticipationObjectByRegistrant(
                this.ObjectID.Value, 
                this.ObjectType, 
                registrantId, 
                msgs, 
                dc);
        }

        public ActivityPayment AddPayment(decimal amount, Registrant registrant, Transaction transaction, RegistrantSubscriptionPlan registrantSubscriptionPlan, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var payment = new ActivityPayment()
            {
                ParticipationObjectId = this.ObjectID,
                ParticipationObjectTypeId = this.ObjectType,
                Amount = amount,
                RegistrantId = registrant.ID,
                AccountId = this.OwnerAccount.ID,
                TransactionId = transaction.ID,
                RegistrantSubscriptionPlanId = registrantSubscriptionPlan?.ID,
                CreateDate = DateTime.UtcNow,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0)
            };

            msgs.AddRange(payment.Save(dc));

            return payment;
        }
    }
}

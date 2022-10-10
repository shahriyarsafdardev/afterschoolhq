using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchoolLibrary.Models
{
    public class ParticipationObject
    {
        public ParticipationObject(int participationObjectId, ParticipationObjectType participationObjectType, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            RequiresParentalConsent = true;
            IsInviteOnly = false;
            InitObject(participationObjectId, participationObjectType, true, msgs, dc);
        }

        public ParticipationObject(int participationObjectId, ParticipationObjectType participationObjectType, bool loadAllDetails, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            RequiresParentalConsent = true;
            IsInviteOnly = false;
            InitObject(participationObjectId, participationObjectType, loadAllDetails, msgs, dc);
        }

        public ParticipationObject(Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            InitActivity(activity, true, msgs, dc);
        }

        public ParticipationObject(ActivityGroup activityGroup, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            InitActivityGroup(activityGroup, true, msgs, dc);
        }

        public ParticipationObject(AccountActivityGroup aag, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            InitAccountActivityGroup(aag, true, msgs, dc);
        }

        public ParticipationObject(ActivityType activityType)
        {
            ActivityType = activityType;
        }

        public object Object { get; set; }
        public int? ObjectID { get; set; }
        public ParticipationObjectType ObjectType { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public ActivitySchedule Schedule { get; set; }
        public Location Location { get; set; }
        public Account Provider { get; set; }
        public GenderRestrictionType GenderRestriction { get; set; }
        public int? MinGradeId { get; set; }
        public int? MaxGradeId { get; set; }
        public int? AgeStart { get; set; }
        public int? AgeEnd { get; set; }
        public decimal? Cost { get; set; }
        public CostUnitType? CostUnitType { get; set; }
        public RegistrationPeriod RegistrationPeriod { get; set; }
        public List<SubscriptionPlan> PaymentPlans { get; set; }
        public List<SubscriptionPlan> SubscriptionPlans { get; set; }
        public List<SubscriptionPlan> PostPayPlans { get; set; }
        public string UniqueName { get; set; }
        public RegistrationMode RegistrationMode { get; set; }
        public Form Form { get; set; }
        public Account OwnerAccount { get; set; }
        public bool AllowToPayLater { get; set; }
        public bool IsFree { get; set; }
        public bool IsRegistrationFree { get; set; }
        public Guid Uid { get; set; }
        public int? SeatsRemaining { get; set; }
        public string Description { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string CostDescription { get; set; }
        public string LocationDetails { get; set; }
        public bool UseStripeConnectedAccount { get; set; }
        public bool RequiresParentalConsent { get; set; }
        public bool IsInviteOnly { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public ActivityType? ActivityType { get; set; }
        public ActivityGroupType? ActivityGroupType { get; set; }
        public ActivityLocationType ActivityLocationType { get; set; }
        public string VirtualLocationInfo { get; set; }
        public int? MinSeatsRequired { get; set; }
        public int? SeatsAvailable { get; set; }
        public bool RegistrationFinalized { get; set; }
        public int? SeatsTaken { get; set; }
        public bool HidePreciseLocation { get; set; }
        public bool IsPublished { get; set; }
        public int? RegistrationCutoffHours { get; set; }
        public bool IsContinuousEnrollmentEnabled { get; set; }
        public bool IsGenderEnforced { get; set; }

        private void InitObject(int participationObjectId, ParticipationObjectType participationObjectType, bool loadAllDetails, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            SubscriptionPlans = new List<SubscriptionPlan>();
            PostPayPlans = new List<SubscriptionPlan>();
            PaymentPlans = new List<SubscriptionPlan>();
            switch (participationObjectType)
            {
                case ParticipationObjectType.AccountActivityGroup:
                    InitAccountActivityGroup(AccountActivityGroup.Load(participationObjectId, msgs, dc), true, msgs, dc);
                    break;
                case ParticipationObjectType.Activity:
                    InitActivity(Activity.Load(participationObjectId, msgs, dc), true, msgs, dc);
                    break;
                case ParticipationObjectType.ActivityGroup:
                    InitActivityGroup(ActivityGroup.Load(participationObjectId, msgs, dc), true, msgs, dc);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void InitActivity(Activity activity, bool loadAllDetails = true, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Object = activity;
            ObjectID = activity.ID;
            ObjectType = ParticipationObjectType.Activity;
            Uid = activity.Uid;
            Name = activity.Name;
            Details = activity.Details;
            GenderRestriction = activity.GenderRestriction;
            ActivityType = activity.ActivityTypeId;
            UniqueName = activity.UniqueName;
            RegistrationMode = activity.RegistrationModeId;
            Description = activity.Description;
            ContactName = activity.ContactName;
            ContactEmail = activity.ContactEmail;
            ContactPhone = activity.ContactPhone;
            CostDescription = activity.CostDescription;
            LocationDetails = activity.LocationDetails;
            Cost = activity.CostPerRegistrant;
            CostUnitType = global::CostUnitType.PerRegistrant;
            AllowToPayLater = activity.AllowToPayLater;
            IsRegistrationFree = !activity.CostPerRegistrant.HasValue || activity.CostPerRegistrant.Value == 0.ToDecimal();
            RequiresParentalConsent = activity.RequiresParentalConsent;
            IsInviteOnly = activity.IsInviteOnly;
            ActivityLocationType = activity.ActivityLocationTypeId;
            LastModifiedDate = activity.LastModifiedDate;
            VirtualLocationInfo = activity.VirtualLocationInfo;
            MinSeatsRequired = activity.MinSeatsRequired;
            SeatsAvailable = activity.SeatsAvailable;
            RegistrationFinalized = activity.RegistrationFinalized;
            HidePreciseLocation = activity.HidePreciseLocation;
            AgeStart = activity.AgeStart;
            AgeEnd = activity.AgeEnd;
            IsPublished = activity.IsPublished;
            RegistrationCutoffHours = activity.RegistrationCutoffHours;
            IsContinuousEnrollmentEnabled = activity.IsContinuousEnrollment;
            IsGenderEnforced = activity.IsGenderEnforced;

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

            if (loadAllDetails)
            {
                SeatsTaken = activity.GetSeatsTaken(msgs, dc);
                Schedule = activity.GetSchedule(msgs, dc);
                Location = activity.GetLocation(msgs, dc);

                if (CanRegisterPerClass)
                    SeatsRemaining = activity.GetSeatsRemainingForNextDate(msgs, dc);
                else
                    SeatsRemaining = activity.GetSeatsRemaining(msgs, dc);

                RegistrationPeriod = activity.GetActiveRegistrationPeriod(msgs, dc);
                Form = activity.GetActiveActivityForm(msgs, dc)?.GetForm(msgs, dc);
                OwnerAccount = activity.GetAccount(msgs, dc);
                Provider = activity.GetProvider(msgs, dc);
                UseStripeConnectedAccount = PaymentAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc);
                PaymentPlans = activity.GetActivePaymentPlans(msgs, dc);
                SubscriptionPlans = activity.GetActiveSubscriptionPlans(msgs, dc);
                PostPayPlans = activity.GetActivePostPayPlans(msgs, dc);
                IsFree = IsRegistrationFree && SubscriptionPlans.Count == 0 && PaymentPlans.Count == 0 && PostPayPlans.Count == 0;
                StartDate = activity.GetFirstDate(msgs, dc);
                EndDate = activity.GetLastDate(msgs, dc);
            }
        }

        private void InitActivityGroup(ActivityGroup activityGroup, bool loadAllDetails = true, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Object = activityGroup;
            ObjectID = activityGroup.ID;
            ObjectType = ParticipationObjectType.ActivityGroup;
            Uid = activityGroup.Uid;
            Name = activityGroup.Name;
            Details = activityGroup.Details.IfBlank(activityGroup.Description);
            GenderRestriction = activityGroup.GenderRestriction;
            MinGradeId = activityGroup.MinGradeLevelId;
            MaxGradeId = activityGroup.MaxGradeLevelId;
            AgeStart = activityGroup.AgeStart;
            AgeEnd = activityGroup.AgeEnd;
            Cost = activityGroup.Cost;
            CostUnitType = activityGroup.CostUnitTypeId;
            ActivityGroupType = activityGroup.ActivityGroupTypeId;
            UniqueName = activityGroup.UniqueName;
            RegistrationMode = activityGroup.RegistrationModeId;
            ContactEmail = activityGroup.ContactEmail;
            ContactName = activityGroup.ContactName;
            ContactPhone = activityGroup.ContactPhone;
            IsRegistrationFree = !activityGroup.Cost.HasValue || activityGroup.Cost.Value == 0.ToDecimal();
            IsInviteOnly = activityGroup.IsInviteOnly;
            AllowToPayLater = activityGroup.AllowToPayLater;
            RequiresParentalConsent = activityGroup.RequiresParentalConsent;
            StartDate = activityGroup.StartDate;
            EndDate = activityGroup.EndDate;
            LastModifiedDate = activityGroup.LastModifiedDate;
            IsPublished = activityGroup.IsPublished;
            IsGenderEnforced = false;

            if (loadAllDetails)
            {
                Schedule = activityGroup.GetSchedule(msgs, dc);
                RegistrationPeriod = activityGroup.GetLatestRegistrationPeriod(msgs, dc);
                Form = activityGroup.GetForm(msgs, dc);
                OwnerAccount = activityGroup.GetAccount(msgs, dc);
                Location = OwnerAccount.GetDefaultLocation(msgs, dc);
                SeatsRemaining = activityGroup.GetSeatsRemaining(msgs, dc);
                UseStripeConnectedAccount = PaymentAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc);
                PaymentPlans = activityGroup.GetPaymentPlans(msgs, dc);
                SubscriptionPlans = activityGroup.GetSubscriptionPlans(msgs, dc);
                IsFree = IsRegistrationFree && SubscriptionPlans.Count == 0 && PaymentPlans.Count == 0;
                StartDate = activityGroup.GetFirstDate(msgs, dc);
                EndDate = activityGroup.GetLastDate(msgs, dc);
            }
        }

        private void InitAccountActivityGroup(AccountActivityGroup aag, bool loadAllDetails = true, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Object = aag;
            var activityGroup = aag.GetActivityGroup(msgs, dc);
            ObjectID = aag.ID;
            ObjectType = ParticipationObjectType.AccountActivityGroup;
            Uid = activityGroup.Uid;
            Name = activityGroup.Name;
            Details = aag.Details ?? activityGroup.Details.IfEmpty(activityGroup.Description);
            GenderRestriction = activityGroup.GenderRestriction;
            MinGradeId = aag.MinGradeLevelId ?? activityGroup.MinGradeLevelId;
            MaxGradeId = aag.MaxGradeLevelId ?? activityGroup.MaxGradeLevelId;
            AgeStart = activityGroup.AgeStart;
            AgeEnd = activityGroup.AgeEnd;
            Cost = aag.Cost;
            CostUnitType = aag.CostUnitTypeId;
            // TODO: PayLater = activityGroup.AllowToPayLater;
            ActivityGroupType = activityGroup.ActivityGroupTypeId;
            UniqueName = activityGroup.UniqueName;
            ContactName = aag.ContactName;
            ContactEmail = aag.ContactEmail;
            ContactPhone = aag.ContactPhone;
            RegistrationMode = RegistrationMode.Apply; // aag.RegistrationModeId;
            IsRegistrationFree = !aag.Cost.HasValue || aag.Cost.Value == 0.ToDecimal();
            LastModifiedDate = activityGroup.LastModifiedDate;
            IsPublished = aag.IsPublished;
            IsGenderEnforced = false;

            if (loadAllDetails)
            {
                Schedule = activityGroup.GetSchedule(msgs, dc);
                RegistrationPeriod = aag.GetLatestRegistrationPeriod(msgs, dc);
                Form = aag.GetForm(msgs, dc);
                OwnerAccount = aag.GetAccount(msgs, dc);
                Location = OwnerAccount.GetDefaultLocation(msgs, dc);
                UseStripeConnectedAccount = PaymentAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc);
                PaymentPlans = activityGroup.GetPaymentPlans(msgs, dc);
                SubscriptionPlans = activityGroup.GetSubscriptionPlans(msgs, dc);
                IsFree = IsRegistrationFree && SubscriptionPlans.Count == 0 && PaymentPlans.Count == 0;
            }
        }

        public bool AcceptsGenderType (GenderType? gender)
        {
            if (gender.IsEmpty())
                return false;
            
            return gender switch
            {
                GenderType.Male => GenderRestriction == GenderRestrictionType.MaleOnly,
                GenderType.Female => GenderRestriction == GenderRestrictionType.FemaleOnly,
                _ => false,
            };
        }

        #region QueryObjects

        public bool IsMatch(ParticipationObject participationObject)
        {
            return participationObject.ObjectID == ObjectID && participationObject.ObjectType == ObjectType;
        }

        public bool IsNotFreeAndCanAcceptPayments => !IsFree && PaymentAccount.CanProcessPayments();

        public Account PaymentAccount => Provider ?? OwnerAccount;

        public bool CanRegisterPerClass => IsActivity() && GetActivity().CostFrequencyTypeId == ActivityCostFrequencyType.PerClass;

        public string GetHomePageUrl(bool isAfterSchool = true)
        {
            switch (ObjectType)
            {
                case ParticipationObjectType.Activity:
                    return ActivityService.BuildHomePageUrl(OwnerAccount.UniqueName, ActivityType.Value, UniqueName);
                case ParticipationObjectType.ActivityGroup:
                    return ((ActivityGroup) Object).GetHomePageUrl(isAfterSchool);
                case ParticipationObjectType.AccountActivityGroup:
                    return ((AccountActivityGroup) Object).GetHomePageUrl(isAfterSchool);
            }

            return "";
        }

        public string GetBrowseUrl(bool isAfterSchool = true)
        {
            switch (ObjectType)
            {
                case ParticipationObjectType.Activity:
                    return GetHomePageUrl(isAfterSchool);
                case ParticipationObjectType.ActivityGroup:
                    return ((ActivityGroup) Object).BrowseUrl;
                case ParticipationObjectType.AccountActivityGroup:
                    return GetHomePageUrl(isAfterSchool);
            }

            return "";
        }

        public string GetRegistrationPageUrl(bool isAfterSchool = true)
        {
            return $"{GetHomePageUrl(isAfterSchool)}/register";
        }

        public string GetActivityDateSelectionUrl(bool isAfterSchool = true)
        {
            return $"{GetHomePageUrl(isAfterSchool)}/selectdates";
        }

        public bool IsSport()
        {
            if (ObjectType == ParticipationObjectType.Activity)
                return ActivityType.Value == global::ActivityType.Team;

            return ActivityGroupType == global::ActivityGroupType.League;
        }

        public (GradeLevel minGradeLevel, GradeLevel maxGradeLevel) GetMinAndMaxGradeLevels(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (MinGradeId.HasValue && MaxGradeId.HasValue)
                return (GetMinGradeLevel(msgs, dc), GetMaxGradeLevel(msgs, dc));

            return (null, null);
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
        ///     if seats remaining is null meaning no seats are set (could be a league or unlimited access)
        ///     or seats remaining > 0
        /// </summary>
        /// <returns></returns>
        public bool HasSeatsAvailable()
        {
            return !SeatsRemaining.HasValue || SeatsRemaining.Value > 0;
        }

        public string GetRegistrationThankYou(RegistrantActivity registrantActivity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (registrantActivity.AssignmentTypeId == RegistrationAssignmentType.Applied)
            {
                if (IsActivity())
                    return GetActivity().GetActivityProperty(AccountPropertyType.ApplicationThankYouEmail, msgs, dc)?.Value;
                if (IsActivityGroup())
                    return GetActivityGroup().GetGroupProperty(AccountPropertyType.ApplicationThankYouEmail, msgs, dc)?.Value;
                if (IsAccountActivityGroup())
                    return GetAccountActivityGroup()
                        .GetActivityGroup(msgs, dc)
                        .GetGroupProperty(AccountPropertyType.ApplicationThankYouEmail, msgs, dc)?
                        .Value;
            }
            else
            {
                if (ObjectType == ParticipationObjectType.Activity)
                    return GetActivity().GetActivityProperty(AccountPropertyType.RegistrationThankYouEmail, msgs, dc)?.Value;
                if (IsActivityGroup())
                    return GetActivityGroup().GetGroupProperty(AccountPropertyType.RegistrationThankYouEmail, msgs, dc)?.Value;
                if (IsAccountActivityGroup())
                    return GetAccountActivityGroup()
                        .GetActivityGroup(msgs, dc)
                        .GetGroupProperty(AccountPropertyType.RegistrationThankYouEmail, msgs, dc)?
                        .Value;
            }

            return "";
        }

        public int GetRegistrantCountByAssignmentType(RegistrationAssignmentType assignmentType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByAssignmentType(
                assignmentType,
                participationObjectId: ObjectID,
                participationObjectType: ObjectType,
                msgs: msgs,
                dc: dc);
        }

        public int GetRegistrantCountByAssignmentTypes(List<RegistrationAssignmentType> assignmentTypes, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByAssignmentTypes(
                assignmentTypes,
                participationObjectId: ObjectID,
                participationObjectType: ObjectType,
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

        public ActivityInstance GetUpcomingActivityInstance(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (IsActivity())
            {
                var instances = Schedule.GetUpcomingInstance(msgs, dc);
                return instances.FirstOrDefault();
            }

            return null;
        }

        public bool IsInSession(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (IsActivity())
                return GetActivity().IsInSession(msgs, dc);

            if (IsActivityGroup())
                return GetActivityGroup().IsInSession(msgs, dc);

            if (IsAccountActivityGroup())
                return GetAccountActivityGroup().IsInSession(msgs, dc);

            return false;
        }

        public bool IsOutOfSession(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (IsActivity())
                return GetActivity().IsOutOfSession(msgs, dc);

            if (IsActivityGroup())
                return GetActivityGroup().IsOutOfSession(msgs, dc);

            if (IsAccountActivityGroup())
                return GetAccountActivityGroup().IsOutOfSession(msgs, dc);

            return false;
        }

        public bool WillBeInSession(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (IsActivity())
                return GetActivity().WillBeInSession(msgs, dc);

            if (IsActivityGroup())
                return GetActivityGroup().WillBeInSession(msgs, dc);

            if (IsAccountActivityGroup())
                return GetAccountActivityGroup().WillBeInSession(msgs, dc);

            return false;
        }

        public bool IsStudentGradeInRange(Registrant registrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var registrantGradeLevelAnswer = registrant.GetAnswer(QuestionDataType.RegistrantGrade, msgs, dc);
            if (registrantGradeLevelAnswer.Text.IsNotEmpty())
                return IsGradeInRange(registrantGradeLevelAnswer.Text, msgs, dc);
            return false;
        }
        
        public bool IsGradeInRange(string answer, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!MinGradeId.HasValue && !MaxGradeId.HasValue)
                return true;

            var gradeLevels = GradeLevel.Load(null, null, null, msgs, dc).OrderBy(gradeLevel => gradeLevel.ID).ToList();
            if (!MinGradeId.HasValue)
                MinGradeId = gradeLevels.First().ID;

            if (!MaxGradeId.HasValue)
                MaxGradeId = gradeLevels.Last().ID;

            var registrantGradeLevel = gradeLevels.FirstOrDefault(gradeLevel => gradeLevel.VerifyGradeLevel(answer));

            if (registrantGradeLevel.IsNotEmpty() && registrantGradeLevel.ID.IsNotEmpty() &&
                registrantGradeLevel.ID.Value.IsInRange(MinGradeId.Value, MaxGradeId.Value))
                return true;

            return false;
        }

        public bool IsStudentAgeInRange(Registrant registrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var registrantDOB = registrant.GetAnswer(QuestionDataType.RegistrantDateOfBirth, msgs, dc)?.Text.ToDateTime();

            return IsAgeInRange(registrantDOB, msgs);
        }

        public bool IsAgeInRange(DateTime? dateOfBirth, SystemMessages msgs = null)
        {
            if (!AgeStart.HasValue && !AgeEnd.HasValue)
                return true;

            if (!dateOfBirth.HasValue)
            {
                msgs.Add("Missing registrant date of birth", MessageType.UserError);
                return false;
            }

            var registrantAge = dateOfBirth.Value.GetAgeInYears();

            if (registrantAge < AgeStart)
                return false;

            if (registrantAge > AgeEnd)
                return false;

            return true;
        }

        public bool RestrictByGrade()
        {
            if (IsActivity())
                return GetActivity().RestrictByGrade();

            if (IsActivityGroup())
                return GetActivityGroup().RestrictByGrade();

            return true;
        }

        public ObjectType GetWebSessionObjectType()
        {
            switch (ObjectType)
            {
                case ParticipationObjectType.AccountActivityGroup:
                    return global::ObjectType.AccountActivityGroup;

                case ParticipationObjectType.Activity:
                    return global::ObjectType.Activity;

                case ParticipationObjectType.ActivityGroup:
                    return global::ObjectType.ActivityGroup;

                default:
                    return global::ObjectType.NotSet;
            }
        }

        #endregion

        #region GetObjects

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

        public bool IsSession()
        {
            return IsActivityGroup() && GetActivityGroup().ActivityGroupTypeId == global::ActivityGroupType.Session;
        }

        public ActivityGroup GetActivityGroup()
        {
            if (IsActivityGroup())
                return (ActivityGroup) Object;

            return null;
        }

        public AccountActivityGroup GetAccountActivityGroup()
        {
            if (IsAccountActivityGroup())
                return (AccountActivityGroup) Object;

            return null;
        }

        public Activity GetActivity()
        {
            if (IsActivity())
                return (Activity) Object;

            return null;
        }

        public int? GetActivityGroupId()
        {
            return IsActivityGroup() ? ObjectID : null;
        }

        public int? GetAnyRelatedActivityGroupId()
        {
            if (IsAccountActivityGroup())
                return GetAccountActivityGroup().ActivityGroupId;
            if (IsActivityGroup())
                return GetActivityGroupId();
            if (IsActivity())
                return GetActivity().ActivityGroupId;

            return new int?();
        }

        public int? GetAccountActivityGroupId()
        {
            return IsAccountActivityGroup() ? ObjectID : null;
        }

        public int? GetAnyRelatedAccountActivityGroupId()
        {
            if (IsAccountActivityGroup())
                return GetAccountActivityGroupId();
            if (IsActivity())
                return GetActivity().AccountActivityGroupId;

            return new int?();
        }

        public int? GetActivityId()
        {
            return IsActivity() ? ObjectID : null;
        }

        public int? GetId()
        {
            return GetActivityId() ?? GetActivityGroupId() ?? GetAccountActivityGroupId();
        }

        public Subscriber AddOrUpdateSubscriber(WebUser user, SubscriberStatus status, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var subscriber = Subscriber.LoadByObjectIdByObjectTypeIdByWebUserId(
                ObjectID.Value,
                ObjectType.ToAHQObjectType(),
                user.ID.Value,
                msgs,
                dc);

            if (subscriber.IsEmpty())
                return SubscriberService.CreateNew(this, user, status, loggedInUser, msgs, dc);

            subscriber.UpdateSubscriber(status, loggedInUser, msgs, dc);

            return subscriber;
        }

        public bool HasSubscriptionPlans()
        {
            return SubscriptionPlans.IsNotEmpty();
        }

        public SubscriptionPlan GetSubscriptionPlan()
        {
            if (HasSubscriptionPlans())
                return SubscriptionPlans.FirstOrDefault();

            return null;
        }

        public bool HasPostPayPlan()
        {
            return PostPayPlans.IsNotEmpty();
        }

        public SubscriptionPlan GetPostPayPlan()
        {
            if (HasPostPayPlan())
                return PostPayPlans.FirstOrDefault();

            return null;
        }

        public bool IsExternallyProvided()
        {
            return Provider.IsNotEmpty();
        }

        public bool IsExternalToAccount(Account account)
        {
            if (IsExternallyProvided())
                return Provider.ID != account.ID;

            if (Location.IsNotEmpty())
                return Location.HostAccountId == account.ID && OwnerAccount.ID != account.ID;

            return false;
        }

        public bool IsExternalProvider(Account account)
        {
            if (IsExternallyProvided())
                return Provider.ID == account.ID;

            return false;
        }

        #endregion

        #region Financial

        public bool ShouldParentPay()
        {
            var subscriptionCheck = !SubscriptionPlans.Any() || !SubscriptionPlans.First().SplitCostEvenly || !IsRegistrationFree;

            return !IsFree &&
                   RegistrationMode == RegistrationMode.Register &&
                   SeatsRemaining > 0 &&
                   subscriptionCheck;
        }

        public decimal GetAmountPaidByRegistrant(int registrantId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return ActivityPayment.GetTotalPaidByParticipationObjectByRegistrant(
                ObjectID.Value,
                ObjectType,
                registrantId,
                false,
                msgs,
                dc);
        }

        public ActivityPayment AddPayment(decimal amount, Registrant registrant, Transaction transaction, RegistrantSubscriptionPlan registrantSubscriptionPlan,
            DateTime createDate, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var payment = new ActivityPayment
            {
                ParticipationObjectId = ObjectID,
                ParticipationObjectTypeId = ObjectType,
                Amount = amount,
                RegistrantId = registrant?.ID,
                AccountId = PaymentAccount?.ID,
                TransactionId = transaction?.ID,
                RegistrantSubscriptionPlanId = registrantSubscriptionPlan?.ID,
                CreateDate = createDate,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = createDate,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
            };

            msgs.AddRange(payment.Save(dc));

            return payment;
        }

        public ActivityPayment AddPayment(decimal amountReceivedByProvider, decimal amountPaidByParent, bool isAmountProRated, Registrant registrant,
            Transaction transaction, RegistrantSubscriptionPlan registrantSubscriptionPlan, DateTime createDate, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            var payment = new ActivityPayment
            {
                ParticipationObjectId = ObjectID,
                ParticipationObjectTypeId = ObjectType,
                Amount = amountReceivedByProvider,
                AmountPaidByParent = amountPaidByParent,
                IsProRated = isAmountProRated,
                RegistrantId = registrant?.ID,
                AccountId = PaymentAccount?.ID,
                TransactionId = transaction?.ID,
                RegistrantSubscriptionPlanId = registrantSubscriptionPlan?.ID,
                CreateDate = createDate,
                CreatedBy = loggedInUser?.ID.IfEmpty(0),
                LastModifiedDate = createDate,
                LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
            };

            msgs.AddRange(payment.Save(dc));

            return payment;
        }

        #region "PostPayPlan"

        public decimal GetPostPayCost()
        {
            if (PostPayPlans.IsNotEmpty() && PostPayPlans.Any() && PostPayPlans.First().CostPerPeriod.HasValue)
                return PostPayPlans.First().CostPerPeriod.Value;

            return 0;
        }

        public decimal GetPostPayCostWithConvenienceFee()
        {
            var plan = PostPayPlans?.FirstOrDefault();

            if (plan.IsNotEmpty())
                return GetPostPayCost() + (plan.IsConvenienceFeeTakenOutOfCost ? 0 : GetPostPayConvenienceFee());

            return 0;
        }

        public decimal GetPostPayProviderAmount()
        {
            var plan = PostPayPlans?.FirstOrDefault();

            if (plan.IsNotEmpty() && plan.IsConvenienceFeeTakenOutOfCost)
                return GetPostPayCost() - GetPostPayConvenienceFee();

            return GetPostPayCost();
        }

        public decimal GetPostPayConvenienceFee()
        {
            var plan = PostPayPlans?.FirstOrDefault();

            if (plan.IsNotEmpty())
                return new FeeCalculationObject(
                        GetPostPayCost(),
                        OwnerAccount,
                        plan.IsConvenienceFeeTakenOutOfCost)
                    .GetConvenienceFee();

            return 0;
        }

        public decimal GetPostPayCreditCardFee()
        {
            var plan = PostPayPlans?.FirstOrDefault();

            if (plan.IsNotEmpty())
                return new FeeCalculationObject(
                        GetPostPayCost(),
                        PaymentAccount,
                        plan.IsConvenienceFeeTakenOutOfCost)
                    .GetCreditCardFee();

            return 0;
        }

        public decimal GetCheckoutPostPayCreditCardFee()
        {
            var plan = PostPayPlans?.FirstOrDefault();

            if ((plan?.NumberOfPeriodsPrePaid).IsPositive())
                return GetPostPayCreditCardFee() * plan.NumberOfPeriodsPrePaid.Value;

            return GetPostPayCreditCardFee();
        }

        public decimal GetCheckoutPostPayConvenienceFee()
        {
            var plan = PostPayPlans?.FirstOrDefault();

            if ((plan?.NumberOfPeriodsPrePaid).IsPositive())
                return GetPostPayConvenienceFee() * plan.NumberOfPeriodsPrePaid.Value;

            return GetPostPayConvenienceFee();
        }

        public decimal GetPostPayCheckoutCostWithConvenienceFee()
        {
            var plan = PostPayPlans?.FirstOrDefault();

            if ((plan?.NumberOfPeriodsPrePaid).IsPositive())
                return plan.NumberOfPeriodsPrePaid.Value * GetPostPayCostWithConvenienceFee();

            return GetPostPayCostWithConvenienceFee();
        }

        public decimal GetPostPayCheckoutCostWithoutConvenienceFee()
        {
            var plan = PostPayPlans?.FirstOrDefault();

            if ((plan?.NumberOfPeriodsPrePaid).IsPositive())
                return plan.NumberOfPeriodsPrePaid.Value * GetPostPayCost();

            return GetPostPayCost();
        }

        #endregion

        public decimal GetCheckoutCostWithConvenienceFee(int quantity = 1)
        {
            if (PostPayPlans.IsNotEmpty() && PostPayPlans.Any())
                return GetPostPayCheckoutCostWithConvenienceFee();

            if (Cost.HasValue)
            {
                if (IsActivity())
                {
                    var activity = GetActivity();

                    if (activity.IsNotEmpty())
                        return (Cost.Value
                                + (activity.IsConvenienceFeeTakenOutOfCost ? 0 : GetConvenienceFee())) * quantity;
                }

                return (Cost.Value + GetConvenienceFee()) * quantity;
            }

            return 0;
        }

        public decimal GetCheckoutTotalCost(int quantity = 1)
        {
            if (Cost.HasValue)
                return GetCheckoutCostWithConvenienceFee(quantity) + GetCreditCardFee(quantity);

            return 0;
        }

        public decimal GetCreditCardFee(int quantity = 1)
        {
            if (Cost.HasValue)
                return new FeeCalculationObject(
                        Cost.Value,
                        PaymentAccount,
                        this)
                    .GetCreditCardFee() * quantity;

            return 0;
        }

        public decimal GetConvenienceFee(int quantity = 1)
        {
            if (Cost.HasValue)
                return new FeeCalculationObject(
                        Cost.Value,
                        OwnerAccount,
                        this)
                    .GetConvenienceFee() * quantity;

            return 0;
        }

        /// <summary>
        ///     use-case: simple scenario is classes are from Oct 1 - Dec 31, and someone signs up Nov 1, they should only have to pay for Nov 1 - Dec 31st.
        ///     We'd have to calculate the # of payments based on total periods and total activity instances
        /// </summary>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        public int? GetNumberOfRemainingPayments(int totalPeriods, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            if (totalPeriods > 0 && Schedule.IsNotEmpty() && Schedule.ID.HasValue)
            {
                var activityInstances = ActivityInstance.LoadByActivityScheduleId(Schedule.ID.Value, msgs);
                var upcomingInstances = Schedule.GetUpcomingInstance(msgs);

                var numberofPeriodsToBePaid = upcomingInstances.Count * totalPeriods / activityInstances.Count;
                return Convert.ToInt32(Math.Ceiling(numberofPeriodsToBePaid.ToDecimal()));
            }

            return null;
        }

        public int GetNumberOfClassesInCurrentCycle(BillingPeriodType billingPeriodTypeId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var classes = 0;
            if (Schedule.IsNotEmpty() && Schedule.ID.HasValue)
            {
                if (billingPeriodTypeId == BillingPeriodType.Month)
                    return ActivityInstance
                        .LoadByActivityScheduleIdByDateRange(Schedule.ID.Value, DateTime.Now.FirstDateOfMonth(), DateTime.Now.LastDateOfMonth(), msgs, dc)
                        .Count;
                if (billingPeriodTypeId == BillingPeriodType.Week)
                    return ActivityInstance
                        .LoadByActivityScheduleIdByDateRange(Schedule.ID.Value, DateTime.Now.FirstDateOfWeek(), DateTime.Now.LastDateOfWeek(), msgs, dc).Count;
            }

            return classes;
        }

        public int GetNumberOfRemainingClassesInCurrentCycle(BillingPeriodType billingPeriodTypeId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var classes = 0;
            if (Schedule.IsNotEmpty() && Schedule.ID.HasValue)
            {
                // Add one day because last day return datetime at beginning of day (12:00 am) 
                if (billingPeriodTypeId == BillingPeriodType.Month)
                    return ActivityInstance
                        .LoadByActivityScheduleIdByDateRange(Schedule.ID.Value, DateTime.UtcNow, DateTime.UtcNow.LastDateOfMonth().AddDays(1), msgs, dc).Count;
                if (billingPeriodTypeId == BillingPeriodType.Week)
                    return ActivityInstance
                        .LoadByActivityScheduleIdByDateRange(Schedule.ID.Value, DateTime.UtcNow, DateTime.UtcNow.LastDateOfWeek().AddDays(1), msgs, dc).Count;
            }

            return classes;
        }

        public int GetNumberOfRemainingClassesInNextCycle(BillingPeriodType billingPeriodTypeId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (Schedule.IsNotEmpty() && Schedule.ID.HasValue)
            {
                if (billingPeriodTypeId == BillingPeriodType.Month)
                    return ActivityInstance.LoadByActivityScheduleIdByDateRange(Schedule.ID.Value, DateTime.Now.AddMonths(1).FirstDateOfMonth(),
                        DateTime.Now.AddMonths(1).LastDateOfMonth(), msgs, dc).Count;
                if (billingPeriodTypeId == BillingPeriodType.Week)
                    return ActivityInstance.LoadByActivityScheduleIdByDateRange(Schedule.ID.Value, DateTime.Now.AddWeeks(1).FirstDateOfWeek(),
                        DateTime.Now.AddWeeks(1).LastDateOfWeek(), msgs, dc).Count;
            }

            return 0;
        }

        #endregion

        #region DemoObjects

        private static List<ParticipationObject> GetDemoActivities(List<GradeLevel> grades)
        {
            var activities = new List<ParticipationObject>();

            activities.Add(new ParticipationObject(global::ActivityType.Activity)
            {
                Name = "Chess Club",
                Details =
                    "<p>Chess is a fun game that incorporates many aspects of the IB learner profile: knowledgeable, inquirers, thinkers, communicators, principled, open-minded, caring, risk-takers, balanced, and reflective. The chess club is meant to foster scholar’s intellectual growth while having fun.</p>",
                ContactName = "Mrs Fisher", Cost = 25, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[0].ID, MaxGradeId = grades[4].ID,
                SeatsRemaining = 15, LocationDetails = "Room 110",
            });

            activities.Add(new ParticipationObject(global::ActivityType.Activity)
            {
                Name = "Robotics",
                Details =
                    "<p>Spend the semester learning about robots while we experiment and play with them.  We will we discuss what makes robots different from humans while exploring circuits and conductivity as well as simple machines.  Over the course of the semester each participant will build one of these robots.  Every semester we build a differnet robotics kit than the last semester.  Unlike other robotic clubs, each child will get to keep their completed robot at the end of the semester to show off to their friends!</p>",
                ContactName = "Mr Johnson", Cost = 150, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[7].ID, MaxGradeId = grades[9].ID,
                SeatsRemaining = 3, LocationDetails = "Cafeteria",
            });

            activities.Add(new ParticipationObject(global::ActivityType.Activity)
            {
                Name = "Theater Club",
                Details =
                    "<p>Theater Club is an ongoing workshop program that runs throughout the school year. Participants learn and practice skills related to Musical Theatre (acting, singing, light choreography, and staging) through instruction, theatre games, exercises and activities designed to teach while having fun.</p>",
                ContactName = "Mrs Campbell", Cost = 65, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[7].ID, MaxGradeId = grades[9].ID,
                SeatsRemaining = 11, LocationDetails = "Auditorium",
            });

            activities.Add(new ParticipationObject(global::ActivityType.Activity)
            {
                Name = "Ham Radio Club",
                Details =
                    "<p>The Amateur Radio Club helps students, faculty, and staff prepare for Federal Communications Commission license exams. We learn Morse code, basic electronics, and rules and regulations governing ham radio. We operate various 2-way radios to talk to people both locally and globally, and train to be emergency and disaster communications volunteers. Anyone interested in ham radio is welcome to attend.</p>",
                ContactName = "Mr Jackson", Cost = 10, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[10].ID, MaxGradeId = grades[13].ID,
                SeatsRemaining = 20, LocationDetails = "Communication Room",
            });

            activities.Add(new ParticipationObject(global::ActivityType.Activity)
            {
                Name = "SADD",
                Details =
                    "<p>SADD's mission is to provide students with the best prevention and intervention tools possible to deal with the issues of underage drinking, drunk driving, drug abuse, and other destructive decisions.</p>",
                ContactName = "Mrs Wellington", Cost = 0, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[10].ID,
                MaxGradeId = grades[13].ID, SeatsRemaining = 34, LocationDetails = "Room 202",
            });

            activities.Add(new ParticipationObject(global::ActivityType.Activity)
            {
                Name = "Backpacking Club",
                Details =
                    "<p>Backpacking club is an informal group of students and teachers who enjoy hiking and camping. Trips are overnight, usually leaving Saturday about 9 A.M. and returning Sunday about 1 P.M. Equipment can be borrowed or rented. No experience is necessary.</p>",
                ContactName = "Mrs Flanagan", Cost = 110, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[10].ID,
                MaxGradeId = grades[13].ID, SeatsRemaining = 12, LocationDetails = "Football Field",
            });

            activities.Add(new ParticipationObject(global::ActivityType.Activity)
            {
                Name = "Investment Club",
                Details =
                    "<p>Students will examine the core aspects of the stock market and how it functions.  Various websites (yahoo finance, smartmoney, bloomberg, etc.) will be utilized to assist students in gathering key information that will be helpful in making investment decisions.  Students will participate in the a stock market game that is sponsored by the New York Times.  The stock market game will provide students with an imaginary $100,000 that will be invested and monitored throughout an eight month period.  Continuously updated standings will provide students with their investment ranking, and the highest earning teams will be awarded prizes at the conclusion.</p>",
                ContactName = "Mrs Wilson", Cost = 25, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[10].ID, MaxGradeId = grades[13].ID,
                SeatsRemaining = 7, LocationDetails = "Room 105",
            });

            activities.Add(new ParticipationObject(global::ActivityType.Activity)
            {
                Name = "Boys Vocal Music Club",
                Details =
                    "<p>Vocal Music Club is an after-school club for all students that enjoy singing.  Whether you sign up for Chorus, Band, Orchestra or Exploring Music, being in this club will give you the opportunity to sing many different styles of music, either alone or as part of an ensemble, on a weekly basis.</p>",
                ContactName = "Mr Temple", Cost = 0, GenderRestriction = GenderRestrictionType.MaleOnly, MinGradeId = grades[4].ID, MaxGradeId = grades[7].ID,
                SeatsRemaining = 2, LocationDetails = "Music Room 1",
            });

            activities.Add(new ParticipationObject(global::ActivityType.Activity)
            {
                Name = "Garden Club",
                Details =
                    "<p>Students participate in garden projects on a drop-in basis.  Projects include: preparing garden beds for planting, planting seeds and seedlings, watering, maintaining the compost pile, weeding and harvesting.</p>",
                ContactName = "Mr Wendell", Cost = 0, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[0].ID, MaxGradeId = grades[3].ID,
                SeatsRemaining = 0, LocationDetails = "Garden Center",
            });

            activities.Add(new ParticipationObject(global::ActivityType.Activity)
            {
                Name = "Art Club",
                Details =
                    "<p>In Art Club we make bowls, mugs, plaques, and other sculptures all of which are glazed and fired in the kiln.  All grades are welcome.  You do not have to be in art to join the art club.  Come down and see what it is all about! </p>",
                ContactName = "Mrs Apple", Cost = 5, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[2].ID, MaxGradeId = grades[5].ID,
                SeatsRemaining = 13, LocationDetails = "Room 285",
            });

            activities.Add(new ParticipationObject(global::ActivityType.Activity)
            {
                Name = "Girls Spikeball Club",
                Details =
                    "<p>Students in this club love the game of Spikeball.  Spikeball is a modified version of 2v2 volleyball. Like volleyball, the net and ball is replaced with a small trampoline-like object with string netting and a small bouncing ball (usually yellow) with a 12-inch circumference.  Games are played to 11, 15 or 21. Spikeball is also great for any age group and very easy to learn!  There will be pickup games and tournaments when the club meets.  Basic rules and strategies will be taught to all beginners that join the club.</p>",
                ContactName = "Mr Ganderson", Cost = 45, GenderRestriction = GenderRestrictionType.FemaleOnly, MinGradeId = grades[10].ID,
                MaxGradeId = grades[11].ID, SeatsRemaining = 17, LocationDetails = "Gym",
            });

            activities.Add(new ParticipationObject(global::ActivityType.Activity)
            {
                Name = "Career Club",
                Details =
                    "<p>Members explore their interests to discover careers that may match them. Students research the academic, technical, and experiential requirements for those careers, so they can prepare for life after school. The club provides many resources to help students become career ready.</p>",
                ContactName = "Mrs Bethel", Cost = 0, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[12].ID, MaxGradeId = grades[13].ID,
                SeatsRemaining = 6, LocationDetails = "Room 150",
            });

            return activities;
        }

        public static void GenerateDemoObjects(ref Account account, List<ParticipationObject> participationObjects, List<Location> locations,
            string accountName, string accountUniqueName, List<GradeLevel> grades)
        {
            account = new Account
            {
                Name = accountName,
                UniqueName = accountUniqueName,
                IsActive = true,
                Uid = Guid.NewGuid(),
            };

            var reg = new Regex(BasicLibrary.Utilities.UrlFriendlyRegEx, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var schedule = ActivitySchedule.GetNewDefaultSchedule(generateDates: true);
            var currentAccount = account;

            locations.Add(new Location
            {
                Name = "Launch Fishers",
                Address1 = "12175 Visionary Way",
                Address2 = "#710",
                City = "Fishers",
                State = "IN",
                PostalCode = "46038",
                Country = "US",
            });

            var registrationPeriod = new RegistrationPeriod
            {
                StartDateTime = DateTime.UtcNow.Date,
                EndDateTime = DateTime.UtcNow.Date.AddDays(2).AddMinutes(-1),
            };

            // load demo account
            participationObjects.AddRange(GetDemoActivities(grades));
            participationObjects.ForEach(participationObject =>
            {
                var uniqueName = reg.Replace(participationObject.Name.GetMaxWords(25), "");

                participationObject.OwnerAccount = currentAccount;
                participationObject.Schedule = schedule;
                participationObject.Description = "";
                participationObject.UniqueName = uniqueName;
                participationObject.AllowToPayLater = true;
                participationObject.ContactEmail = "hello@afterschoolhq.com";
                participationObject.ContactPhone = "1-888-231-1724";
                participationObject.CostUnitType = global::CostUnitType.PerRegistrant;
                participationObject.Location = locations.First();
                participationObject.PaymentPlans = new List<SubscriptionPlan>();
                participationObject.SubscriptionPlans = new List<SubscriptionPlan>();
                participationObject.RegistrationMode = RegistrationMode.Register;
                participationObject.RegistrationPeriod = registrationPeriod;
                participationObject.Uid = Guid.NewGuid();
                participationObject.IsFree = participationObject.Cost == 0;
                participationObject.IsRegistrationFree = participationObject.Cost == 0;
                participationObject.ObjectType = ParticipationObjectType.Activity;
            });
        }

        #endregion
    }
}
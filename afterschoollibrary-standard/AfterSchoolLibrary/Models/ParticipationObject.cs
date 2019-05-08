using System;
using System.Linq;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
        public Account Provider { get; set; }
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
        public bool UseStripeConnectedAccount { get; set; }

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
            Form = activity.GetActiveActivityForm(msgs, dc)?.GetForm(msgs, dc);

            OwnerAccount = activity.GetAccount(msgs, dc);
            Provider = activity.GetProvider(msgs, dc);

            UseStripeConnectedAccount = PaymentAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc);

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
            UseStripeConnectedAccount = PaymentAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc);

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
            UseStripeConnectedAccount = PaymentAccount.HasFeature(FeatureType.UseStripeConnectedAccount, msgs, dc);

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

        public ParticipationObject(ActivityType activityType)
        {
            this.ActivityType = activityType;
        }

        public bool IsNotFreeAndCanAcceptPayments
        {
            get
            {
                return !IsFree && PaymentAccount.CanProcessPayments();
            } 
        }

        public Account PaymentAccount
        {
            get
            {
                return Provider ?? OwnerAccount;
            }
            
        }

        public string GetHomePageUrl(bool isAfterSchool = true)
        {
            switch (this.ObjectType)
            {
                case ParticipationObjectType.Activity:
                    return ActivityService.BuildHomePageUrl(OwnerAccount.UniqueName, ActivityType.Value, UniqueName);
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

        public int? GetId()
        {
            return GetActivityId() ?? GetActivityGroupId() ?? GetAccountActivityGroupId();
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
                AccountId = this.PaymentAccount.ID,
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

        public decimal GetCheckoutTotalCost()
        {
            if (this.Cost.HasValue)
                return Cost.Value
                    + GetCreditCardFee(FeesPaidByType.Parent)
                    + GetConvenienceFee(FeesPaidByType.Parent);

            return 0;
        }

        public decimal GetCreditCardFee(FeesPaidByType feesPaidByType)
        {
            if (Cost.HasValue && PaymentAccount.CreditCardFeesPaidBy == feesPaidByType && !UseStripeConnectedAccount)
                return Billing.Platform.GetCreditCardFee(Cost.Value, PaymentAccount);

            return 0;
        }

        public decimal GetConvenienceFee(FeesPaidByType feesPaidByType)
        {
            if (Cost.HasValue && PaymentAccount.ConvenienceFeesPaidBy == feesPaidByType && !UseStripeConnectedAccount)
                return Billing.Platform.GetConvenienceFee(Cost.Value, PaymentAccount);

            return 0;
        }

        private static List<ParticipationObject> GetDemoActivities(List<GradeLevel> grades)
        {
            var activities = new List<ParticipationObject>();

            activities.Add(new ParticipationObject(global::ActivityType.Activity) { Name = "Chess Club", Details = "<p>Chess is a fun game that incorporates many aspects of the IB learner profile: knowledgeable, inquirers, thinkers, communicators, principled, open-minded, caring, risk-takers, balanced, and reflective. The chess club is meant to foster scholar’s intellectual growth while having fun.</p>", ContactName = "Mrs Fisher", Cost = 25, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[0].ID, MaxGradeId = grades[4].ID, SeatsRemaining = 15, LocationDetails = "Room 110" });

            activities.Add(new ParticipationObject(global::ActivityType.Activity) { Name = "Robotics", Details = "<p>Spend the semester learning about robots while we experiment and play with them.  We will we discuss what makes robots different from humans while exploring circuits and conductivity as well as simple machines.  Over the course of the semester each participant will build one of these robots.  Every semester we build a differnet robotics kit than the last semester.  Unlike other robotic clubs, each child will get to keep their completed robot at the end of the semester to show off to their friends!</p>", ContactName = "Mr Johnson", Cost = 150, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[7].ID, MaxGradeId = grades[9].ID, SeatsRemaining = 3, LocationDetails = "Cafeteria" });

            activities.Add(new ParticipationObject(global::ActivityType.Activity) { Name = "Theater Club", Details = "<p>Theater Club is an ongoing workshop program that runs throughout the school year. Participants learn and practice skills related to Musical Theatre (acting, singing, light choreography, and staging) through instruction, theatre games, exercises and activities designed to teach while having fun.</p>", ContactName = "Mrs Campbell", Cost = 65, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[7].ID, MaxGradeId = grades[9].ID, SeatsRemaining = 11, LocationDetails = "Auditorium" });

            activities.Add(new ParticipationObject(global::ActivityType.Activity) { Name = "Ham Radio Club", Details = "<p>The Amateur Radio Club helps students, faculty, and staff prepare for Federal Communications Commission license exams. We learn Morse code, basic electronics, and rules and regulations governing ham radio. We operate various 2-way radios to talk to people both locally and globally, and train to be emergency and disaster communications volunteers. Anyone interested in ham radio is welcome to attend.</p>", ContactName = "Mr Jackson", Cost = 10, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[10].ID, MaxGradeId = grades[13].ID, SeatsRemaining = 20, LocationDetails = "Communication Room" });

            activities.Add(new ParticipationObject(global::ActivityType.Activity) { Name = "SADD", Details = "<p>SADD's mission is to provide students with the best prevention and intervention tools possible to deal with the issues of underage drinking, drunk driving, drug abuse, and other destructive decisions.</p>", ContactName = "Mrs Wellington", Cost = 0, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[10].ID, MaxGradeId = grades[13].ID, SeatsRemaining = 34, LocationDetails = "Room 202" });

            activities.Add(new ParticipationObject(global::ActivityType.Activity) { Name = "Backpacking Club", Details = "<p>Backpacking club is an informal group of students and teachers who enjoy hiking and camping. Trips are overnight, usually leaving Saturday about 9 A.M. and returning Sunday about 1 P.M. Equipment can be borrowed or rented. No experience is necessary.</p>", ContactName = "Mrs Flanagan", Cost = 110, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[10].ID, MaxGradeId = grades[13].ID, SeatsRemaining = 12, LocationDetails = "Football Field" });

            activities.Add(new ParticipationObject(global::ActivityType.Activity) { Name = "Investment Club", Details = "<p>Students will examine the core aspects of the stock market and how it functions.  Various websites (yahoo finance, smartmoney, bloomberg, etc.) will be utilized to assist students in gathering key information that will be helpful in making investment decisions.  Students will participate in the a stock market game that is sponsored by the New York Times.  The stock market game will provide students with an imaginary $100,000 that will be invested and monitored throughout an eight month period.  Continuously updated standings will provide students with their investment ranking, and the highest earning teams will be awarded prizes at the conclusion.</p>", ContactName = "Mrs Wilson", Cost = 25, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[10].ID, MaxGradeId = grades[13].ID, SeatsRemaining = 7, LocationDetails = "Room 105" });

            activities.Add(new ParticipationObject(global::ActivityType.Activity) { Name = "Boys Vocal Music Club", Details = "<p>Vocal Music Club is an after-school club for all students that enjoy singing.  Whether you sign up for Chorus, Band, Orchestra or Exploring Music, being in this club will give you the opportunity to sing many different styles of music, either alone or as part of an ensemble, on a weekly basis.</p>", ContactName = "Mr Temple", Cost = 0, GenderRestriction = GenderRestrictionType.MaleOnly, MinGradeId = grades[4].ID, MaxGradeId = grades[7].ID, SeatsRemaining = 2, LocationDetails = "Music Room 1" });

            activities.Add(new ParticipationObject(global::ActivityType.Activity) { Name = "Garden Club", Details = "<p>Students participate in garden projects on a drop-in basis.  Projects include: preparing garden beds for planting, planting seeds and seedlings, watering, maintaining the compost pile, weeding and harvesting.</p>", ContactName = "Mr Wendell", Cost = 0, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[0].ID, MaxGradeId = grades[3].ID, SeatsRemaining = 0, LocationDetails = "Garden Center" });

            activities.Add(new ParticipationObject(global::ActivityType.Activity) { Name = "Art Club", Details = "<p>In Art Club we make bowls, mugs, plaques, and other sculptures all of which are glazed and fired in the kiln.  All grades are welcome.  You do not have to be in art to join the art club.  Come down and see what it is all about! </p>", ContactName = "Mrs Apple", Cost = 5, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[2].ID, MaxGradeId = grades[5].ID, SeatsRemaining = 13, LocationDetails = "Room 285" });

            activities.Add(new ParticipationObject(global::ActivityType.Activity) { Name = "Girls Spikeball Club", Details = "<p>Students in this club love the game of Spikeball.  Spikeball is a modified version of 2v2 volleyball. Like volleyball, the net and ball is replaced with a small trampoline-like object with string netting and a small bouncing ball (usually yellow) with a 12-inch circumference.  Games are played to 11, 15 or 21. Spikeball is also great for any age group and very easy to learn!  There will be pickup games and tournaments when the club meets.  Basic rules and strategies will be taught to all beginners that join the club.</p>", ContactName = "Mr Ganderson", Cost = 45, GenderRestriction = GenderRestrictionType.FemaleOnly, MinGradeId = grades[10].ID, MaxGradeId = grades[11].ID, SeatsRemaining = 17, LocationDetails = "Gym" });

            activities.Add(new ParticipationObject(global::ActivityType.Activity) { Name = "Career Club", Details = "<p>Members explore their interests to discover careers that may match them. Students research the academic, technical, and experiential requirements for those careers, so they can prepare for life after school. The club provides many resources to help students become career ready.</p>", ContactName = "Mrs Bethel", Cost = 0, GenderRestriction = GenderRestrictionType.None, MinGradeId = grades[12].ID, MaxGradeId = grades[13].ID, SeatsRemaining = 6, LocationDetails = "Room 150" });

            return activities;
        }

        public static void GenerateDemoObjects(ref Account account, List<ParticipationObject> participationObjects, List<Location> locations, string accountName, string accountUniqueName, List<GradeLevel> grades)
        {
            account = new Account()
            {
                Name = accountName,
                UniqueName = accountUniqueName,
                IsActive = true,
                Uid = Guid.NewGuid()
            };

            var reg = new Regex(BasicLibrary.Utilities.UrlFriendlyRegEx, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var schedule = ActivitySchedule.GetNewDefaultSchedule(generateDates: true);
            var currentAccount = account;

            locations.Add(new Location()
            {
                Name = "Launch Fishers",
                Address1 = "12175 Visionary Way",
                Address2 = "#710",
                City = "Fishers",
                State = "IN",
                PostalCode = "46038",
                Country = "US"
            });

            var registrationPeriod = new RegistrationPeriod()
            {
                StartDateTime = DateTime.UtcNow.Date,
                EndDateTime = DateTime.UtcNow.Date.AddDays(2).AddMinutes(-1)
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
                participationObject.ContactEmail = $"hello@afterschoolhq.com";
                participationObject.ContactPhone = "1-888-231-1724";
                participationObject.CostUnitType = global::CostUnitType.PerRegistrant;
                participationObject.Location = locations.First();
                participationObject.PaymentPlans = new List<SubscriptionPlan>();
                participationObject.SubscriptionPlans = new List<SubscriptionPlan>();
                participationObject.WaitingListSeatsRemaining = 10;
                participationObject.RegistrationMode = RegistrationMode.Register;
                participationObject.RegistrationPeriod = registrationPeriod;
                participationObject.Uid = Guid.NewGuid();
                participationObject.IsFree = participationObject.Cost == 0;
                participationObject.IsTuitionFree = participationObject.Cost == 0;
                participationObject.ObjectType = ParticipationObjectType.Activity;
            });
        }
    }
}

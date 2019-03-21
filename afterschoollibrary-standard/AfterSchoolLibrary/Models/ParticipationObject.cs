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

        public ParticipationObject(ActivityType activityType)
        {
            this.ActivityType = activityType;
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

        private static List<Tuple<string, string>> GetDemoActivities()
        {
            var activities = new List<Tuple<string, string>>();

            activities.Add(new Tuple<string, string>("Chess Club", "<p>Chess is a fun game that incorporates many aspects of the IB learner profile: knowledgeable, inquirers, thinkers, communicators, principled, open-minded, caring, risk-takers, balanced, and reflective. The chess club is meant to foster scholar’s intellectual growth while having fun.</p>"));

            activities.Add(new Tuple<string, string>("Robotics", "<p>Spend the semester learning about robots while we experiment and play with them.  We will we discuss what makes robots different from humans while exploring circuits and conductivity as well as simple machines.  Over the course of the semester each participant will build one of these robots.  Every semester we build a differnet robotics kit than the last semester.  Unlike other robotic clubs, each child will get to keep their completed robot at the end of the semester to show off to their friends!</p>"));

            activities.Add(new Tuple<string, string>("Theater Club", "<p>Theater Club is an ongoing workshop program that runs throughout the school year. Participants learn and practice skills related to Musical Theatre (acting, singing, light choreography, and staging) through instruction, theatre games, exercises and activities designed to teach while having fun.</p>"));

            activities.Add(new Tuple<string, string>("Ham Radio Club", "<p>The Amateur Radio Club helps students, faculty, and staff prepare for Federal Communications Commission license exams. We learn Morse code, basic electronics, and rules and regulations governing ham radio. We operate various 2-way radios to talk to people both locally and globally, and train to be emergency and disaster communications volunteers. Anyone interested in ham radio is welcome to attend.</p>"));

            activities.Add(new Tuple<string, string>("SADD", "<p>SADD's mission is to provide students with the best prevention and intervention tools possible to deal with the issues of underage drinking, drunk driving, drug abuse, and other destructive decisions.</p>"));

            activities.Add(new Tuple<string, string>("Backpacking Club", "<p>Backpacking club is an informal group of students and teachers who enjoy hiking and camping. Trips are overnight, usually leaving Saturday about 9 A.M. and returning Sunday about 1 P.M. Equipment can be borrowed or rented. No experience is necessary.</p>"));

            activities.Add(new Tuple<string, string>("Investment Club", "<p>Students will examine the core aspects of the stock market and how it functions.  Various websites (yahoo finance, smartmoney, bloomberg, etc.) will be utilized to assist students in gathering key information that will be helpful in making investment decisions.  Students will participate in the a stock market game that is sponsored by the New York Times.  The stock market game will provide students with an imaginary $100,000 that will be invested and monitored throughout an eight month period.  Continuously updated standings will provide students with their investment ranking, and the highest earning teams will be awarded prizes at the conclusion.</p>"));

            activities.Add(new Tuple<string, string>("Vocal Music Club", "<p>Vocal Music Club is an after-school club for all students that enjoy singing.  Whether you sign up for Chorus, Band, Orchestra or Exploring Music, being in this club will give you the opportunity to sing many different styles of music, either alone or as part of an ensemble, on a weekly basis.</p>"));

            activities.Add(new Tuple<string, string>("Garden Club", "<p>Students participate in garden projects on a drop-in basis.  Projects include: preparing garden beds for planting, planting seeds and seedlings, watering, maintaining the compost pile, weeding and harvesting.</p>"));

            activities.Add(new Tuple<string, string>("Art Club", "<p>In Art Club we make bowls, mugs, plaques, and other sculptures all of which are glazed and fired in the kiln.  All grades are welcome.  You do not have to be in art to join the art club.  Come down and see what it is all about! </p>"));

            activities.Add(new Tuple<string, string>("Vocal Music Club", "<p>Vocal Music Club is an after-school club for all students that enjoy singing.  Whether you sign up for Chorus, Band, Orchestra or Exploring Music, being in this club will give you the opportunity to sing many different styles of music, either alone or as part of an ensemble, on a weekly basis.</p>"));

            return activities;
        }

        public static void GenerateDemoObjects(ref Account account, List<ParticipationObject> participationObjects, List<Location> locations, string accountName, List<GradeLevel> grades)
        {
            var reg = new Regex(BasicLibrary.Utilities.UrlFriendlyRegEx, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var accountUniqueName = reg.Replace(accountName.GetMaxWords(25), "");

            account = new Account()
            {
                Name = accountName.SplitByUpperCase(),
                UniqueName = accountUniqueName,
                IsActive = true,
                Uid = Guid.NewGuid()
            };

            var activityNames = new List<string>() { "Chess Club", "Robotics", "Theater Club", "Movie Making" };
            var contactNames = new List<string>() { "Geraldine Fisher", "Robert Henderson", "James Boyce" };
            var locationDetails = new List<string>() { "Room 105", "Meet in the Cafeteria", "Gym", "Football Field", "Room 200", "The Library", "Media Center" };
            var costs = new List<decimal>() { 100, 50, 27.50M, 0, 10 };
            var genderRestrictions = Enum.GetValues(typeof(GenderRestrictionType)).Cast<GenderRestrictionType>().ToList();
            var gradeRestrictions = grades.OrderBy(x => x.ID).Take(11).ToList();
            var schedule = ActivitySchedule.GetNewDefaultSchedule(generateDates: true);
            var numberOfSeats = new List<int>() { 16, 20, 37, 30, 25, 13, 1, 6 };
            var currentAccount = account;

            locations.Add(new Location()
            {
                Address1 = "12175 Visionary Way",
                Address2 = "#710",
                City = "Indianapolis",
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
            GetDemoActivities().ForEach(activity =>
            {
                var uniqueName = reg.Replace(activity.Item1.GetMaxWords(25), "");
                var cost = costs.Shuffle().First();
                var minGradeId = gradeRestrictions.Shuffle().First().ID;
                var maxGradeId = minGradeId + 3;

                participationObjects.Add(new ParticipationObject(global::ActivityType.Activity)
                {
                    OwnerAccount = currentAccount,
                    Schedule = schedule,
                    SeatsRemaining = numberOfSeats.Shuffle().First(),
                    Description = "",
                    UniqueName = uniqueName,
                    Name = activity.Item1,                    
                    AllowToPayLater = true,
                    ContactName = contactNames.Shuffle().First(),
                    ContactEmail = $"info@{accountUniqueName}.com",
                    ContactPhone = "888-123-1234",
                    Cost = cost,
                    CostUnitType = global::CostUnitType.PerRegistrant,
                    Details = activity.Item2,
                    Location = locations.First(),
                    PaymentPlans = new List<SubscriptionPlan>(),
                    SubscriptionPlans = new List<SubscriptionPlan>(),
                    WaitingListSeatsRemaining = 10,
                    RegistrationMode = RegistrationMode.Register,
                    RegistrationPeriod = registrationPeriod,
                    Uid = Guid.NewGuid(),
                    LocationDetails = $"<p>{locationDetails.Shuffle().First()}</p>",
                    IsFree = cost == 0,
                    IsTuitionFree = cost == 0,
                    GenderRestriction = genderRestrictions.Shuffle().First(),
                    MinGradeId = minGradeId,
                    MaxGradeId = maxGradeId,
                    ObjectType = ParticipationObjectType.Activity,
                });
            });
        }
    }
}

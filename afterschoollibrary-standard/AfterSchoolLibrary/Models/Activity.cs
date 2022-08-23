using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Models
{
    /// <summary>
    ///     The after school activity/team that a registrant/student is registering for
    /// </summary>
    public class Activity : BasicLookUpObject
    {
        #region "ClassGenerator"

        /// <summary>
        ///     Required.
        /// </summary>
        public int? SeatsAvailable
        {
            get => (int?) MDV["SeatsAvailable"].Value;
            set => MDV["SeatsAvailable"].Value = value;
        }

        /// <summary>
        ///     Refers to the related parent program
        /// </summary>
        public int? ProgramId
        {
            get => (int?) MDV["ProgramId"].Value;
            set => MDV["ProgramId"].Value = value;
        }

        /// <summary>
        ///     Required.
        /// </summary>
        public int? AccountId
        {
            get => (int?) MDV["AccountId"].Value;
            set => MDV["AccountId"].Value = value;
        }

        /// <summary>
        ///     the actual provider account who financially is responsible
        /// </summary>
        public int? ProviderAccountId
        {
            get => (int?) MDV["ProviderAccountId"].Value;
            set => MDV["ProviderAccountId"].Value = value;
        }

        public int? AgeStart
        {
            get => (int?) MDV["AgeStart"].Value;
            set => MDV["AgeStart"].Value = value;
        }

        public int? AgeEnd
        {
            get => (int?) MDV["AgeEnd"].Value;
            set => MDV["AgeEnd"].Value = value;
        }

        public int? GradeLevelStart
        {
            get => (int?) MDV["GradeLevelStart"].Value;
            set => MDV["GradeLevelStart"].Value = value;
        }

        public int? GradeLevelEnd
        {
            get => (int?) MDV["GradeLevelEnd"].Value;
            set => MDV["GradeLevelEnd"].Value = value;
        }

        public decimal? CostPerRegistrant
        {
            get => (decimal?) MDV["CostPerRegistrant"].Value;
            set => MDV["CostPerRegistrant"].Value = value;
        }

        public string CostDescription
        {
            get => (string) MDV["CostDescription"].Value;
            set => MDV["CostDescription"].Value = value;
        }

        /// <summary>
        ///     Required.
        /// </summary>
        public Guid Uid
        {
            get => (Guid) MDV["Uid"].Value;
            set => MDV["Uid"].Value = value;
        }

        public string LocationDetails
        {
            get => (string) MDV["LocationDetails"].Value;
            set => MDV["LocationDetails"].Value = value;
        }

        public string ScheduleDetails
        {
            get => (string) MDV["ScheduleDetails"].Value;
            set => MDV["ScheduleDetails"].Value = value;
        }

        public int? InstructorId
        {
            get => (int?) MDV["InstructorId"].Value;
            set => MDV["InstructorId"].Value = value;
        }

        public string ContactName
        {
            get => (string) MDV["ContactName"].Value;
            set => MDV["ContactName"].Value = value;
        }

        public string ContactPhone
        {
            get => (string) MDV["ContactPhone"].Value;
            set => MDV["ContactPhone"].Value = value;
        }

        public string ContactEmail
        {
            get => (string) MDV["ContactEmail"].Value;
            set => MDV["ContactEmail"].Value = value;
        }

        public bool AllowToPayLater
        {
            get => (bool) MDV["AllowToPayLater"].Value;
            set => MDV["AllowToPayLater"].Value = value;
        }

        public GenderRestrictionType GenderRestriction
        {
            get => (GenderRestrictionType) MDV["GenderRestriction"].Value;
            set => MDV["GenderRestriction"].Value = value;
        }

        public RegistrationMode RegistrationModeId
        {
            get => (RegistrationMode) MDV["RegistrationModeId"].Value;
            set => MDV["RegistrationModeId"].Value = value;
        }

        public int? LocationId
        {
            get => (int?) MDV["LocationId"].Value;
            set => MDV["LocationId"].Value = value;
        }

        public bool IsDeleted
        {
            get => (bool) MDV["IsDeleted"].Value;
            set => MDV["IsDeleted"].Value = value;
        }

        public bool IsPublished
        {
            get => (bool) MDV["IsPublished"].Value;
            set => MDV["IsPublished"].Value = value;
        }

        public bool IsArchived
        {
            get => (bool) MDV["IsArchived"].Value;
            set => MDV["IsArchived"].Value = value;
        }

        public bool IsInviteOnly
        {
            get => (bool) MDV["IsInviteOnly"].Value;
            set => MDV["IsInviteOnly"].Value = value;
        }

        public bool IsContinuousEnrollment
        {
            get => (bool) MDV["IsContinuousEnrollment"].Value;
            set => MDV["IsContinuousEnrollment"].Value = value;
        }

        public int? ActivityGroupId
        {
            get => (int?) MDV["ActivityGroupId"].Value;
            set => MDV["ActivityGroupId"].Value = value;
        }

        public int? ActivityScheduleId
        {
            get => (int?) MDV["ActivityScheduleId"].Value;
            set => MDV["ActivityScheduleId"].Value = value;
        }

        public string UniqueName
        {
            get => (string) MDV["UniqueName"].Value;
            set => MDV["UniqueName"].Value = value;
        }

        /// <summary>
        ///     For tracking which section or grouping a team is in
        /// </summary>
        public int? ActivitySectionId
        {
            get => (int?) MDV["ActivitySectionId"].Value;
            set => MDV["ActivitySectionId"].Value = value;
        }

        /// <summary>
        ///     Required.
        /// </summary>
        public ActivityType ActivityTypeId
        {
            get => (ActivityType) MDV["ActivityTypeId"].Value;
            set => MDV["ActivityTypeId"].Value = value;
        }

        /// <summary>
        ///     Used to track when a team has been submitted to a league
        /// </summary>
        public DateTime? DateSubmittedToActivityGroup
        {
            get => (DateTime?) MDV["DateSubmittedToActivityGroup"].Value;
            set => MDV["DateSubmittedToActivityGroup"].Value = value;
        }

        /// <summary>
        ///     When a activity is part of a league that is from an external source (such as a team) then it should be linked to the account version of that league
        /// </summary>
        public int? AccountActivityGroupId
        {
            get => (int?) MDV["AccountActivityGroupId"].Value;
            set => MDV["AccountActivityGroupId"].Value = value;
        }

        /// <summary>
        ///     Track what kind of leader it is - internal or external
        /// </summary>
        public ActivityLeaderType LeaderTypeId
        {
            get => (ActivityLeaderType) MDV["LeaderTypeId"].Value;
            set => MDV["LeaderTypeId"].Value = value;
        }

        /// <summary>
        ///     Indicates whether parental consent is required
        /// </summary>
        public bool RequiresParentalConsent
        {
            get => (bool) MDV["RequiresParentalConsent"].Value;
            set => MDV["RequiresParentalConsent"].Value = value;
        }

        /// <summary>
        ///     Indicates whether a student is allowed to register
        /// </summary>
        public bool IsStudentAllowedToRegister
        {
            get => (bool) MDV["IsStudentAllowedToRegister"].Value;
            set => MDV["IsStudentAllowedToRegister"].Value = value;
        }

        /// <summary>
        ///     Physical or Virtual
        /// </summary>
        public ActivityLocationType ActivityLocationTypeId
        {
            get => (ActivityLocationType) MDV["ActivityLocationTypeId"].Value;
            set => MDV["ActivityLocationTypeId"].Value = value;
        }

        /// <summary>
        ///     Link for virtual class
        /// </summary>
        public string VirtualLocationInfo
        {
            get => (string) MDV["VirtualLocationInfo"].Value;
            set => MDV["VirtualLocationInfo"].Value = value;
        }

        /// <summary>
        ///     If true then don't want to add a pin for the exact location
        /// </summary>
        public bool HidePreciseLocation
        {
            get => (bool) MDV["HidePreciseLocation"].Value;
            set => MDV["HidePreciseLocation"].Value = value;
        }

        /// <summary>
        ///     Minimum seats required to have the activity
        /// </summary>
        public int? MinSeatsRequired
        {
            get => (int?) MDV["MinSeatsRequired"].Value;
            set => MDV["MinSeatsRequired"].Value = value;
        }

        /// <summary>
        ///     Used to close registration and send notifications
        /// </summary>
        public bool RegistrationFinalized
        {
            get => (bool) MDV["RegistrationFinalized"].Value;
            set => MDV["RegistrationFinalized"].Value = value;
        }

        /// <summary>
        ///     Used to set the interest form used on the activity page
        /// </summary>
        public int? InterestFormId
        {
            get => (int?) MDV["InterestFormId"].Value;
            set => MDV["InterestFormId"].Value = value;
        }

        /// <summary>
        ///     Used to allow a provider to set the final fee a parent sees
        /// </summary>
        public bool IsConvenienceFeeTakenOutOfCost
        {
            get => (bool) MDV["IsConvenienceFeeTakenOutOfCost"].Value;
            set => MDV["IsConvenienceFeeTakenOutOfCost"].Value = value;
        }

        /// <summary>
        ///     Use to determine if registration fee applies to the whole activity or each class
        /// </summary>
        public ActivityCostFrequencyType CostFrequencyTypeId
        {
            get => (ActivityCostFrequencyType) MDV["CostFrequencyTypeId"].Value;
            set => MDV["CostFrequencyTypeId"].Value = value;
        }

        /// <summary>
        ///     the number of hours before the activity time that a parent can register
        /// </summary>
        public int? RegistrationCutoffHours
        {
            get => (int?) MDV["RegistrationCutoffHours"].Value;
            set => MDV["RegistrationCutoffHours"].Value = value;
        }

        public string AccountName => (string) MDV["AccountName"].Value;
        public string AccountTimeZone => (string) MDV["AccountTimeZone"].Value;
        public bool? ActivityGroupIsArchived => (bool?) MDV["ActivityGroupIsArchived"].Value;
        public string ActivityGroupName => (string) MDV["ActivityGroupName"].Value;
        public Guid ActivityGroupUid => (Guid) MDV["ActivityGroupUid"].Value;


        public Activity()
        {
            Config(ObjectMap, DatabaseQuery, typeof(Activity).Name, "ID");
            LoadObjectValues(ObjectMap);
            Uid = Guid.NewGuid();
            AllowToPayLater = false;
            GenderRestriction = GenderRestrictionType.None;
            RegistrationModeId = RegistrationMode.NotSet;
            IsDeleted = false;
            IsPublished = false;
            IsArchived = false;
            IsInviteOnly = false;
            IsContinuousEnrollment = false;
            ActivityTypeId = ActivityType.NotSet;
            LeaderTypeId = ActivityLeaderType.NotSet;
            RequiresParentalConsent = false;
            IsStudentAllowedToRegister = false;
            ActivityLocationTypeId = ActivityLocationType.NotSet;
            HidePreciseLocation = false;
            RegistrationFinalized = false;
            IsConvenienceFeeTakenOutOfCost = false;
            CostFrequencyTypeId = ActivityCostFrequencyType.NotSet;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                const string className = nameof(Activity);
                var dp = get_BaseObjectMap(className);
                dp.Add(className, "SeatsAvailable", SqlDbType.Int);
                dp.Add(className, "ProgramId", SqlDbType.Int);
                dp.Add(className, "AccountId", SqlDbType.Int);
                dp.Add(className, "ProviderAccountId", SqlDbType.Int);
                dp.Add(className, "AgeStart", SqlDbType.Int);
                dp.Add(className, "AgeEnd", SqlDbType.Int);
                dp.Add(className, "GradeLevelStart", SqlDbType.Int);
                dp.Add(className, "GradeLevelEnd", SqlDbType.Int);
                dp.Add(className, "CostPerRegistrant", SqlDbType.Money);
                dp.Add(className, "CostDescription", SqlDbType.NVarChar, "1000");
                dp.Add(className, "Uid", SqlDbType.UniqueIdentifier);
                dp.Add(className, "LocationDetails", SqlDbType.NVarChar, "4000");
                dp.Add(className, "ScheduleDetails", SqlDbType.NVarChar, "4000");
                dp.Add(className, "InstructorId", SqlDbType.Int);
                dp.Add(className, "ContactName", SqlDbType.NVarChar, "50");
                dp.Add(className, "ContactPhone", SqlDbType.NVarChar, "50");
                dp.Add(className, "ContactEmail", SqlDbType.NVarChar, "100");
                dp.Add(className, "AllowToPayLater", SqlDbType.Bit);
                dp.Add(className, "GenderRestriction", SqlDbType.Int);
                dp.Add(className, "RegistrationModeId", SqlDbType.Int);
                dp.Add(className, "LocationId", SqlDbType.Int);
                dp.Add(className, "IsDeleted", SqlDbType.Bit);
                dp.Add(className, "IsPublished", SqlDbType.Bit);
                dp.Add(className, "IsArchived", SqlDbType.Bit);
                dp.Add(className, "IsInviteOnly", SqlDbType.Bit);
                dp.Add(className, "IsContinuousEnrollment", SqlDbType.Bit);
                dp.Add(className, "ActivityGroupId", SqlDbType.Int);
                dp.Add(className, "ActivityScheduleId", SqlDbType.Int);
                dp.Add(className, "UniqueName", SqlDbType.NVarChar, "100");
                dp.Add(className, "ActivitySectionId", SqlDbType.Int);
                dp.Add(className, "ActivityTypeId", SqlDbType.Int);
                dp.Add(className, "DateSubmittedToActivityGroup", SqlDbType.DateTime);
                dp.Add(className, "AccountActivityGroupId", SqlDbType.Int);
                dp.Add(className, "LeaderTypeId", SqlDbType.Int);
                dp.Add(className, "RequiresParentalConsent", SqlDbType.Bit);
                dp.Add(className, "IsStudentAllowedToRegister", SqlDbType.Bit);
                dp.Add(className, "ActivityLocationTypeId", SqlDbType.Int);
                dp.Add(className, "VirtualLocationInfo", SqlDbType.NVarChar, "1000");
                dp.Add(className, "HidePreciseLocation", SqlDbType.Bit);
                dp.Add(className, "MinSeatsRequired", SqlDbType.Int);
                dp.Add(className, "RegistrationFinalized", SqlDbType.Bit);
                dp.Add(className, "InterestFormId", SqlDbType.Int);
                dp.Add(className, "IsConvenienceFeeTakenOutOfCost", SqlDbType.Bit);
                dp.Add(className, "CostFrequencyTypeId", SqlDbType.Int);
                dp.Add(className, "RegistrationCutoffHours", SqlDbType.Int);
                dp.Add(className, "AccountName", typeof(Account).Name, "Name", SqlDbType.NVarChar, "Account");
                dp.Add(className, "AccountTimeZone", typeof(Account).Name, "TimeZone", SqlDbType.NVarChar, "Account");
                dp.Add(className, "ActivityGroupIsArchived", typeof(ActivityGroup).Name, "IsArchived", SqlDbType.Bit, "ActivityGroup");
                dp.Add(className, "ActivityGroupName", typeof(ActivityGroup).Name, "Name", SqlDbType.NVarChar, "ActivityGroup");
                dp.Add(className, "ActivityGroupUid", typeof(ActivityGroup).Name, "Uid", SqlDbType.UniqueIdentifier, "ActivityGroup");

                return dp;
            }
        }

        public static Activity LoadObject(DataRow row)
        {
            var obj = new Activity();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, typeof(Activity).Name);
                dq.Joins.Add(Account.ObjectMap["ID"], ObjectMap["AccountId"], DatabaseJoinTypeEnum.InnerJoin, "Account");
                dq.Joins.Add(ActivityGroup.ObjectMap["ID"], ObjectMap["ActivityGroupId"], DatabaseJoinTypeEnum.LeftOuterJoin, "ActivityGroup");

                return dq;
            }
        }

        public static Activity Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Activity> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            return BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

        public SystemMessages Save(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if (SeatsAvailable.IsEmpty())
                msgs.Add("SeatsAvailable is required on Activity object", MessageType.Error);
            if (AccountId.IsEmpty())
                msgs.Add("AccountId is required on Activity object", MessageType.Error);
            if (Uid.IsEmpty())
                msgs.Add("Uid is required on Activity object", MessageType.Error);
            if (ActivityTypeId.IsEmpty())
                msgs.Add("ActivityTypeId is required on Activity object", MessageType.Error);

            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Activity).Name));

            return msgs;
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Activity).Name);
        }

        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return _Create(DatabaseQuery, dc, ObjectMap);
        }

        public static SystemMessages Alter(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            DatabaseParameters dps;

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["SeatsAvailable"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ProgramId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["AccountId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ProviderAccountId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["AgeStart"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["AgeEnd"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["GradeLevelStart"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["GradeLevelEnd"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["CostPerRegistrant"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["CostDescription"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["Uid"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["LocationDetails"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ScheduleDetails"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["InstructorId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ContactName"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ContactPhone"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ContactEmail"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["AllowToPayLater"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["GenderRestriction"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["RegistrationModeId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["LocationId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsDeleted"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsPublished"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsArchived"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsInviteOnly"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsContinuousEnrollment"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ActivityGroupId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ActivityScheduleId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["UniqueName"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ActivitySectionId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ActivityTypeId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["DateSubmittedToActivityGroup"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["AccountActivityGroupId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["LeaderTypeId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["RequiresParentalConsent"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsStudentAllowedToRegister"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ActivityLocationTypeId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["VirtualLocationInfo"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["HidePreciseLocation"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["MinSeatsRequired"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["RegistrationFinalized"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["InterestFormId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsConvenienceFeeTakenOutOfCost"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["CostFrequencyTypeId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["RegistrationCutoffHours"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));


            return msgs;
        }

        public Activity Reload(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Load(ID.Value, msgs, dc);
        }

        public static List<Activity> LoadByAccountId(int accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadByAccountIdByIsPublished(int accountId, bool isPublished, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);
            dq.Criterias.Add(ObjectMap["IsPublished"], isPublished);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadByLocationId(int locationId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["LocationId"], locationId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static Activity LoadByUid(Guid uid, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Uid"], uid);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static Activity LoadByUidByAccountId(Guid uid, int accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Uid"], uid);
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static Activity LoadByUidByProviderAccountId(Guid uid, int providerAccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Uid"], uid);
            dq.Criterias.Add(ObjectMap["ProviderAccountId"], providerAccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Activity> LoadByActivityGroupId(int activityGroupId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityGroupId"], activityGroupId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadByActivityGroupIdByAccountId(int activityGroupId, int accountId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityGroupId"], activityGroupId);
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadByAccountActivityGroupId(int accountActivityGroupId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountActivityGroupId"], accountActivityGroupId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadByActivityScheduleId(int activityScheduleId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityScheduleId"], activityScheduleId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadByProviderAccountId(int providerAccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ProviderAccountId"], providerAccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static Activity LoadByUniqueNameByAccountId(string uniqueName, int accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["UniqueName"], uniqueName);
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        #endregion

        #region "UserDefined"

        public bool HasSubscriptionPlan(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.GetCount(
                new Hashtable
                {
                    {"ParticipationObjectId", ID.Value},
                    {"ParticipationObjectTypeId", (int) ParticipationObjectType.Activity},
                    {"PlanTypeId", (int) SubscriptionPlanType.Subscription},
                    {"IsActive", true},
                },
                msgs,
                dc) > 0;
        }

        public bool HasPaymentPlan(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.GetCount(
                new Hashtable
                {
                    {"ParticipationObjectId", ID.Value},
                    {"ParticipationObjectTypeId", (int) ParticipationObjectType.Activity},
                    {"PlanTypeId", (int) SubscriptionPlanType.PaymentPlan},
                    {"IsActive", true},
                },
                msgs,
                dc) > 0;
        }

        public bool HasPostPayPlan(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.GetCount(
                new Hashtable
                {
                    {"ParticipationObjectId", ID.Value},
                    {"ParticipationObjectTypeId", (int) ParticipationObjectType.Activity},
                    {"PlanTypeId", (int) SubscriptionPlanType.PostPayPlan},
                    {"IsActive", true},
                },
                msgs,
                dc) > 0;
        }

        public string HomePageUrl => this.GetHomePageUrl();

        public string ShortHomePageUrl => this.GetShortHomePageUrl();

        public string PreviewUrl => $"{HomePageUrl}{"?preview=true"}";

        public static Activity LoadByUid(Guid uid, int accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Uid"], uid);
            dq.Criterias.Add(ObjectMap["accountId"], accountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static Activity LoadByIdByAccountId(int id, int accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Id"], id);
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Activity> LoadList(List<int> ids, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids.ToArray(), DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadListByIdsByAccountId(List<int> ids, int accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids.ToArray(), DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadListByIdsByGroupByIsArchivedIsDeleted(List<int> ids, int? activityGroupId, int? accountActivityGroupId,
            ActivityType? activityType, bool? isArchived, bool? isDeleted, bool? ungroupedOnly, int? start, int? length, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids.ToArray(), DatabaseCriteriaOperatorEnum.In);

            if (activityGroupId.IsNotEmpty())
                dq.Criterias.Add(ObjectMap["ActivityGroupId"], activityGroupId);
            else if (ungroupedOnly.HasValue && ungroupedOnly.Value)
                dq.Criterias.Add(ObjectMap["ActivityGroupId"], null, DatabaseCriteriaOperatorEnum.Is);

            if (accountActivityGroupId.IsNotEmpty())
                dq.Criterias.Add(ObjectMap["AccountActivityGroupId"], accountActivityGroupId);

            if (activityType.IsNotEmpty())
                dq.Criterias.Add(ObjectMap["ActivityTypeId"], (int) activityType);

            if (isArchived.IsNotEmpty())
                dq.Criterias.Add(ObjectMap["IsArchived"], isArchived);

            if (isDeleted.IsNotEmpty())
                dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);

            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;

            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadList(List<int> ids, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadListWithLocations(List<int> ids, List<int> locationIds, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["LocationId"], locationIds, DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        private static DatabaseQuery LoadByAccountIdQuery(int? accountId, int? activityGroupId, int? accountActivityGroupId, ActivityType? activityType,
            bool? isArchived, bool? isDeleted, bool? isSubmitted, bool? ungroupedOnly, string q, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.UseParameterizedQuery = false;
            dq.Connection = dc;

            if (accountId.HasValue)
            {
                var dcs = new DatabaseCriterias(LogicalOperatorType.Or);
                dcs.Add(ObjectMap["AccountId"], accountId);
                dcs.Add(ObjectMap["ProviderAccountId"], accountId);

                if (q.IsNotEmpty() && activityType == ActivityType.Activity)
                    dq.Criterias.Add(ObjectMap["Name"], q, DatabaseCriteriaOperatorEnum.Like);

                var locationQuery = get_DatabaseQuery(Location.ObjectMap, typeof(Location).Name);
                locationQuery.UseParameterizedQuery = false;
                locationQuery.SelectParameters.Clear();
                locationQuery.SelectParameters.Add(Location.ObjectMap["ID"]);
                locationQuery.Criterias.Add(Location.ObjectMap["HostAccountId"], accountId);
                dcs.Add(ObjectMap["LocationId"], locationQuery, DatabaseCriteriaOperatorEnum.In);

                dq.Criterias.Add(dcs);
            }

            if (activityGroupId.HasValue)
                dq.Criterias.Add(ObjectMap["activityGroupId"], activityGroupId.Value);
            else if (ungroupedOnly.HasValue && ungroupedOnly.Value)
                dq.Criterias.Add(ObjectMap["activityGroupId"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);

            if (accountActivityGroupId.HasValue)
                dq.Criterias.Add(ObjectMap["accountActivityGroupId"], accountActivityGroupId.Value);

            if (activityType.HasValue)
                dq.Criterias.Add(ObjectMap["ActivityTypeId"], (int) activityType.Value);

            if (isSubmitted.HasValue)
                dq.Criterias.Add(ObjectMap["DateSubmittedToActivityGroup"], DBNull.Value, DatabaseCriteriaOperatorEnum.IsNot);

            if (isArchived.HasValue)
            {
                if (!isArchived.Value)
                {
                    var dcs = new DatabaseCriterias(LogicalOperatorType.And);
                    var dcs2 = new DatabaseCriterias(LogicalOperatorType.Or);
                    dcs.Add(ObjectMap["isArchived"], false);

                    dcs2.Add(ObjectMap["ActivityGroupId"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
                    dcs2.Add(ObjectMap["ActivityGroupIsArchived"], false);
                    dcs.AddGroup(dcs2);

                    dq.Criterias.Add(dcs);
                }
                else
                {
                    var dcs = new DatabaseCriterias(LogicalOperatorType.Or);
                    var dcs2 = new DatabaseCriterias(LogicalOperatorType.And);
                    dcs.Add(ObjectMap["isArchived"], true);

                    dcs2.Add(ObjectMap["ActivityGroupId"], DBNull.Value, DatabaseCriteriaOperatorEnum.IsNot);
                    dcs2.Add(ObjectMap["ActivityGroupIsArchived"], true);
                    dcs.AddGroup(dcs2);

                    dq.Criterias.Add(dcs);
                }
            }

            if (isDeleted.HasValue)
                dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted.Value);

            return dq;
        }

        public static int GetCountByAccountId(int accountId, string q, int? activityGroupId, ActivityType? activityType, bool? isArchived, bool? isDeleted,
            bool? ungroupedOnly, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = LoadByAccountIdQuery(accountId, activityGroupId, null, activityType, isArchived, isDeleted, null, ungroupedOnly, q, dc);
            return dq.GetCount(msgs);
        }

        public static int GetCountByActivityGroupId(int? activityGroupId, ActivityType? activityType, bool? isArchived, bool? isDeleted, bool? isSubmitted,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = LoadByAccountIdQuery(null, activityGroupId, null, activityType, isArchived, isDeleted, isSubmitted, false, null, dc);
            return dq.GetCount(msgs);
        }

        public static int GetCountByAccountActivityGroupId(int? accountActivityGroupId, ActivityType? activityType, bool? isArchived, bool? isDeleted,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = LoadByAccountIdQuery(null, null, accountActivityGroupId, activityType, isArchived, isDeleted, null, false, null, dc);
            return dq.GetCount(msgs);
        }

        public static List<Activity> LoadActivity(int accountId, string orderByProperty, OrderByDirectionEnum orderByDirection, int? start = 0, int? length = 1,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = LoadByAccountIdQuery(accountId, null, null, null, null, null, null, null, null, dc);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            dq.OrderBys.Add(orderByProperty, orderByDirection);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadByAccountId(int accountId, int? activityGroupId, int? accountActivityGroupId, ActivityType? activityType,
            bool? isArchived, bool? isDeleted, bool? ungroupedOnly, int? start, int? length, string q, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = LoadByAccountIdQuery(accountId, activityGroupId, accountActivityGroupId, activityType, isArchived, isDeleted, null, ungroupedOnly, q, dc);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            dq.OrderBys.Add("Name", OrderByDirectionEnum.Ascending);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadByAccountIdByDate(int accountId, DateTime? startDate = null, DateTime? endDate = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["RegistrationCloseDate"], startDate, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["RegistrationCloseDate"], endDate, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadByActivityScheduleIdByAccountId(int activityScheduleId, int accountId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityScheduleId"], activityScheduleId);
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCountByProviderAccountId(int providerAccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ProviderAccountId"], providerAccountId);
            return dq.GetCount(msgs);
        }

        public bool IsFree(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return (!CostPerRegistrant.HasValue || CostPerRegistrant.Value == 0) && !HasSubscriptionPlan(msgs, dc) && !HasPaymentPlan(msgs, dc) &&
                   !HasPostPayPlan(msgs, dc);
        }

        public bool IsPublishable()
        {
            if (Name.IsEmpty())
                return false;

            if (!SeatsAvailable.HasValue)
                return false;

            if (!ActivityGroupId.HasValue)
            {
                if (this.GetActiveRegistrationPeriod().IsEmpty())
                    return false;

                if (this.GetActiveActivityForm().IsEmpty())
                    return false;
            }

            if (ContactName.IsBlank())
                return false;

            if (ContactEmail.IsBlank())
                return false;

            if (ContactPhone.IsBlank())
                return false;

            if (!ActivityGroupId.HasValue)
            {
                var applicationThankYouEmail = this.GetActivityProperty(AccountPropertyType.ApplicationThankYouEmail);
                if (applicationThankYouEmail.IsEmpty() || applicationThankYouEmail.Value.IsEmpty())
                    return false;

                var registrationThankYouEmail = this.GetActivityProperty(AccountPropertyType.RegistrationThankYouEmail);
                if (registrationThankYouEmail.IsEmpty() || registrationThankYouEmail.Value.IsEmpty())
                    return false;
            }

            if (!ID.HasValue)
                return false;

            var details = this.GetActivityDetails();
            if (Utilities.RemoveHTMLAndEntities(details?.Value).IsBlank())
                return false;

            var activityGroup = this.GetActivityGroup();
            if (IsArchived)
                return false;

            if (activityGroup.IsNotEmpty() && activityGroup.IsArchived)
                return false;

            return true;
        }

        public bool IsPubliclyViewable =>
            IsPublished && !IsArchived && (this.GetActivityGroup().IsEmpty() || !this.GetActivityGroup().IsArchived) && !IsDeleted;

        public bool IsGroupArchived => this.GetActivityGroup().IsNotEmpty() && this.GetActivityGroup().IsArchived;

        public bool HasSchedule => ActivityScheduleId.HasValue;

        #endregion
    }
}
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

        public string Details
        {
            get => (string) MDV["Details"].Value;
            set => MDV["Details"].Value = value;
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

        public bool IsGenderEnforced
        {
            get => (bool) MDV["IsGenderEnforced"].Value;
            set => MDV["IsGenderEnforced"].Value = value;
        }

        /// <summary>
        ///     For setting the activity to featured
        /// </summary>
        public bool IsFeatured
        {
            get => (bool) MDV["IsFeatured"].Value;
            set => MDV["IsFeatured"].Value = value;
        }

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
            IsGenderEnforced = false;
            IsFeatured = false;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                var _dp = new DatabaseParameters();
                var className = typeof(Activity).Name;
                _dp = get_BaseObjectMap(className);
                _dp.Add(className, "SeatsAvailable", SqlDbType.Int);
                _dp.Add(className, "ProgramId", SqlDbType.Int);
                _dp.Add(className, "AccountId", SqlDbType.Int);
                _dp.Add(className, "ProviderAccountId", SqlDbType.Int);
                _dp.Add(className, "AgeStart", SqlDbType.Int);
                _dp.Add(className, "AgeEnd", SqlDbType.Int);
                _dp.Add(className, "GradeLevelStart", SqlDbType.Int);
                _dp.Add(className, "GradeLevelEnd", SqlDbType.Int);
                _dp.Add(className, "CostPerRegistrant", SqlDbType.Money);
                _dp.Add(className, "CostDescription", SqlDbType.NVarChar, "1000");
                _dp.Add(className, "Uid", SqlDbType.UniqueIdentifier);
                _dp.Add(className, "Details", SqlDbType.NVarChar, "MAX");
                _dp.Add(className, "LocationDetails", SqlDbType.NVarChar, "4000");
                _dp.Add(className, "ScheduleDetails", SqlDbType.NVarChar, "4000");
                _dp.Add(className, "InstructorId", SqlDbType.Int);
                _dp.Add(className, "ContactName", SqlDbType.NVarChar, "50");
                _dp.Add(className, "ContactPhone", SqlDbType.NVarChar, "50");
                _dp.Add(className, "ContactEmail", SqlDbType.NVarChar, "100");
                _dp.Add(className, "AllowToPayLater", SqlDbType.Bit);
                _dp.Add(className, "GenderRestriction", SqlDbType.Int);
                _dp.Add(className, "RegistrationModeId", SqlDbType.Int);
                _dp.Add(className, "LocationId", SqlDbType.Int);
                _dp.Add(className, "IsDeleted", SqlDbType.Bit);
                _dp.Add(className, "IsPublished", SqlDbType.Bit);
                _dp.Add(className, "IsArchived", SqlDbType.Bit);
                _dp.Add(className, "IsInviteOnly", SqlDbType.Bit);
                _dp.Add(className, "IsContinuousEnrollment", SqlDbType.Bit);
                _dp.Add(className, "ActivityGroupId", SqlDbType.Int);
                _dp.Add(className, "ActivityScheduleId", SqlDbType.Int);
                _dp.Add(className, "UniqueName", SqlDbType.NVarChar, "100");
                _dp.Add(className, "ActivitySectionId", SqlDbType.Int);
                _dp.Add(className, "ActivityTypeId", SqlDbType.Int);
                _dp.Add(className, "DateSubmittedToActivityGroup", SqlDbType.DateTime);
                _dp.Add(className, "AccountActivityGroupId", SqlDbType.Int);
                _dp.Add(className, "LeaderTypeId", SqlDbType.Int);
                _dp.Add(className, "RequiresParentalConsent", SqlDbType.Bit);
                _dp.Add(className, "IsStudentAllowedToRegister", SqlDbType.Bit);
                _dp.Add(className, "ActivityLocationTypeId", SqlDbType.Int);
                _dp.Add(className, "VirtualLocationInfo", SqlDbType.NVarChar, "1000");
                _dp.Add(className, "HidePreciseLocation", SqlDbType.Bit);
                _dp.Add(className, "MinSeatsRequired", SqlDbType.Int);
                _dp.Add(className, "RegistrationFinalized", SqlDbType.Bit);
                _dp.Add(className, "InterestFormId", SqlDbType.Int);
                _dp.Add(className, "IsConvenienceFeeTakenOutOfCost", SqlDbType.Bit);
                _dp.Add(className, "CostFrequencyTypeId", SqlDbType.Int);
                _dp.Add(className, "RegistrationCutoffHours", SqlDbType.Int);
                _dp.Add(className, "AccountName", typeof(Account).Name, "Name", SqlDbType.NVarChar, "Account");
                _dp.Add(className, "AccountTimeZone", typeof(Account).Name, "TimeZone", SqlDbType.NVarChar, "Account");
                _dp.Add(className, "ActivityGroupIsArchived", typeof(ActivityGroup).Name, "IsArchived", SqlDbType.Bit, "ActivityGroup");
                _dp.Add(className, "ActivityGroupName", typeof(ActivityGroup).Name, "Name", SqlDbType.NVarChar, "ActivityGroup");
                _dp.Add(className, "ActivityGroupUid", typeof(ActivityGroup).Name, "Uid", SqlDbType.UniqueIdentifier, "ActivityGroup");
                _dp.Add(className, "IsGenderEnforced", SqlDbType.Bit);
                _dp.Add(className, "IsFeatured", SqlDbType.Bit);

                return _dp;
            }
        }

        public static Activity LoadObject(DataRow row)
        {
            var obj = new Activity();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }
        
        public static dynamic LoadOdynamicbject(DataRow row)
        {
            dynamic obj = new dynamic();
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

        public static List<dynamic> LoadByDistrict(Account account, int orderBy, int start, int length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            // dq.Criterias.Add(ObjectMap["AccountID"], accounts.Select(x => x.ID.Value).ToList(), DatabaseCriteriaOperatorEnum.In);
            // dq.SelectRange.Start = start;
            // dq.SelectRange.Length = length;
            // dq.OrderBys.Add("CreateDate", orderByDirection);
            
            var sql = GetAllActivitiesByDistrict(account.ID.Value,orderBy, start, length);
            return dq.ExecuteSelect(sql, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCountByDistrict(Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            // dq.Criterias.Add(ObjectMap["AccountID"], accounts.Select(x => x.ID.Value).ToList(), DatabaseCriteriaOperatorEnum.In);
            // return dq.GetCount(msgs);
            
            var sql = GetAllActivitiesByDistrictCount(account.ID.Value);
            return Convert.ToInt32(dq.ExecuteSelect(sql, msgs)?.Rows[0]?.ItemArray[0]);
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
            dps.Add(ObjectMap["Details"]);
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

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsGenderEnforced"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsFeatured"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));


            return msgs;
        }

        public Activity Reload(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Load(ID.Value, msgs, dc);
        }

        public static List<Activity> LoadByAccountId(int AccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadByAccountIdByIsPublished(int AccountId, bool IsPublished, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["IsPublished"], IsPublished);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadByLocationId(int LocationId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["LocationId"], LocationId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static Activity LoadByUid(Guid Uid, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Uid"], Uid);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static Activity LoadByUidByAccountId(Guid Uid, int AccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Uid"], Uid);
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static Activity LoadByUidByProviderAccountId(Guid Uid, int ProviderAccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Uid"], Uid);
            dq.Criterias.Add(ObjectMap["ProviderAccountId"], ProviderAccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Activity> LoadByActivityGroupId(int ActivityGroupId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadByActivityGroupIdByAccountId(int ActivityGroupId, int AccountId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadByAccountActivityGroupId(int AccountActivityGroupId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountActivityGroupId"], AccountActivityGroupId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadByActivityScheduleId(int ActivityScheduleId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityScheduleId"], ActivityScheduleId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadByProviderAccountId(int ProviderAccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ProviderAccountId"], ProviderAccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static Activity LoadByUniqueNameByAccountId(string UniqueName, int AccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["UniqueName"], UniqueName);
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        private static string GetAllActivitiesByDistrict(int hostAccountId, int OrderBy, int? start, int? length)
        {
            var sql = $@"select
	            school.name as ,
	            activity.*
            from
	            account school left outer join
	            [location] activitylocation on school.id = activitylocation.HostAccountId left outer join
	            activity on activity.locationid = activitylocation.id left outer join
	            account provider on activity.AccountId = provider.ID
            where
	            school.id in (
                select visitoraccountid
                from accountrelationship
                where hostaccountid = {hostAccountId}
              ) and
              school.accounttypeid = 2 AND
              school.isdeleted = 0 AND
              school.istest = 0
              order by activity.CreateDate";
            
            if(OrderBy == 0)
                sql += $" desc offset {start} rows fetch next {length} rows only";
            
            if(OrderBy == 1)
                sql += $" asc offset {start} rows fetch next {length} rows only";

            return sql;
        }
        
        private static string GetAllActivitiesByDistrictCount(int hostAccountId)
        {
            var sql = $@"select Count(distinct activity.id)
            from
	            account school left outer join
	            [location] activitylocation on school.id = activitylocation.HostAccountId left outer join
	            activity on activity.locationid = activitylocation.id left outer join
	            account provider on activity.AccountId = provider.ID
            where
	            school.id in (
                select visitoraccountid
                from accountrelationship
                where hostaccountid = {hostAccountId}
              ) and
              school.accounttypeid = 2 AND
              school.isdeleted = 0 AND
              school.istest = 0";

            return sql;
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

        public static Activity LoadByUid(Guid Uid, int accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Uid"], Uid);
            dq.Criterias.Add(ObjectMap["accountId"], accountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static Activity LoadByIdByAccountId(int Id, int AccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Id"], Id);
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Activity> LoadList(List<int> ids, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids.ToArray(), DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadListByIdsByAccountId(List<int> ids, int AccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids.ToArray(), DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
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
                    dcs.Add(ObjectMap["isArchived"], isArchived.Value);

                    dcs2.Add(ObjectMap["ActivityGroupId"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
                    dcs2.Add(ObjectMap["ActivityGroupIsArchived"], isArchived.Value);
                    dcs.AddGroup(dcs2);

                    dq.Criterias.Add(dcs);
                }
                else
                {
                    var dcs = new DatabaseCriterias(LogicalOperatorType.Or);
                    var dcs2 = new DatabaseCriterias(LogicalOperatorType.And);
                    dcs.Add(ObjectMap["isArchived"], isArchived.Value);

                    dcs2.Add(ObjectMap["ActivityGroupId"], DBNull.Value, DatabaseCriteriaOperatorEnum.IsNot);
                    dcs2.Add(ObjectMap["ActivityGroupIsArchived"], isArchived.Value);
                    dcs.AddGroup(dcs2);

                    dq.Criterias.Add(dcs);
                }
            }

            if (isDeleted.HasValue)
                dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted.Value);

            return dq;
        }

        public static int GetCountByAccountId(int AccountId, string q, int? activityGroupId, ActivityType? activityType, bool? isArchived, bool? isDeleted,
            bool? ungroupedOnly, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = LoadByAccountIdQuery(AccountId, activityGroupId, null, activityType, isArchived, isDeleted, null, ungroupedOnly, q, dc);
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

        public static List<Activity> LoadActivity(int AccountId, string OrderByProperty, OrderByDirectionEnum orderByDirection, int? start = 0, int? length = 1,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = LoadByAccountIdQuery(AccountId, null, null, null, null, null, null, null, null, dc);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            dq.OrderBys.Add(OrderByProperty, orderByDirection);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadByAccountId(int AccountId, int? activityGroupId, int? accountActivityGroupId, ActivityType? activityType,
            bool? isArchived, bool? isDeleted, bool? ungroupedOnly, int? start, int? length, string q, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = LoadByAccountIdQuery(AccountId, activityGroupId, accountActivityGroupId, activityType, isArchived, isDeleted, null, ungroupedOnly, q, dc);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            dq.OrderBys.Add("Name", OrderByDirectionEnum.Ascending);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadByAccountIdByDate(int AccountId, DateTime? startDate = null, DateTime? endDate = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["RegistrationCloseDate"], startDate, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["RegistrationCloseDate"], endDate, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadByActivityScheduleIdByAccountId(int ActivityScheduleId, int AccountId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityScheduleId"], ActivityScheduleId);
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCountByProviderAccountId(int ProviderAccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ProviderAccountId"], ProviderAccountId);
            return dq.GetCount(msgs);
        }

        public bool GetIsFree(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return (!CostPerRegistrant.HasValue || CostPerRegistrant.Value == 0) && !HasSubscriptionPlan(msgs, dc) && !HasPaymentPlan(msgs, dc) &&
                   !HasPostPayPlan(msgs, dc);
        }

        public bool IsFree => (!CostPerRegistrant.HasValue || CostPerRegistrant.Value == 0) && !HasSubscriptionPlan() && !HasPaymentPlan() && !HasPostPayPlan();

        public bool HasRegistrationFeeOrPostPayPlan => ((CostPerRegistrant.HasValue && CostPerRegistrant.Value > 0) || HasPostPayPlan()) &&
                                                       !HasSubscriptionPlan() && !HasPaymentPlan();

        public bool IsPublishable => this.GetPercentPublishable() == 100.ToDouble() && !IsArchived &&
                                     (this.GetActivityGroup().IsEmpty() || !this.GetActivityGroup().IsArchived);

        public bool IsPubliclyViewable => IsPublished && !IsArchived &&
                                          (this.GetActivityGroup().IsEmpty() || !this.GetActivityGroup().IsArchived) &&
                                          !IsDeleted;

        public bool IsGroupArchived => this.GetActivityGroup().IsNotEmpty() && this.GetActivityGroup().IsArchived;

        public bool HasSchedule => ActivityScheduleId.HasValue;

        #endregion
    }
}
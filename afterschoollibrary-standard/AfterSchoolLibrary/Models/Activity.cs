using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Data;
using System.Collections;

namespace AfterSchoolLibrary.Models
{
	/// <summary>
    /// The after school activity/team that a registrant/student is registering for
    /// </summary>
	
    public class Activity : BaseEventWithAddress
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? SeatsAvailable { get { return (int?)MDV["SeatsAvailable"].Value; } set { MDV["SeatsAvailable"].Value = value; } }
		public int? ProgramId { get { return (int?)MDV["ProgramId"].Value; } set { MDV["ProgramId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public int? AgeStart { get { return (int?)MDV["AgeStart"].Value; } set { MDV["AgeStart"].Value = value; } }
		public int? AgeEnd { get { return (int?)MDV["AgeEnd"].Value; } set { MDV["AgeEnd"].Value = value; } }
		public int? GradeLevelStart { get { return (int?)MDV["GradeLevelStart"].Value; } set { MDV["GradeLevelStart"].Value = value; } }
		public int? GradeLevelEnd { get { return (int?)MDV["GradeLevelEnd"].Value; } set { MDV["GradeLevelEnd"].Value = value; } }
		public decimal? CostPerRegistrant { get { return (decimal?)MDV["CostPerRegistrant"].Value; } set { MDV["CostPerRegistrant"].Value = value; } }
		public string CostDescription { get { return (string)MDV["CostDescription"].Value; } set { MDV["CostDescription"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public string Details { get { return (string)MDV["Details"].Value; } set { MDV["Details"].Value = value; } }
		public string LocationDetails { get { return (string)MDV["LocationDetails"].Value; } set { MDV["LocationDetails"].Value = value; } }
		public string ScheduleDetails { get { return (string)MDV["ScheduleDetails"].Value; } set { MDV["ScheduleDetails"].Value = value; } }
		public int? InstructorId { get { return (int?)MDV["InstructorId"].Value; } set { MDV["InstructorId"].Value = value; } }
		public string ContactName { get { return (string)MDV["ContactName"].Value; } set { MDV["ContactName"].Value = value; } }
		public string ContactPhone { get { return (string)MDV["ContactPhone"].Value; } set { MDV["ContactPhone"].Value = value; } }
		public string ContactEmail { get { return (string)MDV["ContactEmail"].Value; } set { MDV["ContactEmail"].Value = value; } }
		public int? WaitingListSeats { get { return (int?)MDV["WaitingListSeats"].Value; } set { MDV["WaitingListSeats"].Value = value; } }
		public bool AllowToPayLater { get { return (bool)MDV["AllowToPayLater"].Value; } set { MDV["AllowToPayLater"].Value = value; } }
		public GenderRestrictionType GenderRestriction { get { return (GenderRestrictionType)MDV["GenderRestriction"].Value; } set { MDV["GenderRestriction"].Value = value; } }
		public RegistrationMode RegistrationModeId { get { return (RegistrationMode)MDV["RegistrationModeId"].Value; } set { MDV["RegistrationModeId"].Value = value; } }
		public int? LocationId { get { return (int?)MDV["LocationId"].Value; } set { MDV["LocationId"].Value = value; } }
		public bool IsDeleted { get { return (bool)MDV["IsDeleted"].Value; } set { MDV["IsDeleted"].Value = value; } }
		public bool IsPublished { get { return (bool)MDV["IsPublished"].Value; } set { MDV["IsPublished"].Value = value; } }
		public bool IsArchived { get { return (bool)MDV["IsArchived"].Value; } set { MDV["IsArchived"].Value = value; } }
		public int? ActivityGroupId { get { return (int?)MDV["ActivityGroupId"].Value; } set { MDV["ActivityGroupId"].Value = value; } }
		public int? ActivityScheduleId { get { return (int?)MDV["ActivityScheduleId"].Value; } set { MDV["ActivityScheduleId"].Value = value; } }
		public string UniqueName { get { return (string)MDV["UniqueName"].Value; } set { MDV["UniqueName"].Value = value; } }
		public int? ActivitySectionId { get { return (int?)MDV["ActivitySectionId"].Value; } set { MDV["ActivitySectionId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public ActivityType ActivityTypeId { get { return (ActivityType)MDV["ActivityTypeId"].Value; } set { MDV["ActivityTypeId"].Value = value; } }
		public DateTime? DateSubmittedToActivityGroup { get { return (DateTime?)MDV["DateSubmittedToActivityGroup"].Value; } set { MDV["DateSubmittedToActivityGroup"].Value = value; } }
		public int? AccountActivityGroupId { get { return (int?)MDV["AccountActivityGroupId"].Value; } set { MDV["AccountActivityGroupId"].Value = value; } }
		public string AccountName { get { return (string)MDV["AccountName"].Value; } }
		public string AccountStripePublishableKey { get { return (string)MDV["AccountStripePublishableKey"].Value; } }
		public string AccountTimeZone { get { return (string)MDV["AccountTimeZone"].Value; } }
		public bool? ActivityGroupIsArchived { get { return (bool?)MDV["ActivityGroupIsArchived"].Value; } }
		public string ActivityGroupName { get { return (string)MDV["ActivityGroupName"].Value; } }
		public Guid ActivityGroupUid { get { return (Guid)MDV["ActivityGroupUid"].Value; } }
		

        public Activity()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Activity).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.Uid = Guid.NewGuid();
			this.AllowToPayLater = false;
			this.GenderRestriction = GenderRestrictionType.None;
			this.RegistrationModeId = RegistrationMode.NotSet;
			this.IsDeleted = false;
			this.IsPublished = false;
			this.IsArchived = false;
			this.ActivityTypeId = ActivityType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(Activity).Name;
                _dp = BaseEventWithAddress.get_BaseObjectMap(className);
				_dp.Add(className, "SeatsAvailable", System.Data.SqlDbType.Int);
				_dp.Add(className, "ProgramId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AgeStart", System.Data.SqlDbType.Int);
				_dp.Add(className, "AgeEnd", System.Data.SqlDbType.Int);
				_dp.Add(className, "GradeLevelStart", System.Data.SqlDbType.Int);
				_dp.Add(className, "GradeLevelEnd", System.Data.SqlDbType.Int);
				_dp.Add(className, "CostPerRegistrant", System.Data.SqlDbType.Money);
				_dp.Add(className, "CostDescription", System.Data.SqlDbType.NVarChar , "1000");
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "Details", System.Data.SqlDbType.NVarChar , "4000");
				_dp.Add(className, "LocationDetails", System.Data.SqlDbType.NVarChar , "4000");
				_dp.Add(className, "ScheduleDetails", System.Data.SqlDbType.NVarChar , "4000");
				_dp.Add(className, "InstructorId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ContactName", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "ContactPhone", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "ContactEmail", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "WaitingListSeats", System.Data.SqlDbType.Int);
				_dp.Add(className, "AllowToPayLater", System.Data.SqlDbType.Bit);
				_dp.Add(className, "GenderRestriction", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrationModeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "LocationId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsDeleted", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsPublished", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsArchived", System.Data.SqlDbType.Bit);
				_dp.Add(className, "ActivityGroupId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityScheduleId", System.Data.SqlDbType.Int);
				_dp.Add(className, "UniqueName", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "ActivitySectionId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "DateSubmittedToActivityGroup", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "AccountActivityGroupId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountName", typeof(Account).Name, "Name", System.Data.SqlDbType.NVarChar, "Account");
				_dp.Add(className, "AccountStripePublishableKey", typeof(Account).Name, "StripePublishableKey", System.Data.SqlDbType.NVarChar, "Account");
				_dp.Add(className, "AccountTimeZone", typeof(Account).Name, "TimeZone", System.Data.SqlDbType.NVarChar, "Account");
				_dp.Add(className, "ActivityGroupIsArchived", typeof(ActivityGroup).Name, "IsArchived", System.Data.SqlDbType.Bit, "ActivityGroup");
				_dp.Add(className, "ActivityGroupName", typeof(ActivityGroup).Name, "Name", System.Data.SqlDbType.NVarChar, "ActivityGroup");
				_dp.Add(className, "ActivityGroupUid", typeof(ActivityGroup).Name, "Uid", System.Data.SqlDbType.UniqueIdentifier, "ActivityGroup");
				
                return _dp;
            }
        }

        protected static Activity LoadObject(System.Data.DataRow row)
        {
            Activity obj = new Activity();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(Activity).Name);
				dq.Joins.Add(Account.ObjectMap["ID"], Activity.ObjectMap["AccountId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Account");
				dq.Joins.Add(ActivityGroup.ObjectMap["ID"], Activity.ObjectMap["ActivityGroupId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "ActivityGroup");
				
                return dq;
            }
        }

        public static Activity Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Activity> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return Activity.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return Activity.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.SeatsAvailable.IsEmpty())
				msgs.Add("SeatsAvailable is required on Activity object", MessageType.Error);
			if (this.AccountId.IsEmpty())
				msgs.Add("AccountId is required on Activity object", MessageType.Error);
			if (this.Uid.IsEmpty())
				msgs.Add("Uid is required on Activity object", MessageType.Error);
			if (this.ActivityTypeId.IsEmpty())
				msgs.Add("ActivityTypeId is required on Activity object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Activity).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Activity).Name);
        }

        public static SystemMessages Create(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return BaseObject._Create(DatabaseQuery, dc, ObjectMap);
        }

        public static SystemMessages Alter(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            SystemMessages msgs = new SystemMessages();
            BasicLibrary.DataAccess.DatabaseParameters dps;

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["SeatsAvailable"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ProgramId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AgeStart"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AgeEnd"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["GradeLevelStart"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["GradeLevelEnd"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["CostPerRegistrant"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["CostDescription"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Details"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LocationDetails"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ScheduleDetails"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["InstructorId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ContactName"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ContactPhone"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ContactEmail"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["WaitingListSeats"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AllowToPayLater"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["GenderRestriction"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrationModeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LocationId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsDeleted"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsPublished"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsArchived"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityGroupId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityScheduleId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["UniqueName"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivitySectionId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DateSubmittedToActivityGroup"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountActivityGroupId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Activity Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Activity.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<Activity> LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Activity> LoadByAccountIdByIsPublished(int AccountId, bool IsPublished, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["IsPublished"], IsPublished);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Activity> LoadByLocationId(int LocationId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["LocationId"], LocationId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Activity LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<Activity> LoadByActivityGroupId(int ActivityGroupId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Activity> LoadByAccountActivityGroupId(int AccountActivityGroupId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountActivityGroupId"], AccountActivityGroupId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Activity> LoadByActivityScheduleId(int ActivityScheduleId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityScheduleId"], ActivityScheduleId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Activity LoadByUniqueNameByAccountId(string UniqueName, int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["UniqueName"], UniqueName);
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public bool HasSubscriptionPlan(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.GetCount(
                new Hashtable() {
                    { "ParticipationObjectId", this.ID.Value },
                    { "ParticipationObjectTypeId", (int)ParticipationObjectType.Activity },
                    { "PlanTypeId", (int)SubscriptionPlanType.Subscription },
                    { "IsActive", true }
                },
                msgs,
                dc) > 0;
        }

        public bool HasPaymentPlan(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.GetCount(
               new Hashtable() {
                    { "ParticipationObjectId", this.ID.Value },
                    { "ParticipationObjectTypeId", (int)ParticipationObjectType.Activity },
                    { "PlanTypeId", (int)SubscriptionPlanType.PaymentPlan },
                    { "IsActive", true }
               },
               msgs,
               dc) > 0;
        }

        public string HomePageUrl
        {
            get
            {
                return Services.ActivityService.GetHomePageUrl(this, true);
            }
        }

        public string PreviewUrl
        {
            get
            {
                return $"{HomePageUrl}{"?preview=true"}";
            }
        }

        public static List<Activity> LoadList(List<int> ids, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadList(List<int> ids, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadListWithLocations(List<int> ids, List<int> locationIds, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["LocationId"], locationIds, DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        private static DatabaseQuery LoadByAccountIdQuery(int? accountId, int? activityGroupId, int? accountActivityGroupId, ActivityType? activityType, bool? isArchived, bool? isDeleted, bool? isSubmitted, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.UseParameterizedQuery = false;
            dq.Connection = dc;

            if (accountId.HasValue)
            {
                dq.Criterias.Add(ObjectMap["AccountId"], accountId);
            }
            if (activityGroupId.HasValue)
            {
                dq.Criterias.Add(ObjectMap["activityGroupId"], activityGroupId.Value);
            }
            if (accountActivityGroupId.HasValue)
            {
                dq.Criterias.Add(ObjectMap["accountActivityGroupId"], accountActivityGroupId.Value);
            }
            if (activityType.HasValue)
            {
                dq.Criterias.Add(ObjectMap["ActivityTypeId"], (int)activityType.Value);
            }
            if (isSubmitted.HasValue)
            {
                dq.Criterias.Add(ObjectMap["DateSubmittedToActivityGroup"], DBNull.Value, DatabaseCriteriaOperatorEnum.IsNot);
            }
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
            {
                dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted.Value);
            }
            return dq;
        }

        public static int GetCountByAccountId(int AccountId, int? activityGroupId, ActivityType? activityType, bool? isArchived, bool? isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = LoadByAccountIdQuery(AccountId, activityGroupId, null, activityType, isArchived, isDeleted, null, dc);
            return dq.GetCount(msgs);
        }

        public static int GetCountByActivityGroupId(int? activityGroupId, ActivityType? activityType, bool? isArchived, bool? isDeleted, bool? isSubmitted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = LoadByAccountIdQuery(null, activityGroupId, null, activityType, isArchived, isDeleted, isSubmitted, dc);
            return dq.GetCount(msgs);
        }

        public static int GetCountByAccountActivityGroupId(int? accountActivityGroupId, ActivityType? activityType, bool? isArchived, bool? isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = LoadByAccountIdQuery(null, null, accountActivityGroupId, activityType, isArchived, isDeleted, null, dc);
            return dq.GetCount(msgs);
        }

        public static List<Activity> LoadByAccountId(int AccountId, int? activityGroupId, int? accountActivityGroupId, ActivityType? activityType, bool? isArchived, bool? isDeleted, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = LoadByAccountIdQuery(AccountId, activityGroupId, accountActivityGroupId, activityType, isArchived, isDeleted, null, dc: dc);
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

        public bool IsFree
        {
            get
            {
                return !this.CostPerRegistrant.HasValue || this.CostPerRegistrant.Value == 0;
            }
        }

        public bool IsPublishable
        {
            get
            {
                return (Services.ActivityService.GetPercentPublishable(this) == 100.ToDouble() ? true : false) && !IsArchived && (Services.ActivityService.GetActivityGroup(this).IsEmpty() || !Services.ActivityService.GetActivityGroup(this).IsArchived);
            }
        }

        public bool IsPubliclyViewable
        {
            get
            {
                return IsPublished && !IsArchived && (Services.ActivityService.GetActivityGroup(this).IsEmpty() || !Services.ActivityService.GetActivityGroup(this).IsArchived) && !IsDeleted;
            }
        }

        public bool IsGroupArchived
        {
            get
            {
                return Services.ActivityService.GetActivityGroup(this).IsNotEmpty() && Services.ActivityService.GetActivityGroup(this).IsArchived;
            }
        }
        
        public bool HasAddress
        {
            get
            {
                return (!Address1.IsEmpty() && ((!City.IsEmpty() && !State.IsEmpty()) || !PostalCode.IsEmpty()));                
            }
        }          
        
        public bool HasSchedule
        {
            get
            {
                return this.ActivityScheduleId.HasValue;
            }
        }        

        #endregion
    }
}

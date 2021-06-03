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
    /// Either a Session or a League. This object has many Activities or Teams.
    /// </summary>
	
    public class ActivityGroup : BasicLookUpObject
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public RegistrationMode RegistrationModeId { get { return (RegistrationMode)MDV["RegistrationModeId"].Value; } set { MDV["RegistrationModeId"].Value = value; } }
		public string UniqueName { get { return (string)MDV["UniqueName"].Value; } set { MDV["UniqueName"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public int? FormId { get { return (int?)MDV["FormId"].Value; } set { MDV["FormId"].Value = value; } }
		public bool IsPublished { get { return (bool)MDV["IsPublished"].Value; } set { MDV["IsPublished"].Value = value; } }
		public bool IsArchived { get { return (bool)MDV["IsArchived"].Value; } set { MDV["IsArchived"].Value = value; } }
		public bool IsInviteOnly { get { return (bool)MDV["IsInviteOnly"].Value; } set { MDV["IsInviteOnly"].Value = value; } }
		public bool HasLotteryRun { get { return (bool)MDV["HasLotteryRun"].Value; } set { MDV["HasLotteryRun"].Value = value; } }
		public ActivityGroupType ActivityGroupTypeId { get { return (ActivityGroupType)MDV["ActivityGroupTypeId"].Value; } set { MDV["ActivityGroupTypeId"].Value = value; } }
		public int? MinGradeLevelId { get { return (int?)MDV["MinGradeLevelId"].Value; } set { MDV["MinGradeLevelId"].Value = value; } }
		public int? MaxGradeLevelId { get { return (int?)MDV["MaxGradeLevelId"].Value; } set { MDV["MaxGradeLevelId"].Value = value; } }
		public int? AgeStart { get { return (int?)MDV["AgeStart"].Value; } set { MDV["AgeStart"].Value = value; } }
		public int? AgeEnd { get { return (int?)MDV["AgeEnd"].Value; } set { MDV["AgeEnd"].Value = value; } }
		public GenderRestrictionType GenderRestriction { get { return (GenderRestrictionType)MDV["GenderRestriction"].Value; } set { MDV["GenderRestriction"].Value = value; } }
		public int? AdministratorAccountId { get { return (int?)MDV["AdministratorAccountId"].Value; } set { MDV["AdministratorAccountId"].Value = value; } }
		public int? ActivityScheduleId { get { return (int?)MDV["ActivityScheduleId"].Value; } set { MDV["ActivityScheduleId"].Value = value; } }
		public decimal? Cost { get { return (decimal?)MDV["Cost"].Value; } set { MDV["Cost"].Value = value; } }
		public CostUnitType CostUnitTypeId { get { return (CostUnitType)MDV["CostUnitTypeId"].Value; } set { MDV["CostUnitTypeId"].Value = value; } }
		public string ContactName { get { return (string)MDV["ContactName"].Value; } set { MDV["ContactName"].Value = value; } }
		public string ContactPhone { get { return (string)MDV["ContactPhone"].Value; } set { MDV["ContactPhone"].Value = value; } }
		public string ContactEmail { get { return (string)MDV["ContactEmail"].Value; } set { MDV["ContactEmail"].Value = value; } }
		public int? SeatsAvailable { get { return (int?)MDV["SeatsAvailable"].Value; } set { MDV["SeatsAvailable"].Value = value; } }
		public int? WaitingListSeats { get { return (int?)MDV["WaitingListSeats"].Value; } set { MDV["WaitingListSeats"].Value = value; } }
		public bool RequiresParentalConsent { get { return (bool)MDV["RequiresParentalConsent"].Value; } set { MDV["RequiresParentalConsent"].Value = value; } }
		public bool AllowToPayLater { get { return (bool)MDV["AllowToPayLater"].Value; } set { MDV["AllowToPayLater"].Value = value; } }
		public string Details { get { return (string)MDV["Details"].Value; } set { MDV["Details"].Value = value; } }
		public DateTime? StartDate { get { return (DateTime?)MDV["StartDate"].Value; } set { MDV["StartDate"].Value = value; } }
		public DateTime? EndDate { get { return (DateTime?)MDV["EndDate"].Value; } set { MDV["EndDate"].Value = value; } }
		public bool IsDeleted { get { return (bool)MDV["IsDeleted"].Value; } set { MDV["IsDeleted"].Value = value; } }
		public string AccountName { get { return (string)MDV["AccountName"].Value; } }
		

        public ActivityGroup()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(ActivityGroup).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.RegistrationModeId = RegistrationMode.NotSet;
			this.Uid = Guid.NewGuid();
			this.IsPublished = false;
			this.IsArchived = false;
			this.IsInviteOnly = false;
			this.HasLotteryRun = false;
			this.ActivityGroupTypeId = ActivityGroupType.NotSet;
			this.GenderRestriction = GenderRestrictionType.None;
			this.CostUnitTypeId = CostUnitType.NotSet;
			this.RequiresParentalConsent = false;
			this.AllowToPayLater = false;
			this.IsDeleted = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(ActivityGroup).Name;
                _dp = BasicLookUpObject.get_BaseObjectMap(className);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrationModeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "UniqueName", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "FormId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsPublished", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsArchived", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsInviteOnly", System.Data.SqlDbType.Bit);
				_dp.Add(className, "HasLotteryRun", System.Data.SqlDbType.Bit);
				_dp.Add(className, "ActivityGroupTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "MinGradeLevelId", System.Data.SqlDbType.Int);
				_dp.Add(className, "MaxGradeLevelId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AgeStart", System.Data.SqlDbType.Int);
				_dp.Add(className, "AgeEnd", System.Data.SqlDbType.Int);
				_dp.Add(className, "GenderRestriction", System.Data.SqlDbType.Int);
				_dp.Add(className, "AdministratorAccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityScheduleId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Cost", System.Data.SqlDbType.Money);
				_dp.Add(className, "CostUnitTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ContactName", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "ContactPhone", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "ContactEmail", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "SeatsAvailable", System.Data.SqlDbType.Int);
				_dp.Add(className, "WaitingListSeats", System.Data.SqlDbType.Int);
				_dp.Add(className, "RequiresParentalConsent", System.Data.SqlDbType.Bit);
				_dp.Add(className, "AllowToPayLater", System.Data.SqlDbType.Bit);
				_dp.Add(className, "Details", System.Data.SqlDbType.NVarChar , "4000");
				_dp.Add(className, "StartDate", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "EndDate", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "IsDeleted", System.Data.SqlDbType.Bit);
				_dp.Add(className, "AccountName", typeof(Account).Name, "Name", System.Data.SqlDbType.NVarChar, "Account");
				
                return _dp;
            }
        }

        public static ActivityGroup LoadObject(System.Data.DataRow row)
        {
            ActivityGroup obj = new ActivityGroup();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivityGroup).Name);
				dq.Joins.Add(Account.ObjectMap["ID"], ActivityGroup.ObjectMap["AccountId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Account");
				
                return dq;
            }
        }

        public static ActivityGroup Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ActivityGroup> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return ActivityGroup.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return ActivityGroup.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.AccountId.IsEmpty())
				msgs.Add("AccountId is required on ActivityGroup object", MessageType.Error);
			if (this.Uid.IsEmpty())
				msgs.Add("Uid is required on ActivityGroup object", MessageType.Error);
			if (this.Name.IsEmpty())
				msgs.Add("Name is required on ActivityGroup object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ActivityGroup).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ActivityGroup).Name);
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
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrationModeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["UniqueName"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["FormId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsPublished"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsArchived"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsInviteOnly"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["HasLotteryRun"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityGroupTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["MinGradeLevelId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["MaxGradeLevelId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AgeStart"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AgeEnd"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["GenderRestriction"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AdministratorAccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityScheduleId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Cost"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["CostUnitTypeId"]);
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
			dps.Add(ObjectMap["SeatsAvailable"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["WaitingListSeats"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RequiresParentalConsent"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AllowToPayLater"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Details"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StartDate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["EndDate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsDeleted"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public ActivityGroup Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return ActivityGroup.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<ActivityGroup> LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityGroup> LoadByAccountIdByActivityGroupTypeId(int AccountId, ActivityGroupType ActivityGroupTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["ActivityGroupTypeId"], ActivityGroupTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityGroup> LoadByAccountIdByIsPublished(int AccountId, bool IsPublished, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["IsPublished"], IsPublished);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityGroup> LoadByAccountIdByIsPublishedByIsArchived(int AccountId, bool IsPublished, bool IsArchived, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["IsPublished"], IsPublished);
			dq.Criterias.Add(ObjectMap["IsArchived"], IsArchived);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityGroup> LoadByAccountIdByIsPublishedByIsInviteOnly(int AccountId, bool IsPublished, bool IsInviteOnly, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["IsPublished"], IsPublished);
			dq.Criterias.Add(ObjectMap["IsInviteOnly"], IsInviteOnly);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static ActivityGroup LoadByAccountIdByUniqueName(int AccountId, string UniqueName, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["UniqueName"], UniqueName);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static ActivityGroup LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static ActivityGroup LoadByUidByAccountId(Guid Uid, int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static ActivityGroup LoadByUidByIsPublished(Guid Uid, bool IsPublished, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["IsPublished"], IsPublished);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static ActivityGroup LoadByUidByIsPublishedByIsArchived(Guid Uid, bool IsPublished, bool IsArchived, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["IsPublished"], IsPublished);
			dq.Criterias.Add(ObjectMap["IsArchived"], IsArchived);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static ActivityGroup LoadByUidByIsPublishedByIsInviteOnly(Guid Uid, bool IsPublished, bool IsInviteOnly, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["IsPublished"], IsPublished);
			dq.Criterias.Add(ObjectMap["IsInviteOnly"], IsInviteOnly);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<ActivityGroup> LoadByAdministratorAccountId(int AdministratorAccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AdministratorAccountId"], AdministratorAccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        public bool IsFree
        {
            get
            {
                return !this.Cost.HasValue || this.Cost.Value <= 0;
            }
        }
        public string GetHomePageUrl(bool isAfterSchool = true, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return $"{Helpers.Utilities.BaseUrl()}/{this.GetAccount(msgs, dc).UniqueName.ToLower()}/{this.ActivityGroupTypeId.ToString().ToLower()}/{this.UniqueName.ToLower()}";
        }

        public string GetShortHomePageUrl(bool isAfterSchool = true, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return $"{Helpers.Utilities.ShortBaseUrl()}/{this.GetAccount(msgs, dc).UniqueName.ToLower()}/{this.ActivityGroupTypeId.ToString().ToLower()}/{this.UniqueName.ToLower()}";
        }

        public string HomePageUrl
        {
            get
            {
                return this.GetHomePageUrl(true);
            }
        }

        public string ShortHomePageUrl
        {
            get
            {
                return this.GetShortHomePageUrl(true);
            }
        }

        public string BrowseUrl
        {
            get
            {
                return $"{HomePageUrl}/browse";
            }
        }

        public string ShortBrowseUrl
        {
            get
            {
                return $"{ShortHomePageUrl}/browse";
            }
        }

        public bool IsPublishable
        {
            get
            {
                return !IsArchived &&
                    (this.ActivityGroupTypeId == ActivityGroupType.League || !this.GetActivities().Where(x => !x.ActivityScheduleId.HasValue).Any()) &&
                    this.GetRegistrationPeriods().Any();
            }
        }

        public bool IsPubliclyViewable
        {
            get
            {
                return IsPublished && !IsArchived;
            }
        }

        public static List<ActivityGroup> LoadList(List<int> ids, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static ActivityGroup LoadByIdByAccountId(int Id, int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Id"], Id);
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ActivityGroup> LoadByAccountId(int AccountId, ActivityGroupType activityGroupTypeId, int? start, int? length, bool isDeleted = false, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["ActivityGroupTypeId"], (int)activityGroupTypeId);
            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<ActivityGroup> LoadByAccountId(int AccountId, ActivityGroupType activityGroupTypeId, bool isArchived, int? start, int? length, bool isDeleted = false, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["isArchived"], isArchived);
            dq.Criterias.Add(ObjectMap["ActivityGroupTypeId"], (int)activityGroupTypeId);
            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static ActivityGroup LoadByAccountIdByName(int AccountId, string Name, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["Name"], Name);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static bool ExistsByAccountIdByName(int AccountId, string Name, Guid existingUid, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["Name"], Name);
            dq.Criterias.Add(ObjectMap["Uid"], existingUid, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.NotEqualTo);
            return dq.GetCount(msgs) > 0;
        }

        public static string GenerateUniqueName(string name, int? activityGroupId, int? accountId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            //unique name
            var reg = new System.Text.RegularExpressions.Regex(
                BasicLibrary.Utilities.UrlFriendlyRegEx, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

            var origUniqueName = reg.Replace(name.GetMaxWords(25), "");
            string uniqueName = origUniqueName;
            var duplicate = ActivityGroup.LoadByAccountIdByUniqueName(accountId.Value, uniqueName, msgs, dc);
            int count = 1;
            while (duplicate.IsNotEmpty() && duplicate.ID != activityGroupId)
            {
                uniqueName = origUniqueName + count.ToString();
                count++;
                duplicate = ActivityGroup.LoadByAccountIdByUniqueName(accountId.Value, uniqueName, msgs, dc);
            }
            return uniqueName;
        }


        private List<RegistrationPeriod> _RegistrationPeriods = null;
        public List<RegistrationPeriod> GetRegistrationPeriods(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (_RegistrationPeriods.IsEmpty())
                _RegistrationPeriods = RegistrationPeriod.LoadByActivityGroupId(this.ID.Value, msgs, dc);

            return _RegistrationPeriods;
        }

        public List<Registration> GetRegistrations(int? start = null, int? length = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registration.LoadByActivityGroupId(this.ID.Value, start, length, msgs, dc);
        }

        public int GetRegistrationCount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registration.GetCount(new Hashtable() { { "ActivityGroupId", this.ID.Value } }, msgs, dc);
        }

        private List<Activity> _Activities = null;
        public List<Activity> GetActivities(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetActivities(null, false, false, null, null, false, msgs, dc);
        }

        public List<Activity> GetActivities(ActivityType? activityType, bool? isArchived, bool? isDeleted, int? start, int? length, bool forceReload = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (_Activities.IsEmpty() || forceReload)
                _Activities = Activity.LoadByAccountId(this.AccountId.Value, this.ID.Value, null, activityType, isArchived, isDeleted, null, start, length, msgs, dc);

            return _Activities;
        }

        public List<Activity> GetInvitedToActivities(Guid inviteUid, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var activities = GetActivities(msgs, dc);
            var finalList = new List<Activity>();
            var rag = RegistrantActivityGroup.LoadByUid(inviteUid, msgs, dc);

            if (rag.IsEmpty())
                return new List<Activity>();

            var registrantId = rag.RegistrantId;

            activities.ForEach(activity => {
                var exists = RegistrantActivity.ExistsByParticipationObjectByRegistrants(
                    participationObjectId: activity.ID.Value,
                    participationObjectType: ParticipationObjectType.Activity,
                    registrantIds: registrantId.Value.AsList(),
                    assignmentTypes: RegistrationAssignmentType.Invited.AsList(),
                    hasParentalConsent: null,
                    msgs: msgs,
                    dc: dc);

                if (exists)
                    finalList.Add(activity);
            });

            if (!finalList.Any())
                return activities;

            return finalList;
        }

        public List<RegistrantActivity> _RegistrantActivities = null;
        public List<RegistrantActivity> GetRegistrantActivities(List<Registration> registrations = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (registrations.IsEmpty())
                registrations = Registration.LoadByActivityGroupId(this.ID.Value, msgs, dc);

            if (_RegistrantActivities.IsEmpty())
            {
                if (registrations.Any())
                {
                    var registrationIds = registrations.Select(x => x.ID.Value).ToList();
                    _RegistrantActivities = RegistrantActivity.LoadByRegistrationIds(registrationIds, msgs, dc);

                    var activityIds = _RegistrantActivities.Where(x => x.ParticipationObjectTypeId == ParticipationObjectType.Activity)
                                                            .Select(x => x.ParticipationObjectId.Value)
                                                            .ToList();

                    var deletedActivityIds = Activity.LoadList(activityIds, true, msgs, dc).Select(activity => activity.ID.Value);
                    _RegistrantActivities = _RegistrantActivities.Where(x =>
                                                                    x.ParticipationObjectTypeId == ParticipationObjectType.Activity && 
                                                                    x.ParticipationObjectId.Value.NotIn(deletedActivityIds)
                                                                ).ToList();
                }
                else
                    _RegistrantActivities = new List<RegistrantActivity>();
            }

            return _RegistrantActivities;
        }

        public List<Registrant> GetRegistrants(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var registrations = GetRegistrations(null, null, msgs, dc);

            if (registrations.Any())
                return registrations.Select(x => x.ToRegistrant()).DistinctBy(x => x.ID.Value).ToList();

            // backwards compatibility for group registrations that aren't directly related to the group
            return GetRegistrantActivities(null, msgs, dc).Select(x => x.ToRegistrant()).DistinctBy(x => x.ID.Value).ToList();
        }

        public int GetTotalSeatsLeft(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return this.GetActivities(null, false, false, null, null, false, msgs: msgs, dc: dc).Sum(x => x.SeatsAvailable.Value) - this.GetSeatCountByAssignmentTypes(Registration.ReservedStatuses, msgs: msgs, dc: dc);
        }

        public int GetSeatCountByAssignmentType(RegistrationAssignmentType rat, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByAssignmentType(assignmentType: rat, activityGroupId: this.ID.Value, msgs: msgs, dc: dc);
        }

        public int GetSeatCountByAssignmentTypes(List<RegistrationAssignmentType> rats, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByAssignmentTypes(assignmentTypes: rats, activityGroupId: this.ID.Value, msgs: msgs, dc: dc);
        }

        public int GetActivityCount(ActivityType? activityType, bool? isArchived, bool? isDeleted, bool? isSubmitted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (this.ID.HasValue)
                return Activity.GetCountByActivityGroupId(this.ID.Value, activityType, isArchived, isDeleted, isSubmitted, msgs, dc);

            return 0;
        }

        public int GetActivityCountByAccountId(int accountId, ActivityType? activityType, bool? isArchived, bool? isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (this.ID.HasValue)
                return Activity.GetCountByAccountId(accountId, this.ID, activityType, isArchived, isDeleted, null, msgs, dc);

            return 0;
        }

        private Account _Account = null;
        public Account GetAccount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (_Account.IsEmpty())
                _Account = Account.Load(this.AccountId.Value, msgs, dc);

            return _Account;
        }

        public AccountProperty AddOrUpdateGroupProperty(AccountPropertyType type, string value, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var ap = this.GetGroupProperty(type, dc: dc);

            if (ap.IsEmpty())
            {
                ap = new AccountProperty();
                ap.ActivityGroupId = this.ID.Value;
                ap.AccountPropertyTypeId = type;
                ap.CreateDate = DateTime.UtcNow;
                ap.CreatedBy = loggedInUserId;
            }

            ap.Value = value;
            ap.LastModifiedDate = DateTime.UtcNow;
            ap.LastModifiedBy = loggedInUserId;
            msgs.AddRange(ap.Save(dc: dc));

            return ap;
        }

        public AccountProperty GetGroupProperty(AccountPropertyType propertyType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetGroupProperties(msgs, dc: dc).Find(x => x.AccountPropertyTypeId == propertyType);
        }

        private List<AccountProperty> _Properties = null;
        public List<AccountProperty> GetGroupProperties(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (_Properties.IsEmpty())
                _Properties = AccountProperty.LoadByActivityGroupId(this.ID.Value, msgs, dc: dc);

            return _Properties;
        }

        private List<ActivityMessage> _ActivityMessages = null;
        public List<ActivityMessage> GetMessages(DatabaseConnection dc = null)
        {
            if (_ActivityMessages.IsEmpty())
                _ActivityMessages = ActivityMessage.LoadByActivityGroupId(this.ID.Value, dc: dc);

            return _ActivityMessages;
        }

        public int GetInvitedContactCount(DatabaseConnection dc = null)
        {
            return RegistrantActivityGroup.GetCount(
                    criterias: new Hashtable() { { "ActivityGroupId", this.ID.Value } },
                    dc: dc
                ); 
        }
        
        public ActivitySchedule GetSchedule(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (this.ActivityScheduleId.HasValue)
                return ActivitySchedule.LoadWithInstances(this.ActivityScheduleId.Value, msgs, dc);

            return null;
        }

        /// <summary>
        /// Convert to the universal participation object, something a registrant participates in once registered
        /// </summary>
        /// <param name="activityGroup"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public ParticipationObject GetParticipationObject(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return new ParticipationObject(this, msgs, dc);
        }

        public bool HasPaymentPlan(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.GetCount(
               new Hashtable() {
                    { "ParticipationObjectId", this.ID.Value },
                    { "ParticipationObjectTypeId", (int)ParticipationObjectType.ActivityGroup },
                    { "PlanTypeId", (int)SubscriptionPlanType.PaymentPlan },
                    { "IsActive", true }
               },
               msgs,
               dc) > 0;
        }

        #endregion
    }
}

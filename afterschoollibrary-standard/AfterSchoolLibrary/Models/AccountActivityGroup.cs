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
    /// Used when an external account is invited to add an activity or team to another account's session or league
    /// </summary>
	
    public class AccountActivityGroup : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? ActivityGroupId { get { return (int?)MDV["ActivityGroupId"].Value; } set { MDV["ActivityGroupId"].Value = value; } }
		public InviteStatus InviteStatusId { get { return (InviteStatus)MDV["InviteStatusId"].Value; } set { MDV["InviteStatusId"].Value = value; } }
		public bool IsPublished { get { return (bool)MDV["IsPublished"].Value; } set { MDV["IsPublished"].Value = value; } }
		public decimal? Cost { get { return (decimal?)MDV["Cost"].Value; } set { MDV["Cost"].Value = value; } }
		public CostUnitType CostUnitTypeId { get { return (CostUnitType)MDV["CostUnitTypeId"].Value; } set { MDV["CostUnitTypeId"].Value = value; } }
		public string ContactName { get { return (string)MDV["ContactName"].Value; } set { MDV["ContactName"].Value = value; } }
		public string ContactPhone { get { return (string)MDV["ContactPhone"].Value; } set { MDV["ContactPhone"].Value = value; } }
		public string ContactEmail { get { return (string)MDV["ContactEmail"].Value; } set { MDV["ContactEmail"].Value = value; } }
		public int? FormId { get { return (int?)MDV["FormId"].Value; } set { MDV["FormId"].Value = value; } }
		public string AccountName { get { return (string)MDV["AccountName"].Value; } }
		public string ActivityGroupUid { get { return (string)MDV["ActivityGroupUid"].Value; } }
		public ActivityGroupType ActivityGroupActivityGroupTypeId { get { return (ActivityGroupType)MDV["ActivityGroupActivityGroupTypeId"].Value; } }
		public string ActivityGroupUniqueName { get { return (string)MDV["ActivityGroupUniqueName"].Value; } }
		public int? ActivityGroupAccountId { get { return (int?)MDV["ActivityGroupAccountId"].Value; } }
		public bool ActivityGroupIsArchived { get { return (bool)MDV["ActivityGroupIsArchived"].Value; } }
		

        public AccountActivityGroup()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(AccountActivityGroup).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.InviteStatusId = InviteStatus.NotSet;
			this.IsPublished = false;
			this.CostUnitTypeId = CostUnitType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(AccountActivityGroup).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityGroupId", System.Data.SqlDbType.Int);
				_dp.Add(className, "InviteStatusId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsPublished", System.Data.SqlDbType.Bit);
				_dp.Add(className, "Cost", System.Data.SqlDbType.Money);
				_dp.Add(className, "CostUnitTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ContactName", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "ContactPhone", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "ContactEmail", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "FormId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountName", typeof(Account).Name, "Name", System.Data.SqlDbType.NVarChar, "Account");
				_dp.Add(className, "ActivityGroupUid", typeof(ActivityGroup).Name, "Uid", System.Data.SqlDbType.NVarChar, "ActivityGroup");
				_dp.Add(className, "ActivityGroupActivityGroupTypeId", typeof(ActivityGroup).Name, "ActivityGroupTypeId", System.Data.SqlDbType.Int, "ActivityGroup");
				_dp.Add(className, "ActivityGroupUniqueName", typeof(ActivityGroup).Name, "UniqueName", System.Data.SqlDbType.NVarChar, "ActivityGroup");
				_dp.Add(className, "ActivityGroupAccountId", typeof(ActivityGroup).Name, "AccountId", System.Data.SqlDbType.Int, "ActivityGroup");
				_dp.Add(className, "ActivityGroupIsArchived", typeof(ActivityGroup).Name, "IsArchived", System.Data.SqlDbType.Int, "ActivityGroup");
				
                return _dp;
            }
        }

        public static AccountActivityGroup LoadObject(System.Data.DataRow row)
        {
            AccountActivityGroup obj = new AccountActivityGroup();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(AccountActivityGroup).Name);
				dq.Joins.Add(Account.ObjectMap["ID"], AccountActivityGroup.ObjectMap["AccountId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Account");
				dq.Joins.Add(ActivityGroup.ObjectMap["ID"], AccountActivityGroup.ObjectMap["ActivityGroupId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "ActivityGroup");
				
                return dq;
            }
        }

        public static AccountActivityGroup Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<AccountActivityGroup> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return AccountActivityGroup.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return AccountActivityGroup.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.AccountId.IsEmpty())
				msgs.Add("AccountId is required on AccountActivityGroup object", MessageType.Error);
			if (this.ActivityGroupId.IsEmpty())
				msgs.Add("ActivityGroupId is required on AccountActivityGroup object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(AccountActivityGroup).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(AccountActivityGroup).Name);
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
			dps.Add(ObjectMap["ActivityGroupId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["InviteStatusId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsPublished"]);
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
			dps.Add(ObjectMap["FormId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public AccountActivityGroup Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return AccountActivityGroup.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<AccountActivityGroup> LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<AccountActivityGroup> LoadByActivityGroupId(int ActivityGroupId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<AccountActivityGroup> LoadByAccountIdByIsPublished(int AccountId, bool IsPublished, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["IsPublished"], IsPublished);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<AccountActivityGroup> LoadByAccountIdByInviteStatusId(int AccountId, InviteStatus InviteStatusId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["InviteStatusId"], InviteStatusId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static AccountActivityGroup LoadByAccountIdByActivityGroupId(int AccountId, int ActivityGroupId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public bool IsPublishable
        {
            get
            {
                return HasRegistrationPeriods() && ContactEmail.IsNotEmpty() && ContactPhone.IsNotEmpty() && ContactName.IsNotEmpty() && FormId.HasValue;
            }
        }

        public static List<AccountActivityGroup> LoadList(List<int> ids, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public bool HasRegistrationPeriods(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrationPeriod.GetCount(new System.Collections.Hashtable() { { "AccountActivityGroupid", this.ID.Value } }, msgs, dc) > 0;
        }

        public static List<AccountActivityGroup> LoadByAccountId(int AccountId, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        private List<RegistrationPeriod> _RegistrationPeriods = null;
        public List<RegistrationPeriod> GetRegistrationPeriods(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (_RegistrationPeriods.IsEmpty())
                _RegistrationPeriods = RegistrationPeriod.LoadByAccountActivityGroupId(this.ID.Value, msgs, dc);

            return _RegistrationPeriods;
        }

        public SystemMessages UpdateInvitationStatus(InviteStatus inviteStatus, int currentAccountId, DatabaseConnection dc = null)
        {
            this.InviteStatusId = inviteStatus;
            this.LastModifiedBy = currentAccountId;
            this.LastModifiedDate = DateTime.UtcNow;

            return this.Save(dc);
        }

        public ActivityGroup GetActivityGroup(SystemMessages msgs, DatabaseConnection dc = null)
        {
            return ActivityGroup.Load(this.ActivityGroupId.Value, msgs, dc);
        }

        public static bool IsExternalLeague(int accountId, int activityGroupId)
        {
            return AccountActivityGroup.LoadByAccountId(accountId).Where(aag => aag.ActivityGroupId == activityGroupId).Count() > 0;
        }

        public SystemMessages SetIsPublished(bool isPublished, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            this.IsPublished = isPublished;
            this.LastModifiedBy = loggedInUserId;
            this.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(this.Save(dc: dc));

            return msgs;
        }

        public ParticipationObject GetParticipationObject(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return new ParticipationObject(this, msgs, dc);
        }

        public List<Registration> GetRegistrations(int? start = null, int? length = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registration.LoadByAccountActivityGroupId(this.ID.Value, start, length, msgs, dc);
        }

        public int GetRegistrationCount(DatabaseConnection dc = null)
        {
            return Registration.GetCount(criterias: new Hashtable() { { "AccountActivityGroupId", this.ID.Value } }, dc: dc);
        }

        public List<Activity> GetActivities(ActivityType? activityType = null, bool? isArchived = null, bool? isDeleted = null, int? start = null, int? length = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Activity.LoadByAccountId(this.AccountId.Value, null, this.ID, activityType, isArchived, isDeleted, null, start, length, null, msgs, dc);
        }

        public int GetActivityCount(ActivityType? activityType = null, bool? isArchived = null, bool? isDeleted = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Activity.GetCountByAccountActivityGroupId(this.ID.Value, activityType, isArchived, isDeleted, msgs, dc);
        }

        public int GetSeatCountByAssignmentTypes(List<RegistrationAssignmentType> assignmentTypes, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByAssignmentTypes(
                assignmentTypes: assignmentTypes,
                participationObjectId: this.ID.Value,
                participationObjectType: ParticipationObjectType.AccountActivityGroup,
                msgs: msgs,
                dc: dc);
        }

        public int GetSeatCountByAssignmentType(RegistrationAssignmentType assignmentType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByAssignmentType(
                assignmentType: assignmentType,
                participationObjectId: this.ID.Value,
                participationObjectType: ParticipationObjectType.AccountActivityGroup,
                msgs: msgs,
                dc: dc);
        }

        public Form GetForm(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (this.FormId.HasValue)
                return Form.Load(this.FormId.Value, msgs, dc);

            return null;
        }

        public List<RegistrantActivity> GetRegistrantActivities(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByParticipationObject(this.ID.Value, ParticipationObjectType.AccountActivityGroup, msgs, dc);
        }

        public List<Registrant> GetRegistrants(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetRegistrantActivities(msgs, dc).Select(registrantActivity => registrantActivity.ToRegistrant()).ToList();
        }

        #endregion
    }
}

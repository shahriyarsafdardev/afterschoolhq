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
    /// Used to keep track of which host has allowed which visitor access to post activities. Activity Providers can request access to a School or other host account, the host can approve of it.
    /// </summary>
	
    public class AccountRelationship : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? HostAccountId { get { return (int?)MDV["HostAccountId"].Value; } set { MDV["HostAccountId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? VisitorAccountId { get { return (int?)MDV["VisitorAccountId"].Value; } set { MDV["VisitorAccountId"].Value = value; } }
		public HostPermissionType HostPermissionTypeId { get { return (HostPermissionType)MDV["HostPermissionTypeId"].Value; } set { MDV["HostPermissionTypeId"].Value = value; } }
		public ApprovalStatus ApprovalStatusId { get { return (ApprovalStatus)MDV["ApprovalStatusId"].Value; } set { MDV["ApprovalStatusId"].Value = value; } }
		public AccountType AccountAccountTypeId { get { return (AccountType)MDV["AccountAccountTypeId"].Value; } }
		

        public AccountRelationship()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(AccountRelationship).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.HostPermissionTypeId = HostPermissionType.NotSet;
			this.ApprovalStatusId = ApprovalStatus.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(AccountRelationship).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "HostAccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "VisitorAccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "HostPermissionTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ApprovalStatusId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountAccountTypeId", typeof(Account).Name, "AccountTypeId", System.Data.SqlDbType.Int, "Account");
				
                return _dp;
            }
        }

        public static AccountRelationship LoadObject(System.Data.DataRow row)
        {
            AccountRelationship obj = new AccountRelationship();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(AccountRelationship).Name);
				dq.Joins.Add(Account.ObjectMap["ID"], AccountRelationship.ObjectMap["VisitorAccountId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Account");
				
                return dq;
            }
        }

        public static AccountRelationship Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<AccountRelationship> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return AccountRelationship.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return AccountRelationship.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.HostAccountId.IsEmpty())
				msgs.Add("HostAccountId is required on AccountRelationship object", MessageType.Error);
			if (this.VisitorAccountId.IsEmpty())
				msgs.Add("VisitorAccountId is required on AccountRelationship object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(AccountRelationship).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(AccountRelationship).Name);
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
			dps.Add(ObjectMap["HostAccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["VisitorAccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["HostPermissionTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ApprovalStatusId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public AccountRelationship Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return AccountRelationship.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<AccountRelationship> LoadByHostAccountId(int HostAccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<AccountRelationship> LoadByHostAccountIdByApprovalStatusId(int HostAccountId, ApprovalStatus ApprovalStatusId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
			dq.Criterias.Add(ObjectMap["ApprovalStatusId"], ApprovalStatusId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<AccountRelationship> LoadByVisitorAccountId(int VisitorAccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["VisitorAccountId"], VisitorAccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<AccountRelationship> LoadByVisitorAccountIdByApprovalStatusId(int VisitorAccountId, ApprovalStatus ApprovalStatusId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["VisitorAccountId"], VisitorAccountId);
			dq.Criterias.Add(ObjectMap["ApprovalStatusId"], ApprovalStatusId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static AccountRelationship LoadByHostAccountIdByVisitorAccountId(int HostAccountId, int VisitorAccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
			dq.Criterias.Add(ObjectMap["VisitorAccountId"], VisitorAccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<AccountRelationship> LoadByHostAccountIdByHostPermissionTypeId(int HostAccountId, HostPermissionType HostPermissionTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
			dq.Criterias.Add(ObjectMap["HostPermissionTypeId"], HostPermissionTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<AccountRelationship> LoadByVisitorAccountIdByHostPermissionTypeId(int VisitorAccountId, HostPermissionType HostPermissionTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["VisitorAccountId"], VisitorAccountId);
			dq.Criterias.Add(ObjectMap["HostPermissionTypeId"], HostPermissionTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        public static int GetCount(int hostAccountId, List<AccountType> accountTypes, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["HostAccountId"], hostAccountId);
            dq.Criterias.Add(ObjectMap["AccountAccountTypeId"], accountTypes.Select(x => (int)x).ToList(), DatabaseCriteriaOperatorEnum.In);
            return dq.GetCount(msgs);
        }

        public static List<AccountRelationship> LoadByRequested(int HostAccountId, AccountType? accountType, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
            if (accountType.HasValue)
                dq.Criterias.Add(ObjectMap["AccountAccountTypeId"], (int)accountType.Value);
            dq.Criterias.Add(ObjectMap["ApprovalStatusId"], ApprovalStatus.Requested);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<AccountRelationship> LoadByApproved(int HostAccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
            dq.Criterias.Add(ObjectMap["ApprovalStatusId"], ApprovalStatus.Approved);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<AccountRelationship> LoadByApproved(int HostAccountId, AccountType accountType, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
            dq.Criterias.Add(ObjectMap["AccountAccountTypeId"], (int)accountType);
            dq.Criterias.Add(ObjectMap["ApprovalStatusId"], ApprovalStatus.Approved);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<AccountRelationship> LoadByApproved(int HostAccountId, List<AccountType> accountTypes, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
            dq.Criterias.Add(ObjectMap["AccountAccountTypeId"], accountTypes.Select(x => (int)x));
            dq.Criterias.Add(ObjectMap["ApprovalStatusId"], ApprovalStatus.Approved);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<AccountRelationship> LoadByDenied(int HostAccountId, AccountType accountType, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
            dq.Criterias.Add(ObjectMap["AccountAccountTypeId"], (int)accountType);
            dq.Criterias.Add(ObjectMap["ApprovalStatusId"], ApprovalStatus.Denied);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<AccountRelationship> LoadByHostAccountIdByAccountType(int HostAccountId, AccountType accountType, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
            dq.Criterias.Add(ObjectMap["AccountAccountTypeId"], (int)accountType);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<AccountRelationship> LoadByHostAccountIdByAccountTypes(int HostAccountId, List<AccountType> accountTypes, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
            dq.Criterias.Add(ObjectMap["AccountAccountTypeId"], accountTypes.Select(x=> (int)x).ToList(), DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static AccountRelationship LoadByIdByHostAccountId(int ID, int HostAccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ID);
            dq.Criterias.Add(ObjectMap["HostAccountId"], HostAccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        #endregion
    }
}

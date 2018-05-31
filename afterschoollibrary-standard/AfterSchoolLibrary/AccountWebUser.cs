using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Data;
using System.Collections;

namespace AfterSchoolLibrary
{
	
    public class AccountWebUser : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public int? WebUserId { get { return (int?)MDV["WebUserId"].Value; } set { MDV["WebUserId"].Value = value; } }
		public bool IsActive { get { return (bool)MDV["IsActive"].Value; } set { MDV["IsActive"].Value = value; } }
		public bool IsDefault { get { return (bool)MDV["IsDefault"].Value; } set { MDV["IsDefault"].Value = value; } }
		public UserType UserTypeId { get { return (UserType)MDV["UserTypeId"].Value; } set { MDV["UserTypeId"].Value = value; } }
		public string WebUserUsername { get { return (string)MDV["WebUserUsername"].Value; } }
		public string WebUserEmail { get { return (string)MDV["WebUserEmail"].Value; } }
		public string WebUserFirstName { get { return (string)MDV["WebUserFirstName"].Value; } }
		public string WebUserLastName { get { return (string)MDV["WebUserLastName"].Value; } }
		public bool WebUserIsActive { get { return (bool)MDV["WebUserIsActive"].Value; } }
		public bool WebUserIsRegistered { get { return (bool)MDV["WebUserIsRegistered"].Value; } }
		public Guid WebUserUid { get { return (Guid)MDV["WebUserUid"].Value; } }
		public string WebUserPhone { get { return (string)MDV["WebUserPhone"].Value; } }
		public DateTime? WebUserCreateDate { get { return (DateTime?)MDV["WebUserCreateDate"].Value; } }
		public string AccountName { get { return (string)MDV["AccountName"].Value; } }
		public int? AccountOwnerId { get { return (int?)MDV["AccountOwnerId"].Value; } }
		public bool AccountIsAdminAccount { get { return (bool)MDV["AccountIsAdminAccount"].Value; } }
		public DateTime? AccountCreateDate { get { return (DateTime?)MDV["AccountCreateDate"].Value; } }
		public bool AccountIsActive { get { return (bool)MDV["AccountIsActive"].Value; } }
		

        public AccountWebUser()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(AccountWebUser).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.IsActive = false;
			this.IsDefault = false;
			this.UserTypeId = UserType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(AccountWebUser).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "WebUserId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsActive", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsDefault", System.Data.SqlDbType.Bit);
				_dp.Add(className, "UserTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "WebUserUsername", typeof(WebUser).Name, "Username", System.Data.SqlDbType.NVarChar, "WebUser");
				_dp.Add(className, "WebUserEmail", typeof(WebUser).Name, "Email", System.Data.SqlDbType.NVarChar, "WebUser");
				_dp.Add(className, "WebUserFirstName", typeof(WebUser).Name, "FirstName", System.Data.SqlDbType.NVarChar, "WebUser");
				_dp.Add(className, "WebUserLastName", typeof(WebUser).Name, "LastName", System.Data.SqlDbType.NVarChar, "WebUser");
				_dp.Add(className, "WebUserIsActive", typeof(WebUser).Name, "IsActive", System.Data.SqlDbType.Bit, "WebUser");
				_dp.Add(className, "WebUserIsRegistered", typeof(WebUser).Name, "IsRegistered", System.Data.SqlDbType.Bit, "WebUser");
				_dp.Add(className, "WebUserUid", typeof(WebUser).Name, "Uid", System.Data.SqlDbType.UniqueIdentifier, "WebUser");
				_dp.Add(className, "WebUserPhone", typeof(WebUser).Name, "Phone", System.Data.SqlDbType.NVarChar, "WebUser");
				_dp.Add(className, "WebUserCreateDate", typeof(WebUser).Name, "CreateDate", System.Data.SqlDbType.DateTime, "WebUser");
				_dp.Add(className, "AccountName", typeof(Account).Name, "Name", System.Data.SqlDbType.NVarChar, "Account");
				_dp.Add(className, "AccountOwnerId", typeof(Account).Name, "OwnerId", System.Data.SqlDbType.Int, "Account");
				_dp.Add(className, "AccountIsAdminAccount", typeof(Account).Name, "IsAdminAccount", System.Data.SqlDbType.Bit, "Account");
				_dp.Add(className, "AccountCreateDate", typeof(Account).Name, "CreateDate", System.Data.SqlDbType.DateTime, "Account");
				_dp.Add(className, "AccountIsActive", typeof(Account).Name, "IsActive", System.Data.SqlDbType.Bit, "Account");
				
                return _dp;
            }
        }

        protected static AccountWebUser LoadObject(System.Data.DataRow row)
        {
            AccountWebUser obj = new AccountWebUser();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(AccountWebUser).Name);
				dq.Joins.Add(WebUser.ObjectMap["ID"], AccountWebUser.ObjectMap["WebUserId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "WebUser");
				dq.Joins.Add(Account.ObjectMap["ID"], AccountWebUser.ObjectMap["AccountId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Account");
				
                return dq;
            }
        }

        public static AccountWebUser Load(int id, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<AccountWebUser> Load(Hashtable criterias = null, int? start = null, int? length = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return AccountWebUser.BaseLoad(criterias, ObjectMap, dq).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return AccountWebUser.BaseGetCount(criterias, ObjectMap, dq);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Save2(DatabaseQuery, dc, ObjectMap, typeof(AccountWebUser).Name);
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(AccountWebUser).Name);
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
			dps.Add(ObjectMap["WebUserId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsActive"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsDefault"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["UserTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public AccountWebUser Reload(BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return AccountWebUser.Load(this.ID.Value, dc);
		}
		
		public static List<AccountWebUser> LoadByAccountId(int AccountId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<AccountWebUser> LoadByWebUserId(int WebUserId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static AccountWebUser LoadByAccountIdByWebUserId(int AccountId, int WebUserId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static AccountWebUser LoadByWebUserIdByIsDefault(int WebUserId, bool IsDefault, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
			dq.Criterias.Add(ObjectMap["IsDefault"], IsDefault);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<AccountWebUser> LoadByWebUserIdByIsActive(int WebUserId, bool IsActive, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["WebUserId"], WebUserId);
			dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<AccountWebUser> LoadByAccountIdByIsActive(int AccountId, bool IsActive, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"
        public string WebUserDisplayName { get { return BaseWebUser.BuildDisplayName(this.WebUserFirstName, this.WebUserLastName); } }

        public WebUser ToWebUser(bool loadFromDb = false, DatabaseConnection dc = null)
        {
            if (loadFromDb)
            {
                return WebUser.Load(this.WebUserId.Value, dc);
            }
            else
            {
                return new WebUser()
                {
                    ID = this.WebUserId,
                    FirstName = this.WebUserFirstName,
                    LastName = this.WebUserLastName,
                    Email = this.WebUserEmail,
                    Uid = this.WebUserUid,
                    Phone = this.WebUserPhone,
                    Username = this.WebUserUsername,
                    IsActive = this.WebUserIsActive,
                    IsRegistered = this.WebUserIsRegistered,
                    CreateDate = this.WebUserCreateDate
                };
            }
        }
        public Account ToAccount(bool loadFromDb = false, DatabaseConnection dc = null)
        {
            if (loadFromDb)
            {
                return Account.Load(this.AccountId.Value, dc);
            }
            else
            {
                return new Account()
                {
                    ID = this.AccountId,
                    Name = this.AccountName,
                    IsAdminAccount = this.AccountIsAdminAccount,
                    CreateDate = this.AccountCreateDate
                };
            }
        }

        public WebUser GetAccountOwner(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            if (this.AccountOwnerId.HasValue)
            {
                return WebUser.Load(this.AccountOwnerId.Value, dc);
            }
            else
            {
                return new WebUser();
            }
        }

        public bool HasAccessToFeature(FeatureType ft)
        {
            return Permissions.HasAccessToFeature(ft, this.UserTypeId, PermissionObjectType.Account);
        }
        #endregion
    }
}

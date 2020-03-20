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
    /// Where legal details and other info is stored for the account holder, specifically for sending checks
    /// </summary>
	
    public class BankPaymentAccount : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		/// <summary>
		/// Required.
		/// If the bank account is real
		/// </summary>
		public bool IsVerified { get { return (bool)MDV["IsVerified"].Value; } set { MDV["IsVerified"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public string RoutingNumber { get { return (string)MDV["RoutingNumber"].Value; } set { MDV["RoutingNumber"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public string AccountNumber { get { return (string)MDV["AccountNumber"].Value; } set { MDV["AccountNumber"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public string BankName { get { return (string)MDV["BankName"].Value; } set { MDV["BankName"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public string BankCountry { get { return (string)MDV["BankCountry"].Value; } set { MDV["BankCountry"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public BankAccountType BankAccountTypeId { get { return (BankAccountType)MDV["BankAccountTypeId"].Value; } set { MDV["BankAccountTypeId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public string AccountHoldersName { get { return (string)MDV["AccountHoldersName"].Value; } set { MDV["AccountHoldersName"].Value = value; } }
		

        public BankPaymentAccount()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(BankPaymentAccount).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.IsVerified = false;
			this.BankAccountTypeId = BankAccountType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(BankPaymentAccount).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsVerified", System.Data.SqlDbType.Bit);
				_dp.Add(className, "RoutingNumber", System.Data.SqlDbType.NVarChar , "20");
				_dp.Add(className, "AccountNumber", System.Data.SqlDbType.NVarChar , "20");
				_dp.Add(className, "BankName", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "BankCountry", System.Data.SqlDbType.NVarChar , "2");
				_dp.Add(className, "BankAccountTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountHoldersName", System.Data.SqlDbType.NVarChar , "50");
				
                return _dp;
            }
        }

        public static BankPaymentAccount LoadObject(System.Data.DataRow row)
        {
            BankPaymentAccount obj = new BankPaymentAccount();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(BankPaymentAccount).Name);
				
                return dq;
            }
        }

        public static BankPaymentAccount Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<BankPaymentAccount> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return BankPaymentAccount.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return BankPaymentAccount.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.AccountId.IsEmpty())
				msgs.Add("AccountId is required on BankPaymentAccount object", MessageType.Error);
			if (this.RoutingNumber.IsEmpty())
				msgs.Add("RoutingNumber is required on BankPaymentAccount object", MessageType.Error);
			if (this.AccountNumber.IsEmpty())
				msgs.Add("AccountNumber is required on BankPaymentAccount object", MessageType.Error);
			if (this.BankName.IsEmpty())
				msgs.Add("BankName is required on BankPaymentAccount object", MessageType.Error);
			if (this.BankCountry.IsEmpty())
				msgs.Add("BankCountry is required on BankPaymentAccount object", MessageType.Error);
			if (this.BankAccountTypeId.IsEmpty())
				msgs.Add("BankAccountTypeId is required on BankPaymentAccount object", MessageType.Error);
			if (this.AccountHoldersName.IsEmpty())
				msgs.Add("AccountHoldersName is required on BankPaymentAccount object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(BankPaymentAccount).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(BankPaymentAccount).Name);
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
			dps.Add(ObjectMap["IsVerified"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RoutingNumber"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountNumber"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["BankName"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["BankCountry"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["BankAccountTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountHoldersName"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public BankPaymentAccount Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return BankPaymentAccount.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static BankPaymentAccount LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        #endregion
    }
}

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
	[Serializable]
    public class Transaction : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public decimal? Amount { get { return (decimal?)MDV["Amount"].Value; } set { MDV["Amount"].Value = value; } }
		public int? BillerAccountId { get { return (int?)MDV["BillerAccountId"].Value; } set { MDV["BillerAccountId"].Value = value; } }
		public int? PayerAccountId { get { return (int?)MDV["PayerAccountId"].Value; } set { MDV["PayerAccountId"].Value = value; } }
		public SourceType SourceTypeId { get { return (SourceType)MDV["SourceTypeId"].Value; } set { MDV["SourceTypeId"].Value = value; } }
		public TransactionType TransactionTypeId { get { return (TransactionType)MDV["TransactionTypeId"].Value; } set { MDV["TransactionTypeId"].Value = value; } }
		public int? RegistrantSubscriptionId { get { return (int?)MDV["RegistrantSubscriptionId"].Value; } set { MDV["RegistrantSubscriptionId"].Value = value; } }
		public int? RegistrantPaymentPlanId { get { return (int?)MDV["RegistrantPaymentPlanId"].Value; } set { MDV["RegistrantPaymentPlanId"].Value = value; } }
		public string Description { get { return (string)MDV["Description"].Value; } set { MDV["Description"].Value = value; } }
		public int? ActivityId { get { return (int?)MDV["ActivityId"].Value; } set { MDV["ActivityId"].Value = value; } }
		public int? ActivityGroupId { get { return (int?)MDV["ActivityGroupId"].Value; } set { MDV["ActivityGroupId"].Value = value; } }
		

        public Transaction()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Transaction).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.TransactionTypeId = TransactionType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(Transaction).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "Amount", System.Data.SqlDbType.Decimal);
				_dp.Add(className, "BillerAccountId", System.Data.SqlDbType.int);
				_dp.Add(className, "PayerAccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "SourceTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "TransactionTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrantSubscriptionId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrantPaymentPlanId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Description", System.Data.SqlDbType.NVarChar, 400);
				_dp.Add(className, "ActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityGroupId", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        protected static Transaction LoadObject(System.Data.DataRow row)
        {
            Transaction obj = new Transaction();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(Transaction).Name);
				
                return dq;
            }
        }

        public static Transaction Load(int id, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Transaction> Load(Hashtable criterias = null, int? start = null, int? length = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return Transaction.BaseLoad(criterias, ObjectMap, dq).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return Transaction.BaseGetCount(criterias, ObjectMap, dq);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Transaction).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Transaction).Name);
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
			dps.Add(ObjectMap["Amount"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["BillerAccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PayerAccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["SourceTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["TransactionTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrantSubscriptionId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrantPaymentPlanId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Description"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityGroupId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Transaction Reload(BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Transaction.Load(this.ID.Value, dc);
		}
		
		

        #endregion

		#region "UserDefined"

        #endregion
    }
}

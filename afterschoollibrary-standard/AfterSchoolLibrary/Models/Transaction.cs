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
	/// <summary>
    /// Used to keep track of where money is coming and going in the system. A system of record for payments.
    /// </summary>
	[Serializable]
    public class Transaction : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public decimal? Amount { get { return (decimal?)MDV["Amount"].Value; } set { MDV["Amount"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? BillerAccountId { get { return (int?)MDV["BillerAccountId"].Value; } set { MDV["BillerAccountId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? PayerAccountId { get { return (int?)MDV["PayerAccountId"].Value; } set { MDV["PayerAccountId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public PaymentType PaymentTypeId { get { return (PaymentType)MDV["PaymentTypeId"].Value; } set { MDV["PaymentTypeId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public string Description { get { return (string)MDV["Description"].Value; } set { MDV["Description"].Value = value; } }
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public int? PurchaseId { get { return (int?)MDV["PurchaseId"].Value; } set { MDV["PurchaseId"].Value = value; } }
		public decimal? PlatformPercentFee { get { return (decimal?)MDV["PlatformPercentFee"].Value; } set { MDV["PlatformPercentFee"].Value = value; } }
		public decimal? PlatformFlatFee { get { return (decimal?)MDV["PlatformFlatFee"].Value; } set { MDV["PlatformFlatFee"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public TransactionType TransactionTypeId { get { return (TransactionType)MDV["TransactionTypeId"].Value; } set { MDV["TransactionTypeId"].Value = value; } }
		public string StripeChargeOrRefundId { get { return (string)MDV["StripeChargeOrRefundId"].Value; } set { MDV["StripeChargeOrRefundId"].Value = value; } }
		public PaymentSourceType PaymentSourceTypeId { get { return (PaymentSourceType)MDV["PaymentSourceTypeId"].Value; } set { MDV["PaymentSourceTypeId"].Value = value; } }
		

        public Transaction()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Transaction).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.PaymentTypeId = PaymentType.NotSet;
			this.Uid = Guid.NewGuid();
			this.TransactionTypeId = TransactionType.NotSet;
			this.PaymentSourceTypeId = PaymentSourceType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(Transaction).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "Amount", System.Data.SqlDbType.Money);
				_dp.Add(className, "BillerAccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "PayerAccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "PaymentTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Description", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "PurchaseId", System.Data.SqlDbType.Int);
				_dp.Add(className, "PlatformPercentFee", System.Data.SqlDbType.Decimal , "10,2");
				_dp.Add(className, "PlatformFlatFee", System.Data.SqlDbType.Decimal , "10,2");
				_dp.Add(className, "TransactionTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "StripeChargeOrRefundId", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "PaymentSourceTypeId", System.Data.SqlDbType.Int);
				
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

        public static Transaction Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Transaction> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return Transaction.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return Transaction.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.Amount.IsEmpty())
				msgs.Add("Amount is required on Transaction object", MessageType.Error);
			if (this.BillerAccountId.IsEmpty())
				msgs.Add("BillerAccountId is required on Transaction object", MessageType.Error);
			if (this.PayerAccountId.IsEmpty())
				msgs.Add("PayerAccountId is required on Transaction object", MessageType.Error);
			if (this.PaymentTypeId.IsEmpty())
				msgs.Add("PaymentTypeId is required on Transaction object", MessageType.Error);
			if (this.Description.IsEmpty())
				msgs.Add("Description is required on Transaction object", MessageType.Error);
			if (this.TransactionTypeId.IsEmpty())
				msgs.Add("TransactionTypeId is required on Transaction object", MessageType.Error);
			
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
			dps.Add(ObjectMap["PaymentTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Description"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PurchaseId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PlatformPercentFee"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PlatformFlatFee"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["TransactionTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StripeChargeOrRefundId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PaymentSourceTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Transaction Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Transaction.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<Transaction> LoadByBillerAccountId(int BillerAccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Transaction> LoadByPayerAccountId(int PayerAccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["PayerAccountId"], PayerAccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Transaction LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<Transaction> LoadByBillerAccountIdByTransactionTypeId(int BillerAccountId, TransactionType TransactionTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId);
			dq.Criterias.Add(ObjectMap["TransactionTypeId"], TransactionTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Transaction> LoadByPayerAccountIdByTransactionTypeId(int PayerAccountId, TransactionType TransactionTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["PayerAccountId"], PayerAccountId);
			dq.Criterias.Add(ObjectMap["TransactionTypeId"], TransactionTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Transaction LoadByStripeChargeOrRefundId(string StripeChargeOrRefundId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["StripeChargeOrRefundId"], StripeChargeOrRefundId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public static decimal GetTotalAmountReceivedByAccountId(int accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByBillerAccountId(accountId, msgs, dc).Sum(transaction => transaction.Amount.Value);
        }
        public static decimal GetTotalAmountPaidByAccountId(int accountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Transaction.LoadByPayerAccountId(accountId, msgs, dc).Sum(transaction => transaction.Amount.Value);
        }

        public static List<Transaction> LoadByBillerAccountId(int BillerAccountId, DateTime? startDate, DateTime? endDate, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);
            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            dq.OrderBys.Add(ObjectMap["CreateDate"], OrderByDirectionEnum.Descending);

            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Transaction> LoadByPayerAccountId(int PayerAccountId, DateTime? startDate, DateTime? endDate, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["PayerAccountId"], PayerAccountId);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);
            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            dq.OrderBys.Add(ObjectMap["CreateDate"], OrderByDirectionEnum.Descending);

            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCountByBillerAccountId(int BillerAccountId, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);
            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            return dq.GetCount();
        }

        public static int GetCountByPayerAccountId(int PayerAccountId, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["PayerAccountId"], PayerAccountId);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);
            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            return dq.GetCount();
        }

        #endregion
    }
}

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
		public decimal? PaymentTypeFeeCharged { get { return (decimal?)MDV["PaymentTypeFeeCharged"].Value; } set { MDV["PaymentTypeFeeCharged"].Value = value; } }
		public decimal? ConvenienceFeeCharged { get { return (decimal?)MDV["ConvenienceFeeCharged"].Value; } set { MDV["ConvenienceFeeCharged"].Value = value; } }
		/// <summary>
		/// which account is paying the payment type (stripe or ach) fee
		/// </summary>
		public int? PaymentTypeFeePayerAccountId { get { return (int?)MDV["PaymentTypeFeePayerAccountId"].Value; } set { MDV["PaymentTypeFeePayerAccountId"].Value = value; } }
		/// <summary>
		/// which account is paying the convenience fee
		/// </summary>
		public int? ConvenienceFeePayerAccountId { get { return (int?)MDV["ConvenienceFeePayerAccountId"].Value; } set { MDV["ConvenienceFeePayerAccountId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public TransactionType TransactionTypeId { get { return (TransactionType)MDV["TransactionTypeId"].Value; } set { MDV["TransactionTypeId"].Value = value; } }
		public string StripeChargeOrRefundId { get { return (string)MDV["StripeChargeOrRefundId"].Value; } set { MDV["StripeChargeOrRefundId"].Value = value; } }
		public PaymentSourceType PaymentSourceTypeId { get { return (PaymentSourceType)MDV["PaymentSourceTypeId"].Value; } set { MDV["PaymentSourceTypeId"].Value = value; } }
		public int? OriginalTransactionId { get { return (int?)MDV["OriginalTransactionId"].Value; } set { MDV["OriginalTransactionId"].Value = value; } }
		/// <summary>
		/// If associated to a payout then it cannot be requested for another payout
		/// </summary>
		public int? PayoutId { get { return (int?)MDV["PayoutId"].Value; } set { MDV["PayoutId"].Value = value; } }
		public bool IsDeleted { get { return (bool)MDV["IsDeleted"].Value; } set { MDV["IsDeleted"].Value = value; } }
		public bool UsedStripeConnect { get { return (bool)MDV["UsedStripeConnect"].Value; } set { MDV["UsedStripeConnect"].Value = value; } }
		public decimal? PlatformPercentFee { get { return (decimal?)MDV["PlatformPercentFee"].Value; } set { MDV["PlatformPercentFee"].Value = value; } }
		public decimal? PlatformFlatFee { get { return (decimal?)MDV["PlatformFlatFee"].Value; } set { MDV["PlatformFlatFee"].Value = value; } }
		public int? CouponId { get { return (int?)MDV["CouponId"].Value; } set { MDV["CouponId"].Value = value; } }
		public decimal? CouponValue { get { return (decimal?)MDV["CouponValue"].Value; } set { MDV["CouponValue"].Value = value; } }
		

        public Transaction()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Transaction).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.PaymentTypeId = PaymentType.NotSet;
			this.Uid = Guid.NewGuid();
			this.TransactionTypeId = TransactionType.NotSet;
			this.PaymentSourceTypeId = PaymentSourceType.NotSet;
			this.IsDeleted = false;
			this.UsedStripeConnect = false;
			
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
				_dp.Add(className, "PaymentTypeFeeCharged", System.Data.SqlDbType.Decimal , "10,2");
				_dp.Add(className, "ConvenienceFeeCharged", System.Data.SqlDbType.Decimal , "10,2");
				_dp.Add(className, "PaymentTypeFeePayerAccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ConvenienceFeePayerAccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "TransactionTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "StripeChargeOrRefundId", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "PaymentSourceTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "OriginalTransactionId", System.Data.SqlDbType.Int);
				_dp.Add(className, "PayoutId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsDeleted", System.Data.SqlDbType.Bit);
				_dp.Add(className, "UsedStripeConnect", System.Data.SqlDbType.Bit);
				_dp.Add(className, "PlatformPercentFee", System.Data.SqlDbType.Decimal , "10,2");
				_dp.Add(className, "PlatformFlatFee", System.Data.SqlDbType.Decimal , "10,2");
				_dp.Add(className, "CouponId", System.Data.SqlDbType.Int);
				_dp.Add(className, "CouponValue", System.Data.SqlDbType.Decimal , "10,2");
				
                return _dp;
            }
        }

        public static Transaction LoadObject(System.Data.DataRow row)
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
			dps.Add(ObjectMap["PaymentTypeFeeCharged"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ConvenienceFeeCharged"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PaymentTypeFeePayerAccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ConvenienceFeePayerAccountId"]);
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

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["OriginalTransactionId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PayoutId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsDeleted"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["UsedStripeConnect"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PlatformPercentFee"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PlatformFlatFee"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["CouponId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["CouponValue"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Transaction Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Transaction.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<Transaction> LoadByBillerAccountIdByIsDeleted(int BillerAccountId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Transaction> LoadByPayerAccountIdByIsDeleted(int PayerAccountId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["PayerAccountId"], PayerAccountId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Transaction LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static Transaction LoadByUidByBillerAccountId(Guid Uid, int BillerAccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static Transaction LoadByUidByPayerAccountId(Guid Uid, int PayerAccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["PayerAccountId"], PayerAccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<Transaction> LoadByBillerAccountIdByTransactionTypeIdByIsDeleted(int BillerAccountId, TransactionType TransactionTypeId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId);
			dq.Criterias.Add(ObjectMap["TransactionTypeId"], TransactionTypeId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Transaction> LoadByPayerAccountIdByTransactionTypeIdByIsDeleted(int PayerAccountId, TransactionType TransactionTypeId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["PayerAccountId"], PayerAccountId);
			dq.Criterias.Add(ObjectMap["TransactionTypeId"], TransactionTypeId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Transaction LoadByStripeChargeOrRefundId(string StripeChargeOrRefundId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["StripeChargeOrRefundId"], StripeChargeOrRefundId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<Transaction> LoadByOriginalTransactionIdByIsDeleted(int OriginalTransactionId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["OriginalTransactionId"], OriginalTransactionId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Transaction> LoadByPayoutIdByIsDeleted(int PayoutId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["PayoutId"], PayoutId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Transaction> LoadByCouponIdByIsDeleted(int CouponId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["CouponId"], CouponId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Transaction> LoadByCouponIdByPayerAccountIdByIsDeleted(int CouponId, int PayerAccountId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["CouponId"], CouponId);
			dq.Criterias.Add(ObjectMap["PayerAccountId"], PayerAccountId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Transaction> LoadByCouponIdByBillerAccountIdByIsDeleted(int CouponId, int BillerAccountId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["CouponId"], CouponId);
			dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        public bool HasPayout
        {
            get
            {
                return PayoutId.HasValue;
            }
        }

        public static List<Transaction> LoadList(List<int> ids, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, DatabaseCriteriaOperatorEnum.In);           
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Transaction> LoadByDate(DateTime? startDate, DateTime? endDate, bool isDeleted, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;

            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            dq.OrderBys.Add(ObjectMap["CreateDate"], OrderByDirectionEnum.Descending);
            dq.OrderBys.Add(ObjectMap["Description"], OrderByDirectionEnum.Ascending);

            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Transaction> LoadByAccountId(int accountId, string search, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, bool isDeleted, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return LoadByBillerOrPayerAccountId(accountId, accountId, search, startDate, endDate, participationObjects, isDeleted, start, length, msgs, dc);
        }

        public static List<Transaction> LoadByBillerAccountId(int BillerAccountId, string search, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, bool isDeleted, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return LoadByBillerOrPayerAccountId(BillerAccountId, null, search, startDate, endDate, participationObjects, isDeleted, start, length, msgs, dc);
        }

        public static List<Transaction> LoadByPayerAccountId(int PayerAccountId, string search, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, bool isDeleted, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return LoadByBillerOrPayerAccountId(null, PayerAccountId, search, startDate, endDate, participationObjects, isDeleted, start, length, msgs, dc);
        }

        private static List<Transaction> LoadByBillerOrPayerAccountId(int? BillerAccountId, int? PayerAccountId, string search, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, bool isDeleted, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = BuildSearchQuery(BillerAccountId, PayerAccountId, search, startDate, endDate, participationObjects, isDeleted, msgs, dc);

            dq.OrderBys.Add(ObjectMap["CreateDate"], OrderByDirectionEnum.Descending);
            dq.OrderBys.Add(ObjectMap["Description"], OrderByDirectionEnum.Ascending);

            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        private static DatabaseQuery GetParticipationObjectQuery(List<(int ID, ParticipationObjectType ObjectType)> participationObjects)
        {
            var activityPaymentQuery = ActivityPayment.get_DatabaseQuery(ActivityPayment.ObjectMap, typeof(ActivityPayment).Name);
            activityPaymentQuery.UseParameterizedQuery = false;
            activityPaymentQuery.SelectParameters.Clear();
            activityPaymentQuery.SelectParameters.Add(
                ActivityPayment.ObjectMap["ID"],
                null,
                $"COUNT({ActivityPayment.ObjectMap["ID"].ParameterFullName})");

            activityPaymentQuery.Criterias.Add(ActivityPayment.ObjectMap["TransactionId"], 0, Transaction.ObjectMap["ID"].ParameterFullName);

            var dcs = new DatabaseCriterias(LogicalOperatorType.Or);

            participationObjects.ForEach(participationObject => {
                var dcs2 = new DatabaseCriterias(LogicalOperatorType.And);
                dcs2.Add(ActivityPayment.ObjectMap["ParticipationObjectId"], participationObject.ID);
                dcs2.Add(ActivityPayment.ObjectMap["ParticipationObjectTypeId"], (int)participationObject.ObjectType);
                dcs.Add(dcs2);
            });

            activityPaymentQuery.Criterias.Add(dcs);

            return activityPaymentQuery;
        }

        public static int GetCount(DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;

            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);
            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            return dq.GetCount(msgs);
        }

        public static int GetCountByBillerAccountId(int BillerAccountId, string search, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetCountByBillerOrPayerAccountId(BillerAccountId, null, search, startDate, endDate, participationObjects, isDeleted, msgs, dc);
        }

        public static int GetCountByPayerAccountId(int PayerAccountId, string search, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, bool isDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return GetCountByBillerOrPayerAccountId(null, PayerAccountId, search, startDate, endDate, participationObjects, isDeleted, msgs, dc);
        }

        public static int GetCountByAccountId(int accountId, string search, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, bool isDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return GetCountByBillerOrPayerAccountId(accountId, accountId, search, startDate, endDate, participationObjects, isDeleted, msgs, dc);
        }

        private static DatabaseQuery BuildSearchQuery(int? BillerAccountId, int? PayerAccountId, string search, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, bool isDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;

            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);

            if (BillerAccountId.HasValue && PayerAccountId.HasValue)
            {
                var dcs = new DatabaseCriterias(LogicalOperatorType.Or);
                dcs.Add(ObjectMap["BillerAccountId"], BillerAccountId.Value);
                dcs.Add(ObjectMap["PayerAccountId"], PayerAccountId.Value);
                dq.Criterias.Add(dcs);
            }
            else
            {
                if (BillerAccountId.HasValue)
                    dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId.Value);

                if (PayerAccountId.HasValue)
                    dq.Criterias.Add(ObjectMap["PayerAccountId"], PayerAccountId.Value);
            }

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);
            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            if (search.IsNotBlank())
                AddSearchQueryJoins(dq, search);

            if (participationObjects.IsNotEmpty() && participationObjects.Count > 0)
                dq.Criterias.Add(
                    GetParticipationObjectQuery(participationObjects),
                    0,
                    DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanParameter);

            return dq;
        }

        internal static void AddSearchQueryJoins(DatabaseQuery dq, string search)
        {
            var searchDcs = new DatabaseCriterias(LogicalOperatorType.Or);
            searchDcs.Add(Transaction.ObjectMap["Description"], search, DatabaseCriteriaOperatorEnum.Like);

            // Account Search
            dq.Joins.Add(Account.ObjectMap["ID"], Transaction.ObjectMap["PayerAccountId"], DatabaseJoinTypeEnum.InnerJoin);
            var accountDcs = new DatabaseCriterias(LogicalOperatorType.Or);

            accountDcs.Add(Account.ObjectMap["Name"], search, DatabaseCriteriaOperatorEnum.Like);
            search.Split(' ').ToList().ForEach(item => accountDcs.Add(Account.ObjectMap["Name"], item, DatabaseCriteriaOperatorEnum.Like));

            // WebUser Search
            dq.Joins.Add(WebUser.ObjectMap["ID"], Account.ObjectMap["OwnerId"], DatabaseJoinTypeEnum.InnerJoin);

            var webUserDcs = new DatabaseCriterias(LogicalOperatorType.Or);

            webUserDcs.Add(WebUser.ObjectMap["FirstName"], search, DatabaseCriteriaOperatorEnum.Like);
            webUserDcs.Add(WebUser.ObjectMap["LastName"], search, DatabaseCriteriaOperatorEnum.Like);
            webUserDcs.Add(WebUser.ObjectMap["Email"], search, DatabaseCriteriaOperatorEnum.Like);

            search.Split(' ').ToList().ForEach(item => {
                webUserDcs.Add(WebUser.ObjectMap["FirstName"], item, DatabaseCriteriaOperatorEnum.Like);
                webUserDcs.Add(WebUser.ObjectMap["LastName"], item, DatabaseCriteriaOperatorEnum.Like);
                webUserDcs.Add(WebUser.ObjectMap["Email"], item, DatabaseCriteriaOperatorEnum.Like);
            });

            // Registrant Search
            dq.Joins.Add(Purchase.ObjectMap["ID"], Transaction.ObjectMap["PurchaseId"], DatabaseJoinTypeEnum.LeftOuterJoin);
            dq.Joins.Add(Registrant.ObjectMap["ID"], Purchase.ObjectMap["RegistrantId"], DatabaseJoinTypeEnum.LeftOuterJoin);

            var registrantDcs = new DatabaseCriterias(LogicalOperatorType.Or);
            registrantDcs.Add(Registrant.ObjectMap["FirstName"], search, DatabaseCriteriaOperatorEnum.Like);
            registrantDcs.Add(Registrant.ObjectMap["LastName"], search, DatabaseCriteriaOperatorEnum.Like);
            registrantDcs.Add(Registrant.ObjectMap["Email"], search, DatabaseCriteriaOperatorEnum.Like);

            search.Split(' ').ToList().ForEach(item => {
                registrantDcs.Add(Registrant.ObjectMap["FirstName"], item, DatabaseCriteriaOperatorEnum.Like);
                registrantDcs.Add(Registrant.ObjectMap["LastName"], item, DatabaseCriteriaOperatorEnum.Like);
                registrantDcs.Add(Registrant.ObjectMap["Email"], item, DatabaseCriteriaOperatorEnum.Like);
            });

            searchDcs.Add(accountDcs);
            searchDcs.Add(webUserDcs);
            searchDcs.Add(registrantDcs);

            dq.Criterias.Add(searchDcs);
        }

        private static int GetCountByBillerOrPayerAccountId(int? BillerAccountId, int? PayerAccountId, string search, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, bool isDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return BuildSearchQuery(BillerAccountId, PayerAccountId, search, startDate, endDate, participationObjects, isDeleted, msgs, dc).GetCount(msgs);
        }

        public static List<(int ID, ParticipationObjectType ParticipationObjectTypeId, string Name)> GetParticipationObjectsByBillerOrPayerAccountId(int? billerAccountId, int? payerAccountId, DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;

            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);

            if (billerAccountId.HasValue)
                dq.Criterias.Add(ObjectMap["billerAccountId"], billerAccountId.Value);

            if (payerAccountId.HasValue)
                dq.Criterias.Add(ObjectMap["PayerAccountId"], payerAccountId.Value);

            dq.Joins.Add(ActivityPayment.ObjectMap["TransactionId"], ObjectMap["ID"], DatabaseJoinTypeEnum.InnerJoin);

            dq.SelectParameters.Clear();
            dq.SelectParameters.Add(ActivityPayment.ObjectMap["ParticipationObjectId"]);
            dq.SelectParameters.Add(ActivityPayment.ObjectMap["ParticipationObjectTypeId"]);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);
            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            var list = new List<(int, ParticipationObjectType, string)>();
            if (results.Rows.Count > 0)
                foreach (DataRow row in results.Rows)
                    list.Add((
                            (int)row[0],
                            (ParticipationObjectType)row[1],
                            new ParticipationObject
                            (
                                participationObjectId: (int)row[0], 
                                participationObjectType: (ParticipationObjectType)row[1],
                                loadAllDetails: false,
                                msgs: msgs,
                                dc: dc
                            ).Name
                        ));

            return list;
        }

        public static int GetCountByPurchaseId(int PurchaseId, bool isDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["PurchaseId"], PurchaseId);
            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);

            return dq.GetCount(msgs);
        }

        public static decimal GetTotalFeesPaid(DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["PlatformFlatFee"].ParameterFullName} + ({ObjectMap["PlatformPercentFee"].ParameterFullName} * {ObjectMap["Amount"].ParameterFullName})), 0) + ISNULL(SUM({ObjectMap["PaymentTypeFeeCharged"].ParameterFullName} + {ObjectMap["ConvenienceFeeCharged"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            dq.Criterias.Add(ObjectMap["TransactionTypeId"], (int)TransactionType.Charge);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalAHQProfit(DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $@"
                            ISNULL(SUM({ObjectMap["PlatformFlatFee"].ParameterFullName} + ({ObjectMap["PlatformPercentFee"].ParameterFullName} * {ObjectMap["Amount"].ParameterFullName} - ({Billing.StripeHelper.StripePercentFee} * {ObjectMap["Amount"].ParameterFullName}) - {Billing.StripeHelper.StripeFlatFee})), 0) + 
                            ISNULL(SUM({ObjectMap["PaymentTypeFeeCharged"].ParameterFullName} + {ObjectMap["ConvenienceFeeCharged"].ParameterFullName} - ({Billing.StripeHelper.StripePercentFee} * {ObjectMap["Amount"].ParameterFullName}) - {Billing.StripeHelper.StripeFlatFee}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            dq.Criterias.Add(ObjectMap["TransactionTypeId"], (int)TransactionType.Charge);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalFeesPaidByBillerAccount(int BillerAccountId, string search, PaymentType? paymentType, DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["PlatformFlatFee"].ParameterFullName} + ({ObjectMap["PlatformPercentFee"].ParameterFullName} * {ObjectMap["Amount"].ParameterFullName})), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId);
            dq.Criterias.Add(ObjectMap["TransactionTypeId"], (int)TransactionType.Charge);

            //.. the way the code used to be was that a transaction either had the platform percent fee + flatfee or it had the ConvenienceFee + paymenttypefee
            //.. We are now storing the platformfee + flat fee all the time, so it is mistakenly using that old calculation which is not valid for new accounts
            //.. so it should only use PlatformFees if there is no convenience fee (and really this is for only old accounts)
            //.. in sort, if we have ConvenienceFeeCharged <=0 or Null, return 0 and if there is ConvenienceFeeCharged > 0, take it as it is since it is calculated charges
            var dcs = new DatabaseCriterias(LogicalOperatorType.Or);
            dcs.Add(ObjectMap["ConvenienceFeeCharged"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
            dcs.Add(ObjectMap["ConvenienceFeeCharged"], 0, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);
            dq.Criterias.Add(dcs);

            if (paymentType.HasValue)
                dq.Criterias.Add(ObjectMap["PaymentTypeId"], (int)paymentType.Value);

            if (search.IsNotBlank())
                AddSearchQueryJoins(dq, search);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal() + 
                    GetTotalConvenienceFeesPaidByBillerAccount(BillerAccountId, paymentType, startDate, endDate, isDeleted, msgs, dc) +
                    GetTotalPaymentTypeFeesPaidByBillerAccount(BillerAccountId, paymentType, startDate, endDate, isDeleted, msgs, dc);

            return 0;
        }

        public static decimal GetTotalAmountBilledOut(DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["Amount"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            dq.Criterias.Add(ObjectMap["TransactionTypeId"], (int)TransactionType.Charge);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalAmountRefunded(DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["Amount"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            dq.Criterias.Add(ObjectMap["TransactionTypeId"], (int)TransactionType.Refund);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        /// <summary>
        /// calculates the total "amount" that was billed by the biller account, not the amount that was collected
        /// this is why we have to subtract the fees paid by the payer out of the transaction amount
        /// </summary>
        /// <param name="BillerAccountId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static decimal GetTotalAmountBilledByBillerAccount(int BillerAccountId, string search, PaymentSourceType? paymentSourceType, PaymentType? paymentType, DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["Amount"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId);

            if (paymentSourceType.HasValue)
                dq.Criterias.Add(ObjectMap["PaymentSourceTypeId"], (int)paymentSourceType.Value);

            if (paymentType.HasValue)
                dq.Criterias.Add(ObjectMap["PaymentTypeId"], (int)paymentType.Value);

            dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);

            if (search.IsNotBlank())
                AddSearchQueryJoins(dq, search);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal() -
                    GetTotalConvenienceFeesNotPaidByBillerAccount(BillerAccountId, paymentSourceType, paymentType, startDate, endDate, isDeleted, msgs, dc) -
                    GetTotalPaymentTypeFeesNotPaidByBillerAccount(BillerAccountId, paymentSourceType, paymentType, startDate, endDate, isDeleted, msgs, dc);

            return 0;
        }

        public static decimal GetTotalConvenienceFeesNotPaidByBillerAccount(int BillerAccountId, PaymentSourceType? paymentSourceType, PaymentType? paymentType, DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["ConvenienceFeeCharged"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId);
            dq.Criterias.Add(ObjectMap["ConvenienceFeePayerAccountId"], BillerAccountId, DatabaseCriteriaOperatorEnum.NotEqualTo);
            dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);

            if (paymentSourceType.HasValue)
                dq.Criterias.Add(ObjectMap["PaymentSourceTypeId"], (int)paymentSourceType.Value);

            if (paymentType.HasValue)
                dq.Criterias.Add(ObjectMap["PaymentTypeId"], (int)paymentType.Value);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalConvenienceFeesNotPaidByPayerAccount(int PayerAccountId, string search, DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["ConvenienceFeeCharged"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["PayerAccountId"], PayerAccountId);
            dq.Criterias.Add(ObjectMap["ConvenienceFeePayerAccountId"], PayerAccountId, DatabaseCriteriaOperatorEnum.NotEqualTo);
            dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);

            if (search.IsNotBlank())
                AddSearchQueryJoins(dq, search);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalPaymentTypeFeesNotPaidByBillerAccount(int BillerAccountId, PaymentSourceType? paymentSourceType, PaymentType? paymentType, DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["PaymentTypeFeeCharged"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId);
            dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);
            dq.Criterias.Add(ObjectMap["PaymentTypeFeePayerAccountId"], BillerAccountId, DatabaseCriteriaOperatorEnum.NotEqualTo);

            if (paymentSourceType.HasValue)
                dq.Criterias.Add(ObjectMap["PaymentSourceTypeId"], (int)paymentSourceType.Value);

            if (paymentType.HasValue)
                dq.Criterias.Add(ObjectMap["PaymentTypeId"], (int)paymentType.Value);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalPaymentTypeFeesNotPaidByPayerAccount(int PayerAccountId, string search, DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["PaymentTypeFeeCharged"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["PayerAccountId"], PayerAccountId);
            dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);
            dq.Criterias.Add(ObjectMap["PaymentTypeFeePayerAccountId"], PayerAccountId, DatabaseCriteriaOperatorEnum.NotEqualTo);

            if (search.IsNotBlank())
                AddSearchQueryJoins(dq, search);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalConvenienceFeesPaidByBillerAccount(int BillerAccountId, PaymentType? paymentType, DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["ConvenienceFeeCharged"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["ConvenienceFeePayerAccountId"], BillerAccountId);
            dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId);
            dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);

            if (paymentType.HasValue)
                dq.Criterias.Add(ObjectMap["PaymentTypeId"], (int)paymentType.Value);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalPaymentTypeFeesPaidByBillerAccount(int BillerAccountId, PaymentType? paymentType, DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["PaymentTypeFeeCharged"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["PaymentTypeFeePayerAccountId"], BillerAccountId);
            dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId);
            dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);

            if (paymentType.HasValue)
                dq.Criterias.Add(ObjectMap["PaymentTypeId"], (int)paymentType.Value);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalConvenienceFeesReceivedByBillerAccount(int BillerAccountId, PaymentType? paymentType, DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["ConvenienceFeeCharged"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId);
            dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);

            if (paymentType.HasValue)
                dq.Criterias.Add(ObjectMap["PaymentTypeId"], (int)paymentType.Value);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalAmountPaidByPayerAccount(int PayerAccountId, string search, PaymentSourceType? paymentSourceType, DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {

            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["Amount"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["PayerAccountId"], PayerAccountId);
            dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);

            if (paymentSourceType.HasValue)
                dq.Criterias.Add(ObjectMap["PaymentSourceTypeId"], (int)paymentSourceType.Value);

            if (search.IsNotBlank())
                AddSearchQueryJoins(dq, search);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalConvenienceFeesPaidByPayerAccount(int PayerAccountId, string search, PaymentType? paymentType, DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["ConvenienceFeeCharged"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["ConvenienceFeePayerAccountId"], PayerAccountId);
            dq.Criterias.Add(ObjectMap["PayerAccountId"], PayerAccountId);
            dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);

            if (search.IsNotBlank())
                AddSearchQueryJoins(dq, search);

            if (paymentType.HasValue)
                dq.Criterias.Add(ObjectMap["PaymentTypeId"], (int)paymentType.Value);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalPaymentTypeFeesPaidByPayerAccount(int PayerAccountId, string search, PaymentType? paymentType, DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["PaymentTypeFeeCharged"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["PaymentTypeFeePayerAccountId"], PayerAccountId);
            dq.Criterias.Add(ObjectMap["PayerAccountId"], PayerAccountId);
            dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);

            if (search.IsNotBlank())
                AddSearchQueryJoins(dq, search);

            if (paymentType.HasValue)
                dq.Criterias.Add(ObjectMap["PaymentTypeId"], (int)paymentType.Value);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalRefundByOriginalTransaction(int OriginalTransactionId, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["Amount"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["OriginalTransactionId"], OriginalTransactionId);
            dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalRefundByOriginalTransaction(int OriginalTransactionId, PaymentSourceType paymentSourceType, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {

            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["Amount"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["OriginalTransactionId"], OriginalTransactionId);
            dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);
            dq.Criterias.Add(ObjectMap["PaymentSourceTypeId"], (int)paymentSourceType);

            var results = dq.Select(msgs);
            if (results.IsNotEmpty() && results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalRefundFeesByOriginalTransaction(int OriginalTransactionId, int? feePayerId, PaymentSourceType paymentSourceType, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {

            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["ConvenienceFeeCharged"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["ConvenienceFeeCharged"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            var sp2 = new SelectParameter(ObjectMap["PaymentTypeFeeCharged"]);
            sp2.Function = $"ISNULL(SUM({ObjectMap["PaymentTypeFeeCharged"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp2);

            var sp3 = new SelectParameter(ObjectMap["ConvenienceFeePayerAccountId"]);
            dq.SelectParameters.Add(sp3);

            var sp4 = new SelectParameter(ObjectMap["PaymentTypeFeePayerAccountId"]);
            dq.SelectParameters.Add(sp4);

            dq.Criterias.Add(ObjectMap["OriginalTransactionId"], OriginalTransactionId);
            dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);
            dq.Criterias.Add(ObjectMap["PaymentSourceTypeId"], (int)paymentSourceType);

            dq.GroupBys.Add(ObjectMap["ConvenienceFeePayerAccountId"]);
            dq.GroupBys.Add(ObjectMap["PaymentTypeFeePayerAccountId"]);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
            {
                var convenienceFee = results.Rows[0][0].ToString().ToDecimal();
                var transactionFee = results.Rows[0][1].ToString().ToDecimal();

                if (feePayerId.HasValue)
                {
                    var convenienceFeePayerId = results.Rows[0][2].ToString().ToInteger();
                    var transactionFeePayerId = results.Rows[0][3].ToString().ToInteger();
                    var fees = 0M;

                    if (feePayerId == convenienceFeePayerId)
                        fees += convenienceFee;

                    if (feePayerId == transactionFeePayerId)
                        fees += transactionFee;

                    return fees;
                }

                return convenienceFee + transactionFee;
            }   

            return 0;
        }

        public static decimal GetTotalAmountCreditedToAccount(int accountId, string search, DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["Amount"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["BillerAccountId"], accountId);
            dq.Criterias.Add(ObjectMap["TransactionTypeId"], (int)TransactionType.Credit);
            dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);

            if (search.IsNotBlank())
                AddSearchQueryJoins(dq, search);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalAmountDebitedFromAccount(int accountId, string search, DateTime? startDate, DateTime? endDate, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["Amount"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["PayerAccountId"], accountId);
            dq.Criterias.Add(ObjectMap["TransactionTypeId"], (int)TransactionType.Debit);
            dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);

            if (search.IsNotBlank())
                AddSearchQueryJoins(dq, search);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static List<Transaction> LoadByOriginalTransactionIdByIsDeleted(int OriginalTransactionId, PaymentSourceType paymentSourceType, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["OriginalTransactionId"], OriginalTransactionId);
            dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
            dq.Criterias.Add(ObjectMap["PaymentSourceTypeId"], (int)paymentSourceType);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Transaction> GetUnmarkedConnectedAccountTransactions(SystemMessages msgs, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;

            var sql = "select * from [Transaction] where ConvenienceFeePayerAccountId = BillerAccountId and PlatformFlatFee is null and PayoutId is null";

            return dq.ExecuteSelect(sql, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        /// <summary>
        /// This method is created to get amount of payout for providers
        /// In future, we will be using stripe webhooks to get connected accounts payouts
        /// But till then we will be using this work around
        /// </summary>
        /// <param name="BillerAccountId"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static decimal GetPayoutAmountForBillerAccount(int BillerAccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;

            var sql = $@"   
                            SELECT 
                               ISNULL(SUM(Amount - ConvenienceFeeCharged - PaymentTypeFeeCharged), 0) AS Amount 
                            FROM [Transaction] 
                            WHERE 
                                BillerAccountId = {BillerAccountId} AND 
                                PayoutId IS NULL AND 
                                IsDeleted = 0 AND 
                                (
                                    (PaymentTypeId = 1 and GETDATE() >= DATEADD(day, 2, CreateDate)) OR  -- cc 2 days
                                    (PaymentTypeId = 5 and GETDATE() >= DATEADD(day, 5, CreateDate))     -- ach 5 days
								)";

            var dt = dq.ExecuteSelect(sql, msgs);

            if (dt.Rows.Any())
                return dt.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static List<Transaction> LoadByIdsByBillerAccountId(List<int> ids, int accountId, SystemMessages msgs, DatabaseConnection dc)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["BillerAccountId"], accountId);
            dq.Criterias.Add(ObjectMap["ID"], ids, DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Transaction> LoadByIdsByPayerAccountId(List<int> ids, int accountId, SystemMessages msgs, DatabaseConnection dc)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["PayerAccountId"], accountId);
            dq.Criterias.Add(ObjectMap["ID"], ids, DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        /// <summary>
        /// This method is created to get all the transactions for providers
        /// it will be used to create manual payouts
        /// </summary>
        /// <param name="BillerAccountId"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns>List<Transaction></returns>
        public static List<Transaction> LoadTransactionReadyToPayoutForBillerAccount(int BillerAccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;

            var sql = $@"
                            SELECT *
                            FROM [Transaction] 
                            WHERE 
                                BillerAccountId = {BillerAccountId} AND 
                                PayoutId IS NULL AND 
                                IsDeleted = 0 AND 
                                (
                                    (PaymentTypeId = 1 and GETDATE() >= DATEADD(day, 2, CreateDate)) OR  -- cc 2 days
                                    (PaymentTypeId = 5 and GETDATE() >= DATEADD(day, 5, CreateDate))     -- ach 5 days
								)";

            return dq.ExecuteSelect(sql, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        /// <summary>
        /// This method is created to get transaction for activity payment
        /// it will be used to create manual payouts
        /// </summary>
        /// <param name="TransactionId"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns>Transaction</returns>
        public static Transaction LoadTransactionReadyPayout(int TransactionId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;

            var sql = $@"
                            SELECT TOP 1 *
                            FROM [Transaction] 
                            WHERE 
                                Id = {TransactionId} AND 
                                PayoutId IS NULL AND 
                                IsDeleted = 0 AND 
                                (
                                    (PaymentTypeId = 1 and GETDATE() >= DATEADD(day, 2, CreateDate)) OR  -- cc 2 days
                                    (PaymentTypeId = 5 and GETDATE() >= DATEADD(day, 5, CreateDate))     -- ach 5 days
								)";

            var dt = dq.ExecuteSelect(sql, msgs);

            if (dt.Rows.Any())
                return LoadObject(dt.Rows[0]);

            return null;
        }

        #endregion
    }
}

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
		public decimal? PlatformPercentFee { get { return (decimal?)MDV["PlatformPercentFee"].Value; } set { MDV["PlatformPercentFee"].Value = value; } }
		public decimal? PlatformFlatFee { get { return (decimal?)MDV["PlatformFlatFee"].Value; } set { MDV["PlatformFlatFee"].Value = value; } }
		

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
				_dp.Add(className, "PaymentTypeFeeCharged", System.Data.SqlDbType.Decimal , "10,2");
				_dp.Add(className, "ConvenienceFeeCharged", System.Data.SqlDbType.Decimal , "10,2");
				_dp.Add(className, "PaymentTypeFeePayerAccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ConvenienceFeePayerAccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "TransactionTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "StripeChargeOrRefundId", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "PaymentSourceTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "OriginalTransactionId", System.Data.SqlDbType.Int);
				_dp.Add(className, "PayoutId", System.Data.SqlDbType.Int);
				_dp.Add(className, "PlatformPercentFee", System.Data.SqlDbType.Decimal , "10,2");
				_dp.Add(className, "PlatformFlatFee", System.Data.SqlDbType.Decimal , "10,2");
				
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
			dps.Add(ObjectMap["PlatformPercentFee"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PlatformFlatFee"]);
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

		public static List<Transaction> LoadByOriginalTransactionId(int OriginalTransactionId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["OriginalTransactionId"], OriginalTransactionId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Transaction> LoadByPayoutId(int PayoutId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["PayoutId"], PayoutId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        public static List<Transaction> LoadList(List<int> ids, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, DatabaseCriteriaOperatorEnum.In);           
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Transaction> LoadByBillerAccountId(int BillerAccountId, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return LoadByBillerOrPayerAccountId(BillerAccountId, null, startDate, endDate, participationObjects, start, length, msgs, dc);
        }

        public static List<Transaction> LoadByPayerAccountId(int PayerAccountId, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return LoadByBillerOrPayerAccountId(null, PayerAccountId, startDate, endDate, participationObjects, start, length, msgs, dc);
        }

        private static List<Transaction> LoadByBillerOrPayerAccountId(int? BillerAccountId, int? PayerAccountId, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;

            if (BillerAccountId.HasValue)
                dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId.Value);

            if (PayerAccountId.HasValue)
                dq.Criterias.Add(ObjectMap["PayerAccountId"], PayerAccountId.Value);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            if (participationObjects.IsNotEmpty() && participationObjects.Count > 0)
                dq.Criterias.Add(
                    GetParticipationObjectQuery(participationObjects),
                    0,
                    DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanParameter);

            dq.OrderBys.Add(ObjectMap["Description"], OrderByDirectionEnum.Ascending);
            dq.OrderBys.Add(ObjectMap["CreateDate"], OrderByDirectionEnum.Descending);

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

        public static int GetCountByBillerAccountId(int BillerAccountId, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetCountByBillerOrPayerAccountId(BillerAccountId, null, startDate, endDate, participationObjects, msgs, dc);
        }

        public static int GetCountByPayerAccountId(int PayerAccountId, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return GetCountByBillerOrPayerAccountId(null, PayerAccountId, startDate, endDate, participationObjects, msgs, dc);
        }

        private static int GetCountByBillerOrPayerAccountId(int? BillerAccountId, int? PayerAccountId, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> participationObjects, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;

            if (BillerAccountId.HasValue)
                dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId.Value);

            if (PayerAccountId.HasValue)
                dq.Criterias.Add(ObjectMap["PayerAccountId"], PayerAccountId.Value);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);
            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            if (participationObjects.IsNotEmpty() && participationObjects.Count > 0)
                dq.Criterias.Add(
                    GetParticipationObjectQuery(participationObjects),
                    0,
                    DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanParameter);

            return dq.GetCount();
        }

        public static List<(int ID, ParticipationObjectType ParticipationObjectTypeId, string Name)> GetParticipationObjectsByBillerAccountId(int BillerAccountId, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return GetParticipationObjectsByBillerOrPayerAccountId(BillerAccountId, null, startDate, endDate, msgs, dc);
        }

        public static List<(int ID, ParticipationObjectType ParticipationObjectTypeId, string Name)> GetParticipationObjectsByPayerAccountId(int PayerAccountId, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return GetParticipationObjectsByBillerOrPayerAccountId(null, PayerAccountId, startDate, endDate, msgs, dc);
        }

        private static List<(int ID, ParticipationObjectType ParticipationObjectTypeId, string Name)> GetParticipationObjectsByBillerOrPayerAccountId(int? billerAccountId, int? payerAccountId, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;

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

        public static int GetCountByPurchaseId(int PurchaseId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["PurchaseId"], PurchaseId);

            return dq.GetCount(msgs);
        }

        public static decimal GetTotalFeesPaidByBillerAccount(int BillerAccountId, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["PlatformFlatFee"].ParameterFullName} + ({ObjectMap["PlatformPercentFee"].ParameterFullName} * {ObjectMap["Amount"].ParameterFullName})), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal() + 
                    GetTotalConvenienceFeesPaidByBillerAccount(BillerAccountId, startDate, endDate, msgs, dc) +
                    GetTotalPaymentTypeFeesPaidByBillerAccount(BillerAccountId, startDate, endDate, msgs, dc);

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
        public static decimal GetTotalAmountBilledByBillerAccount(int BillerAccountId, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["Amount"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal() -
                    GetTotalConvenienceFeesNotPaidByBillerAccount(BillerAccountId, startDate, endDate, msgs, dc) -
                    GetTotalPaymentTypeFeesNotPaidByBillerAccount(BillerAccountId, startDate, endDate, msgs, dc);

            return 0;
        }

        public static decimal GetTotalConvenienceFeesNotPaidByBillerAccount(int BillerAccountId, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
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

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalPaymentTypeFeesNotPaidByBillerAccount(int BillerAccountId, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["PaymentTypeFeeCharged"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["BillerAccountId"], BillerAccountId);
            dq.Criterias.Add(ObjectMap["PaymentTypeFeePayerAccountId"], BillerAccountId, DatabaseCriteriaOperatorEnum.NotEqualTo);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalConvenienceFeesPaidByBillerAccount(int BillerAccountId, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["ConvenienceFeeCharged"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["ConvenienceFeePayerAccountId"], BillerAccountId);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalPaymentTypeFeesPaidByBillerAccount(int BillerAccountId, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["PaymentTypeFeeCharged"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["PaymentTypeFeePayerAccountId"], BillerAccountId);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalAmountPaidByPayerAccount(int PayerAccountId, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, DatabaseConnection dc = null)
        {

            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["Amount"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["PayerAccountId"], PayerAccountId);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalRefundByOriginalTransaction(int OriginalTransactionId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {

            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["Amount"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["OriginalTransactionId"], OriginalTransactionId);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        #endregion
    }
}

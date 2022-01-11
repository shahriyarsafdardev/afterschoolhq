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
    /// Used to transfer money from the AHQ Platform to the provider
    /// </summary>
	
    public class Payout : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public decimal? Amount { get { return (decimal?)MDV["Amount"].Value; } set { MDV["Amount"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public PayoutStatus PayoutStatusId { get { return (PayoutStatus)MDV["PayoutStatusId"].Value; } set { MDV["PayoutStatusId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public PaymentType PaymentTypeId { get { return (PaymentType)MDV["PaymentTypeId"].Value; } set { MDV["PaymentTypeId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public DateTime? DateRequested { get { return (DateTime?)MDV["DateRequested"].Value; } set { MDV["DateRequested"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? RequestedBy { get { return (int?)MDV["RequestedBy"].Value; } set { MDV["RequestedBy"].Value = value; } }
		/// <summary>
		/// a place to store the check id or stripe payout id
		/// </summary>
		public string ExternalPayoutId { get { return (string)MDV["ExternalPayoutId"].Value; } set { MDV["ExternalPayoutId"].Value = value; } }
		/// <summary>
		/// a place to store statements that will be used by stripe payout
		/// </summary>
		public string StatementDescriptor { get { return (string)MDV["StatementDescriptor"].Value; } set { MDV["StatementDescriptor"].Value = value; } }
		

        public Payout()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Payout).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.Uid = Guid.NewGuid();
			this.PayoutStatusId = PayoutStatus.NotSet;
			this.PaymentTypeId = PaymentType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(Payout).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "Amount", System.Data.SqlDbType.Money);
				_dp.Add(className, "PayoutStatusId", System.Data.SqlDbType.Int);
				_dp.Add(className, "PaymentTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "DateRequested", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "RequestedBy", System.Data.SqlDbType.Int);
				_dp.Add(className, "ExternalPayoutId", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "StatementDescriptor", System.Data.SqlDbType.NVarChar , "22");
				
                return _dp;
            }
        }

        public static Payout LoadObject(System.Data.DataRow row)
        {
            Payout obj = new Payout();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(Payout).Name);
				
                return dq;
            }
        }

        public static Payout Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Payout> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return Payout.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return Payout.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.AccountId.IsEmpty())
				msgs.Add("AccountId is required on Payout object", MessageType.Error);
			if (this.Amount.IsEmpty())
				msgs.Add("Amount is required on Payout object", MessageType.Error);
			if (this.PayoutStatusId.IsEmpty())
				msgs.Add("PayoutStatusId is required on Payout object", MessageType.Error);
			if (this.PaymentTypeId.IsEmpty())
				msgs.Add("PaymentTypeId is required on Payout object", MessageType.Error);
			if (this.DateRequested.IsEmpty())
				msgs.Add("DateRequested is required on Payout object", MessageType.Error);
			if (this.RequestedBy.IsEmpty())
				msgs.Add("RequestedBy is required on Payout object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Payout).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Payout).Name);
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
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Amount"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PayoutStatusId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PaymentTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DateRequested"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RequestedBy"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ExternalPayoutId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StatementDescriptor"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Payout Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Payout.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<Payout> LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Payout> LoadByAccountIdByPayoutStatusId(int AccountId, PayoutStatus PayoutStatusId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["PayoutStatusId"], PayoutStatusId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Payout> LoadByPayoutStatusId(PayoutStatus PayoutStatusId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["PayoutStatusId"], PayoutStatusId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Payout LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static Payout LoadByUidByAccountId(Guid Uid, int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public static List<Payout> Load(PayoutStatus? payoutStatusId, DateTime? startDate, DateTime? endDate, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;

            if (payoutStatusId.HasValue)
                dq.Criterias.Add(ObjectMap["PayoutStatusId"], (int)payoutStatusId.Value);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            dq.OrderBys.Add(ObjectMap["CreateDate"], OrderByDirectionEnum.Descending);

            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;

            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(PayoutStatus? payoutStatusId, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;

            if (payoutStatusId.HasValue)
                dq.Criterias.Add(ObjectMap["PayoutStatusId"], (int)payoutStatusId.Value);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            return dq.GetCount(msgs);
        }

        public static List<Payout> LoadPayouts(int accountId, PayoutStatus? payoutStatusId, DateTime? startDate, DateTime? endDate, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            if (payoutStatusId.HasValue)
                dq.Criterias.Add(ObjectMap["PayoutStatusId"], (int)payoutStatusId.Value);

            dq.OrderBys.Add(ObjectMap["CreateDate"], OrderByDirectionEnum.Descending);

            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCountByAccountId(int accountId, PayoutStatus? payoutStatusId, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {

            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            if (payoutStatusId.HasValue)
                dq.Criterias.Add(ObjectMap["PayoutStatusId"], (int)payoutStatusId.Value);

            return dq.GetCount(msgs);
        }

        public static Payout GetLastPayout(int accountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], accountId);

            dq.OrderBys.Add(ObjectMap["CreateDate"], OrderByDirectionEnum.Descending);

            dq.SelectRange.Start = 0;
            dq.SelectRange.Length = 1;

            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static decimal GetTotalAmountByAccountId(int accountId, string search, PayoutStatus? payoutStatusId, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["Amount"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            dq.Criterias.Add(ObjectMap["AccountId"], accountId);

            if (payoutStatusId.HasValue)
                dq.Criterias.Add(ObjectMap["PayoutStatusId"], (int)payoutStatusId.Value);

            if (search.IsNotBlank())
                AddSearchQuery(dq, search);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        private static void AddSearchQuery(DatabaseQuery dq, string search)
        {
            dq.Joins.Add(Transaction.ObjectMap["PayoutId"], ObjectMap["ID"], DatabaseJoinTypeEnum.InnerJoin);

            Transaction.AddSearchQueryJoins(dq, search);
        }

        public static decimal GetTotalAmount(PayoutStatus? payoutStatusId, DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();

            var sp = new SelectParameter(ObjectMap["Amount"]);
            sp.Function = $"ISNULL(SUM({ObjectMap["Amount"].ParameterFullName}), 0)";
            dq.SelectParameters.Add(sp);

            if (payoutStatusId.HasValue)
                dq.Criterias.Add(ObjectMap["PayoutStatusId"], (int)payoutStatusId.Value);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        #endregion
    }
}

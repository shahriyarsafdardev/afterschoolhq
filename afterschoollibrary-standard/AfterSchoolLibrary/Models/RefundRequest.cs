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
    /// 
    /// </summary>
	
    public class RefundRequest : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public decimal? Amount { get { return (decimal?)MDV["Amount"].Value; } set { MDV["Amount"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? TransactionId { get { return (int?)MDV["TransactionId"].Value; } set { MDV["TransactionId"].Value = value; } }
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public bool RequestFulfilled { get { return (bool)MDV["RequestFulfilled"].Value; } set { MDV["RequestFulfilled"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public RefundRequestStatus RefundRequestStatusId { get { return (RefundRequestStatus)MDV["RefundRequestStatusId"].Value; } set { MDV["RefundRequestStatusId"].Value = value; } }
		public string RequestReason { get { return (string)MDV["RequestReason"].Value; } set { MDV["RequestReason"].Value = value; } }
		public string DeniedReason { get { return (string)MDV["DeniedReason"].Value; } set { MDV["DeniedReason"].Value = value; } }
		public DateTime? ApprovedDate { get { return (DateTime?)MDV["ApprovedDate"].Value; } set { MDV["ApprovedDate"].Value = value; } }
		public DateTime? DeniedDate { get { return (DateTime?)MDV["DeniedDate"].Value; } set { MDV["DeniedDate"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public RefundType RequestedRefundTypeId { get { return (RefundType)MDV["RequestedRefundTypeId"].Value; } set { MDV["RequestedRefundTypeId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public RefundType ApprovedRefundTypeId { get { return (RefundType)MDV["ApprovedRefundTypeId"].Value; } set { MDV["ApprovedRefundTypeId"].Value = value; } }
		

        public RefundRequest()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(RefundRequest).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.Uid = Guid.NewGuid();
			this.RequestFulfilled = false;
			this.RefundRequestStatusId = RefundRequestStatus.NotSet;
			this.RequestedRefundTypeId = RefundType.NotSet;
			this.ApprovedRefundTypeId = RefundType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(RefundRequest).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "Amount", System.Data.SqlDbType.Money);
				_dp.Add(className, "TransactionId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "RequestFulfilled", System.Data.SqlDbType.Bit);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RefundRequestStatusId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RequestReason", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "DeniedReason", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "ApprovedDate", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "DeniedDate", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "RequestedRefundTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ApprovedRefundTypeId", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        public static RefundRequest LoadObject(System.Data.DataRow row)
        {
            RefundRequest obj = new RefundRequest();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(RefundRequest).Name);
				
                return dq;
            }
        }

        public static RefundRequest Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<RefundRequest> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return RefundRequest.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return RefundRequest.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.Amount.IsEmpty())
				msgs.Add("Amount is required on RefundRequest object", MessageType.Error);
			if (this.TransactionId.IsEmpty())
				msgs.Add("TransactionId is required on RefundRequest object", MessageType.Error);
			if (this.AccountId.IsEmpty())
				msgs.Add("AccountId is required on RefundRequest object", MessageType.Error);
			if (this.RefundRequestStatusId.IsEmpty())
				msgs.Add("RefundRequestStatusId is required on RefundRequest object", MessageType.Error);
			if (this.RequestedRefundTypeId.IsEmpty())
				msgs.Add("RequestedRefundTypeId is required on RefundRequest object", MessageType.Error);
			if (this.ApprovedRefundTypeId.IsEmpty())
				msgs.Add("ApprovedRefundTypeId is required on RefundRequest object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(RefundRequest).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(RefundRequest).Name);
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
			dps.Add(ObjectMap["TransactionId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RequestFulfilled"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RefundRequestStatusId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RequestReason"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DeniedReason"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ApprovedDate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DeniedDate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RequestedRefundTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ApprovedRefundTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public RefundRequest Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return RefundRequest.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static RefundRequest LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static RefundRequest LoadByUidByAccountId(Guid Uid, int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<RefundRequest> LoadByTransactionId(int TransactionId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["TransactionId"], TransactionId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<RefundRequest> LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<RefundRequest> LoadByAccountIdByRefundRequestStatusId(int AccountId, RefundRequestStatus RefundRequestStatusId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["RefundRequestStatusId"], RefundRequestStatusId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<RefundRequest> LoadByRefundRequestStatusId(RefundRequestStatus RefundRequestStatusId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RefundRequestStatusId"], RefundRequestStatusId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

		public bool IsCompleted 
		{
			get
			{
				return this.RefundRequestStatusId == RefundRequestStatus.Completed;
			}
		}

		public bool IsApprovedByProvider
		{
			get
			{
				return this.RefundRequestStatusId == RefundRequestStatus.ApprovedByProvider;
			}
		}

		public bool IsApprovedByAfterSchoolHQ
		{
			get
			{
				return this.RefundRequestStatusId == RefundRequestStatus.ApprovedByAfterSchoolHQ;
			}
		}

		public bool IsApproved
		{
			get
			{
				return IsApprovedByProvider && IsApprovedByAfterSchoolHQ;
			}
		}

		public bool IsDenied
		{
			get
			{
				return this.RefundRequestStatusId.In(new RefundRequestStatus[] { RefundRequestStatus.DeniedByAfterSchoolHQ, RefundRequestStatus.DeniedByProvider });
			}
		}

		public bool IsReadyForAHQApproval
		{
			get
			{
				return this.RefundRequestStatusId.In(new RefundRequestStatus[] { RefundRequestStatus.ApprovedByProvider, RefundRequestStatus.RequestedByProvider });
			}
		}

		public static List<RefundRequestStatus> InProcessStatuses
		{
			get
			{
				return Enum.GetValues(typeof(RefundRequestStatus)).Cast<RefundRequestStatus>().Where(x => x.NotIn(ClosedStatuses)).ToList();
			}
		}

		public static List<RefundRequestStatus> ClosedStatuses
		{
			get
			{
				return (new RefundRequestStatus[] { RefundRequestStatus.NotSet, RefundRequestStatus.DeniedByAfterSchoolHQ, RefundRequestStatus.DeniedByProvider, RefundRequestStatus.Completed }).ToList();
			}
		}

		public static int GetCountByTransactionId(int TransactionId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["TransactionId"], TransactionId);
			return dq.GetCount(msgs);
		}

		public static int GetCountByTransactionId(int TransactionId, List<RefundRequestStatus> refundRequestStatuses, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["TransactionId"], TransactionId);
			dq.Criterias.Add(ObjectMap["RefundRequestStatusId"], refundRequestStatuses.Select(x=>(int)x).ToList(), DatabaseCriteriaOperatorEnum.In);
			return dq.GetCount(msgs);
		}

		public static int GetCount(DateTime? startDate, DateTime? endDate, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;

			if (startDate.HasValue)
				dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);
			if (endDate.HasValue)
				dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

			return dq.GetCount(msgs);
		}

		public static List<RefundRequest> LoadByDate(DateTime? startDate, DateTime? endDate, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.UseParameterizedQuery = false;

			if (startDate.HasValue)
				dq.Criterias.Add(ObjectMap["CreateDate"], startDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

			if (endDate.HasValue)
				dq.Criterias.Add(ObjectMap["CreateDate"], endDate.Value, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

			dq.OrderBys.Add(ObjectMap["CreateDate"], OrderByDirectionEnum.Descending);

			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		#endregion
    }
}

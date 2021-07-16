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
    /// keeps track of payments for an activity from a registrant/student - can have negative numbers for refunds
    /// </summary>
	
    public class ActivityPayment : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? RegistrantId { get { return (int?)MDV["RegistrantId"].Value; } set { MDV["RegistrantId"].Value = value; } }
		public int? ActivityId { get { return (int?)MDV["ActivityId"].Value; } set { MDV["ActivityId"].Value = value; } }
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		/// <summary>
		/// used to define AmountReceivedByProvider
		/// </summary>
		public decimal? Amount { get { return (decimal?)MDV["Amount"].Value; } set { MDV["Amount"].Value = value; } }
		public decimal? AmountPaidByParent { get { return (decimal?)MDV["AmountPaidByParent"].Value; } set { MDV["AmountPaidByParent"].Value = value; } }
		public bool IsProRated { get { return (bool)MDV["IsProRated"].Value; } set { MDV["IsProRated"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public ParticipationObjectType ParticipationObjectTypeId { get { return (ParticipationObjectType)MDV["ParticipationObjectTypeId"].Value; } set { MDV["ParticipationObjectTypeId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? ParticipationObjectId { get { return (int?)MDV["ParticipationObjectId"].Value; } set { MDV["ParticipationObjectId"].Value = value; } }
		public int? TransactionId { get { return (int?)MDV["TransactionId"].Value; } set { MDV["TransactionId"].Value = value; } }
		public int? RegistrantSubscriptionPlanId { get { return (int?)MDV["RegistrantSubscriptionPlanId"].Value; } set { MDV["RegistrantSubscriptionPlanId"].Value = value; } }
		public bool IsDeleted { get { return (bool)MDV["IsDeleted"].Value; } set { MDV["IsDeleted"].Value = value; } }
		

        public ActivityPayment()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(ActivityPayment).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.Uid = Guid.NewGuid();
			this.IsProRated = false;
			this.ParticipationObjectTypeId = ParticipationObjectType.NotSet;
			this.IsDeleted = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(ActivityPayment).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "RegistrantId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "Amount", System.Data.SqlDbType.Money);
				_dp.Add(className, "AmountPaidByParent", System.Data.SqlDbType.Money);
				_dp.Add(className, "IsProRated", System.Data.SqlDbType.Bit);
				_dp.Add(className, "ParticipationObjectTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ParticipationObjectId", System.Data.SqlDbType.Int);
				_dp.Add(className, "TransactionId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrantSubscriptionPlanId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsDeleted", System.Data.SqlDbType.Bit);
				
                return _dp;
            }
        }

        public static ActivityPayment LoadObject(System.Data.DataRow row)
        {
            ActivityPayment obj = new ActivityPayment();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivityPayment).Name);
				
                return dq;
            }
        }

        public static ActivityPayment Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ActivityPayment> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return ActivityPayment.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return ActivityPayment.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.RegistrantId.IsEmpty())
				msgs.Add("RegistrantId is required on ActivityPayment object", MessageType.Error);
			if (this.Uid.IsEmpty())
				msgs.Add("Uid is required on ActivityPayment object", MessageType.Error);
			if (this.ParticipationObjectTypeId.IsEmpty())
				msgs.Add("ParticipationObjectTypeId is required on ActivityPayment object", MessageType.Error);
			if (this.ParticipationObjectId.IsEmpty())
				msgs.Add("ParticipationObjectId is required on ActivityPayment object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ActivityPayment).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ActivityPayment).Name);
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
			dps.Add(ObjectMap["RegistrantId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

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
			dps.Add(ObjectMap["AmountPaidByParent"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsProRated"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ParticipationObjectTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ParticipationObjectId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["TransactionId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrantSubscriptionPlanId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsDeleted"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public ActivityPayment Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return ActivityPayment.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<ActivityPayment> LoadByActivityIdByIsDeleted(int ActivityId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityPayment> LoadByAccountIdByIsDeleted(int AccountId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityPayment> LoadByRegistrantIdByIsDeleted(int RegistrantId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityPayment> LoadByRegistrantSubscriptionPlanIdByIsDeleted(int RegistrantSubscriptionPlanId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantSubscriptionPlanId"], RegistrantSubscriptionPlanId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static ActivityPayment LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<ActivityPayment> LoadByParticipationObject(int ParticipationObjectId, ParticipationObjectType ParticipationObjectTypeId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ParticipationObjectId"], ParticipationObjectId);
			dq.Criterias.Add(ObjectMap["ParticipationObjectTypeId"], ParticipationObjectTypeId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityPayment> LoadByParticipationObjectByRegistrant(int ParticipationObjectId, ParticipationObjectType ParticipationObjectTypeId, int RegistrantId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ParticipationObjectId"], ParticipationObjectId);
			dq.Criterias.Add(ObjectMap["ParticipationObjectTypeId"], ParticipationObjectTypeId);
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityPayment> LoadByTransactionIdByIsDeleted(int TransactionId, bool IsDeleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["TransactionId"], TransactionId);
			dq.Criterias.Add(ObjectMap["IsDeleted"], IsDeleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        public static decimal GetTotalPaidByAccount(int accountId, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();
            var sp = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["Amount"]);
            sp.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp);
            dq.Criterias.Add(ObjectMap["accountId"], accountId);
			dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);

			var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalPaidByParticipationObject(int objectId, ParticipationObjectType objectType, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();
            var sp = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["Amount"]);
            sp.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp);
            dq.Criterias.Add(ObjectMap["participationObjectTypeId"], (int)objectType);
            dq.Criterias.Add(ObjectMap["participationObjectId"], objectId);
			dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);

			var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

        public static decimal GetTotalPaidByParticipationObjectByRegistrant(int objectId, ParticipationObjectType objectType, int registrantId, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectParameters.Clear();
            var sp = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["Amount"]);
            sp.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
            dq.SelectParameters.Add(sp);
            dq.Criterias.Add(ObjectMap["participationObjectTypeId"], (int)objectType);
            dq.Criterias.Add(ObjectMap["participationObjectId"], objectId);
			dq.Criterias.Add(ObjectMap["registrantId"], registrantId);
			dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);

			var results = dq.Select(msgs);
            if (results.Rows.Any())
                return results.Rows[0][0].ToString().ToDecimal();

            return 0;
        }

		public static decimal GetAmountPaidByParentByParticipationObjectByRegistrant(int objectId, ParticipationObjectType objectType, int registrantId, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectParameters.Clear();
			var sp = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["AmountPaidByParent"]);
			sp.Function = "ISNULL(SUM(" + BasicLibrary.DataAccess.DatabaseQuery.FunctionReplaceValue + "), 0)";
			dq.SelectParameters.Add(sp);
			dq.Criterias.Add(ObjectMap["participationObjectTypeId"], (int)objectType);
			dq.Criterias.Add(ObjectMap["participationObjectId"], objectId);
			dq.Criterias.Add(ObjectMap["registrantId"], registrantId);
			dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);

			var results = dq.Select(msgs);
			if (results.Rows.Any())
				return results.Rows[0][0].ToString().ToDecimal();

			return 0;
		}

		public static bool GetIsProRatedFlagByParentByParticipationObjectByRegistrant(int objectId, ParticipationObjectType objectType, int registrantId, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectParameters.Clear();
			var sp = new BasicLibrary.DataAccess.SelectParameter(ObjectMap["IsProRated"]);			
			dq.SelectParameters.Add(sp);
			dq.Criterias.Add(ObjectMap["participationObjectTypeId"], (int)objectType);
			dq.Criterias.Add(ObjectMap["participationObjectId"], objectId);
			dq.Criterias.Add(ObjectMap["registrantId"], registrantId);
			dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted);

			var results = dq.Select(msgs);
			if (results.Rows.Any())
				return results.Rows[0][0].ToString().ToBoolean();

			return false;
		}

		public Transaction GetTransaction(SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			return Transaction.Load(this.TransactionId.Value, msgs, dc);
		}

        #endregion
    }
}

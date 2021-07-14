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
    /// Used to handle prepayments then credit them later to an attendance record
    /// </summary>
	
    public class SubscriptionPlanPrePayment : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? ParticipationObjectId { get { return (int?)MDV["ParticipationObjectId"].Value; } set { MDV["ParticipationObjectId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public ParticipationObjectType ParticipationObjectTypeId { get { return (ParticipationObjectType)MDV["ParticipationObjectTypeId"].Value; } set { MDV["ParticipationObjectTypeId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? TransactionId { get { return (int?)MDV["TransactionId"].Value; } set { MDV["TransactionId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? RegistrationId { get { return (int?)MDV["RegistrationId"].Value; } set { MDV["RegistrationId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? PayerAccountId { get { return (int?)MDV["PayerAccountId"].Value; } set { MDV["PayerAccountId"].Value = value; } }
		public int? AttendanceRegistrantId { get { return (int?)MDV["AttendanceRegistrantId"].Value; } set { MDV["AttendanceRegistrantId"].Value = value; } }
		public decimal? Value { get { return (decimal?)MDV["Value"].Value; } set { MDV["Value"].Value = value; } }
		

        public SubscriptionPlanPrePayment()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(SubscriptionPlanPrePayment).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.ParticipationObjectTypeId = ParticipationObjectType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(SubscriptionPlanPrePayment).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "ParticipationObjectId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ParticipationObjectTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "TransactionId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrationId", System.Data.SqlDbType.Int);
				_dp.Add(className, "PayerAccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AttendanceRegistrantId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Value", System.Data.SqlDbType.Money);
				
                return _dp;
            }
        }

        public static SubscriptionPlanPrePayment LoadObject(System.Data.DataRow row)
        {
            SubscriptionPlanPrePayment obj = new SubscriptionPlanPrePayment();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(SubscriptionPlanPrePayment).Name);
				
                return dq;
            }
        }

        public static SubscriptionPlanPrePayment Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<SubscriptionPlanPrePayment> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return SubscriptionPlanPrePayment.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return SubscriptionPlanPrePayment.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.ParticipationObjectId.IsEmpty())
				msgs.Add("ParticipationObjectId is required on SubscriptionPlanPrePayment object", MessageType.Error);
			if (this.ParticipationObjectTypeId.IsEmpty())
				msgs.Add("ParticipationObjectTypeId is required on SubscriptionPlanPrePayment object", MessageType.Error);
			if (this.TransactionId.IsEmpty())
				msgs.Add("TransactionId is required on SubscriptionPlanPrePayment object", MessageType.Error);
			if (this.RegistrationId.IsEmpty())
				msgs.Add("RegistrationId is required on SubscriptionPlanPrePayment object", MessageType.Error);
			if (this.PayerAccountId.IsEmpty())
				msgs.Add("PayerAccountId is required on SubscriptionPlanPrePayment object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(SubscriptionPlanPrePayment).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(SubscriptionPlanPrePayment).Name);
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
			dps.Add(ObjectMap["ParticipationObjectId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ParticipationObjectTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["TransactionId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrationId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PayerAccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AttendanceRegistrantId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Value"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public SubscriptionPlanPrePayment Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return SubscriptionPlanPrePayment.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<SubscriptionPlanPrePayment> LoadByParticipationObjectIdByParticipationObjectTypeId(int ParticipationObjectId, ParticipationObjectType ParticipationObjectTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ParticipationObjectId"], ParticipationObjectId);
			dq.Criterias.Add(ObjectMap["ParticipationObjectTypeId"], ParticipationObjectTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SubscriptionPlanPrePayment> LoadByRegistrationIdByParticipationObjectIdByParticipationObjectTypeId(int RegistrationId, int ParticipationObjectId, ParticipationObjectType ParticipationObjectTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrationId"], RegistrationId);
			dq.Criterias.Add(ObjectMap["ParticipationObjectId"], ParticipationObjectId);
			dq.Criterias.Add(ObjectMap["ParticipationObjectTypeId"], ParticipationObjectTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SubscriptionPlanPrePayment> LoadByTransactionId(int TransactionId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["TransactionId"], TransactionId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SubscriptionPlanPrePayment> LoadByRegistrationId(int RegistrationId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrationId"], RegistrationId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SubscriptionPlanPrePayment> LoadByPayerAccountId(int PayerAccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["PayerAccountId"], PayerAccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static SubscriptionPlanPrePayment LoadByAttendanceRegistrantId(int AttendanceRegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AttendanceRegistrantId"], AttendanceRegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

		public static List<SubscriptionPlanPrePayment> LoadUnused(int RegistrationId, int ParticipationObjectId, ParticipationObjectType ParticipationObjectTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.Criterias.Add(ObjectMap["RegistrationId"], RegistrationId);
			dq.Criterias.Add(ObjectMap["ParticipationObjectId"], ParticipationObjectId);
			dq.Criterias.Add(ObjectMap["ParticipationObjectTypeId"], (int)ParticipationObjectTypeId);
			dq.Criterias.Add(ObjectMap["AttendanceRegistrantId"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static int GetCountByAttendanceRegistrantId(int AttendanceRegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AttendanceRegistrantId"], AttendanceRegistrantId);
			return dq.GetCount(msgs);
		}

		#endregion
    }
}

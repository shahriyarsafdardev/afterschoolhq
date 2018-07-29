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
    /// Keeps track of which registrants are on which payment plans
    /// </summary>
	[Serializable]
    public class RegistrantPaymentPlan : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? PurchaseId { get { return (int?)MDV["PurchaseId"].Value; } set { MDV["PurchaseId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? RegistrantId { get { return (int?)MDV["RegistrantId"].Value; } set { MDV["RegistrantId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? PaymentPlanId { get { return (int?)MDV["PaymentPlanId"].Value; } set { MDV["PaymentPlanId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public DateTime? StartDate { get { return (DateTime?)MDV["StartDate"].Value; } set { MDV["StartDate"].Value = value; } }
		

        public RegistrantPaymentPlan()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(RegistrantPaymentPlan).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(RegistrantPaymentPlan).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "PurchaseId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrantId", System.Data.SqlDbType.Int);
				_dp.Add(className, "PaymentPlanId", System.Data.SqlDbType.Int);
				_dp.Add(className, "StartDate", System.Data.SqlDbType.DateTime);
				
                return _dp;
            }
        }

        protected static RegistrantPaymentPlan LoadObject(System.Data.DataRow row)
        {
            RegistrantPaymentPlan obj = new RegistrantPaymentPlan();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(RegistrantPaymentPlan).Name);
				
                return dq;
            }
        }

        public static RegistrantPaymentPlan Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<RegistrantPaymentPlan> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return RegistrantPaymentPlan.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return RegistrantPaymentPlan.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.PurchaseId.IsEmpty())
				msgs.Add("PurchaseId is required on RegistrantPaymentPlan object", MessageType.Error);
			if (this.RegistrantId.IsEmpty())
				msgs.Add("RegistrantId is required on RegistrantPaymentPlan object", MessageType.Error);
			if (this.PaymentPlanId.IsEmpty())
				msgs.Add("PaymentPlanId is required on RegistrantPaymentPlan object", MessageType.Error);
			if (this.StartDate.IsEmpty())
				msgs.Add("StartDate is required on RegistrantPaymentPlan object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(RegistrantPaymentPlan).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(RegistrantPaymentPlan).Name);
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
			dps.Add(ObjectMap["PurchaseId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrantId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PaymentPlanId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StartDate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public RegistrantPaymentPlan Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return RegistrantPaymentPlan.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<RegistrantPaymentPlan> LoadByPurchaseId(int PurchaseId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["PurchaseId"], PurchaseId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<RegistrantPaymentPlan> LoadByRegistrantId(int RegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<RegistrantPaymentPlan> LoadByPaymentPlanId(int PaymentPlanId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["PaymentPlanId"], PaymentPlanId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static RegistrantPaymentPlan LoadByRegistrantIdByPaymentPlanId(int RegistrantId, int PaymentPlanId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			dq.Criterias.Add(ObjectMap["PaymentPlanId"], PaymentPlanId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public PaymentPlan GetPaymentPlan(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return PaymentPlan.Load(this.PaymentPlanId.Value, msgs, dc);
        }

        public Registrant GetRegistrant(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.Load(this.RegistrantId.Value, msgs, dc);
        }

        #endregion
    }
}

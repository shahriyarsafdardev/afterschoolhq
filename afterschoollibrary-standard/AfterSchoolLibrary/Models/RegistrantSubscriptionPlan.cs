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
    /// Keeps track of which registrants are on which subscription plans
    /// </summary>
	[Serializable]
    public class RegistrantSubscriptionPlan : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? PurchaseId { get { return (int?)MDV["PurchaseId"].Value; } set { MDV["PurchaseId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? SubscriptionPlanId { get { return (int?)MDV["SubscriptionPlanId"].Value; } set { MDV["SubscriptionPlanId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? RegistrantId { get { return (int?)MDV["RegistrantId"].Value; } set { MDV["RegistrantId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public DateTime? StartDate { get { return (DateTime?)MDV["StartDate"].Value; } set { MDV["StartDate"].Value = value; } }
		public string StripeSubscriptionId { get { return (string)MDV["StripeSubscriptionId"].Value; } set { MDV["StripeSubscriptionId"].Value = value; } }
		public string StripeCustomerId { get { return (string)MDV["StripeCustomerId"].Value; } set { MDV["StripeCustomerId"].Value = value; } }
		

        public RegistrantSubscriptionPlan()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(RegistrantSubscriptionPlan).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(RegistrantSubscriptionPlan).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "PurchaseId", System.Data.SqlDbType.Int);
				_dp.Add(className, "SubscriptionPlanId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrantId", System.Data.SqlDbType.Int);
				_dp.Add(className, "StartDate", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "StripeSubscriptionId", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "StripeCustomerId", System.Data.SqlDbType.NVarChar , "50");
				
                return _dp;
            }
        }

        protected static RegistrantSubscriptionPlan LoadObject(System.Data.DataRow row)
        {
            RegistrantSubscriptionPlan obj = new RegistrantSubscriptionPlan();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(RegistrantSubscriptionPlan).Name);
				
                return dq;
            }
        }

        public static RegistrantSubscriptionPlan Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<RegistrantSubscriptionPlan> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return RegistrantSubscriptionPlan.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return RegistrantSubscriptionPlan.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.PurchaseId.IsEmpty())
				msgs.Add("PurchaseId is required on RegistrantSubscriptionPlan object", MessageType.Error);
			if (this.SubscriptionPlanId.IsEmpty())
				msgs.Add("SubscriptionPlanId is required on RegistrantSubscriptionPlan object", MessageType.Error);
			if (this.RegistrantId.IsEmpty())
				msgs.Add("RegistrantId is required on RegistrantSubscriptionPlan object", MessageType.Error);
			if (this.StartDate.IsEmpty())
				msgs.Add("StartDate is required on RegistrantSubscriptionPlan object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(RegistrantSubscriptionPlan).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(RegistrantSubscriptionPlan).Name);
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
			dps.Add(ObjectMap["SubscriptionPlanId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrantId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StartDate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StripeSubscriptionId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StripeCustomerId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public RegistrantSubscriptionPlan Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return RegistrantSubscriptionPlan.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<RegistrantSubscriptionPlan> LoadByPurchaseId(int PurchaseId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["PurchaseId"], PurchaseId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<RegistrantSubscriptionPlan> LoadByRegistrantId(int RegistrantId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<RegistrantSubscriptionPlan> LoadBySubscriptionPlanId(int SubscriptionPlanId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["SubscriptionPlanId"], SubscriptionPlanId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static RegistrantSubscriptionPlan LoadByRegistrantIdBySubscriptionPlanId(int RegistrantId, int SubscriptionPlanId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantId"], RegistrantId);
			dq.Criterias.Add(ObjectMap["SubscriptionPlanId"], SubscriptionPlanId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static RegistrantSubscriptionPlan LoadByStripeSubscriptionId(string StripeSubscriptionId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["StripeSubscriptionId"], StripeSubscriptionId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public SubscriptionPlan GetSubscriptionPlan(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.Load(this.SubscriptionPlanId.Value, msgs, dc);
        }

        public Registrant GetRegistrant(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.Load(this.RegistrantId.Value, msgs, dc);
        }

        #endregion
    }
}

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
    public class PaymentPlan : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public int? ActivityId { get { return (int?)MDV["ActivityId"].Value; } set { MDV["ActivityId"].Value = value; } }
		public int? TotalPeriods { get { return (int?)MDV["TotalPeriods"].Value; } set { MDV["TotalPeriods"].Value = value; } }
		public BillingPeriodType BillingPeriodTypeId { get { return (BillingPeriodType)MDV["BillingPeriodTypeId"].Value; } set { MDV["BillingPeriodTypeId"].Value = value; } }
		public decimal? CostPerPeriod { get { return (decimal?)MDV["CostPerPeriod"].Value; } set { MDV["CostPerPeriod"].Value = value; } }
		public bool IsActive { get { return (bool)MDV["IsActive"].Value; } set { MDV["IsActive"].Value = value; } }
		

        public PaymentPlan()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(PaymentPlan).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.BillingPeriodTypeId = BillingPeriodType.NotSet;
			this.IsActive = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(PaymentPlan).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "ActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "TotalPeriods", System.Data.SqlDbType.Int);
				_dp.Add(className, "BillingPeriodTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "CostPerPeriod", System.Data.SqlDbType.Money);
				_dp.Add(className, "IsActive", System.Data.SqlDbType.Bit);
				
                return _dp;
            }
        }

        protected static PaymentPlan LoadObject(System.Data.DataRow row)
        {
            PaymentPlan obj = new PaymentPlan();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(PaymentPlan).Name);
				
                return dq;
            }
        }

        public static PaymentPlan Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<PaymentPlan> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return PaymentPlan.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return PaymentPlan.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.ActivityId.IsEmpty())
				msgs.Add("ActivityId is required on PaymentPlan object", MessageType.Error);
			if (this.TotalPeriods.IsEmpty())
				msgs.Add("TotalPeriods is required on PaymentPlan object", MessageType.Error);
			if (this.CostPerPeriod.IsEmpty())
				msgs.Add("CostPerPeriod is required on PaymentPlan object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(PaymentPlan).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(PaymentPlan).Name);
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
			dps.Add(ObjectMap["ActivityId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["TotalPeriods"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["BillingPeriodTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["CostPerPeriod"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsActive"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public PaymentPlan Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return PaymentPlan.Load(this.ID.Value, msgs, dc: dc);
		}
		
		

        #endregion

		#region "UserDefined"

        #endregion
    }
}

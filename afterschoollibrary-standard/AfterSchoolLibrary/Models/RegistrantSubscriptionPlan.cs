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
		public decimal? Amount { get { return (decimal?)MDV["Amount"].Value; } set { MDV["Amount"].Value = value; } }
		public int? TotalPeriods { get { return (int?)MDV["TotalPeriods"].Value; } set { MDV["TotalPeriods"].Value = value; } }
		public decimal? ConvenienceFee { get { return (decimal?)MDV["ConvenienceFee"].Value; } set { MDV["ConvenienceFee"].Value = value; } }
		public decimal? PaymentTypeFee { get { return (decimal?)MDV["PaymentTypeFee"].Value; } set { MDV["PaymentTypeFee"].Value = value; } }
		public FeesPaidByType PaymentTypeFeesPaidBy { get { return (FeesPaidByType)MDV["PaymentTypeFeesPaidBy"].Value; } set { MDV["PaymentTypeFeesPaidBy"].Value = value; } }
		public FeesPaidByType ConvenienceFeesPaidBy { get { return (FeesPaidByType)MDV["ConvenienceFeesPaidBy"].Value; } set { MDV["ConvenienceFeesPaidBy"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public PaymentType PaymentTypeId { get { return (PaymentType)MDV["PaymentTypeId"].Value; } set { MDV["PaymentTypeId"].Value = value; } }
		public BillingPeriodType BillingPeriodTypeId { get { return (BillingPeriodType)MDV["BillingPeriodTypeId"].Value; } set { MDV["BillingPeriodTypeId"].Value = value; } }
		public DateTime? DateCancelled { get { return (DateTime?)MDV["DateCancelled"].Value; } set { MDV["DateCancelled"].Value = value; } }
		public DateTime? ValidUntil { get { return (DateTime?)MDV["ValidUntil"].Value; } set { MDV["ValidUntil"].Value = value; } }
		

        public RegistrantSubscriptionPlan()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(RegistrantSubscriptionPlan).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.PaymentTypeFeesPaidBy = FeesPaidByType.NotSet;
			this.ConvenienceFeesPaidBy = FeesPaidByType.NotSet;
			this.PaymentTypeId = PaymentType.NotSet;
			this.BillingPeriodTypeId = BillingPeriodType.NotSet;
			
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
				_dp.Add(className, "Amount", System.Data.SqlDbType.Money);
				_dp.Add(className, "TotalPeriods", System.Data.SqlDbType.Int);
				_dp.Add(className, "ConvenienceFee", System.Data.SqlDbType.Money);
				_dp.Add(className, "PaymentTypeFee", System.Data.SqlDbType.Money);
				_dp.Add(className, "PaymentTypeFeesPaidBy", System.Data.SqlDbType.Int);
				_dp.Add(className, "ConvenienceFeesPaidBy", System.Data.SqlDbType.Int);
				_dp.Add(className, "PaymentTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "BillingPeriodTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "DateCancelled", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "ValidUntil", System.Data.SqlDbType.DateTime);
				
                return _dp;
            }
        }

        public static RegistrantSubscriptionPlan LoadObject(System.Data.DataRow row)
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
			if (this.PaymentTypeId.IsEmpty())
				msgs.Add("PaymentTypeId is required on RegistrantSubscriptionPlan object", MessageType.Error);
			
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

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Amount"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["TotalPeriods"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ConvenienceFee"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PaymentTypeFee"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PaymentTypeFeesPaidBy"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ConvenienceFeesPaidBy"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PaymentTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["BillingPeriodTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DateCancelled"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ValidUntil"]);
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

        public decimal TotalPaidWithFees
        {
            get
            {
				if (Amount.HasValue)
					return Amount.Value + 
						(ConvenienceFeesPaidBy == FeesPaidByType.Parent ? ConvenienceFee.Value : 0) + 
						(PaymentTypeFeesPaidBy == FeesPaidByType.Parent ? PaymentTypeFee.Value : 0);

				return 0;
            }
        }

        public SubscriptionPlan GetSubscriptionPlan(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.Load(this.SubscriptionPlanId.Value, msgs, dc);
        }

        public Registrant GetRegistrant(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.Load(this.RegistrantId.Value, msgs, dc);
		}

		public static bool ExistsByParticipationObjectByRegistrants(int participationObjectId, ParticipationObjectType participationObjectType, List<int> registrantIds, bool includeExpired = false, SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.Criterias.Add(ObjectMap["RegistrantId"], registrantIds, DatabaseCriteriaOperatorEnum.In);

			if (!includeExpired)
				dq.Criterias.Add(ObjectMap["DateCancelled"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);

			dq.Joins.Add(SubscriptionPlan.ObjectMap["ID"], ObjectMap["SubscriptionPlanId"], DatabaseJoinTypeEnum.InnerJoin);
			dq.Criterias.Add(SubscriptionPlan.ObjectMap["participationObjectId"], participationObjectId);
			dq.Criterias.Add(SubscriptionPlan.ObjectMap["participationObjectTypeId"], (int)participationObjectType);

			return dq.GetCount(msgs) > 0;
		}

		public static bool ExistsByParticipationObjectByRegistrantsByPlanType(int participationObjectId, ParticipationObjectType participationObjectType, List<int> registrantIds, SubscriptionPlanType planType, bool includeExpired = false, SystemMessages msgs = null, DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.Criterias.Add(ObjectMap["RegistrantId"], registrantIds, DatabaseCriteriaOperatorEnum.In);

			if (!includeExpired)
				dq.Criterias.Add(ObjectMap["DateCancelled"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);

			dq.Joins.Add(SubscriptionPlan.ObjectMap["ID"], ObjectMap["SubscriptionPlanId"], DatabaseJoinTypeEnum.InnerJoin);
			dq.Criterias.Add(SubscriptionPlan.ObjectMap["participationObjectId"], participationObjectId);
			dq.Criterias.Add(SubscriptionPlan.ObjectMap["participationObjectTypeId"], (int)participationObjectType);
			dq.Criterias.Add(SubscriptionPlan.ObjectMap["subscriptionPlanTypeId"], (int)planType);

			return dq.GetCount(msgs) > 0;
		}

		#endregion
	}
}

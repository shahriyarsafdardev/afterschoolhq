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
    /// Used to create payment options for a particular activity or activity group
    /// </summary>
	[Serializable]
    public class PaymentPlan : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public ParticipationObjectType ParticipationObjectTypeId { get { return (ParticipationObjectType)MDV["ParticipationObjectTypeId"].Value; } set { MDV["ParticipationObjectTypeId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? ParticipationObjectId { get { return (int?)MDV["ParticipationObjectId"].Value; } set { MDV["ParticipationObjectId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? PlanOwnerAccountId { get { return (int?)MDV["PlanOwnerAccountId"].Value; } set { MDV["PlanOwnerAccountId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? TotalPeriods { get { return (int?)MDV["TotalPeriods"].Value; } set { MDV["TotalPeriods"].Value = value; } }
		public BillingPeriodType BillingPeriodTypeId { get { return (BillingPeriodType)MDV["BillingPeriodTypeId"].Value; } set { MDV["BillingPeriodTypeId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public decimal? CostPerPeriod { get { return (decimal?)MDV["CostPerPeriod"].Value; } set { MDV["CostPerPeriod"].Value = value; } }
		public bool IsActive { get { return (bool)MDV["IsActive"].Value; } set { MDV["IsActive"].Value = value; } }
		public string StripePlanId { get { return (string)MDV["StripePlanId"].Value; } set { MDV["StripePlanId"].Value = value; } }
		

        public PaymentPlan()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(PaymentPlan).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.ParticipationObjectTypeId = ParticipationObjectType.NotSet;
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
				_dp.Add(className, "ParticipationObjectTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ParticipationObjectId", System.Data.SqlDbType.Int);
				_dp.Add(className, "PlanOwnerAccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "TotalPeriods", System.Data.SqlDbType.Int);
				_dp.Add(className, "BillingPeriodTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "CostPerPeriod", System.Data.SqlDbType.Money);
				_dp.Add(className, "IsActive", System.Data.SqlDbType.Bit);
				_dp.Add(className, "StripePlanId", System.Data.SqlDbType.NVarChar, 50);
				
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

			if (this.ParticipationObjectTypeId.IsEmpty())
				msgs.Add("ParticipationObjectTypeId is required on PaymentPlan object", MessageType.Error);
			if (this.ParticipationObjectId.IsEmpty())
				msgs.Add("ParticipationObjectId is required on PaymentPlan object", MessageType.Error);
			if (this.PlanOwnerAccountId.IsEmpty())
				msgs.Add("PlanOwnerAccountId is required on PaymentPlan object", MessageType.Error);
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
			dps.Add(ObjectMap["ParticipationObjectTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ParticipationObjectId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PlanOwnerAccountId"]);
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

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StripePlanId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public PaymentPlan Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return PaymentPlan.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<PaymentPlan> LoadByParticipationObject(int ParticipationObjectId, ParticipationObjectType ParticipationObjectTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ParticipationObjectId"], ParticipationObjectId);
			dq.Criterias.Add(ObjectMap["ParticipationObjectTypeId"], ParticipationObjectTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<PaymentPlan> LoadByPlanOwnerAccountId(int PlanOwnerAccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["PlanOwnerAccountId"], PlanOwnerAccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}



        #endregion

        public bool IsPurchased(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantPaymentPlan.GetCount(new Hashtable() { { "ID", this.ID.Value } }, msgs, dc) > 0;
        }

        #region "UserDefined"
        public static List<PaymentPlan> LoadActiveByParticipationObject(int ParticipationObjectId, ParticipationObjectType ParticipationObjectTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ParticipationObjectId"], ParticipationObjectId);
            dq.Criterias.Add(ObjectMap["ParticipationObjectTypeId"], ParticipationObjectTypeId);
            dq.Criterias.Add(ObjectMap["IsActive"], true);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public SystemMessages Deactivate(int loggedInUserId, DatabaseConnection dc = null)
        {
            this.IsActive = false;
            this.LastModifiedDate = DateTime.UtcNow;
            this.LastModifiedBy = loggedInUserId;
            return this.SaveWithStripe(dc);
        }

        public Activity GetActivity(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Activity.Load(this.ParticipationObjectId.Value, msgs, dc);
        }

        public Account GetAccount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.Load(this.PlanOwnerAccountId.Value, msgs, dc);
        }

        public string GetNickName(Activity activity)
        {
            return $"{activity.Name} Plan {this.CostPerPeriod.ToCurrency()} / {this.BillingPeriodTypeId} for {this.TotalPeriods} {this.BillingPeriodTypeId}";
        }

        public SystemMessages SaveWithStripe(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(this.Save(dc));

            if (msgs.HasErrors())
                return msgs;

            var activity = this.GetActivity(msgs, dc);
            var account = activity.GetAccount(msgs, dc);

            // do stripe
            Stripe.StripeProduct product = Billing.StripeHelper.GetProduct(activity.Uid.ToString(), account.StripeUserId, msgs);

            if (product.IsEmpty())
            {
                product = Billing.StripeHelper.CreateProduct(
                    activity.Uid.ToString(),
                    activity.Name,
                    activity.UniqueName,
                    this.IsActive,
                    account.StripeUserId,
                    msgs);
            }

            if (msgs.HasErrors())
                return msgs;

            Stripe.StripePlan plan = null;

            if (this.StripePlanId.IsNotEmpty())
            {
                plan = Billing.StripeHelper.GetPlan(this.StripePlanId, account.StripeUserId, msgs);
                Billing.StripeHelper.UpdatePlan(this.StripePlanId, plan.Nickname, plan.Active, account.StripeUserId, msgs);
            }
            else
            {
                plan = Billing.StripeHelper.CreatePlan(
                    product,
                    null,
                    this.CostPerPeriod.Value,
                    this.BillingPeriodTypeId,
                    this.GetNickName(activity),
                    account.StripeUserId,
                    msgs
                    );
            }

            // update stripe plan id
            this.StripePlanId = plan.Id;
            msgs.AddRange(this.Save(dc));

            return msgs;
        }

        #endregion
    }
}

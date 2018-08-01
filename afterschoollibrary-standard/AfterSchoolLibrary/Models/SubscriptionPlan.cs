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
    /// Used to create subscription options for a particular activity or activity group
    /// </summary>
	[Serializable]
    public class SubscriptionPlan : BasicObjectWithModified
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
		public decimal? CostPerPeriod { get { return (decimal?)MDV["CostPerPeriod"].Value; } set { MDV["CostPerPeriod"].Value = value; } }
		public BillingPeriodType BillingPeriodTypeId { get { return (BillingPeriodType)MDV["BillingPeriodTypeId"].Value; } set { MDV["BillingPeriodTypeId"].Value = value; } }
		public bool DoesAutoRenew { get { return (bool)MDV["DoesAutoRenew"].Value; } set { MDV["DoesAutoRenew"].Value = value; } }
		public bool IsActive { get { return (bool)MDV["IsActive"].Value; } set { MDV["IsActive"].Value = value; } }
		public int? TotalPeriods { get { return (int?)MDV["TotalPeriods"].Value; } set { MDV["TotalPeriods"].Value = value; } }
		public string StripePlanId { get { return (string)MDV["StripePlanId"].Value; } set { MDV["StripePlanId"].Value = value; } }
		public SubscriptionPlanType PlanTypeId { get { return (SubscriptionPlanType)MDV["PlanTypeId"].Value; } set { MDV["PlanTypeId"].Value = value; } }
		

        public SubscriptionPlan()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(SubscriptionPlan).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.ParticipationObjectTypeId = ParticipationObjectType.NotSet;
			this.BillingPeriodTypeId = BillingPeriodType.NotSet;
			this.DoesAutoRenew = false;
			this.IsActive = false;
			this.PlanTypeId = SubscriptionPlanType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(SubscriptionPlan).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "ParticipationObjectTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ParticipationObjectId", System.Data.SqlDbType.Int);
				_dp.Add(className, "PlanOwnerAccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "CostPerPeriod", System.Data.SqlDbType.Money);
				_dp.Add(className, "BillingPeriodTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "DoesAutoRenew", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsActive", System.Data.SqlDbType.Bit);
				_dp.Add(className, "TotalPeriods", System.Data.SqlDbType.Int);
				_dp.Add(className, "StripePlanId", System.Data.SqlDbType.NVarChar, 50);
				_dp.Add(className, "PlanTypeId", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        protected static SubscriptionPlan LoadObject(System.Data.DataRow row)
        {
            SubscriptionPlan obj = new SubscriptionPlan();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(SubscriptionPlan).Name);
				
                return dq;
            }
        }

        public static SubscriptionPlan Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<SubscriptionPlan> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return SubscriptionPlan.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return SubscriptionPlan.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.ParticipationObjectTypeId.IsEmpty())
				msgs.Add("ParticipationObjectTypeId is required on SubscriptionPlan object", MessageType.Error);
			if (this.ParticipationObjectId.IsEmpty())
				msgs.Add("ParticipationObjectId is required on SubscriptionPlan object", MessageType.Error);
			if (this.PlanOwnerAccountId.IsEmpty())
				msgs.Add("PlanOwnerAccountId is required on SubscriptionPlan object", MessageType.Error);
			if (this.CostPerPeriod.IsEmpty())
				msgs.Add("CostPerPeriod is required on SubscriptionPlan object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(SubscriptionPlan).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(SubscriptionPlan).Name);
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
			dps.Add(ObjectMap["CostPerPeriod"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["BillingPeriodTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DoesAutoRenew"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsActive"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["TotalPeriods"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StripePlanId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PlanTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public SubscriptionPlan Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return SubscriptionPlan.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<SubscriptionPlan> LoadByParticipationObject(int ParticipationObjectId, ParticipationObjectType ParticipationObjectTypeId, SubscriptionPlanType PlanTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ParticipationObjectId"], ParticipationObjectId);
			dq.Criterias.Add(ObjectMap["ParticipationObjectTypeId"], ParticipationObjectTypeId);
			dq.Criterias.Add(ObjectMap["PlanTypeId"], PlanTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<SubscriptionPlan> LoadByPlanOwnerAccountId(int PlanOwnerAccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["PlanOwnerAccountId"], PlanOwnerAccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        public bool IsPurchased(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantSubscriptionPlan.GetCount(new Hashtable() { { "SubscriptionPlanId", this.ID.Value } }, msgs, dc) > 0;
        }

        public static List<SubscriptionPlan> LoadActiveByParticipationObject(int ParticipationObjectId, ParticipationObjectType ParticipationObjectTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
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
            return this.Save(dc);
        }

        public Activity GetActivity(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Activity.Load(this.ParticipationObjectId.Value, msgs, dc);
        }

        public ActivityGroup GetActivityGroup(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityGroup.Load(this.ParticipationObjectId.Value, msgs, dc);
        }

        public Account GetAccount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.Load(this.PlanOwnerAccountId.Value, msgs, dc);
        }

        public string GetNickName(string participationObjectName)
        {
            return $"{participationObjectName} Plan {this.CostPerPeriod.ToCurrency()} / {this.BillingPeriodTypeId} for {this.TotalPeriods} {this.BillingPeriodTypeId}";
        }

        private SubscriptionPlan CopyFields()
        {
            return new SubscriptionPlan()
            {
                ParticipationObjectTypeId = this.ParticipationObjectTypeId,
                ParticipationObjectId = this.ParticipationObjectId,
                PlanOwnerAccountId = this.PlanOwnerAccountId,
                CostPerPeriod = this.CostPerPeriod,
                BillingPeriodTypeId = this.BillingPeriodTypeId,
                DoesAutoRenew = this.DoesAutoRenew,
                IsActive = this.IsActive,
                StripePlanId = this.StripePlanId,
                PlanTypeId = this.PlanTypeId,
                TotalPeriods = this.TotalPeriods
            };
        }

        public SubscriptionPlan Copy(int newParticipationObjectId, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            SubscriptionPlan copy = null;

            Database.ExecuteTransaction(
               action: (SystemMessages amsgs, DatabaseConnection adc) =>
               {
                   copy = this.CopyFields();
                   copy.ParticipationObjectId = newParticipationObjectId;
                   copy.CreateDate = DateTime.UtcNow;
                   copy.CreatedBy = loggedInUserId;
                   copy.LastModifiedDate = DateTime.UtcNow;
                   copy.LastModifiedBy = loggedInUserId;
                   amsgs.AddRange(copy.Save(adc));
               },
               onSuccess: (SystemMessages smsgs) =>
               {
                   msgs.AddRange(smsgs);
               },
               onFailure: (SystemMessages fmsgs) =>
               {
                   msgs.AddRange(fmsgs);
               },
               dc: dc
           );

            return copy;
        }

        public SystemMessages SaveWithStripe(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(this.Save(dc));

            if (msgs.HasErrors())
                return msgs;

            Account account = null;
            string uid = "";
            string name = "";
            string uniqueName = "";

            if (this.ParticipationObjectTypeId == ParticipationObjectType.Activity)
            {
                var activity = this.GetActivity(msgs, dc);
                account = activity.GetAccount(msgs, dc);
                uid = activity.Uid.ToString();
                name = activity.Name;
                uniqueName = activity.UniqueName;
            }
            else
            {
                var activityGroup = this.GetActivityGroup(msgs, dc);
                account = activityGroup.GetAccount(msgs, dc);
                uid = activityGroup.Uid.ToString();
                name = activityGroup.Name;
                uniqueName = activityGroup.UniqueName;
            }

            // do stripe
            Stripe.StripeProduct product = Billing.StripeHelper.GetProduct(uid, account.StripeUserId, msgs);

            if (product.IsEmpty())
            {
                product = Billing.StripeHelper.CreateProduct(
                    uid,
                    name,
                    uniqueName,
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
                    this.GetNickName(name),
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

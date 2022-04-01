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
		public int? StartDayOfMonth { get { return (int?)MDV["StartDayOfMonth"].Value; } set { MDV["StartDayOfMonth"].Value = value; } }
		public PlanPaymentStartType PaymentStartTypeId { get { return (PlanPaymentStartType)MDV["PaymentStartTypeId"].Value; } set { MDV["PaymentStartTypeId"].Value = value; } }
		/// <summary>
		/// Split cost evenly amongst all parents signed up
		/// </summary>
		public bool SplitCostEvenly { get { return (bool)MDV["SplitCostEvenly"].Value; } set { MDV["SplitCostEvenly"].Value = value; } }
		/// <summary>
		/// Count the registration fee as a down payment
		/// </summary>
		public bool RegFeeAsDownPayment { get { return (bool)MDV["RegFeeAsDownPayment"].Value; } set { MDV["RegFeeAsDownPayment"].Value = value; } }
		public bool IsConvenienceFeeTakenOutOfCost { get { return (bool)MDV["IsConvenienceFeeTakenOutOfCost"].Value; } set { MDV["IsConvenienceFeeTakenOutOfCost"].Value = value; } }
		public int? NumberOfPeriodsPrePaid { get { return (int?)MDV["NumberOfPeriodsPrePaid"].Value; } set { MDV["NumberOfPeriodsPrePaid"].Value = value; } }
		public bool IsOngoingSubscription { get { return (bool)MDV["IsOngoingSubscription"].Value; } set { MDV["IsOngoingSubscription"].Value = value; } }
		public DateTime? StartDate { get { return (DateTime?)MDV["StartDate"].Value; } set { MDV["StartDate"].Value = value; } }
		public DateTime? EndDate { get { return (DateTime?)MDV["EndDate"].Value; } set { MDV["EndDate"].Value = value; } }
		

        public SubscriptionPlan()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(SubscriptionPlan).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.ParticipationObjectTypeId = ParticipationObjectType.NotSet;
			this.BillingPeriodTypeId = BillingPeriodType.NotSet;
			this.DoesAutoRenew = false;
			this.IsActive = false;
			this.PlanTypeId = SubscriptionPlanType.NotSet;
			this.PaymentStartTypeId = PlanPaymentStartType.NotSet;
			this.SplitCostEvenly = false;
			this.RegFeeAsDownPayment = false;
			this.IsConvenienceFeeTakenOutOfCost = false;
			this.IsOngoingSubscription = false;
			
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
				_dp.Add(className, "StripePlanId", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "PlanTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "StartDayOfMonth", System.Data.SqlDbType.Int);
				_dp.Add(className, "PaymentStartTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "SplitCostEvenly", System.Data.SqlDbType.Bit);
				_dp.Add(className, "RegFeeAsDownPayment", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsConvenienceFeeTakenOutOfCost", System.Data.SqlDbType.Bit);
				_dp.Add(className, "NumberOfPeriodsPrePaid", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsOngoingSubscription", System.Data.SqlDbType.Bit);
				_dp.Add(className, "StartDate", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "EndDate", System.Data.SqlDbType.DateTime);
				
                return _dp;
            }
        }

        public static SubscriptionPlan LoadObject(System.Data.DataRow row)
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

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StartDayOfMonth"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PaymentStartTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["SplitCostEvenly"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegFeeAsDownPayment"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsConvenienceFeeTakenOutOfCost"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["NumberOfPeriodsPrePaid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsOngoingSubscription"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StartDate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["EndDate"]);
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

		public static SubscriptionPlan LoadByStripePlanId(string StripePlanId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["StripePlanId"], StripePlanId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
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
			return $"{participationObjectName} Plan {this.CostPerPeriod.ToCurrency()} / {this.BillingPeriodTypeId}";
        }

        #endregion
    }
}

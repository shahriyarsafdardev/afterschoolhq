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
    /// This is how an package is assigned to an account. This is the only way an account can actually login and also have features that are accessible.
    /// </summary>
	[Serializable]
    public class AccountPackage : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? PackageId { get { return (int?)MDV["PackageId"].Value; } set { MDV["PackageId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public DateTime? StartDate { get { return (DateTime?)MDV["StartDate"].Value; } set { MDV["StartDate"].Value = value; } }
		public DateTime? EndDate { get { return (DateTime?)MDV["EndDate"].Value; } set { MDV["EndDate"].Value = value; } }
		public decimal? Price { get { return (decimal?)MDV["Price"].Value; } set { MDV["Price"].Value = value; } }
		public BillingPeriodType BillingPeriodTypeId { get { return (BillingPeriodType)MDV["BillingPeriodTypeId"].Value; } set { MDV["BillingPeriodTypeId"].Value = value; } }
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public DateTime? TrialStartDate { get { return (DateTime?)MDV["TrialStartDate"].Value; } set { MDV["TrialStartDate"].Value = value; } }
		public DateTime? TrialEndDate { get { return (DateTime?)MDV["TrialEndDate"].Value; } set { MDV["TrialEndDate"].Value = value; } }
		public bool IsActive { get { return (bool)MDV["IsActive"].Value; } set { MDV["IsActive"].Value = value; } }
		public string StripeSubscriptionId { get { return (string)MDV["StripeSubscriptionId"].Value; } set { MDV["StripeSubscriptionId"].Value = value; } }
		public string PackageName { get { return (string)MDV["PackageName"].Value; } }
		public Guid PackageUid { get { return (Guid)MDV["PackageUid"].Value; } }
		public decimal? PackagePrice { get { return (decimal?)MDV["PackagePrice"].Value; } }
		public decimal? PackagePricePerYear { get { return (decimal?)MDV["PackagePricePerYear"].Value; } }
		

        public AccountPackage()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(AccountPackage).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.BillingPeriodTypeId = BillingPeriodType.NotSet;
			this.Uid = Guid.NewGuid();
			this.IsActive = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(AccountPackage).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "PackageId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "StartDate", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "EndDate", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "Price", System.Data.SqlDbType.Money);
				_dp.Add(className, "BillingPeriodTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "TrialStartDate", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "TrialEndDate", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "IsActive", System.Data.SqlDbType.Bit);
				_dp.Add(className, "StripeSubscriptionId", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "PackageName", typeof(Package).Name, "Name", System.Data.SqlDbType.NVarChar, "Package");
				_dp.Add(className, "PackageUid", typeof(Package).Name, "Uid", System.Data.SqlDbType.UniqueIdentifier, "Package");
				_dp.Add(className, "PackagePrice", typeof(Package).Name, "Price", System.Data.SqlDbType.Money, "Package");
				_dp.Add(className, "PackagePricePerYear", typeof(Package).Name, "PricePerYear", System.Data.SqlDbType.Money, "Package");
				
                return _dp;
            }
        }

        public static AccountPackage LoadObject(System.Data.DataRow row)
        {
            AccountPackage obj = new AccountPackage();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(AccountPackage).Name);
				dq.Joins.Add(Package.ObjectMap["ID"], AccountPackage.ObjectMap["PackageId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Package");
				
                return dq;
            }
        }

        public static AccountPackage Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<AccountPackage> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return AccountPackage.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return AccountPackage.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.PackageId.IsEmpty())
				msgs.Add("PackageId is required on AccountPackage object", MessageType.Error);
			if (this.AccountId.IsEmpty())
				msgs.Add("AccountId is required on AccountPackage object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(AccountPackage).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(AccountPackage).Name);
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
			dps.Add(ObjectMap["PackageId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StartDate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["EndDate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Price"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["BillingPeriodTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["TrialStartDate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["TrialEndDate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsActive"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StripeSubscriptionId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public AccountPackage Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return AccountPackage.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<AccountPackage> LoadByPackageId(int PackageId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["PackageId"], PackageId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<AccountPackage> LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static AccountPackage LoadByAccountIdByIsActive(int AccountId, bool IsActive, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static AccountPackage LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static AccountPackage LoadByUidByAccountId(Guid Uid, int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public bool HasFeature(FeatureType featureType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return this.GetFeatures(msgs, dc).Where(x => x.FeatureTypeId == featureType && x.Max.HasValue && x.Max > 0).Any();
        }

        public AccountPackageFeature GetFeature(FeatureType featureType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return this.GetFeatures(msgs, dc).Where(x => x.FeatureTypeId == featureType).FirstOrDefault();
        }
        
        public AccountPackageFeature AddPackageFeature(PackageFeature pf, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return AddOrUpdatePackageFeature(pf.FeatureTypeId, pf.Max.Value, loggedInUser, msgs, dc);
        }

        public AccountPackageFeature AddOrUpdatePackageFeature(FeatureType featureTypeId, decimal maxValue, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var apf = AccountPackageFeature.LoadByAccountPackageIdByFeatureTypeId(this.ID.Value, featureTypeId, msgs, dc);

            if (apf.IsEmpty())
            {
                apf = new AccountPackageFeature();
                apf.AccountPackageId = this.ID;
                apf.FeatureTypeId = featureTypeId;
                apf.CreateDate = DateTime.UtcNow;
                apf.CreatedBy = loggedInUser?.ID.IfEmpty(0);
            }

            apf.Max = maxValue;
            apf.LastModifiedDate = DateTime.UtcNow;
            apf.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);
            msgs.AddRange(apf.Save(dc: dc));

            return apf;
        }

        public Package ToPackage()
        {
            return new Package()
            {
                ID = this.PackageId,
                Name = this.PackageName,
                Price = this.PackagePrice,
                PricePerYear = this.PackagePricePerYear
            };
        }

        public SystemMessages CancelPackage(DatabaseConnection dc = null)
        {
            this.IsActive = false;
            this.EndDate = DateTime.UtcNow;
            return this.Save(dc: dc);
        }

        public SystemMessages CancelPackageWithStripe(string stripeCustomerId, bool cancelAtPeriodEnd = true, DatabaseConnection dc = null)
        {
            var msgs = this.CancelPackage(dc: dc);

            if(this.StripeSubscriptionId.IsNotEmpty())
            {
                msgs.AddRange(Billing.StripeHelper.CancelSubscription(this.StripeSubscriptionId, null, cancelAtPeriodEnd));
            }

            return msgs;
        }

        public Package GetPackage(SystemMessages msgs, DatabaseConnection dc = null)
        {
            return Package.Load(this.PackageId.Value, msgs, dc);
        }

        public SystemMessages SaveWithStripe(string stripeCustomerId, bool upgradingFromTrial, string priceId, DatabaseConnection dc = null)
        {
            SystemMessages msgs = new SystemMessages();

            msgs.AddRange(this.Save(dc: dc));

            if (!stripeCustomerId.IsEmpty())
            {
                // first time, may have payment info
                if (this.StripeSubscriptionId.IsEmpty())
                {
                    var subscription = Billing.StripeHelper.CreatePackageSubscription(
                        customerId: stripeCustomerId, 
                        priceId: priceId, //TODO: dynamic price for subscription
                        sourceId: null,
                        platformFeePercent: null,
                        // trialEndDate: this.TrialEndDate, 
                        startDate: DateTime.UtcNow,
                        stripeAccountId: null,
                        stripeCouponId: null,
                        metaData: null,
                        msgs: msgs);

                    if (subscription.IsNotEmpty())
                    {
                        this.StripeSubscriptionId = subscription.Id;
                        msgs.AddRange(this.Save(dc: dc));
                    }
                    
                }
                else
                {
                    Billing.StripeHelper.UpdatePackageSubscription(this.StripeSubscriptionId, priceId, msgs);
                }
            }

            return msgs;
        }

        public bool IsFree
        {
            get
            {
                return this.Price.Value == (0).ToDecimal();
            }
        }

        public bool IsDuringTrial
        {
            get
            {
                return this.TrialEndDate.HasValue ? this.TrialEndDate.Value > DateTime.UtcNow : false;
            }
        }

        public bool IsTrialOver
        {
            get
            {
                return TrialDaysRemaining == 0;
            }
        }

        public int TrialDaysRemaining
        {
            get
            {
                if (IsDuringTrial)
                    return (TrialEndDate.Value - DateTime.UtcNow).Days + 1;

                return 0;
            }
        }

        public bool IsTrialExpired(Account account)
        {
            return !this.IsFree && this.IsTrialOver && !account.HasPaymentCard;
        }

        public List<AccountPackageFeature> GetFeatures(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountPackageFeature.LoadByAccountPackageId(this.ID.Value, msgs, dc);
        }

        public SystemMessages UpdateTrialDays(int trialDays, DatabaseConnection dc)
        {
            var msgs = new SystemMessages();

            this.TrialEndDate = TrialStartDate.Value.AddDays(trialDays);
            msgs.AddRange(this.Save(dc));

            return msgs;
        }

        public static AccountPackage LoadFirstFromListByAccountIdByIsActiveByOrderProperty(int AccountId, bool IsActive, string OrderByProperty, OrderByDirectionEnum orderByDirection, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
            dq.OrderBys.Add(OrderByProperty, orderByDirection);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<AccountPackage> LoadByIds(List<int> ids, SystemMessages msgs, DatabaseConnection dc)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["IsActive"], true);
            dq.Criterias.Add(ObjectMap["ID"], ids, DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        #endregion
    }
}

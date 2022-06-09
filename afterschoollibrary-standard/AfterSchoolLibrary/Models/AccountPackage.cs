using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AfterSchoolLibrary.Billing;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Models
{
    /// <summary>
    ///     This is how an package is assigned to an account. This is the only way an account can actually login and also have
    ///     features that are accessible.
    /// </summary>
    [Serializable]
    public class AccountPackage : BasicObjectWithModified
    {
        #region "ClassGenerator"

        /// <summary>
        ///     Required.
        /// </summary>
        public int? PackageId
        {
            get => (int?) MDV["PackageId"].Value;
            set => MDV["PackageId"].Value = value;
        }

        /// <summary>
        ///     Required.
        /// </summary>
        public int? AccountId
        {
            get => (int?) MDV["AccountId"].Value;
            set => MDV["AccountId"].Value = value;
        }

        public DateTime? StartDate
        {
            get => (DateTime?) MDV["StartDate"].Value;
            set => MDV["StartDate"].Value = value;
        }

        public DateTime? EndDate
        {
            get => (DateTime?) MDV["EndDate"].Value;
            set => MDV["EndDate"].Value = value;
        }

        public decimal? Price
        {
            get => (decimal?) MDV["Price"].Value;
            set => MDV["Price"].Value = value;
        }

        public BillingPeriodType BillingPeriodTypeId
        {
            get => (BillingPeriodType) MDV["BillingPeriodTypeId"].Value;
            set => MDV["BillingPeriodTypeId"].Value = value;
        }

        public Guid Uid
        {
            get => (Guid) MDV["Uid"].Value;
            set => MDV["Uid"].Value = value;
        }

        public DateTime? TrialStartDate
        {
            get => (DateTime?) MDV["TrialStartDate"].Value;
            set => MDV["TrialStartDate"].Value = value;
        }

        public DateTime? TrialEndDate
        {
            get => (DateTime?) MDV["TrialEndDate"].Value;
            set => MDV["TrialEndDate"].Value = value;
        }

        public bool IsActive
        {
            get => (bool) MDV["IsActive"].Value;
            set => MDV["IsActive"].Value = value;
        }

        public string StripeSubscriptionId
        {
            get => (string) MDV["StripeSubscriptionId"].Value;
            set => MDV["StripeSubscriptionId"].Value = value;
        }

        public string PackageName => (string) MDV["PackageName"].Value;
        public Guid PackageUid => (Guid) MDV["PackageUid"].Value;
        public decimal? PackagePrice => (decimal?) MDV["PackagePrice"].Value;
        public decimal? PackagePricePerYear => (decimal?) MDV["PackagePricePerYear"].Value;


        public AccountPackage()
        {
            Config(ObjectMap, DatabaseQuery, typeof(AccountPackage).Name, "ID");
            LoadObjectValues(ObjectMap);
            BillingPeriodTypeId = BillingPeriodType.NotSet;
            Uid = Guid.NewGuid();
            IsActive = false;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                var _dp = new DatabaseParameters();
                var className = typeof(AccountPackage).Name;
                _dp = get_BaseObjectMap(className);
                _dp.Add(className, "PackageId", SqlDbType.Int);
                _dp.Add(className, "AccountId", SqlDbType.Int);
                _dp.Add(className, "StartDate", SqlDbType.DateTime);
                _dp.Add(className, "EndDate", SqlDbType.DateTime);
                _dp.Add(className, "Price", SqlDbType.Money);
                _dp.Add(className, "BillingPeriodTypeId", SqlDbType.Int);
                _dp.Add(className, "Uid", SqlDbType.UniqueIdentifier);
                _dp.Add(className, "TrialStartDate", SqlDbType.DateTime);
                _dp.Add(className, "TrialEndDate", SqlDbType.DateTime);
                _dp.Add(className, "IsActive", SqlDbType.Bit);
                _dp.Add(className, "StripeSubscriptionId", SqlDbType.NVarChar, "50");
                _dp.Add(className, "PackageName", typeof(Package).Name, "Name", SqlDbType.NVarChar, "Package");
                _dp.Add(className, "PackageUid", typeof(Package).Name, "Uid", SqlDbType.UniqueIdentifier, "Package");
                _dp.Add(className, "PackagePrice", typeof(Package).Name, "Price", SqlDbType.Money, "Package");
                _dp.Add(className, "PackagePricePerYear", typeof(Package).Name, "PricePerYear", SqlDbType.Money,
                    "Package");

                return _dp;
            }
        }

        public static AccountPackage LoadObject(DataRow row)
        {
            var obj = new AccountPackage();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, typeof(AccountPackage).Name);
                dq.Joins.Add(Package.ObjectMap["ID"], ObjectMap["PackageId"], DatabaseJoinTypeEnum.InnerJoin,
                    "Package");

                return dq;
            }
        }

        public static AccountPackage Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<AccountPackage> Load(Hashtable criterias = null, int? start = null, int? length = null,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            return BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

        public SystemMessages Save(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if (PackageId.IsEmpty())
                msgs.Add("PackageId is required on AccountPackage object", MessageType.Error);
            if (AccountId.IsEmpty())
                msgs.Add("AccountId is required on AccountPackage object", MessageType.Error);

            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(AccountPackage).Name));

            return msgs;
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(AccountPackage).Name);
        }

        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return _Create(DatabaseQuery, dc, ObjectMap);
        }

        public static SystemMessages Alter(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            DatabaseParameters dps;

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["PackageId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["AccountId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["StartDate"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["EndDate"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["Price"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["BillingPeriodTypeId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["Uid"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["TrialStartDate"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["TrialEndDate"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsActive"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["StripeSubscriptionId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));


            return msgs;
        }

        public AccountPackage Reload(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Load(ID.Value, msgs, dc);
        }

        public static List<AccountPackage> LoadByPackageId(int PackageId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["PackageId"], PackageId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<AccountPackage> LoadByAccountId(int AccountId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static AccountPackage LoadByAccountIdByIsActive(int AccountId, bool IsActive, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static AccountPackage LoadByUid(Guid Uid, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Uid"], Uid);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static AccountPackage LoadByUidByAccountId(Guid Uid, int AccountId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
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
            return GetFeatures(msgs, dc)
                .Any(x => x.FeatureTypeId == featureType && x.Max.HasValue && x.Max > 0);
        }

        public bool HasAnyFeature(FeatureType[] featureTypes, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var features = GetFeatures(msgs, dc);

            foreach (var featureType in featureTypes)
                if (features.Any(x => x.FeatureTypeId == featureType && x.Max.HasValue && x.Max > 0))
                    return true;

            return false;
        }

        public AccountPackageFeature GetFeature(FeatureType featureType, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var features = GetFeatures(msgs, dc);
            return features.FirstOrDefault(x => x.FeatureTypeId == featureType);
        }

        public AccountPackageFeature AddPackageFeature(PackageFeature pf, WebUser loggedInUser, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            return AddOrUpdatePackageFeature(pf.FeatureTypeId, pf.Max.Value, loggedInUser, msgs, dc);
        }

        public AccountPackageFeature AddOrUpdatePackageFeature(FeatureType featureTypeId, decimal maxValue,
            WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return AddOrUpdatePackageFeature(featureTypeId, maxValue, loggedInUser?.ID.IfEmpty(0), msgs, dc);
        }

        public AccountPackageFeature AddOrUpdatePackageFeature(FeatureType featureTypeId, decimal maxValue,
            int? lastModifiedById,
            SystemMessages msgs, DatabaseConnection dc)
        {
            var apf = AccountPackageFeature.LoadByAccountPackageIdByFeatureTypeId(ID.Value, featureTypeId, msgs, dc);

            if (apf.IsEmpty())
            {
                apf = new AccountPackageFeature();
                apf.AccountPackageId = ID;
                apf.FeatureTypeId = featureTypeId;
                apf.CreateDate = DateTime.UtcNow;
                apf.CreatedBy = lastModifiedById;
            }

            apf.Max = maxValue;
            apf.LastModifiedDate = DateTime.UtcNow;
            apf.LastModifiedBy = lastModifiedById;
            msgs.AddRange(apf.Save(dc));

            return apf;
        }

        public Package ToPackage()
        {
            return new Package
            {
                ID = PackageId,
                Name = PackageName,
                Price = PackagePrice,
                PricePerYear = PackagePricePerYear,
            };
        }

        public SystemMessages CancelPackage(DatabaseConnection dc = null)
        {
            IsActive = false;
            EndDate = DateTime.UtcNow;
            return Save(dc);
        }

        public SystemMessages CancelPackageWithStripe(string stripeCustomerId, bool cancelAtPeriodEnd = true,
            DatabaseConnection dc = null)
        {
            var msgs = CancelPackage(dc);

            if (StripeSubscriptionId.IsNotEmpty())
                msgs.AddRange(new StripeHelper(StripeHelper.AHQSubsSecretKey).CancelSubscription(StripeSubscriptionId, null, cancelAtPeriodEnd));

            return msgs;
        }

        public Package GetPackage(SystemMessages msgs, DatabaseConnection dc = null)
        {
            return Package.Load(PackageId.Value, msgs, dc);
        }

        public SystemMessages SaveWithStripe(string stripeCustomerId, bool upgradingFromTrial, string priceId,
            DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(Save(dc));

            if (!stripeCustomerId.IsEmpty())
            {
                var stripeHelper = new StripeHelper(StripeHelper.AHQSubsSecretKey);
                
                // first time, may have payment info
                if (StripeSubscriptionId.IsEmpty())
                {
                    var subscription = stripeHelper.CreatePackageSubscription(
                        stripeCustomerId,
                        priceId, //TODO: dynamic price for subscription
                        null,
                        null,
                        // trialEndDate: this.TrialEndDate, 
                        DateTime.UtcNow,
                        null,
                        null,
                        null,
                        msgs);

                    if (subscription.IsNotEmpty())
                    {
                        StripeSubscriptionId = subscription.Id;
                        msgs.AddRange(Save(dc));
                    }
                }
                else
                {
                    stripeHelper.UpdatePackageSubscription(StripeSubscriptionId, priceId, msgs);
                }
            }

            return msgs;
        }

        public bool IsFree => Price.Value == 0.ToDecimal();

        public bool IsDuringTrial => TrialEndDate.HasValue ? TrialEndDate.Value > DateTime.UtcNow : false;

        public bool IsTrialOver => TrialDaysRemaining == 0;

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
            return !IsFree && IsTrialOver && !account.HasPaymentCard;
        }

        public List<AccountPackageFeature> GetFeatures(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountPackageFeature.LoadByAccountPackageId(ID.Value, msgs, dc);
        }

        public SystemMessages UpdateTrialDays(int trialDays, DatabaseConnection dc)
        {
            var msgs = new SystemMessages();

            TrialEndDate = TrialStartDate.Value.AddDays(trialDays);
            msgs.AddRange(Save(dc));

            return msgs;
        }

        public static AccountPackage LoadFirstFromListByAccountIdByIsActiveByOrderProperty(int AccountId, bool IsActive,
            string OrderByProperty, OrderByDirectionEnum orderByDirection, SystemMessages msgs = null,
            DatabaseConnection dc = null)
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
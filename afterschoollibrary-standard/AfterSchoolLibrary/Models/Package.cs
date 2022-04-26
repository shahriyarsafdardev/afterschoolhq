using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AfterSchoolLibrary.Billing;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Stripe;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchoolLibrary.Models
{
	/// <summary>
	///     This is an actual package that an account/organization can purchase when signing up. There are multiple packages
	///     that are able to be purchased at a time. A package containts multiple Features.
	/// </summary>
	public class Package : BasicLookUpObject
    {
        #region "ClassGenerator"

        /// <summary>
        ///     Required.
        /// </summary>
        public decimal? Price
        {
            get => (decimal?) MDV["Price"].Value;
            set => MDV["Price"].Value = value;
        }

        /// <summary>
        ///     Required.
        /// </summary>
        public decimal? PricePerYear
        {
            get => (decimal?) MDV["PricePerYear"].Value;
            set => MDV["PricePerYear"].Value = value;
        }

        public bool IsActive
        {
            get => (bool) MDV["IsActive"].Value;
            set => MDV["IsActive"].Value = value;
        }

        public bool IsPopular
        {
            get => (bool) MDV["IsPopular"].Value;
            set => MDV["IsPopular"].Value = value;
        }

        public bool IsAfterSchool
        {
            get => (bool) MDV["IsAfterSchool"].Value;
            set => MDV["IsAfterSchool"].Value = value;
        }

        public Guid Uid
        {
            get => (Guid) MDV["Uid"].Value;
            set => MDV["Uid"].Value = value;
        }

        public string StripePricePerMonthId
        {
            get => (string) MDV["StripePricePerMonthId"].Value;
            set => MDV["StripePricePerMonthId"].Value = value;
        }

        public string StripePricePerYearId
        {
            get => (string) MDV["StripePricePerYearId"].Value;
            set => MDV["StripePricePerYearId"].Value = value;
        }


        public Package()
        {
            Config(ObjectMap, DatabaseQuery, typeof(Package).Name, "ID");
            LoadObjectValues(ObjectMap);
            IsActive = false;
            IsPopular = false;
            IsAfterSchool = false;
            Uid = Guid.NewGuid();
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                var _dp = new DatabaseParameters();
                var className = typeof(Package).Name;
                _dp = get_BaseObjectMap(className);
                _dp.Add(className, "Price", SqlDbType.Money);
                _dp.Add(className, "PricePerYear", SqlDbType.Money);
                _dp.Add(className, "IsActive", SqlDbType.Bit);
                _dp.Add(className, "IsPopular", SqlDbType.Bit);
                _dp.Add(className, "IsAfterSchool", SqlDbType.Bit);
                _dp.Add(className, "Uid", SqlDbType.UniqueIdentifier);
                _dp.Add(className, "StripePricePerMonthId", SqlDbType.NVarChar, "50");
                _dp.Add(className, "StripePricePerYearId", SqlDbType.NVarChar, "50");

                return _dp;
            }
        }

        public static Package LoadObject(DataRow row)
        {
            var obj = new Package();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, typeof(Package).Name);

                return dq;
            }
        }

        public static Package Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Package> Load(Hashtable criterias = null, int? start = null, int? length = null,
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

            if (Price.IsEmpty())
                msgs.Add("Price is required on Package object", MessageType.Error);
            if (PricePerYear.IsEmpty())
                msgs.Add("PricePerYear is required on Package object", MessageType.Error);
            if (Name.IsEmpty())
                msgs.Add("Name is required on Package object", MessageType.Error);

            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Package).Name));

            return msgs;
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Package).Name);
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
            dps.Add(ObjectMap["Price"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["PricePerYear"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsActive"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsPopular"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsAfterSchool"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["Uid"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["StripePricePerMonthId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["StripePricePerYearId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));


            return msgs;
        }

        public Package Reload(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Load(ID.Value, msgs, dc);
        }

        public static List<Package> LoadByIsActive(bool IsActive, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Package> LoadByIsActiveByIsAfterSchool(bool IsActive, bool IsAfterSchool,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
            dq.Criterias.Add(ObjectMap["IsAfterSchool"], IsAfterSchool);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Package> LoadByIsPopular(bool IsPopular, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["IsPopular"], IsPopular);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Package> LoadByIsPopularByIsAfterSchool(bool IsPopular, bool IsAfterSchool,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["IsPopular"], IsPopular);
            dq.Criterias.Add(ObjectMap["IsAfterSchool"], IsAfterSchool);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static Package LoadByUid(Guid Uid, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Uid"], Uid);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        #endregion

        #region "UserDefined"

        public bool HasFeature(FeatureType featureType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetFeatures(msgs, dc).Any(x => x.FeatureTypeId == featureType && x.Max.HasValue && x.Max > 0);
        }

        public PackageFeature GetFeature(FeatureType featureType, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return GetFeatures(msgs, dc).FirstOrDefault(x => x.FeatureTypeId == featureType);
        }

        public List<PackageFeature> GetFeatures(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return PackageFeature.LoadByPackageId(ID.Value, msgs, dc);
        }

        public SystemMessages SaveWithStripe(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            Price monthlyPrice = null;
            Price yearlyPrice = null;

            var stripeProduct = AddOrUpdateStripeProduct(msgs);

            if (msgs.HasNoErrors() && stripeProduct.IsNotEmpty())
            {
                monthlyPrice = AddOrUpdateStripePrice(
                    stripeProduct.Id,
                    StripePricePerMonthId,
                    Price.Value,
                    Name,
                    BillingPeriodType.Month,
                    msgs);

                yearlyPrice = AddOrUpdateStripePrice(
                    stripeProduct.Id,
                    StripePricePerYearId,
                    PricePerYear.Value,
                    Name,
                    BillingPeriodType.Year,
                    msgs);
            }

            if (msgs.HasNoErrors())
            {
                // always want to save the price ids because updating the id creates a new one.
                if (monthlyPrice.IsNotEmpty())
                    StripePricePerMonthId = monthlyPrice.Id;

                if (yearlyPrice.IsNotEmpty())
                    StripePricePerYearId = yearlyPrice.Id;

                if (monthlyPrice.IsNotEmpty() || yearlyPrice.IsNotEmpty())
                    Save(dc);
            }

            return msgs;
        }

        private Stripe.Product AddOrUpdateStripeProduct(SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var product = StripeHelper.GetProduct(GetStripeProductId(), null, msgs);

            if (product.IsEmpty())
                product = StripeHelper.CreateProduct(
                    GetStripeProductId(),
                    GetStripeProductName(),
                    $"{Utilities.AppName(IsAfterSchool)} - {Name}".MaxChars(22),
                    true,
                    null,
                    null,
                    msgs);
            else
                product = StripeHelper.UpdateProduct(
                    GetStripeProductId(),
                    GetStripeProductName(),
                    $"{Utilities.AppName(IsAfterSchool)} - {Name}".MaxChars(22),
                    true,
                    null,
                    msgs);

            return product;
        }

        private Price AddOrUpdateStripePrice(string productId, string priceId, decimal amount, string nickname,
            BillingPeriodType billingPeriodType, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Price price = null;

            if (priceId.IsNotEmpty())
                price = StripeHelper.GetPrice(priceId, msgs);

            if (price.IsEmpty())
                price = StripeHelper.CreatePrice(
                    productId,
                    amount * 100,
                    nickname,
                    billingPeriodType,
                    null,
                    new Dictionary<string, string>
                    {
                        {"billingPeriodTypeId", ((int) billingPeriodType).ToString()},
                    },
                    msgs);

            return price;
        }

        private Plan AddOrUpdateStripePlan(Stripe.Product product, decimal price, BillingPeriodType billingPeriodType,
            SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var plan = StripeHelper.GetPlan(GetStripePlanId(billingPeriodType), null, msgs);

            if (plan.IsEmpty())
            {
                plan = StripeHelper.CreatePlan(
                    product,
                    GetStripePlanId(billingPeriodType),
                    price,
                    billingPeriodType,
                    GetStripePlanName(billingPeriodType),
                    null,
                    null,
                    msgs);
            }

            return plan;
        }

        public string GetStripePlanId(BillingPeriodType billingPeriodType)
        {
            return Uid + (billingPeriodType == BillingPeriodType.Year
                ? $"-{billingPeriodType.ToString().ToLower()}ly"
                : "");
        }

        public string GetStripePlanName(BillingPeriodType billingPeriodType)
        {
            return $"{Name} - {billingPeriodType.ToString()}ly";
        }

        public string GetStripeProductId()
        {
            return Uid.ToString();
        }

        public string GetStripeProductName()
        {
            return $"{Name}";
        }

        public bool IsAnyAccountUsing(DatabaseConnection dc = null)
        {
            if (ID.HasValue)
                return AccountPackage.GetCount(
                    new Hashtable
                    {
                        {"PackageId", ID.Value},
                    },
                    dc: dc
                ) > 0;

            return false;
        }

        public bool IsFree(BillingPeriodType billingPeriodType)
        {
            if (billingPeriodType == BillingPeriodType.Month)
                return Price.Value == 0.ToDecimal();

            return PricePerYear.Value == 0.ToDecimal();
        }

        public void CopyFeaturesToAccountPackage(AccountPackage ap, int loggedInUserId, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            GetFeatures(dc: dc).ForEach(pf => ap.AddPackageFeature(pf, new WebUser {ID = loggedInUserId}, msgs, dc));
        }

        #endregion
    }
}
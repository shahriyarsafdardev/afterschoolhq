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
    /// This is an actual package that an account/organization can purchase when signing up. There are multiple packages that are able to be purchased at a time. A package containts multiple Features.
    /// </summary>
	
    public class Package : BasicLookUpObject
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public decimal? Price { get { return (decimal?)MDV["Price"].Value; } set { MDV["Price"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public decimal? PricePerYear { get { return (decimal?)MDV["PricePerYear"].Value; } set { MDV["PricePerYear"].Value = value; } }
		public bool IsActive { get { return (bool)MDV["IsActive"].Value; } set { MDV["IsActive"].Value = value; } }
		public bool IsPopular { get { return (bool)MDV["IsPopular"].Value; } set { MDV["IsPopular"].Value = value; } }
		public bool IsAfterSchool { get { return (bool)MDV["IsAfterSchool"].Value; } set { MDV["IsAfterSchool"].Value = value; } }
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public string StripePricePerMonthId { get { return (string)MDV["StripePricePerMonthId"].Value; } set { MDV["StripePricePerMonthId"].Value = value; } }
		public string StripePricePerYearId { get { return (string)MDV["StripePricePerYearId"].Value; } set { MDV["StripePricePerYearId"].Value = value; } }
		

        public Package()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Package).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.IsActive = false;
			this.IsPopular = false;
			this.IsAfterSchool = false;
			this.Uid = Guid.NewGuid();
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(Package).Name;
                _dp = BasicLookUpObject.get_BaseObjectMap(className);
				_dp.Add(className, "Price", System.Data.SqlDbType.Money);
				_dp.Add(className, "PricePerYear", System.Data.SqlDbType.Money);
				_dp.Add(className, "IsActive", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsPopular", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsAfterSchool", System.Data.SqlDbType.Bit);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "StripePricePerMonthId", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "StripePricePerYearId", System.Data.SqlDbType.NVarChar , "50");
				
                return _dp;
            }
        }

        public static Package LoadObject(System.Data.DataRow row)
        {
            Package obj = new Package();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(Package).Name);
				
                return dq;
            }
        }

        public static Package Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Package> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return Package.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return Package.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.Price.IsEmpty())
				msgs.Add("Price is required on Package object", MessageType.Error);
			if (this.PricePerYear.IsEmpty())
				msgs.Add("PricePerYear is required on Package object", MessageType.Error);
			if (this.Name.IsEmpty())
				msgs.Add("Name is required on Package object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Package).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Package).Name);
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
			dps.Add(ObjectMap["Price"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PricePerYear"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsActive"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsPopular"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsAfterSchool"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StripePricePerMonthId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StripePricePerYearId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Package Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Package.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<Package> LoadByIsActive(bool IsActive, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Package> LoadByIsActiveByIsAfterSchool(bool IsActive, bool IsAfterSchool, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
			dq.Criterias.Add(ObjectMap["IsAfterSchool"], IsAfterSchool);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Package> LoadByIsPopular(bool IsPopular, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["IsPopular"], IsPopular);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Package> LoadByIsPopularByIsAfterSchool(bool IsPopular, bool IsAfterSchool, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["IsPopular"], IsPopular);
			dq.Criterias.Add(ObjectMap["IsAfterSchool"], IsAfterSchool);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Package LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
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
            return this.GetFeatures(msgs, dc).Where(x => x.FeatureTypeId == featureType && x.Max.HasValue && x.Max > 0).Any();
        }

        public PackageFeature GetFeature(FeatureType featureType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return this.GetFeatures(msgs, dc).Where(x => x.FeatureTypeId == featureType).FirstOrDefault();
        }

        public List<PackageFeature> GetFeatures(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return PackageFeature.LoadByPackageId(this.ID.Value, msgs, dc);
        }
        
        public SystemMessages SaveWithStripe(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            Stripe.Product stripeProduct = null;
            Stripe.Price monthlyPrice = null;
            Stripe.Price yearlyPrice = null;

            stripeProduct = AddOrUpdateStripeProduct(msgs);

            if (msgs.HasNoErrors() && stripeProduct.IsNotEmpty())
            {                
                monthlyPrice = AddOrUpdateStripePrice(
                    stripeProduct.Id,
                    this.StripePricePerMonthId,
                    this.Price.Value,
                    this.Name,
                    BillingPeriodType.Month,
                    msgs);

                yearlyPrice = AddOrUpdateStripePrice(
                    stripeProduct.Id,
                    this.StripePricePerYearId,
                    this.PricePerYear.Value,
                    this.Name,
                    BillingPeriodType.Year,
                    msgs);
            }

            if(msgs.HasNoErrors())
            {
                // always want to save the price ids because updating the id creates a new one.
                if (monthlyPrice.IsNotEmpty())
                    this.StripePricePerMonthId = monthlyPrice.Id;

                if (yearlyPrice.IsNotEmpty())
                    this.StripePricePerYearId = yearlyPrice.Id;

                if (monthlyPrice.IsNotEmpty() || yearlyPrice.IsNotEmpty())
                    this.Save(dc);
            }

            return msgs;
        }

        private Stripe.Product AddOrUpdateStripeProduct(SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            
            var product = Billing.StripeHelper.GetProduct(this.GetStripeProductId(), null, msgs);

            if (product.IsEmpty())
            {
                product = Billing.StripeHelper.CreateProduct(
                    this.GetStripeProductId(),
                    this.GetStripeProductName(),
                    $"{Helpers.Utilities.AppName(this.IsAfterSchool)} - {this.Name}".MaxChars(22),
                    true,
                    null,
                    null,
                    msgs);
            }
            else
            {
                product = Billing.StripeHelper.UpdateProduct(
                    this.GetStripeProductId(),
                    this.GetStripeProductName(),
                    $"{Helpers.Utilities.AppName(this.IsAfterSchool)} - {this.Name}".MaxChars(22),
                    true,
                    null,
                    msgs);
            }

            return product;
        }

        private Stripe.Price AddOrUpdateStripePrice(string productId, string priceId, decimal amount, string nickname, BillingPeriodType billingPeriodType, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.Price price = null;

            if (priceId.IsNotEmpty())
                price = Billing.StripeHelper.GetPrice(priceId, msgs);

            if (price.IsEmpty())
            {
                price = Billing.StripeHelper.CreatePrice(
                     productId,
                     amount * 100,
                     nickname,
                     billingPeriodType,
                     null,
                     new Dictionary<string, string>()
                         {
                            { "billingPeriodTypeId", ((int)billingPeriodType).ToString() }
                         },
                     msgs);
            }

            return price;

        }

        private Stripe.Plan AddOrUpdateStripePlan(Stripe.Product product, decimal price, BillingPeriodType billingPeriodType, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            
            var plan = Billing.StripeHelper.GetPlan(this.GetStripePlanId(billingPeriodType), null, msgs);

            if (plan.IsEmpty())
            {
                plan = Billing.StripeHelper.CreatePlan(
                    product,
                    this.GetStripePlanId(billingPeriodType),
                    price,
                    billingPeriodType,
                    this.GetStripePlanName(billingPeriodType),
                    null,
                    null,
                    msgs);
            }
            else
            {
                // TODO: Need to update anything?
            }

            return plan;
        }

        public string GetStripePlanId(BillingPeriodType billingPeriodType)
        {
            return this.Uid.ToString() + (billingPeriodType == BillingPeriodType.Year ? $"-{billingPeriodType.ToString().ToLower()}ly" : "");
        }

        public string GetStripePlanName(BillingPeriodType billingPeriodType)
        {
            return $"{this.Name.ToString()} - {billingPeriodType.ToString()}ly";
        }

        public string GetStripeProductId()
        {
            return this.Uid.ToString();
        }

        public string GetStripeProductName()
        {
            return $"{this.Name.ToString()}";
        }

        public bool IsAnyAccountUsing(DatabaseConnection dc = null)
        {
            if (this.ID.HasValue)
                return AccountPackage.GetCount(
                    criterias: new Hashtable()
                    {
                        {"PackageId", this.ID.Value}
                    },
                    dc: dc
                ) > 0;

            return false;
        }

        public bool IsFree(BillingPeriodType billingPeriodType)
        {
            if (billingPeriodType == BillingPeriodType.Month)
                return this.Price.Value == (0).ToDecimal();

            return this.PricePerYear.Value == (0).ToDecimal();
        }

        public void CopyFeaturesToAccountPackage(AccountPackage ap, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {            
            this.GetFeatures(dc: dc).ForEach(pf => ap.AddPackageFeature(pf, new WebUser() { ID = loggedInUserId }, msgs, dc));
        }

        #endregion
    }
}

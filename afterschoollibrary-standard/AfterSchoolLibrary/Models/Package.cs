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
    /// This is an actual package that an account/organization can purchase when signing up. There are multiple packages that are able to be purchased at a time. A package containts multiple Features.
    /// </summary>
	
    public class Package : BasicLookUpObject
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required
		/// </summary>
		public decimal? Price { get { return (decimal?)MDV["Price"].Value; } set { MDV["Price"].Value = value; } }
		/// <summary>
		/// Required
		/// </summary>
		public decimal? PricePerYear { get { return (decimal?)MDV["PricePerYear"].Value; } set { MDV["PricePerYear"].Value = value; } }
		public bool IsActive { get { return (bool)MDV["IsActive"].Value; } set { MDV["IsActive"].Value = value; } }
		public bool IsPopular { get { return (bool)MDV["IsPopular"].Value; } set { MDV["IsPopular"].Value = value; } }
		public bool IsAfterSchool { get { return (bool)MDV["IsAfterSchool"].Value; } set { MDV["IsAfterSchool"].Value = value; } }
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		

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
				
                return _dp;
            }
        }

        protected static Package LoadObject(System.Data.DataRow row)
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

        public bool HasFeature(FeatureType featureType, DatabaseConnection dc = null)
        {
            return this.GetFeatures(dc: dc).Where(x => x.FeatureTypeId == featureType && x.Max.HasValue && x.Max > 0).Any();
        }

        public PackageFeature GetFeature(FeatureType featureType, DatabaseConnection dc = null)
        {
            return this.GetFeatures(dc: dc).Where(x => x.FeatureTypeId == featureType).FirstOrDefault();
        }

        public void ResetFeatureList()
        {
            this._Features = null;
        }

        private List<PackageFeature> _Features;
        public List<PackageFeature> GetFeatures(DatabaseConnection dc = null)
        {
            if (_Features.IsEmpty())
                _Features = PackageFeature.LoadByPackageId(this.ID.Value, dc: dc);

            return _Features;
        }

        public SystemMessages SaveWithStripe(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(this.Save(dc: dc));

            //add to stripe plan, should do on every save
            if (msgs.HasNoErrors())
            {
                var planService = new Stripe.StripePlanService(StripeHelper.SecretKey);
                var requestOptions = new Stripe.StripeRequestOptions();
                requestOptions.ApiKey = StripeHelper.SecretKey;

                msgs.AddRange(AddOrUpdateStripePlan(planService, requestOptions, BillingPeriodType.Month, this.Price.Value));
                msgs.AddRange(AddOrUpdateStripePlan(planService, requestOptions, BillingPeriodType.Year, this.PricePerYear.Value));
            }

            return msgs;
        }

        private SystemMessages AddOrUpdateStripeProduct(Stripe.StripeProductService productService, Stripe.StripeRequestOptions requestOptions, BillingPeriodType billingPeriodType)
        {
            var msgs = new SystemMessages();

            try
            {
                Stripe.StripeProduct product = null;

                try
                {
                    product = productService.Get("", requestOptions);
                }
                catch (Stripe.StripeException ex)
                {
                    //no product found
                }

                if (product.IsEmpty())
                {
                    var options = new Stripe.StripeProductCreateOptions();
                    options.Id = this.GetStripePlanId(billingPeriodType);
                    options.Name = this.GetStripePlanName(billingPeriodType);
                    options.StatementDescriptor = $"{Utilities.AppName(false)} - {this.Name}".MaxChars(22);
                    options.Shippable = false;
                    options.Active = true;

                    productService = new Stripe.StripeProductService(StripeHelper.SecretKey);
                    productService.Create(options, requestOptions);
                }
                else
                {
                    var options = new Stripe.StripeProductUpdateOptions();
                    options.Name = this.GetStripePlanName(billingPeriodType);
                    options.StatementDescriptor = $"{Utilities.AppName(false)} - {this.Name}".MaxChars(22);
                    productService.Update(product.Id, options, requestOptions);
                }
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
            }

            return msgs;
        }

        private SystemMessages AddOrUpdateStripePlan(Stripe.StripePlanService planService, Stripe.StripeRequestOptions requestOptions, BillingPeriodType billingPeriodType, decimal price)
        {
            var msgs = new SystemMessages();
            try
            {
                var product = GetStripeProduct(requestOptions, billingPeriodType);
                Stripe.StripePlan plan = null;

                try
                {
                    plan = planService.Get(this.GetStripePlanId(billingPeriodType), requestOptions);
                }
                catch (Stripe.StripeException ex)
                {
                    //no plan found
                }

                if (plan.IsEmpty())
                {
                    var options = new Stripe.StripePlanCreateOptions();
                    options.Id = this.GetStripePlanId(billingPeriodType);
                    options.Amount = Convert.ToInt32(price * 100);
                    options.Currency = "usd";
                    options.IntervalCount = 1;
                    options.Interval = billingPeriodType.ToString().ToLower();
                    options.Nickname = this.GetStripePlanName(billingPeriodType);
                    options.ProductId = product.Id;

                    planService = new Stripe.StripePlanService(StripeHelper.SecretKey);
                    planService.Create(options, requestOptions);
                }
                else
                {
                    var options = new Stripe.StripePlanUpdateOptions();
                    // TODO: does the product need to be updated?
                    planService.Update(plan.Id, options, requestOptions);
                }
            }
            catch(Exception ex)
            {
                msgs.Add(ex.Message, MessageType.Error, ex);
            }
            
            return msgs;
        }

        public Stripe.StripeProduct GetStripeProduct(Stripe.StripeRequestOptions requestOptions, BillingPeriodType billingPeriodType)
        {
            var productService = new Stripe.StripeProductService(StripeHelper.SecretKey);
            Stripe.StripeProduct product = null;

            try
            {
                product = productService.Get(GetStripePlanId(billingPeriodType), requestOptions);
            }
            catch(Exception ex)
            {
                // no existing plan/product
            }

            if (product.IsEmpty())
            {
                AddOrUpdateStripeProduct(productService, requestOptions, billingPeriodType);
                product = productService.Get(GetStripePlanId(billingPeriodType), requestOptions);
            }
            return product;
        }

        public string GetStripePlanId(BillingPeriodType billingPeriodType)
        {
            return this.ID.ToString() + (billingPeriodType == BillingPeriodType.Year ? $" - {billingPeriodType.ToString()}ly" : "");
        }

        public string GetStripePlanName(BillingPeriodType billingPeriodType)
        {
            return $"{this.Name.ToString()} - {billingPeriodType.ToString()}ly";
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

        public SystemMessages CopyFeaturesToAccountPackage(AccountPackage ap, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var apfs = ap.GetFeatures(dc: dc);
            
            this.GetFeatures(dc: dc).ForEach(pf => msgs.AddRange(ap.AddPackageFeature(pf, loggedInUserId, dc: dc)));
            ap.ResetFeatureList();

            return msgs;
        }

        #endregion
    }
}

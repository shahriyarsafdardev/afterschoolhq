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
    /// Used to give discounts to parents during the purchase of a registration, product, etc
    /// </summary>
	
    public class Coupon : BasicLookUpObject
    {
		#region "ClassGenerator"

		public string Description { get { return (string)MDV["Description"].Value; } set { MDV["Description"].Value = value; } }
		/// <summary>
		/// Required.
		/// Store the amount a percentage or flat amount. If percentage it should be stored as a decimal meaning 100% should be stored as 1.00
		/// </summary>
		public decimal? Amount { get { return (decimal?)MDV["Amount"].Value; } set { MDV["Amount"].Value = value; } }
		/// <summary>
		/// Required.
		/// Either a percentage or a flat amount
		/// </summary>
		public CouponAmountType AmountTypeId { get { return (CouponAmountType)MDV["AmountTypeId"].Value; } set { MDV["AmountTypeId"].Value = value; } }
		/// <summary>
		/// Required.
		/// the account that is the owner
		/// </summary>
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public DateTime? ExpirationDate { get { return (DateTime?)MDV["ExpirationDate"].Value; } set { MDV["ExpirationDate"].Value = value; } }
		/// <summary>
		/// Determines what type of object the coupon will apply to during a purchase
		/// </summary>
		public CouponApplyType AppliesTo { get { return (CouponApplyType)MDV["AppliesTo"].Value; } set { MDV["AppliesTo"].Value = value; } }
		public int? MaxNumberOfUsesPerPerson { get { return (int?)MDV["MaxNumberOfUsesPerPerson"].Value; } set { MDV["MaxNumberOfUsesPerPerson"].Value = value; } }
		/// <summary>
		/// The amount of uses that a coupon can have across all users
		/// </summary>
		public int? MaxNumberOfUsesAll { get { return (int?)MDV["MaxNumberOfUsesAll"].Value; } set { MDV["MaxNumberOfUsesAll"].Value = value; } }
		public string CouponCode { get { return (string)MDV["CouponCode"].Value; } set { MDV["CouponCode"].Value = value; } }
		public string StripeCouponId { get { return (string)MDV["StripeCouponId"].Value; } set { MDV["StripeCouponId"].Value = value; } }
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public bool IsDisabled { get { return (bool)MDV["IsDisabled"].Value; } set { MDV["IsDisabled"].Value = value; } }
		

        public Coupon()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Coupon).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.AmountTypeId = CouponAmountType.NotSet;
			this.AppliesTo = CouponApplyType.NotSet;
			this.Uid = Guid.NewGuid();
			this.IsDisabled = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(Coupon).Name;
                _dp = BasicLookUpObject.get_BaseObjectMap(className);
				_dp.Add(className, "Description", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "Amount", System.Data.SqlDbType.Money);
				_dp.Add(className, "AmountTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ExpirationDate", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "AppliesTo", System.Data.SqlDbType.Int);
				_dp.Add(className, "MaxNumberOfUsesPerPerson", System.Data.SqlDbType.Int);
				_dp.Add(className, "MaxNumberOfUsesAll", System.Data.SqlDbType.Int);
				_dp.Add(className, "CouponCode", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "StripeCouponId", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "IsDisabled", System.Data.SqlDbType.Bit);
				
                return _dp;
            }
        }

        public static Coupon LoadObject(System.Data.DataRow row)
        {
            Coupon obj = new Coupon();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(Coupon).Name);
				
                return dq;
            }
        }

        public static Coupon Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Coupon> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return Coupon.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return Coupon.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.Amount.IsEmpty())
				msgs.Add("Amount is required on Coupon object", MessageType.Error);
			if (this.AmountTypeId.IsEmpty())
				msgs.Add("AmountTypeId is required on Coupon object", MessageType.Error);
			if (this.AccountId.IsEmpty())
				msgs.Add("AccountId is required on Coupon object", MessageType.Error);
			if (this.Name.IsEmpty())
				msgs.Add("Name is required on Coupon object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Coupon).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Coupon).Name);
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
			dps.Add(ObjectMap["Description"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Amount"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AmountTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ExpirationDate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AppliesTo"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["MaxNumberOfUsesPerPerson"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["MaxNumberOfUsesAll"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["CouponCode"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StripeCouponId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsDisabled"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Coupon Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Coupon.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<Coupon> LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Coupon LoadByAccountIdByCouponCode(int AccountId, string CouponCode, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["CouponCode"], CouponCode);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static Coupon LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static Coupon LoadByUidByAccountId(Guid Uid, int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

		public static List<Coupon> LoadByAccountId(int AccountId,string search, DateTime? startDate, DateTime? endDate, List<(int ID, ParticipationObjectType ObjectType)> particiPationObjects, bool isDeleted, int? start, int? length,  SystemMessages msgs, DatabaseConnection dc)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		#endregion
    }
}

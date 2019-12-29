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
    /// Used to limit coupons to particlar objects
    /// </summary>
	
    public class PurchaseableObjectCoupon : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? PurchaseableObjectId { get { return (int?)MDV["PurchaseableObjectId"].Value; } set { MDV["PurchaseableObjectId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public PurchaseableObjectType PurchaseableObjectTypeId { get { return (PurchaseableObjectType)MDV["PurchaseableObjectTypeId"].Value; } set { MDV["PurchaseableObjectTypeId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? CouponId { get { return (int?)MDV["CouponId"].Value; } set { MDV["CouponId"].Value = value; } }
		

        public PurchaseableObjectCoupon()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(PurchaseableObjectCoupon).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.PurchaseableObjectTypeId = PurchaseableObjectType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(PurchaseableObjectCoupon).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "PurchaseableObjectId", System.Data.SqlDbType.Int);
				_dp.Add(className, "PurchaseableObjectTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "CouponId", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        protected static PurchaseableObjectCoupon LoadObject(System.Data.DataRow row)
        {
            PurchaseableObjectCoupon obj = new PurchaseableObjectCoupon();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(PurchaseableObjectCoupon).Name);
				
                return dq;
            }
        }

        public static PurchaseableObjectCoupon Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<PurchaseableObjectCoupon> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return PurchaseableObjectCoupon.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return PurchaseableObjectCoupon.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.PurchaseableObjectId.IsEmpty())
				msgs.Add("PurchaseableObjectId is required on PurchaseableObjectCoupon object", MessageType.Error);
			if (this.PurchaseableObjectTypeId.IsEmpty())
				msgs.Add("PurchaseableObjectTypeId is required on PurchaseableObjectCoupon object", MessageType.Error);
			if (this.CouponId.IsEmpty())
				msgs.Add("CouponId is required on PurchaseableObjectCoupon object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(PurchaseableObjectCoupon).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(PurchaseableObjectCoupon).Name);
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
			dps.Add(ObjectMap["PurchaseableObjectId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PurchaseableObjectTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["CouponId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public PurchaseableObjectCoupon Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return PurchaseableObjectCoupon.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static PurchaseableObjectCoupon LoadByCouponByPurchaseableObject(int CouponId, int PurchaseableObjectId, PurchaseableObjectType PurchaseableObjectTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["CouponId"], CouponId);
			dq.Criterias.Add(ObjectMap["PurchaseableObjectId"], PurchaseableObjectId);
			dq.Criterias.Add(ObjectMap["PurchaseableObjectTypeId"], PurchaseableObjectTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<PurchaseableObjectCoupon> LoadByCouponId(int CouponId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["CouponId"], CouponId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<PurchaseableObjectCoupon> LoadByPurchaseableObject(int PurchaseableObjectId, PurchaseableObjectType PurchaseableObjectTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["PurchaseableObjectId"], PurchaseableObjectId);
			dq.Criterias.Add(ObjectMap["PurchaseableObjectTypeId"], PurchaseableObjectTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        #endregion
    }
}

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
    /// Used to associate an account product to a activity. A product offered to buy via a participation object (activity, activity group)
    /// </summary>
	[Serializable]
    public class ProductOffering : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public ParticipationSourceType ParticipationSourceTypeId { get { return (ParticipationSourceType)MDV["ParticipationSourceTypeId"].Value; } set { MDV["ParticipationSourceTypeId"].Value = value; } }
		public int? ParticipationSourceId { get { return (int?)MDV["ParticipationSourceId"].Value; } set { MDV["ParticipationSourceId"].Value = value; } }
		public int? ProductId { get { return (int?)MDV["ProductId"].Value; } set { MDV["ProductId"].Value = value; } }
		

        public ProductOffering()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(ProductOffering).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.ParticipationSourceTypeId = ParticipationSourceType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(ProductOffering).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "ParticipationSourceTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ParticipationSourceId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ProductId", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        protected static ProductOffering LoadObject(System.Data.DataRow row)
        {
            ProductOffering obj = new ProductOffering();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ProductOffering).Name);
				
                return dq;
            }
        }

        public static ProductOffering Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ProductOffering> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return ProductOffering.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return ProductOffering.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.ParticipationSourceTypeId.IsEmpty())
				msgs.Add("ParticipationSourceTypeId is required on ProductOffering object", MessageType.Error);
			if (this.ParticipationSourceId.IsEmpty())
				msgs.Add("ParticipationSourceId is required on ProductOffering object", MessageType.Error);
			if (this.ProductId.IsEmpty())
				msgs.Add("ProductId is required on ProductOffering object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ProductOffering).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ProductOffering).Name);
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
			dps.Add(ObjectMap["ParticipationSourceTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ParticipationSourceId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ProductId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public ProductOffering Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return ProductOffering.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<ProductOffering> LoadByParticipationSource(int ParticipationSourceId, ParticipationSourceType ParticipationSourceTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ParticipationSourceId"], ParticipationSourceId);
			dq.Criterias.Add(ObjectMap["ParticipationSourceTypeId"], ParticipationSourceTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ProductOffering> LoadByProductId(int ProductId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ProductId"], ProductId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static ProductOffering LoadByParticipationSourceIdByParticipationSourceTypeIdByProductId(int ParticipationSourceId, ParticipationSourceType ParticipationSourceTypeId, int ProductId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ParticipationSourceId"], ParticipationSourceId);
			dq.Criterias.Add(ObjectMap["ParticipationSourceTypeId"], ParticipationSourceTypeId);
			dq.Criterias.Add(ObjectMap["ProductId"], ProductId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        #endregion
    }
}

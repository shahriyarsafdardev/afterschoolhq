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
    /// This is how a feature is added specifically to one account. This allows us to modify features just for one account and not others.
    /// </summary>
	[Serializable]
    public class AccountPackageFeature : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? AccountPackageId { get { return (int?)MDV["AccountPackageId"].Value; } set { MDV["AccountPackageId"].Value = value; } }
		public FeatureType FeatureTypeId { get { return (FeatureType)MDV["FeatureTypeId"].Value; } set { MDV["FeatureTypeId"].Value = value; } }
		public decimal? Max { get { return (decimal?)MDV["Max"].Value; } set { MDV["Max"].Value = value; } }
		

        public AccountPackageFeature()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(AccountPackageFeature).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.FeatureTypeId = FeatureType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(AccountPackageFeature).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "AccountPackageId", System.Data.SqlDbType.Int);
				_dp.Add(className, "FeatureTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Max", System.Data.SqlDbType.Decimal , "10,2");
				
                return _dp;
            }
        }

        public static AccountPackageFeature LoadObject(System.Data.DataRow row)
        {
            AccountPackageFeature obj = new AccountPackageFeature();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(AccountPackageFeature).Name);
				
                return dq;
            }
        }

        public static AccountPackageFeature Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<AccountPackageFeature> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return AccountPackageFeature.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return AccountPackageFeature.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.AccountPackageId.IsEmpty())
				msgs.Add("AccountPackageId is required on AccountPackageFeature object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(AccountPackageFeature).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(AccountPackageFeature).Name);
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
			dps.Add(ObjectMap["AccountPackageId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["FeatureTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Max"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public AccountPackageFeature Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return AccountPackageFeature.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<AccountPackageFeature> LoadByAccountPackageId(int AccountPackageId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountPackageId"], AccountPackageId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static AccountPackageFeature LoadByAccountPackageIdByFeatureTypeId(int AccountPackageId, FeatureType FeatureTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountPackageId"], AccountPackageId);
			dq.Criterias.Add(ObjectMap["FeatureTypeId"], FeatureTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public static bool HasAccountPackageByFeatureTypeId(int AccountPackageId, FeatureType FeatureTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountPackageId"], AccountPackageId);
            dq.Criterias.Add(ObjectMap["FeatureTypeId"], FeatureTypeId);
            dq.Criterias.Add(ObjectMap["Max"], 0, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanParameter);
            return dq.GetCount(msgs) > 0;
        }

        public static bool HasAccountPackageByFeatureTypeIds(int AccountPackageId, List<FeatureType> FeatureTypeIds, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountPackageId"], AccountPackageId);
            dq.Criterias.Add(ObjectMap["FeatureTypeId"], FeatureTypeIds.Select(x=> (int)x).ToList(), DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["Max"], 0, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanParameter);
            return dq.GetCount(msgs) == FeatureTypeIds.Count;
        }

        public static List<AccountPackageFeature> LoadByFeatureTypeId(FeatureType FeatureTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FeatureTypeId"], (int)FeatureTypeId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<AccountPackageFeature> LoadByFeatureTypeIdByMaxValue(FeatureType FeatureTypeId, int Max, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FeatureTypeId"], (int)FeatureTypeId);
            dq.Criterias.Add(ObjectMap["Max"], Max, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanParameter);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public decimal GetMaxValue()
        {
            return this.Max.HasValue ? this.Max.Value : 0;
        }

        #endregion
    }
}

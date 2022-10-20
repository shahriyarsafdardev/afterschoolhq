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
    public class FeatureFlag : BaseObject
    {
        public string FlagName { get { return (string)MDV["FlagName"].Value; } set { MDV["FlagName"].Value = value; } }
        public bool IsEnabled { get { return (bool)MDV["IsEnabled"].Value; } set { MDV["IsEnabled"].Value = value; } }
        
        public FeatureFlag()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(FeatureFlag).Name);
            this.LoadObjectValues(ObjectMap);
			
        }
        
        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(FeatureFlag).Name;
                _dp.Add(className, "FlagName", System.Data.SqlDbType.NVarChar , "100");
                _dp.Add(className, "IsEnabled", System.Data.SqlDbType.Bit);
				
                return _dp;
            }
        }

        public static FeatureFlag LoadObject(System.Data.DataRow row)
        {
            FeatureFlag obj = new FeatureFlag();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }
        
        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(FeatureFlag).Name);
				
                return dq;
            }
        }
        
        public static bool CheckFlagIsEnabled(string FlagName, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["FlagName"], FlagName);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault().IsEnabled;
        }
        
    }
}
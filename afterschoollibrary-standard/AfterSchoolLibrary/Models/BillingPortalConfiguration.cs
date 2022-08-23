using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Data;
using System.Collections;
using AfterSchoolLibrary.Enums;
using Amazon.DynamoDBv2.Model;

namespace AfterSchoolLibrary.Models
{
    public class BillingPortalConfiguration: BasicObject
    {
        public string StripeId
        {
            get => (string) MDV["StripeId"].Value;
            set => MDV["StripeId"].Value = value;
        }
        public BillingPortalConfigurationType BillingPortalConfigurationTypeId
        {
            get => (BillingPortalConfigurationType) MDV["BillingPortalConfigurationTypeId"].Value;
            set => MDV["BillingPortalConfigurationTypeId"].Value = value;
        }
        
        public BillingPortalConfiguration()
        {
            this.Config(ObjectMap, DatabaseQuery, nameof(BillingPortalConfiguration), "ID");
            this.LoadObjectValues(ObjectMap);
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = nameof(BillingPortalConfiguration);
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "StripeId", System.Data.SqlDbType.NVarChar, "50");
				_dp.Add(className, "BillingPortalConfigurationTypeId", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        public static BillingPortalConfiguration LoadObject(System.Data.DataRow row)
        {
            var obj = new BillingPortalConfiguration();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, nameof(BillingPortalConfiguration));
				
                return dq;
            }
        }

        public static BillingPortalConfiguration Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<BillingPortalConfiguration> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return BillingPortalConfiguration.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return BillingPortalConfiguration.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.StripeId.IsEmpty())
				msgs.Add("StripeId is required on BillingPortalConfiguration object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, nameof(BillingPortalConfiguration)));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, nameof(BillingPortalConfiguration));
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
			dps.Add(ObjectMap["StripeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["BillingPortalConfigurationTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));
			

            return msgs;
        }

		public BillingPortalConfiguration Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return BillingPortalConfiguration.Load(this.ID.Value, msgs, dc: dc);
		}

        public static BillingPortalConfiguration LoadByBillingPortalConfigurationId(int StripeId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["StripeId"], StripeId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static BillingPortalConfiguration LoadByBillingPortalConfigurationTypeId(BillingPortalConfigurationType BillingPortalConfigurationTypeId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["BillingPortalConfigurationTypeId"], (int)BillingPortalConfigurationTypeId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }
    }
}
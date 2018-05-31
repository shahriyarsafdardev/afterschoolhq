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
	
    public class RegistrationActivity : BasicObject
    {
		#region "ClassGenerator"

		public int? RegistrationId { get { return (int?)MDV["RegistrationId"].Value; } set { MDV["RegistrationId"].Value = value; } }
		public int? ActivityId { get { return (int?)MDV["ActivityId"].Value; } set { MDV["ActivityId"].Value = value; } }
		public string ActivityName { get { return (string)MDV["ActivityName"].Value; } }
		public Guid ActivityUid { get { return (Guid)MDV["ActivityUid"].Value; } }
		public int? ActivityAccountId { get { return (int?)MDV["ActivityAccountId"].Value; } }
		public decimal? ActivityCostPerRegistrant { get { return (decimal?)MDV["ActivityCostPerRegistrant"].Value; } }
		public string ActivityConfirmationMessage { get { return (string)MDV["ActivityConfirmationMessage"].Value; } }
		public bool ActivityIsArchived { get { return (bool)MDV["ActivityIsArchived"].Value; } }
		public bool ActivityGroupIsArchived { get { return (bool)MDV["ActivityGroupIsArchived"].Value; } }
		

        public RegistrationActivity()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(RegistrationActivity).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(RegistrationActivity).Name;
                _dp = BasicObject.get_BaseObjectMap(className);
				_dp.Add(className, "RegistrationId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityName", typeof(Activity).Name, "Name", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivityUid", typeof(Activity).Name, "Uid", System.Data.SqlDbType.UniqueIdentifier, "Activity");
				_dp.Add(className, "ActivityAccountId", typeof(Activity).Name, "AccountId", System.Data.SqlDbType.Int, "Activity");
				_dp.Add(className, "ActivityCostPerRegistrant", typeof(Activity).Name, "CostPerRegistrant", System.Data.SqlDbType.Decimal, "Activity");
				_dp.Add(className, "ActivityConfirmationMessage", typeof(Activity).Name, "ConfirmationMessage", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivityIsArchived", typeof(Activity).Name, "IsArchived", System.Data.SqlDbType.Bit, "Activity");
				_dp.Add(className, "ActivityGroupIsArchived", typeof(ActivityGroup).Name, "IsArchived", System.Data.SqlDbType.Bit, "ActivityGroup");
				
                return _dp;
            }
        }

        protected static RegistrationActivity LoadObject(System.Data.DataRow row)
        {
            RegistrationActivity obj = new RegistrationActivity();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(RegistrationActivity).Name);
				dq.Joins.Add(Activity.ObjectMap["ID"], RegistrationActivity.ObjectMap["ActivityId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Activity");
				dq.Joins.Add(ActivityGroup.ObjectMap["ID"], Activity.ObjectMap["ID"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "ActivityGroup");
				
                return dq;
            }
        }

        public static RegistrationActivity Load(int id, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<RegistrationActivity> Load(Hashtable criterias = null, int? start = null, int? length = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return RegistrationActivity.BaseLoad(criterias, ObjectMap, dq).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return RegistrationActivity.BaseGetCount(criterias, ObjectMap, dq);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Save2(DatabaseQuery, dc, ObjectMap, typeof(RegistrationActivity).Name);
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(RegistrationActivity).Name);
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
			dps.Add(ObjectMap["RegistrationId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public RegistrationActivity Reload(BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return RegistrationActivity.Load(this.ID.Value, dc);
		}
		
		public static RegistrationActivity LoadByActivityIdByRegistrationId(int ActivityId, int RegistrationId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			dq.Criterias.Add(ObjectMap["RegistrationId"], RegistrationId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<RegistrationActivity> LoadByActivityId(int ActivityId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<RegistrationActivity> LoadByRegistrationId(int RegistrationId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrationId"], RegistrationId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        public static List<RegistrationActivity> LoadByAccountId(int AccountId, bool? isArchived, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityAccountId"], AccountId);
            if (isArchived.HasValue)
            {
                var dcs = new DatabaseCriterias(LogicalOperatorType.Or);
                dcs.Add(ObjectMap["ActivityIsArchived"], isArchived.Value);
                dcs.Add(ObjectMap["ActivityGroupIsArchived"], isArchived.Value);
                dq.Criterias.Add(dcs);
            }
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        #endregion
    }
}

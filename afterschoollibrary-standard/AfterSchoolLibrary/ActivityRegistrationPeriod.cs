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
	
    public class ActivityRegistrationPeriod : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public int? RegistrationPeriodId { get { return (int?)MDV["RegistrationPeriodId"].Value; } set { MDV["RegistrationPeriodId"].Value = value; } }
		public int? ActivityId { get { return (int?)MDV["ActivityId"].Value; } set { MDV["ActivityId"].Value = value; } }
		public bool IsActive { get { return (bool)MDV["IsActive"].Value; } set { MDV["IsActive"].Value = value; } }
		public string RegistrationPeriodName { get { return (string)MDV["RegistrationPeriodName"].Value; } }
		public DateTime? RegistrationPeriodStartDateTime { get { return (DateTime?)MDV["RegistrationPeriodStartDateTime"].Value; } }
		public DateTime? RegistrationPeriodEndDateTime { get { return (DateTime?)MDV["RegistrationPeriodEndDateTime"].Value; } }
		public string RegistrationPeriodDescription { get { return (string)MDV["RegistrationPeriodDescription"].Value; } }
		public RegistrationMode RegistrationPeriodRegistrationModeId { get { return (RegistrationMode)MDV["RegistrationPeriodRegistrationModeId"].Value; } }
		public int? ActivityIsDeleted { get { return (int?)MDV["ActivityIsDeleted"].Value; } }
		

        public ActivityRegistrationPeriod()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(ActivityRegistrationPeriod).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.IsActive = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(ActivityRegistrationPeriod).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "RegistrationPeriodId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsActive", System.Data.SqlDbType.Bit);
				_dp.Add(className, "RegistrationPeriodName", typeof(RegistrationPeriod).Name, "Name", System.Data.SqlDbType.NVarChar, "RegistrationPeriod");
				_dp.Add(className, "RegistrationPeriodStartDateTime", typeof(RegistrationPeriod).Name, "StartDateTime", System.Data.SqlDbType.DateTime, "RegistrationPeriod");
				_dp.Add(className, "RegistrationPeriodEndDateTime", typeof(RegistrationPeriod).Name, "EndDateTime", System.Data.SqlDbType.DateTime, "RegistrationPeriod");
				_dp.Add(className, "RegistrationPeriodDescription", typeof(RegistrationPeriod).Name, "Description", System.Data.SqlDbType.NVarChar, "RegistrationPeriod");
				_dp.Add(className, "RegistrationPeriodRegistrationModeId", typeof(RegistrationPeriod).Name, "RegistrationModeId", System.Data.SqlDbType.Int, "RegistrationPeriod");
				_dp.Add(className, "ActivityIsDeleted", typeof(Activity).Name, "IsDeleted", System.Data.SqlDbType.Int, "Activity");
				
                return _dp;
            }
        }

        protected static ActivityRegistrationPeriod LoadObject(System.Data.DataRow row)
        {
            ActivityRegistrationPeriod obj = new ActivityRegistrationPeriod();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivityRegistrationPeriod).Name);
				dq.Joins.Add(RegistrationPeriod.ObjectMap["ID"], ActivityRegistrationPeriod.ObjectMap["RegistrationPeriodId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "RegistrationPeriod");
				dq.Joins.Add(Activity.ObjectMap["ID"], ActivityRegistrationPeriod.ObjectMap["ActivityId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Activity");
				
                return dq;
            }
        }

        public static ActivityRegistrationPeriod Load(int id, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ActivityRegistrationPeriod> Load(Hashtable criterias = null, int? start = null, int? length = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return ActivityRegistrationPeriod.BaseLoad(criterias, ObjectMap, dq).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return ActivityRegistrationPeriod.BaseGetCount(criterias, ObjectMap, dq);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ActivityRegistrationPeriod).Name);
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ActivityRegistrationPeriod).Name);
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
			dps.Add(ObjectMap["RegistrationPeriodId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsActive"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public ActivityRegistrationPeriod Reload(BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return ActivityRegistrationPeriod.Load(this.ID.Value, dc);
		}
		
		public static List<ActivityRegistrationPeriod> LoadByRegistrationPeriodId(int RegistrationPeriodId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrationPeriodId"], RegistrationPeriodId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityRegistrationPeriod> LoadByActivityId(int ActivityId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static ActivityRegistrationPeriod LoadByActivityIdByIsActive(int ActivityId, bool IsActive, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static ActivityRegistrationPeriod LoadByRegistrationPeriodIdByActivityId(int RegistrationPeriodId, int ActivityId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrationPeriodId"], RegistrationPeriodId);
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"
        
        public RegistrationPeriod ToRegistrationPeriod(bool loadFromDb = false, DatabaseConnection dc = null)
        {
            if (loadFromDb)
            {
                return RegistrationPeriod.Load(this.RegistrationPeriodId.Value, dc);
            }
            else
            {
                return new RegistrationPeriod()
                {
                    ID = this.RegistrationPeriodId,
                    StartDateTime = this.RegistrationPeriodStartDateTime,
                    EndDateTime = this.RegistrationPeriodEndDateTime,
                    Description = this.RegistrationPeriodDescription,
                    Name = this.RegistrationPeriodName,
                    RegistrationModeId = this.RegistrationPeriodRegistrationModeId
                };
            }
        }

        public Activity ToActivity(bool loadFromDb = false, DatabaseConnection dc = null)
        {
            if (loadFromDb)
            {
                return Activity.Load(this.ActivityId.Value, dc);
            }
            else
            {
                return new Activity()
                {
                    ID = this.ActivityId,
                    StartDateTime = this.RegistrationPeriodStartDateTime,
                    EndDateTime = this.RegistrationPeriodEndDateTime,
                    Description = this.RegistrationPeriodDescription,
                    Name = this.RegistrationPeriodName
                };
            }
        }

        public RegistrationPeriod GetRegistrationPeriod(DatabaseConnection dc = null)
        {
            return RegistrationPeriod.Load(this.RegistrationPeriodId.Value, dc);
        }

        public Activity GetActivity(DatabaseConnection dc = null)
        {
            return Activity.Load(this.ActivityId.Value, dc);
        }

        #endregion
    }
}

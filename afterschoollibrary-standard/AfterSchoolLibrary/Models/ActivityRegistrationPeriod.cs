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
    /// Used to store all registration periods that are used for an activity. An activity can only have one active registration period at a time
    /// </summary>
	
    public class ActivityRegistrationPeriod : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? RegistrationPeriodId { get { return (int?)MDV["RegistrationPeriodId"].Value; } set { MDV["RegistrationPeriodId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
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

        public static ActivityRegistrationPeriod LoadObject(System.Data.DataRow row)
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

        public static ActivityRegistrationPeriod Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ActivityRegistrationPeriod> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return ActivityRegistrationPeriod.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return ActivityRegistrationPeriod.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.RegistrationPeriodId.IsEmpty())
				msgs.Add("RegistrationPeriodId is required on ActivityRegistrationPeriod object", MessageType.Error);
			if (this.ActivityId.IsEmpty())
				msgs.Add("ActivityId is required on ActivityRegistrationPeriod object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ActivityRegistrationPeriod).Name));

			return msgs;
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

		public ActivityRegistrationPeriod Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return ActivityRegistrationPeriod.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<ActivityRegistrationPeriod> LoadByRegistrationPeriodId(int RegistrationPeriodId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrationPeriodId"], RegistrationPeriodId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityRegistrationPeriod> LoadByActivityId(int ActivityId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityRegistrationPeriod> LoadByActivityIdByIsActive(int ActivityId, bool IsActive, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityRegistrationPeriod> LoadByRegistrationPeriodIdByActivityId(int RegistrationPeriodId, int ActivityId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrationPeriodId"], RegistrationPeriodId);
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"
        
        public RegistrationPeriod ToRegistrationPeriod(bool loadFromDb = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (loadFromDb)
            {
                return RegistrationPeriod.Load(this.RegistrationPeriodId.Value, msgs, dc: dc);
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
                return Activity.Load(this.ActivityId.Value, dc: dc);
            }
            else
            {
                return new Activity()
                {
                    ID = this.ActivityId,
                    Description = this.RegistrationPeriodDescription,
                    Name = this.RegistrationPeriodName
                };
            }
        }

        public RegistrationPeriod GetRegistrationPeriod(DatabaseConnection dc = null)
        {
            return RegistrationPeriod.Load(this.RegistrationPeriodId.Value, dc: dc);
        }

        public Activity GetActivity(SystemMessages msgs, DatabaseConnection dc = null)
        {
            return Activity.Load(this.ActivityId.Value, msgs, dc);
        }

        #endregion
    }
}

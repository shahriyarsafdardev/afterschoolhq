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
    /// This is the record of an activity sponsor/teacher/etc taking attendance for an activity instance or basically an actual class.
    /// </summary>
	
    public class Attendance : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? ActivityId { get { return (int?)MDV["ActivityId"].Value; } set { MDV["ActivityId"].Value = value; } }
		public int? ActivityInstanceId { get { return (int?)MDV["ActivityInstanceId"].Value; } set { MDV["ActivityInstanceId"].Value = value; } }
		public DateTime? Date { get { return (DateTime?)MDV["Date"].Value; } set { MDV["Date"].Value = value; } }
		public bool IsCompleted { get { return (bool)MDV["IsCompleted"].Value; } set { MDV["IsCompleted"].Value = value; } }
		public bool IsCharged { get { return (bool)MDV["IsCharged"].Value; } set { MDV["IsCharged"].Value = value; } }
		public string ActivityName { get { return (string)MDV["ActivityName"].Value; } }
		public Guid ActivityUid { get { return (Guid)MDV["ActivityUid"].Value; } }
		public int? ActivityAccountId { get { return (int?)MDV["ActivityAccountId"].Value; } }
		public string AccountName { get { return (string)MDV["AccountName"].Value; } }
		

        public Attendance()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Attendance).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.IsCompleted = false;
			this.IsCharged = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(Attendance).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "ActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityInstanceId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Date", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "IsCompleted", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsCharged", System.Data.SqlDbType.Bit);
				_dp.Add(className, "ActivityName", typeof(Activity).Name, "Name", System.Data.SqlDbType.NVarChar, "Activity");
				_dp.Add(className, "ActivityUid", typeof(Activity).Name, "Uid", System.Data.SqlDbType.UniqueIdentifier, "Activity");
				_dp.Add(className, "ActivityAccountId", typeof(Activity).Name, "AccountId", System.Data.SqlDbType.Int, "Activity");
				_dp.Add(className, "AccountName", typeof(Account).Name, "Name", System.Data.SqlDbType.NVarChar, "Account");
				
                return _dp;
            }
        }

        public static Attendance LoadObject(System.Data.DataRow row)
        {
            Attendance obj = new Attendance();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(Attendance).Name);
				dq.Joins.Add(Activity.ObjectMap["ID"], Attendance.ObjectMap["ActivityId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Activity");
				dq.Joins.Add(Account.ObjectMap["ID"], Activity.ObjectMap["AccountId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Account");
				
                return dq;
            }
        }

        public static Attendance Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Attendance> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return Attendance.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return Attendance.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.ActivityId.IsEmpty())
				msgs.Add("ActivityId is required on Attendance object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Attendance).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Attendance).Name);
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
			dps.Add(ObjectMap["ActivityId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityInstanceId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Date"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsCompleted"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsCharged"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Attendance Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Attendance.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<Attendance> LoadByActivityId(int ActivityId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Attendance> LoadByActivityIdByDate(int ActivityId, DateTime Date, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			dq.Criterias.Add(ObjectMap["Date"], Date);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Attendance> LoadByActivityIdByIsCompleted(int ActivityId, bool IsCompleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			dq.Criterias.Add(ObjectMap["IsCompleted"], IsCompleted);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Attendance> LoadByActivityInstanceId(int ActivityInstanceId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityInstanceId"], ActivityInstanceId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        public Activity ToActivity(bool loadFromDb = false, DatabaseConnection dc = null)
        {
            if (loadFromDb)
                return Activity.Load(this.ActivityId.Value, dc: dc);
            
            return new Activity()
            {
                ID = ActivityId,
                Name = ActivityName,
                AccountId = ActivityAccountId,
                Uid = ActivityUid
            };
        }

        public static List<Attendance> LoadByAccountIdByDate(int AccountId, DateTime Date, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityAccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["Date"], Date);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Attendance> LoadByAccountIdByIsCompleted(int AccountId, bool IsCompleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityAccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["IsCompleted"], IsCompleted);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCountByAccountIdByIsCompleted(int AccountId, bool IsCompleted, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityAccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["IsCompleted"], IsCompleted);
            return dq.GetCount(msgs);
        }

        public static List<Attendance> LoadByActivityId(int activityId, string orderbyParam, OrderByDirectionEnum direction, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            dq.OrderBys.Add(orderbyParam, direction);

            var criterias = new Hashtable() { { "ActivityId", activityId } };

            return Attendance.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Attendance> LoadByActivityIdByDateRange(int ActivityId, DateTime StartDate, DateTime EndDate, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
            dq.Criterias.Add(ObjectMap["Date"], StartDate, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);
            dq.Criterias.Add(ObjectMap["Date"], EndDate, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }


        #endregion
    }
}

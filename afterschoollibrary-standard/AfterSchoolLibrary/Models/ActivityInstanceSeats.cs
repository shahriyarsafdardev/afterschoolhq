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
    /// Store number of seats per actual date of an activity useful for events and tutoring
    /// </summary>
	
    public class ActivityInstanceSeats : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? RegistrantActivityId { get { return (int?)MDV["RegistrantActivityId"].Value; } set { MDV["RegistrantActivityId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? ActivityInstanceId { get { return (int?)MDV["ActivityInstanceId"].Value; } set { MDV["ActivityInstanceId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? NumberOfSeats { get { return (int?)MDV["NumberOfSeats"].Value; } set { MDV["NumberOfSeats"].Value = value; } }
		

        public ActivityInstanceSeats()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(ActivityInstanceSeats).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(ActivityInstanceSeats).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "RegistrantActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityInstanceId", System.Data.SqlDbType.Int);
				_dp.Add(className, "NumberOfSeats", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        public static ActivityInstanceSeats LoadObject(System.Data.DataRow row)
        {
            ActivityInstanceSeats obj = new ActivityInstanceSeats();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivityInstanceSeats).Name);
				
                return dq;
            }
        }

        public static ActivityInstanceSeats Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ActivityInstanceSeats> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return ActivityInstanceSeats.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return ActivityInstanceSeats.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.RegistrantActivityId.IsEmpty())
				msgs.Add("RegistrantActivityId is required on ActivityInstanceSeats object", MessageType.Error);
			if (this.ActivityInstanceId.IsEmpty())
				msgs.Add("ActivityInstanceId is required on ActivityInstanceSeats object", MessageType.Error);
			if (this.NumberOfSeats.IsEmpty())
				msgs.Add("NumberOfSeats is required on ActivityInstanceSeats object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ActivityInstanceSeats).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ActivityInstanceSeats).Name);
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
			dps.Add(ObjectMap["RegistrantActivityId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityInstanceId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["NumberOfSeats"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public ActivityInstanceSeats Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return ActivityInstanceSeats.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<ActivityInstanceSeats> LoadByRegistrantActivityId(int RegistrantActivityId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantActivityId"], RegistrantActivityId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityInstanceSeats> LoadByActivityInstanceId(int ActivityInstanceId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityInstanceId"], ActivityInstanceId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static ActivityInstanceSeats LoadByRegistrantActivityIdByActivityInstanceId(int RegistrantActivityId, int ActivityInstanceId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["RegistrantActivityId"], RegistrantActivityId);
			dq.Criterias.Add(ObjectMap["ActivityInstanceId"], ActivityInstanceId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

		public static int GetSeatsTakenByActivityInstanceId(int ActivityInstanceId, SystemMessages msgs, DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;

            dq.SelectParameters.Clear();
            dq.SelectParameters.Add(ObjectMap["NumberOfSeats"], "SumOfSeats", $"ISNULL(SUM({DatabaseQuery.FunctionReplaceValue}), 0)");

            dq.Criterias.Add(ObjectMap["ActivityInstanceId"], ActivityInstanceId);
            //dq.Criterias.Add(RegistrantActivity.ObjectMap["AssignmentTypeId"], Registration.ReservedStatuses);
            //dq.Joins.Add(RegistrantActivity.ObjectMap["ID"], ObjectMap["RegistrantActivityId"], DatabaseJoinTypeEnum.InnerJoin);

            var dt = dq.Select(msgs);

			if (dt.Rows.Count > 0)
				return dt.Rows[0][0].ToString().ToInteger();

			return 0;
		}

		#endregion
    }
}

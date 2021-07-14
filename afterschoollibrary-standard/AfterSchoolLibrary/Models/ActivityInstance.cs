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
    /// An actual date that an activity is occurring. So this would be 1 class at a specific date and time
    /// </summary>
	
    public class ActivityInstance : BasicObjectWithModified
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? ActivityScheduleId { get { return (int?)MDV["ActivityScheduleId"].Value; } set { MDV["ActivityScheduleId"].Value = value; } }
		public int? ActivityId { get { return (int?)MDV["ActivityId"].Value; } set { MDV["ActivityId"].Value = value; } }
		public int? ActivityGroupId { get { return (int?)MDV["ActivityGroupId"].Value; } set { MDV["ActivityGroupId"].Value = value; } }
		public DayOfWeekASL Day { get { return (DayOfWeekASL)MDV["Day"].Value; } set { MDV["Day"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public DateTime? StartDateTime { get { return (DateTime?)MDV["StartDateTime"].Value; } set { MDV["StartDateTime"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public DateTime? EndDateTime { get { return (DateTime?)MDV["EndDateTime"].Value; } set { MDV["EndDateTime"].Value = value; } }
		public InstanceType InstanceTypeId { get { return (InstanceType)MDV["InstanceTypeId"].Value; } set { MDV["InstanceTypeId"].Value = value; } }
		public int? LocationId { get { return (int?)MDV["LocationId"].Value; } set { MDV["LocationId"].Value = value; } }
		

        public ActivityInstance()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(ActivityInstance).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.Day = DayOfWeekASL.NotSet;
			this.InstanceTypeId = InstanceType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(ActivityInstance).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "ActivityScheduleId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityGroupId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Day", System.Data.SqlDbType.Int);
				_dp.Add(className, "StartDateTime", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "EndDateTime", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "InstanceTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "LocationId", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        public static ActivityInstance LoadObject(System.Data.DataRow row)
        {
            ActivityInstance obj = new ActivityInstance();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivityInstance).Name);
				
                return dq;
            }
        }

        public static ActivityInstance Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ActivityInstance> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return ActivityInstance.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return ActivityInstance.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.ActivityScheduleId.IsEmpty())
				msgs.Add("ActivityScheduleId is required on ActivityInstance object", MessageType.Error);
			if (this.StartDateTime.IsEmpty())
				msgs.Add("StartDateTime is required on ActivityInstance object", MessageType.Error);
			if (this.EndDateTime.IsEmpty())
				msgs.Add("EndDateTime is required on ActivityInstance object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ActivityInstance).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ActivityInstance).Name);
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
			dps.Add(ObjectMap["ActivityScheduleId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityGroupId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Day"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StartDateTime"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["EndDateTime"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["InstanceTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LocationId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public ActivityInstance Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return ActivityInstance.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<ActivityInstance> LoadByActivityId(int ActivityId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityId"], ActivityId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityInstance> LoadByActivityGroupId(int ActivityGroupId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityInstance> LoadByActivityScheduleId(int ActivityScheduleId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityScheduleId"], ActivityScheduleId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityInstance> LoadByActivityScheduleIdByInstanceTypeId(int ActivityScheduleId, InstanceType InstanceTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityScheduleId"], ActivityScheduleId);
			dq.Criterias.Add(ObjectMap["InstanceTypeId"], InstanceTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        public DateRange GetDateRange()
        {
            if (StartDateTime.HasValue && this.EndDateTime.HasValue)
                return new DateRange(this.StartDateTime.Value, this.EndDateTime.Value);

            return null;
        }

        //public static List<DateRange> CreateDateRangesFromSchedule(ActivitySchedule schedule, SystemMessages msgs, DatabaseConnection dc = null)
        //{
        //    var ranges = new List<DateRange>();

        //    if (schedule.IsNotEmpty())
        //    {
        //        foreach (var day in schedule.GetDaysOfTheWeek(msgs, dc))
        //        {
        //            var currentDate = schedule.StartDate.Value.AddWeeks(0).NextDayOfWeek(day.ToDayOfWeek());

        //            while (currentDate <= schedule.StartDate.Value)
        //            {
        //                ranges.Add(new DateRange()
        //                {
        //                    StartDate = $"{currentDate.ToShortDateString()} {schedule.DailyStartTime.Value}".ToDateTime(),
        //                    EndDate = $"{currentDate.ToShortDateString()} {schedule.DailyEndTime.Value}".ToDateTime(),
        //                });
        //                currentDate = currentDate.AddWeeks(1);
        //            }
        //        }
        //    }

        //    return ranges;
        //}

        

        public List<Match> GetMatches(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (this.ID.IsNotEmpty())
                return Match.LoadByActivityInstanceId(this.ID.Value, msgs, dc);
           
            return new List<Match>(); 
        }

        public List<Activity> GetTeams(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return this.GetMatches(msgs, dc)
                .Select(match => Activity.Load(match.TeamId.Value, msgs, dc))
                .ToList();
        }

		public static List<ActivityInstance> LoadByActivityScheduleIdByDateRange(int ActivityScheduleId, DateTime StartDateTime, DateTime EndDateTime, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;

			var sql = $@"
				SELECT * FROM ActivityInstance WHERE ActivityScheduleId = {ActivityScheduleId} AND StartDateTime >= '{StartDateTime}' AND EndDateTime <= '{EndDateTime}'
			";

			return dq.ExecuteSelect(sql, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}


		#endregion
    }
}

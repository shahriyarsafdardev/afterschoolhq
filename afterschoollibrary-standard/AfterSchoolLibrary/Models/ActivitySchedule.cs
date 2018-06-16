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
	
    public class ActivitySchedule : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public string Name { get { return (string)MDV["Name"].Value; } set { MDV["Name"].Value = value; } }
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public int? ActivityGroupId { get { return (int?)MDV["ActivityGroupId"].Value; } set { MDV["ActivityGroupId"].Value = value; } }
		public string ScheduleDetails { get { return (string)MDV["ScheduleDetails"].Value; } set { MDV["ScheduleDetails"].Value = value; } }
		public bool RepeatsWeekly { get { return (bool)MDV["RepeatsWeekly"].Value; } set { MDV["RepeatsWeekly"].Value = value; } }
		public DateTime? StartDate { get { return (DateTime?)MDV["StartDate"].Value; } set { MDV["StartDate"].Value = value; } }
		public DateTime? EndDate { get { return (DateTime?)MDV["EndDate"].Value; } set { MDV["EndDate"].Value = value; } }
		public DateTime? DailyStartTime { get { return (DateTime?)MDV["DailyStartTime"].Value; } set { MDV["DailyStartTime"].Value = value; } }
		public DateTime? DailyEndTime { get { return (DateTime?)MDV["DailyEndTime"].Value; } set { MDV["DailyEndTime"].Value = value; } }
		

        public ActivitySchedule()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(ActivitySchedule).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.RepeatsWeekly = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(ActivitySchedule).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "Name", System.Data.SqlDbType.NVarChar, 100);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityGroupId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ScheduleDetails", System.Data.SqlDbType.NVarChar, 4000);
				_dp.Add(className, "RepeatsWeekly", System.Data.SqlDbType.Bit);
				_dp.Add(className, "StartDate", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "EndDate", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "DailyStartTime", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "DailyEndTime", System.Data.SqlDbType.DateTime);
				
                return _dp;
            }
        }

        protected static ActivitySchedule LoadObject(System.Data.DataRow row)
        {
            ActivitySchedule obj = new ActivitySchedule();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivitySchedule).Name);
				
                return dq;
            }
        }

        public static ActivitySchedule Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ActivitySchedule> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return ActivitySchedule.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return ActivitySchedule.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.AccountId.IsEmpty())
				msgs.Add("AccountId is required on ActivitySchedule object", MessageType.Error);
			if (this.StartDate.IsEmpty())
				msgs.Add("StartDate is required on ActivitySchedule object", MessageType.Error);
			if (this.EndDate.IsEmpty())
				msgs.Add("EndDate is required on ActivitySchedule object", MessageType.Error);
			if (this.DailyStartTime.IsEmpty())
				msgs.Add("DailyStartTime is required on ActivitySchedule object", MessageType.Error);
			if (this.DailyEndTime.IsEmpty())
				msgs.Add("DailyEndTime is required on ActivitySchedule object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ActivitySchedule).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ActivitySchedule).Name);
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
			dps.Add(ObjectMap["Name"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityGroupId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ScheduleDetails"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RepeatsWeekly"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StartDate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["EndDate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DailyStartTime"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DailyEndTime"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public ActivitySchedule Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return ActivitySchedule.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<ActivitySchedule> LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivitySchedule> LoadByActivityGroupId(int ActivityGroupId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        private List<ActivityInstance> _instances;
        public List<ActivityInstance> GetInstances(bool forceReload = false, DatabaseConnection dc = null)
        {
            if (this.ID.HasValue && (_instances.IsEmpty() || forceReload))
                _instances = ActivityInstance.LoadByActivityScheduleId(this.ID.Value, dc: dc)
                    .OrderBy(x => x.StartDateTime)
                    .ToList();

            return _instances;
        }
        
        public static ActivitySchedule LoadByAccountIdByName(int AccountId, string Name, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["Name"], Name);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static bool ExistsByAccountIdByName(int AccountId, string Name, int existingId, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["Name"], Name);
            dq.Criterias.Add(ObjectMap["ID"], existingId, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.NotEqualTo);
            return dq.GetCount() > 0;
        }

        public static string GenerateUniqueName(int accountId, string name, DatabaseConnection dc = null)
        {
            var rtnName = name;
            var period = ActivitySchedule.LoadByAccountIdByName(accountId, name, dc: dc);
            var counter = 1;

            while (period.IsNotEmpty())
            {
                counter++;
                rtnName = $"{name} {counter.ToString()}";
                period = ActivitySchedule.LoadByAccountIdByName(accountId, rtnName, dc: dc);
            }

            return rtnName;
        }

        public static ActivitySchedule EditExisting(string scheduleId, string scheduleName, string details, bool repeatsWeekly, DateTime? startDate, DateTime? endDate, DateTime? dailyStartTime, DateTime? dailyEndTime, string scheduleDates, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            ActivitySchedule rtn = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) => {

                    var schedule = ActivitySchedule.Load(scheduleId.ToInteger(), dc: adc);
                    schedule.Name = scheduleName;
                    schedule.ScheduleDetails = details.IfBlank("").MaxChars(4000);
                    schedule.RepeatsWeekly = repeatsWeekly;
                    schedule.StartDate = startDate;
                    schedule.EndDate = endDate;
                    schedule.DailyStartTime = dailyStartTime;
                    schedule.DailyEndTime = dailyEndTime;
                    schedule.LastModifiedDate = DateTime.UtcNow;
                    schedule.LastModifiedBy = loggedInUserId;
                    amsgs.AddRange(schedule.Save(adc));

                    amsgs.AddRange(schedule.UpdateInstances(scheduleDates, loggedInUserId, adc));

                    rtn = schedule;
                },
                onSuccess: (SystemMessages smsgs) => {
                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages fmsgs) => {
                    msgs.AddRange(fmsgs);
                },
                dc: dc
            );

            return rtn;
        }

        public static ActivitySchedule CreateNew(string scheduleName, string details, bool repeatsWeekly, DateTime? startDate, DateTime? endDate, DateTime? dailyStartTime, DateTime? dailyEndTime, string scheduleDates, int accountId, int loggedInUserId, int? group, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            ActivitySchedule rtn = null;

            Database.ExecuteTransaction(
               action: (SystemMessages actionMsgs, DatabaseConnection adc) => 
               {
                   var schedule = new ActivitySchedule();
                   schedule.Name = scheduleName;
                   schedule.ScheduleDetails = details.IfBlank("").MaxChars(4000);
                   schedule.AccountId = accountId;
                   schedule.RepeatsWeekly = repeatsWeekly;
                   schedule.StartDate = startDate;
                   schedule.EndDate = endDate;
                   schedule.DailyStartTime = dailyStartTime;
                   schedule.DailyEndTime = dailyEndTime;
                   schedule.CreateDate = schedule.LastModifiedDate = DateTime.UtcNow;
                   schedule.CreatedBy = schedule.LastModifiedBy = loggedInUserId;
                   schedule.ActivityGroupId = group;
                   actionMsgs.AddRange(schedule.Save(adc));

                   actionMsgs.AddRange(schedule.UpdateInstances(scheduleDates, loggedInUserId, adc));

                   rtn = schedule;
               },
               onSuccess: (SystemMessages successMsgs) => 
               {
                   msgs.AddRange(successMsgs);
               },
               onFailure: (SystemMessages failureMsgs) => 
               {
                   msgs.AddRange(failureMsgs);
               },
               dc: dc
           );

           return rtn;
        }

        public static ActivitySchedule GetNewDefaultSchedule(List<string> scheduleDates = null)
        {
            var schedule = new ActivitySchedule();
            schedule.StartDate = DateTime.UtcNow;
            schedule.EndDate = DateTime.UtcNow.AddWeeks(1);
            schedule.DailyStartTime = DateTime.UtcNow.Date.AddHours(15);
            schedule.DailyEndTime = DateTime.UtcNow.Date.AddHours(17);
            schedule.RepeatsWeekly = true;

            if (scheduleDates.IsNotEmpty())
                schedule.AddInstances(
                    scheduleDates.Select(x =>
                        new ActivityInstance()
                        {
                            StartDateTime = x.Split('-')[0].ToDateTime(),
                            EndDateTime = x.Split('-')[1].ToDateTime()
                        }).ToList());

            return schedule;
        }

        // TODO: Save? what about instances that get added but aren't saved?
        private void AddInstances(List<ActivityInstance> list)
        {
            _instances = new List<ActivityInstance>();
            _instances.AddRange(list);
        }

        public static ActivitySchedule LoadWithInstances(int ActivityScheduleId, DatabaseConnection dc = null)
        {
            var schedule = ActivitySchedule.Load(ActivityScheduleId, dc: dc);
            schedule?.GetInstances(false, dc: dc);
            return schedule;
        }

        public ActivityInstance AddInstance(DateTime startDateTime, DateTime endDateTime, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var instance = new ActivityInstance();
            instance.ActivityScheduleId = this.ID;
            instance.StartDateTime = startDateTime;
            instance.EndDateTime = endDateTime;
            instance.CreateDate = instance.LastModifiedDate = DateTime.UtcNow;
            instance.CreatedBy = instance.LastModifiedBy = loggedInUserId;
            instance.Day = instance.StartDateTime.Value.DayOfWeek.ToDayOfWeekASL();
            msgs.AddRange(instance.Save(dc: dc));

            return instance;
        }

        public SystemMessages UpdateScheduleAndInstances(bool repeatsWeekly, DateTime? startDate, DateTime? endDate, DateTime? dailyStartTime, DateTime? dailyEndTime, string scheduleDates, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            this.RepeatsWeekly = repeatsWeekly;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.DailyStartTime = dailyStartTime;
            this.DailyEndTime = dailyEndTime;
            this.CreateDate = this.LastModifiedDate = DateTime.UtcNow;
            this.CreatedBy = this.LastModifiedBy = loggedInUserId;
            msgs.AddRange(this.Save(dc: dc));

            msgs.AddRange(UpdateInstances(scheduleDates, loggedInUserId, dc: dc));

            return msgs;
        }

        public SystemMessages UpdateInstances(string scheduleDates, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var scheduleList = scheduleDates.Split(',').Select(x => x.Split('-')).ToList();
            var existingInstances = this.GetInstances(false, dc: dc);

            // delete instances
            existingInstances
                .Where(x => !scheduleList
                    .Where(y => y[0].ToDateTime() == x.StartDateTime.Value && y[1].ToDateTime() == x.EndDateTime.Value)
                    .Any()
                 )
                .ToList()
                .ForEach(instance => msgs.AddRange(instance.Delete(dc: dc)));

            // add new instances
            scheduleList
                .Where(x => 
                    !existingInstances
                        .Where(y => y.StartDateTime.Value == x[0].ToDateTime() && y.EndDateTime.Value == x[1].ToDateTime())
                        .Any())
                .ToList()
                .ForEach(item => this.AddInstance(item[0].ToDateTime(), item[1].ToDateTime(), loggedInUserId, msgs, dc: dc));            

            msgs.AddRange(this.ValidateInstances(dc: dc));

            return msgs;
        }

        public SystemMessages ValidateInstances(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            // get all instances currently saved in the transaction
            var updatedInstances = this.GetInstances(forceReload: true, dc: dc);

            // check for dates where end time is less than start time
            if (updatedInstances.Where(x => !x.GetDateRange().IsValid()).Any())
                msgs.Add("You have dates where the end time is earlier than or equal to the start time", MessageType.UserError);
            
            // check for overlapping times 
            if (msgs.HasNoErrors() && DateRange.HasOverlappingDates(updatedInstances.Select(x => x.GetDateRange())))
                msgs.Add("You have dates that overlap", MessageType.UserError);

            return msgs;
        }

        public List<DayOfWeekASL> GetDaysOfTheWeek(DatabaseConnection dc = null)
        {
            if (this.GetInstances(false, dc: dc).IsNotEmpty())
                return this.GetInstances(false, dc: dc).Select(x => x.Day).Distinct().ToList();

            return new List<DayOfWeekASL>();
        }

        public ActivitySchedule GetCopy()
        {
            var schedule = new ActivitySchedule();
            schedule.Name = this.Name + " [copy]";
            schedule.ScheduleDetails = this.ScheduleDetails;
            schedule.StartDate = this.StartDate;
            schedule.EndDate = this.EndDate;
            schedule.DailyStartTime = this.DailyStartTime;
            schedule.DailyEndTime = this.DailyEndTime;
            schedule.RepeatsWeekly = this.RepeatsWeekly;

            schedule.AddInstances(this.GetInstances());

            return schedule;
        }

        public ActivitySchedule CopyAndSave(int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            ActivitySchedule copy = null;

            if(msgs.IsEmpty()) msgs = new SystemMessages();

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    copy = new ActivitySchedule
                    {
                        AccountId = this.AccountId,
                        DailyEndTime = this.DailyEndTime,
                        DailyStartTime = this.DailyStartTime,
                        EndDate = this.EndDate,
                        StartDate = this.StartDate,
                        Name = this.Name + " [copy]",
                        RepeatsWeekly = this.RepeatsWeekly,
                        ScheduleDetails = this.ScheduleDetails,
                        CreateDate = DateTime.UtcNow,
                        CreatedBy = loggedInUserId,
                        LastModifiedDate = DateTime.UtcNow,
                        LastModifiedBy = loggedInUserId
                    };
                    amsgs.AddRange(copy.Save(adc));

                    // add instances
                    this.GetInstances(dc: adc)
                        .ToList()
                        .ForEach(instance => copy.AddInstance(instance.StartDateTime.Value, instance.EndDateTime.Value, loggedInUserId, amsgs, adc));
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                },
                dc: dc
            );

            return copy;

        }

        public List<Activity> GetActivities(DatabaseConnection dc = null)
        {
            return Activity.LoadByActivityScheduleId(this.ID.Value, dc: dc);
        }

        public Account GetAccount(DatabaseConnection dc = null)
        {
            return Account.Load(this.AccountId.Value, dc: dc);
        }

        public ActivityGroup GetActivityGroup(DatabaseConnection dc = null)
        {
            return ActivityGroup.Load(this.ActivityGroupId.Value, dc: dc);
        }

        #endregion
    }
}

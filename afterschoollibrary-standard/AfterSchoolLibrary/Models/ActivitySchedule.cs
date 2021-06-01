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
    /// The schedule an activity/team, session/league will follow. This is the schedule definition. Actual dates are stored in the ActivityInstance
    /// </summary>
	
    public class ActivitySchedule : BasicObjectWithModified
    {
		#region "ClassGenerator"

		public string Name { get { return (string)MDV["Name"].Value; } set { MDV["Name"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public int? ActivityGroupId { get { return (int?)MDV["ActivityGroupId"].Value; } set { MDV["ActivityGroupId"].Value = value; } }
		public string ScheduleDetails { get { return (string)MDV["ScheduleDetails"].Value; } set { MDV["ScheduleDetails"].Value = value; } }
		public bool RepeatsWeekly { get { return (bool)MDV["RepeatsWeekly"].Value; } set { MDV["RepeatsWeekly"].Value = value; } }
		public bool IsOngoing { get { return (bool)MDV["IsOngoing"].Value; } set { MDV["IsOngoing"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public DateTime? StartDate { get { return (DateTime?)MDV["StartDate"].Value; } set { MDV["StartDate"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public DateTime? EndDate { get { return (DateTime?)MDV["EndDate"].Value; } set { MDV["EndDate"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public DateTime? DailyStartTime { get { return (DateTime?)MDV["DailyStartTime"].Value; } set { MDV["DailyStartTime"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public DateTime? DailyEndTime { get { return (DateTime?)MDV["DailyEndTime"].Value; } set { MDV["DailyEndTime"].Value = value; } }
		

        public ActivitySchedule()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(ActivitySchedule).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.RepeatsWeekly = false;
			this.IsOngoing = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(ActivitySchedule).Name;
                _dp = BasicObjectWithModified.get_BaseObjectMap(className);
				_dp.Add(className, "Name", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityGroupId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ScheduleDetails", System.Data.SqlDbType.NVarChar , "4000");
				_dp.Add(className, "RepeatsWeekly", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsOngoing", System.Data.SqlDbType.Bit);
				_dp.Add(className, "StartDate", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "EndDate", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "DailyStartTime", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "DailyEndTime", System.Data.SqlDbType.DateTime);
				
                return _dp;
            }
        }

        public static ActivitySchedule LoadObject(System.Data.DataRow row)
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
			dps.Add(ObjectMap["IsOngoing"]);
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

		public static List<ActivitySchedule> LoadByAccountIdByIsOngoing(int AccountId, bool IsOngoing, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["IsOngoing"], IsOngoing);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivitySchedule> LoadByActivityGroupIdByIsOngoing(int ActivityGroupId, bool IsOngoing, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
			dq.Criterias.Add(ObjectMap["IsOngoing"], IsOngoing);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        private List<ActivityInstance> _instances;
        public List<ActivityInstance> GetInstances(bool forceReload = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (this.ID.HasValue && (_instances.IsEmpty() || forceReload))
                _instances = ActivityInstance.LoadByActivityScheduleId(this.ID.Value, msgs, dc)
                    .OrderBy(x => x.StartDateTime)
                    .ToList();

            return _instances ?? new List<ActivityInstance>();
        }

        public List<ActivityInstance> GetInSessionInstances(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetInstances(msgs: msgs, dc: dc).Where(instance => instance.InstanceTypeId == InstanceType.InSession).ToList();
        }

        public List<ActivityInstance> GetOutSessionInstances(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetInstances(msgs: msgs, dc: dc).Where(instance => instance.InstanceTypeId == InstanceType.OutOfSession).ToList();
        }

        public static ActivitySchedule LoadByIdByAccountId(int Id, int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Id"], Id);
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static ActivitySchedule LoadByAccountIdByName(int AccountId, string Name, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["Name"], Name);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
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

        public static string GenerateUniqueName(int accountId, string name, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var rtnName = name;
            var schedule = ActivitySchedule.LoadByAccountIdByName(accountId, name, msgs, dc);
            var counter = 1;

            while (schedule.IsNotEmpty())
            {
                counter++;
                rtnName = $"{name} {counter.ToString()}";
                schedule = ActivitySchedule.LoadByAccountIdByName(accountId, rtnName, msgs, dc);
            }

            return rtnName;
        }

        public static ActivitySchedule EditExisting(string scheduleId, string scheduleName, string details, bool repeatsWeekly, bool isOngoing, DateTime? startDate, DateTime? endDate, DateTime? dailyStartTime, DateTime? dailyEndTime, List<DateRange> scheduleDates, List<DateTime> scheduleDatesClosed, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            ActivitySchedule schedule = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) => {

                    schedule = ActivitySchedule.Load(scheduleId.ToInteger(), amsgs, adc);

                    amsgs.AddRange(schedule.UpdateScheduleAndInstances(
                        name: scheduleName,
                        details: details,
                        repeatsWeekly: repeatsWeekly,
                        isOngoing: isOngoing,
                        startDate: startDate,
                        endDate: endDate,
                        dailyStartTime: dailyStartTime,
                        dailyEndTime: dailyEndTime,
                        scheduleDates: scheduleDates,
                        scheduleDatesClosed: scheduleDatesClosed,
                        loggedInUserId: loggedInUserId,
                        dc: adc)); 
                },
                onSuccess: (SystemMessages smsgs) => {
                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages fmsgs) => {
                    msgs.AddRange(fmsgs);
                },
                dc: dc
            );

            return schedule;
        }

        public static ActivitySchedule GetNewDefaultSchedule(List<DateRange> scheduleDates = null, bool generateDates = false)
        {
            var schedule = new ActivitySchedule();
            schedule.StartDate = DateTime.UtcNow.AddWeeks(1).FirstDateOfWeek();
            schedule.EndDate = schedule.StartDate.Value.AddWeeks(9).AddDays(-1);
            schedule.DailyStartTime = DateTime.UtcNow.Date.AddHours(15);
            schedule.DailyEndTime = DateTime.UtcNow.Date.AddHours(16);
            schedule.RepeatsWeekly = true;

            if (generateDates)
            {
                scheduleDates = new List<DateRange>();

                for (int i = 0; i < 8; i++)
                {
                    var date = schedule.StartDate.Value.AddDays((int)DayOfWeek.Monday);
                    scheduleDates.Add(
                        new DateRange(date.Date.AddHours(schedule.DailyStartTime.Value.Hour), date.Date.AddHours(schedule.DailyEndTime.Value.Hour))
                    );
                }
            }

            if (scheduleDates.IsNotEmpty())
                schedule.AddInstances(
                    scheduleDates.Select(dateRange =>
                        new ActivityInstance()
                        {
                            StartDateTime = dateRange.StartDate,
                            EndDateTime = dateRange.EndDate,
                            InstanceTypeId = InstanceType.InSession,
                            Day = dateRange.StartDate.DayOfWeek.ToDayOfWeekASL()
                        }).ToList());

            return schedule;
        }

        public static ActivitySchedule GetNewDefaultSchedule(RegistrationPeriod period)
        {
            if (period.IsEmpty())
                return GetNewDefaultSchedule();

            return new ActivitySchedule
            {
                StartDate = period.EndDateTime.Value.Date.FirstDateOfWeek().AddWeeks(1),
                EndDate = period.EndDateTime.Value.Date.FirstDateOfWeek().AddWeeks(9),
                DailyStartTime = DateTime.UtcNow.Date.AddHours(15),
                DailyEndTime = DateTime.UtcNow.Date.AddHours(17),
                RepeatsWeekly = true
            };
        }

        // TODO: Save? what about instances that get added but aren't saved?
        private void AddInstances(List<ActivityInstance> list)
        {
            _instances = new List<ActivityInstance>();
            _instances.AddRange(list);
        }

        public static ActivitySchedule LoadWithInstances(int ActivityScheduleId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var schedule = ActivitySchedule.Load(ActivityScheduleId, msgs, dc);
            schedule?.GetInstances(false, msgs, dc);
            return schedule;
        }

        public ActivityInstance AddInstance(DateRange dateRange, InstanceType instanceType, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return AddInstance(dateRange.StartDate, dateRange.EndDate, instanceType, loggedInUserId, msgs, dc);
        }

        public ActivityInstance AddInstance(DateTime startDateTime, DateTime endDateTime, InstanceType instanceType, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            msgs = msgs ?? new SystemMessages();

            var instance = new ActivityInstance
            {
                ActivityScheduleId = this.ID,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                InstanceTypeId = instanceType,
                CreateDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = loggedInUserId,
                LastModifiedBy = loggedInUserId
            };
            instance.Day = instance.StartDateTime.Value.DayOfWeek.ToDayOfWeekASL();
            msgs.AddRange(instance.Save(dc));

            return instance;
        }

        public SystemMessages UpdateScheduleAndInstances(string name, string details, bool repeatsWeekly, bool isOngoing, DateTime? startDate, DateTime? endDate, DateTime? dailyStartTime, DateTime? dailyEndTime, List<DateRange> scheduleDates, List<DateTime> scheduleDatesClosed, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            this.Name = name;
            this.ScheduleDetails = details;
            this.RepeatsWeekly = repeatsWeekly;
            this.IsOngoing = isOngoing;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.DailyStartTime = dailyStartTime;
            this.DailyEndTime = dailyEndTime;
            this.CreateDate = this.LastModifiedDate = DateTime.UtcNow;
            this.CreatedBy = this.LastModifiedBy = loggedInUserId;
            msgs.AddRange(this.Save(dc: dc));

            msgs.AddRange(UpdateInstances(scheduleDates, scheduleDatesClosed, loggedInUserId, dc));

            return msgs;
        }

        public SystemMessages UpdateInstances(List<DateRange> scheduleDates, List<DateTime> scheduleDatesClosed, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var existingInstances = this.GetInstances(false, msgs, dc);

            // IN SESSION
            // delete in session instances
            existingInstances
                .Where(instance => instance.InstanceTypeId == InstanceType.InSession)
                .Where(instance => !scheduleDates
                    .Where(dateRange => 
                        dateRange.StartDate == instance.StartDateTime.Value && 
                        dateRange.EndDate == instance.EndDateTime.Value
                    )
                    .Any()
                 )
                .ToList()
                .ForEach(instance => msgs.AddRange(instance.Delete(dc)));

            // add new in session instances
            scheduleDates
                .Where(dateRange => 
                    !existingInstances
                        .Where(instance => instance.InstanceTypeId == InstanceType.InSession)
                        .Where(instance => 
                            instance.StartDateTime.Value == dateRange.StartDate && 
                            instance.EndDateTime.Value == dateRange.EndDate
                        )
                        .Any())
                .ToList()
                .ForEach(dateRange => this.AddInstance(dateRange, InstanceType.InSession, loggedInUserId, msgs, dc));

            // OUT OF SESSION
            // delete out of session instances
            existingInstances
                .Where(instance => instance.InstanceTypeId == InstanceType.OutOfSession)
                .Where(instance => !scheduleDatesClosed
                    .Where(date =>
                        date == instance.StartDateTime.Value
                    )
                    .Any()
                 )
                .ToList()
                .ForEach(instance => msgs.AddRange(instance.Delete(dc)));

            // add new out of session instances
            scheduleDatesClosed
                .Where(date =>
                    !existingInstances
                        .Where(instance => instance.InstanceTypeId == InstanceType.OutOfSession)
                        .Where(instance =>
                            instance.StartDateTime.Value == date
                        )
                        .Any())
                .ToList()
                .ForEach(date => this.AddInstance(new DateRange(date, date), InstanceType.OutOfSession, loggedInUserId, msgs, dc));


            msgs.AddRange(this.ValidateInSessionInstances(dc));

            return msgs;
        }

        public SystemMessages ValidateInSessionInstances(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            // get all instances currently saved in the transaction
            var updatedInstances = this.GetInstances(true, msgs, dc)
                                    .Where(x => x.InstanceTypeId == InstanceType.InSession)
                                    .ToList();

            // check for dates where end time is less than start time
            if (updatedInstances.Where(x => !x.GetDateRange().IsValid()).Any())
                msgs.Add("You have dates where the end time is earlier than or equal to the start time", MessageType.UserError);
            
            // check for overlapping times 
            if (msgs.HasNoErrors() && DateRange.HasOverlappingDates(updatedInstances.Select(x => x.GetDateRange())))
                msgs.Add("You have dates that overlap", MessageType.UserError);

            return msgs;
        }

        public List<DayOfWeekASL> GetDaysOfTheWeek(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (this.GetInstances(false, msgs, dc).IsNotEmpty())
                return this.GetInstances(false, msgs, dc).Where(instance => instance.InstanceTypeId == InstanceType.InSession).Select(x => x.Day).Distinct().ToList();

            return new List<DayOfWeekASL>();
        }

        public ActivitySchedule GetCopy()
        {
            var schedule = new ActivitySchedule
            {
                Name = this.Name + " [copy]",
                ScheduleDetails = this.ScheduleDetails,
                StartDate = this.StartDate,
                EndDate = this.EndDate,
                DailyStartTime = this.DailyStartTime,
                DailyEndTime = this.DailyEndTime,
                RepeatsWeekly = this.RepeatsWeekly,
                IsOngoing = this.IsOngoing
            };

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
                        IsOngoing = this.IsOngoing,
                        ScheduleDetails = this.ScheduleDetails,
                        CreateDate = DateTime.UtcNow,
                        CreatedBy = loggedInUserId,
                        LastModifiedDate = DateTime.UtcNow,
                        LastModifiedBy = loggedInUserId,
                        ActivityGroupId = this.ActivityGroupId
                    };
                    amsgs.AddRange(copy.Save(adc));

                    // add instances
                    this.GetInstances(false, amsgs, adc)
                        .ToList()
                        .ForEach(instance => 
                            copy.AddInstance(
                                instance.StartDateTime.Value, 
                                instance.EndDateTime.Value, 
                                instance.InstanceTypeId,
                                loggedInUserId, 
                                amsgs, 
                                adc)
                        );
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

        public List<Activity> GetActivities(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Activity.LoadByActivityScheduleIdByAccountId(this.ID.Value, this.AccountId.Value, msgs, dc);
        }

        public Account GetAccount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.Load(this.AccountId.Value, msgs, dc);
        }

        public ActivityGroup GetActivityGroup(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityGroup.Load(this.ActivityGroupId.Value, msgs, dc);
        }

        public List<ActivityInstance> GetUpcomingInstance(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetInSessionInstances(msgs, dc)
                .Where(x => x.StartDateTime.Value > DateTime.UtcNow)
                .OrderBy(x => x.StartDateTime.Value)
                .ToList();
        }

        #endregion
    }
}

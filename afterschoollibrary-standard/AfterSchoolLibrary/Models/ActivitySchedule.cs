using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Models
{
    /// <summary>
    ///     The schedule an activity/team, session/league will follow. This is the schedule definition. Actual dates are stored
    ///     in the ActivityInstance
    /// </summary>
    public class ActivitySchedule : BasicObjectWithModified
    {
        #region "ClassGenerator"

        public string Name
        {
            get { return (string) MDV["Name"].Value; }
            set { MDV["Name"].Value = value; }
        }

        /// <summary>
        ///     Required.
        /// </summary>
        public int? AccountId
        {
            get { return (int?) MDV["AccountId"].Value; }
            set { MDV["AccountId"].Value = value; }
        }

        public int? ActivityGroupId
        {
            get { return (int?) MDV["ActivityGroupId"].Value; }
            set { MDV["ActivityGroupId"].Value = value; }
        }

        public string ScheduleDetails
        {
            get { return (string) MDV["ScheduleDetails"].Value; }
            set { MDV["ScheduleDetails"].Value = value; }
        }

        public bool RepeatsWeekly
        {
            get { return (bool) MDV["RepeatsWeekly"].Value; }
            set { MDV["RepeatsWeekly"].Value = value; }
        }

        public bool IsOngoing
        {
            get { return (bool) MDV["IsOngoing"].Value; }
            set { MDV["IsOngoing"].Value = value; }
        }

        /// <summary>
        ///     Required.
        /// </summary>
        public DateTime? StartDate
        {
            get { return (DateTime?) MDV["StartDate"].Value; }
            set { MDV["StartDate"].Value = value; }
        }

        /// <summary>
        ///     Required.
        /// </summary>
        public DateTime? EndDate
        {
            get { return (DateTime?) MDV["EndDate"].Value; }
            set { MDV["EndDate"].Value = value; }
        }

        /// <summary>
        ///     Required.
        /// </summary>
        public DateTime? DailyStartTime
        {
            get { return (DateTime?) MDV["DailyStartTime"].Value; }
            set { MDV["DailyStartTime"].Value = value; }
        }

        /// <summary>
        ///     Required.
        /// </summary>
        public DateTime? DailyEndTime
        {
            get { return (DateTime?) MDV["DailyEndTime"].Value; }
            set { MDV["DailyEndTime"].Value = value; }
        }

        public String DaysOfTheWeek
        {
            get { return (String) MDV["DaysOfTheWeek"].Value; }
            set { MDV["DaysOfTheWeek"].Value = value; }
        }


        public ActivitySchedule()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(ActivitySchedule).Name, "ID");
            this.LoadObjectValues(ObjectMap);
            this.RepeatsWeekly = false;
            this.IsOngoing = false;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                DatabaseParameters _dp = new DatabaseParameters();
                string className = typeof(ActivitySchedule).Name;
                _dp = get_BaseObjectMap(className);
                _dp.Add(className, "Name", SqlDbType.NVarChar, "100");
                _dp.Add(className, "AccountId", SqlDbType.Int);
                _dp.Add(className, "ActivityGroupId", SqlDbType.Int);
                _dp.Add(className, "ScheduleDetails", SqlDbType.NVarChar, "4000");
                _dp.Add(className, "RepeatsWeekly", SqlDbType.Bit);
                _dp.Add(className, "IsOngoing", SqlDbType.Bit);
                _dp.Add(className, "StartDate", SqlDbType.DateTime);
                _dp.Add(className, "EndDate", SqlDbType.DateTime);
                _dp.Add(className, "DailyStartTime", SqlDbType.DateTime);
                _dp.Add(className, "DailyEndTime", SqlDbType.DateTime);
                _dp.Add(className, "DaysOfTheWeek", SqlDbType.NVarChar, "50");

                return _dp;
            }
        }

        public static ActivitySchedule LoadObject(DataRow row)
        {
            ActivitySchedule obj = new ActivitySchedule();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                DatabaseQuery dq = get_DatabaseQuery(ObjectMap, typeof(ActivitySchedule).Name);

                return dq;
            }
        }

        public static ActivitySchedule Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ActivitySchedule> Load(Hashtable criterias = null, int? start = null, int? length = null,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

        public SystemMessages Save(DatabaseConnection dc = null)
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

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ActivitySchedule).Name);
        }

        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return _Create(DatabaseQuery, dc, ObjectMap);
        }

        public static SystemMessages Alter(DatabaseConnection dc = null)
        {
            SystemMessages msgs = new SystemMessages();
            DatabaseParameters dps;

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["Name"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["AccountId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ActivityGroupId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ScheduleDetails"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["RepeatsWeekly"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsOngoing"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["StartDate"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["EndDate"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["DailyStartTime"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["DailyEndTime"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["DaysOfTheWeek"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));


            return msgs;
        }

        public ActivitySchedule Reload(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Load(this.ID.Value, msgs, dc: dc);
        }

        public static List<ActivitySchedule> LoadByAccountId(int AccountId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<ActivitySchedule> LoadByActivityGroupId(int ActivityGroupId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<ActivitySchedule> LoadByAccountIdByIsOngoing(int AccountId, bool IsOngoing,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["IsOngoing"], IsOngoing);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<ActivitySchedule> LoadByActivityGroupIdByIsOngoing(int ActivityGroupId, bool IsOngoing,
            SystemMessages msgs = null, DatabaseConnection dc = null)
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

        public List<ActivityInstance> GetInstances(bool forceReload = false, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (this.ID.HasValue && (_instances.IsEmpty() || forceReload))
                _instances = ActivityInstance.LoadByActivityScheduleId(this.ID.Value, msgs, dc)
                    .OrderBy(x => x.StartDateTime)
                    .ToList();

            return _instances ?? new List<ActivityInstance>();
        }

        public int GetInstanceCount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityInstance.GetCountByActivityScheduleId(this.ID.Value, msgs, dc);
        }

        public List<ActivityInstance> GetInSessionInstances(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetInstances(msgs: msgs, dc: dc).Where(instance => instance.InstanceTypeId == InstanceType.InSession)
                .ToList();
        }

        public ActivityInstance GetInSessionInstanceByDate(DateTime startDate, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return ActivityInstance.LoadByActivityScheduleIdByStartDate(this.ID.Value, startDate, msgs, dc);
        }

        public List<ActivityInstance> GetInSessionInstances(DateTime startDate, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            return GetInSessionInstances(msgs, dc).Where(x => x.StartDateTime.Value > startDate).ToList();
        }

        public List<ActivityInstance> GetOutSessionInstances(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetInstances(msgs: msgs, dc: dc)
                .Where(instance => instance.InstanceTypeId == InstanceType.OutOfSession).ToList();
        }

        public static ActivitySchedule LoadByIdByAccountId(int Id, int AccountId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Id"], Id);
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static ActivitySchedule LoadByAccountIdByName(int AccountId, string Name, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["Name"], Name);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static bool ExistsByAccountIdByName(int AccountId, string Name, int existingId,
            DatabaseConnection dc = null)
        {
            DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["Name"], Name);
            dq.Criterias.Add(ObjectMap["ID"], existingId, DatabaseCriteriaOperatorEnum.NotEqualTo);
            return dq.GetCount() > 0;
        }

        public static string GenerateUniqueName(int accountId, string name, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var rtnName = name;
            var schedule = LoadByAccountIdByName(accountId, name, msgs, dc);
            var counter = 1;

            while (schedule.IsNotEmpty())
            {
                counter++;
                rtnName = $"{name} {counter.ToString()}";
                schedule = LoadByAccountIdByName(accountId, rtnName, msgs, dc);
            }

            return rtnName;
        }

        public static ActivitySchedule EditExisting(string scheduleId, string scheduleName, string details,
            bool repeatsWeekly, bool isOngoing, DateTime? startDate, DateTime? endDate, DateTime? dailyStartTime,
            DateTime? dailyEndTime, List<ActivityInstance> scheduleDates, List<DateTime> scheduleDatesClosed,
            string daysOfTheWeek, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            ActivitySchedule schedule = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    schedule = Load(scheduleId.ToInteger(), amsgs, adc);

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
                        daysOfTheWeek: daysOfTheWeek,
                        loggedInUserId: loggedInUserId,
                        dc: adc));
                },
                onSuccess: (SystemMessages smsgs) => { msgs.AddRange(smsgs); },
                onFailure: (SystemMessages fmsgs) => { msgs.AddRange(fmsgs); },
                dc: dc
            );

            return schedule;
        }

        public static ActivitySchedule GetNewDefaultSchedule(List<DateRange> scheduleDates = null,
            bool generateDates = false)
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
                    var date = schedule.StartDate.Value.AddDays((int) DayOfWeek.Monday);
                    scheduleDates.Add(
                        new DateRange(date.Date.AddHours(schedule.DailyStartTime.Value.Hour),
                            date.Date.AddHours(schedule.DailyEndTime.Value.Hour))
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
                            Day = dateRange.StartDate.DayOfWeek.ToDayOfWeekASL(),
                        }).ToList());

            return schedule;
        }

        public static ActivitySchedule GetNewDefaultSchedule(Activity activity, SystemMessages msgs)
        {
            var period = ActivityService.GetActiveRegistrationPeriod(activity, msgs);

            if (period.IsEmpty())
                return GetNewDefaultSchedule();

            var startDate = period.EndDateTime.Value.Date.FirstDateOfWeek().AddWeeks(1);
            var endDate = period.EndDateTime.Value.Date.FirstDateOfWeek().AddWeeks(9);

            if (activity.IsContinuousEnrollment)
                startDate = period.StartDateTime.Value.FirstDateOfWeek().AddWeeks(1);

            if (activity.CostFrequencyTypeId == ActivityCostFrequencyType.PerClass)
            {
                startDate = DateTime.UtcNow.Date.AddDays(1);
                endDate = startDate.AddWeeks(9);
            }

            return new ActivitySchedule
            {
                StartDate = startDate,
                EndDate = endDate,
                DailyStartTime = DateTime.UtcNow.Date.AddHours(15),
                DailyEndTime = DateTime.UtcNow.Date.AddHours(17),
                RepeatsWeekly = true,
            };
        }

        // TODO: Save? what about instances that get added but aren't saved?
        private void AddInstances(List<ActivityInstance> list)
        {
            _instances = new List<ActivityInstance>();
            _instances.AddRange(list);
        }

        public static ActivitySchedule LoadWithInstances(int ActivityScheduleId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var schedule = Load(ActivityScheduleId, msgs, dc);
            schedule?.GetInstances(false, msgs, dc);
            return schedule;
        }

        public ActivityInstance AddInstance(DateRange dateRange, InstanceType instanceType, int loggedInUserId,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            return AddInstance(dateRange.StartDate, dateRange.EndDate, instanceType, loggedInUserId, msgs, dc);
        }

        public ActivityInstance AddInstance(ActivityInstance instance, InstanceType instanceType, int loggedInUserId,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            return AddInstance(instance.StartDateTime.Value, instance.EndDateTime.Value, instance.SeatsAvailable,
                instanceType, loggedInUserId, msgs, dc);
        }

        public ActivityInstance AddInstance(DateTime startDateTime, DateTime endDateTime, InstanceType instanceType,
            int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AddInstance(startDateTime, endDateTime, null, instanceType, loggedInUserId, msgs, dc);
        }

        public ActivityInstance AddInstance(DateTime startDateTime, DateTime endDateTime, int? seatsAvailable,
            InstanceType instanceType, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            msgs = msgs ?? new SystemMessages();

            var instance = new ActivityInstance
            {
                ActivityScheduleId = this.ID,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                InstanceTypeId = instanceType,
                SeatsAvailable = seatsAvailable,
                CreateDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = loggedInUserId,
                LastModifiedBy = loggedInUserId,
            };
            instance.Day = instance.StartDateTime.Value.DayOfWeek.ToDayOfWeekASL();
            msgs.AddRange(instance.Save(dc));

            return instance;
        }

        public void UpdateInstance(ActivityInstance instance, int? numberOfSeats, int loggedInUserId,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            msgs = msgs ?? new SystemMessages();

            instance.SeatsAvailable = numberOfSeats;
            instance.LastModifiedBy = loggedInUserId;
            instance.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(instance.Save(dc));
        }

        public SystemMessages UpdateScheduleAndInstances(string name, string details, bool repeatsWeekly,
            bool isOngoing, DateTime? startDate, DateTime? endDate, DateTime? dailyStartTime, DateTime? dailyEndTime,
            List<ActivityInstance> scheduleDates, List<DateTime> scheduleDatesClosed, string daysOfTheWeek,
            int loggedInUserId, DatabaseConnection dc = null)
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
            this.DaysOfTheWeek = daysOfTheWeek;
            this.CreateDate = this.LastModifiedDate = DateTime.UtcNow;
            this.CreatedBy = this.LastModifiedBy = loggedInUserId;
            msgs.AddRange(this.Save(dc: dc));

            msgs.AddRange(UpdateInstances(scheduleDates, scheduleDatesClosed, loggedInUserId, dc));

            return msgs;
        }

        public SystemMessages UpdateInstances(List<ActivityInstance> scheduleDates, List<DateTime> scheduleDatesClosed,
            int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var existingInstances = this.GetInstances(false, msgs, dc);

            // IN SESSION
            // delete in session instances
            if (!this.IsOngoing)
            {
                existingInstances
                    .Where(instance => instance.InstanceTypeId == InstanceType.InSession)
                    .Where(instance => !scheduleDates
                        .Any(newInstance =>
                            newInstance.StartDateTime.Value == instance.StartDateTime.Value &&
                            newInstance.EndDateTime.Value == instance.EndDateTime.Value
                        )
                    )
                    .ToList()
                    .ForEach(instance => CheckForActivityInstanceSeats(instance, msgs, dc));

                // add new in session instances
                scheduleDates
                    .Where(newInstance =>
                        !existingInstances
                            .Where(instance => instance.InstanceTypeId == InstanceType.InSession)
                            .Any(instance =>
                                instance.StartDateTime.Value == newInstance.StartDateTime.Value &&
                                instance.EndDateTime.Value == newInstance.EndDateTime.Value
                            ))
                    .ToList()
                    .ForEach(instance => this.AddInstance(instance, InstanceType.InSession, loggedInUserId, msgs, dc));

                // update instances
                existingInstances
                    .Where(instance => instance.InstanceTypeId == InstanceType.InSession)
                    .Where(instance => scheduleDates
                        .Any(newInstance =>
                            newInstance.StartDateTime.Value == instance.StartDateTime.Value &&
                            newInstance.EndDateTime.Value == instance.EndDateTime.Value
                        )
                    )
                    .ToList()
                    .ForEach(instance => this.UpdateInstance(instance, scheduleDates
                        .FirstOrDefault(newInstance =>
                            newInstance.StartDateTime.Value == instance.StartDateTime.Value &&
                            newInstance.EndDateTime.Value == instance.EndDateTime.Value
                        )?.SeatsAvailable, loggedInUserId, msgs, dc));
            }

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
                .ForEach(instance => CheckForActivityInstanceSeats(instance, msgs, dc));

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
                .ForEach(date =>
                    this.AddInstance(new DateRange(date, date), InstanceType.OutOfSession, loggedInUserId, msgs, dc));


            msgs.AddRange(this.ValidateInSessionInstances(dc));

            return msgs;
        }


        public SystemMessages CheckForActivityInstanceSeats(ActivityInstance instance, SystemMessages msgs,
            DatabaseConnection dc = null)
        {
            //.. if ActivityInstanceSeats exist for ActivityInstance (users already registered for given ActivityInstance)
            //.. then the ActivityInstance should not be deleted
            var occupiedSeats =
                ActivityInstanceSeats.GetSeatsTakenByActivityInstanceId(ActivityInstanceId: instance.ID.Value,
                    msgs: msgs, dc: dc);

            if (occupiedSeats <= 0)
                msgs.AddRange(instance.Delete(dc));
            else
                msgs.Add(
                    message:
                    $"Schedule instance for {instance.StartDateTime.ToShortDateString()} To {instance.EndDateTime.ToShortDateString()} cannot be removed as students already registered in this interval.",
                    messageType: MessageType.UserError);

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
            var dateRanges = updatedInstances.Select(x => x.GetDateRange()).ToArray();
            if (dateRanges.Any(x => !(x.StartDate <= x.EndDate)))
                msgs.Add("You have dates where the end time is earlier than or equal to the start time",
                    MessageType.UserError);

            if (msgs.HasNoErrors())
                msgs.AddRange(CheckForOverlappingTimes(dateRanges));

            return msgs;
        }

        public static SystemMessages CheckForOverlappingTimes(IReadOnlyList<DateRange> dateRanges)
        {
            var msgs = new SystemMessages();
            if (HasOverlappingDates(dateRanges))
            {
                msgs.Add("You have dates that overlap", MessageType.UserError);
            }

            return msgs;
        }

        private static bool HasOverlappingDates(IReadOnlyList<DateRange> dateRanges)
        {
            for (var i = 0; i < dateRanges.Count; i++)
            {
                for (var j = i + 1; j < dateRanges.Count; j++)
                {
                    if (DateRangesOverlap(dateRanges[i], dateRanges[j]))
                        return true;
                }
            }

            return false;
        }

        private static bool DateRangesOverlap(DateRange dateRange1, DateRange dateRange2)
        {
            return (dateRange1.StartDate > dateRange2.StartDate ? dateRange1.StartDate : dateRange2.StartDate) <
                   (dateRange1.EndDate < dateRange2.EndDate ? dateRange1.EndDate : dateRange2.EndDate);
        }

        public List<DayOfWeekASL> GetDaysOfTheWeek(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (this.DaysOfTheWeek.IsNotBlank())
                return this.DaysOfTheWeek.Split(',').ToIntegerArray().Select(x => (DayOfWeekASL) x)
                    .OrderBy(x => (int) x).ToList();

            var instances = this.GetInstances(false, msgs, dc);
            if (instances.IsNotEmpty())
                return instances
                    .Where(instance => instance.InstanceTypeId == InstanceType.InSession)
                    .Select(x => x.Day).Distinct()
                    .OrderBy(x => (int) x)
                    .ToList();

            return new List<DayOfWeekASL>();
        }

        public ActivitySchedule GetCopy(SystemMessages msgs = null, DatabaseConnection dc = null)
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
                IsOngoing = this.IsOngoing,
            };

            schedule.AddInstances(this.GetInstances(msgs: msgs, dc: dc));
            return schedule;
        }

        public ActivitySchedule CopyAndSave(int loggedInUserId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            ActivitySchedule copy = null;

            if (msgs.IsEmpty()) msgs = new SystemMessages();

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
                        ActivityGroupId = this.ActivityGroupId,
                        DaysOfTheWeek = this.DaysOfTheWeek,
                    };
                    amsgs.AddRange(copy.Save(adc));

                    // only copy out of session instances for ongoing activities
                    if (this.IsOngoing)
                        this.GetOutSessionInstances(amsgs, adc)
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
                    else
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
                onSuccess: (SystemMessages smsgs) => { msgs.AddRange(smsgs); },
                onFailure: (SystemMessages fmsgs) => { msgs.AddRange(fmsgs); },
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
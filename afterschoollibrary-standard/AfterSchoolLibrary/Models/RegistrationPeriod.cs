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
    /// The registration time window that the registrant/student has to be able to register online
    /// </summary>
	
    public class RegistrationPeriod : BaseEvent
    {
		#region "ClassGenerator"

		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public RegistrationMode RegistrationModeId { get { return (RegistrationMode)MDV["RegistrationModeId"].Value; } set { MDV["RegistrationModeId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public int? ActivityGroupId { get { return (int?)MDV["ActivityGroupId"].Value; } set { MDV["ActivityGroupId"].Value = value; } }
		public int? AccountActivityGroupId { get { return (int?)MDV["AccountActivityGroupId"].Value; } set { MDV["AccountActivityGroupId"].Value = value; } }
		

        public RegistrationPeriod()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(RegistrationPeriod).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.RegistrationModeId = RegistrationMode.NotSet;
			this.Uid = Guid.NewGuid();
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(RegistrationPeriod).Name;
                _dp = BaseEvent.get_BaseObjectMap(className);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrationModeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "ActivityGroupId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountActivityGroupId", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        public static RegistrationPeriod LoadObject(System.Data.DataRow row)
        {
            RegistrationPeriod obj = new RegistrationPeriod();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(RegistrationPeriod).Name);
				
                return dq;
            }
        }

        public static RegistrationPeriod Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<RegistrationPeriod> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return RegistrationPeriod.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return RegistrationPeriod.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.Uid.IsEmpty())
				msgs.Add("Uid is required on RegistrationPeriod object", MessageType.Error);
			if (this.StartDateTime.IsEmpty())
				msgs.Add("StartDateTime is required on RegistrationPeriod object", MessageType.Error);
			if (this.EndDateTime.IsEmpty())
				msgs.Add("EndDateTime is required on RegistrationPeriod object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(RegistrationPeriod).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(RegistrationPeriod).Name);
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
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrationModeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityGroupId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountActivityGroupId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public RegistrationPeriod Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return RegistrationPeriod.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<RegistrationPeriod> LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static RegistrationPeriod LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<RegistrationPeriod> LoadByActivityGroupId(int ActivityGroupId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<RegistrationPeriod> LoadByActivityGroupIdByAccountId(int ActivityGroupId, int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<RegistrationPeriod> LoadByAccountActivityGroupId(int AccountActivityGroupId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountActivityGroupId"], AccountActivityGroupId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"

        public static RegistrationPeriod LoadByIdByAccountId(int Id, int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Id"], Id);
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<RegistrationPeriod> LoadByActivityGroupIds(List<int> ActivityGroupIds, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            if (ActivityGroupIds.IsEmpty() || !ActivityGroupIds.Any())
                return new List<RegistrationPeriod>();

            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupIds, DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<RegistrationPeriod> LoadByAccountActivityGroupIds(List<int> AccountActivityGroupIds, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountActivityGroupId"], AccountActivityGroupIds, DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<RegistrationPeriod> LoadByAccountId(int AccountId, int? start, int? length, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static RegistrationPeriod LoadByAccountIdByName(int AccountId, string Name, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["Name"], Name);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static bool ExistsByAccountIdByName(int AccountId, string Name, int existingId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["Name"], Name);
            dq.Criterias.Add(ObjectMap["ID"], existingId, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.NotEqualTo);
            return dq.GetCount(msgs) > 0;
        }

        public static List<RegistrationPeriod> LoadByAccountIdByDate(int AccountId, DateTime? startDate = null, DateTime? endDate = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["EndDateTime"], startDate, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["EndDateTime"], endDate, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static string GenerateUniqueName(int accountId, string name, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var rtnName = name;
            var period = RegistrationPeriod.LoadByAccountIdByName(accountId, name, msgs, dc);
            var counter = 1;

            while (period.IsNotEmpty())
            {
                counter++;
                rtnName = name + " " + counter.ToString();
                period = RegistrationPeriod.LoadByAccountIdByName(accountId, rtnName, msgs, dc);
            }

            return rtnName;
        }

        public List<ActivityRegistrationPeriod> GetActivityRegistrationPeriods(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityRegistrationPeriod.LoadByRegistrationPeriodId(this.ID.Value, msgs, dc);
        }

        public List<Activity> GetActivitiesInLocations(List<int> locationIds, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var activityIds = GetActivityRegistrationPeriods(msgs, dc)
                    .Select(x => x.ActivityId.Value)
                    .ToList();

            if (activityIds.Any())
                return Activity.LoadListWithLocations(
                        activityIds,
                        locationIds,
                        msgs,
                        dc)
                    .Where(x => !x.IsDeleted)
                    .ToList();

            return new List<Activity>();
        }

        public List<Activity> GetActivities(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var activityIds = GetActivityRegistrationPeriods(msgs, dc)
                    .Where(x => x.IsActive)
                    .Select(x => x.ActivityId.Value)
                    .ToList();

            if (activityIds.Any())
                return Activity.LoadList(
                        activityIds, 
                        msgs, 
                        dc)
                    .Where(x => !x.IsDeleted)
                    .ToList();

            return new List<Activity>();
        }

        public int GetActivityCount(DatabaseConnection dc = null)
        {
            return ActivityRegistrationPeriod.GetCount(
                new Hashtable()
                {
                    { "RegistrationPeriodId", this.ID.Value },
                    { "IsActive", true },
                    { "ActivityIsDeleted", false }
                },
                dc: dc
            );
        }

        public RegistrationPeriod Copy(string newName, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc)
        {
            var period = new RegistrationPeriod
            {
                AccountId = this.AccountId,
                CreateDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = loggedInUserId,
                LastModifiedBy = loggedInUserId,
                Description = this.Description,
                EndDateTime = this.EndDateTime,
                Location = this.Location,
                Name = newName.IfEmpty(this.Name + " Copy").MaxChars(50),
                StartDateTime = this.StartDateTime
            };
            msgs.AddRange(period.Save(dc: dc));

            return period;
        }

        public List<Registration> GetRegistrations(int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return Registration.LoadByRegistrationPeriodId(this.ID.Value, start, length, dc: dc);
        }

        public int GetRegistrationCount(DatabaseConnection dc = null)
        {
            return Registration.GetCount(criterias: new Hashtable() { { "RegistrationPeriodId", this.ID.Value } }, dc: dc);
        }

        //public List<RegistrantActivity> GetRegistrantActivities(List<Registration> registrations = null, DatabaseConnection dc = null)
        //{
        //    if (registrations.IsEmpty())
        //        registrations = Registration.LoadByRegistrationPeriodId(this.ID.Value, dc: dc);

        //    if (registrations.Any())
        //    {
        //        return RegistrantActivity.LoadByRegistrationIds(registrations.Select(x => x.ID.Value).ToList(), dc: dc)
        //            .Where(x => !x.ActivityIsDeleted)
        //            .ToList();
        //    }

        //    return new List<RegistrantActivity>();
        //}

        public int GetSeatCountByAssignmentType(RegistrationAssignmentType rat, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByAssignmentType(assignmentType: rat, registrationPeriodId: this.ID.Value, dc: dc);
        }

        //public List<RegistrantAssignment> RandomizePlacementAttempt(Account account, SystemMessages msgs, DatabaseConnection dc = null)
        //{
        //    if (msgs.IsEmpty()) msgs = new SystemMessages();
        //    var rr = new RegistrationRandomization(this);
        //    return rr.AttemptLottery(msgs, dc: dc);
        //}

        //public SystemMessages RandomizePlacementApply(List<RegistrantAssignment> assignments, Account account, DatabaseConnection dc = null)
        //{
        //    var msgs = new SystemMessages();
        //    var rr = new RegistrationRandomization(this);
        //    msgs.AddRange(rr.ApplyLottery(assignments, dc: dc));
        //    return msgs;
        //}

        //public SystemMessages RandomizePlacementReset(DatabaseConnection dc = null)
        //{
        //    var msgs = new SystemMessages();

        //    Database.ExecuteTransaction(
        //        action: (SystemMessages amsgs, DatabaseConnection adc) => 
        //        {
        //            this.GetRegistrantActivities(dc: adc)
        //                .ForEach(ra => 
        //                    msgs.AddRange(ra.ChangeSeatAssignment(RegistrationAssignmentType.Applied, adc))
        //                );
        //        },
        //        onSuccess: (SystemMessages smsgs) =>
        //        {
        //            msgs.AddRange(smsgs);
        //        },
        //        onFailure: (SystemMessages fmsgs) =>
        //        {
        //            msgs.AddRange(fmsgs);
        //        }
        //    );
            
        //    return msgs;
        //}

        private ActivityGroup _Group = null;
        public ActivityGroup GetGroup(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (_Group.IsEmpty())
                _Group = ActivityGroup.Load(this.ActivityGroupId.Value, msgs, dc);

            return _Group;
        }

        #endregion
    }
}

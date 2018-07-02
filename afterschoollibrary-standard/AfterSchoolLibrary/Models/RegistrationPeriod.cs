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
	/// <summary>
    /// The registration time window that the registrant/student has to be able to register online
    /// </summary>
	
    public class RegistrationPeriod : BaseEvent
    {
		#region "ClassGenerator"

		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public RegistrationMode RegistrationModeId { get { return (RegistrationMode)MDV["RegistrationModeId"].Value; } set { MDV["RegistrationModeId"].Value = value; } }
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

        protected static RegistrationPeriod LoadObject(System.Data.DataRow row)
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

		

        #endregion

		#region "UserDefined"

        public string GetStatus(DatabaseConnection dc = null)
        {
            if (WillOpen(dc: dc))
                return "Not Open";
            else if (IsOpen(dc: dc))
                return "Open";
            else if (IsClosed(dc: dc))
                return "Closed";

            return "N/A";
        }

        public static List<RegistrationPeriod> LoadByAccountId(int AccountId, int? start, int? length, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static RegistrationPeriod LoadByAccountIdByName(int AccountId, string Name, DatabaseConnection dc = null)
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

        public static List<RegistrationPeriod> LoadByAccountIdByDate(int AccountId, DateTime? startDate = null, DateTime? endDate = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["EndDateTime"], startDate, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["EndDateTime"], endDate, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static string GenerateUniqueName(int accountId, string name, DatabaseConnection dc = null)
        {
            var rtnName = name;
            var period = RegistrationPeriod.LoadByAccountIdByName(accountId, name, dc: dc);
            var counter = 1;

            while (period.IsNotEmpty())
            {
                counter++;
                rtnName = name + " " + counter.ToString();
                period = RegistrationPeriod.LoadByAccountIdByName(accountId, rtnName, dc: dc);
            }

            return rtnName;
        }

        public bool IsOpen(DatabaseConnection dc = null)
        {
            var now = this.GetAccount(dc: dc).ConvertToAccountTimeZone(DateTime.UtcNow);
            return this.StartDateTime.Value <= now && this.EndDateTime.Value >= now;
        }

        public bool IsClosed(DatabaseConnection dc = null)
        {
            var now = this.GetAccount(dc: dc).ConvertToAccountTimeZone(DateTime.UtcNow);
            return this.EndDateTime.Value < now;
        }

        public bool WillOpen(DatabaseConnection dc = null)
        {
            var now = this.GetAccount(dc: dc).ConvertToAccountTimeZone(DateTime.UtcNow);
            return this.StartDateTime.Value > now;
        }

        public TimeSpan WillOpenIn(DatabaseConnection dc = null)
        {
            var now = this.GetAccount(dc: dc).ConvertToAccountTimeZone(DateTime.UtcNow);
            return this.StartDateTime.Value - now;
        }

        public TimeSpan WillCloseIn(DatabaseConnection dc = null)
        {
            var now = this.GetAccount(dc: dc).ConvertToAccountTimeZone(DateTime.UtcNow);
            return this.EndDateTime.Value - now;
        }

        public List<ActivityRegistrationPeriod> GetActivityRegistrationPeriods(DatabaseConnection dc = null)
        {
            return ActivityRegistrationPeriod.LoadByRegistrationPeriodId(this.ID.Value, dc: dc);
        }

        public List<Activity> GetActivities(DatabaseConnection dc = null)
        {
            return Activity.LoadList(
                GetActivityRegistrationPeriods(dc: dc)
                    .Select(x => x.ActivityId.Value)
                    .ToList(), dc: dc)
                .Where(x => x.IsDeleted == false)
                .ToList();
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

        public SystemMessages AddActivity(Activity activity, bool isActive, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var item = ActivityRegistrationPeriod.LoadByRegistrationPeriodIdByActivityId(this.ID.Value, activity.ID.Value, dc: dc);

            if ((item.IsEmpty() || (item.IsNotEmpty() && !item.IsActive)) && isActive)
            {
                activity.GetActivityRegistrationPeriods(dc: dc).ForEach(period => {
                    period.IsActive = false;
                    period.LastModifiedDate = DateTime.UtcNow;
                    period.LastModifiedBy = loggedInUserId;
                    msgs.AddRange(period.Save(dc: dc));
                });
            }

            //if record doesn't already exist
            if (item.IsEmpty())
            {
                var activityperiod = new ActivityRegistrationPeriod
                {
                    RegistrationPeriodId = this.ID,
                    ActivityId = activity.ID,
                    IsActive = isActive,
                    CreateDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    CreatedBy = loggedInUserId,
                    LastModifiedBy = loggedInUserId
                };
                msgs.AddRange(activityperiod.Save(dc: dc));
            }
            else
            {
                item.IsActive = isActive;
                msgs.AddRange(item.Save(dc: dc));
            }

            return msgs;
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
                Name = newName.IfEmpty(this.Name + " Copy"),
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

        public List<RegistrantActivity> GetRegistrantActivities(List<Registration> registrations = null, DatabaseConnection dc = null)
        {
            if (registrations.IsEmpty())
                registrations = Registration.LoadByRegistrationPeriodId(this.ID.Value, dc: dc);

            if(registrations.Any())
                return RegistrantActivity.LoadByRegistrationIds(registrations.Select(x => x.ID.Value).ToList(), dc: dc)
                    .Where(x => !x.ActivityIsDeleted)
                    .ToList();
            else
                return new List<RegistrantActivity>();
        }

        public int GetSeatCountByAssignmentType(RegistrationAssignmentType rat, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByAssignmentType(rat: rat, registrationPeriodId: this.ID.Value, dc: dc);
        }

        public List<RegistrantAssignment> RandomizePlacementAttempt(Account account, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var rr = new RegistrationRandomization(this);
            return rr.AttemptLottery(msgs, dc: dc);
        }

        public SystemMessages RandomizePlacementApply(List<RegistrantAssignment> assignments, Account account, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var rr = new RegistrationRandomization(this);
            msgs.AddRange(rr.ApplyLottery(assignments, dc: dc));
            return msgs;
        }

        public SystemMessages RandomizePlacementReset(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) => 
                {
                    this.GetRegistrantActivities(dc: adc)
                        .ForEach(ra => 
                            msgs.AddRange(ra.ChangeSeatAssignment(RegistrationAssignmentType.Application, adc))
                        );
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                }
            );
            
            return msgs;
        }

        private ActivityGroup _Group = null;
        public ActivityGroup GetGroup(DatabaseConnection dc = null)
        {
            if (_Group.IsEmpty())
                _Group = ActivityGroup.Load(this.ActivityGroupId.Value, dc: dc);

            return _Group;
        }

        private Account _Account = null;
        public Account GetAccount(DatabaseConnection dc = null)
        {
            if (_Account.IsEmpty())
            {
                if (this.AccountId.HasValue)
                {
                    _Account = Account.Load(this.AccountId.Value, dc: dc);
                }
                else
                {
                    var arps = this.GetActivityRegistrationPeriods(dc: dc).Where(x => x.IsActive).ToList();
                    if (arps.Any())
                        _Account = arps.First().GetActivity(dc: dc).GetAccount(dc: dc);
                }
            }
            return _Account;
        }

        public static SystemMessages NotifyRegistrants(int id, string emailmessage, List<int> registrationIds = null, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var regPeriod = RegistrationPeriod.Load(id, dc: dc);
            var account = regPeriod.GetAccount(dc: dc);
            var registrations = new List<Registration>();

            if (registrationIds.IsEmpty())
                registrations = regPeriod.GetGroup(dc: dc).GetRegistrations(dc: dc);
            else
                registrations = Registration.LoadList(registrationIds, dc: dc);

            if (registrations.Any())
            {
                var subject = "";
                var body = "";

                if (regPeriod.IsOpen())
                {
                    subject = $"Registration for {account.Name} is open again";
                    body = "<p>{0},</p>{3}<p>Please click the following link to add to your registration: {1}</p>{2}";
                }
                else if (regPeriod.WillOpen())
                {
                    subject = $"Registration for {account.Name} will open again soon";
                    body = "<p>{0},</p>{3}<p>Registration will open " + regPeriod.StartDateTime.ToShortDateString() + ". Please click the following link to add to your registration: {1}</p>{2}";
                }

                foreach (var reg in registrations)
                {
                    SendGridHelper.SendMail(
                        fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
                            Email.AppEmailAddress, 
                            $"{account.Name} via {Utilities.AppName(account.IsAfterSchool)}"),
                        toEmail: new SendGrid.Helpers.Mail.EmailAddress(reg.RegistrantEmail, reg.Name),
                        subject: subject,
                        body: string.Format(
                            body, 
                            reg.RegistrantFirstName, 
                            reg.AddToRegistrationUrl, 
                            Email.GetAppSignature(account.IsAfterSchool, "invite_enrollees"), 
                            emailmessage),
                        replyTo: new SendGrid.Helpers.Mail.EmailAddress(account.Email, account.Name),
                        emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                    );
                }
            }

            return msgs;
        }

        #endregion
    }
}

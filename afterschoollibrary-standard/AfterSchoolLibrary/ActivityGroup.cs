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
	
    public class ActivityGroup : BasicLookUpObject
    {
		#region "ClassGenerator"

		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public RegistrationMode RegistrationModeId { get { return (RegistrationMode)MDV["RegistrationModeId"].Value; } set { MDV["RegistrationModeId"].Value = value; } }
		public string UniqueName { get { return (string)MDV["UniqueName"].Value; } set { MDV["UniqueName"].Value = value; } }
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public int? FormId { get { return (int?)MDV["FormId"].Value; } set { MDV["FormId"].Value = value; } }
		public bool IsPublished { get { return (bool)MDV["IsPublished"].Value; } set { MDV["IsPublished"].Value = value; } }
		public bool IsArchived { get { return (bool)MDV["IsArchived"].Value; } set { MDV["IsArchived"].Value = value; } }
		public bool IsInviteOnly { get { return (bool)MDV["IsInviteOnly"].Value; } set { MDV["IsInviteOnly"].Value = value; } }
		public bool HasLotteryRun { get { return (bool)MDV["HasLotteryRun"].Value; } set { MDV["HasLotteryRun"].Value = value; } }
		public string AccountName { get { return (string)MDV["AccountName"].Value; } }
		

        public ActivityGroup()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(ActivityGroup).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.RegistrationModeId = RegistrationMode.NotSet;
			this.Uid = Guid.NewGuid();
			this.IsPublished = false;
			this.IsArchived = false;
			this.IsInviteOnly = false;
			this.HasLotteryRun = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(ActivityGroup).Name;
                _dp = BasicLookUpObject.get_BaseObjectMap(className);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrationModeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "UniqueName", System.Data.SqlDbType.NVarChar, 100);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "FormId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsPublished", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsArchived", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsInviteOnly", System.Data.SqlDbType.Bit);
				_dp.Add(className, "HasLotteryRun", System.Data.SqlDbType.Bit);
				_dp.Add(className, "AccountName", typeof(Account).Name, "Name", System.Data.SqlDbType.NVarChar, "Account");
				
                return _dp;
            }
        }

        protected static ActivityGroup LoadObject(System.Data.DataRow row)
        {
            ActivityGroup obj = new ActivityGroup();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivityGroup).Name);
				dq.Joins.Add(Account.ObjectMap["ID"], ActivityGroup.ObjectMap["AccountId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Account");
				
                return dq;
            }
        }

        public static ActivityGroup Load(int id, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<ActivityGroup> Load(Hashtable criterias = null, int? start = null, int? length = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return ActivityGroup.BaseLoad(criterias, ObjectMap, dq).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return ActivityGroup.BaseGetCount(criterias, ObjectMap, dq);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Save2(DatabaseQuery, dc, ObjectMap, typeof(ActivityGroup).Name);
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(ActivityGroup).Name);
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
			dps.Add(ObjectMap["UniqueName"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["FormId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsPublished"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsArchived"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsInviteOnly"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["HasLotteryRun"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public ActivityGroup Reload(BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return ActivityGroup.Load(this.ID.Value, dc);
		}
		
		public static List<ActivityGroup> LoadByAccountId(int AccountId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityGroup> LoadByAccountIdByIsPublished(int AccountId, bool IsPublished, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["IsPublished"], IsPublished);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<ActivityGroup> LoadByAccountIdByIsPublishedByIsInviteOnly(int AccountId, bool IsPublished, bool IsInviteOnly, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["IsPublished"], IsPublished);
			dq.Criterias.Add(ObjectMap["IsInviteOnly"], IsInviteOnly);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static ActivityGroup LoadByAccountIdByUniqueName(int AccountId, string UniqueName, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["UniqueName"], UniqueName);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static ActivityGroup LoadByUid(Guid Uid, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static ActivityGroup LoadByUidByIsPublished(Guid Uid, bool IsPublished, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["IsPublished"], IsPublished);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static ActivityGroup LoadByUidByIsPublishedByIsInviteOnly(Guid Uid, bool IsPublished, bool IsInviteOnly, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["IsPublished"], IsPublished);
			dq.Criterias.Add(ObjectMap["IsInviteOnly"], IsInviteOnly);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public string HomePageUrl
        {
            get
            {
                return this.GetAccount().HomePageUrl + "?group=" + this.Uid.ToString();
            }
        }

        public bool IsPublishable
        {
            get
            {
                return !IsArchived && !this.GetActivities().Where(x => !x.ActivityScheduleId.HasValue).Any();
            }
        }

        public bool IsPubliclyViewable
        {
            get
            {
                return IsPublished && !IsArchived;
            }
        }

        public SystemMessages SetIsPublished(bool isPublished, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            this.IsPublished = isPublished;
            this.LastModifiedBy = loggedInUserId;
            this.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(this.Save(dc));

            return msgs;
        }

        public static List<ActivityGroup> LoadByAccountId(int AccountId, int? start, int? length, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<ActivityGroup> LoadByAccountId(int AccountId, bool isArchived, int? start, int? length, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["isArchived"], isArchived);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static ActivityGroup LoadByAccountIdByName(int AccountId, string Name, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["Name"], Name);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static bool ExistsByAccountIdByName(int AccountId, string Name, Guid existingUid, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["Name"], Name);
            dq.Criterias.Add(ObjectMap["Uid"], existingUid, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.NotEqualTo);
            return dq.GetCount() > 0;
        }

        public static string GenerateUniqueName(int accountId, string name, DatabaseConnection dc = null)
        {
            var reg = new System.Text.RegularExpressions.Regex(BasicLibrary.Utilities.UrlFriendlyRegEx, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

            string origUniqueName = reg.Replace(name, "").MaxChars(25);
            string uniqueName = origUniqueName;
            var duplicate = ActivityGroup.LoadByAccountIdByUniqueName(accountId, uniqueName, dc);
            int count = 1;
            while (!duplicate.IsEmpty() && duplicate.ID != accountId)
            {
                uniqueName = origUniqueName + count.ToString();
                count++;
                duplicate = ActivityGroup.LoadByAccountIdByUniqueName(accountId, uniqueName, dc);
            }
            return uniqueName;            
        }

        private List<RegistrationPeriod> _RegistrationPeriods = null;
        public List<RegistrationPeriod> GetRegistrationPeriods(DatabaseConnection dc = null)
        {
            if (_RegistrationPeriods.IsEmpty())
            {
                _RegistrationPeriods = RegistrationPeriod.LoadByActivityGroupId(this.ID.Value, dc);
            }
            return _RegistrationPeriods;
        }

        public List<Registration> GetRegistrations(int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return Registration.LoadByActivityGroupId(this.ID.Value, start, length, dc);
        }

        public int GetRegistrationCount(DatabaseConnection dc = null)
        {
            return Registration.GetCount(criterias: new Hashtable() { { "ActivityGroupId", this.ID.Value } }, dc: dc);
        }

        private List<Activity> _Activities = null;
        public List<Activity> GetActivities(DatabaseConnection dc = null)
        {
            return GetActivities(false, false, null, null, false, dc);
        }
        public List<Activity> GetActivities(bool? isArchived, bool? isDeleted, int? start, int? length, bool forceReload = false, DatabaseConnection dc = null)
        {
            if (_Activities.IsEmpty() || forceReload)
            {
                _Activities = Activity.LoadByAccountId(this.AccountId.Value, this.ID.Value, IsArchived, isDeleted, start, length, dc);
            }
            return _Activities;
        }

        private Form _Form = null;
        public Form GetForm(DatabaseConnection dc = null)
        {
            if (_Form.IsEmpty())
            {
                _Form = Form.Load(this.FormId.Value, dc);
            }
            return _Form;
        }

        public List<RegistrantActivity> _RegistrantActivities = null;
        public List<RegistrantActivity> GetRegistrantActivities(List<Registration> registrations = null, DatabaseConnection dc = null)
        {
            if (registrations.IsEmpty())
            {
                registrations = Registration.LoadByActivityGroupId(this.ID.Value, dc);
            }
            if (_RegistrantActivities.IsEmpty())
            {
                if (registrations.Any())
                {
                    _RegistrantActivities = RegistrantActivity.LoadByRegistrationIds(registrations.Select(x => x.ID.Value).ToList(), dc).Where(x => x.ActivityIsDeleted == false).ToList();
                }
                else
                {
                    _RegistrantActivities = new List<RegistrantActivity>();
                }
            }
            return _RegistrantActivities;
        }

        public List<Registrant> GetRegistrants(List<Registration> registrations = null, DatabaseConnection dc = null)
        {
            return GetRegistrantActivities(registrations, dc).Select(x => x.ToRegistrant()).DistinctBy(x => x.ID.Value).ToList();
        }

        public int GetTotalSeatsLeft(DatabaseConnection dc = null)
        {
            return this.GetActivities(false, false, null, null, false, dc: dc).Sum(x => x.SeatsAvailable.Value) - this.GetSeatCountByAssignmentType(RegistrationAssignmentType.Reserved, dc);
        }

        public int GetSeatCountByAssignmentType(RegistrationAssignmentType rat, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByAssignmentType(rat: rat, activityGroupId: this.ID.Value, dc: dc);
        }

        public List<RegistrantAssignment> RandomizePlacementAttempt(RegistrationPeriod period, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var rr = new RegistrationRandomization(this);
            rr.RegistrationPeriod = period;
            return rr.AttemptLottery(msgs, dc);
        }

        public SystemMessages RandomizePlacementApply(RegistrationPeriod period, List<RegistrantAssignment> assignments, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var rr = new RegistrationRandomization(this);
            rr.RegistrationPeriod = period;
            msgs.AddRange(rr.ApplyLottery(assignments, dc));
            return msgs;
        }

        public SystemMessages RandomizePlacementReset(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, BasicLibrary.DataAccess.DatabaseConnection adc) =>
                {
                    var ras = this.GetRegistrantActivities(dc: adc);
                    foreach (var ra in ras)
                    {
                        amsgs.AddRange(ra.ChangeSeatAssignment(RegistrationAssignmentType.Application, adc));
                    }
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

            return msgs;
        }

        public int GetActivityCount(bool? isArchived, bool? isDeleted, DatabaseConnection dc = null)
        {
            if (this.ID.HasValue)
            {
                return Activity.GetCountByAccountId(this.AccountId.Value, this.ID, isArchived, isDeleted, dc);
            }
            return 0;
        }

        private Account _Account = null;
        public Account GetAccount(DatabaseConnection dc = null)
        {
            if (_Account.IsEmpty())
            {
                _Account = Account.Load(this.AccountId.Value, dc);
            }
            return _Account;
        }

        public RegistrationPeriod GetCurrentOrNextRegistrationPeriod(DatabaseConnection dc = null)
        {
            var period = this.GetCurrentRegistrationPeriod(dc);
            if (period.IsEmpty())
            {
                return GetNextRegistrationPeriod(dc);
            }
            return period;
        }

        public RegistrationPeriod GetNextRegistrationPeriod(DatabaseConnection dc = null)
        {
            var now = this.GetAccount(dc).ConvertToAccountTimeZone(DateTime.UtcNow);
            return this.GetRegistrationPeriods(dc).Where(x => now < x.StartDateTime.Value).OrderBy(x => x.StartDateTime.Value).FirstOrDefault();
        }

        public RegistrationPeriod GetCurrentRegistrationPeriod(DatabaseConnection dc = null)
        {
            var now = this.GetAccount(dc).ConvertToAccountTimeZone(DateTime.UtcNow);
            return this.GetRegistrationPeriods(dc).Where(x => x.StartDateTime.Value <= now && now <= x.EndDateTime.Value).FirstOrDefault();
        }

        public bool IsOpenNow(DatabaseConnection dc = null)
        {
            var now = this.GetAccount(dc).ConvertToAccountTimeZone(DateTime.UtcNow);
            return this.GetRegistrationPeriods(dc).Where(x => x.StartDateTime.Value <= now && now <= x.EndDateTime.Value).Count() > 0;
        }

        public bool IsClosedNow(DatabaseConnection dc = null)
        {
            return !IsOpenNow(dc);
        }

        public bool WillOpen(DatabaseConnection dc = null)
        {
            if (!IsOpenNow(dc))
            {
                var periods = this.GetRegistrationPeriods(dc);
                var now = this.GetAccount(dc).ConvertToAccountTimeZone(DateTime.UtcNow);
                return periods.Where(x => now < x.StartDateTime.Value).Count() > 0;
            }
            return false;
        }

        public TimeSpan WillOpenIn(DatabaseConnection dc = null)
        {
            if (WillOpen(dc))
            {
                var now = this.GetAccount(dc).ConvertToAccountTimeZone(DateTime.UtcNow);
                var periods = this.GetRegistrationPeriods(dc);
                var period = periods.Where(x => x.StartDateTime.Value >= now && now <= x.EndDateTime.Value).OrderBy(x => x.StartDateTime.Value).FirstOrDefault();
                return period.StartDateTime.Value - now;
            }
            else
            {
                return new TimeSpan();
            }
        }

        public RegistrantActivityGroup InviteContact(Registrant contact, int? periodId, int invitationSeats, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if(msgs.IsEmpty()) msgs = new SystemMessages();
            var rag = RegistrantActivityGroup.LoadByActivityGroupIdByRegistrantId(this.ID.Value, contact.ID.Value, dc);

            if (rag.IsEmpty())
            {
                rag = new RegistrantActivityGroup();
                rag.CreateDate = DateTime.UtcNow;
                rag.CreatedBy = loggedInUserId;
                rag.ActivityGroupId = this.ID;
                rag.RegistrantId = contact.ID;
            }
            rag.NumberOfInvitationSeats = invitationSeats;
            rag.NumberOfSeatsDeclined = 0;
            rag.RegistrationPeriodId = periodId;
            rag.LastModifiedDate = DateTime.UtcNow;
            rag.LastModifiedBy = loggedInUserId;
            msgs.AddRange(rag.Save(dc));
            return rag;
        }

        public TimeSpan WillCloseIn(DatabaseConnection dc = null)
        {
            if (IsOpenNow(dc))
            {
                var now = this.GetAccount(dc).ConvertToAccountTimeZone(DateTime.UtcNow);
                return this.GetCurrentRegistrationPeriod(dc).EndDateTime.Value - now;
            }
            else
            {
                return new TimeSpan();
            }
        }

        public static ActivityGroup SaveActivityGroup(int loggedInUserId, Account account, string activityGroupUid, System.Collections.Specialized.NameValueCollection values, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var name = values["name"];
            var regmode = values["regmode"];
            var form = values["form"];
            var description = values["description"];
            var isinviteonly = values["isinviteonly"];

            if (name.IsEmpty())
            {
                msgs.Add("Label is required", MessageType.UserError);
            }
            else if (ActivityGroup.ExistsByAccountIdByName(account.ID.Value, name, activityGroupUid.ToGuid()))
            {
                msgs.Add("Label is already in use", MessageType.UserError);
            }
            if (regmode.IsEmpty())
            {
                msgs.Add("Registration Mode is required", MessageType.UserError);
            }
            if (form.IsEmpty())
            {
                msgs.Add("Form is required", MessageType.UserError);
            }

            var group = ActivityGroup.LoadByUid(activityGroupUid.ToGuid());
            var isNew = false;
            if (group.IsEmpty())
            {
                group = new ActivityGroup();
                group.CreateDate = DateTime.UtcNow;
                group.CreatedBy = loggedInUserId;
                group.Uid = activityGroupUid.ToGuid();
                isNew = true;
            }

            if (name.IsNotEmpty()) group.Name = name.MaxChars(50);
            if (description.IsNotEmpty()) group.Description = description.MaxChars(400);
            if (regmode.IsNumber()) group.RegistrationModeId = (RegistrationMode)regmode.ToInteger();
            if (form.IsNumber()) group.FormId = form.ToInteger();

            group.IsInviteOnly = false;
            if (isinviteonly.IsNotEmpty()) group.IsInviteOnly = isinviteonly.ToBoolean();

            if (msgs.HasNoErrors())
            {
                var dc = Database.StartTransaction();

                try
                {
                    if (form.ToInteger() == 0)
                    {
                        var defaultForm = account.GetDefaultForm(dc);
                        var newForm = defaultForm.Copy(loggedInUserId, account.ID.Value, msgs, dc);
                        newForm.Name = (group.Name + " Form").MaxChars(100, "...");
                        msgs.AddRange(newForm.Save(dc));

                        group.FormId = newForm.ID;
                    }

                    group.AccountId = account.ID;
                    group.UniqueName = ActivityGroup.GenerateUniqueName(group.AccountId.Value, group.Name, dc);
                    group.LastModifiedDate = DateTime.UtcNow;
                    group.LastModifiedBy = loggedInUserId;
                    msgs.AddRange(group.Save(dc));

                    var activities = group.GetActivities(false, false, null, null, false, dc: dc);
                    foreach (var activity in activities)
                    {
                        activity.RegistrationModeId = group.RegistrationModeId;
                        msgs.AddRange(activity.Save(dc));
                    }

                    if (isNew)
                    {
                        group.AddOrUpdateGroupProperty(AccountPropertyType.LandingPageText, "Please register below.", loggedInUserId, msgs, dc);
                        group.AddOrUpdateGroupProperty(AccountPropertyType.RegistrationThankYouEmail, "Thank you for registering.", loggedInUserId, msgs, dc);
                        group.AddOrUpdateGroupProperty(AccountPropertyType.ApplicationThankYouEmail, "Thank you for applying.", loggedInUserId, msgs, dc);
                        group.AddOrUpdateGroupProperty(AccountPropertyType.NoActivitiesText, "Registration is now closed.", loggedInUserId, msgs, dc);
                    }
                }
                catch (Exception ex)
                {
                    msgs.Add(ex.Message, MessageType.Error, ex);
                }
                finally
                {
                    Database.EndTransaction(dc, msgs);
                }

            }

            return group;
        }

        public AccountProperty AddOrUpdateGroupProperty(AccountPropertyType type, string value, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var ap = this.GetGroupProperty(type, dc);

            if (ap.IsEmpty())
            {
                ap = new AccountProperty();
                ap.ActivityGroupId = this.ID.Value;
                ap.AccountPropertyTypeId = type;
                ap.CreateDate = DateTime.UtcNow;
                ap.CreatedBy = loggedInUserId;
            }

            ap.Value = value;
            ap.LastModifiedDate = DateTime.UtcNow;
            ap.LastModifiedBy = loggedInUserId;
            msgs.AddRange(ap.Save(dc));

            return ap;
        }

        public AccountProperty GetGroupProperty(AccountPropertyType propertyType, DatabaseConnection dc = null)
        {
            return GetGroupProperties(dc).Find(x => x.AccountPropertyTypeId == propertyType);
        }

        private List<AccountProperty> _Properties = null;
        public List<AccountProperty> GetGroupProperties(DatabaseConnection dc = null)
        {
            if (_Properties.IsEmpty())
            {
                _Properties = AccountProperty.LoadByActivityGroupId(this.ID.Value, dc);
            }
            return _Properties;
        }

        private List<ActivityMessage> _ActivityMessages = null;
        public List<ActivityMessage> GetMessages(DatabaseConnection dc = null)
        {
            if (_ActivityMessages.IsEmpty())
            {
                _ActivityMessages = ActivityMessage.LoadByActivityGroupId(this.ID.Value, dc);
            }
            return _ActivityMessages;
        }

        public ActivityMessage AddMessage(string subject, string body, List<int> registrantIds, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            ActivityMessage am = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages smsgs, DatabaseConnection adc) =>
                {
                    am = new ActivityMessage();
                    am.ActivityGroupId = this.ID;
                    am.AccountId = this.AccountId;
                    am.Subject = subject.MaxChars(150);
                    am.Body = body;
                    am.CreateDate = DateTime.UtcNow;
                    am.CreatedBy = loggedInUserId;
                    msgs.AddRange(am.Save(dc));

                    if (msgs.HasNoErrors())
                    {
                        foreach (var registrantId in registrantIds)
                        {
                            var amr = new ActivityMessageRegistrant();
                            amr.RegistrantId = registrantId;
                            amr.ActivityMessageId = am.ID;
                            amr.CreateDate = DateTime.UtcNow;
                            amr.CreatedBy = loggedInUserId;
                            msgs.AddRange(amr.Save(dc));
                        }
                    }
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
                     
            return am;
        }

        public SystemMessages MoveToArchive(int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            this.IsArchived = true;
            this.LastModifiedDate = DateTime.UtcNow;
            this.LastModifiedBy = loggedInUserId;
            msgs.AddRange(this.Save(dc));

            return msgs;
        }

        public List<ActivitySchedule> GetSchedules(DatabaseConnection dc = null)
        {
            return ActivitySchedule.LoadByActivityGroupId(this.ID.Value, dc);
        }

        public SystemMessages MoveFromArchive(int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            this.IsArchived = false;
            this.LastModifiedDate = DateTime.UtcNow;
            this.LastModifiedBy = loggedInUserId;
            msgs.AddRange(this.Save(dc));
            return msgs;
        }

        public SystemMessages InviteContacts(List<Registrant> contacts, RegistrationPeriod period, List<int> activityIds, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            // add contacts
            foreach (var contact in contacts)
            {
                var rag = this.InviteContact(contact, period?.ID, 1, loggedInUserId, msgs, dc);
                if (activityIds.Any())
                {
                    msgs.AddRange(contact.AddInviteToActivities(activityIds, loggedInUserId, this.ID, period?.ID, dc));
                }
            }

            return msgs;
        }

        public static SystemMessages EmailInvitedContacts(int id, int periodId, List<int> contactIds, string emailmessage, string inviteUrl, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.Load(id, dc);
            var account = group.GetAccount(dc);
            var period = RegistrationPeriod.Load(periodId, dc);
            var loggedInUser = WebUser.Load(loggedInUserId, dc);
            var contacts = Registrant.LoadList(contactIds, dc);

            if (contacts.IsNotEmpty() && contacts.Any())
            {
                var subject = "";
                var body = "";

                subject = string.Format("You have been invited to register for {0}", group.Name);

                var regOpenText = "";
                if (period.IsNotEmpty())
                {
                    if (period.IsOpen(dc))
                    {
                        regOpenText = "Registration is open now.";
                    }
                    else if (group.WillOpen(dc))
                    {
                        regOpenText = "Registration will open " + period.StartDateTime.ToShortDateString() + ".";
                    }
                }

                var acceptInviteText = "To accept this invite and register, please click the following link: <a href=\"{0}\">{0}</a>";
                var declineInviteText = "<p>To decline this invite, please click the following link: <a href=\"{0}\">{0}</a></p>";
                var userSignature = string.Format("<p>Regards,<br />{0}</p>", loggedInUser.DisplayName);
                var declineUrl = "";

                body = "{0}{1}<p>{2} {3}</p>{4}{5}{6}";

                foreach (var contact in contacts)
                {
                    var finalInviteUrl = inviteUrl;
                    var rag = RegistrantActivityGroup.LoadByActivityGroupIdByRegistrantId(group.ID.Value, contact.ID.Value, dc);

                    var salutation = "<p>Hello,</p>";
                    if (contact.FirstName.IsNotBlank())
                        salutation = string.Format("<p>Hello {0},</p>", contact.FirstName);

                    finalInviteUrl += (finalInviteUrl.Contains("?") ? "&" : "?") + "invite=" + rag.Uid.ToString();
                    declineUrl = finalInviteUrl + "&declined=1";

                    var acceptInviteTextWithLink = string.Format(acceptInviteText, finalInviteUrl);
                    var declineInviteTextWithLink = string.Format(declineInviteText, declineUrl);

                    SendGridHelper.SendMail(
                        fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
                            Email.AppEmailAddress, 
                            string.Format("{0} via {1}", account.Name, 
                            Utilities.AppName(account.IsAfterSchool))),
                        toEmail: new SendGrid.Helpers.Mail.EmailAddress(contact.Email, contact.Name),
                        subject: subject,
                        body: string.Format(
                            body,
                            salutation,
                            emailmessage,
                            regOpenText,
                            acceptInviteTextWithLink,
                            declineInviteTextWithLink,
                            userSignature,
                            Email.GetAppSignature(account.IsAfterSchool, "invite_contacts")),
                        replyTo: new SendGrid.Helpers.Mail.EmailAddress(account.Email, account.Name),
                        emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                    );
                }
            }

            return msgs;
        }

        public List<RegistrantActivityGroup> GetInvitedContacts(int? start, int? length, DatabaseConnection dc = null)
        {
            return RegistrantActivityGroup.LoadByActivityGroupId(this.ID.Value, start, length, dc);
        }

        public int GetInvitedContactCount(DatabaseConnection dc = null)
        {
            return RegistrantActivityGroup.GetCount(
                    criterias: new Hashtable() { { "ActivityGroupId", this.ID.Value } },
                    dc: dc
                ); 
        }

        public static SystemMessages SendRegistrationConfirmations(int id, List<Registration> registrations = null, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.Load(id, dc);

            if (registrations.IsEmpty())
                registrations = group.GetRegistrations(dc: dc);

            var firstReg = registrations.First();

            // send one bcc for first reg
            registrations.ForEach(reg => msgs.AddRange(Registration.SendConfirmation(reg.ID.Value, firstReg.ID == reg.ID, null)));

            return msgs;
        }

        public ActivityGroup Copy(string newName, bool useSameForm, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            ActivityGroup group = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // copy group
                    group = new ActivityGroup();
                    group.AccountId = this.AccountId;
                    group.CreateDate = group.LastModifiedDate = DateTime.UtcNow;
                    group.CreatedBy = group.LastModifiedBy = loggedInUserId;
                    group.Description = this.Description;
                    group.FormId = this.FormId;
                    group.IsArchived = this.IsArchived;
                    group.IsInviteOnly = this.IsInviteOnly;
                    group.IsPublished = false;
                    group.Name = newName;
                    group.RegistrationModeId = this.RegistrationModeId;
                    group.UniqueName = ActivityGroup.GenerateUniqueName(this.AccountId.Value, newName, adc);
                    msgs.AddRange(group.Save(adc));

                    // copy AccountProperties / Content
                    var properties = this.GetGroupProperties(adc);
                    foreach(var item in properties)
                    {
                        var newprop = new AccountProperty();
                        newprop.AccountPropertyTypeId = item.AccountPropertyTypeId;
                        newprop.ActivityGroupId = group.ID;
                        newprop.CreateDate = newprop.LastModifiedDate = DateTime.UtcNow;
                        newprop.CreatedBy = newprop.LastModifiedBy = loggedInUserId;
                        newprop.Value = item.Value;
                        msgs.AddRange(newprop.Save(adc));
                    }

                    // activities
                    var activities = this.GetActivities(adc);
                    foreach(var activity in activities)
                    {
                        var newact = activity.Copy(activity.Name, true, loggedInUserId, amsgs, adc);
                        newact.ActivityGroupId = group.ID;
                        newact.ActivityScheduleId = null;
                        newact.IsPublished = true;
                        msgs.AddRange(newact.Save(adc));
                    }
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

            return group;
        }

        #endregion
    }
}

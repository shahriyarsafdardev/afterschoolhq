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
	[Serializable]
    public class Account : BaseCompany
    {
		#region "ClassGenerator"

		public string StripeAccessToken { get { return (string)MDV["StripeAccessToken"].Value; } set { MDV["StripeAccessToken"].Value = value; } }
		public string StripePublishableKey { get { return (string)MDV["StripePublishableKey"].Value; } set { MDV["StripePublishableKey"].Value = value; } }
		public string StripeSecretKey { get { return (string)MDV["StripeSecretKey"].Value; } set { MDV["StripeSecretKey"].Value = value; } }
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public string UniqueName { get { return (string)MDV["UniqueName"].Value; } set { MDV["UniqueName"].Value = value; } }
		public int? OwnerId { get { return (int?)MDV["OwnerId"].Value; } set { MDV["OwnerId"].Value = value; } }
		public bool IsAdminAccount { get { return (bool)MDV["IsAdminAccount"].Value; } set { MDV["IsAdminAccount"].Value = value; } }
		public bool IsActive { get { return (bool)MDV["IsActive"].Value; } set { MDV["IsActive"].Value = value; } }
		public bool IsAfterSchool { get { return (bool)MDV["IsAfterSchool"].Value; } set { MDV["IsAfterSchool"].Value = value; } }
		public string StripeCustomerId { get { return (string)MDV["StripeCustomerId"].Value; } set { MDV["StripeCustomerId"].Value = value; } }
		public string StripeDefaultCardId { get { return (string)MDV["StripeDefaultCardId"].Value; } set { MDV["StripeDefaultCardId"].Value = value; } }
		public int? DefaultFormId { get { return (int?)MDV["DefaultFormId"].Value; } set { MDV["DefaultFormId"].Value = value; } }
		public string PayPalEmailAddress { get { return (string)MDV["PayPalEmailAddress"].Value; } set { MDV["PayPalEmailAddress"].Value = value; } }
		public string StripeRefreshToken { get { return (string)MDV["StripeRefreshToken"].Value; } set { MDV["StripeRefreshToken"].Value = value; } }
		public string StripeUserId { get { return (string)MDV["StripeUserId"].Value; } set { MDV["StripeUserId"].Value = value; } }
		public string TimeZone { get { return (string)MDV["TimeZone"].Value; } set { MDV["TimeZone"].Value = value; } }
		public string LogoUrl { get { return (string)MDV["LogoUrl"].Value; } set { MDV["LogoUrl"].Value = value; } }
		public string LandingPageText { get { return (string)MDV["LandingPageText"].Value; } set { MDV["LandingPageText"].Value = value; } }
		public AccountType AccountTypeId { get { return (AccountType)MDV["AccountTypeId"].Value; } set { MDV["AccountTypeId"].Value = value; } }
		

        public Account()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Account).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.Uid = Guid.NewGuid();
			this.IsAdminAccount = false;
			this.IsActive = false;
			this.IsAfterSchool = false;
			this.AccountTypeId = AccountType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(Account).Name;
                _dp = BaseCompany.get_BaseObjectMap(className);
				_dp.Add(className, "StripeAccessToken", System.Data.SqlDbType.NVarChar, 100);
				_dp.Add(className, "StripePublishableKey", System.Data.SqlDbType.NVarChar, 100);
				_dp.Add(className, "StripeSecretKey", System.Data.SqlDbType.NVarChar, 100);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "UniqueName", System.Data.SqlDbType.NVarChar, 50);
				_dp.Add(className, "OwnerId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsAdminAccount", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsActive", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsAfterSchool", System.Data.SqlDbType.Bit);
				_dp.Add(className, "StripeCustomerId", System.Data.SqlDbType.NVarChar, 50);
				_dp.Add(className, "StripeDefaultCardId", System.Data.SqlDbType.NVarChar, 50);
				_dp.Add(className, "DefaultFormId", System.Data.SqlDbType.Int);
				_dp.Add(className, "PayPalEmailAddress", System.Data.SqlDbType.NVarChar, 100);
				_dp.Add(className, "StripeRefreshToken", System.Data.SqlDbType.NVarChar, 100);
				_dp.Add(className, "StripeUserId", System.Data.SqlDbType.NVarChar, 100);
				_dp.Add(className, "TimeZone", System.Data.SqlDbType.NVarChar, 30);
				_dp.Add(className, "LogoUrl", System.Data.SqlDbType.NVarChar, 400);
				_dp.Add(className, "LandingPageText", System.Data.SqlDbType.NVarChar, 4000);
				_dp.Add(className, "AccountTypeId", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        protected static Account LoadObject(System.Data.DataRow row)
        {
            Account obj = new Account();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(Account).Name);
				
                return dq;
            }
        }

        public static Account Load(int id, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Account> Load(Hashtable criterias = null, int? start = null, int? length = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return Account.BaseLoad(criterias, ObjectMap, dq).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return Account.BaseGetCount(criterias, ObjectMap, dq);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.Uid.IsEmpty())
				msgs.Add("Uid is required on Account object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Account).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Account).Name);
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
			dps.Add(ObjectMap["StripeAccessToken"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StripePublishableKey"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StripeSecretKey"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["UniqueName"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["OwnerId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsAdminAccount"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsActive"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsAfterSchool"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StripeCustomerId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StripeDefaultCardId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DefaultFormId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["PayPalEmailAddress"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StripeRefreshToken"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StripeUserId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["TimeZone"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LogoUrl"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LandingPageText"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Account Reload(BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Account.Load(this.ID.Value, dc);
		}
		
		public static List<Account> LoadByOwnerId(int OwnerId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["OwnerId"], OwnerId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Account LoadByUniqueName(string UniqueName, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["UniqueName"], UniqueName);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static Account LoadByIsAdminAccount(bool IsAdminAccount, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["IsAdminAccount"], IsAdminAccount);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static Account LoadByUid(Guid Uid, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<Account> LoadByAccountTypeId(AccountType AccountTypeId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountTypeId"], AccountTypeId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		

        #endregion

		#region "UserDefined"
        public string AccountNumber
        {
            get
            {
                if (ID.HasValue)
                {
                    return ID.Value.ToString().PadLeft(7, '0');
                }
                else
                {
                    return "";
                }
            }
        }

        public string HomePageUrl
        {
            get
            {
                return string.Format("{0}/{1}", Utilities.BaseUrl(this.IsAfterSchool), this.UniqueName);
            }

        }

        public bool HasPaymentCard
        {
            get
            {
                return this.StripeDefaultCardId.IsNotEmpty();
            }
        }

        public string TimeZoneAbbr
        {
            get
            {
                if (TimeZone.IsNotBlank())
                {
                    return TimeZone.Split('-')[1].Trim();
                }
                else
                {
                    return "";
                }
            }
        }

        public bool HasFeature(FeatureType featureType, DatabaseConnection dc = null)
        {
            return this.GetActiveAccountPackage(dc).HasFeature(featureType);
        }

        public WebUser GetAccountOwner(DatabaseConnection dc = null)
        {
            return WebUser.Load(this.OwnerId.Value, dc);
        }

        public List<WebUser> GetAccountOwnersAndAdmins(DatabaseConnection dc = null)
        {
            var awus = AccountWebUser.LoadByAccountIdByIsActive(this.ID.Value, true, dc);
            return awus.Where(x => x.UserTypeId == UserType.Administrator || x.UserTypeId == UserType.Owner).Select(x => x.ToWebUser()).ToList();
        }

        public List<Activity> GetPublishedActivities(DatabaseConnection dc = null)
        {

            var periods = this.GetOpenAndUpcomingRegistrationPeriods(dc);

            List<Activity> activities = new List<Activity>();
            foreach(var period in periods)
            {
                activities.AddRange(period.GetActivities(dc).Where(x => x.IsPubliclyViewable));
            }
            var list = new List<Activity>();

            foreach(var activity in activities)
            {
                var mainActivity = activity;

                if (activity.IsChildActivity)
                {
                    var parentActivities = activities.Where(x => x.ID.Value == activity.ParentActivityId.Value);
                    if (parentActivities.Count() > 0)
                    {
                        mainActivity = parentActivities.First();
                    }
                    else
                    {
                        mainActivity = activity.GetParentActivity(dc);
                    }
                }

                var af = mainActivity.GetActiveActivityForm(dc);
                if(af.IsNotEmpty())
                {
                    list.Add(activity);
                }
            }

            return list;
        }

        public List<ActivityForm> GetActivityForms(DatabaseConnection dc = null)
        {
            return ActivityForm.LoadByAccountId(this.ID.Value, dc);
        }

        public List<Activity> GetActivities(UserType userType, WebUser user, bool? isArchived, bool? isDeleted, int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            if (user.IsEmpty() || new UserType[] { UserType.Administrator, UserType.Owner, UserType.SuperAdmin }.Contains(userType))
            {
                return Activity.LoadByAccountId(this.ID.Value, null, isArchived, isDeleted, start, length, dc).ToList();
            }
            else
            {
                var activities = Activity.LoadByAccountId(this.ID.Value, null, isArchived, isDeleted, start, length, dc).ToList();
                var list = new List<Activity>();
                foreach (var activity in activities)
                {
                    if (user.HasAccessToActivity(activity, dc))
                    {
                        list.Add(activity);
                    }
                }

                return list;
            }
        }

        public int GetActivityCount(UserType userType, WebUser user, bool? isArchived, bool? isDeleted, DatabaseConnection dc = null)
        {
            if (user.IsEmpty() || new UserType[] { UserType.Administrator, UserType.Owner, UserType.SuperAdmin }.Contains(userType))
            {
                return Activity.GetCountByAccountId(this.ID.Value, null, isArchived, isDeleted, dc);
            }
            else
            {
                return GetActivities(userType, user, isArchived, isDeleted, dc: dc).Count();
            }
        }

        public int SearchContactCount(string searchtext, int? contactListId, DatabaseConnection dc = null)
        {
            return Registrant.SearchRegistrantCount(searchtext, contactListId, this.ID.Value, dc);
        }

        public List<Registrant> SearchContacts(string searchtext, int? contactListId, int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return Registrant.SearchRegistrants(searchtext, contactListId, this.ID.Value, start, length, dc);
        }

        public int GetContactCount(int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return Registrant.GetCountByAccountId(this.ID.Value, dc);
        }

        public List<Registrant> GetContacts(int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return Registrant.LoadByAccountId(this.ID.Value, start, length, dc);
        }

        public List<ContactList> GetContactLists(int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return ContactList.LoadByAccountId(this.ID.Value, start, length, dc);
        }

        public int GetContactListCount(DatabaseConnection dc = null)
        {
            return ContactList.GetCount(criterias: new Hashtable() { { "AccountId", this.ID.Value } }, dc: dc);
        }

        public List<Registrant> GetRegistrantsByActivityId(int activityId, int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return this.GetRegistrantActivitiesByActivityId(activityId, start, length, dc).Select(x => x.ToRegistrant()).ToList();
        }

        public List<RegistrantActivity> GetRegistrantActivitiesByActivityId(int activityId, int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByActivityId(activityId, start, length, dc).Where(x => x.ActivityAccountId == this.ID).ToList();
        }

        public int GetRegistrantActivityCountByActivityId(int activityId, int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetCountByActivityIdByAccountId(activityId, this.ID.Value, dc);
        }

        public List<Form> GetForms(DatabaseConnection dc = null)
        {
            return Form.LoadByAccountId(this.ID.Value, dc);
        }

        public List<ActivityGroup> GetActivityGroups(DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountId(this.ID.Value, dc);
        }

        public List<ActivityGroup> GetPublishedActivityGroups(DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountIdByIsPublished(this.ID.Value, true, dc);
        }

        public List<ActivityGroup> GetActivityGroups(bool IsArchived, int? start, int? length, DatabaseConnection dc = null)
        {
            return ActivityGroup.LoadByAccountId(this.ID.Value, IsArchived, start, length, dc);
        }

        public int GetActivityGroupCount(bool IsArchived = false, DatabaseConnection dc = null)
        {
            return ActivityGroup.GetCount(new Hashtable() { { "AccountId", this.ID.Value }, { "IsArchived", IsArchived } }, dc);
        }

        public List<ActivitySchedule> GetActivitySchedules(DatabaseConnection dc = null)
        {
            return ActivitySchedule.LoadByAccountId(this.ID.Value, dc);
        }

        public List<RegistrationPeriod> GetRegistrationPeriods(DatabaseConnection dc = null)
        {
            return RegistrationPeriod.LoadByAccountId(this.ID.Value, dc);
        }

        public List<RegistrationPeriod> GetRegistrationPeriods(int? start, int? length, DatabaseConnection dc = null)
        {
            return RegistrationPeriod.LoadByAccountId(this.ID.Value, start, length, dc);
        }

        public int GetRegistrationPeriodCount(DatabaseConnection dc = null)
        {
            return RegistrationPeriod.GetCount(new Hashtable() { { "AccountId", this.ID.Value } }, dc);
        }

        public List<RegistrationPeriod> GetUpcomingRegistrationPeriods(DatabaseConnection dc = null)
        {
            return GetRegistrationPeriods().Where(x => x.StartDateTime.Value > this.ConvertToAccountTimeZone(DateTime.UtcNow)).OrderBy(x => x.StartDateTime.Value).ToList();
        }

        public List<RegistrationPeriod> GetOpenRegistrationPeriods(DatabaseConnection dc = null)
        {
            var periods = GetRegistrationPeriods();
            return periods.Where(x => x.StartDateTime.Value <= this.ConvertToAccountTimeZone(DateTime.UtcNow) && x.EndDateTime.Value >= this.ConvertToAccountTimeZone(DateTime.UtcNow)).ToList();
        }

        public List<RegistrationPeriod> GetClosedRegistrationPeriods(DatabaseConnection dc = null)
        {
            return GetRegistrationPeriods().Where(x => x.EndDateTime.Value < this.ConvertToAccountTimeZone(DateTime.UtcNow)).ToList();
        }

        public List<RegistrationPeriod> GetOpenAndUpcomingRegistrationPeriods(DatabaseConnection dc = null)
        {
            var periods = GetOpenRegistrationPeriods(dc);
            periods.AddRange(GetUpcomingRegistrationPeriods(dc));
            return periods;
        }

        public Form GetDefaultForm(DatabaseConnection dc = null)
        {
            int? defaultFormId = null;
            if (this.DefaultFormId.HasValue)
            {
                defaultFormId = this.DefaultFormId;
            }
            else
            {
                defaultFormId = Form.GetLatestTemplateForm(1, dc).ID;
            }
            return Form.Load(defaultFormId.Value, dc);
        }

        public List<AccountWebUser> GetMembers(DatabaseConnection dc = null)
        {
            return AccountWebUser.LoadByAccountId(this.ID.Value, dc);
        }

        public List<AccountWebUser> GetActiveMembers(DatabaseConnection dc = null)
        {
            return AccountWebUser.LoadByAccountIdByIsActive(this.ID.Value, true, dc);
        }

        public AccountPackage GetActiveAccountPackage(DatabaseConnection dc = null)
        {
            return AccountPackage.LoadByAccountIdByIsActive(this.ID.Value, true, dc);
        }

        public List<AccountPackage> GetAccountPackages(DatabaseConnection dc = null)
        {
            return AccountPackage.LoadByAccountId(this.ID.Value, dc);
        }

        public WebUser AddUser(WebUser wu, UserType ut, int loggedInUserId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return AddUser(wu.FirstName, wu.LastName, wu.Email, ut, loggedInUserId, msgs, dc);
        }

        public WebUser AddUser(string firstName, string lastName, string email, UserType ut, int loggedInUserId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return AddUser(firstName, lastName, email, null, false, true, false, ut, loggedInUserId, msgs, dc);
        }

        public WebUser AddUser(string firstName, string lastName, string email, string password, bool isActive, bool isRegistered, bool isVerified, UserType ut, int loggedInUserId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            if(msgs.IsEmpty()) msgs = new SystemMessages();
            bool doCommit = false;
            WebUser wu = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, BasicLibrary.DataAccess.DatabaseConnection adc) =>
                {
                    wu = WebUser.LoadByEmail(email.Trim(), adc);

                    if (wu.IsEmpty())
                    {
                        wu = new WebUser();
                        wu.FirstName = firstName;
                        wu.LastName = lastName;
                        wu.Email = email.Trim();
                        wu.Username = WebUser.GenerateUsername(firstName, lastName, adc);
                        if (!password.IsEmpty())
                        {
                            wu.Password = WebUser.EncryptPassword(password);
                        }
                        wu.CreateDate = wu.LastModifiedDate = DateTime.Now;
                        wu.CreatedBy = wu.LastModifiedBy = loggedInUserId;
                        wu.IsActive = isActive;
                        wu.IsRegistered = isRegistered;
                        wu.VerifiedAccount = isVerified;
                        wu.Uid = Guid.NewGuid();
                        amsgs.AddRange(wu.Save(adc));
                    }

                    AccountWebUser gwu = AccountWebUser.LoadByAccountIdByWebUserId(this.ID.Value, wu.ID.Value, adc);
                    if (gwu.IsEmpty())
                    {
                        gwu = new AccountWebUser();
                        gwu.CreateDate = DateTime.UtcNow;
                        gwu.CreatedBy = loggedInUserId;
                        gwu.WebUserId = wu.ID.Value;
                        gwu.AccountId = this.ID.Value;
                        gwu.IsDefault = false;
                        gwu.IsActive = isActive;
                    }
                    else
                    {
                        gwu.IsActive = true;
                    }
                    gwu.LastModifiedDate = DateTime.UtcNow;
                    gwu.LastModifiedBy = loggedInUserId;
                    gwu.UserTypeId = ut;

                    amsgs.AddRange(gwu.Save(adc));

                    if (ut == UserType.Owner)
                    {
                        this.OwnerId = wu.ID.Value;
                        amsgs.AddRange(this.Save(adc));
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

            return wu;
        }

        public SystemMessages SendlWelcomeEmailTrial(DatabaseConnection dc = null)
        {
            return EmailHelper.SendWelcomeTrial(this.GetAccountOwner(dc), this);
        }

        public SystemMessages SendWelcomeEmailPurchase(DatabaseConnection dc = null)
        {
            return EmailHelper.SendWelcomePurchase(this.GetAccountOwner(dc), this);
        }

        public SystemMessages InviteUser(WebUser wu, UserType ut, WebUser loggedInUser, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return InviteUser(wu.FirstName, wu.LastName, wu.Email, ut, loggedInUser, dc);
        }

        public SystemMessages InviteUser(string firstName, string lastName, string email, UserType ut, WebUser loggedInUser, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return InviteUser(firstName, lastName, email, null, ut, loggedInUser, dc);
        }

        public SystemMessages InviteUser(string firstName, string lastName, string email, string message, UserType ut, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            WebUser wu = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, BasicLibrary.DataAccess.DatabaseConnection adc) =>
                {
                    wu = WebUser.LoadByEmail(email.Trim(), adc);

                    if (wu.IsEmpty())
                    {
                        wu = new WebUser();
                        wu.FirstName = firstName;
                        wu.LastName = lastName;
                        wu.Email = email.Trim();
                        wu.CreateDate = wu.LastModifiedDate = DateTime.Now;
                        wu.CreatedBy = wu.LastModifiedBy = loggedInUser.ID.Value;
                        wu.IsActive = false;
                        wu.Uid = Guid.NewGuid();
                        amsgs.AddRange(wu.Save(adc));
                    }

                    var awu = AccountWebUser.LoadByAccountIdByWebUserId(this.ID.Value, wu.ID.Value, adc);
                    if (awu.IsEmpty())
                    {
                        awu = new AccountWebUser();
                        awu.CreateDate = DateTime.Now;
                        awu.CreatedBy = loggedInUser.ID.Value;
                        awu.WebUserId = wu.ID.Value;
                        awu.AccountId = this.ID.Value;
                        awu.IsDefault = true;                      
                    }
                    else
                    {
                        awu.IsActive = true;
                    }
                    awu.LastModifiedDate = DateTime.Now;
                    awu.LastModifiedBy = loggedInUser.ID.Value;
                    awu.UserTypeId = ut;

                    amsgs.AddRange(awu.Save(adc));

                    if (ut == UserType.Owner)
                    {
                        this.OwnerId = wu.ID.Value;
                        amsgs.AddRange(this.Save(adc));
                    }
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                    msgs.AddRange(wu.SendInvite(email.Trim(), message, loggedInUser, this, dc));
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                    msgs.Add("An invitation was not sent to " + email.Trim() + ", a system error has occurred", MessageType.UserError);
                },
                dc: dc
            );            

            return msgs;
        }

        public static string GenerateUniqueName(string name, int? accountId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            //unique name
            var reg = new System.Text.RegularExpressions.Regex(BasicLibrary.Utilities.UrlFriendlyRegEx, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);

            string origUniqueName = reg.Replace(name, "").MaxChars(25);
            string uniqueName = origUniqueName;
            Account duplicate = Account.LoadByUniqueName(uniqueName, dc);
            int count = 1;            
            while (duplicate.IsNotEmpty() && duplicate.ID != accountId)
            {
                uniqueName = origUniqueName + count.ToString();
                count++;
                duplicate = Account.LoadByUniqueName(uniqueName, dc);
            }
            return uniqueName;
        }

        private enum SignUpType
        {
            NotSet = 0,
            NewAccountSignUp,
            TrialSignUp,
            ConvertingFromTrial,
            ChangePackage,
            ChangePackageDuringTrial,
            ChangeBillingPeriodType
        }

        private SignUpType GetSignUpType(Package p, AccountPackage activePackage, BillingPeriodType billingPeriodType, ref int trialDayCount, DatabaseConnection dc = null)
        {
            var signupType = SignUpType.NotSet;

            // has payment info
            if (this.StripeDefaultCardId.IsNotEmpty())
            {
                // paying as soon as they sign up
                if (activePackage.IsEmpty())
                {
                    signupType = SignUpType.NewAccountSignUp;
                }
                // changing packages but have already paid, force those upgrading from trials to upgrade on current plan
                else if (activePackage.PackageId != p.ID)
                {
                    signupType = SignUpType.ChangePackage;
                }
                // if same package but the billing period is changing
                else if (activePackage.PackageId == p.ID && activePackage.BillingPeriodTypeId != billingPeriodType)
                {
                    signupType = SignUpType.ChangeBillingPeriodType;
                }
                // simply converting from the same package from trial
                else if (activePackage.TrialEndDate.HasValue)
                {
                    signupType = SignUpType.ConvertingFromTrial;
                }
            }
            else if (p.IsFree(billingPeriodType) || (activePackage.IsNotEmpty() && activePackage.IsFree))
            {
                // new sign up
                if (activePackage.IsEmpty())
                {
                    signupType = SignUpType.NewAccountSignUp;
                }
                // could be downgrading to a free account
                else if (activePackage.PackageId != p.ID)
                {
                    signupType = SignUpType.ChangePackage;
                }
            }
            else
            {
                if (activePackage.IsEmpty())
                {
                    trialDayCount = p.HasFeature(FeatureType.TrialDays) ? p.GetFeature(FeatureType.TrialDays, dc).GetMaxValue() : 0;
                }
                else if (activePackage.HasFeature(FeatureType.TrialDays))
                {
                    trialDayCount = this.GetTrialDaysRemaining(activePackage, dc);
                }

                if (trialDayCount > 0)
                {
                    // totally new
                    if (activePackage.IsEmpty())
                    {
                        signupType = SignUpType.TrialSignUp;
                    }
                    // still during the trial but changing packages with no new payment info
                    else
                    {
                        signupType = SignUpType.ChangePackageDuringTrial;
                    }
                }
            }

            return signupType;
        }

        public AccountPackage AddPackage(int package, BillingPeriodType billingPeriodType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if(msgs.IsEmpty()) msgs = new SystemMessages();

            Package p = null;
            AccountPackage currentPackage = null;
            AccountPackage newap = new AccountPackage();
            bool closeTransaction = false;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, BasicLibrary.DataAccess.DatabaseConnection adc) =>
                {
                    p = Package.Load(package, dc);
                    currentPackage = this.GetActiveAccountPackage(dc);

                    var trialDayCount = 0;

                    // is user:
                    //      paying for their Trial account
                    //      upgrading to a different package
                    //      paying for a new package with no trial account

                    var signupType = this.GetSignUpType(p, currentPackage, billingPeriodType, ref trialDayCount, dc);

                    if (signupType != SignUpType.NotSet)
                    {
                        if (signupType == SignUpType.ChangePackage ||
                            signupType == SignUpType.ChangeBillingPeriodType ||
                            signupType == SignUpType.ChangePackageDuringTrial)
                        {
                            //cancel existing package
                            msgs.AddRange(currentPackage.CancelPackage(dc));
                        }

                        if (msgs.HasNoErrors())
                        {
                            if (signupType != SignUpType.ConvertingFromTrial)
                            {
                                var newStripePlanId = p.GetStripePlanId(billingPeriodType);

                                //create new package
                                newap.AccountId = this.ID;
                                newap.PackageId = package;
                                newap.BillingPeriodTypeId = billingPeriodType;
                                newap.CreateDate = DateTime.UtcNow;
                                newap.CreatedBy = loggedInUser.ID;
                                newap.LastModifiedDate = DateTime.UtcNow;
                                newap.LastModifiedBy = loggedInUser.ID;
                                newap.IsActive = true;
                                newap.Price = billingPeriodType == BillingPeriodType.Month ? p.Price : p.PricePerYear;
                                newap.StripeSubscriptionId = currentPackage.IsNotEmpty() ? currentPackage.StripeSubscriptionId : null;

                                // set trial end date if there are still trial days
                                if (trialDayCount > 0)
                                {
                                    newap.TrialStartDate = DateTime.UtcNow;
                                    newap.TrialEndDate = DateTime.UtcNow.AddDays(trialDayCount);
                                }
                                else
                                {
                                    newap.StartDate = DateTime.UtcNow;
                                }

                                msgs.AddRange(newap.SaveWithStripe(this.StripeCustomerId, false, newStripePlanId, dc));
                            }
                            // if it has a trial date end in the future, then end it now if they are converting from trial
                            else if (currentPackage.TrialEndDate.HasValue && currentPackage.TrialEndDate.Value >= DateTime.Now)
                            {
                                currentPackage.TrialEndDate = DateTime.UtcNow;
                                currentPackage.LastModifiedDate = DateTime.UtcNow;
                                currentPackage.LastModifiedBy = loggedInUser.ID;

                                msgs.AddRange(currentPackage.SaveWithStripe(this.StripeCustomerId, true, null, dc));

                                newap = currentPackage;
                            }

                        }

                        if (msgs.HasNoErrors() && newap.ID.HasValue)
                        {
                            msgs.AddRange(p.CopyFeaturesToAccountPackage(newap, loggedInUser.ID.Value, dc));
                        }

                        // make sure the account package is set to the real trial days passed in
                        if (msgs.HasNoErrors())
                        {
                            var apf = newap.GetFeature(FeatureType.TrialDays, dc);
                            if (apf.IsNotEmpty())
                            {
                                apf.Max = trialDayCount;
                                msgs.AddRange(apf.Save(dc));
                            }
                        }
                    }
                    else
                    {
                        msgs.Add(string.Format("Credit Card is required to sign up for {0} Package", p.Name), MessageType.UserError);
                    }
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                    msgs.Add(string.Format("{0} Package has been activated", p.Name), MessageType.Success);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                },
                dc: dc
            );            

            return newap;
        }

        public int GetTrialDaysRemaining(AccountPackage currentPackage, DatabaseConnection dc = null)
        {
            var daysRemaining = 0;
            var firstAccountPackage = this.GetAccountPackages(dc).OrderBy(x => x.StartDate.Value).First();
            var daysSinceSignUp = (DateTime.UtcNow - firstAccountPackage.StartDate.Value).Days;
            if (daysSinceSignUp < currentPackage.GetFeature(FeatureType.TrialDays).GetMaxValue() + 1) // they get the full last day
            {
                daysRemaining = currentPackage.GetFeature(FeatureType.TrialDays).GetMaxValue() - daysSinceSignUp;
            }
            return daysRemaining;
        }

        public SystemMessages SaveWithStripe(DatabaseConnection dc = null)
        {
            SystemMessages msgs = new SystemMessages();

            msgs.AddRange(Save(dc));

            if (msgs.HasNoErrors())
            {
                msgs.AddRange(this.CreateStripeCustomer(dc));
            }

            return msgs;
        }

        public SystemMessages CreateStripeCustomer(DatabaseConnection dc = null)
        {
            SystemMessages msgs = new SystemMessages();

            Stripe.StripeRequestOptions requestOptions = new Stripe.StripeRequestOptions();
            requestOptions.ApiKey = StripeHelper.SecretKey;

            Stripe.StripeCustomer customer = this.GetStripeCustomer();

            if (!customer.IsEmpty())
            {
                Stripe.StripeCustomerUpdateOptions options = new Stripe.StripeCustomerUpdateOptions();
                options.Description = this.Name;
                options.Email = this.Email;

                Stripe.StripeCustomerService service = new Stripe.StripeCustomerService(StripeHelper.SecretKey);
                try
                {
                    customer = service.Update(this.StripeCustomerId, options, requestOptions);
                }
                catch (Stripe.StripeException ex)
                {
                    msgs.Add("Error connecting to billing service", MessageType.Warning, ex);
                }
            }
            else
            {
                Stripe.StripeCustomerCreateOptions options = new Stripe.StripeCustomerCreateOptions();
                options.Description = this.Name;
                options.Email = this.Email;

                Stripe.StripeCustomerService service = new Stripe.StripeCustomerService(StripeHelper.SecretKey);
                try
                {
                    customer = service.Create(options);
                }
                catch (Stripe.StripeException ex)
                {
                    msgs.Add("Error connecting to billing service", MessageType.Warning, ex);
                }

                if (!customer.IsEmpty())
                {
                    this.StripeCustomerId = customer.Id;
                    msgs.AddRange(Save(dc));
                }
            }
            return msgs;
        }

        public SystemMessages AddCreditCard(string name, string number, string expMonth, string expYear, string cvc, bool makePrimary, DatabaseConnection dc = null)
        {
            SystemMessages msgs = new SystemMessages();

            if(this.StripeCustomerId.IsEmpty())
            {
                msgs.AddRange(this.CreateStripeCustomer(dc));
            }

            if (msgs.HasNoErrors())
            {
                Stripe.StripeCardCreateOptions options = new Stripe.StripeCardCreateOptions();
                options.SourceCard = new Stripe.SourceCard();
                options.SourceCard.Name = name;
                options.SourceCard.Number = number;
                options.SourceCard.ExpirationMonth = expMonth.ToInteger();
                options.SourceCard.ExpirationYear = expYear.ToInteger();
                options.SourceCard.Cvc = cvc;

                Stripe.StripeCard card = null;
                Stripe.StripeCardService service = new Stripe.StripeCardService(StripeHelper.SecretKey);
                try
                {
                    card = service.Create(this.StripeCustomerId, options);
                    if(!card.IsEmpty() && makePrimary)
                    {
                        this.StripeDefaultCardId = card.Id;
                        msgs.AddRange(this.Save(dc));
                    }
                }
                catch(Stripe.StripeException ex)
                {
                    //error storing card
                    msgs.Add(ex.Message, MessageType.Error, ex);
                }
                catch(Exception ex)
                {
                    msgs.Add("Error storing card", MessageType.Error, ex);
                }

                if(msgs.HasNoErrors() && makePrimary)
                {
                    msgs.AddRange(SetStripeDefaultCard(card.Id));
                }

                if (msgs.HasNoErrors())
                {
                    msgs.Add("The payment card has been successfully added", MessageType.Success);
                }
            }

            return msgs;
        }

        public List<Stripe.StripeCard> GetStripeCards(SystemMessages msgs = null)
        {
            if(msgs.IsEmpty()) msgs = new SystemMessages();

            List<Stripe.StripeCard> cards = new List<Stripe.StripeCard>();
            Stripe.StripeCustomer customer = this.GetStripeCustomer();
            if (!customer.IsEmpty())
            {
                try
                {
                    Stripe.StripeCardService service = new Stripe.StripeCardService(StripeHelper.SecretKey);
                    cards.AddRange(service.List(customer.Id).ToList());
                }
                catch (Stripe.StripeException ex)
                {
                    //error storing card
                    msgs.Add("Error retrieving cards", MessageType.Error, ex);
                }
            }

            return cards;
        }

        public List<Stripe.StripeCharge> GetStripeBillingHistory(SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            List<Stripe.StripeCharge> charges = new List<Stripe.StripeCharge>();
            Stripe.StripeCustomer customer = this.GetStripeCustomer();
            if (!customer.IsEmpty())
            {
                try
                {
                    Stripe.StripeChargeService service = new Stripe.StripeChargeService(StripeHelper.SecretKey);
                    Stripe.StripeChargeListOptions options = new Stripe.StripeChargeListOptions();
                    options.CustomerId = customer.Id;
                    options.Limit = 10;
                    charges.AddRange(service.List(options).ToList());
                }
                catch (Stripe.StripeException ex)
                {
                    //error storing card
                    msgs.Add("Error retrieving billing history", MessageType.Error, ex);
                }
            }

            return charges;
        }

        public List<Stripe.StripeRefund> GetStripeRefundHistory(SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            List<Stripe.StripeRefund> refunds = new List<Stripe.StripeRefund>();
            Stripe.StripeCustomer customer = this.GetStripeCustomer();
            if (!customer.IsEmpty())
            {
                try
                {
                    var service = new Stripe.StripeRefundService(StripeHelper.SecretKey);
                    refunds.AddRange(service.List(null).ToList());
                }
                catch (Stripe.StripeException ex)
                {
                    msgs.Add("Error retrieving refund history", MessageType.Error, ex);
                }
            }

            return refunds;
        }

        public Stripe.StripeCustomer GetStripeCustomer(SystemMessages msgs = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Stripe.StripeCustomer customer = null;
            if (!StripeCustomerId.IsEmpty())
            {
                try
                {
                    Stripe.StripeCustomerService service = new Stripe.StripeCustomerService(StripeHelper.SecretKey);
                    customer = service.Get(this.StripeCustomerId);
                }
                catch (Stripe.StripeException ex)
                {
                    msgs.Add("Error retrieving customer information", MessageType.Error, ex);     
                }
            }
            return customer;
        }

        public SystemMessages SetStripeDefaultCard(string cardId)
        {
            SystemMessages msgs = new SystemMessages();

            var customer = GetStripeCustomer();

            Stripe.StripeCustomerUpdateOptions options = new Stripe.StripeCustomerUpdateOptions();
            options.DefaultSource = cardId;
            try
            {
                Stripe.StripeCustomerService service = new Stripe.StripeCustomerService(StripeHelper.SecretKey);
                service.Update(customer.Id, options);
            }
            catch (Stripe.StripeException ex)
            {
                msgs.Add("Error retrieving update default payment information", MessageType.Error, ex);
            }

            return msgs;
        }

        public Registrant AddRegistrant(int loggedInUserId, RegistrantType registrantType, bool isMaster, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var registrant = new Registrant();
            registrant.AccountId = this.ID;
            registrant.RegistrantTypeId = registrantType;
            registrant.IsMaster = isMaster;
            registrant.CreateDate = registrant.LastModifiedDate = DateTime.UtcNow;
            registrant.CreatedBy = registrant.LastModifiedBy = loggedInUserId;
            msgs.AddRange(registrant.Save(dc));

            return registrant;
        }

        public DateTime ConvertToAccountTimeZone(DateTime dt)
        {
            switch (this.TimeZoneAbbr)
            {
                case "EST":
                    return TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
                case "CST":
                    return TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
                case "MST":
                    return TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));
                case "PST":
                    return TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"));
                case "AKST":
                    return TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.FindSystemTimeZoneById("Alaskan Standard Time"));
                case "HAST":
                    return TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.FindSystemTimeZoneById("Hawaiian Standard Time"));
                default:
                    return dt;
            }
        }

        public DateTime NowInAccountTimeZone()
        {
            return ConvertToAccountTimeZone(DateTime.UtcNow);
        }

        public List<RegistrantActivity> GetRegistrantActivities(int? start = null, int? length = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.LoadByAccountId(this.ID.Value, start, length, dc).ToList();
        }

        public int GetRegistrantActivityCount(DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetCount(new Hashtable() { { "ActivityAccountId", this.ID.Value } });
        }

        public int GetRegistrantCount(DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByAccountId(this.ID.Value, dc);
        }

        public List<Attendance> GetCompletedAttendance(DatabaseConnection dc = null)
        {
            return Attendance.LoadByAccountIdByIsCompleted(this.ID.Value, true, dc);
        }

        public int GetAttendancePresentCount(DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.GetPresentCountByAccountId(this.ID.Value, dc);
        }

        public int GetAttendancePossibleCount(DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.GetPossibleCountByAccountId(this.ID.Value, dc);
        }

        public List<Attendance> GetAttendanceByDate(DateTime date, DatabaseConnection dc = null)
        {
            return Attendance.LoadByAccountIdByDate(this.ID.Value, date, dc);
        }

        public List<AccountPackageFeature> GetAccountPackageFeatures(DatabaseConnection dc = null)
        {
            return AccountPackageFeature.LoadByAccountPackageId(this.GetActiveAccountPackage(dc).ID.Value, dc);
        }

        public AccountProperty GetAccountProperty(AccountPropertyType propertyType, DatabaseConnection dc = null)
        {
            return AccountProperty.LoadByAccountIdByAccountPropertyTypeId(this.ID.Value, propertyType, dc);
        }

        public List<Registration> GetRegistrations(DatabaseConnection dc = null)
        {
            return Registration.LoadByAccountId(this.ID.Value, dc);
        }

        public List<Registration> GetCurrentRegistrations(DatabaseConnection dc = null)
        {
            return Registration.LoadCurrentByAccountId(this.ID.Value, dc);
        }

        public List<Registration> GetRegistrations(int start, int length, DatabaseConnection dc = null)
        {
            return Registration.LoadByAccountId(this.ID.Value, start, length, dc);
        }

        public int GetRegistrationCount(DatabaseConnection dc = null)
        {
            return Registration.GetCount(new Hashtable() { { "AccountId", this.ID.Value } }, dc);
        }

        public List<Registration> SearchRegistrations(string searchText, int? questionId, List<int> activityGroupIds, List<int> activityIds, RegistrationStatus regStatus, RegistrationAssignmentType rat, int? start, int? length, DatabaseConnection dc = null)
        {
            return Registration.Search(searchText, this.ID.Value, questionId, activityGroupIds, activityIds, regStatus, rat, start, length, dc);
        }

        public int SearchRegistrationCount(string searchText, int? questionId, List<int> activityGroupIds, List<int> activityIds, RegistrationStatus regStatus, RegistrationAssignmentType rat, DatabaseConnection dc = null)
        {
            return Registration.SearchCount(searchText, this.ID.Value, questionId, activityGroupIds, activityIds, regStatus, rat, dc);
        }

        public Activity GetDefaultNewActivity()
        {
            var activity = new Activity();
            activity.ID = 0;
            activity.WaitingListSeats = 0;
            activity.SeatsAvailable = Math.Min(20, this.GetActiveAccountPackage().GetFeature(FeatureType.Participants).Max.Value);
            activity.CostPerRegistrant = 0;
            activity.AccountId = this.ID;
            activity.RegistrationModeId = RegistrationMode.Register;
            activity.AllowToPayLater = true;
            return activity;
        }

        public RegistrationPeriod GetDefaultNewRegistrationPeriod()
        {
            var activePeriod = new RegistrationPeriod();
            activePeriod.ID = 0;
            activePeriod.StartDateTime = string.Format("{0} {1}", this.ConvertToAccountTimeZone(DateTime.UtcNow).ToShortDateString(), "8:00 AM").ToDateTime();
            activePeriod.EndDateTime = string.Format("{0} {1}", this.ConvertToAccountTimeZone(DateTime.UtcNow).AddWeeks(1).ToShortDateString(), "12:00 AM").ToDateTime();
            return activePeriod;
        }

        public static Account NewAccount(string company, string email, string phone, bool isActive, int loggedInUserId, bool isAfterSchool, SystemMessages msgs, DatabaseConnection dc)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var account = new Account();
            account.Name = company;
            account.IsAdminAccount = false;
            account.IsActive = isActive;
            account.Email = email;
            account.Phone = phone;
            account.UniqueName = Account.GenerateUniqueName(account.Name, account.ID, dc);
            account.CreateDate = account.LastModifiedDate = DateTime.UtcNow;
            account.CreatedBy = account.LastModifiedBy = loggedInUserId;
            account.IsAfterSchool = isAfterSchool;
            msgs.AddRange(account.SaveWithStripe(dc));
            return account;
        }
        
        #endregion
    }
}

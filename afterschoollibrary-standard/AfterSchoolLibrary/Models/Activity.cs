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
    /// The after school activity/team that a registrant/student is registering for
    /// </summary>
	
    public class Activity : BaseEventWithAddress
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public int? SeatsAvailable { get { return (int?)MDV["SeatsAvailable"].Value; } set { MDV["SeatsAvailable"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		public int? AgeStart { get { return (int?)MDV["AgeStart"].Value; } set { MDV["AgeStart"].Value = value; } }
		public int? AgeEnd { get { return (int?)MDV["AgeEnd"].Value; } set { MDV["AgeEnd"].Value = value; } }
		public int? GradeLevelStart { get { return (int?)MDV["GradeLevelStart"].Value; } set { MDV["GradeLevelStart"].Value = value; } }
		public int? GradeLevelEnd { get { return (int?)MDV["GradeLevelEnd"].Value; } set { MDV["GradeLevelEnd"].Value = value; } }
		public decimal? CostPerRegistrant { get { return (decimal?)MDV["CostPerRegistrant"].Value; } set { MDV["CostPerRegistrant"].Value = value; } }
		public string CostDescription { get { return (string)MDV["CostDescription"].Value; } set { MDV["CostDescription"].Value = value; } }
		public bool AllowPreRegistration { get { return (bool)MDV["AllowPreRegistration"].Value; } set { MDV["AllowPreRegistration"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public string Details { get { return (string)MDV["Details"].Value; } set { MDV["Details"].Value = value; } }
		public string LocationDetails { get { return (string)MDV["LocationDetails"].Value; } set { MDV["LocationDetails"].Value = value; } }
		public string ScheduleDetails { get { return (string)MDV["ScheduleDetails"].Value; } set { MDV["ScheduleDetails"].Value = value; } }
		public string RegistrationTimeZone { get { return (string)MDV["RegistrationTimeZone"].Value; } set { MDV["RegistrationTimeZone"].Value = value; } }
		public string ContactName { get { return (string)MDV["ContactName"].Value; } set { MDV["ContactName"].Value = value; } }
		public string ContactPhone { get { return (string)MDV["ContactPhone"].Value; } set { MDV["ContactPhone"].Value = value; } }
		public string ContactEmail { get { return (string)MDV["ContactEmail"].Value; } set { MDV["ContactEmail"].Value = value; } }
		public int? WaitingListSeats { get { return (int?)MDV["WaitingListSeats"].Value; } set { MDV["WaitingListSeats"].Value = value; } }
		public bool AcceptPayPal { get { return (bool)MDV["AcceptPayPal"].Value; } set { MDV["AcceptPayPal"].Value = value; } }
		public bool AcceptCreditCard { get { return (bool)MDV["AcceptCreditCard"].Value; } set { MDV["AcceptCreditCard"].Value = value; } }
		public int? ParentActivityId { get { return (int?)MDV["ParentActivityId"].Value; } set { MDV["ParentActivityId"].Value = value; } }
		public bool AllowToPayLater { get { return (bool)MDV["AllowToPayLater"].Value; } set { MDV["AllowToPayLater"].Value = value; } }
		public bool AllowOrganizationRegistration { get { return (bool)MDV["AllowOrganizationRegistration"].Value; } set { MDV["AllowOrganizationRegistration"].Value = value; } }
		public GenderRestrictionType GenderRestriction { get { return (GenderRestrictionType)MDV["GenderRestriction"].Value; } set { MDV["GenderRestriction"].Value = value; } }
		public RegistrationMode RegistrationModeId { get { return (RegistrationMode)MDV["RegistrationModeId"].Value; } set { MDV["RegistrationModeId"].Value = value; } }
		public int? LocationId { get { return (int?)MDV["LocationId"].Value; } set { MDV["LocationId"].Value = value; } }
		public bool IsDeleted { get { return (bool)MDV["IsDeleted"].Value; } set { MDV["IsDeleted"].Value = value; } }
		public bool IsPublished { get { return (bool)MDV["IsPublished"].Value; } set { MDV["IsPublished"].Value = value; } }
		public bool IsArchived { get { return (bool)MDV["IsArchived"].Value; } set { MDV["IsArchived"].Value = value; } }
		public int? ActivityGroupId { get { return (int?)MDV["ActivityGroupId"].Value; } set { MDV["ActivityGroupId"].Value = value; } }
		public int? ActivityScheduleId { get { return (int?)MDV["ActivityScheduleId"].Value; } set { MDV["ActivityScheduleId"].Value = value; } }
		public bool ScheduleRequiresAllDays { get { return (bool)MDV["ScheduleRequiresAllDays"].Value; } set { MDV["ScheduleRequiresAllDays"].Value = value; } }
		public string UniqueName { get { return (string)MDV["UniqueName"].Value; } set { MDV["UniqueName"].Value = value; } }
		public bool IsTemplate { get { return (bool)MDV["IsTemplate"].Value; } set { MDV["IsTemplate"].Value = value; } }
		public int? ActivitySectionId { get { return (int?)MDV["ActivitySectionId"].Value; } set { MDV["ActivitySectionId"].Value = value; } }
		/// <summary>
		/// Required.
		/// </summary>
		public ActivityType ActivityTypeId { get { return (ActivityType)MDV["ActivityTypeId"].Value; } set { MDV["ActivityTypeId"].Value = value; } }
		public DateTime? DateSubmittedToActivityGroup { get { return (DateTime?)MDV["DateSubmittedToActivityGroup"].Value; } set { MDV["DateSubmittedToActivityGroup"].Value = value; } }
		public string AccountName { get { return (string)MDV["AccountName"].Value; } }
		public string AccountPayPalEmailAddress { get { return (string)MDV["AccountPayPalEmailAddress"].Value; } }
		public string AccountStripePublishableKey { get { return (string)MDV["AccountStripePublishableKey"].Value; } }
		public string AccountTimeZone { get { return (string)MDV["AccountTimeZone"].Value; } }
		public bool? ActivityGroupIsArchived { get { return (bool?)MDV["ActivityGroupIsArchived"].Value; } }
		public string ActivityGroupName { get { return (string)MDV["ActivityGroupName"].Value; } }
		

        public Activity()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Activity).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.AllowPreRegistration = false;
			this.Uid = Guid.NewGuid();
			this.AcceptPayPal = false;
			this.AcceptCreditCard = false;
			this.AllowToPayLater = false;
			this.AllowOrganizationRegistration = false;
			this.GenderRestriction = GenderRestrictionType.None;
			this.RegistrationModeId = RegistrationMode.NotSet;
			this.IsDeleted = false;
			this.IsPublished = false;
			this.IsArchived = false;
			this.ScheduleRequiresAllDays = false;
			this.IsTemplate = false;
			this.ActivityTypeId = ActivityType.NotSet;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(Activity).Name;
                _dp = BaseEventWithAddress.get_BaseObjectMap(className);
				_dp.Add(className, "SeatsAvailable", System.Data.SqlDbType.Int);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AgeStart", System.Data.SqlDbType.Int);
				_dp.Add(className, "AgeEnd", System.Data.SqlDbType.Int);
				_dp.Add(className, "GradeLevelStart", System.Data.SqlDbType.Int);
				_dp.Add(className, "GradeLevelEnd", System.Data.SqlDbType.Int);
				_dp.Add(className, "CostPerRegistrant", System.Data.SqlDbType.Money);
				_dp.Add(className, "CostDescription", System.Data.SqlDbType.NVarChar, 1000);
				_dp.Add(className, "AllowPreRegistration", System.Data.SqlDbType.Bit);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "Details", System.Data.SqlDbType.NVarChar, 4000);
				_dp.Add(className, "LocationDetails", System.Data.SqlDbType.NVarChar, 4000);
				_dp.Add(className, "ScheduleDetails", System.Data.SqlDbType.NVarChar, 4000);
				_dp.Add(className, "RegistrationTimeZone", System.Data.SqlDbType.NVarChar, 30);
				_dp.Add(className, "ContactName", System.Data.SqlDbType.NVarChar, 50);
				_dp.Add(className, "ContactPhone", System.Data.SqlDbType.NVarChar, 50);
				_dp.Add(className, "ContactEmail", System.Data.SqlDbType.NVarChar, 100);
				_dp.Add(className, "WaitingListSeats", System.Data.SqlDbType.Int);
				_dp.Add(className, "AcceptPayPal", System.Data.SqlDbType.Bit);
				_dp.Add(className, "AcceptCreditCard", System.Data.SqlDbType.Bit);
				_dp.Add(className, "ParentActivityId", System.Data.SqlDbType.Int);
				_dp.Add(className, "AllowToPayLater", System.Data.SqlDbType.Bit);
				_dp.Add(className, "AllowOrganizationRegistration", System.Data.SqlDbType.Bit);
				_dp.Add(className, "GenderRestriction", System.Data.SqlDbType.Int);
				_dp.Add(className, "RegistrationModeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "LocationId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsDeleted", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsPublished", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsArchived", System.Data.SqlDbType.Bit);
				_dp.Add(className, "ActivityGroupId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityScheduleId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ScheduleRequiresAllDays", System.Data.SqlDbType.Bit);
				_dp.Add(className, "UniqueName", System.Data.SqlDbType.NVarChar, 100);
				_dp.Add(className, "IsTemplate", System.Data.SqlDbType.Bit);
				_dp.Add(className, "ActivitySectionId", System.Data.SqlDbType.Int);
				_dp.Add(className, "ActivityTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "DateSubmittedToActivityGroup", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "AccountName", typeof(Account).Name, "Name", System.Data.SqlDbType.NVarChar, "Account");
				_dp.Add(className, "AccountPayPalEmailAddress", typeof(Account).Name, "PayPalEmailAddress", System.Data.SqlDbType.NVarChar, "Account");
				_dp.Add(className, "AccountStripePublishableKey", typeof(Account).Name, "StripePublishableKey", System.Data.SqlDbType.NVarChar, "Account");
				_dp.Add(className, "AccountTimeZone", typeof(Account).Name, "TimeZone", System.Data.SqlDbType.NVarChar, "Account");
				_dp.Add(className, "ActivityGroupIsArchived", typeof(ActivityGroup).Name, "IsArchived", System.Data.SqlDbType.Bit, "ActivityGroup");
				_dp.Add(className, "ActivityGroupName", typeof(ActivityGroup).Name, "Name", System.Data.SqlDbType.NVarChar, "ActivityGroup");
				
                return _dp;
            }
        }

        protected static Activity LoadObject(System.Data.DataRow row)
        {
            Activity obj = new Activity();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(Activity).Name);
				dq.Joins.Add(Account.ObjectMap["ID"], Activity.ObjectMap["AccountId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Account");
				dq.Joins.Add(ActivityGroup.ObjectMap["ID"], Activity.ObjectMap["ActivityGroupId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.LeftOuterJoin, "ActivityGroup");
				
                return dq;
            }
        }

        public static Activity Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Activity> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return Activity.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return Activity.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.SeatsAvailable.IsEmpty())
				msgs.Add("SeatsAvailable is required on Activity object", MessageType.Error);
			if (this.AccountId.IsEmpty())
				msgs.Add("AccountId is required on Activity object", MessageType.Error);
			if (this.Uid.IsEmpty())
				msgs.Add("Uid is required on Activity object", MessageType.Error);
			if (this.ActivityTypeId.IsEmpty())
				msgs.Add("ActivityTypeId is required on Activity object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Activity).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Activity).Name);
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
			dps.Add(ObjectMap["SeatsAvailable"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AgeStart"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AgeEnd"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["GradeLevelStart"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["GradeLevelEnd"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["CostPerRegistrant"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["CostDescription"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AllowPreRegistration"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Details"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LocationDetails"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ScheduleDetails"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrationTimeZone"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ContactName"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ContactPhone"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ContactEmail"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["WaitingListSeats"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AcceptPayPal"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AcceptCreditCard"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ParentActivityId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AllowToPayLater"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["AllowOrganizationRegistration"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["GenderRestriction"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["RegistrationModeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LocationId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsDeleted"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsPublished"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsArchived"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityGroupId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityScheduleId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ScheduleRequiresAllDays"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["UniqueName"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsTemplate"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivitySectionId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ActivityTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DateSubmittedToActivityGroup"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Activity Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Activity.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<Activity> LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Activity> LoadByAccountIdByIsPublished(int AccountId, bool IsPublished, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			dq.Criterias.Add(ObjectMap["IsPublished"], IsPublished);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Activity> LoadByLocationId(int LocationId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["LocationId"], LocationId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Activity LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<Activity> LoadByParentActivityId(int ParentActivityId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ParentActivityId"], ParentActivityId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Activity> LoadByActivityGroupId(int ActivityGroupId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityGroupId"], ActivityGroupId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static List<Activity> LoadByActivityScheduleId(int ActivityScheduleId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["ActivityScheduleId"], ActivityScheduleId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Activity LoadByUniqueNameByAccountId(string UniqueName, int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["UniqueName"], UniqueName);
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public SubscriptionPlan GetSubscriptionPlan(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var list = SubscriptionPlan.LoadActiveByParticipationObject(this.ID.Value, ParticipationObjectType.Activity, msgs, dc);
            SubscriptionPlan subscriptionPlan = null;

            if (list.Any())
            {
                subscriptionPlan = list.First();
            }

            return subscriptionPlan;
        }

        public bool HasSubscriptionPlan(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SubscriptionPlan.LoadActiveByParticipationObject(this.ID.Value, ParticipationObjectType.Activity, msgs, dc).Any();
        }

        public PaymentPlan GetPaymentPlan(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var list = PaymentPlan.LoadActiveByParticipationObject(this.ID.Value, ParticipationObjectType.Activity, msgs, dc);
            PaymentPlan paymentPlan = null;

            if (list.Any())
            {
                paymentPlan = list.First();
            }

            return paymentPlan;
        }

        public bool HasPaymentPlan(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return PaymentPlan.LoadActiveByParticipationObject(this.ID.Value, ParticipationObjectType.Activity, msgs, dc).Any();
        }


        public string GetHomePageUrl(bool isAfterSchool = true, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return $"{Utilities.BaseUrl(isAfterSchool)}/{this.GetAccount(msgs, dc).UniqueName.ToLower()}/{this.ActivityTypeId.ToString().ToLower()}/{this.UniqueName.ToLower()}";
        }

        public string GetRegistrationPageUrl(bool isAfterSchool = true, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return $"{Utilities.BaseUrl(isAfterSchool)}/{this.GetHomePageUrl(isAfterSchool, msgs, dc)}/register";
        }

        public string HomePageUrl
        {
            get
            {
                return GetHomePageUrl(true);
            }
        }

        public string PreviewUrl
        {
            get
            {
                return $"{HomePageUrl}{"?preview=true"}";
            }
        }

        public bool CanAcceptPayments
        {
            get
            {
                return this.GetAccount().GetActiveAccountPackage().HasFeature(FeatureType.AcceptPayments);
            }
        }

        public static List<Activity> LoadList(List<int> ids, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, BasicLibrary.DataAccess.DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadListWithLocations(List<int> ids, List<int> locationIds, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["LocationId"], locationIds, DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        private static DatabaseQuery LoadByAccountIdQuery(int AccountId, int? activityGroupId, bool? isArchived, bool? isDeleted, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.UseParameterizedQuery = false;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            if (activityGroupId.HasValue)
            {
                dq.Criterias.Add(ObjectMap["activityGroupId"], activityGroupId.Value);
            }
            if (isArchived.HasValue)
            {
                if (!isArchived.Value)
                {
                    var dcs = new DatabaseCriterias(LogicalOperatorType.And);
                    var dcs2 = new DatabaseCriterias(LogicalOperatorType.Or);
                    dcs.Add(ObjectMap["isArchived"], isArchived.Value);

                    dcs2.Add(ObjectMap["ActivityGroupId"], DBNull.Value, DatabaseCriteriaOperatorEnum.Is);
                    dcs2.Add(ObjectMap["ActivityGroupIsArchived"], isArchived.Value);
                    dcs.AddGroup(dcs2);

                    dq.Criterias.Add(dcs);
                }
                else
                {
                    var dcs = new DatabaseCriterias(LogicalOperatorType.Or);
                    var dcs2 = new DatabaseCriterias(LogicalOperatorType.And);
                    dcs.Add(ObjectMap["isArchived"], isArchived.Value);

                    dcs2.Add(ObjectMap["ActivityGroupId"], DBNull.Value, DatabaseCriteriaOperatorEnum.IsNot);
                    dcs2.Add(ObjectMap["ActivityGroupIsArchived"], isArchived.Value);
                    dcs.AddGroup(dcs2);

                    dq.Criterias.Add(dcs);
                }


            }
            if (isDeleted.HasValue)
            {
                dq.Criterias.Add(ObjectMap["isDeleted"], isDeleted.Value);
            }
            return dq;
        }

        public static int GetCountByAccountId(int AccountId, int? activityGroupId, bool? isArchived, bool? isDeleted, DatabaseConnection dc = null)
        {
            return LoadByAccountIdQuery(AccountId, activityGroupId, isArchived, isDeleted, dc: dc).GetCount();
        }

        public static List<Activity> LoadByAccountId(int AccountId, int? activityGroupId, bool? isArchived, bool? isDeleted, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = LoadByAccountIdQuery(AccountId, activityGroupId, isArchived, isDeleted, dc: dc);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            dq.OrderBys.Add("Name", OrderByDirectionEnum.Ascending);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> LoadByAccountIdByDate(int AccountId, DateTime? startDate = null, DateTime? endDate = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);

            if (startDate.HasValue)
                dq.Criterias.Add(ObjectMap["RegistrationCloseDate"], startDate, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (endDate.HasValue)
                dq.Criterias.Add(ObjectMap["RegistrationCloseDate"], endDate, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);

            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public string RegistrationTimeZoneAbbr
        {
            get
            {
                if (RegistrationTimeZone.IsNotBlank())
                    return RegistrationTimeZone.Split('-')[1].Trim();
                else
                    return "";
            }
        }

        public double GetPercentPublishable(DatabaseConnection dc = null)
        {
            int totalFields = 0;
            int totalErrors = 0;

            totalFields++;
            if (Name.IsEmpty())
            {
                totalErrors++;
            }
            totalFields++;
            if (SeatsAvailable.HasValue == false)
            {
                totalErrors++;
            }

            if (!this.ActivityGroupId.HasValue)
            {
                totalFields++;
                if (this.GetActiveRegistrationPeriod(dc: dc).IsEmpty())
                {
                    totalErrors++;
                }
                totalFields++;
                if (this.GetActiveActivityForm(dc: dc).IsEmpty())
                {
                    totalErrors++;
                }
            }

            totalFields++;
            if (ContactName.IsBlank())
            {
                totalErrors++;
            }
            totalFields++;
            if (ContactEmail.IsBlank())
            {
                totalErrors++;
            }
            totalFields++;
            if (ContactPhone.IsBlank())
            {
                totalErrors++;
            }
            totalFields++;
            if (BasicLibrary.Utilities.RemoveHTMLAndEntities(Details).IsBlank())
            {
                totalErrors++;
            }
            totalFields++;
            if (!HasSchedule)
            {
                totalErrors++;
            }
            totalFields++;
            if (BasicLibrary.Utilities.RemoveHTMLAndEntities(LocationDetails).IsBlank())
            {
                totalErrors++;
            }
            //totalFields++;
            //if (Address1.IsBlank())
            //{
            //    totalErrors++;
            //}
            //totalFields++;
            //if (City.IsBlank())
            //{
            //    totalErrors++;
            //}
            //totalFields++;
            //if (State.IsBlank())
            //{
            //    totalErrors++;
            //}
            //totalFields++;
            //if (PostalCode.IsBlank())
            //{
            //    totalErrors++;
            //}
            totalFields++;
            if (CostPerRegistrant.HasValue && CostPerRegistrant.Value > 0 && !AllowToPayLater && !AcceptCreditCard && !AcceptPayPal)
            {
                totalErrors++;
            }
            if (!this.ActivityGroupId.HasValue)
            {
                totalFields++;
                if (this.GetActivityProperty(AccountPropertyType.ApplicationThankYouEmail).IsEmpty() ||
                    this.GetActivityProperty(AccountPropertyType.ApplicationThankYouEmail).Value.IsEmpty())
                {
                    totalErrors++;
                }
                totalFields++;
                if (this.GetActivityProperty(AccountPropertyType.RegistrationThankYouEmail).IsEmpty() ||
                    this.GetActivityProperty(AccountPropertyType.RegistrationThankYouEmail).Value.IsEmpty())
                {
                    totalErrors++;
                }
            }

            return ((totalFields - totalErrors) / totalFields) * 100;

        }

        public bool IsPublishable
        {
            get
            {
                return (GetPercentPublishable() == 100.ToDouble() ? true : false) && !IsArchived && (this.GetActivityGroup().IsEmpty() || !this.GetActivityGroup().IsArchived);
            }
        }

        public bool IsPubliclyViewable
        {
            get
            {
                return IsPublished && !IsArchived && (this.GetActivityGroup().IsEmpty() || !this.GetActivityGroup().IsArchived) && !IsDeleted;
            }
        }

        public bool IsGroupArchived
        {
            get
            {
                return this.GetActivityGroup().IsNotEmpty() && this.GetActivityGroup().IsArchived;
            }
        }

        public SystemMessages SetIsPublished(bool isPublished, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            this.IsPublished = isPublished;
            this.LastModifiedBy = loggedInUserId;
            this.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(this.Save(dc: dc));
            
            // DEPRECATED
            if (!this.IsChildActivity)
            {
                var childActivities = this.GetChildActivities(dc: dc);
                foreach (var a in childActivities)
                {
                    msgs.AddRange(a.SetIsPublished(isPublished, loggedInUserId, dc: dc));
                }
            }

            return msgs;
        }

        public bool IsChildActivity { get { return this.ParentActivityId.HasValue; } }

        public Activity GetParentActivity(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (this.ParentActivityId.HasValue)
                return Activity.Load(this.ParentActivityId.Value, msgs, dc);

            return null;
        }

        private List<ActivityDay> _Days = null;
        public List<ActivityDay> GetDays(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (_Days.IsEmpty())
                _Days = ActivityDay.LoadByActivityId(this.ID.Value, msgs, dc);

            return _Days;
        }

        public List<DayOfWeekASL> GetDaysOfTheWeek(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (this.GetSchedule(msgs: msgs, dc: dc)?.GetDaysOfTheWeek(msgs, dc).Any() ?? false)
                return this.GetSchedule(msgs: msgs, dc: dc)?.GetDaysOfTheWeek();

            return new List<DayOfWeekASL>();
        }

        public List<Registrant> GetRegistrants(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetRegistrantActivities(msgs, dc).Select(x => x.ToRegistrant()).ToList();
        }

        public List<RegistrantActivity> GetRegistrantActivities(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetRegistrantActivities(false, msgs, dc);
        }

        public List<RegistrantActivity> GetRegistrantActivities(bool includeInvited, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var ras = new List<RegistrantActivity>();

            if (includeInvited)
                ras = RegistrantActivity.LoadByActivityId(this.ID.Value, msgs, dc).ToList();
            else
                ras = RegistrantActivity.LoadByActivityId(this.ID.Value, msgs, dc).Where(x =>
                    x.NumberOfApplicationSeats.HasValue ||
                    x.NumberOfSeats.HasValue ||
                    x.NumberOfWaitingListSeats.HasValue
                ).ToList();
                
            return ras;
        }

        public int GetRegistrantCount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByActivityId(this.ID.Value, msgs, dc);
        }

        public int GetRegistrantCountByApplicationType(RegistrationAssignmentType rat, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantActivity.GetSeatCountByAssignmentType(rat: rat, activityId: this.ID, msgs: msgs, dc: dc);
        }

        public bool IsFull(DatabaseConnection dc = null)
        {
            return this.GetSeatsRemaining(dc: dc) == 0;
        }

        public bool IsWaitingListFull(DatabaseConnection dc = null)
        {
            return this.GetWaitingListSeatsRemaining(dc: dc) == 0;
        }

        public bool HasGradeLevel(GradeLevel grade, DatabaseConnection dc = null)
        {
            return GetGradeLevels(dc: dc).Where(x => x.ID.Value == grade.ID.Value).Count() > 0;
        }

        private List<GradeLevel> _GradeLevels = null;
        public List<GradeLevel> GetGradeLevels(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (_GradeLevels.IsEmpty())
                _GradeLevels = ActivityGradeLevel.LoadByActivityId(this.ID.Value, msgs, dc: dc)
                    .Select(x => x.ToGradeLevel())
                    .OrderBy(x => x.ID)
                    .ToList();

            return _GradeLevels;
        }

        public List<ActivityWebUser> GetTeamMembers(DatabaseConnection dc = null)
        {
            return ActivityWebUser.LoadByActivityId(this.ID.Value, dc: dc);
        }

        public List<Form> GetForms(DatabaseConnection dc = null)
        {
            return ActivityForm.LoadByActivityId(this.ID.Value, dc: dc).Select(x => x.ToForm()).ToList();
        }

        public Form GetActiveForm(DatabaseConnection dc = null)
        {
            var af = GetActiveActivityForm(dc: dc);
            if (af.IsEmpty())
                return null;

            return af.ToForm();
        }

        public ActivityForm GetActiveActivityForm(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (this.ID.HasValue)
                return ActivityForm.LoadByActivityIdByIsActive(this.ID.Value, true, msgs, dc);

            return null;
        }

        public List<ActivityForm> GetActivityForms(DatabaseConnection dc = null)
        {
            if (this.ID.HasValue)
                return ActivityForm.LoadByActivityId(this.ID.Value, dc: dc);

            return new List<ActivityForm>();
        }

        public List<Attendance> GetAttendance(DatabaseConnection dc = null)
        {
            return Attendance.LoadByActivityId(this.ID.Value, dc: dc);
        }

        public List<Attendance> GetCompletedAttendance(DatabaseConnection dc = null)
        {
            return Attendance.LoadByActivityIdByIsCompleted(this.ID.Value, true, dc: dc);
        }

        public int GetAttendancePresentCount(DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.GetPresentCountByActivityId(this.ID.Value, dc: dc);
        }

        public int GetAttendancePossibleCount(DatabaseConnection dc = null)
        {
            return AttendanceRegistrant.GetPossibleCountByActivityId(this.ID.Value, dc: dc);
        }

        public List<Attendance> GetAttendanceByDate(DateTime date, DatabaseConnection dc = null)
        {
            return Attendance.LoadByActivityIdByDate(this.ID.Value, date, dc: dc);
        }

        public List<ActivityRegistrationPeriod> GetActivityRegistrationPeriods(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityRegistrationPeriod.LoadByActivityId(this.ID.Value, msgs, dc);
        }

        public List<RegistrationPeriod> GetRegistrationPeriods(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetActivityRegistrationPeriods(msgs, dc)
                .Select(x => x.ToRegistrationPeriod())
                .ToList();
        }

        public List<RegistrationPeriod> GetUpcomingRegistrationPeriods(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetRegistrationPeriods(msgs, dc)
                .Where(x => x.StartDateTime.Value > DateTime.UtcNow)
                .OrderBy(x => x.StartDateTime.Value)
                .ToList();
        }

        public RegistrationPeriod GetCurrentRegistrationPeriod(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetRegistrationPeriods(msgs, dc)
                .Where(x => x.StartDateTime.Value <= DateTime.UtcNow && x.EndDateTime.Value >= DateTime.UtcNow)
                .FirstOrDefault();
        }

        public RegistrationPeriod GetCurrentOrNextRegistrationPeriod(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var period = this.GetCurrentRegistrationPeriod(msgs, dc);
            if (period.IsEmpty())
                return GetUpcomingRegistrationPeriods(msgs, dc).FirstOrDefault();

            return period;
        }

        public RegistrationPeriod GetActiveRegistrationPeriod(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (this.ID.HasValue)
            {
                var item = ActivityRegistrationPeriod.LoadByActivityIdByIsActive(this.ID.Value, true, msgs, dc);
                if (item.IsNotEmpty())
                    return item.ToRegistrationPeriod(true, msgs, dc);
            }
            return null;
        }

        public Account GetAccount(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.Load(this.AccountId.Value, msgs, dc);
        }

        private List<ActivityMessage> _ActivityMessages = null;
        public List<ActivityMessage> GetMessages(DatabaseConnection dc = null)
        {
            if (_ActivityMessages.IsEmpty())
                _ActivityMessages = ActivityMessage.LoadByActivityId(this.ID.Value, dc: dc);

            return _ActivityMessages;
        }

        private Activity CopyFields()
        {
            return new Activity()
            {
                AccountId = this.AccountId,
                Address1 = this.Address1,
                Address2 = this.Address2,
                City = this.City,
                ContactEmail = this.ContactEmail,
                ContactPhone = this.ContactPhone,
                ContactName = this.ContactName,
                Country = this.Country,
                Description = this.Description,
                Details = this.Details,
                Email = this.Email,
                LocationDetails = this.LocationDetails,
                Phone = this.Phone,
                PostalCode = this.PostalCode,
                RegistrationTimeZone = this.RegistrationTimeZone,
                SeatsAvailable = this.SeatsAvailable,
                State = this.State,
                Uid = Guid.NewGuid(),
                WaitingListSeats = this.WaitingListSeats,
                AcceptPayPal = this.AcceptPayPal,
                AcceptCreditCard = this.AcceptCreditCard,
                CostPerRegistrant = this.CostPerRegistrant,
                CostDescription = this.CostDescription,
                GenderRestriction = this.GenderRestriction,
                GradeLevelStart = this.GradeLevelStart,
                GradeLevelEnd = this.GradeLevelEnd,
                AgeStart = this.AgeStart,
                AgeEnd = this.AgeEnd,
                AllowOrganizationRegistration = this.AllowOrganizationRegistration,
                AllowPreRegistration = this.AllowPreRegistration,
                AllowToPayLater = this.AllowToPayLater,
                Website = this.Website,
                RegistrationModeId = this.RegistrationModeId,
                ActivityGroupId = this.ActivityGroupId,
                ActivityTypeId = this.ActivityTypeId,
                LocationId = this.LocationId
            };
        }

        [Obsolete]
        public List<Activity> GetChildActivities(DatabaseConnection dc = null)
        {
            return Activity.LoadByParentActivityId(this.ID.Value, dc: dc);
        }

        [Obsolete]
        public Activity CreateChild(string newName, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var activity = this.CopyFields();
            activity.Name = newName;
            activity.ParentActivityId = this.ID;
            activity.CreateDate = DateTime.UtcNow;
            activity.CreatedBy = loggedInUserId;
            activity.LastModifiedDate = DateTime.UtcNow;
            activity.LastModifiedBy = loggedInUserId;
            msgs.AddRange(activity.Save(dc: dc));

            //link activity with form for the parent activity
            var parentActivityForm = this.GetActiveActivityForm(dc: dc);
            activity.AddFormAndActivitate(parentActivityForm.ToForm(true, dc: dc), loggedInUserId, parentActivityForm.IsPublished, msgs, dc: dc);

            //will use same team permissions as parent

            return activity;
        }

        public Activity Copy(string newName, bool useSameForm, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Activity copy = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var account = this.GetAccount(amsgs, adc);

                    copy = this.CopyFields();
                    copy.Name = newName;
                    copy.CreateDate = DateTime.UtcNow;
                    copy.CreatedBy = loggedInUserId;
                    copy.LastModifiedDate = DateTime.UtcNow;
                    copy.LastModifiedBy = loggedInUserId;
                    copy.UniqueName = ActivityService.GenerateUniqueName(newName, null, account.ID, amsgs, adc);
                    amsgs.AddRange(copy.Save(adc));

                    //don't copy the whole form is the account can manage forms
                    if (account.HasFeature(FeatureType.ManageForms, amsgs, adc))
                    {
                        //get form and use the same form
                        var formToUse = this.GetActiveForm(adc);
                        if (formToUse.IsEmpty())
                            formToUse = account.GetDefaultForm(msgs, adc);

                        copy.AddFormAndActivitate(formToUse, loggedInUserId, false, amsgs, dc: adc);
                    }
                    //copy form completely
                    else
                    {
                        Form formToCopy = null;
                        if (useSameForm)
                            formToCopy = this.GetActiveForm(adc);
                        else
                            formToCopy = account.GetDefaultForm(msgs, adc);

                        var newForm = formToCopy.Copy(loggedInUserId, copy.AccountId.Value, amsgs, dc: adc);
                        copy.AddFormAndActivitate(newForm, loggedInUserId, false, amsgs, dc: adc);
                    }

                    // copy team and permissions
                    this.GetTeamMembers(adc).ForEach(member => member.Copy(copy.ID.Value, loggedInUserId, amsgs, adc));

                    var activePeriod = this.GetActiveRegistrationPeriod(amsgs, adc);
                    if (activePeriod.IsNotEmpty())
                    {
                        //copy registration period if doesn't have activity groups
                        if (!account.HasFeature(FeatureType.ManageActivityGroups, amsgs, adc))
                        {
                            var periodName = RegistrationPeriod.GenerateUniqueName(account.ID.Value, $"{copy.Name} - Reg Window", amsgs, adc);
                            var newPeriod = activePeriod.Copy(periodName, loggedInUserId, amsgs, dc: adc);
                            amsgs.AddRange(newPeriod.AddActivity(copy, true, loggedInUserId, adc));
                        }
                        else
                            amsgs.AddRange(activePeriod.AddActivity(copy, true, loggedInUserId, adc));
                    }

                    // copy Activity Properties
                    this.GetActivityProperties(adc).ForEach(property => {
                        var newProperty = new AccountProperty
                        {
                            ActivityId = copy.ID,
                            Value = property.Value,
                            AccountPropertyTypeId = property.AccountPropertyTypeId,
                            LastModifiedDate = DateTime.UtcNow,
                            CreateDate = DateTime.UtcNow,
                            LastModifiedBy = loggedInUserId,
                            CreatedBy = loggedInUserId
                        };
                        amsgs.AddRange(newProperty.Save(adc));
                    });

                    // copy days
                    this.GetDays(amsgs, adc).ForEach(day => {
                        var newDay = new ActivityDay
                        {
                            ActivityId = copy.ID,
                            ActivityRecurrenceId = day.ActivityRecurrenceId,
                            Day = day.Day,
                            EndTime = day.EndTime,
                            StartTime = day.StartTime,
                            CreateDate = DateTime.UtcNow,
                            LastModifiedDate = DateTime.UtcNow,
                            CreatedBy = loggedInUserId,
                            LastModifiedBy = loggedInUserId
                        };
                        amsgs.AddRange(newDay.Save(adc));
                    });

                    // copy grades
                    this.GetGradeLevels(amsgs, adc).ForEach(grade => {
                        copy.AddGrade(grade.ID.Value, loggedInUserId, amsgs, dc: adc);
                    });

                    // NOTE: copy schedule?
                    if (copy.ActivityScheduleId.HasValue)
                    {
                        copy.ActivityScheduleId = this.GetSchedule(false, amsgs, adc)
                            .CopyAndSave(loggedInUserId, amsgs, adc)
                            .ID;
                        amsgs.AddRange(copy.Save(adc));
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

            return copy;
        }

        public SystemMessages CopyDefaultForm(int loggedInUserId, DatabaseConnection dc = null)
        {
            SystemMessages msgs = new SystemMessages();
            if(this.GetActiveForm(dc: dc).IsEmpty())
            {
                var account = this.GetAccount(dc: dc);
                var formToCopy = account.GetDefaultForm(dc: dc);
                var newForm = formToCopy.Copy(loggedInUserId, this.AccountId.Value, msgs, dc: dc);
                var af = this.AddFormAndActivitate(newForm, loggedInUserId, false, msgs, dc: dc);

                account.DefaultFormId = af.FormId;
                msgs.AddRange(account.Save(dc: dc));
            }

            return msgs;
        }

        public ActivityForm AddFormAndActivitate(Form form, int loggedInUserId, bool isPublished = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            //deactivate all other ones
            var afs = this.GetActivityForms(dc: dc);
            foreach(var item in afs)
            {
                item.IsActive = false;
                msgs.AddRange(item.Save(dc: dc));
            }

            var af = ActivityForm.LoadByActivityIdByFormId(this.ID.Value, form.ID.Value, dc: dc);
            if (af.IsEmpty())
            {
                af = new ActivityForm();
                af.ActivityId = this.ID;
                af.AccountId = this.AccountId;
                af.Name = this.Name + " Registration Form";
                af.Description = "Please fill in the registration form below";
                af.FormId = form.ID;
                af.IsPublished = isPublished;
                af.CreateDate = DateTime.UtcNow;
                af.CreatedBy = loggedInUserId;
            }

            af.LastModifiedDate = DateTime.UtcNow;
            af.LastModifiedBy = loggedInUserId;
            af.IsActive = true;
            msgs.AddRange(af.Save(dc: dc));

            return af;
        }

        public bool HasAddress
        {
            get
            {
                return (!Address1.IsEmpty() && ((!City.IsEmpty() && !State.IsEmpty()) || !PostalCode.IsEmpty()));                
            }
        }

        public static Activity EditExisting(int loggedInUserId, Account account, string activityId, Dictionary<string, object> values, SystemMessages msgs = null)
        {
            return SaveActivity(loggedInUserId, account, activityId, values, msgs);
        }

        public static Activity CreateNew(int loggedInUserId, Account account, Dictionary<string, object> values, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return SaveActivity(loggedInUserId, account, null, values, msgs, dc: dc);
        }

        private static Activity SaveActivity(int loggedInUserId, Account account, string activityId, Dictionary<string, object> values, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();            

            var name = values.GetValueOrDefault("name");
            var seatsavailable = values.GetValueOrDefault("seatsavailable");
            var waitinglistseats = values.GetValueOrDefault("waitinglistseats");
            var registrationperiod = values.GetValueOrDefault("registrationperiod");
            var registrationopendate = values.GetValueOrDefault("registrationopendate");
            var registrationopentime = values.GetValueOrDefault("registrationopentime");
            var registrationclosedate = values.GetValueOrDefault("registrationclosedate");
            var registrationclosetime = values.GetValueOrDefault("registrationclosetime");
            var timezone = values.GetValueOrDefault("timezone");
            var contactname = values.GetValueOrDefault("contactname");
            var contactphone = values.GetValueOrDefault("contactphone");
            var contactemail = values.GetValueOrDefault("contactemail");
            var details = values.GetValueOrDefault("details");
            var location = values.GetValueOrDefault("location");
            var locationname = values.GetValueOrDefault("locationname");
            var locationdetails = values.GetValueOrDefault("locationdetails");
            var scheduledetails = values.GetValueOrDefault("scheduledetails");
            var address1 = values.GetValueOrDefault("address1");
            var address2 = values.GetValueOrDefault("address2");
            var city = values.GetValueOrDefault("city");
            var state = values.GetValueOrDefault("state");
            var postalcode = values.GetValueOrDefault("postalcode");
            var ismylocation = values.GetValueOrDefault("mylocation");
            var locationtype = values.GetValueOrDefault("locationtype");
            var cost = values.GetValueOrDefault("cost");
            var costdescription = values.GetValueOrDefault("costdescription");
            var acceptpaypal = values.GetValueOrDefault("acceptpaypal");
            var acceptcreditcard = values.GetValueOrDefault("acceptcreditcard");
            var allowtopaylater = values.GetValueOrDefault("allowtopaylater");
            var form = values.GetValueOrDefault("form");
            var alloworganization = values.GetValueOrDefault("alloworganization");
            var agestart = values.GetValueOrDefault("agestart");
            var ageend = values.GetValueOrDefault("ageend");
            var gradestart = values.GetValueOrDefault("gradestart");
            var gradeend = values.GetValueOrDefault("gradeend");
            var genderrestrictions = values.GetValueOrDefault("genderrestrictions");
            var regmode = values.GetValueOrDefault("regmode");
            var days = values.GetValueOrDefault("days");
            var gradeIds = values.GetValueOrDefault("gradeIds");
            var group = values.GetValueOrDefault("group");
            var registrationthankyouemail = values.GetValueOrDefault("registrationthankyouemail");
            var applicationthankyouemail = values.GetValueOrDefault("applicationthankyouemail");
            var daysoftheweek = values.GetValueOrDefault("daysoftheweek");
            var repeatsweekly = values.GetValueOrDefault("repeatsweekly");
            var startdate = values.GetValueOrDefault("startdate");
            var enddate = values.GetValueOrDefault("enddate");
            var dailystarttime = values.GetValueOrDefault("dailystarttime");
            var dailyendtime = values.GetValueOrDefault("dailyendtime");
            var scheduledates = values.GetValueOrDefault("scheduledates");
            var schedule = values.GetValueOrDefault("schedule");
            var hassubscriptionplan = values.GetValueOrDefault("hassubscriptionplan");
            var subscriptionplancost = values.GetValueOrDefault("subscriptionplancost");
            var subscriptionplanperiod = values.GetValueOrDefault("subscriptionplanperiod");
            var haspaymentplan = values.GetValueOrDefault("haspaymentplan");
            var paymentplancost = values.GetValueOrDefault("paymentplancost");
            var paymentplanperiod = values.GetValueOrDefault("paymentplanperiod");
            var paymentplanperiodnumber = values.GetValueOrDefault("paymentplanperiodnumber");

            if (activityId.IsNumber() == false || activityId.ToInteger() == 0)
            {
                if (name.IsEmpty())
                {
                    msgs.Add("Activity Name is required", MessageType.UserError);
                }
                if (!seatsavailable.IsNumber())
                {
                    msgs.Add("Total Available Seats is required and must be greater than 0", MessageType.UserError);
                }
                else
                {
                    var ap = account.GetActiveAccountPackage(msgs, dc);
                    var maxParticipants = ap.GetFeature(FeatureType.Participants, msgs, dc).Max.Value;
                    if (maxParticipants < seatsavailable.ToString().ToInteger())
                        msgs.Add(string.Format("Total Available Seats is required and must be less than or equal to {0}", maxParticipants), MessageType.UserError);

                }
                if (group.IsEmpty() && registrationperiod.ToString().ToInteger() > 0)
                {
                    if (!$"{registrationopendate} {registrationopentime}".IsDateTime())
                        msgs.Add("Registration Open Date & Time is required", MessageType.UserError);

                    if (!$"{registrationclosedate} {registrationclosetime}".IsDateTime())
                        msgs.Add("Registration Close Date & Time is required", MessageType.UserError);
                }   

                if (!waitinglistseats.IsNumber())
                    waitinglistseats = "0";

                //pre-load account info
                if (contactphone.IsBlank())
                {
                    contactphone = account.Phone;
                }
                if (address1.IsBlank())
                {
                    address1 = account.Address1;
                }
                if (address2.IsBlank())
                {
                    address2 = account.Address2;
                }
                if (city.IsBlank())
                {
                    city = account.City;
                }
                if (state.IsBlank())
                {
                    state = account.State;
                }
                if (postalcode.IsBlank())
                {
                    postalcode = account.PostalCode;
                }
                if (cost.IsBlank())
                {
                    cost = "0";
                }

            }            

            Activity activity = null;

            if (activityId.IsNotEmpty())
            {
                activity = Activity.Load(activityId.ToInteger(), msgs, dc);
                if (seatsavailable.IsNumber() && 
                    activity.GetRegistrantCountByApplicationType(RegistrationAssignmentType.Reserved, msgs, dc) <= seatsavailable.ToString().ToInteger())
                {
                    activity.SeatsAvailable = seatsavailable.ToString().ToInteger();
                }
                if (waitinglistseats.IsNumber() && 
                    activity.GetRegistrantCountByApplicationType(RegistrationAssignmentType.WaitingList, msgs, dc) <= waitinglistseats.ToString().ToInteger())
                {
                    activity.WaitingListSeats = waitinglistseats.ToString().ToInteger();
                }
            }
            else
            {
                activity = new Activity();
                activity.ActivityTypeId = ActivityType.Activity;
                activity.CreateDate = DateTime.UtcNow;
                activity.CreatedBy = loggedInUserId;
                activity.SeatsAvailable = seatsavailable.ToString().ToInteger();
                activity.WaitingListSeats = waitinglistseats.ToString().ToInteger();
                activity.AccountId = account.ID;
                activity.LocationId = account.DefaultLocationId;

                if (group.IsEmpty() && !form.IsNumber())
                {
                    msgs.Add("Registration Form is required", MessageType.UserError);
                }
            }

            activity.LastModifiedDate = DateTime.UtcNow;
            activity.LastModifiedBy = loggedInUserId;

            if (name.IsNotEmpty())
            {
                activity.Name = name.ToString();
                activity.UniqueName = ActivityService.GenerateUniqueName(name.ToString(), activity.ID, activity.AccountId, msgs, dc);
            }

            if (agestart.IsNumber())
            {
                if (agestart.ToString().ToInteger() >= 0)
                    activity.AgeStart = agestart.ToString().ToInteger();
                else
                    activity.AgeStart = null;
            }
            if (ageend.IsNumber())
            {
                if (ageend.ToString().ToInteger() >= 0)
                    activity.AgeEnd = ageend.ToString().ToInteger();
                else
                    activity.AgeEnd = null;
            }

            if (gradestart.IsNumber() && gradeend.IsNumber())
            {
                if (gradestart.ToString().ToInteger() > -1)
                    activity.GradeLevelStart = gradestart.ToString().ToInteger();
                else
                    activity.GradeLevelStart = null;

                if (gradeend.ToString().ToInteger() > -1)
                    activity.GradeLevelEnd = gradeend.ToString().ToInteger();
                else
                    activity.GradeLevelEnd = null;
            }

            if (genderrestrictions.IsNumber())
                activity.GenderRestriction = (GenderRestrictionType)genderrestrictions.ToString().ToInteger();
            
            if (contactname.IsNotEmpty()) activity.ContactName = contactname.ToString().IfBlank("").MaxChars(50);
            if (contactphone.IsNotEmpty()) activity.ContactPhone = contactphone.ToString().IfBlank("").MaxChars(50);
            if (contactemail.IsNotEmpty()) activity.ContactEmail = contactemail.ToString().IfBlank("").MaxChars(100);
            if (details.IsNotEmpty()) activity.Details = details.ToString().IfBlank("").MaxChars(4000);            

            if (cost.IsNumber())
            {
                activity.CostPerRegistrant = cost.ToString().ToDecimal();
            }
            if (costdescription.IsNotEmpty())
            {
                activity.CostDescription = costdescription.ToString().IfBlank("").MaxChars(1000);
            }
            
            if (acceptcreditcard.IsNotEmpty() && acceptcreditcard.ToString().IsBoolean())
            {
                activity.AcceptCreditCard = acceptcreditcard.ToString().ToBoolean();
            }
            if (acceptpaypal.IsNotEmpty() && acceptpaypal.ToString().IsBoolean())
            {
                activity.AcceptPayPal = acceptpaypal.ToString().ToBoolean();
            }
            if (allowtopaylater.IsNotEmpty() && allowtopaylater.ToString().IsBoolean())
            {
                activity.AllowToPayLater = allowtopaylater.ToString().ToBoolean();
            }
            if (alloworganization.IsNotEmpty() && alloworganization.ToString().IsBoolean())
            {
                activity.AllowOrganizationRegistration = alloworganization.ToString().ToBoolean();
            }
            if (regmode.IsNumber())
            {
                activity.RegistrationModeId = (RegistrationMode)regmode.ToString().ToInteger();
            }
            else
            {
                // activity.RegistrationModeId = RegistrationMode.Register;
            }

            if(activity.CostPerRegistrant > 0 && !activity.AcceptCreditCard && !activity.AcceptPayPal && !activity.AllowToPayLater)
            {
                msgs.Add("You must either accept credit cards, PayPal, or allow to pay later if you are charging a fee", MessageType.UserError);
            }

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, BasicLibrary.DataAccess.DatabaseConnection adc) =>
                    {
                        if (!activity.ID.HasValue)
                        {
                            activity.RegistrationTimeZone = account.TimeZone;

                            if (contactname.IsEmpty())
                            {
                                activity.ContactName = account.GetAccountOwner(amsgs, adc).DisplayName;
                            }
                            if (contactemail.IsEmpty())
                            {
                                activity.ContactEmail = account.Email;
                            }
                            if (contactphone.IsEmpty())
                            {
                                activity.ContactPhone = account.Phone;
                            }
                        }
                        if (group.IsNotEmpty() && group.ToString().IsGuid())
                        {
                            activity.ActivityGroupId = ActivityGroup.LoadByUid(group.ToString().ToGuid(), amsgs, adc).ID.Value;
                        }

                        amsgs.AddRange(activity.Save(adc));

                        //skip if it is attached to a activity group
                        if (!activity.ActivityGroupId.HasValue)
                        {
                            //copy form
                            if (amsgs.HasNoErrors() && !activity.IsChildActivity && form.IsNumber())
                            {
                                amsgs.AddRange(activity.SetupNewForm(form.ToString().ToInteger(), account, loggedInUserId, adc));
                            }

                            //create registration period if not created yet and there is no activitygroup (activity groups have their own registration period)
                            if (amsgs.HasNoErrors() && registrationperiod.IsNumber())
                            {
                                amsgs.AddRange(activity.SetupRegistrationPeriods(
                                    registrationperiod.IfEmpty(""), 
                                    registrationopendate.IfEmpty(""), 
                                    registrationopentime.IfEmpty(""), 
                                    registrationclosedate.IfEmpty(""), 
                                    registrationclosetime.IfEmpty(""), 
                                    account, 
                                    loggedInUserId, 
                                    adc));
                            }
                        }

                        //add creator to activity team if new activity
                        if (amsgs.HasNoErrors() && activityId.IsEmpty())
                        {
                            amsgs.AddRange(activity.SetupTeam(account, loggedInUserId, adc));
                        }

                        //days // deprecated
                        //if (amsgs.HasNoErrors() && days.IsNotEmpty())
                        //{
                        //    amsgs.AddRange(activity.SetupActivityDays(days.ToString(), account, loggedInUserId, adc));
                        //}

                        // setup schedule
                        if(amsgs.HasNoErrors() && (schedule.IsNotEmpty() || scheduledates.IsNotEmpty()))
                        {
                            amsgs.AddRange(activity.SetupSchedule(
                                account,
                                schedule.IfEmpty(""), 
                                (List<DateRange>)scheduledates,
                                scheduledetails.IfEmpty(""),
                                repeatsweekly.IfEmpty(""), 
                                startdate.IfEmpty(""), 
                                enddate.IfEmpty(""), 
                                dailystarttime.IfEmpty(""), 
                                dailyendtime.IfEmpty(""), 
                                loggedInUserId, 
                                adc));
                        }

                        //grades
                        /*
                        if (amsgs.HasNoErrors() && gradeIds.IsNotEmpty())
                        {
                            amsgs.AddRange(activity.SetupGrades(gradeIds.ToString(), account, loggedInUserId, adc));
                        }
                        */

                        //setup initial activityProperties
                        if (amsgs.HasNoErrors())
                        {
                            amsgs.AddRange(activity.SetupActivityProperties(
                                registrationthankyouemail.IfEmpty(""), 
                                applicationthankyouemail.IfEmpty(""), 
                                loggedInUserId, 
                                adc));
                        }

                        //setup location
                        if (amsgs.HasNoErrors() && (location.IsNotEmpty() || locationname.IsNotEmpty()))
                        {
                            var hostAccountId = ismylocation.IfEmpty("").ToBoolean() ? account.ID : new int?();

                            amsgs.AddRange(activity.SetupLocation(
                                location.IfEmpty(""),
                                locationname.IfEmpty(""),
                                address1.IfEmpty(""),
                                address2.IfEmpty(""),
                                city.IfEmpty(""),
                                state.IfEmpty(""),
                                postalcode.IfEmpty(""),
                                locationdetails.IfEmpty(""),
                                ismylocation.IfEmpty(""),
                                locationtype.IfEmpty(""),
                                loggedInUserId,
                                adc
                                ));

                            if (!account.DefaultLocationId.HasValue)
                                msgs.AddRange(account.SetDefaultLocation(activity.LocationId.Value, loggedInUserId, dc));
                        }

                        //setup SubscriptionPlan
                        if (amsgs.HasNoErrors())
                        {
                            amsgs.AddRange(activity.SetupSubscriptionPlan(
                                account,
                                activity,
                                hassubscriptionplan.IfEmpty(""),
                                subscriptionplancost.IfEmpty(""),
                                subscriptionplanperiod.IfEmpty(""),
                                loggedInUserId,
                                adc
                            ));
                        }

                        //setup PaymentPlan
                        if (amsgs.HasNoErrors())
                        {
                            amsgs.AddRange(activity.SetupPaymentPlan(
                                account,
                                activity,
                                haspaymentplan.IfEmpty(""),
                                paymentplancost.IfEmpty(""),
                                paymentplanperiod.IfEmpty(""),
                                paymentplanperiodnumber.IfEmpty(""),
                                loggedInUserId,
                                adc
                            ));
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
            }

            return activity;
        }

        private SystemMessages SetupSchedule(Account account, string schedule, List<DateRange> scheduledates, string scheduledetails, string repeatsweekly, string startdate, string enddate, string dailystarttime, string dailyendtime, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            this.ScheduleRequiresAllDays = true;

            //if (account.HasFeature(FeatureType.ManageActivityGroups, msgs, dc) &&
            //    schedule.IsNumber() &&
            //    schedule.ToString().ToInteger() > 0 &&
            //    scheduledates.IsEmpty())
            //{
            //    this.ActivityScheduleId = schedule.ToString().ToInteger();
            //}
            //else if (scheduledates.IsNotEmpty())
            //{
                var newSchedule = Services.ActivityScheduleService.SetupSchedule(
                    this.AccountId.Value,
                    schedule.IfEmpty("0").ToInteger(),
                    $"{this.Name} - Schedule",
                    scheduledetails.ToString(),
                    repeatsweekly.ToString(),
                    startdate.ToString(),
                    enddate.ToString(),
                    dailystarttime.ToString(),
                    dailyendtime.ToString(),
                    (List<DateRange>)scheduledates,
                    account,
                    this.ActivityGroupId,
                    loggedInUserId,
                    msgs,
                    dc);

                this.ActivityScheduleId = newSchedule.ID;
            //}

            msgs.AddRange(this.Save(dc));

            return msgs;
        }

        private SystemMessages SetupSubscriptionPlan(Account account, Activity activity, string hassubscriptionplan, string subscriptionplancost, string subscriptionplanperiod, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            // if the subscription plan option(UI) is activated
            if (hassubscriptionplan.ToBoolean())
            {
                 // if there is no subscription plan active in the DB
                 // let's create a new one
                if (!activity.HasSubscriptionPlan())
                {
                    var subscriptionPlan = Services.SubscriptionPlanService.CreateNew(
                            account,
                            ParticipationObjectType.Activity,
                            activity.ID.Value,
                            subscriptionplancost.ToDecimal(),
                            (BillingPeriodType)subscriptionplanperiod.ToInteger(),
                            true,
                            loggedInUserId,
                            msgs,
                            dc
                        );
                }
                else
                {
                    SubscriptionPlan subscriptionPlan = activity.GetSubscriptionPlan();

                    // if the plan is purchased and begin edited
                    if (subscriptionPlan.IsPurchased() && (subscriptionPlan.CostPerPeriod != Convert.ToDecimal(subscriptionplancost)))// || subscriptionPlan.BillingPeriodTypeId != Convert.ToInt32(subscriptionplanperiod))
                    {
                        // first: desactivate the current plan
                        subscriptionPlan.Deactivate(loggedInUserId, dc);

                        // second: create a new plan
                        var newSubscriptionPlan = Services.SubscriptionPlanService.CreateNew(
                                account,
                                ParticipationObjectType.Activity,
                                activity.ID.Value,
                                subscriptionplancost.ToDecimal(),
                                (BillingPeriodType)subscriptionplanperiod.ToInteger(),
                                true,
                                loggedInUserId,
                                msgs,
                                dc
                            );
                    }
                    else
                    {
                        // update the current plan
                        subscriptionPlan.LastModifiedBy = loggedInUserId;
                        subscriptionPlan.LastModifiedDate = DateTime.UtcNow;
                        subscriptionPlan.CostPerPeriod = Convert.ToDecimal(subscriptionplancost);
                        subscriptionPlan.BillingPeriodTypeId = (BillingPeriodType)Convert.ToInt32(subscriptionplanperiod);
                        msgs.AddRange(subscriptionPlan.Save(dc));
                    }
                }

            }
            else if (activity.HasSubscriptionPlan())
            {
                SubscriptionPlan subscriptionPlan = activity.GetSubscriptionPlan();

                subscriptionPlan.Deactivate(loggedInUserId, dc);
            }

            return msgs;
        }

        private SystemMessages SetupPaymentPlan(Account account, Activity activity, string haspaymentplan, string paymentplancost, string paymentplanperiod, string paymentplanperiodnumber, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            // if the payment plan option(UI) is activated
            if (haspaymentplan.ToBoolean())
            {
                // if there is no payment plan active in the DB
                // let's create a new one
                if (!activity.HasPaymentPlan())
                {
                    var paymentPlan = Services.PaymentPlanService.CreateNew(
                        account,
                        ParticipationObjectType.Activity,
                        activity.ID.Value,
                        paymentplancost.ToDecimal(),
                        (BillingPeriodType)paymentplanperiod.ToInteger(),
                        paymentplanperiodnumber.ToInteger(),
                        loggedInUserId,
                        msgs,
                        dc);
                    msgs.AddRange(paymentPlan.Save(dc));
                }
                else
                {
                    PaymentPlan subscriptionPlan = activity.GetPaymentPlan();
                    // update the current plan
                    subscriptionPlan.LastModifiedBy = loggedInUserId;
                    subscriptionPlan.LastModifiedDate = DateTime.UtcNow;
                    subscriptionPlan.CostPerPeriod = paymentplancost.ToDecimal();
                    subscriptionPlan.TotalPeriods = paymentplanperiodnumber.ToInteger();
                    subscriptionPlan.BillingPeriodTypeId = (BillingPeriodType)paymentplanperiod.ToInteger();
                    msgs.AddRange(subscriptionPlan.Save(dc));
                }

            }
            else if (activity.HasPaymentPlan())
            {
                PaymentPlan paymentPlan = activity.GetPaymentPlan();

                paymentPlan.Deactivate(loggedInUserId, dc);
            }

            return msgs;
        }

        private SystemMessages SetupLocation(string location, string locationname, string address1, string address2, string city, string state, string postalcode, string locationdetails, string ismylocation, string locationtype, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            // just switching locations
            if (location.IsNumber())
            {
                this.LocationId = location.ToString().ToInteger();
            }
            // they entered a new location possibly? 
            // otherwise somehow the UI allowed them to enter a location that already exists, with the exact same location name
            else
            {
                var locationType = LocationType.NotSet;
                var hostAccountId = new int?();

                if (locationtype.IsNumber())
                    locationType = (LocationType)locationtype.ToInteger();

                if (ismylocation.IsBoolean() && ismylocation.ToBoolean())
                    hostAccountId = this.AccountId;

                var activityLocation = Services.LocationService.CreateNewWithValidation(
                        locationname,
                        address1,
                        address2,
                        city,
                        state,
                        postalcode,
                        "US",
                        locationType,
                        hostAccountId,
                        new Account { ID = this.AccountId },
                        new WebUser { ID = loggedInUserId },
                        msgs,
                        dc);

                this.LocationId = activityLocation.ID;
            }

            
            if (locationdetails.IsNotEmpty())
                this.LocationDetails = locationdetails.IfBlank("").MaxChars(4000);

            msgs.AddRange(this.Save(dc));

            return msgs;
        }

        private SystemMessages SetupActivityProperties(string registrationthankyouemail, string applicationthankyouemail, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if (registrationthankyouemail.IsEmpty())
            {
                registrationthankyouemail = "Thank you for registering";
            }
            if (applicationthankyouemail.IsEmpty())
            {
                applicationthankyouemail = "Thank you for applying";
            }

            this.AddOrUpdateActivityProperty(AccountPropertyType.ApplicationThankYouEmail, applicationthankyouemail, loggedInUserId, msgs, dc: dc);
            this.AddOrUpdateActivityProperty(AccountPropertyType.RegistrationThankYouEmail, registrationthankyouemail, loggedInUserId, msgs, dc: dc);

            return msgs;
        }

        [Obsolete]
        private SystemMessages SetupGrades(string gradeIds, Account account, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var gradeIdsList = gradeIds.IsEmpty() ? new List<int>() : gradeIds.Split(',').Select(x => x.ToInteger()).ToList();
            var currentGrades = this.GetGradeLevels(dc: dc);

            currentGrades
                .Where(x => !gradeIdsList.Contains(x.ID.Value))
                .ToList()
                .ForEach(grade => msgs.AddRange(this.RemoveGrade(grade, dc: dc)));

            gradeIdsList
                .Where(gradeId => !currentGrades.Select(grade => grade.ID.Value).Contains(gradeId))
                .ToList()
                .ForEach(gradeId => this.AddGrade(gradeId, loggedInUserId, msgs, dc: dc));

            //forces reload of grade levels
            this._GradeLevels = null;
            return msgs;
        }

        private ActivityDay AddActivityDay(DayOfWeekASL day, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var newDay = new ActivityDay();
            newDay.Day = day;
            newDay.ActivityId = this.ID;
            newDay.CreateDate = newDay.LastModifiedDate = DateTime.UtcNow;
            newDay.CreatedBy = newDay.LastModifiedBy = loggedInUserId;
            msgs.AddRange(newDay.Save(dc: dc));
            return newDay;
        }

        private SystemMessages SetupActivityDays(string days, Account account, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var daysList = days.Split(',').Select(x => (DayOfWeekASL)x.ToInteger()).ToList();
            var currentDaysOfWeek = this.GetDaysOfTheWeek(dc: dc);
            var currentDays = this.GetDays(dc: dc);

            // delete days
            currentDaysOfWeek
                .Where(x => !daysList.Contains(x))
                .ToList()
                .ForEach(day => msgs.AddRange(currentDays.Where(x => x.Day == day).First().Delete(dc: dc)));

            // add days
            daysList
                .Where(x => !currentDaysOfWeek.Contains(x))
                .ToList()
                .ForEach(day => this.AddActivityDay(day, loggedInUserId, msgs, dc: dc));  
            
            //forces reload of days
            this._Days = null;
            return msgs;
        }

        private SystemMessages SetupRegistrationPeriods(string registrationperiod, string registrationopendate, string registrationopentime, string registrationclosedate, string registrationclosetime, Account account, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if (registrationperiod.ToInteger() == 0)
            {
                var periodname = RegistrationPeriod.GenerateUniqueName(account.ID.Value, string.Format("{0} - Reg Window", this.Name), dc: dc);

                this.AddNewRegistrationPeriod(
                    periodname,
                    string.Format("{0} {1}", registrationopendate, registrationopentime).ToDateTime(),
                    string.Format("{0} {1}", registrationclosedate, registrationclosetime).ToDateTime(),
                    account.ID.Value,
                    loggedInUserId,
                    msgs,
                    dc);
            }
            else
            {
                var period = RegistrationPeriod.Load(registrationperiod.ToInteger(), dc: dc);

                //if user doesn't have feature then we're updaing the period in the background
                if (!account.HasFeature(FeatureType.ManageActivityGroups, dc: dc))
                {
                    period.StartDateTime = string.Format("{0} {1}", registrationopendate, registrationopentime).ToDateTime();
                    period.EndDateTime = string.Format("{0} {1}", registrationclosedate, registrationclosetime).ToDateTime();
                    period.LastModifiedDate = DateTime.UtcNow;
                    period.LastModifiedBy = loggedInUserId;
                    msgs.AddRange(period.Save(dc: dc));
                }
                msgs.AddRange(period.AddActivity(this, true, loggedInUserId, dc: dc));
            }
            return msgs;
        }

        private SystemMessages SetupTeam(Account account, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            //validate the user is part of this account (not a global admin)
            var awu = AccountWebUser.LoadByAccountIdByWebUserId(this.AccountId.Value, loggedInUserId, dc: dc);

            if (awu.IsNotEmpty())
            {
                this.AddTeamMember(loggedInUserId, true, loggedInUserId, msgs, dc: dc);
            }

            //add account admins to activity
            account.GetAccountOwnersAndAdmins(dc: dc)
                .ForEach(user => this.AddTeamMember(user.ID.Value, true, loggedInUserId, msgs, dc: dc));

            return msgs;
        }

        private SystemMessages SetupNewForm(int formId, Account account, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var activeForm = this.GetActiveActivityForm(dc: dc);

            //already has created a form, no need to copy it again
            if (activeForm.IsEmpty() || activeForm.FormId != formId)
            {
                var isPublished = false;

                // deactivating existing form
                if (activeForm.IsNotEmpty())
                {
                    //deactivate old form
                    isPublished = activeForm.IsPublished;
                    activeForm.IsActive = false;
                    msgs.AddRange(activeForm.Save(dc: dc));
                }

                Form newForm = null;
                if (formId == 0)
                {
                    //if creating a new form, copy from the default form
                    var defaultForm = account.GetDefaultForm(dc: dc);
                    newForm = defaultForm.Copy(loggedInUserId, account.ID.Value, msgs, dc: dc);
                    newForm.Name = (this.Name + " Form").MaxChars(100, "...");
                    msgs.AddRange(newForm.Save(dc: dc));
                }
                else
                {
                    //get the already existing form
                    newForm = Form.Load(formId, dc: dc);
                }

                if (newForm.IsNotEmpty())
                {
                    //link form to activity
                    this.AddFormAndActivitate(newForm, loggedInUserId, isPublished, msgs, dc: dc);

                    //add to children
                    var children = this.GetChildActivities(dc: dc);
                    foreach (var child in children)
                    {
                        //deactivate old form
                        var childActiveForm = child.GetActiveActivityForm(dc: dc);
                        if (childActiveForm.IsNotEmpty())
                        {
                            childActiveForm.IsActive = false;
                            msgs.AddRange(childActiveForm.Save(dc: dc));
                        }
                        child.AddFormAndActivitate(newForm, loggedInUserId, isPublished, msgs, dc: dc);
                    }
                }
            }

            return msgs;
        }

        public RegistrationPeriod AddNewRegistrationPeriod(string name, DateTime startDateTime, DateTime endDateTime, int accountId, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var period = new RegistrationPeriod();
            period.StartDateTime = startDateTime;
            period.EndDateTime = endDateTime;
            period.Name = name;
            period.AccountId = accountId;
            period.CreateDate = period.LastModifiedDate = DateTime.UtcNow;
            period.CreatedBy = period.LastModifiedBy = loggedInUserId;
            msgs.AddRange(period.Save(dc: dc));

            msgs.AddRange(period.AddActivity(this, true, loggedInUserId, dc: dc));

            return period;
        }

        public ActivityWebUser AddTeamMember(int webUserId, bool isLeader, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var teammember = new ActivityWebUser();
            teammember.WebUserId = webUserId;
            teammember.ActivityId = this.ID;
            teammember.IsLeader = isLeader;
            teammember.CreateDate = DateTime.UtcNow;
            teammember.CreatedBy = loggedInUserId;
            teammember.LastModifiedDate = DateTime.UtcNow;
            teammember.LastModifiedBy = loggedInUserId;
            msgs.AddRange(teammember.Save(dc: dc));

            return teammember;
        }

        [Obsolete]
        public SystemMessages RemoveGrade(GradeLevel grade, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var activityGrade = ActivityGradeLevel.LoadByActivityIdByGradeLevelId(this.ID.Value, grade.ID.Value, dc: dc);
            if (activityGrade.IsNotEmpty())
            {
                msgs.AddRange(activityGrade.Delete(dc: dc));
            }
            return msgs;
        }

        [Obsolete]
        public ActivityGradeLevel AddGrade(int gradeId, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc= null)
        {
            ActivityGradeLevel activityGrade = ActivityGradeLevel.LoadByActivityIdByGradeLevelId(this.ID.Value, gradeId, dc: dc);

            if (activityGrade.IsEmpty())
            {
                activityGrade = new ActivityGradeLevel();
                activityGrade.ActivityId = this.ID;
                activityGrade.GradeLevelId = gradeId;
                activityGrade.CreateDate = DateTime.UtcNow;
                activityGrade.CreatedBy = loggedInUserId;
                msgs.AddRange(activityGrade.Save(dc: dc));
            }

            return activityGrade;
        }

        public SystemMessages MarkAsDeleted(int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            this.IsDeleted = true;
            this.LastModifiedDate = DateTime.UtcNow;
            this.LastModifiedBy = loggedInUserId;
            msgs.AddRange(this.Save(dc: dc));
            return msgs;
        }

        public SystemMessages MoveToArchive(int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            this.IsArchived = true;
            this.LastModifiedDate = DateTime.UtcNow;
            this.LastModifiedBy = loggedInUserId;
            msgs.AddRange(this.Save(dc: dc));
            return msgs;
        }

        public SystemMessages MoveFromArchive(int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            this.IsArchived = false;
            this.LastModifiedDate = DateTime.UtcNow;
            this.LastModifiedBy = loggedInUserId;
            msgs.AddRange(this.Save(dc: dc));
            return msgs;
        }

        [NonSerialized()]
        private ActivityGroup _ActivityGroup = null;
        public ActivityGroup GetActivityGroup(DatabaseConnection dc = null)
        {
            if (_ActivityGroup.IsEmpty() && this.ActivityGroupId.HasValue)
            {
                _ActivityGroup = ActivityGroup.Load(this.ActivityGroupId.Value, dc: dc);
            }
            return _ActivityGroup;
        }

        public AccountProperty AddOrUpdateActivityProperty(AccountPropertyType type, string value, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var ap = this.GetActivityProperty(type, dc: dc);

            if (ap.IsEmpty())
            {
                ap = new AccountProperty();
                ap.ActivityId = this.ID.Value;
                ap.AccountPropertyTypeId = type;
                ap.CreateDate = DateTime.UtcNow;
                ap.CreatedBy = loggedInUserId;
            }

            ap.Value = value;
            ap.LastModifiedDate = DateTime.UtcNow;
            ap.LastModifiedBy = loggedInUserId;
            msgs.AddRange(ap.Save(dc: dc));

            return ap;
        }

        public bool HasSchedule
        {
            get
            {
                return this.ActivityScheduleId.HasValue;
            }
        }

        private ActivitySchedule _Schedule = null;
        public ActivitySchedule GetSchedule(bool reload = false, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (this.ActivityScheduleId.HasValue && (reload || _Schedule.IsEmpty()))
            {
                _Schedule = ActivitySchedule.LoadWithInstances(this.ActivityScheduleId.Value, msgs, dc);
            }
            return _Schedule;
        }

        public AccountProperty GetActivityProperty(AccountPropertyType propertyType, DatabaseConnection dc = null)
        {
            return GetActivityProperties(dc: dc)?.Find(x => x.AccountPropertyTypeId == propertyType); 
        }

        private List<AccountProperty> _Properties = null;
        public List<AccountProperty> GetActivityProperties(DatabaseConnection dc = null)
        {
            if (_Properties.IsEmpty() && this.ID.HasValue)
            {
                _Properties = AccountProperty.LoadByActivityId(this.ID.Value, dc: dc);
            }
            return _Properties;
        }

        public ActivityMessage AddMessage(string subject, string body, List<int> registrantIds, int loggedInUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            ActivityMessage am = null;

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, BasicLibrary.DataAccess.DatabaseConnection adc) =>
                {
                    am = new ActivityMessage();
                    am.ActivityId = this.ID;
                    am.AccountId = this.AccountId;
                    am.Subject = subject.MaxChars(150);
                    am.Body = body;
                    am.CreateDate = DateTime.UtcNow;
                    am.CreatedBy = loggedInUserId;
                    amsgs.AddRange(am.Save(adc));

                    registrantIds.ForEach(registrantId => am.AddRegistrant(registrantId, loggedInUserId, amsgs, dc: dc));
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

        public static List<Activity> GetActivitiesByRegistrantByDay(int registrantId, DayOfWeekASL day, DateTime startDate, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;

            dq.Joins.Add(ActivityInstance.ObjectMap["ActivityScheduleId"], ObjectMap["ActivityScheduleId"]);
            dq.Joins.Add(RegistrantActivity.ObjectMap["ActivityId"], ObjectMap["ID"]);

            dq.Criterias.Add(RegistrantActivity.ObjectMap["RegistrantId"], registrantId);
            dq.Criterias.Add(RegistrantActivity.ObjectMap["NumberOfSeats"], 1, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);
            dq.Criterias.Add(ActivityInstance.ObjectMap["Day"], (int)day);
            dq.Criterias.Add(ActivityInstance.ObjectMap["StartDateTime"], startDate, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Activity> GetByActivitiesByRegistrants(List<int> activityIds, List<int> registrantIds, DayOfWeekASL? day = DayOfWeekASL.NotSet, RegistrationStatus regStatus = RegistrationStatus.NotSet, List<int> groupIds = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;

            dq.Joins.Add(ActivityInstance.ObjectMap["ActivityScheduleId"], ObjectMap["ActivityScheduleId"]);
            dq.Joins.Add(RegistrantActivity.ObjectMap["ActivityId"], ObjectMap["ID"]);

            dq.Criterias.Add(ObjectMap["Id"], activityIds, DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(RegistrantActivity.ObjectMap["RegistrantId"], registrantIds, DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(RegistrantActivity.ObjectMap["NumberOfSeats"], 0, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanParameter);

            if (groupIds.IsNotEmpty() && groupIds.Any())
            {
                dq.Criterias.Add(ObjectMap["ActivityGroupId"], groupIds, DatabaseCriteriaOperatorEnum.In);
            }

            if (day != DayOfWeekASL.NotSet)
            {
                dq.Criterias.Add(ActivityInstance.ObjectMap["Day"], (int)day);
            }
            // dq.Criterias.Add(ActivityInstance.ObjectMap["StartDateTime"], startDate, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);

            if (regStatus == RegistrationStatus.Current)
            {
                var aQuery = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivityInstance).Name);
                aQuery.UseParameterizedQuery = false;
                aQuery.SelectParameters.Clear();
                aQuery.SelectParameters.Add(ActivityInstance.ObjectMap["ActivityScheduleId"]);
                aQuery.Criterias.Add(ActivityInstance.ObjectMap["ActivityScheduleId"], ObjectMap["ActivityScheduleId"]);
                aQuery.Criterias.Add(ActivityInstance.ObjectMap["StartDateTime"], DateTime.UtcNow, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);
                dq.Criterias.Add(ObjectMap["ActivityScheduleId"], aQuery, DatabaseCriteriaOperatorEnum.In);

                var bQuery = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivityInstance).Name);
                bQuery.UseParameterizedQuery = false;
                bQuery.SelectParameters.Clear();
                bQuery.SelectParameters.Add(ActivityInstance.ObjectMap["ActivityScheduleId"]);
                bQuery.Criterias.Add(ActivityInstance.ObjectMap["ActivityScheduleId"], ObjectMap["ActivityScheduleId"]);
                bQuery.Criterias.Add(ActivityInstance.ObjectMap["EndDateTime"], DateTime.UtcNow, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);
                dq.Criterias.Add(ObjectMap["ActivityScheduleId"], bQuery, DatabaseCriteriaOperatorEnum.In);
            }
            else if (regStatus == RegistrationStatus.Future)
            {
                // has no end dates in the past
                var aQuery = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivityInstance).Name);
                aQuery.UseParameterizedQuery = false;
                aQuery.SelectParameters.Clear();
                aQuery.SelectParameters.Add(ActivityInstance.ObjectMap["ActivityScheduleId"]);
                aQuery.Criterias.Add(ActivityInstance.ObjectMap["ActivityScheduleId"], ObjectMap["ActivityScheduleId"]);
                aQuery.Criterias.Add(ActivityInstance.ObjectMap["EndDateTime"], DateTime.UtcNow, DatabaseCriteriaOperatorEnum.DBValueIsLessThanOrEqualToParameter);
                dq.Criterias.Add(ObjectMap["ActivityScheduleId"], aQuery, DatabaseCriteriaOperatorEnum.NotIn);

                // has some records in the activity instance table though
                var bQuery = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivityInstance).Name);
                bQuery.UseParameterizedQuery = false;
                bQuery.SelectParameters.Clear();
                bQuery.SelectParameters.Add(ActivityInstance.ObjectMap["ActivityScheduleId"]);
                bQuery.Criterias.Add(ActivityInstance.ObjectMap["ActivityScheduleId"], ObjectMap["ActivityScheduleId"]);
                dq.Criterias.Add(ObjectMap["ActivityScheduleId"], bQuery, DatabaseCriteriaOperatorEnum.In);
            }
            else if (regStatus == RegistrationStatus.Past)
            {
                // has no start dates in the future
                var aQuery = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivityInstance).Name);
                aQuery.UseParameterizedQuery = false;
                aQuery.SelectParameters.Clear();
                aQuery.SelectParameters.Add(ActivityInstance.ObjectMap["ActivityScheduleId"]);
                aQuery.Criterias.Add(ActivityInstance.ObjectMap["ActivityScheduleId"], ObjectMap["ActivityScheduleId"]);
                aQuery.Criterias.Add(ActivityInstance.ObjectMap["StartDateTime"], DateTime.UtcNow, DatabaseCriteriaOperatorEnum.DBValueIsGreaterThanOrEqualToParameter);
                dq.Criterias.Add(ObjectMap["ActivityScheduleId"], aQuery, DatabaseCriteriaOperatorEnum.NotIn);

                // has some records in the activity instance table though
                var bQuery = BaseObject.get_DatabaseQuery(ObjectMap, typeof(ActivityInstance).Name);
                bQuery.UseParameterizedQuery = false;
                bQuery.SelectParameters.Clear();
                bQuery.SelectParameters.Add(ActivityInstance.ObjectMap["ActivityScheduleId"]);
                bQuery.Criterias.Add(ActivityInstance.ObjectMap["ActivityScheduleId"], ObjectMap["ActivityScheduleId"]);
                dq.Criterias.Add(ObjectMap["ActivityScheduleId"], bQuery, DatabaseCriteriaOperatorEnum.In);
            }


            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public List<GenderType> GetAllowedGenders()
        {
            switch (this.GenderRestriction)
            {
                case GenderRestrictionType.None:
                    return new List<GenderType>() { GenderType.Female, GenderType.Male };
                case GenderRestrictionType.MaleOnly:
                    return new List<GenderType>() { GenderType.Male };
                case GenderRestrictionType.FemaleOnly:
                    return new List<GenderType>() { GenderType.Female };
            }
            return new List<GenderType>();
        }

        public bool IsInSession(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var account = this.GetAccount(msgs, dc);
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);
            var instances = this.GetSchedule(msgs: msgs, dc: dc).GetInstances(msgs: msgs, dc: dc);
            return instances.First().StartDateTime <= now && now <= instances.Last().EndDateTime;
        }

        public bool WillBeInSession(DatabaseConnection dc = null)
        {
            var account = this.GetAccount(dc: dc);
            var now = account.ConvertToAccountTimeZone(DateTime.UtcNow);
            var instances = this.GetSchedule(dc: dc).GetInstances(dc: dc);
            return now < instances.First().StartDateTime;
        }

        public bool IsOutOfSession(DatabaseConnection dc = null)
        {
            var now = this.GetAccount(dc: dc).ConvertToAccountTimeZone(DateTime.UtcNow);
            var instances = this.GetSchedule(dc: dc).GetInstances(dc: dc);
            return now > instances.Last().StartDateTime;
        }

        /// <summary>
        /// gets the first scheduled date for the activity, also gets the start time
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public DateTime? GetFirstDate(DatabaseConnection dc = null)
        {
            return this.GetSchedule(dc: dc).GetInstances(dc: dc).FirstOrDefault()?.StartDateTime;
        }

        /// <summary>
        /// gets the last scheduled date for the activity, also gets the start time
        /// </summary>
        /// <param name="dc"></param>
        /// <returns></returns>
        public DateTime? GetLastDate(DatabaseConnection dc = null)
        {
            return this.GetSchedule(dc: dc).GetInstances(dc: dc).LastOrDefault()?.StartDateTime;
        }

        public static SystemMessages SendRegistrationSummaryReport(int activityId, string uiActivityLabel, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var activity = Activity.Load(activityId, msgs, dc);
            var registrants = activity.GetRegistrants(msgs, dc);

            string subject = $"{activity.Name} Registration Summary Report";
            string body = $"<p>{activity.ContactName},</p><p>Here's your registration summary report for <strong>{activity.Name}</strong>. You have <strong>{registrants.Count.ToString()}</strong> registrations.</p>";

            msgs.AddRange(Registration.EmailCSVReport(
                activity.GetAccount(msgs, dc).ID.Value, 
                null,
                null,
                activity.ActivityGroupId.HasValue ? activity.ActivityGroupId.Value.AsList() : null, 
                activityId.AsList(), 
                "registrations",
                RegistrationReportType.SingleActivity,
                RegistrationAssignmentType.NotSet,
                uiActivityLabel, 
                activity.ContactEmail,
                subject,
                body));

            return msgs;
        }
       
        #endregion
    }
}

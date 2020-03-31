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
    /// The main organization and account holder in the system
    /// </summary>
	[Serializable]
    public class Account : BaseCompany
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required.
		/// </summary>
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public string UniqueName { get { return (string)MDV["UniqueName"].Value; } set { MDV["UniqueName"].Value = value; } }
		/// <summary>
		/// WebUser id that owns/created the account
		/// </summary>
		public int? OwnerId { get { return (int?)MDV["OwnerId"].Value; } set { MDV["OwnerId"].Value = value; } }
		public bool IsAdminAccount { get { return (bool)MDV["IsAdminAccount"].Value; } set { MDV["IsAdminAccount"].Value = value; } }
		public bool IsActive { get { return (bool)MDV["IsActive"].Value; } set { MDV["IsActive"].Value = value; } }
		public bool IsAfterSchool { get { return (bool)MDV["IsAfterSchool"].Value; } set { MDV["IsAfterSchool"].Value = value; } }
		public string StripeCustomerId { get { return (string)MDV["StripeCustomerId"].Value; } set { MDV["StripeCustomerId"].Value = value; } }
		public int? DefaultFormId { get { return (int?)MDV["DefaultFormId"].Value; } set { MDV["DefaultFormId"].Value = value; } }
		public string StripeUserId { get { return (string)MDV["StripeUserId"].Value; } set { MDV["StripeUserId"].Value = value; } }
		public string TimeZone { get { return (string)MDV["TimeZone"].Value; } set { MDV["TimeZone"].Value = value; } }
		public string LogoUrl { get { return (string)MDV["LogoUrl"].Value; } set { MDV["LogoUrl"].Value = value; } }
		public AccountType AccountTypeId { get { return (AccountType)MDV["AccountTypeId"].Value; } set { MDV["AccountTypeId"].Value = value; } }
		public int? DefaultLocationId { get { return (int?)MDV["DefaultLocationId"].Value; } set { MDV["DefaultLocationId"].Value = value; } }
		public int? MinGradeLevelOffered { get { return (int?)MDV["MinGradeLevelOffered"].Value; } set { MDV["MinGradeLevelOffered"].Value = value; } }
		public int? MaxGradeLevelOffered { get { return (int?)MDV["MaxGradeLevelOffered"].Value; } set { MDV["MaxGradeLevelOffered"].Value = value; } }
		public Guid InviteCodeToShare { get { return (Guid)MDV["InviteCodeToShare"].Value; } set { MDV["InviteCodeToShare"].Value = value; } }
		public FeesPaidByType CreditCardFeesPaidBy { get { return (FeesPaidByType)MDV["CreditCardFeesPaidBy"].Value; } set { MDV["CreditCardFeesPaidBy"].Value = value; } }
		public FeesPaidByType ConvenienceFeesPaidBy { get { return (FeesPaidByType)MDV["ConvenienceFeesPaidBy"].Value; } set { MDV["ConvenienceFeesPaidBy"].Value = value; } }
		public bool StripeChargesEnabled { get { return (bool)MDV["StripeChargesEnabled"].Value; } set { MDV["StripeChargesEnabled"].Value = value; } }
		public bool StripePayoutsEnabled { get { return (bool)MDV["StripePayoutsEnabled"].Value; } set { MDV["StripePayoutsEnabled"].Value = value; } }
		public bool IsTest { get { return (bool)MDV["IsTest"].Value; } set { MDV["IsTest"].Value = value; } }
		public bool IsDeleted { get { return (bool)MDV["IsDeleted"].Value; } set { MDV["IsDeleted"].Value = value; } }
		

        public Account()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Account).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.Uid = Guid.NewGuid();
			this.IsAdminAccount = false;
			this.IsActive = false;
			this.IsAfterSchool = false;
			this.AccountTypeId = AccountType.NotSet;
			this.InviteCodeToShare = Guid.NewGuid();
			this.CreditCardFeesPaidBy = FeesPaidByType.NotSet;
			this.ConvenienceFeesPaidBy = FeesPaidByType.NotSet;
			this.StripeChargesEnabled = false;
			this.StripePayoutsEnabled = false;
			this.IsTest = false;
			this.IsDeleted = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(Account).Name;
                _dp = BaseCompany.get_BaseObjectMap(className);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "UniqueName", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "OwnerId", System.Data.SqlDbType.Int);
				_dp.Add(className, "IsAdminAccount", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsActive", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsAfterSchool", System.Data.SqlDbType.Bit);
				_dp.Add(className, "StripeCustomerId", System.Data.SqlDbType.NVarChar , "50");
				_dp.Add(className, "DefaultFormId", System.Data.SqlDbType.Int);
				_dp.Add(className, "StripeUserId", System.Data.SqlDbType.NVarChar , "100");
				_dp.Add(className, "TimeZone", System.Data.SqlDbType.NVarChar , "30");
				_dp.Add(className, "LogoUrl", System.Data.SqlDbType.NVarChar , "400");
				_dp.Add(className, "AccountTypeId", System.Data.SqlDbType.Int);
				_dp.Add(className, "DefaultLocationId", System.Data.SqlDbType.Int);
				_dp.Add(className, "MinGradeLevelOffered", System.Data.SqlDbType.Int);
				_dp.Add(className, "MaxGradeLevelOffered", System.Data.SqlDbType.Int);
				_dp.Add(className, "InviteCodeToShare", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "CreditCardFeesPaidBy", System.Data.SqlDbType.Int);
				_dp.Add(className, "ConvenienceFeesPaidBy", System.Data.SqlDbType.Int);
				_dp.Add(className, "StripeChargesEnabled", System.Data.SqlDbType.Bit);
				_dp.Add(className, "StripePayoutsEnabled", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsTest", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsDeleted", System.Data.SqlDbType.Bit);
				
                return _dp;
            }
        }

        public static Account LoadObject(System.Data.DataRow row)
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

        public static Account Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Account> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return Account.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return Account.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.Uid.IsEmpty())
				msgs.Add("Uid is required on Account object", MessageType.Error);
			if (this.Name.IsEmpty())
				msgs.Add("Name is required on Account object", MessageType.Error);
			
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
			dps.Add(ObjectMap["DefaultFormId"]);
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
			dps.Add(ObjectMap["AccountTypeId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DefaultLocationId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["MinGradeLevelOffered"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["MaxGradeLevelOffered"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["InviteCodeToShare"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["CreditCardFeesPaidBy"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["ConvenienceFeesPaidBy"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StripeChargesEnabled"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["StripePayoutsEnabled"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsTest"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsDeleted"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Account Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Account.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<Account> LoadByOwnerId(int OwnerId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["OwnerId"], OwnerId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Account LoadByUniqueName(string UniqueName, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["UniqueName"], UniqueName);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static Account LoadByIsAdminAccount(bool IsAdminAccount, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["IsAdminAccount"], IsAdminAccount);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static Account LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<Account> LoadByAccountTypeId(AccountType AccountTypeId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountTypeId"], AccountTypeId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Account LoadByStripeUserId(string StripeUserId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["StripeUserId"], StripeUserId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static Account LoadByStripeCustomerId(string StripeCustomerId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["StripeCustomerId"], StripeCustomerId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static Account LoadByInviteCodeToShare(Guid InviteCodeToShare, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["InviteCodeToShare"], InviteCodeToShare);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public bool HasAccountOwner
        {
            get
            {
                return OwnerId.HasValue;
            }
        }

        public bool HasStripeConnectedAccount
        {
            get
            {
                return StripeUserId.IsNotEmpty();
            }
        }

        public bool IsSchool
        {
            get
            {
                return AccountTypeId == AccountType.School;
            }
        }

        public bool IsActivityProvider
        {
            get
            {
                return AccountTypeId == AccountType.ActivityProvider;
            }
        }

        public bool IsCreditCardFeeTakenOutOfCost
        {
            get
            {
                return CreditCardFeesPaidBy == FeesPaidByType.Account;
            }
        }

        public bool IsConvenienceFeeTakenOutOfCost
        {
            get
            {
                return ConvenienceFeesPaidBy == FeesPaidByType.Account;
            }
        }

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
                return $"{Helpers.Utilities.BaseUrl()}/{this.UniqueName}";
            }

        }

        public string BrowseUrl
        {
            get
            {
                return $"{HomePageUrl}/browse";
            }

        }

        public string PayUrl
        {
            get
            {
                return $"{HomePageUrl}/pay";
            }

        }

        public string ShortHomePageUrl
        {
            get
            {
                return $"{Helpers.Utilities.ShortBaseUrl()}/{this.UniqueName}";
            }

        }

        public string HostAccountInviteUrl
        {
            get
            {
                return $"{Helpers.Utilities.BaseUrl()}/signup?invite={this.InviteCodeToShare}";
            }
        }

        public string HostAccountShortInviteUrl
        {
            get
            {
                return $"{Helpers.Utilities.ShortBaseUrl()}/signup?invite={this.InviteCodeToShare}";
            }
        }

        public bool HasPaymentCard
        {
            get
            {
                return false; // this.StripeDefaultCardId.IsNotEmpty();
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

        public static int GetCount(List<AccountType> accountTypes, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountTypeId"], accountTypes.Select(x => (int)x).ToList(), DatabaseCriteriaOperatorEnum.In);
            return dq.GetCount(msgs);
        }

        public static List<Account> LoadByAccountTypeIds(List<AccountType> accountTypes, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountTypeId"], accountTypes.Select(x => (int)x).ToList(), DatabaseCriteriaOperatorEnum.In);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetSearchCount(string search, List<AccountType> accountTypes, bool isDeleted, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            
            if (search.IsNotEmpty())
                dq.Criterias.Add(ObjectMap["Name"], search, DatabaseCriteriaOperatorEnum.Like);

            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            dq.Criterias.Add(ObjectMap["AccountTypeId"], accountTypes.Select(x => (int)x).ToList(), DatabaseCriteriaOperatorEnum.In);
            return dq.GetCount(msgs);
        }

        public static List<Account> Search(string search, List<AccountType> accountTypes, bool isDeleted, int? start, int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            if (search.IsNotEmpty())
                dq.Criterias.Add(ObjectMap["Name"], search, DatabaseCriteriaOperatorEnum.Like);
            dq.Criterias.Add(ObjectMap["AccountTypeId"], accountTypes.Select(x => (int)x).ToList(), DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Account> FullTextSearch(string query, AccountType accountType, int? start, int? length, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var searchTerms = query.Split(' ').Where(term =>
                term.IsNotBlank() &&
                term.ToLower().NotIn(Helpers.Utilities.FullTextSearchWordsToIgnore));

            if (searchTerms.Any())
            {
                var dq = DatabaseQuery;
                dq.Connection = dc;

                var sql = $@"
                    select * from Account
                    where 
                        Contains(name, '{"\""}{searchTerms.ToStringList("*\" AND \"")}{"*\""}') and
                        IsTest = 0 and OwnerId is not NULL and AccountTypeId = {(int)accountType}
                ";

                dq.SelectRange.Start = start;
                dq.SelectRange.Length = length;

                return dq.ExecuteSelect(sql, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
            }

            return new List<Account>();
        }

        public static int LoadTotalMinutesEnrolled(int? accountId, DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;

            var sql = @"With A as
                (
	                select 
		                acc.Name as AccountName, a.Name as ActivityName, ai.StartDateTime, ai.EndDateTime, 
		                DATEDIFF(MINUTE, ai.StartDateTime, ai.EndDateTime) as TotalMinutes,
		                (select count(*) from RegistrantActivity ra where ra.ParticipationObjectId = a.ID and ra.ParticipationObjectTypeId = 1 and ra.AssignmentTypeId in (1,6,7)) as StudentsInClass
	                from 
		                ActivityInstance ai inner join
		                Activity a on ai.ActivityScheduleId = a.ActivityScheduleId inner join
		                Account acc on a.AccountId = acc.ID
                    where";

            if (accountId.HasValue)
                sql += $"(acc.ID = {accountId} or a.LocationId in (select id from Location where Location.HostAccountId = {accountId}))";
            else
                sql += "acc.IsTest = 0";

            if (startDateTime.HasValue)
                sql += $" and ai.StartDateTime >= '{startDateTime.Value.ToString()}'";

            if (endDateTime.HasValue)
                sql += $" and ai.EndDateTime <= '{endDateTime.Value.ToString()}'";

            sql += @"),
                B as
                (
	                select AccountName, ActivityName, StartDateTime, EndDateTime, TotalMinutes, StudentsInClass, TotalMinutes * StudentsInClass as TotalMinutesPerClass from A
                )
                select sum(TotalMinutesPerClass) as Minutes from B";

            var dt = dq.ExecuteSelect(sql, msgs);

            if (dt.Rows.Any())
                return dt.Rows[0][0].ToString().ToInteger();

            return 0;
        }

        public static int LoadTotalSeatsOffered(int? accountId, DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;

            var sql = @"With A as
                (
	                select 
		                a.SeatsAvailable
	                from 
		                ActivityInstance ai inner join
		                Activity a on ai.ActivityScheduleId = a.ActivityScheduleId inner join
		                Account acc on a.AccountId = acc.ID
                    where";

            if (accountId.HasValue)
                sql += $"(acc.ID = {accountId} or a.LocationId in (select id from Location where Location.HostAccountId = {accountId}))";
            else
                sql += "acc.IsTest = 0";

            if (startDateTime.HasValue)
                sql += $" and ai.StartDateTime >= '{startDateTime.Value.ToString()}'";

            if (endDateTime.HasValue)
                sql += $" and ai.EndDateTime <= '{endDateTime.Value.ToString()}'";

            sql += @")
                select sum(SeatsAvailable) from A";

            var dt = dq.ExecuteSelect(sql, msgs);

            if (dt.Rows.Any())
                return dt.Rows[0][0].ToString().ToInteger();

            return 0;
        }

        public static int LoadTotalMinutesOffered(int? accountId, DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;

            var sql = @"With A as
                (
	                select 
		                a.SeatsAvailable,
		                DATEDIFF(MINUTE, ai.StartDateTime, ai.EndDateTime) as TotalMinutes,
		                a.SeatsAvailable * DATEDIFF(MINUTE, ai.StartDateTime, ai.EndDateTime) as MaximumMinutesOffered
	                from 
		                ActivityInstance ai inner join
		                Activity a on ai.ActivityScheduleId = a.ActivityScheduleId inner join
		                Account acc on a.AccountId = acc.ID
                    where";

            if (accountId.HasValue)
                sql += $"(acc.ID = {accountId} or a.LocationId in (select id from Location where Location.HostAccountId = {accountId}))";
            else
                sql += "acc.IsTest = 0";

            if (startDateTime.HasValue)
                sql += $" and ai.StartDateTime >= '{startDateTime.Value.ToString()}'";

            if (endDateTime.HasValue)
                sql += $" and ai.EndDateTime <= '{endDateTime.Value.ToString()}'";

            sql += @")
                select sum(MaximumMinutesOffered) from A";

            var dt = dq.ExecuteSelect(sql, msgs);

            if (dt.Rows.Any())
                return dt.Rows[0][0].ToString().ToInteger();

            return 0;
        }

        public static int LoadTotalStudentsServed(int? accountId, DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;

            var sql = @"
	                select 
		                count(*)
	                from 
		                Registration r inner join 
		                Account acc on r.accountid = acc.id inner join
		                Activity a on acc.id = a.ID
                    where
                        r.FirstRegistrationId is null and
		                r.CompletionStatusId = 2";

            if (accountId.HasValue)
                sql += $" and (acc.ID = {accountId} or a.LocationId in (select id from Location where Location.HostAccountId = {accountId}))";
            else
                sql += " and acc.IsTest = 0";

            if (startDateTime.HasValue)
                sql += $" and r.CreateDate >= '{startDateTime.Value.ToString()}'";

            if (endDateTime.HasValue)
                sql += $" and r.CreateDate <= '{endDateTime.Value.ToString()}'";

            var dt = dq.ExecuteSelect(sql, msgs);

            if (dt.Rows.Any())
                return dt.Rows[0][0].ToString().ToInteger();

            return 0;
        }

        public static List<(string ActivityName, int RegistrationCount)> LoadRegistrationCountByActivity(int? accountId, DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;

            var sql = @"
	                select 
                        distinct 
                            act.Name, count(ra.id) as RegistrationCount
                        from 
                            RegistrantActivity ra inner join
                            Registrant r on ra.registrantid = r.id inner join
                            Account a on r.AccountId = a.ID inner join
                            Activity act on ra.ParticipationObjectId = act.ID and ra.ParticipationObjectTypeId = 1
                        where 
                            r.ParentRegistrantId is not null";

            if (accountId.HasValue)
                sql += $" and (acc.ID = {accountId} or a.LocationId in (select id from Location where Location.HostAccountId = {accountId}))";
            else
                sql += " and acc.IsTest = 0";

            if (startDateTime.HasValue)
                sql += $" and ra.CreateDate >= '{startDateTime.Value.ToString()}'";

            if (endDateTime.HasValue)
                sql += $" and ra.CreateDate <= '{endDateTime.Value.ToString()}'";

            sql += @"
                        group by act.name
                        order by act.Name";

            return dq.ExecuteSelect(sql, msgs)
                        .AsEnumerable()
                        .ToList()
                        .ConvertAll(row => { 
                            return (
                                ActivityName: row["Name"].ToString(),
                                RegistrationCount: row["RegistrationCount"].ToString().ToInteger()
                            ); 
                        });            
        }

        public static List<(string Grade, int RegistrationCount)> LoadRegistrationCountByGrade(int? accountId, DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;

            var sql = @"
	                select
	                    Name,
	                    Abbreviation,
	                    (
		                    select count(r.id) 
		                    from 
			                    Registration r inner join
			                    FormQuestionAnswer fqa on r.ID = fqa.RegistrationId inner join
			                    RegistrantActivity ra on r.ID = ra.RegistrationId inner join
			                    Activity a on ra.ParticipationObjectId = a.ID and ra.ParticipationObjectTypeId = 1 inner join 
			                    Account acc on a.AccountId = acc.ID
		                    where 
			                    fqa.QuestionDataTypeId = 44 and
			                    (fqa.Text = gl.Abbreviation or fqa.Text = gl.Name)";

            if (accountId.HasValue)
                sql += $" and (acc.AccountId = {accountId} or a.LocationId in (select id from Location where Location.HostAccountId = {accountId}))";
            else
                sql += " and acc.IsTest = 0";

            if (startDateTime.HasValue)
                sql += $" and ra.CreateDate >= '{startDateTime.Value.ToString()}'";

            if (endDateTime.HasValue)
                sql += $" and ra.CreateDate <= '{endDateTime.Value.ToString()}'";

            sql += @"
                        ) as RegistrationCount
                    from 
	                    GradeLevel gl
                    Group By
	                    Name,
	                    Abbreviation";

            return dq.ExecuteSelect(sql, msgs)
                        .AsEnumerable()
                        .ToList()
                        .ConvertAll(row => {
                            return (
                                Grade: row["Name"].ToString(),
                                NumberOfRegistrations: row["RegistrationCount"].ToString().ToInteger()
                            );
                        });
        }

        public static (decimal totalProcessed, decimal totalProfit) LoadTotalProcessedAndProfit(int? accountId, DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;

            var sql = $@"
                        With A as
                        (
	                        select
		                        ID,
		                        BillerAccountId,
		                        Amount,
		                        ConvenienceFeeCharged,
		                        PaymentTypeFeeCharged,
		                        PaymentTypeId,
		                        (Case PaymentTypeId
			                        When {(int)PaymentType.CreditCard} Then
				                        (Amount * {Billing.StripeHelper.StripePercentFee}) + {Billing.StripeHelper.StripeFlatFee}
			                        When {(int)PaymentType.ACH} Then
				                        (Amount * {Billing.StripeHelper.StripeBankPercentFee})
		                        End) as StripeFee
	                        From 
		                        [Transaction]
	                        Where
		                        PaymentSourceTypeId = {(int)PaymentSourceType.AfterSchoolPlatform} and
		                        ConvenienceFeeCharged is not null and 
		                        ConvenienceFeeCharged > 0 and
		                        ID in 
		                        (
			                        select 
				                        ap.TransactionId
			                        from 
				                        ActivityPayment ap inner join
				                        Activity a on ap.ParticipationObjectId = a.ID and ap.ParticipationObjectTypeId = {(int)ParticipationObjectType.Activity}
			                        where";

            if (accountId.HasValue)
                sql += $" a.AccountId = {accountId.Value} or a.LocationId in (select id from location where HostAccountId = {accountId.Value})";
            else
                sql += " and acc.IsTest = 0";

            sql += ")";

            if (startDateTime.HasValue)
                sql += $" and CreateDate >= '{startDateTime.Value.ToString()}'";

            if (endDateTime.HasValue)
                sql += $" and CreateDate <= '{endDateTime.Value.ToString()}'";

            sql += @"
                        ),
                        B As
                        (
	                        Select 
		                        ID,
		                        Amount,
		                        ConvenienceFeeCharged,
		                        PaymentTypeFeeCharged,
		                        PaymentTypeId,
		                        StripeFee,
		                        (ConvenienceFeeCharged + PaymentTypeFeeCharged - StripeFee) as Profit
	                        from A
                        )
                        Select ISNULL(Sum(Amount), 0) as Amount, ISNULL(Sum(Profit), 0) as Profit from B
                        ";

            var dt = dq.ExecuteSelect(sql, msgs);

            if (dt.Rows.Any())
                return (dt.Rows[0][0].ToString().ToDecimal(), dt.Rows[0][1].ToString().ToDecimal());

            return (0, 0);
        }

        public static List<Account> LoadConnectedEducatorAccounts(int familyAccountId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;

            var sql = $@"
	                select 
	                    a2.*
                    from 
	                    account a1 inner join
	                    registrant r1 on a1.ID = r1.AccountId inner join
	                    registrant r2 on r1.ID = r2.ParentRegistrantId inner join
	                    account a2 on r2.AccountId = a2.ID
                    where 
	                    a1.ID = {familyAccountId}
                    order by a1.Name";

            return dq.ExecuteSelect(sql, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetConnectedEducatorAccountCount(int familyAccountId, int providerAccount, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;

            var sql = $@"
	                select 
	                    count(a2.*)
                    from 
	                    account a1 inner join
	                    registrant r1 on a1.ID = r1.AccountId inner join
	                    registrant r2 on r1.ID = r2.ParentRegistrantId inner join
	                    account a2 on r2.AccountId = a2.ID
                    where 
	                    a1.ID = {familyAccountId} and
                        a2.ID = {providerAccount}
                    order by a1.Name";

            var dt = dq.ExecuteSelect(sql, msgs);

            if (dt.Rows.Any())
                return dt.Rows[0][0].ToString().ToInteger();

            return 0;
        }

        #endregion
    }
}

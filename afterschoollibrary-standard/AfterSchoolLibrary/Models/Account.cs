using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Helpers;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Microsoft.Data.SqlClient;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchoolLibrary.Models
{
    /// <summary>
    ///     The main organization and account holder in the system
    /// </summary>
    [Serializable]
    public class Account : BaseCompany
    {
        #region "ClassGenerator"

        public string StripeAccessToken
        {
            get => (string) MDV["StripeAccessToken"].Value;
            set => MDV["StripeAccessToken"].Value = value;
        }

        public string StripePublishableKey
        {
            get => (string) MDV["StripePublishableKey"].Value;
            set => MDV["StripePublishableKey"].Value = value;
        }

        public string StripeSecretKey
        {
            get => (string) MDV["StripeSecretKey"].Value;
            set => MDV["StripeSecretKey"].Value = value;
        }

        public string StripeRefreshToken
        {
            get => (string) MDV["StripeRefreshToken"].Value;
            set => MDV["StripeRefreshToken"].Value = value;
        }

        public string StripeDefaultCardId
        {
            get => (string) MDV["StripeDefaultCardId"].Value;
            set => MDV["StripeDefaultCardId"].Value = value;
        }

        /// <summary>
        ///     Required.
        /// </summary>
        public Guid Uid
        {
            get => (Guid) MDV["Uid"].Value;
            set => MDV["Uid"].Value = value;
        }

        public string UniqueName
        {
            get => (string) MDV["UniqueName"].Value;
            set => MDV["UniqueName"].Value = value;
        }

        /// <summary>
        ///     WebUser id that owns/created the account
        /// </summary>
        public int? OwnerId
        {
            get => (int?) MDV["OwnerId"].Value;
            set => MDV["OwnerId"].Value = value;
        }

        public bool IsAdminAccount
        {
            get => (bool) MDV["IsAdminAccount"].Value;
            set => MDV["IsAdminAccount"].Value = value;
        }

        public bool IsActive
        {
            get => (bool) MDV["IsActive"].Value;
            set => MDV["IsActive"].Value = value;
        }

        public bool IsAfterSchool
        {
            get => (bool) MDV["IsAfterSchool"].Value;
            set => MDV["IsAfterSchool"].Value = value;
        }

        public string StripeCustomerId
        {
            get => (string) MDV["StripeCustomerId"].Value;
            set => MDV["StripeCustomerId"].Value = value;
        }

        public int? DefaultFormId
        {
            get => (int?) MDV["DefaultFormId"].Value;
            set => MDV["DefaultFormId"].Value = value;
        }

        public int? InterestFormId
        {
            get => (int?) MDV["InterestFormId"].Value;
            set => MDV["InterestFormId"].Value = value;
        }

        public string StripeUserId
        {
            get => (string) MDV["StripeUserId"].Value;
            set => MDV["StripeUserId"].Value = value;
        }

        public string TimeZone
        {
            get => (string) MDV["TimeZone"].Value;
            set => MDV["TimeZone"].Value = value;
        }

        public string LogoUrl
        {
            get => (string) MDV["LogoUrl"].Value;
            set => MDV["LogoUrl"].Value = value;
        }

        public AccountType AccountTypeId
        {
            get => (AccountType) MDV["AccountTypeId"].Value;
            set => MDV["AccountTypeId"].Value = value;
        }

        public int? DefaultLocationId
        {
            get => (int?) MDV["DefaultLocationId"].Value;
            set => MDV["DefaultLocationId"].Value = value;
        }

        public int? MinGradeLevelOffered
        {
            get => (int?) MDV["MinGradeLevelOffered"].Value;
            set => MDV["MinGradeLevelOffered"].Value = value;
        }

        public int? MaxGradeLevelOffered
        {
            get => (int?) MDV["MaxGradeLevelOffered"].Value;
            set => MDV["MaxGradeLevelOffered"].Value = value;
        }

        public Guid InviteCodeToShare
        {
            get => (Guid) MDV["InviteCodeToShare"].Value;
            set => MDV["InviteCodeToShare"].Value = value;
        }

        public FeesPaidByType CreditCardFeesPaidBy
        {
            get => (FeesPaidByType) MDV["CreditCardFeesPaidBy"].Value;
            set => MDV["CreditCardFeesPaidBy"].Value = value;
        }

        public FeesPaidByType ConvenienceFeesPaidBy
        {
            get => (FeesPaidByType) MDV["ConvenienceFeesPaidBy"].Value;
            set => MDV["ConvenienceFeesPaidBy"].Value = value;
        }

        public bool StripeChargesEnabled
        {
            get => (bool) MDV["StripeChargesEnabled"].Value;
            set => MDV["StripeChargesEnabled"].Value = value;
        }

        public bool StripePayoutsEnabled
        {
            get => (bool) MDV["StripePayoutsEnabled"].Value;
            set => MDV["StripePayoutsEnabled"].Value = value;
        }

        public bool IsTest
        {
            get => (bool) MDV["IsTest"].Value;
            set => MDV["IsTest"].Value = value;
        }

        public bool IsDeleted
        {
            get => (bool) MDV["IsDeleted"].Value;
            set => MDV["IsDeleted"].Value = value;
        }

        public bool IsClaimed
        {
            get => (bool) MDV["IsClaimed"].Value;
            set => MDV["IsClaimed"].Value = value;
        }

        public OnboardingStepType OnboardingStepDone
        {
            get => (OnboardingStepType) MDV["OnboardingStepDone"].Value;
            set => MDV["OnboardingStepDone"].Value = value;
        }

        /// <summary>
        ///     Enables account to set contact form to be used on main page
        /// </summary>
        public int? ContactFormId
        {
            get => (int?) MDV["ContactFormId"].Value;
            set => MDV["ContactFormId"].Value = value;
        }

        /// <summary>
        ///     The main domain for the email to validate against during the claim process
        /// </summary>
        public string ClaimEmailDomain
        {
            get => (string) MDV["ClaimEmailDomain"].Value;
            set => MDV["ClaimEmailDomain"].Value = value;
        }

        /// <summary>
        ///     Subdomain associated to account used to redirect to accounts directory
        /// </summary>
        public string Subdomain
        {
            get => (string) MDV["Subdomain"].Value;
            set => MDV["Subdomain"].Value = value;
        }

        /// <summary>
        ///     default volunteer form id for the account
        /// </summary>
        public int? DefaultVolunteerFormId
        {
            get => (int?) MDV["DefaultVolunteerFormId"].Value;
            set => MDV["DefaultVolunteerFormId"].Value = value;
        }

        public bool? HasDefaultPaymentMethod
        {
            get => (bool?) MDV["HasDefaultPaymentMethod"].Value;
            set => MDV["HasDefaultPaymentMethod"].Value = value;
        }


        public Account()
        {
            Config(ObjectMap, DatabaseQuery, typeof(Account).Name, "ID");
            LoadObjectValues(ObjectMap);
            Uid = Guid.NewGuid();
            IsAdminAccount = false;
            IsActive = false;
            IsAfterSchool = false;
            AccountTypeId = AccountType.NotSet;
            InviteCodeToShare = Guid.NewGuid();
            CreditCardFeesPaidBy = FeesPaidByType.NotSet;
            ConvenienceFeesPaidBy = FeesPaidByType.NotSet;
            StripeChargesEnabled = false;
            StripePayoutsEnabled = false;
            IsTest = false;
            IsDeleted = false;
            IsClaimed = false;
            OnboardingStepDone = OnboardingStepType.NotSet;
            HasDefaultPaymentMethod = false;
        }

        public static DatabaseParameters ObjectMap
        {
            get
            {
                var _dp = new DatabaseParameters();
                var className = typeof(Account).Name;
                _dp = get_BaseObjectMap(className);
                _dp.Add(className, "StripeAccessToken", SqlDbType.NVarChar, "400");
                _dp.Add(className, "StripePublishableKey", SqlDbType.NVarChar, "400");
                _dp.Add(className, "StripeSecretKey", SqlDbType.NVarChar, "400");
                _dp.Add(className, "StripeRefreshToken", SqlDbType.NVarChar, "400");
                _dp.Add(className, "StripeDefaultCardId", SqlDbType.NVarChar, "50");
                _dp.Add(className, "Uid", SqlDbType.UniqueIdentifier);
                _dp.Add(className, "UniqueName", SqlDbType.NVarChar, "50");
                _dp.Add(className, "OwnerId", SqlDbType.Int);
                _dp.Add(className, "IsAdminAccount", SqlDbType.Bit);
                _dp.Add(className, "IsActive", SqlDbType.Bit);
                _dp.Add(className, "IsAfterSchool", SqlDbType.Bit);
                _dp.Add(className, "StripeCustomerId", SqlDbType.NVarChar, "50");
                _dp.Add(className, "DefaultFormId", SqlDbType.Int);
                _dp.Add(className, "InterestFormId", SqlDbType.Int);
                _dp.Add(className, "StripeUserId", SqlDbType.NVarChar, "100");
                _dp.Add(className, "TimeZone", SqlDbType.NVarChar, "30");
                _dp.Add(className, "LogoUrl", SqlDbType.NVarChar, "400");
                _dp.Add(className, "AccountTypeId", SqlDbType.Int);
                _dp.Add(className, "DefaultLocationId", SqlDbType.Int);
                _dp.Add(className, "MinGradeLevelOffered", SqlDbType.Int);
                _dp.Add(className, "MaxGradeLevelOffered", SqlDbType.Int);
                _dp.Add(className, "InviteCodeToShare", SqlDbType.UniqueIdentifier);
                _dp.Add(className, "CreditCardFeesPaidBy", SqlDbType.Int);
                _dp.Add(className, "ConvenienceFeesPaidBy", SqlDbType.Int);
                _dp.Add(className, "StripeChargesEnabled", SqlDbType.Bit);
                _dp.Add(className, "StripePayoutsEnabled", SqlDbType.Bit);
                _dp.Add(className, "IsTest", SqlDbType.Bit);
                _dp.Add(className, "IsDeleted", SqlDbType.Bit);
                _dp.Add(className, "IsClaimed", SqlDbType.Bit);
                _dp.Add(className, "OnboardingStepDone", SqlDbType.Int);
                _dp.Add(className, "ContactFormId", SqlDbType.Int);
                _dp.Add(className, "ClaimEmailDomain", SqlDbType.NVarChar, "50");
                _dp.Add(className, "Subdomain", SqlDbType.NVarChar, "100");
                _dp.Add(className, "DefaultVolunteerFormId", SqlDbType.Int);
                _dp.Add(className, "HasDefaultPaymentMethod", SqlDbType.Bit);
                return _dp;
            }
        }

        public static Account LoadObject(DataRow row)
        {
            var obj = new Account();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static DatabaseQuery DatabaseQuery
        {
            get
            {
                var dq = get_DatabaseQuery(ObjectMap, typeof(Account).Name);

                return dq;
            }
        }

        public static Account Load(int id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Account> Load(Hashtable criterias = null, int? start = null, int? length = null,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.UseParameterizedQuery = false;
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            return BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

        public SystemMessages Save(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if (Uid.IsEmpty())
                msgs.Add("Uid is required on Account object", MessageType.Error);
            if (Name.IsEmpty())
                msgs.Add("Name is required on Account object", MessageType.Error);

            if (msgs.HasNoErrors())
                msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Account).Name));

            return msgs;
        }

        public SystemMessages Delete(DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Account).Name);
        }

        public static SystemMessages Create(DatabaseConnection dc = null)
        {
            return _Create(DatabaseQuery, dc, ObjectMap);
        }

        public static SystemMessages Alter(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            DatabaseParameters dps;

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["StripeAccessToken"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["StripePublishableKey"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["StripeSecretKey"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["StripeRefreshToken"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["StripeDefaultCardId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["Uid"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["UniqueName"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["OwnerId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsAdminAccount"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsActive"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsAfterSchool"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["StripeCustomerId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["DefaultFormId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["InterestFormId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["StripeUserId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["TimeZone"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["LogoUrl"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["AccountTypeId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["DefaultLocationId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["MinGradeLevelOffered"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["MaxGradeLevelOffered"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["InviteCodeToShare"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["CreditCardFeesPaidBy"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ConvenienceFeesPaidBy"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["StripeChargesEnabled"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["StripePayoutsEnabled"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsTest"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsDeleted"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["IsClaimed"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["OnboardingStepDone"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ContactFormId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["ClaimEmailDomain"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["Subdomain"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["DefaultVolunteerFormId"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));

            dps = new DatabaseParameters();
            dps.Add(ObjectMap["HasDefaultPaymentMethod"]);
            msgs.AddRange(_Alter(DatabaseQuery, dc, dps));


            return msgs;
        }

        public Account Reload(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Load(ID.Value, msgs, dc);
        }

        public static List<Account> LoadByOwnerId(int OwnerId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["OwnerId"], OwnerId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static Account LoadByUniqueName(string UniqueName, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["UniqueName"], UniqueName);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static Account LoadByIsAdminAccount(bool IsAdminAccount, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["IsAdminAccount"], IsAdminAccount);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static Account LoadByUid(Guid Uid, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Uid"], Uid);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Account> LoadByAccountTypeId(AccountType AccountTypeId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountTypeId"], AccountTypeId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static Account LoadByStripeUserId(string StripeUserId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["StripeUserId"], StripeUserId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static Account LoadByStripeCustomerId(string StripeCustomerId, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["StripeCustomerId"], StripeCustomerId);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static Account LoadByInviteCodeToShare(Guid InviteCodeToShare, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["InviteCodeToShare"], InviteCodeToShare);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static Account LoadBySubdomain(string Subdomain, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Subdomain"], Subdomain);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        #endregion

        #region "UserDefined"

        public bool HasAccountOwner => OwnerId.HasValue;

        public bool HasStripeConnectedAccount => StripeUserId.IsNotEmpty();

        public bool IsSchool => AccountTypeId == AccountType.School;

        public bool IsActivityProvider => AccountTypeId == AccountType.ActivityProvider;

        public bool IsCreditCardFeeTakenOutOfCost => CreditCardFeesPaidBy == FeesPaidByType.Account;

        public bool IsConvenienceFeeTakenOutOfCost => ConvenienceFeesPaidBy == FeesPaidByType.Account;

        public string AccountNumber
        {
            get
            {
                if (ID.HasValue)
                    return ID.Value.ToString().PadLeft(7, '0');
                return "";
            }
        }

        public string HomePageUrl
        {
            get
            {
                if (Subdomain.IsNotEmpty())
                    return $"http{(Utilities.UseSSL() ? "s" : "")}://{Subdomain}";

                return $"{Utilities.BaseUrl()}/{UniqueName}";
            }
        }

        public string BrowseUrl => $"{HomePageUrl}/browse";
        
        public string BrowseVolunteerOpportunitiesUrl => $"{HomePageUrl}/browseVolunteerOpportunities";

        public string ContactUrl => $"{HomePageUrl}/contact";

        public string PayUrl => $"{HomePageUrl}/pay";

        public string ShortHomePageUrl => $"{Utilities.ShortBaseUrl()}/{UniqueName}";

        public string HostAccountInviteUrl => $"{Utilities.BaseUrl()}/signup/welcome?invite={InviteCodeToShare}";

        public string HostAccountShortInviteUrl =>
            $"{Utilities.ShortBaseUrl()}/signup/welcome?invite={InviteCodeToShare}";

        public string HostAccountTeacherInviteUrl =>
            $"{Utilities.BaseUrl()}/signup/welcome?invite={InviteCodeToShare}&signuptype={(int) SignUpType.Teacher1099}";

        public string UserInviteUrl => $"{Utilities.BaseUrl()}/signup/user?invite={InviteCodeToShare}";

        public string ClaimAccountUrl => $"{Utilities.BaseUrl()}/signup/claim?invite={InviteCodeToShare}";

        public bool HasPaymentCard => StripeDefaultCardId.IsNotEmpty();

        public string TimeZoneAbbr
        {
            get
            {
                if (TimeZone.IsNotBlank())
                    return TimeZone.Split('-')[1].Trim();
                return "";
            }
        }

        public static int Load(List<AccountType> accountTypes, bool isDeleted, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountTypeId"], accountTypes.Select(x => (int) x).ToList(),
                DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            return dq.GetCount(msgs);
        }

        public static int GetCount(List<AccountType> accountTypes, bool isDeleted, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountTypeId"], accountTypes.Select(x => (int) x).ToList(),
                DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            return dq.GetCount(msgs);
        }

        public static List<Account> LoadByAccountTypeIds(List<AccountType> accountTypes, bool isDeleted, int? start,
            int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountTypeId"], accountTypes.Select(x => (int) x).ToList(),
                DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetSearchCount(string search, List<AccountType> accountTypes, bool isDeleted,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;

            if (search.IsNotEmpty())
                dq.Criterias.Add(ObjectMap["Name"], search, DatabaseCriteriaOperatorEnum.Like);

            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            dq.Criterias.Add(ObjectMap["AccountTypeId"], accountTypes.Select(x => (int) x).ToList(),
                DatabaseCriteriaOperatorEnum.In);
            return dq.GetCount(msgs);
        }

        public static List<Account> Search(string search, List<AccountType> accountTypes, bool isDeleted, int? start,
            int? length, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            if (search.IsNotEmpty())
                dq.Criterias.Add(ObjectMap["Name"], search, DatabaseCriteriaOperatorEnum.Like);
            dq.Criterias.Add(ObjectMap["AccountTypeId"], accountTypes.Select(x => (int) x).ToList(),
                DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Account> Search(string search, List<AccountType> accountTypes, bool isDeleted,
            string OrderByProperty, OrderByDirectionEnum orderByDirection, int? start, int? length,
            SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            if (search.IsNotEmpty())
                dq.Criterias.Add(ObjectMap["Name"], search, DatabaseCriteriaOperatorEnum.Like);
            dq.Criterias.Add(ObjectMap["AccountTypeId"], accountTypes.Select(x => (int) x).ToList(),
                DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["IsDeleted"], isDeleted);
            dq.SelectRange.Start = start;
            dq.SelectRange.Length = length;
            dq.OrderBys.Add(OrderByProperty, orderByDirection);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static List<Account> FullTextSearch(string query, List<AccountType> accountTypes, int? start,
            int? length, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var searchTerms = query.Split(' ').Where(term =>
                term.IsNotBlank() &&
                term.ToLower().NotIn(Utilities.FullTextSearchWordsToIgnore));

            if (searchTerms.Any())
            {
                var dq = DatabaseQuery;
                dq.Connection = dc;

                var sql = $@"
                    select * from Account
                    where 
                        Contains(name, '{"\""}{searchTerms.ToStringList("*\" AND \"")}{"*\""}') 
                        AND IsActive = 1 
                        AND IsDeleted = 0 
                        AND IsTest = 0 
                        AND OwnerId is not NULL 
                        AND AccountTypeId in ({accountTypes.Select(x => (int) x).ToList().ToStrings().ToStringList()})
                    Order by Name asc
                ";

                dq.SelectRange.Start = start;
                dq.SelectRange.Length = length;

                return dq.ExecuteSelect(sql, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
            }

            return new List<Account>();
        }

        public static int LoadTotalMinutesEnrolled(int? accountId, DateTime? startDateTime, DateTime? endDateTime,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            // The query is causing long load times. It needs to be optimized.
            // As a quick fix to help customers, short-circuit and return nothing.
            return 0;
            
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
                sql +=
                    $"(acc.ID = {accountId} or a.LocationId in (select id from Location where Location.HostAccountId = {accountId}))";
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
                select ISNULL(sum(TotalMinutesPerClass), 0) as Minutes from B";

            var dt = dq.ExecuteSelect(sql, msgs);

            if (dt.Rows.Any())
                return dt.Rows[0][0].ToString().ToInteger();

            return 0;
        }

        public static int LoadTotalSeatsOffered(int? accountId, DateTime? startDateTime, DateTime? endDateTime,
            SystemMessages msgs, DatabaseConnection dc = null)
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
                sql +=
                    $"(acc.ID = {accountId} or a.LocationId in (select id from Location where Location.HostAccountId = {accountId}))";
            else
                sql += "acc.IsTest = 0";

            if (startDateTime.HasValue)
                sql += $" and ai.StartDateTime >= '{startDateTime.Value.ToString()}'";

            if (endDateTime.HasValue)
                sql += $" and ai.EndDateTime <= '{endDateTime.Value.ToString()}'";

            sql += @")
                select ISNULL(sum(SeatsAvailable), 0) from A";

            var dt = dq.ExecuteSelect(sql, msgs);

            if (dt.Rows.Any())
                return dt.Rows[0][0].ToString().ToInteger();

            return 0;
        }

        public static int LoadTotalMinutesOffered(int? accountId, DateTime? startDateTime, DateTime? endDateTime,
            SystemMessages msgs, DatabaseConnection dc = null)
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
                sql +=
                    $"(acc.ID = {accountId} or a.LocationId in (select id from Location where Location.HostAccountId = {accountId}))";
            else
                sql += "acc.IsTest = 0";

            if (startDateTime.HasValue)
                sql += $" and ai.StartDateTime >= '{startDateTime.Value.ToString()}'";

            if (endDateTime.HasValue)
                sql += $" and ai.EndDateTime <= '{endDateTime.Value.ToString()}'";

            sql += @")
                select ISNULL(sum(MaximumMinutesOffered), 0) from A";

            var dt = dq.ExecuteSelect(sql, msgs);

            if (dt.Rows.Any())
                return dt.Rows[0][0].ToString().ToInteger();

            return 0;
        }

        public static int LoadTotalStudentsServed(int? accountId, DateTime? startDateTime, DateTime? endDateTime,
            SystemMessages msgs, DatabaseConnection dc = null)
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
                sql +=
                    $" and (acc.ID = {accountId} or a.LocationId in (select id from Location where Location.HostAccountId = {accountId}))";
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

        public static List<(string ActivityName, int RegistrationCount)> LoadRegistrationCountByActivity(int? accountId,
            DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs, DatabaseConnection dc = null)
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
                sql +=
                    $" and (acc.ID = {accountId} or a.LocationId in (select id from Location where Location.HostAccountId = {accountId}))";
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
                .ConvertAll(row =>
                {
                    return (
                        ActivityName: row["Name"].ToString(),
                        RegistrationCount: row["RegistrationCount"].ToString().ToInteger()
                    );
                });
        }

        public static List<(string Grade, int RegistrationCount)> LoadRegistrationCountByGrade(int? accountId,
            DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs, DatabaseConnection dc = null)
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
                sql +=
                    $" and (acc.AccountId = {accountId} or a.LocationId in (select id from Location where Location.HostAccountId = {accountId}))";
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
                .ConvertAll(row =>
                {
                    return (
                        Grade: row["Name"].ToString(),
                        NumberOfRegistrations: row["RegistrationCount"].ToString().ToInteger()
                    );
                });
        }

        public static (decimal totalProcessed, decimal totalProfit) LoadTotalProcessedAndProfit(int? accountId,
            DateTime? startDateTime, DateTime? endDateTime, SystemMessages msgs, DatabaseConnection dc = null)
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
			                        When {(int) PaymentType.CreditCard} Then
				                        (Amount * {StripeHelper.StripePercentFee}) + {StripeHelper.StripeFlatFee}
			                        When {(int) PaymentType.ACH} Then
				                        (Amount * {StripeHelper.StripeBankPercentFee})
		                        End) as StripeFee
	                        From 
		                        [Transaction]
	                        Where
		                        PaymentSourceTypeId = {(int) PaymentSourceType.AfterSchoolPlatform} and
		                        ConvenienceFeeCharged is not null and 
		                        ConvenienceFeeCharged > 0 and
		                        ID in 
		                        (
			                        select 
				                        ap.TransactionId
			                        from 
				                        ActivityPayment ap inner join
				                        Activity a on ap.ParticipationObjectId = a.ID and ap.ParticipationObjectTypeId = {(int) ParticipationObjectType.Activity}
			                        where";

            if (accountId.HasValue)
                sql +=
                    $" a.AccountId = {accountId.Value} or a.LocationId in (select id from location where HostAccountId = {accountId.Value})";
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

        public static List<Account> LoadConnectedEducatorAccounts(int familyAccountId, SystemMessages msgs,
            DatabaseConnection dc = null)
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

        public static int GetConnectedEducatorAccountCount(int familyAccountId, int providerAccount,
            SystemMessages msgs, DatabaseConnection dc = null)
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


        /// <summary>
        ///     Global search query for accounts with different criteria
        /// </summary>
        /// <param name="hostAccountId">Parent account id</param>
        /// <param name="search">Search would be string like m, which will be resulted into '"m*"' for full text sql</param>
        /// <param name="enrollToday"></param>
        /// <param name="grades"></param>
        /// <param name="ages"></param>
        /// <param name="gender"></param>
        /// <param name="services"></param>
        /// <param name="timing"></param>
        /// <param name="pricing"></param>
        /// <param name="cost"></param>
        /// <param name="starting"></param>
        /// <param name="other"></param>
        /// <param name="specialNeeds"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="PageNum"></param>
        /// <param name="PageSize"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <param name="neighborhoods"></param>
        /// <param name="currentRowNumber">while user scroll, this will be updated and considered as page size</param>
        /// <returns></returns>
        public static DataTable SearchVisitorAccountsByHostAccount(int hostAccountId, string search, string enrollToday,
            string grades, string ages, string gender, string services, string timing, string pricing, string cost,
            string starting, string other, string specialNeeds, decimal? latitude, decimal? longitude, int PageNum,
            int PageSize, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            using (var connection = new SqlConnection(BasicLibrary.Utilities.ConnectionString))
            {
                var sql = SearchQuery.LoadAccountSearchQuery(enrollToday, cost, starting, latitude, longitude, PageNum,
                    PageSize, hostAccountId > 0);

                var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@AccountId", hostAccountId <= 0 ? Convert.DBNull : hostAccountId);
                command.Parameters.AddWithValue("@SearchTerm", search.IsNotBlank() ? $"\"*{search}*\"" : "\"\"");
                command.Parameters.AddWithValue("@Grades", grades.IsNotEmpty() ? grades : Convert.DBNull);
                command.Parameters.AddWithValue("@Age", ages.IsNotEmpty() ? ages : Convert.DBNull);
                command.Parameters.AddWithValue("@Gender", gender.IsNotEmpty() ? gender : Convert.DBNull);
                command.Parameters.AddWithValue("@Cost", cost.IsNotEmpty() ? "1" : Convert.DBNull);
                command.Parameters.AddWithValue("@Starting", starting.IsNotEmpty() ? "1" : Convert.DBNull);
                command.Parameters.AddWithValue("@IsELearningCenter",
                    other.IsNotBlank() && other.Contains("e_learning") ? "1" : Convert.DBNull);
                command.Parameters.AddWithValue("@IsVirtualProgram",
                    other.IsNotBlank() && other.Contains("virtual_programming") ? "1" : Convert.DBNull);
                command.Parameters.AddWithValue("@IsOpenDuringCovid",
                    other.IsNotBlank() && other.Contains("covid_protections") ? "1" : Convert.DBNull);
                command.Parameters.AddWithValue("@Services", services.IsNotEmpty() ? services : Convert.DBNull);
                command.Parameters.AddWithValue("@Timing", timing.IsNotEmpty() ? timing : Convert.DBNull);
                command.Parameters.AddWithValue("@PricingOptions", pricing.IsNotEmpty() ? pricing : Convert.DBNull);
                command.Parameters.AddWithValue("@SpecialNeeds",
                    specialNeeds.IsNotEmpty() ? specialNeeds : Convert.DBNull);
                command.Parameters.AddWithValue("@Latitude",
                    latitude.HasValue && longitude.HasValue ? latitude.Value : Convert.DBNull);
                command.Parameters.AddWithValue("@Longitude",
                    latitude.HasValue && longitude.HasValue ? longitude.Value : Convert.DBNull);

                try
                {
                    connection.Open();
                    using (var dr = command.ExecuteReader())
                    {
                        var tb = new DataTable();
                        tb.Load(dr);
                        connection.Dispose();
                        return tb;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                return null;
            }
        }

        public static List<string> LoadAllSubdomains(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["Subdomain"], null, DatabaseCriteriaOperatorEnum.IsNot);

            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).Select(a => a.Subdomain).ToList();
        }

        public static List<Account> LoadByIdsByAccountTypes(List<int> ids, List<AccountType> accountTypes,
            SystemMessages msgs, DatabaseConnection dc)
        {
            var dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], ids, DatabaseCriteriaOperatorEnum.In);
            dq.Criterias.Add(ObjectMap["IsActive"], true);
            dq.Criterias.Add(ObjectMap["IsDeleted"], false);
            dq.Criterias.Add(ObjectMap["IsTest"], false);
            dq.Criterias.Add(ObjectMap["AccountTypeId"], accountTypes.Select(x => (int) x).ToList(),
                DatabaseCriteriaOperatorEnum.In);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        #endregion
    }
}
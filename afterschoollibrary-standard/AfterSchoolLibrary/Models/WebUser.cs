using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Data;
using System.Collections;
using System.Security.Cryptography;

namespace AfterSchoolLibrary
{
	[Serializable]
    public class WebUser : BaseWebUser
    {
		#region "ClassGenerator"

		public string FacebookId { get { return (string)MDV["FacebookId"].Value; } set { MDV["FacebookId"].Value = value; } }
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public bool IsActive { get { return (bool)MDV["IsActive"].Value; } set { MDV["IsActive"].Value = value; } }
		public DateTime? LastActivity { get { return (DateTime?)MDV["LastActivity"].Value; } set { MDV["LastActivity"].Value = value; } }
		public bool VerifiedAccount { get { return (bool)MDV["VerifiedAccount"].Value; } set { MDV["VerifiedAccount"].Value = value; } }
		public bool IsRegistered { get { return (bool)MDV["IsRegistered"].Value; } set { MDV["IsRegistered"].Value = value; } }
		public int? DefaultAccountId { get { return (int?)MDV["DefaultAccountId"].Value; } set { MDV["DefaultAccountId"].Value = value; } }
		

        public WebUser()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(WebUser).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.Uid = Guid.NewGuid();
			this.IsActive = false;
			this.VerifiedAccount = false;
			this.IsRegistered = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(WebUser).Name;
                _dp = BaseWebUser.get_BaseObjectMap(className);
				_dp.Add(className, "FacebookId", System.Data.SqlDbType.NVarChar, 100);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "IsActive", System.Data.SqlDbType.Bit);
				_dp.Add(className, "LastActivity", System.Data.SqlDbType.DateTime);
				_dp.Add(className, "VerifiedAccount", System.Data.SqlDbType.Bit);
				_dp.Add(className, "IsRegistered", System.Data.SqlDbType.Bit);
				_dp.Add(className, "DefaultAccountId", System.Data.SqlDbType.Int);
				
                return _dp;
            }
        }

        protected static WebUser LoadObject(System.Data.DataRow row)
        {
            WebUser obj = new WebUser();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(WebUser).Name);
				
                return dq;
            }
        }

        public static WebUser Load(int id, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<WebUser> Load(Hashtable criterias = null, int? start = null, int? length = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return WebUser.BaseLoad(criterias, ObjectMap, dq).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return WebUser.BaseGetCount(criterias, ObjectMap, dq);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.Uid.IsEmpty())
				msgs.Add("Uid is required on WebUser object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(WebUser).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(WebUser).Name);
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
			dps.Add(ObjectMap["FacebookId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsActive"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["LastActivity"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["VerifiedAccount"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsRegistered"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["DefaultAccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public WebUser Reload(BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return WebUser.Load(this.ID.Value, dc);
		}
		
		public static WebUser LoadByUid(Guid Uid, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static WebUser LoadByUidByIsActive(Guid Uid, bool IsActive, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		public static List<WebUser> LoadByIsActive(bool IsActive, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["IsActive"], IsActive);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static WebUser LoadByFacebookId(string FacebookId, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["FacebookId"], FacebookId);
			return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public string GetInviteUrl(Account account)
        {
            return AfterSchoolLibrary.Email.BaseUrl(account.IsAfterSchool) + $"signup/new/{this.Uid.ToString()}?a={account.Uid}";
        }

        public static WebUser LoadByEmail(string email, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["email"], email);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static bool IsEmailInUseByAnotherUser(string email, int currentUserId, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["email"], email);
            dq.Criterias.Add(ObjectMap["ID"], currentUserId, DatabaseCriteriaOperatorEnum.NotEqualTo);
            return dq.GetCount() > 0;
        }

        public static bool ExistsByEmail(string email, DatabaseConnection dc = null)
        {
            return GetCount(
                criterias: new Hashtable(){
                    {"email", email}
                },
                dc: dc
                ) > 0;
        }

        public static bool ExistsByUid(Guid uid, DatabaseConnection dc = null)
        {
            return GetCount(
                criterias: new Hashtable(){
                    {"uid", uid}
                },
                dc: dc
                ) > 0;
        }

        public static WebUser LoadByUsername(string username, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["username"], username);
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static WebUser LoginByUsername(string username, string password, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["username"], username);
            dq.Criterias.Add(ObjectMap["password"], WebUser.EncryptPassword(password));
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static WebUser LoginByEmail(string email, string password, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["email"], email);
            dq.Criterias.Add(ObjectMap["password"], WebUser.EncryptPassword(password));
            return dq.Select().AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static string GenerateRandomCryptographicKey(int keyLength)
        {
            return Convert.ToBase64String(GenerateRandomCryptographicBytes(keyLength));
        }


        public static byte[] GenerateRandomCryptographicBytes(int keyLength)
        {
            RNGCryptoServiceProvider rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            byte[] randomBytes = new byte[keyLength];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return randomBytes;
        }

 
        public static (byte[] salt, byte[] hash) SaltandHashPassword(string password)
        {
            byte[] saltBytes = GenerateRandomCryptographicBytes(32 - password.Length);
            byte[] passwordinBytes = Encoding.UTF8.GetBytes(password);
            List<byte> passwordSalted = new List<byte>();
            passwordSalted.AddRange(passwordinBytes);
            passwordSalted.AddRange(saltBytes);
            byte[] hashedBytes = SHA256.Create().ComputeHash(passwordSalted.ToArray());
            return (saltBytes, hashedBytes);
        }

        public static string EncryptPassword(string password)
        {
            return Convert.ToBase64String(Encryption.Encrypt(password, Utilities.AppAESKey, Utilities.AppIV));
        }

        public static string DecryptPassword(string password)
        {
            return Encryption.DecryptToString(Convert.FromBase64String(password), Utilities.AppAESKey, Utilities.AppIV);
        }
        public SystemMessages SendInvite(string email, string message, WebUser loggedInUser, Account account, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var subject = string.Format("You have been invited by {0} to use {1}", account.Name, Utilities.AppName(account.IsAfterSchool));
            var body = "";

            if (!message.IsEmpty())
                body += "<p>" + message + "</p><p>-------------------------------</p>";

            if (!this.IsActive)
                body += $"<p>Click on the following link to create your account and start using {Utilities.AppName(account.IsAfterSchool)}: <a href=\"{this.GetInviteUrl(account)}\">{this.GetInviteUrl(account)}</a></p>";
            else
                body += $"<p>Click the following login link to access \"{account.Name}\": <a href=\"{AfterSchoolLibrary.Email.BaseUrl(account.IsAfterSchool)}login?group={account.ID.Value.ToString()}\">{AfterSchoolLibrary.Email.HostName(account.IsAfterSchool)}/login</a></p>";

            body += "<p>Thank you!!</p>";
            body += AfterSchoolLibrary.Email.GetAppSignature(isAfterSchool: account.IsAfterSchool, utm_campaign: "invite_team_member");

            msgs.AddRange(SendGridHelper.SendMail(
                fromEmail: new SendGrid.Helpers.Mail.EmailAddress(AfterSchoolLibrary.Email.AppEmailAddress, Utilities.AppName(account.IsAfterSchool)),
                toEmail: new SendGrid.Helpers.Mail.EmailAddress(email),
                subject: subject,
                body: body,
                replyTo: new SendGrid.Helpers.Mail.EmailAddress(loggedInUser.Email, loggedInUser.DisplayName),
                emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                ));

            return msgs;
        }

        public List<Account> GetAccounts(DatabaseConnection dc = null)
        {
            return AccountWebUser.LoadByWebUserIdByIsActive(this.ID.Value, true, dc).Select(x => x.ToAccount()).ToList();
        }

        public UserType GetUserType(Account a, DatabaseConnection dc = null)
        {
            if (!a.IsEmpty())
            {
                var awu = AccountWebUser.LoadByAccountIdByWebUserId(a.ID.Value, this.ID.Value, dc);
                if (!awu.IsEmpty())
                    return awu.UserTypeId;
            }
            return UserType.NotSet;
        }
        public SystemMessages UpdateLastActivity(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            this.LastActivity = DateTime.UtcNow;
            msgs.AddRange(this.Save(dc));
            return msgs;
        }
        public SystemMessages UpdateLastLogin(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            this.LastLogin = DateTime.UtcNow;
            msgs.AddRange(this.Save(dc));
            return msgs;
        }
        public bool HasAccessToFeature(FeatureType ft, Account g, DatabaseConnection dc = null)
        {
            if (!g.IsEmpty())
            {
                var awu = AccountWebUser.LoadByAccountIdByWebUserId(g.ID.Value, this.ID.Value, dc);
                if (awu.IsEmpty() == false && awu.UserTypeId != UserType.NotSet)
                    return awu.HasAccessToFeature(ft);
            }
            return false;
        }
        public bool HasAccessTo(Account a, DatabaseConnection dc = null)
        {
            var awu = AccountWebUser.LoadByAccountIdByWebUserId(a.ID.Value, this.ID.Value, dc);
            return awu.IsNotEmpty() && awu.IsActive;
        }
        public bool HasAccessToActivity(Activity a, DatabaseConnection dc = null)
        {
            return ActivityWebUser.LoadByActivityIdByWebUserId(a.ID.Value, this.ID.Value, dc).IsNotEmpty();
        }
        public bool IsActivityLeader(Activity a, DatabaseConnection dc = null)
        {
            var awu = this.GetActivityWebUser(a, dc);
            return awu.IsNotEmpty() && awu.IsLeader;
        }
        public ActivityWebUser GetActivityWebUser(Activity a, DatabaseConnection dc = null)
        {
            return a.ID.HasValue ? ActivityWebUser.LoadByActivityIdByWebUserId(a.ID.Value, this.ID.Value, dc): null;
        }
        public bool IsActiveFor(Account a, DatabaseConnection dc = null)
        {
            return this.HasAccessTo(a, dc);
        }

        public static string GenerateUsername(string firstName, string lastName, DatabaseConnection dc = null)
        {
            //unique name
            var reg = new System.Text.RegularExpressions.Regex(
                BasicLibrary.Utilities.UrlFriendlyRegEx, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
                );

            string username = $"{firstName} {lastName}";
            string origUniqueName = reg.Replace(username, "").MaxChars(25);
            string uniqueName = origUniqueName;
            var duplicate = WebUser.LoadByUsername(uniqueName, dc);
            int count = 1;
            while (!duplicate.IsEmpty())
            {
                uniqueName = origUniqueName + count.ToString();
                count++;
                duplicate = WebUser.LoadByUsername(uniqueName, dc);
            }
            return uniqueName;
        }

        public SystemMessages SendVerificationEmail(bool isAfterSchool)
        {
            return EmailHelper.SendEmailVerification(this, isAfterSchool);
        } 

        public SystemMessages MarkAsVerified(DatabaseConnection dc = null)
        {
            this.VerifiedAccount = true;
            this.LastModifiedBy = this.ID.Value;
            this.LastModifiedDate = DateTime.UtcNow;
            return this.Save(dc);
        }

        public SystemMessages ChangePassword(string password, DatabaseConnection dc = null)
        {
            this.Password = WebUser.EncryptPassword(password);
            this.LastModifiedBy = this.ID;
            this.LastModifiedDate = DateTime.UtcNow;
            return this.Save(dc);
        }

        public SystemMessages SendPasswordResetToken(bool isAfterSchool, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var token = PasswordResetToken.AddOrUpdateToken(this, msgs, dc);

            msgs.AddRange(EmailHelper.SendPasswordReset(this, token, isAfterSchool));

            return msgs;
        }

        public Registrant CreateRegistrant(RegistrantType registrantType, bool isMaster, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var registrant = new Registrant();
            registrant.RegistrantTypeId = registrantType;
            registrant.IsMaster = isMaster;
            registrant.IsContactOnly = true;
            registrant.WebUserId = this.ID;
            registrant.CreateDate = registrant.LastModifiedDate = DateTime.UtcNow;
            registrant.CreatedBy = registrant.LastModifiedBy = this.ID;
            msgs.AddRange(registrant.Save(dc));

            return registrant;
        }

        public List<Registrant> GetRegistrants(DatabaseConnection dc = null)
        {
            return Registrant.LoadByWebUserIdByIsMaster(this.ID.Value, true, dc);
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Linq;
using SendGrid.Helpers.Mail;

namespace AfterSchoolLibrary.Services
{
    public static class WebUserService
    {
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

        public static (byte[] saltedbytes, byte[] hashedbytes) SaltandHashPassword(string password)
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
        public static SystemMessages SendInviteToAccount(this WebUser user, Account account, string message, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var subject = $"You have been invited by {account.Name} to use {Utilities.AppName(account.IsAfterSchool)}";
            var body = "";

            if (!message.IsEmpty())
                body += "<p>" + message + "</p><p>-------------------------------</p>";

            if (!user.IsActive)
                body += $"<p>Click on the following link to create your account and start using {Utilities.AppName(account.IsAfterSchool)}: <a href=\"{user.GetInviteUrl(account)}\">{user.GetInviteUrl(account)}</a></p>";
            else
                body += $"<p>Click the following login link to access \"{account.Name}\": <a href=\"{Email.BaseUrl(account.IsAfterSchool)}login?group={account.ID.Value.ToString()}\">{Email.HostName(account.IsAfterSchool)}/login</a></p>";

            body += "<p>Thank you!!</p>";
            body += Email.GetAppSignature(isAfterSchool: account.IsAfterSchool, utm_campaign: "invite_team_member");

            msgs.AddRange(SendGridHelper.SendMail(
                fromEmail: new EmailAddress(Email.AppEmailAddress, Utilities.AppName(account.IsAfterSchool)),
                toEmail: new EmailAddress(user.Email),
                subject: subject,
                body: body,
                replyTo: new EmailAddress(loggedInUser.Email, loggedInUser.DisplayName),
                emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                ));

            return msgs;
        }

        public static List<Account> GetAccounts(this WebUser user, DatabaseConnection dc = null)
        {
            return AccountWebUser.LoadByWebUserIdByIsActive(user.ID.Value, true, dc: dc)
                .Select(x => x.ToAccount())
                .ToList();
        }

        public static UserType GetUserType(this WebUser user, Account account, DatabaseConnection dc = null)
        {
            if (!account.IsEmpty())
            {
                var awu = AccountWebUser.LoadByAccountIdByWebUserId(account.ID.Value, user.ID.Value, dc: dc);
                if (!awu.IsEmpty())
                    return awu.UserTypeId;
            }
            return UserType.NotSet;
        }
        public static SystemMessages UpdateLastActivity(this WebUser user, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            user.LastActivity = DateTime.UtcNow;
            msgs.AddRange(user.Save(dc: dc));
            return msgs;
        }
        public static SystemMessages UpdateLastLogin(this WebUser user, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            user.LastLogin = DateTime.UtcNow;
            msgs.AddRange(user.Save(dc: dc));
            return msgs;
        }
        public static bool HasAccessToFeature(this WebUser user, FeatureType ft, Account account, DatabaseConnection dc = null)
        {
            if (!account.IsEmpty())
            {
                var awu = AccountWebUser.LoadByAccountIdByWebUserId(account.ID.Value, user.ID.Value, dc: dc);
                if (awu.IsEmpty() == false && awu.UserTypeId != UserType.NotSet)
                    return awu.HasAccessToFeature(ft);
            }
            return false;
        }
        public static bool HasAccessTo(this WebUser user, Account a, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var awu = AccountWebUser.LoadByAccountIdByWebUserId(a.ID.Value, user.ID.Value, msgs, dc);
            return awu.IsNotEmpty() && awu.IsActive;
        }
        public static bool HasAccessToActivity(this WebUser user, Activity a, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityWebUser.LoadByActivityIdByWebUserId(a.ID.Value, user.ID.Value, msgs, dc).IsNotEmpty();
        }
        public static bool IsActivityLeader(this WebUser user, Activity a, DatabaseConnection dc = null)
        {
            var awu = user.GetActivityWebUser(a, dc: dc);
            return awu.IsNotEmpty() && awu.IsLeader;
        }
        public static ActivityWebUser GetActivityWebUser(this WebUser user, Activity a, DatabaseConnection dc = null)
        {
            return a.ID.HasValue ? ActivityWebUser.LoadByActivityIdByWebUserId(a.ID.Value, user.ID.Value, dc: dc) : null;
        }
        public static bool IsActiveFor(this WebUser user, Account a, DatabaseConnection dc = null)
        {
            return user.HasAccessTo(a, dc: dc);
        }

        public static string GenerateUsername(string firstName, string lastName, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            //unique name
            var reg = new System.Text.RegularExpressions.Regex(
                BasicLibrary.Utilities.UrlFriendlyRegEx,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
                );

            string username = $"{firstName} {lastName}";
            string origUniqueName = reg.Replace(username, "").MaxChars(25);
            string uniqueName = origUniqueName;
            var duplicate = WebUser.LoadByUsername(uniqueName, msgs, dc);
            int count = 1;
            while (!duplicate.IsEmpty())
            {
                uniqueName = origUniqueName + count.ToString();
                count++;
                duplicate = WebUser.LoadByUsername(uniqueName, msgs, dc);
            }
            return uniqueName;
        }

        public static SystemMessages SendVerificationEmail(this WebUser user, bool isAfterSchool)
        {
            return EmailHelper.SendEmailVerification(user, isAfterSchool);
        }

        public static SystemMessages MarkAsVerified(this WebUser user, DatabaseConnection dc = null)
        {
            user.VerifiedAccount = true;
            user.LastModifiedBy = user.ID.Value;
            user.LastModifiedDate = DateTime.UtcNow;
            return user.Save(dc: dc);
        }

        public static SystemMessages ChangePassword(this WebUser user, string password, DatabaseConnection dc = null)
        {
            user.Password = WebUserService.EncryptPassword(password);
            user.LastModifiedBy = user.ID;
            user.LastModifiedDate = DateTime.UtcNow;
            return user.Save(dc: dc);
        }

        public static SystemMessages SendPasswordResetToken(this WebUser user, bool isAfterSchool, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var token = PasswordResetToken.AddOrUpdateToken(user, msgs, dc);

            if (msgs.HasNoErrors())
                msgs.AddRange(EmailHelper.SendPasswordReset(user, token, isAfterSchool));

            return msgs;
        }

        /// <summary>
        /// Create basic registrant/student record for a user, not attached to an account, so it's attached to the user record, data can be added later
        /// </summary>
        /// <param name="user"></param>
        /// <param name="registrantType"></param>
        /// <param name="isMaster"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static Registrant CreateRegistrant(this WebUser user, RegistrantType registrantType, bool isMaster, bool isParentOrGuardian, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var registrant = new Registrant
            {
                RegistrantTypeId = registrantType,
                IsMaster = isMaster,
                IsContactOnly = isParentOrGuardian,
                IsParentOrGuardian = isParentOrGuardian,
                WebUserId = user.ID,
                CreateDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = user.ID,
                LastModifiedBy = user.ID
            };
            msgs.AddRange(registrant.Save(dc));

            return registrant;
        }

        public static List<Registrant> GetRegistrants(this WebUser user, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.LoadByWebUserIdByIsMaster(user.ID.Value, true, msgs, dc);
        }

        public static string GetInviteUrl(this WebUser user, Account account)
        {
            return Email.BaseUrl(account.IsAfterSchool) + $"signup/new/{user.Uid.ToString()}?a={account.Uid}";
        }

        public static WebUser CreateNew(string firstName, string lastName, string email, string phone, string password, bool isActive, bool isRegistered, bool isVerified, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (firstName.IsEmpty())
                msgs.Add("First Name is required", MessageType.UserError);
            if (lastName.IsEmpty())
                msgs.Add("Last Name is required", MessageType.UserError);

            if (email.IsEmpty())
                msgs.Add("Email is required", MessageType.UserError);
            else if (!email.IsRegExMatch(BasicLibrary.Utilities.EmailRegEx))
                msgs.Add("Email must valid", MessageType.UserError);
            else if (WebUser.ExistsByEmail(email))
                msgs.Add("Email is already in use by another user", MessageType.UserError);

            if (phone.IsEmpty())
                msgs.Add("Phone is required", MessageType.UserError);
            else if (!phone.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                msgs.Add("Phone must valid", MessageType.UserError);
            else if (WebUser.ExistsByPhone(phone))
                msgs.Add("Phone is already in use by another user", MessageType.UserError);

            if (password.IsNotEmpty() && password.Length < 8)
                msgs.Add("Password must be at least 8 characters", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var user = new WebUser()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Phone = phone,
                    IsActive = isActive,
                    IsRegistered = isRegistered,
                    VerifiedAccount = isVerified,
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = loggedInUser?.ID.IfEmpty(0),
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                    Username = WebUserService.GenerateUsername(firstName, lastName)
                };

                if (password.IsNotEmpty())
                    user.Password = WebUserService.EncryptPassword(password);

                msgs.AddRange(user.Save(dc));
                return user;
            }
            return null;
        }

        /// <summary>
        /// For adding a payment account to a student/registrant/participant account
        /// </summary>
        /// <param name="accountOwner"></param>
        /// <param name="phone"></param>
        /// <param name="isActive"></param>
        /// <param name="packageId"></param>
        /// <param name="packageBillingPeriod"></param>
        /// <param name="isAfterSchool"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static Account AddNewRegistrantAccount(this WebUser accountOwner, bool isActive, int? packageId, BillingPeriodType? packageBillingPeriod, bool isAfterSchool, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var account = AccountService.CreateNew(
                orgName: accountOwner.DisplayName,
                email: accountOwner.Email,
                phone: accountOwner.Phone,
                isActive: isActive,
                accountType: AccountType.Registrant,
                accountOwner: accountOwner,
                packageId: null,
                packageBillingPeriod: null,
                loggedInUser: loggedInUser,
                isAfterSchool: isAfterSchool,
                msgs: msgs,
                dc: dc);

            msgs.AddRange(accountOwner.AddAccount(account, UserType.Owner, loggedInUser, dc));

            return account;
        }

        public static Account AddNewRegistrantAccount(this WebUser accountOwner, bool isActive, bool isAfterSchool, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AddNewRegistrantAccount(accountOwner, isActive, null, null, isAfterSchool, loggedInUser, msgs, dc);
        }

        /// <summary>
        /// typically used when sending an invite to an email, 
        /// but in general gives a user a newly created account attached to their webuser account
        /// </summary>
        /// <param name="accountOwner"></param>
        /// <param name="orgName"></param>
        /// <param name="phone"></param>
        /// <param name="isActive"></param>
        /// <param name="accountType"></param>
        /// <param name="packageId"></param>
        /// <param name="packageBillingPeriod"></param>
        /// <param name="isAfterSchool"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static Account AddNewAccount(this WebUser accountOwner, string orgName, string phone, bool isActive, AccountType accountType, int? packageId, BillingPeriodType? packageBillingPeriod, bool isAfterSchool, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var account = AccountService.CreateNew(
                orgName: orgName, 
                email: accountOwner.Email, 
                phone: phone, 
                isActive: isActive, 
                accountType: accountType, 
                accountOwner: accountOwner, 
                packageId: packageId,
                packageBillingPeriod: packageBillingPeriod,
                loggedInUser: loggedInUser, 
                isAfterSchool: isAfterSchool, 
                msgs: msgs, 
                dc: dc);

            msgs.AddRange(accountOwner.AddAccount(account, UserType.Owner, loggedInUser, dc));

            return account;
        }

        /// <summary>
        /// Give a user permissions to an existing account
        /// </summary>
        /// <param name="user"></param>
        /// <param name="account"></param>
        /// <param name="userTypeId"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static SystemMessages AddAccount(this WebUser user, Account account, UserType userTypeId, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            return account.AddUser(user, userTypeId, loggedInUser.ID.Value, dc);
        }

        /// <summary>
        /// User has been invited so the user needs to confirm their information and by adding a password
        /// </summary>
        /// <param name="user"></param>
        /// <param name="account"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static SystemMessages CompleteSignUp(this WebUser user, Account account, string firstName, string lastName, string email, string password, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            user.FirstName = firstName;
            user.LastName = lastName;
            user.Username = WebUserService.GenerateUsername(user.FirstName, user.LastName, msgs, dc);
            user.IsRegistered = true;
            user.IsActive = true;
            user.Password = WebUserService.EncryptPassword(password);

            if (user.Email.IsSameAs(email))
                user.VerifiedAccount = true;
            else
                user.Email = email;

            user.LastModifiedDate = DateTime.UtcNow;
            user.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);

            msgs.AddRange(user.Save(dc));
            msgs.AddRange(user.ActivateForAccount(account, loggedInUser, msgs, dc));

            return msgs;
        }

        /// <summary>
        /// Change the AccountWebUser record to be active. If it's not active user can't login
        /// </summary>
        /// <param name="user"></param>
        /// <param name="account"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        private static SystemMessages ActivateForAccount(this WebUser user, Account account, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var awu = AccountWebUser.LoadByAccountIdByWebUserId(account.ID.Value, user.ID.Value, msgs, dc);

            if (awu.IsNotEmpty())
                msgs.AddRange(awu.Activate(loggedInUser, msgs, dc));
            else
                msgs.Add("User not valid for account", MessageType.UserError);

            return msgs;
        }

        public static Registrant GetParentOrGuardianRegistrant(this WebUser user, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.LoadByWebUserId(user.ID.Value, msgs, dc).Where(registrant => registrant.IsParentOrGuardian).OrderBy(registrant => registrant.ID).FirstOrDefault();
        }
    }
}

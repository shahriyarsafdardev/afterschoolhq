using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Linq;
using SendGrid.Helpers.Mail;

namespace AfterSchoolLibrary
{
    public static class WebUserService
    {
        public static string GenerateRandomCryptographicKey(int keyLength)
        {
            return Convert.ToBase64String(GenerateRandomCryptographicBytes(keyLength));
        }

        public static byte[] GenerateRandomCryptographicBytes(int keyLength)
        {
            var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
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
        public static SystemMessages SendInvite(this WebUser user, string email, string message, WebUser loggedInUser, Account account, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var subject = $"You have been invited by {account.Name} to use {Utilities.AppName(account.IsAfterSchool)}";
            var body = "";

            if (!message.IsEmpty())
                body += "<p>" + message + "</p><p>-------------------------------</p>";

            if (!user.IsActive)
                body += $"<p>Click on the following link to create your account and start using {Utilities.AppName(account.IsAfterSchool)}: <a href=\"{user.GetInviteUrl(account)}\">{user.GetInviteUrl(account)}</a></p>";
            else
                body += $"<p>Click the following login link to access \"{account.Name}\": <a href=\"{AfterSchoolLibrary.Email.BaseUrl(account.IsAfterSchool)}login?group={account.ID.Value.ToString()}\">{AfterSchoolLibrary.Email.HostName(account.IsAfterSchool)}/login</a></p>";

            body += "<p>Thank you!!</p>";
            body += AfterSchoolLibrary.Email.GetAppSignature(isAfterSchool: account.IsAfterSchool, utm_campaign: "invite_team_member");

            msgs.AddRange(SendGridHelper.SendMail(
                fromEmail: new EmailAddress(Email.AppEmailAddress, Utilities.AppName(account.IsAfterSchool)),
                toEmail: new EmailAddress(email),
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

        public static UserType GetUserType(this WebUser user, Account a, DatabaseConnection dc = null)
        {
            if (!a.IsEmpty())
            {
                var awu = AccountWebUser.LoadByAccountIdByWebUserId(a.ID.Value, user.ID.Value, dc: dc);
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
        public static bool HasAccessToFeature(this WebUser user, FeatureType ft, Account g, DatabaseConnection dc = null)
        {
            if (!g.IsEmpty())
            {
                var awu = AccountWebUser.LoadByAccountIdByWebUserId(g.ID.Value, user.ID.Value, dc: dc);
                if (awu.IsEmpty() == false && awu.UserTypeId != UserType.NotSet)
                    return awu.HasAccessToFeature(ft);
            }
            return false;
        }
        public static bool HasAccessTo(this WebUser user, Account a, DatabaseConnection dc = null)
        {
            var awu = AccountWebUser.LoadByAccountIdByWebUserId(a.ID.Value, user.ID.Value, dc: dc);
            return awu.IsNotEmpty() && awu.IsActive;
        }
        public static bool HasAccessToActivity(this WebUser user, Activity a, DatabaseConnection dc = null)
        {
            return ActivityWebUser.LoadByActivityIdByWebUserId(a.ID.Value, user.ID.Value, dc: dc).IsNotEmpty();
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
            var duplicate = WebUser.LoadByUsername(uniqueName, dc: dc);
            int count = 1;
            while (!duplicate.IsEmpty())
            {
                uniqueName = origUniqueName + count.ToString();
                count++;
                duplicate = WebUser.LoadByUsername(uniqueName, dc: dc);
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
            var token = PasswordResetToken.AddOrUpdateToken(user, msgs, dc: dc);

            msgs.AddRange(EmailHelper.SendPasswordReset(user, token, isAfterSchool));

            return msgs;
        }

        public static Registrant CreateRegistrant(this WebUser user, RegistrantType registrantType, bool isMaster, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var registrant = new Registrant
            {
                RegistrantTypeId = registrantType,
                IsMaster = isMaster,
                IsContactOnly = true,
                WebUserId = user.ID,
                CreateDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = user.ID,
                LastModifiedBy = user.ID
            };
            msgs.AddRange(registrant.Save(dc: dc));

            return registrant;
        }

        public static List<Registrant> GetRegistrants(this WebUser user, DatabaseConnection dc = null)
        {
            return Registrant.LoadByWebUserIdByIsMaster(user.ID.Value, true, dc: dc);
        }

        public static string GetInviteUrl(this WebUser user, Account account)
        {
            return AfterSchoolLibrary.Email.BaseUrl(account.IsAfterSchool) + $"signup/new/{user.Uid.ToString()}?a={account.Uid}";
        }

        public static WebUser SignUpUser(string userUid, string accountUid, string firstName, string lastName, string email, string password, string verifyPassword, bool isAfterSchool, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            WebUser user = null;
            Account account = null;

            if (!userUid.IsGuid())
                msgs.Add("Invalid invitation code", MessageType.UserError);
            else
            {
                user = WebUser.LoadByUid(userUid.ToGuid(), null);

                if (user.IsEmpty())
                    msgs.Add("Invalid invitation code", MessageType.UserError);
                else if (user.IsActive)
                    msgs.Add("Account has already been created. Please proceed to the login page", MessageType.UserError);
            }

            if (!accountUid.IsGuid())
            {
                msgs.Add("Invalid invitation code", MessageType.UserError);
            }
            else
            {
                account = Account.LoadByUid(accountUid.ToGuid());
                if (account.IsEmpty())
                    msgs.Add("Invalid invitation code", MessageType.UserError);
            }

            if (firstName.IsEmpty())
                msgs.Add("First Name is required", MessageType.UserError);

            if (lastName.IsEmpty())
                msgs.Add("Last Name is required", MessageType.UserError);

            if (email.IsEmpty())
                msgs.Add("Email is required", MessageType.UserError);
            else if (!email.IsRegExMatch(BasicLibrary.Utilities.EmailRegEx))
                msgs.Add("Please enter a valid email address", MessageType.UserError);
            else if (!user.CanEmailBeUsed(email))
                msgs.Add("This email has already been registered", MessageType.UserError);

            if (password.IsEmpty() || verifyPassword.IsEmpty() || (password != verifyPassword))
                msgs.Add("Passwords are required and must match", MessageType.UserError);
            else if (password.Length < 6)
                msgs.Add("Password must be at least 6 characters", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, BasicLibrary.DataAccess.DatabaseConnection adc) =>
                    {
                        if (user.Email.ToLower().Trim() != email.ToLower().Trim())
                            user.Email = email;
                        else
                            user.VerifiedAccount = true;

                        user.FirstName = firstName;
                        user.LastName = lastName;
                        user.Username = WebUserService.GenerateUsername(user.FirstName, user.LastName, dc: adc);
                        user.IsRegistered = true;
                        user.IsActive = true;
                        user.Password = WebUserService.EncryptPassword(password);
                        amsgs.AddRange(user.Save(adc));

                        var awu = AccountWebUser.LoadByAccountIdByWebUserId(account.ID.Value, user.ID.Value, dc: adc);
                        if (awu.IsNotEmpty())
                        {
                            awu.IsActive = true;
                            amsgs.AddRange(awu.Save(adc));
                        }
                        else
                            msgs.Add("Invalid invitation code", MessageType.UserError);
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        if (!user.VerifiedAccount)
                        {
                            msgs.AddRange(user.SendVerificationEmail(isAfterSchool));
                        }
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
            }

            return user;
        }
    }
}

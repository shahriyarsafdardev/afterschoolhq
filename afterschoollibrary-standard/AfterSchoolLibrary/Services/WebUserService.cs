using System;
using System.Collections.Generic;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Linq;
using SendGrid.Helpers.Mail;
using AfterSchoolLibrary.Models;
using System.Collections;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Enums;

namespace AfterSchoolLibrary.Services
{
    public static class WebUserService
    {
        public static bool IsValidPassword(string plainPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
        }

        public static string EncryptPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
        }

        public static WebUser LoginByUsername(string username, string password, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return WebUser.LoadByUsername(username, true, msgs, dc).ReturnUserIfValidPassword(password);
        }

        public static WebUser LoginByPhone(string phone, string password, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return WebUser.LoadByPhone(phone, true, msgs, dc).ReturnUserIfValidPassword(password);
        }

        public static WebUser LoginByEmail(string email, string password, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return WebUser.LoadByEmail(email, true, msgs, dc).ReturnUserIfValidPassword(password);
        }

        public static WebUser ReturnUserIfValidPassword(this WebUser user, string plainPassword)
        {
            if (user.IsNotEmpty() && IsValidPassword(plainPassword, user.Password))
                return user;

            return null;
        }

        private static string BuildAccountInviteSubject(Account account)
        {
            return $"You have been invited by {account.Name} to use {Helpers.Utilities.AppName(account.IsAfterSchool)}";
        }

        private static string BuildAccountInviteBody(WebUser user, Account account, string message)
        {
            var body = "";

            if (!message.IsEmpty())
                body += "<p>" + message + "</p><p>-------------------------------</p>";

            if (!user.IsActive)
                body += $"<p>Click on the following link to create your account and start using {Helpers.Utilities.AppName(account.IsAfterSchool)}: <a href=\"{user.GetInviteUrl(account)}\">{user.GetInviteUrl(account)}</a></p>";
            else
                body += $"<p>Click the following login link to access \"{account.Name}\": <a href=\"{Helpers.Utilities.BaseUrl()}login?group={account.ID.Value.ToString()}\">{Helpers.Utilities.HostName}/login</a></p>";

            body += "<p>Thank you!!</p>";
            body += Helpers.Email.GetAppSignature(isAfterSchool: account.IsAfterSchool, utm_campaign: "invite_team_member");

            return body;
        }

        public static SystemMessages SendInviteToAccount(this WebUser user, Account account, string message, WebUser loggedInUser, DatabaseConnection dc = null)
        {
            return SendGridHelper.SendMail(
                fromEmail: new EmailAddress(Helpers.Email.AppNoReplyEmailAddress, Helpers.Utilities.AppName(account.IsAfterSchool)),
                toEmail: new EmailAddress(user.Email),
                subject: BuildAccountInviteSubject(account),
                body: BuildAccountInviteBody(user, account, message),
                replyTo: new EmailAddress(loggedInUser.Email, loggedInUser.DisplayName),
                emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                );
        }

        public static List<Account> GetAccounts(this WebUser user, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountWebUser.LoadByWebUserId(user.ID.Value, true, false, false, msgs, dc)
                .Select(x => x.ToAccount(true, msgs, dc))
                .ToList();
        }

        public static Account GetRegistrantAccount(this WebUser user, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return user.GetAccounts(msgs, dc).FirstOrDefault(account => account.AccountTypeId == AccountType.Registrant);
        }

        public static Account GetVolunteerAccount(this WebUser user, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return user.GetAccounts(msgs, dc).FirstOrDefault(account => account.AccountTypeId == AccountType.Volunteer);
        }

        public static bool HasRegistrantAccount(this WebUser user, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return user.GetRegistrantAccount(msgs, dc).IsNotEmpty();
        }

        public static bool HasMultipleAccounts(this WebUser user, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AccountWebUser.GetCount(
                    new Hashtable() 
                    { 
                        { "WebUserId", user.ID.Value }, 
                        { "IsActive", true }, 
                        { "IsDeleted", false }, 
                        { "IsGhostAccount", false } 
                    }, msgs, dc) > 1;                
        }

        public static UserType GetUserType(this WebUser user, Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!account.IsEmpty())
            {
                var awu = AccountWebUser.LoadByAccountIdByWebUserId(account.ID.Value, user.ID.Value, msgs, dc);
                if (!awu.IsEmpty())
                    return awu.UserTypeId;
            }
            return UserType.NotSet;
        }
        public static void UpdateLastActivity(this WebUser user, SystemMessages msgs, DatabaseConnection dc = null)
        {
            user.LastActivity = DateTime.UtcNow;
            msgs.AddRange(user.Save(dc));
        }
        public static void UpdateLastLogin(this WebUser user, SystemMessages msgs, DatabaseConnection dc = null)
        {
            user.LastLogin = DateTime.UtcNow;
            msgs.AddRange(user.Save(dc));
        }
        public static bool HasPermission(this WebUser user, PermissionType permissionType, Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (!account.IsEmpty())
            {
                var awu = AccountWebUser.LoadByAccountIdByWebUserId(account.ID.Value, user.ID.Value, msgs, dc);
                if (awu.IsEmpty() == false && awu.UserTypeId != UserType.NotSet)
                    return awu.HasPermission(permissionType);
            }
            return false;
        }
        public static bool HasAccessTo(this WebUser user, Account a, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (user.IsNotEmpty() && a.IsNotEmpty())
            {
                var awu = AccountWebUser.LoadByAccountIdByWebUserId(a.ID.Value, user.ID.Value, msgs, dc);

                if (awu.IsNotEmpty())
                    return awu.IsNotEmpty() && awu.IsActive;
            }

            return false;
        }
        public static bool HasAccessToActivity(this WebUser user, Activity a, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityWebUser.LoadByActivityIdByWebUserId(a.ID.Value, user.ID.Value, msgs, dc).IsNotEmpty();
        }
        public static bool IsActivityLeader(this WebUser user, Activity a, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var awu = user.GetActivityWebUser(a, msgs, dc);
            return awu.IsNotEmpty() && awu.IsLeader;
        }

        public static bool CanPublishActivity(this WebUser user, Account currentAccount, Activity activity, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var location = activity.GetLocation(msgs, dc);
            var hostAccount = location?.GetHostAccount(msgs, dc);

            // if host account doesn't exist, is unclaimed, or is actually the same as the current account
            if (hostAccount.IsEmpty() || !hostAccount.HasAccountOwner || hostAccount.ID == currentAccount.ID)
                return true;

            if (hostAccount.IsNotEmpty())
                return !currentAccount.RequiresPublishPermissionFor(hostAccount, msgs, dc);

            return false;
        }

        public static ActivityWebUser GetActivityWebUser(this WebUser user, Activity a, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return a.ID.HasValue ? ActivityWebUser.LoadByActivityIdByWebUserId(a.ID.Value, user.ID.Value, msgs, dc) : null;
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
            string origUniqueName = reg.Replace(username.MaxChars(25), "");
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

        public static void ConfirmAccount(this WebUser user, string password, SystemMessages msgs, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    user.IsRegistered = true;
                    user.IsActive = true;
                    user.Password = WebUserService.EncryptPassword(password);
                    msgs.AddRange(user.Save(dc));

                    var awus = AccountWebUser.LoadByWebUserIdByIsDeleted(user.ID.Value, false, msgs, dc);
                    awus.ForEach(awu => {
                        awu.IsActive = true;
                        msgs.AddRange(awu.Save(dc));
                    });
                },
                onSuccess: (SystemMessages smsgs) => 
                {
                    msgs.AddRange(smsgs);
                    user.SendVerificationEmail();
                },
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }

        public static SystemMessages SendVerificationEmail(int userId)
        {
            var msgs = new SystemMessages();
            var user = WebUser.Load(userId, msgs);

            return user.SendVerificationEmail();
        }

        public static SystemMessages SendVerificationEmail(this WebUser user, string redirectUrl = null, bool isAfterSchool = true)
        {
            return Helpers.EmailHelper.SendEmailVerification(user, redirectUrl, isAfterSchool);
        }

        public static SystemMessages SendWelcomePurchase(string userFirstName, string userEmail, bool isAfterSchool = true)
        {
            return Helpers.EmailHelper.SendWelcomePurchase(userFirstName, userEmail, isAfterSchool);
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
                msgs.AddRange(Helpers.EmailHelper.SendPasswordReset(user, token, isAfterSchool));

            return msgs;
        }
        
        public static Registrant GetMasterVolunteerRegistrant(this WebUser user, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.LoadByWebUserId(user.ID.Value, RegistrantPersonType.Volunteer, true, false, msgs, dc).FirstOrDefault();
        }
        
        public static Registrant GetMasterRegistrant(this WebUser user, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Registrant.LoadByWebUserIdByIsMasterByIsDeleted(user.ID.Value, true, false, msgs, dc).FirstOrDefault();
        }

        public static List<Registrant> GetAllRegistrants(this WebUser user, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantWebUser.LoadByWebUserIdByIsDeleted(user.ID.Value, false, msgs, dc)
                .Select(rwu => Registrant.Load(rwu.RegistrantId.Value, msgs, dc))
                .ToList();
        }

        public static List<Registrant> GetChildrenRegistrants(this WebUser user, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantWebUser.LoadByWebUserIdByIsDeleted(user.ID.Value, false, msgs, dc)
                .Select(rwu => Registrant.Load(rwu.RegistrantId.Value, msgs, dc))
                .Where(registrant => !registrant.IsParentOrGuardian)
                .ToList();
        }

        public static string GetInviteUrl(this WebUser user, Account account)
        {
            return $"{Helpers.Email.BaseUrl()}signup/new/{user.Uid.ToString()}?a={account.Uid}";
        }

        public static WebUser InviteNew(string firstName, string lastName, string email, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            ValidateNameForNewUser(firstName, lastName, msgs);

            if (email.IsEmpty())
                msgs.Add("Email is required", MessageType.UserError);
            else if (!email.IsRegExMatch(BasicLibrary.Utilities.EmailRegEx))
                msgs.Add("Email must valid", MessageType.UserError);
            else if (WebUser.ExistsByEmail(email, msgs, dc))
                msgs.Add("Email is already in use by another user", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var user = new WebUser()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = loggedInUser?.ID.IfEmpty(0),
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                    Username = WebUserService.GenerateUsername(firstName, lastName,msgs,dc)
                };                

                msgs.AddRange(user.Save(dc));

                return user;
            }

            return null;
        }

        public static WebUser StartUserAccount(string firstName, string lastName, string email, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            ValidateNameForNewUser(firstName, lastName, msgs);
            
            ValidateEmailForNewUser(email, msgs, dc);

            if (msgs.HasNoErrors())
            {
                var user = new WebUser()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Phone = null,
                    IsActive = false,
                    IsRegistered = false,
                    VerifiedAccount = false,
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = 0, 
                    Username = WebUserService.GenerateUsername(firstName, lastName, msgs, dc)
                };

                msgs.AddRange(user.Save(dc));

                return user;
            }

            return null;
        }

        private static void ValidateNameForNewUser(string firstName, string lastName, SystemMessages msgs)
        {
            if (firstName.IsEmpty())
                msgs.Add("First Name is required", MessageType.UserError);
            if (lastName.IsEmpty())
                msgs.Add("Last Name is required", MessageType.UserError);
        }
        
        private static void ValidateEmailForNewUser(string email, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (email.IsEmpty())
                msgs.Add("Email is required", MessageType.UserError);
            else if (!email.IsRegExMatch(BasicLibrary.Utilities.EmailRegEx))
                msgs.Add("Email must valid", MessageType.UserError);
            else if (WebUser.ExistsByEmail(email, msgs, dc))
                msgs.Add("Email is already in use, do you already have an account?", MessageType.UserError);
        }

        private static void ValidatePasswordForNewUser(string password, SystemMessages msgs)
        {
            if (password.IsNotEmpty() && password.Length < 8)
                msgs.Add("Password must be at least 8 characters", MessageType.UserError);
        }

        private static void ValidateRequiredPhoneForNewUser(string phone, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (phone.IsEmpty())
                msgs.Add("Phone is required", MessageType.UserError);
            else
                ValidateOptionalPhoneForNewUser(phone, msgs, dc);
        }

        private static void ValidateOptionalPhoneForNewUser(string phone, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (!phone.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                msgs.Add("Phone must be valid", MessageType.UserError);
            else if (WebUser.ExistsByPhone(phone, msgs, dc))
                msgs.Add("Phone is already in use, do you already have an account?", MessageType.UserError);
        }
        
        public static WebUser CreateNewWithNoPhone(string firstName, string lastName, string email, string password, bool isActive, bool isRegistered, bool isVerified, string howHearAboutUs, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            ValidateNameForNewUser(firstName, lastName, msgs);
            
            ValidateEmailForNewUser(email, msgs, dc);

            ValidatePasswordForNewUser(password, msgs);

            if (msgs.HasNoErrors())
            {
                var user = new WebUser()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Phone = null,
                    IsActive = isActive,
                    IsRegistered = isRegistered,
                    VerifiedAccount = isVerified,
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = loggedInUser?.ID.IfEmpty(0),
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                    Username = WebUserService.GenerateUsername(firstName, lastName),
                    HowHearAboutUs = howHearAboutUs
                };

                if (password.IsNotEmpty())
                    user.Password = WebUserService.EncryptPassword(password);

                msgs.AddRange(user.Save(dc));

                return user;
            }

            return null;
        }

        public static WebUser CreateNewWithOptionalPhone(string firstName, string lastName, string email, string phone, string password, bool isActive, bool isRegistered, bool isVerified, string howHearAboutUs, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            ValidateNameForNewUser(firstName, lastName, msgs);

            ValidateEmailForNewUser(email, msgs, dc);

            if (phone.IsNotEmpty())
                ValidateOptionalPhoneForNewUser(phone, msgs, dc);

            ValidatePasswordForNewUser(password, msgs);

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
                    HowHearAboutUs = howHearAboutUs,
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

        public static WebUser CreateNewWithOptionalPhone(string firstName, string lastName, string email, string phone, string password, bool isActive, bool isRegistered, bool isVerified, bool allowTextMessages, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            ValidateNameForNewUser(firstName, lastName, msgs);

            ValidateEmailForNewUser(email, msgs, dc);

            if (phone.IsNotEmpty())
                ValidateOptionalPhoneForNewUser(phone, msgs, dc);

            ValidatePasswordForNewUser(password, msgs);

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
                    AllowTextMessages = allowTextMessages,
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

        public static WebUser CreateNew(string firstName, string lastName, string email, string phone, string password, bool isActive, bool isRegistered, bool isVerified, bool allowTextMessages, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            ValidateNameForNewUser(firstName, lastName, msgs);
            
            ValidateEmailForNewUser(email, msgs, dc);

            ValidateRequiredPhoneForNewUser(phone, msgs, dc);

            ValidatePasswordForNewUser(password, msgs);

            if (allowTextMessages && phone.IsEmpty())
                msgs.Add("Mobile Phone is required to accept text messages", MessageType.UserError);

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
                    AllowTextMessages = allowTextMessages,
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
        public static Account AddNewRegistrantAccount(this WebUser user, UserType userType, bool isActive, int? packageId, BillingPeriodType? packageBillingPeriod, bool isAfterSchool, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Account account = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    account = AccountService.CreateNew(
                                    orgName: $"{user.LastName} Family",
                                    email: user.Email,
                                    phone: user.Phone,
                                    isActive: isActive,
                                    accountType: AccountType.Registrant,
                                    accountOwner: userType == UserType.Owner ? user : null,
                                    packageId: null,
                                    packageBillingPeriod: null,
                                    timeZone: null,
                                    createWithStripe: true,
                                    stripeAccountType: StripeAccountType.ConnectAccount,
                                    loggedInUser: loggedInUser,
                                    isAfterSchool: isAfterSchool,
                                    msgs: amsgs,
                                    dc: adc);

                    user.AddAccount(account, userType, loggedInUser, amsgs, adc);

                    account.CreateRegistrant(
                        firstName: user.FirstName,
                        lastName: user.LastName,
                        email: user.Email,
                        registrantOwner: user,
                        canBeAccessedBy: user.AsList(),
                        registrantType: RegistrantType.Individual,
                        isMaster: true,
                        isParentOrGuardian: true,
                        allowTextMessages: false,
                        schoolLocationId: null,
                        profileStatus: RegistrantProfileStatus.Started,
                        loggedInUser: user,
                        msgs: amsgs,
                        dc: adc);
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );            

            return account;
        }

        /// <summary>
        /// For adding a payment account to a volunteer account
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
        public static Account AddNewVolunteerAccount(this WebUser user, UserType userType, bool isActive, bool isAfterSchool, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Account account = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    account = AccountService.CreateNew(
                                    orgName: $"{user.FirstName} Volunteer Account",
                                    email: user.Email,
                                    phone: user.Phone,
                                    isActive: isActive,
                                    accountType: AccountType.Volunteer,
                                    accountOwner: userType == UserType.Owner ? user : null,
                                    packageId: null,
                                    packageBillingPeriod: null,
                                    timeZone: null,
                                    createWithStripe: true,
                                    loggedInUser: loggedInUser,
                                    isAfterSchool: isAfterSchool,
                                    msgs: amsgs,
                                    dc: adc);

                    user.AddAccount(account, userType, loggedInUser, amsgs, adc);

                    account.CreateVolunteerRegistrant(
                        firstName: user.FirstName,
                        lastName: user.LastName,
                        email: user.Email,
                        registrantOwner: user,
                        canBeAccessedBy: user.AsList(),
                        isMaster: true,
                        allowTextMessages: false,
                        profileStatus: RegistrantProfileStatus.Started,
                        loggedInUser: user,
                        msgs: amsgs,
                        dc: adc);
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );            

            return account;
        }

        public static Account AddNewStudentAccount(this WebUser user, UserType userType, bool isActive, bool isAfterSchool, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Account account = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    account = AccountService.CreateNew(
                                    orgName: $"{user.LastName} Family",
                                    email: user.Email,
                                    phone: user.Phone,
                                    isActive: isActive,
                                    accountType: AccountType.Registrant,
                                    accountOwner: userType == UserType.Owner ? user : null,
                                    packageId: null,
                                    packageBillingPeriod: null,
                                    timeZone: null,
                                    createWithStripe: true,
                                    loggedInUser: loggedInUser,
                                    isAfterSchool: isAfterSchool,
                                    msgs: amsgs,
                                    dc: adc);

                    user.AddAccount(account, userType, loggedInUser, amsgs, adc);
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return account;
        }

        /// <summary>
        /// For adding a payment account to a student/registrant/participant account
        /// </summary>
        /// <param name="accountOwner"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="email"></param>
        /// <param name="isActive"></param>
        /// <param name="packageId"></param>
        /// <param name="packageBillingPeriod"></param>
        /// <param name="isAfterSchool"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static Account AddNewRegistrantAccountWithUserName(this WebUser user, UserType userType, string firstName, string lastName, string email,bool isActive, bool isAfterSchool, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Account account = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    account = AccountService.CreateNew(
                                    orgName: $"{lastName} Family",
                                    email: email,
                                    phone: user.Phone,
                                    isActive: isActive,
                                    accountType: AccountType.Registrant,
                                    accountOwner: userType == UserType.Owner ? user : null,
                                    packageId: null,
                                    packageBillingPeriod: null,
                                    timeZone: null,
                                    createWithStripe: true,
                                    loggedInUser: loggedInUser,
                                    isAfterSchool: isAfterSchool,
                                    msgs: amsgs,
                                    dc: adc);

                    user.AddAccount(account, userType, loggedInUser, amsgs, adc);

                    account.CreateRegistrant(
                        firstName: firstName,
                        lastName: lastName,
                        email: email,
                        registrantOwner: user,
                        canBeAccessedBy: user.AsList(),
                        registrantType: RegistrantType.Individual,
                        isMaster: true,
                        isParentOrGuardian: true,
                        allowTextMessages: false,
                        schoolLocationId: null,
                        profileStatus: RegistrantProfileStatus.Started,
                        loggedInUser: user,
                        msgs: amsgs,
                        dc: adc);
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

            return account;
        }

        public static Account AddNewRegistrantAccount(this WebUser accountOwner, UserType userType, bool isActive, bool isAfterSchool, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return AddNewRegistrantAccount(accountOwner, userType, isActive, null, null, isAfterSchool, loggedInUser, msgs, dc);
        }

        public static Account AddNewAccountWithDefaultPackage(this WebUser accountOwner, string orgName, AccountType accountType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var account = AddNewAccount(accountOwner, orgName, null, true, accountType, null, null, null, true, loggedInUser, msgs, dc);

            if (account.IsNotEmpty())
                account.SetDefaultAccountPackage(loggedInUser, msgs, dc);

            return account;
        }

        public static Account AddNewAccountWithDefaultPackageWithStripe(this WebUser accountOwner, string orgName, AccountType accountType, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var account = AddNewAccount(accountOwner, orgName, null, true, accountType, null, null, null, true, loggedInUser, msgs, dc);

            if (account.IsNotEmpty())
            {
                account.CreateStripeCustomer(StripeAccountType.SubscriberAccount, accountOwner, msgs, dc);
                account.SetDefaultAccountPackage(loggedInUser, msgs, dc);
            }

            return account;
        }

        public static Account AddNewAccount(this WebUser accountOwner, string orgName, string phone, bool isActive, AccountType accountType, int? packageId, BillingPeriodType? packageBillingPeriod, string timeZone, bool isAfterSchool, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
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
                timeZone: timeZone,
                createWithStripe: false,
                loggedInUser: loggedInUser, 
                isAfterSchool: isAfterSchool, 
                msgs: msgs, 
                dc: dc);

            accountOwner.AddAccount(account, UserType.Owner, loggedInUser, msgs, dc);

            return account;
        }

        public static Account AddNewAccount(this WebUser accountOwner, string orgName, string claimemaildomain, string phone, bool isActive, AccountType accountType, int? packageId, BillingPeriodType? packageBillingPeriod, string timeZone, bool isAfterSchool, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var account = AccountService.CreateNew(
                orgName: orgName,
                email: accountOwner.Email,
                claimemaildomain: claimemaildomain,
                phone: phone,
                isActive: isActive,
                accountType: accountType,
                accountOwner: accountOwner,
                packageId: packageId,
                packageBillingPeriod: packageBillingPeriod,
                timeZone: timeZone,
                createWithStripe: false,
                loggedInUser: loggedInUser,
                isAfterSchool: isAfterSchool,
                msgs: msgs,
                dc: dc);

            accountOwner.AddAccount(account, UserType.Owner, loggedInUser, msgs, dc);

            return account;
        }

        public static Account AddNewEmptyAccount(this WebUser accountOwner, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {

            var account = AccountService.CreateNewEmptyAccount(
                accountOwner: accountOwner,
                loggedInUser: loggedInUser,
                msgs: msgs,
                dc: dc);

            AccountService.SetDefaultAccountPackage(account, loggedInUser, msgs, dc);

            accountOwner.AddAccount(account, UserType.Owner, loggedInUser, msgs, dc);

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
        public static AccountWebUser AddAccount(this WebUser user, Account account, UserType userTypeId, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            return account.AddUser(user, userTypeId, loggedInUser, msgs, dc);
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
        /// <param name="phone"></param>
        /// <param name="loggedInUser"></param>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static void CompleteSignUp(this WebUser user, Account account, string firstName, string lastName,
            string email, string password, string phone, WebUser loggedInUser, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    user.FirstName = firstName;
                    user.LastName = lastName;
                    user.Username = WebUserService.GenerateUsername(user.FirstName, user.LastName, amsgs, adc);
                    user.IsRegistered = true;
                    user.IsActive = true;
                    user.Password = WebUserService.EncryptPassword(password);
                    user.Phone = phone;
                    user.LastModifiedDate = DateTime.UtcNow;
                    user.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);

                    if (user.Email.IsSameAs(email))
                        user.VerifiedAccount = true;
                    else
                        user.Email = email;

                    amsgs.AddRange(user.Save(adc));

                    user.ActivateForAccount(account, loggedInUser, amsgs, adc);

                    // Registrant accounts don't need a package
                    if (account.AccountTypeId != AccountType.Registrant)
                    {
                        if (!account.HasAccountPackage(amsgs, adc))
                            account.SetDefaultAccountPackage(loggedInUser, amsgs, adc);

                        if (account.StripeCustomerId.IsEmpty())
                            amsgs.AddRange(account.SaveWithStripe(StripeAccountType.SubscriberAccount, dc));
                    } 
                    else {
                        var awu = AccountWebUser.LoadByAccountIdByWebUserId(AccountId: account.ID.Value, WebUserId: user.ID.Value, msgs: amsgs, dc: adc);
                        if (awu.IsNotEmpty() && user.IsActive && awu.IsActive && awu.UserTypeId == UserType.Owner && account.OwnerId != user.ID)
                        {
                            // this is parent completing it's signup
                            // so we will update owner ship of account from student to parent
                            account.OwnerId = user.ID.Value;
                            amsgs.AddRange(account.Save(adc));
                        }
                    }
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );

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
        private static void ActivateForAccount(this WebUser user, Account account, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var awu = AccountWebUser.LoadByAccountIdByWebUserId(account.ID.Value, user.ID.Value, msgs, dc);

            if (awu.IsNotEmpty())
                msgs.AddRange(awu.Activate(loggedInUser, msgs, dc));
            else
                msgs.Add("User not valid for account", MessageType.UserError);
            
        }

        public static bool HasAccessToRegistrant(this WebUser user, Registrant registrant, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return RegistrantWebUser.GetCount(
                new System.Collections.Hashtable()
                {
                    { "WebUserId", user.ID.Value },
                    { "RegistrantId", registrant.ID.Value }
                }, 
                msgs, 
                dc) > 0;
        }

        public static bool IsSubscribedTo(this WebUser user, Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Subscriber.GetCountByObjectByWebUserIdByStatus(
                    account.ID.Value, 
                    AHQObjectType.Account, 
                    user.ID.Value, 
                    SubscriberStatus.Subscribed, 
                    msgs, 
                    dc) > 0;
        }

        public static bool IsTechSupport(this WebUser loggedInUser, Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return loggedInUser.GetUserType(account, msgs, dc) == UserType.TechSupport;
        }

        public static bool IsTechSupportOrOwner(this WebUser loggedInUser, Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var userType = loggedInUser.GetUserType(account, msgs, dc);
            return userType.In(new List<UserType>() { UserType.TechSupport, UserType.Owner });
        }

        public static bool IsTechSupportOwnerOrAdmin(this WebUser loggedInUser, Account account, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var userType = loggedInUser.GetUserType(account, msgs, dc);
            return userType.In(new List<UserType>() { UserType.TechSupport, UserType.Owner, UserType.Administrator });
        }

        #region Clever Integration

        public static WebUser LoginByCleverUserId(string CleverUserId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return WebUser.LoadByCleverUserIdByIsActive(CleverUserId: CleverUserId, IsActive: true, msgs: msgs, dc: dc);
        }

        public static WebUser GetUserByCleverId(string id, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (id.IsNotEmpty())
                return WebUserService.LoginByCleverUserId(CleverUserId: id, msgs: msgs, dc: dc);

            return null;
        }

        public static void DisconnectCleverIdentity(WebUser webUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (webUser.IsNotEmpty())
            {
                webUser.CleverUserId = null;
                webUser.CleverUserName = null;
                msgs.AddRange(webUser.Save(dc: dc));

                if (msgs.HasNoErrors())
                    msgs.Add($"Clever disconnected for {webUser.DisplayName}.", MessageType.Success);
            }
        }

        public static void SetCleverIdentityToUser(string id, string name, WebUser webUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (webUser.IsNotEmpty() && id.IsNotEmpty())
            {
                webUser.CleverUserId = id;
                webUser.CleverUserName = name;
                msgs.AddRange(webUser.Save(dc: dc));

                if (msgs.HasNoErrors())
                    msgs.Add($"Clever integration for {webUser.DisplayName} has been successfully completed.", MessageType.Success);             
            }
        }

        #endregion
        /// <summary>
        /// Basically this should get first user which will be admin user and it will be there in all environments
        /// If that user is not present then bring the first active user
        /// this method should be used for logs
        /// </summary>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static WebUser GetAdminUserForLogs(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            // load admin user
            var webUser = WebUser.Load(id: 1, msgs: msgs, dc: dc);

            // We will have this admin user in all environment but in case
            // if don't find it, just load first Active webuser to be sure that system will not fail
            if (webUser.IsEmpty())
                webUser = WebUser.Load(criterias: new Hashtable() { { "IsActive", 1 } }, msgs: msgs, dc: dc).FirstOrDefault();

            return webUser;
        }
    }
}

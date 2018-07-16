using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Collections;
using BasicLibrary;
using SendGrid.Helpers.Mail;
using SendGrid;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibrary
{
    public class Utilities
    {
        public static string ConnectionString
        {
            get
            {
                return Database.Credentials.ConnectionString;
            }
        }

        public static string PayPalPaymentNotificationUrl(bool isAfterSchool)
        {
            return BaseUrl(isAfterSchool) + "/paypal/notification/";
        }

        public static bool IsProduction
        {
            get
            {
                if (System.Reflection.Assembly.GetExecutingAssembly().CodeBase.ToLower().Contains("-production"))
                    return true;

                return false;
            }
        }
        public static bool IsStaging
        {
            get
            {
                if (System.Reflection.Assembly.GetExecutingAssembly().CodeBase.ToLower().Contains("-staging"))
                    return true;

                return false;
            }
        }
        public static bool IsTest
        {
            get
            {
                if (System.Reflection.Assembly.GetExecutingAssembly().CodeBase.ToLower().Contains("-test"))
                    return true;

                return false;
            }
        }
        public static bool IsCI
        {
            get
            {
                if (System.Reflection.Assembly.GetExecutingAssembly().CodeBase.ToLower().Contains("-ci"))
                    return true;

                return false;
            }
        }
        public static bool IsQA
        {
            get
            {
                if (System.Reflection.Assembly.GetExecutingAssembly().CodeBase.ToLower().Contains("-qa"))
                    return true;

                return false;
            }
        }
        public static bool IsLocalHost
        {
            get
            {
                if (!IsProduction && !IsStaging && !IsCI && !IsQA)
                    return true;

                return false;
            }
        }

        public static string RollbarAccessToken { get { return "9d10f02799c544f5bf4c9e27e0743c54"; } }

        public static string GetRollbarEnvironment()
        {
            if (IsProduction)
            {
                return "production";
            }
            else if (IsStaging)
            {
                return "staging";
            }
            else if (IsCI)
            {
                return "ci";
            }
            else if (IsQA)
            {
                return "qa";
            }
            else
            {
                return "local";
            }
        }

        public static string AppApiKey { get { return MasterAPIKey; } }
        public static string AppApiSecret { get { return MasterAPISecret; } }
        public static string MasterAPIKey { get; set; }
        public static string MasterAPISecret { get; set; }

        public static string AmazonAccessKeyId { get; set; }
        public static string AmazonSecretAccessKeyId { get; set; }

        public static string ReCaptchaPrivateKey { get; set; }
        public static string ReCaptchaPublicKey { get; set; }

        private static string AppDocumentRoot;

        public static byte[] AppAESKey { get; set; }

        private static byte[] _AppIV;
        public static byte[] AppIV
        {
            get
            {
                if (_AppIV.IsEmpty())
                {
                    string filepath = AppDocumentRoot + "\\app\\";
                    if (!System.IO.Directory.Exists(filepath))
                    {
                        System.IO.Directory.CreateDirectory(filepath);
                    }
                    filepath += "appiv";
                    if (!System.IO.File.Exists(filepath))
                    {
                        System.IO.File.WriteAllBytes(filepath, Encryption.GetNewIV());
                    }
                    _AppIV = System.IO.File.ReadAllBytes(filepath);
                }
                return _AppIV;
            }
        }


        public static string EncryptToBase64(string value)
        {
            return Convert.ToBase64String(Encryption.Encrypt(System.Text.UTF8Encoding.UTF8.GetBytes(value), Utilities.AppAESKey, Utilities.AppIV));
        }
        public static string DecryptFromBase64(string value)
        {
            return Encryption.DecryptToString(Convert.FromBase64String(value), Utilities.AppAESKey, Utilities.AppIV);
        }

        public static string MachineName
        {
            get
            {
                return System.Environment.MachineName;
            }
        }

        public static string HostNameProductionAfterSchoolHQ = "";
        public static string HostNameStagingAfterSchoolHQ = "";
        public static string HostNameCIAfterSchoolHQ = "";
        public static string HostNameQAAfterSchoolHQ = "";
        public static string HostNameLocalAfterSchoolHQ = "";

        public static string HostName(bool isAfterSchool)
        {
            if (Utilities.IsProduction)
            {
                return HostNameProductionAfterSchoolHQ;
            }
            else if (Utilities.IsStaging)
            {
                return HostNameStagingAfterSchoolHQ;
            }
            else if (Utilities.IsCI)
            {
                return HostNameCIAfterSchoolHQ;
            }
            else if (Utilities.IsQA)
            {
                return HostNameQAAfterSchoolHQ;
            }
            else
            {
                return HostNameLocalAfterSchoolHQ;
            }
        }

        public static string BaseUrl(bool isAfterSchool)
        {
            return "http" + (Utilities.UseSSL() ? "s" : "") + "://" + HostName(isAfterSchool);
        }

        public static bool UseSSL()
        {
            if (IsLocalHost)
                return false;

            return true;
        }

        public static string BaseUrlHttp(bool isAfterSchool)
        {
            return "http://" + HostName(isAfterSchool);
        }
        public static string BaseUrlHttps(bool isAfterSchool)
        {
            return "https://" + HostName(isAfterSchool);
        }
        
        public static string AppNameEnrollHQ { get; set; }
        public static string AppNameAfterSchoolHQ { get; set;  }
        public static string AppName(bool isAfterSchool)
        {
            if (isAfterSchool)
                return AppNameAfterSchoolHQ;
            return AppNameEnrollHQ;
        }

        public static void InitializeApp()
        {
            // Utilities.UseSSL = true; //can put in a if statement depending on if is live, etc.
            // AfterSchoolLibrary.Utilities.AppName = "Enroll HQ"; //used in email signatures, etc
            Utilities.AppAESKey = System.Text.Encoding.UTF8.GetBytes("9dj3nshcnshf73heydsg1nshdyciefh7"); //32 alphanumeric chars - only set once per app, never changed afterwards, required

            Utilities.MasterAPIKey = ""; //at least 45 random alphanumeric characters
            Utilities.MasterAPISecret = ""; //at least 65 random alphanumeric characters

            Utilities.AmazonBucketNameCI = "afterschoolhq-ci"; //all files are stored in amazon bucket
            Utilities.AmazonBucketNameQA = "afterschoolhq-qa"; //all files are stored in amazon bucket
            Utilities.AmazonBucketNameStaging = "afterschoolhq-staging"; //all files are stored in amazon bucket
            Utilities.AmazonBucketNameTest = "afterschoolhq-test"; //all files are stored in amazon bucket
            Utilities.AmazonBucketNameProduction = "afterschoolhq-prod"; //all files are stored in amazon bucket
            Utilities.AmazonBucketNameLocal = "afterschoolhq-local"; //all files are stored in amazon bucket

            Utilities.AmazonAccessKeyId = "AKIAIRWKZE6AEP57RE2Q";
            Utilities.AmazonSecretAccessKeyId = "j8rbG/g32C2f15U0xLy4os5RNkypQgFxxVqCcS9N";

            Email.AppAdminEmailAddress = "support@afterschoolhq.com"; //all app admin emails are sent to this email address
            Email.ReplyHostName = "afterschoolhq.com"; // can be modified based on live/dev/test server
            Email.SenderHostName = "mailer.afterschoolhq.com"; // can be modified based on live/dev/test server            

            Utilities.HostNameProductionAfterSchoolHQ = "afterschoolhq.com"; //since the hostname may not be able to be picked up from the actual url that is processing the request
            Utilities.HostNameStagingAfterSchoolHQ = "staging.afterschoolhq.com";
            Utilities.HostNameCIAfterSchoolHQ = "ci.afterschoolhq.com";
            Utilities.HostNameQAAfterSchoolHQ = "qa.afterschoolhq.com";
            Utilities.HostNameLocalAfterSchoolHQ = "localhost:54849";

            Utilities.AppNameAfterSchoolHQ = "AfterSchool HQ";
            Utilities.AppNameEnrollHQ = "Enroll HQ";

            Utilities.ReCaptchaPrivateKey = "6Lfk694SAAAAAH5e3gWv3JckrYzz2scslo_rop6M"; //can be used as standard re-captcha
            Utilities.ReCaptchaPublicKey = "6Lfk694SAAAAAIrtt3Hwiz7j4emDRlYuLp73i9VY";
        }

        public static SystemMessages InitDatabase(string adminEmail = null, string adminPassword = null, bool skipTableCreation = false, string accountName = "First Group")
        {
            var msgs = new SystemMessages();

            if (!skipTableCreation)
            {
                var dc2 = new BasicLibrary.DataAccess.DatabaseConnection(AfterSchoolLibrary.Utilities.ConnectionString);

                msgs.AddRange(Database.CreateTables(dc2));
                msgs.AddRange(Database.AlterTables(dc2));
                msgs.AddRange(Database.UpdateData(dc2));
                dc2 = null;
            }

            msgs.Clear();

            var dc = Database.StartTransaction();

            try
            {
                msgs.AddRange(AccountService.CreateAfterSchoolAccount(adminEmail, adminPassword, accountName, dc));
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.UserError);
            }
            finally
            {
                Database.EndTransaction(dc, msgs);
            }

            msgs = new SystemMessages();
            dc = Database.StartTransaction();

            try
            {
                msgs.AddRange(AdditionalData(dc: dc));
                //msgs.AddRange(UpdateAccountFeatures(dc: dc));
                //msgs.AddRange(UpdateLocations(dc: dc));
                //msgs.AddRange(UpdateQuestionGroups(dc: dc));
                //msgs.AddRange(UpdateRegistrationModes(dc: dc));
                //msgs.AddRange(UpdateRegistrations(dc: dc));
                //msgs.AddRange(UpdateRegistrants(dc: dc));
                //msgs.AddRange(UpdateAccountProperties(dc: dc));
                //msgs.AddRange(UpdateAnswers(dc: dc));
                //msgs.AddRange(UpdateGradeLevels(dc: dc));
                //// msgs.AddRange(AddBellecChasseCustomQuestions(dc: dc));
                //// msgs.AddRange(UpdateApplicationSeatIndexesRunOnce(dc: dc));
                //msgs.AddRange(UpdateSessionActions(dc: dc));
                //// msgs.AddRange(UpdateActivityGroupsRunOnce(dc: dc));
                //// msgs.AddRange(UpdateActivities(dc: dc));
                //msgs.AddRange(UpdateAccountPackages(dc: dc));
                // msgs.AddRange(UpdateActivitySchedules(dc: dc));
                //msgs.AddRange(UpdateRegistrantActivities(dc: dc));
                //// msgs.AddRange(HandleDuplicateRegistrations(dc: dc));
                //msgs.AddRange(FixMissingMasterRecords(dc: dc));
                //// msgs.AddRange(FixMessedUpPreferences(dc: dc));
                // msgs.AddRange(UpdateRegistrantEmailQuestionGroup(dc: dc));
                // msgs.AddRange(CreateMasterRecord(dc: dc));
                msgs.AddRange(CreateSessionsLeagues(dc: dc));
                msgs.AddRange(CreateActivitiesTeams(dc: dc));
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.UserError);
            }
            finally
            {
                Database.EndTransaction(dc, msgs);
            }

            return msgs;
        }

        public static SystemMessages CreateSessionsLeagues(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var groups = ActivityGroup.Load(
                    criterias: new Hashtable() { { "ActivityGroupTypeId", 0 } },
                    dc: dc
                );

            groups.ForEach(group => {
                group.ActivityGroupTypeId = ActivityGroupType.Session;
                msgs.AddRange(group.Save(dc));
            });

            return msgs;
        }

        public static SystemMessages CreateActivitiesTeams(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var activities = Activity.Load(
                    criterias: new Hashtable() { { "ActivityTypeId", 0 } },
                    dc: dc
                );

            activities.ForEach(activity => {
                activity.ActivityTypeId = ActivityType.Activity;
                msgs.AddRange(activity.Save(dc));
            });

            return msgs;
        }

        public static SystemMessages LoadAndHashAllUsers(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var users = WebUser.Load(dc: dc);
            foreach (WebUser user in users)
            {
                var Pass = WebUserService.DecryptPassword(user.Password);
                user.Salt = System.Text.Encoding.UTF8.GetString(WebUserService.SaltandHashPassword(Pass).saltedbytes);
                user.Password = WebUserService.SaltandHashPassword(Pass).hashedbytes.ToString();
                msgs.AddRange(user.Save(dc));

            }
            return msgs;
        }

        public static SystemMessages AdditionalData(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(GradeLevel.CreateDefaultGradeLevels(dc));

            var form = FormService.AddNewAfterSchoolDefaultForm(msgs, dc);
            var package = PackageService.CreateDefaultPackage(null, msgs, dc);

            msgs.AddRange(AccountService.CreateSampleAccount(AccountType.School, package.ID.Value, dc));

            return msgs;
        }

        public static SystemMessages UpdateAccountFeatures(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var aps = AccountPackage.Load(dc: dc);
            foreach (var ap in aps)
            {
                msgs.AddRange(ap.ToPackage().CopyFeaturesToAccountPackage(ap, 0, dc: dc));
            }

            return msgs;
        }

        public static SystemMessages UpdateLocations(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var accounts = Account.Load(dc: dc);
            foreach (var account in accounts)
            {
                var locations = Location.LoadByHostAccountId(account.ID.Value, dc: dc);
                if (locations.Count == 0)
                {
                    var location = new Location();
                    location.IsDefault = true;
                    location.HostAccountId = account.ID.Value;
                    location.Address1 = account.Address1;
                    location.Address2 = account.Address2;
                    location.City = account.City;
                    location.State = account.State;
                    location.PostalCode = account.PostalCode;
                    msgs.AddRange(location.Save(dc: dc));
                }
            }

            return msgs;
        }

        public static SystemMessages UpdateQuestionGroups(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var questionGroups = FormQuestionGroup.Load(criterias: new Hashtable() { { "QuestionGroupDataTypeId", (int)QuestionGroupDataType.NotSet } }, dc: dc);
            foreach (var questionGroup in questionGroups)
            {
                QuestionGroupDataType datatype;
                if (Enum.TryParse<QuestionGroupDataType>(questionGroup.Name.Replace(" ", ""), out datatype))
                {
                    questionGroup.QuestionGroupDataTypeId = datatype;
                    msgs.AddRange(questionGroup.Save(dc: dc));
                }
            }

            return msgs;
        }

        public static SystemMessages UpdateAnswers(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var answers = FormQuestionAnswer.Load(criterias: new Hashtable() { { "RegistrationId", DBNull.Value } }, dc: dc);
            answers = answers.Where(x => x.ActivityId.HasValue).ToList();
            foreach (var answer in answers)
            {
                var ra = RegistrantActivity.LoadByActivityIdByRegistrantId(answer.ActivityId.Value, answer.RegistrantId.Value, dc: dc);
                if (ra.IsNotEmpty() && ra.RegistrationId.HasValue)
                {
                    answer.RegistrationId = ra.RegistrationId;
                    answer.QuestionDataTypeId = answer.FormQuestionQuestionDataTypeId;
                    msgs.AddRange(answer.Save(dc: dc));
                }
            }

            return msgs;
        }

        public static SystemMessages UpdateRegistrations(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var registrations = Registration.Load(
                                        criterias: new Hashtable() { { "FirstRegistrationId", DBNull.Value } }, 
                                        dc: dc
                                    )
                                    .Where(x => x.AccountId.HasValue)
                                    .OrderBy(x => x.ID.Value)
                                    .ToList();

            while (registrations.Any())
            {
                var registration = registrations.First();
                var matches = registrations.Where(x => 
                                    x.RegistrantFirstName.IsNotEmpty() &&
                                    x.RegistrantLastName.IsNotEmpty() &&
                                    x.RegistrantEmail.IsNotEmpty() &&
                                    registration.RegistrantFirstName.IsNotEmpty() &&
                                    registration.RegistrantLastName.IsNotEmpty() &&
                                    registration.RegistrantEmail.IsNotEmpty() &&
                                    x.RegistrantFirstName.Trim().ToLower() == registration.RegistrantFirstName.Trim().ToLower() &&
                                    x.RegistrantLastName.Trim().ToLower() == registration.RegistrantLastName.Trim().ToLower() &&
                                    x.RegistrantEmail.Trim().ToLower() == registration.RegistrantEmail.Trim().ToLower() &&
                                    x.ID != registration.ID && 
                                    x.AccountId == registration.AccountId
                                ).ToList();

                foreach (var match in matches)
                {
                    msgs.AddRange(registration.AddRelatedRegistration(match, 0, dc: dc));
                    registrations.Remove(match);
                }

                registrations.Remove(registration);
            }

            /*
            // RAN and is done now, no need to run again
            var registrants = Registrant.Load(criterias: new Hashtable() { { "IsContactOnly", false } },dc: dc);
            foreach (var registrant in registrants)
            {
                var registrationCount = registrant.GetRegistrationCount(dc: dc);
                var activities = registrant.GetActivities(dc: dc);
                if (registrationCount == 0 && activities.Any())
                {
                    var registration = registrant.AddRegistration(0, registrant.AccountId, null, msgs, dc: dc);
                    registration.CreateDate = registration.LastModifiedDate = registrant.CreateDate;
                    msgs.AddRange(registration.Save(dc: dc));

                    foreach (var activity in activities)
                    {
                        msgs.AddRange(registration.AddActivity(activity, 0, dc: dc));

                        var regactivity = RegistrantActivity.LoadByActivityIdByRegistrantId(activity.ID.Value, registrant.ID.Value, dc: dc);
                        if (regactivity.IsNotEmpty())
                        {
                            regactivity.RegistrationId = registration.ID.Value;
                            msgs.AddRange(regactivity.Save(dc: dc));

                            registration.StripeChargeId = regactivity.StripeChargeId;
                            registration.PaymentTypeId = regactivity.PaymentTypeId;
                            registration.PayPalTransactionId = regactivity.PayPalTransactionId;
                            registration.DatePaid = regactivity.DatePaid;
                            registration.AmountPaid = regactivity.AmountPaid;
                            msgs.AddRange(registration.Save(dc: dc));

                            var answers = regactivity.GetAnswers(dc: dc);
                            foreach (var answer in answers)
                            {
                                answer.RegistrationId = registration.ID;
                                answer.QuestionDataTypeId = answer.FormQuestionQuestionDataTypeId;
                                msgs.AddRange(answer.Save(dc: dc));
                            }
                        }
                    }
                }
            }

            var registrations = Registration.Load(criterias: new Hashtable() { { "LastUpdated", DBNull.Value } }, dc: dc);
            foreach(var reg in registrations)
            {
                var ras = reg.GetRegistrantActivities(dc: dc);
                if(ras.Any())
                {
                    var maxdate = ras.Max(x => x.LastModifiedDate.Value);
                    reg.LastUpdated = maxdate;
                }
                else
                {
                    reg.LastUpdated = reg.LastModifiedDate;
                }

                msgs.AddRange(reg.Save(dc: dc));
            }
            */

            return msgs;
        }

        public static SystemMessages UpdateRegistrants(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var registrants = Registrant.Load(
                                    criterias: new Hashtable() { { "FirstRegistrantId", DBNull.Value } },
                                    dc: dc)
                                .Where(x => x.AccountId.HasValue)
                                .OrderBy(x => x.ID.Value)
                                .ToList();

            while (registrants.Any())
            {
                var registrant = registrants.First();
                var matches = registrants.Where(x =>
                                    x.FirstName.IsNotEmpty() &&
                                    x.LastName.IsNotEmpty() &&
                                    x.Email.IsNotEmpty() &&
                                    registrant.FirstName.IsNotEmpty() &&
                                    registrant.LastName.IsNotEmpty() &&
                                    registrant.Email.IsNotEmpty() &&
                                    x.FirstName.Trim().ToLower() == registrant.FirstName.Trim().ToLower() &&
                                    x.LastName.Trim().ToLower() == registrant.LastName.Trim().ToLower() &&
                                    x.Email.Trim().ToLower() == registrant.Email.Trim().ToLower() &&
                                    x.ID != registrant.ID &&
                                    x.AccountId == registrant.AccountId
                                ).ToList();

                foreach (var match in matches)
                {
                    msgs.AddRange(registrant.AddRelatedRegistrant(match, 0, dc: dc));
                    registrants.Remove(match);
                }

                registrants.Remove(registrant);
            }

            registrants = Registrant.Load(criterias: new Hashtable() { { "IsMaster", true } }, dc: dc);
            foreach(var registrant in registrants)
            {
                var potentialIssues = Registrant.Load(criterias: new Hashtable() { { "Uid", registrant.Uid } }, dc: dc);
                if (potentialIssues.Count > 1)
                {
                    var issues = potentialIssues.Where(x => x.ID != registrant.ID).ToList();
                    foreach (var issue in issues)
                    {
                        issue.Uid = Guid.NewGuid();
                        msgs.AddRange(issue.Save(dc: dc));
                    }
                }
            }

            return msgs;
        }

        public static SystemMessages UpdateAccountProperties(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var accounts = Account.Load(dc: dc);
            foreach (var account in accounts)
            {
                var properties = AccountProperty.LoadByAccountId(account.ID.Value, dc: dc);
                if (properties.Count == 0 && account.LandingPageText.IsNotBlank())
                {
                    var property = new AccountProperty();
                    property.AccountId = account.ID;
                    property.AccountPropertyTypeId = AccountPropertyType.LandingPageText;
                    property.Value = account.LandingPageText;
                    property.CreateDate = property.LastModifiedDate = DateTime.UtcNow;
                    property.CreatedBy = property.LastModifiedBy = 0;
                    msgs.AddRange(property.Save(dc: dc));
                }
            }
            return msgs;
        }

        public static SystemMessages UpdateRegistrationModes(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var activities = Activity.Load(criterias: new Hashtable() { { "RegistrationModeId", (int)RegistrationMode.NotSet } }, dc: dc);
            foreach (var activity in activities)
            {
                activity.RegistrationModeId = RegistrationMode.Register;
                msgs.AddRange(activity.Save(dc: dc));
            }

            activities = Activity.Load(criterias: new Hashtable() { { "RegistrationModeId", DBNull.Value } }, dc: dc);
            foreach (var activity in activities)
            {
                activity.RegistrationModeId = RegistrationMode.Register;
                msgs.AddRange(activity.Save(dc: dc));
            }

            return msgs;
        }

        public static SystemMessages UpdateGradeLevels(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var activities = Activity.Load(dc: dc);
            activities = activities.Where(x => x.GradeLevelStart.HasValue && x.GradeLevelEnd.HasValue).ToList();
            foreach (var activity in activities)
            {
                if (activity.GetGradeLevels(dc: dc).Count == 0)
                {
                    for (var i = activity.GradeLevelStart.Value; i <= activity.GradeLevelEnd.Value; i++)
                    {
                        var grade = GradeLevel.Load(i, dc: dc);
                        if (grade.IsNotEmpty())
                        {
                            activity.AddGrade(grade.ID.Value, activity.LastModifiedBy.Value, msgs, dc: dc);
                        }
                    }
                }
            }

            return msgs;
        }

        //public static SystemMessages AddBellecChasseCustomQuestions(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        //{
        //    var msgs = new SystemMessages();

        //    var questions = new QuestionHolder[]
        //    {
        //        new QuestionHolder()
        //        {
        //            GroupName = "Personal Information",
        //            IsActive = true,
        //            IsLocked = false,
        //            IsRequired = false,
        //            Options = "Yes,No",
        //            QuestionDataTypeId = QuestionDataType.NotSet,
        //            QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation,
        //            QuestionTypeId = QuestionType.RadioList,
        //            RegistrantTypeId = RegistrantType.Individual,
        //            Text = "Individualized Education Plan (IEP)"
        //        },
        //        new QuestionHolder()
        //        {
        //            GroupName = "Personal Information",
        //            IsActive = true,
        //            IsLocked = false,
        //            IsRequired = false,
        //            Options = "Teacher 1, Teacher 2",
        //            QuestionDataTypeId = QuestionDataType.NotSet,
        //            QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation,
        //            QuestionTypeId = QuestionType.DropDownList,
        //            RegistrantTypeId = RegistrantType.Individual,
        //            Text = "Homeroom Teacher"
        //        },
        //        new QuestionHolder()
        //        {
        //            GroupName = "Dismissal Information",
        //            IsActive = true,
        //            IsLocked = false,
        //            IsRequired = false,
        //            Options = "Monday,Tuesday, Wednesday,Thursday;Bus,Walker,Car Rider",
        //            QuestionDataTypeId = QuestionDataType.NotSet,
        //            QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
        //            QuestionTypeId = QuestionType.RadioMatrix,
        //            RegistrantTypeId = RegistrantType.Individual,
        //            Text = "Dismissal"
        //        },
        //        new QuestionHolder()
        //        {
        //            GroupName = "Dismissal Information",
        //            IsActive = true,
        //            IsLocked = false,
        //            IsRequired = false,
        //            Options = "Yes,No",
        //            QuestionDataTypeId = QuestionDataType.NotSet,
        //            QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
        //            QuestionTypeId = QuestionType.RadioList,
        //            RegistrantTypeId = RegistrantType.Individual,
        //            Text = "Does your child's transportation arrangements change during inclement weather?"
        //        },
        //        new QuestionHolder()
        //        {
        //            GroupName = "Dismissal Information",
        //            IsActive = true,
        //            IsLocked = false,
        //            IsRequired = false,
        //            Options = "Monday,Tuesday, Wednesday,Thursday;Bus,Walker,Car Rider",
        //            QuestionDataTypeId = QuestionDataType.NotSet,
        //            QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
        //            QuestionTypeId = QuestionType.RadioMatrix,
        //            RegistrantTypeId = RegistrantType.Individual,
        //            Text = "Dismissal Rainy Day"
        //        },
        //        new QuestionHolder()
        //        {
        //            GroupName = "Dismissal Information",
        //            IsActive = true,
        //            IsLocked = false,
        //            IsRequired = false,
        //            Options = "N/A,1A,1,2,3,4,5,6,7,8",
        //            QuestionDataTypeId = QuestionDataType.NotSet,
        //            QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
        //            QuestionTypeId = QuestionType.DropDownList,
        //            RegistrantTypeId = RegistrantType.Individual,
        //            Text = "Bus Number"
        //        },
        //        new QuestionHolder()
        //        {
        //            GroupName = "Terms & Conditions",
        //            IsActive = true,
        //            IsLocked = false,
        //            IsRequired = false,
        //            Options = "",
        //            QuestionDataTypeId = QuestionDataType.NotSet,
        //            QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
        //            QuestionTypeId = QuestionType.Checkbox,
        //            RegistrantTypeId = RegistrantType.Individual,
        //            Text = "I understand the policy and procedures for being enrolled in a 21st CCLC activity",
        //            Link = "https://rebornprojects.com/file/download/fcf9ba11-79c0-472d-a4cb-bdd67c2bf450/21ST-CCLC-Attendance-Behavior-Policy-2016-2017.docx"
        //        }
        //    };

        //    var accountId = 18;
        //    if (Utilities.IsLiveServer || Utilities.IsStagingServer)
        //    {
        //        accountId = 6;
        //    }

        //    var forms = Form.Load(criterias: new Hashtable() { { "AccountId", accountId } }, dc: dc);

        //    foreach (var f in forms)
        //    {
        //        var account = Account.Load(f.AccountId.Value, dc: dc);

        //        foreach (var question in questions)
        //        {
        //            FormQuestionGroup group = FormQuestionGroup.Load(
        //                criterias: new Hashtable() { { "Name", question.GroupName }, { "FormId", f.ID.Value } },
        //                dc: dc).FirstOrDefault();

        //            if (group.IsEmpty())
        //            {
        //                int groupDisplayOrder = f.GetQuestionGroups(dc: dc).Where(x => x.RegistrantTypeId == RegistrantType.Individual).OrderBy(x => x.DisplayOrder).Last().DisplayOrder.Value + 1;
        //                group = new FormQuestionGroup();
        //                group.Name = question.GroupName;
        //                group.FormId = f.ID;
        //                group.DisplayOrder = groupDisplayOrder;
        //                group.RegistrantTypeId = question.RegistrantTypeId;
        //                group.LastModifiedDate = group.CreateDate = DateTime.UtcNow;
        //                group.LastModifiedBy = group.CreatedBy = 0;
        //                msgs.AddRange(group.Save(dc: dc));
        //            }

        //            FormQuestion q = FormQuestion.Load(criterias: new Hashtable() { { "Text", question.Text }, { "FormId", f.ID.Value } }, dc: dc).FirstOrDefault();
        //            if (q.IsEmpty())
        //            {
        //                int displayOrder = 0;
        //                if (group.GetAllQuestions(dc: dc).Any())
        //                {
        //                    displayOrder = group.GetAllQuestions(dc: dc).OrderBy(x => x.DisplayOrder).Last().DisplayOrder.Value + 1;
        //                }
        //                q = new FormQuestion();
        //                q.Text = question.Text;
        //                q.IsActive = question.IsActive;
        //                q.IsRequired = question.IsRequired;
        //                q.FormId = f.ID;
        //                q.FormQuestionGroupId = group.ID;
        //                q.IsTemplate = account.DefaultFormId == f.ID;
        //                q.IsLocked = question.IsLocked;
        //                q.DisplayOrder = displayOrder;
        //                q.QuestionTypeId = question.QuestionTypeId;
        //                q.QuestionDataTypeId = question.QuestionDataTypeId;
        //                q.Options = question.Options;
        //                q.Link = question.Link;
        //                q.LastModifiedDate = q.CreateDate = DateTime.UtcNow;
        //                q.LastModifiedBy = q.CreatedBy = 0;
        //                msgs.AddRange(q.Save(dc: dc));
        //            }
        //        }
        //    }

        //    return msgs;
        //}

        public static SystemMessages UpdateApplicationSeatIndexesRunOnce(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var ras = RegistrantActivity.Load(dc: dc);

            var activityIds = ras.Select(x => x.ActivityId.Value).ToList();

            var accountId = 18;
            if (Utilities.IsProduction || Utilities.IsStaging)
            {
                accountId = 6;
            }

            foreach (var id in activityIds)
            {
                var activity = Activity.Load(id, dc: dc);
                var activityRas = new List<RegistrantActivity>();
                if (activity.GetDays().Count == 0)
                {
                    var index = 0;
                    activityRas = ras.Where(x => x.ActivityId.Value == id && x.NumberOfApplicationSeats.HasValue && x.ActivityAccountId == accountId).OrderBy(x => x.CreateDate.Value).ToList();
                    foreach (var ra in activityRas)
                    {
                        index++;
                        ra.ApplicationSeatIndex = index;
                        msgs.AddRange(ra.Save(dc: dc));
                    }
                }
                else
                {
                    foreach (var day in activity.GetDaysOfTheWeek())
                    {
                        var index = 0;
                        activityRas = ras.Where(x => x.ActivityId.Value == id && x.NumberOfApplicationSeats.HasValue && x.ActivityAccountId == accountId && x.Day == day).OrderBy(x => x.CreateDate.Value).ToList();
                        foreach (var ra in activityRas)
                        {
                            index++;
                            ra.ApplicationSeatIndex = index;
                            msgs.AddRange(ra.Save(dc: dc));
                        }
                    }
                }
            }
            return msgs;
        }

        public static SystemMessages UpdateSessionActions(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var actions = SessionAction.Load(dc: dc);

            actions = actions.Where(x => x.ActivityId.HasValue && !x.RegistrationPeriodId.HasValue).ToList();

            foreach (var action in actions)
            {
                var activity = Activity.Load(action.ActivityId.Value, dc: dc);
                if (activity.IsNotEmpty())
                {
                    var regperiod = activity.GetActiveRegistrationPeriod(dc: dc);
                    if (regperiod.IsNotEmpty())
                    {
                        action.RegistrationPeriodId = regperiod.ID.Value;
                        msgs.AddRange(action.Save(dc: dc));
                    }
                }
            }

            return msgs;
        }        

        public static SystemMessages UpdateActivityGroupsRunOnce(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            /*
            var periods = RegistrationPeriod.Load(dc: dc).Where(x => x.ActivityGroupId.HasValue == false && x.AccountId.HasValue);

            foreach(var period in periods)
            {
                try
                { 
                    var group = new ActivityGroup();
                    group.AccountId = period.AccountId;
                    group.RegistrationModeId = period.RegistrationModeId;
                    group.CreateDate = period.CreateDate;
                    group.CreatedBy = period.CreatedBy;
                    group.LastModifiedDate = period.LastModifiedDate;
                    group.LastModifiedBy = period.LastModifiedBy;
                    group.Description = period.Description;
                    group.Name = period.Name;
                    group.UniqueName = ActivityGroup.GenerateUniqueName(group.AccountId.Value, group.Name, dc: dc);
                    msgs.AddRange(group.Save(dc: dc));

                    period.ActivityGroupId = group.ID;
                    msgs.AddRange(period.Save(dc: dc));

                    var activities = period.GetActivities(dc: dc);
                    foreach(var activity in activities)
                    {
                        activity.ActivityGroupId = group.ID;
                        msgs.AddRange(activity.Save(dc: dc));
                    }

                    var registrations = period.GetRegistrations(dc: dc);
                    foreach (var registration in registrations)
                    {
                        registration.ActivityGroupId = group.ID;
                        msgs.AddRange(registration.Save(dc: dc));
                    }

                    var ras = period.GetRegistrantActivities(dc: dc);
                    foreach (var ra in ras)
                    {
                        ra.ActivityGroupId = group.ID;
                        msgs.AddRange(ra.Save(dc: dc));
                    }

                    group.AddOrUpdateGroupProperty(AccountPropertyType.LandingPageText, "Please register below.", 0, msgs);
                    group.AddOrUpdateGroupProperty(AccountPropertyType.RegistrationThankYouEmail, "Thank you for registering.", 0, msgs);
                    group.AddOrUpdateGroupProperty(AccountPropertyType.ApplicationThankYouEmail, "Thank you for applying.", 0, msgs);
                    group.AddOrUpdateGroupProperty(AccountPropertyType.NoActivitiesText, "Registration is now closed.", 0, msgs);
                }
                catch (Exception ex)
                {
                    msgs.Add(ex.Message, MessageType.Error);
                }
            }
            */
            return msgs;
        }

        public static SystemMessages UpdateActivities(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            //run once
            var msgs = new SystemMessages();
            var activities = Activity.Load(dc: dc);

            //moving publishing to the actual activity instead of the activity form
            foreach(var activity in activities)
            {
                var form = activity.GetActiveActivityForm(dc: dc);
                if (form.IsNotEmpty())
                {
                    activity.IsPublished = form.IsPublished;
                    msgs.AddRange(activity.Save(dc: dc));
                }
            }

            return msgs;
        }

        public static SystemMessages UpdateAccountPackages(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var accountPackages = AccountPackage.Load(criterias: new Hashtable() { { "BillingPeriodTypeId", 0 } }, dc: dc);
            foreach (var accountPackage in accountPackages)
            {
                accountPackage.BillingPeriodTypeId = BillingPeriodType.Month;
                msgs.AddRange(accountPackage.Save(dc: dc));
            }

            return msgs;
        }

        public static SystemMessages UpdateActivitySchedules(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            /*
            var activities = Activity.Load( 
                criterias: 
                new Hashtable() {
                    { "ActivityScheduleId", DBNull.Value }
                },
                dc: dc);

            foreach (var activity in activities)
            {
                var schedule = new ActivitySchedule();
                schedule.Name = ActivitySchedule.GenerateUniqueName(activity.AccountId.Value, activity.Name, dc: dc);
                schedule.ScheduleDetails = activity.ScheduleDetails.IfBlank("").MaxChars(4000);
                schedule.AccountId = activity.AccountId;
                schedule.CreateDate = schedule.LastModifiedDate = DateTime.UtcNow;
                schedule.CreatedBy = schedule.LastModifiedBy = activity.LastModifiedBy;
                msgs.AddRange(schedule.Save(dc: dc));

                activity.ActivityScheduleId = schedule.ID;
                msgs.AddRange(activity.Save(dc: dc));
            }
            */

            var schedules = ActivitySchedule.Load(
                criterias:
                new Hashtable() {
                    { "ActivityGroupid", DBNull.Value }
                },
                dc: dc);

            foreach(var item in schedules)
            {
                var activity = Activity.Load(criterias: new Hashtable() { { "ActivityScheduleId", item.ID } }, dc: dc).First();
                if (activity.ActivityGroupId.HasValue)
                {
                    item.ActivityGroupId = activity.ActivityGroupId;
                    msgs.AddRange(item.Save(dc: dc));
                }
            }

            return msgs;
        }

        public static SystemMessages UpdateRegistrantActivities(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            msgs.AddRange(RegistrantActivity.Update(
                new Hashtable()
                {
                     { "NumberOfInvitationSeats", 0 }
                },
                new Hashtable()
                {
                    { "NumberOfInvitationSeats", DBNull.Value }
                },
                RegistrantActivity.ObjectMap,
                dc
                )
            );
            msgs.AddRange(RegistrantActivity.Update(
                new Hashtable()
                {
                     { "NumberOfSeatsDeclined", 0 }
                },
                new Hashtable()
                {
                    { "NumberOfSeatsDeclined", DBNull.Value }
                },
                RegistrantActivity.ObjectMap,
                dc
                )
            );
            return msgs;
        }

        public static SystemMessages HandleDuplicateRegistrations(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            // running for bellechasse
            var msgs = new SystemMessages();
            var registrations = Registration.LoadByActivityGroupId(175, dc: dc);

            while (registrations.Any())
            {
                var registration = registrations.First();
                var related = registrations.Where(x =>
                                    x.RegistrantFirstName.Trim().ToLower() == registration.RegistrantFirstName.Trim().ToLower() &&
                                    x.RegistrantLastName.Trim().ToLower() == registration.RegistrantLastName.Trim().ToLower() &&
                                    x.RegistrantEmail.Trim().ToLower() == registration.RegistrantEmail.Trim().ToLower() &&
                                    x.ID != registration.ID
                                ).ToList();

                var allRelated = new List<Registration>();
                allRelated.Add(registration);
                allRelated.AddRange(related);

                if (allRelated.Count > 1)
                {
                    var lastRegistration = allRelated.OrderBy(x => x.CreateDate).Last();
                    var lastRegistrant = lastRegistration.GetRegistrant(dc: dc);

                    var regActivities = new List<RegistrantActivity>();
                    var registrationActivities = new List<RegistrationActivity>();
                    var distinctActivityIds = new List<int>();
                    var finalActivityIds = new List<int>();
                    var allRegistrants = allRelated.Select(x => x.GetRegistrant(dc: dc)).ToList();
                    var studentGrades = allRegistrants.Select(x => x.GetAnswer(QuestionDataType.RegistrantGrade, dc: dc)).DistinctBy(x => x.Text).ToList();

                    if (studentGrades.Count != 1)
                    {
                        // somebody said they were different grades on the same registration group / period
                        // that ain't right
                        msgs.Add(string.Format("Has different grades - {0}", registration.ID), MessageType.Notice);
                    }
                    else
                    {
                        foreach (var item in allRelated)
                        {
                            regActivities.AddRange(item.GetRegistrantActivities(dc: dc));
                            registrationActivities.AddRange(item.GetRegistrationActivities(dc: dc));
                        }

                        distinctActivityIds = regActivities.Select(x => x.ActivityId.Value).Distinct().ToList();

                        // check for activities where the grade level of the student is in the list of grades accepted by the activity
                        // if doesn't contain grade level then it will need to be removed
                        foreach (var activityId in distinctActivityIds)
                        {
                            var activity = Activity.Load(activityId, dc: dc);
                            var activityGrades = activity.GetGradeLevels(dc: dc).Select(x => x.Name.Trim().ToLower()).ToList();

                            if (activityGrades.Contains(studentGrades.First().Text.Trim().ToLower()))
                            {
                                finalActivityIds.Add(activityId);
                            }
                        }

                        // keep last registration
                        foreach (var activityId in finalActivityIds)
                        {
                            var activity = Activity.Load(activityId, dc: dc);
                            // get only reg activities for the last registrant
                            var lastRegistrantActivities = regActivities.Where(x => x.RegistrantId == lastRegistrant.ID).ToList();

                            // if the activity is not in the list of activities for this particular last registrant
                            // then we need to remove it from the old registrant and registration
                            // then add it to the last registrant and last registration
                            // will do this by simply changing the registration id and registrant id
                            if (lastRegistrantActivities.Where(x => x.ActivityId == activityId).Count() == 0)
                            {
                                // should move the first reg activity added to any of the registrations, 
                                // since that will have the highest seating index, etc.
                                var regActivityToMove = regActivities.Where(x => x.ActivityId == activityId).OrderBy(x => x.CreateDate).FirstOrDefault();
                                regActivityToMove.RegistrationId = lastRegistration.ID;
                                regActivityToMove.RegistrantId = lastRegistrant.ID;
                                msgs.AddRange(regActivityToMove.Save(dc: dc));

                                // move the matching registration
                                // TODO: probably should cross reference the original registrationId of the regactivity to move with this one that is found
                                var registrationActivityToMove = registrationActivities.Where(x => x.ActivityId == activityId).OrderBy(x => x.CreateDate).FirstOrDefault();
                                registrationActivityToMove.RegistrationId = lastRegistration.ID;
                                msgs.AddRange(registrationActivityToMove.Save(dc: dc));
                            }
                        }

                        // remove activities not in the grade level from the last registration and registrant
                        var activityIdsToRemove = distinctActivityIds.Where(x => !finalActivityIds.Contains(x)).ToList();
                        foreach (var activityId in activityIdsToRemove)
                        {
                            // removes it from the registrant as well
                            msgs.AddRange(lastRegistration.RemoveActivity(activityId, dc: dc));
                        }

                        // Remove all activities from other registrations and registrants
                        // except for the last one registrant and registration
                        foreach (var item in allRelated.Where(x => x.ID != lastRegistration.ID))
                        {
                            foreach (var activityId in regActivities.Where(x => x.RegistrationId == item.ID).Select(x => x.ActivityId.Value))
                            {
                                // removes it from the registrant as well
                                msgs.AddRange(item.RemoveActivity(activityId, dc: dc));
                            }
                        }

                        // now delete the other registrants and registrations
                        foreach (var item in allRelated.Where(x => x.ID != lastRegistration.ID))
                        {
                            var registrant = item.GetRegistrant(dc: dc);
                            msgs.AddRange(registrant.Delete(dc: dc));
                            msgs.AddRange(item.Delete(dc: dc));

                            if (lastRegistration.FirstRegistrationId == item.ID)
                            {
                                lastRegistration.FirstRegistrationId = null;
                                msgs.AddRange(lastRegistration.Save(dc: dc));
                            }
                            if (lastRegistrant.FirstRegistrantId == registrant.ID)
                            {
                                lastRegistrant.FirstRegistrantId = null;
                                msgs.AddRange(lastRegistrant.Save(dc: dc));
                            }
                        }

                        var newRegistrantActivities = lastRegistration.GetRegistrantActivities(dc: dc).OrderBy(x => x.PreferenceNumber).ThenBy(x => x.CreateDate);
                        // update preference order
                        int i = 1;
                        foreach(var ra in newRegistrantActivities)
                        {
                            ra.PreferenceNumber = i;
                            msgs.AddRange(ra.Save(dc: dc));
                            i++;
                        }

                        // check registrations and activities
                        var newRelatedRegs = lastRegistration.GetRelatedRegistrations(dc: dc).Where(x => x.ActivityGroupId == 175).ToList();
                        var newRegistrationActivityIds = lastRegistration.GetRegistrationActivities(dc: dc).Select(x => x.ActivityId.Value).ToList();
                        var newRegistrantActivityIds = newRegistrantActivities.Select(x => x.ActivityId.Value).ToList();

                        if (newRelatedRegs.Count == 0 &&
                            !newRegistrationActivityIds.Except(newRegistrantActivityIds).Any() &&
                            !newRegistrationActivityIds.Except(finalActivityIds).Any())
                        {
                            msgs.Add("It worked", MessageType.Success);
                        }
                        else
                        {
                            msgs.Add(string.Format("It didn't work - {0}", registration.ID), MessageType.Error);
                        }
                    }

                    // remove all the records that were just processed from the list
                    foreach (var item in related)
                    {
                        registrations.Remove(item);
                    }
                }

                registrations.Remove(registration);

            }

            // msgs.Add("Rollback please", MessageType.Error);

            return msgs;
        }

        public static SystemMessages CreateMasterRecord(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var registrant = Registrant.Load(5735, dc: dc);
            var questionIds = registrant.GetAnswers(dc: dc).Select(x => x.FormQuestionId.Value).ToList();
            var questions = FormQuestion.Load(questionIds, dc: dc);
            registrant.CreateMasterCopy(questions, 0, msgs, dc: dc);
            return msgs;
        }

        public static SystemMessages FixMissingMasterRecords(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var registrants = Registrant.Load(dc: dc)
                                .Where(x => x.WebUserId.HasValue && !x.IsMaster && x.ParentRegistrantId.HasValue)
                                .OrderBy(x => x.FirstName)
                                .ThenBy(x => x.LastName)
                                .ThenBy(x => x.Email).ToList();

            foreach (var registrant in registrants)
            {
                var user = WebUser.Load(registrant.WebUserId.Value, dc: dc);
                if (user.Password.IsNotEmpty() && !user.GetAccounts(dc: dc).Any())
                {
                    var master = Registrant.Load(
                                               criterias: new Hashtable()
                                               {
                                                { "ID", registrant.ParentRegistrantId }
                                               },
                                               dc: dc)
                                               .FirstOrDefault();

                    if (master.IsNotEmpty() && master.FirstName.ToLower().Trim() != registrant.FirstName.ToLower().Trim())
                    {
                        registrant.ParentRegistrantId = null;
                        msgs.AddRange(registrant.Save(dc: dc));
                    }
                }
            }

            registrants = Registrant.Load(dc: dc)
                                .Where(x => x.WebUserId.HasValue && !x.IsMaster && !x.ParentRegistrantId.HasValue && x.AccountId.HasValue)
                                .OrderBy(x => x.FirstName)
                                .ThenBy(x => x.LastName)
                                .ThenBy(x => x.Email).ToList();

            foreach (var registrant in registrants)
            {
                var user = WebUser.Load(registrant.WebUserId.Value, dc: dc);
                if (user.Password.IsNotEmpty())
                {
                    var accounts = user.GetAccounts(dc: dc);
                    foreach(var account in accounts)
                    {
                        msgs.AddRange(account.Delete(dc: dc));
                    }
                    var awus = AccountWebUser.LoadByWebUserId(user.ID.Value, dc: dc);
                    foreach (var awu in awus)
                    {
                        msgs.AddRange(awu.Delete(dc: dc));
                    }

                    var potentialMaster = Registrant.Load(
                                            criterias: new Hashtable()
                                            {
                                                { "FirstName", registrant.FirstName.Trim() },
                                                { "LastName", registrant.LastName.Trim() },
                                                { "Email", registrant.Email.Trim() },
                                                { "AccountId", DBNull.Value },
                                                { "IsMaster", true }
                                            }, 
                                            dc: dc)
                                            .FirstOrDefault();

                    if (potentialMaster.IsNotEmpty())
                    {
                        registrant.ParentRegistrantId = potentialMaster.ID;
                        msgs.AddRange(registrant.Save(dc: dc));
                    }
                    else
                    {
                        var answers = registrant.GetAnswers(dc: dc);
                        var questions = FormQuestion.Load(answers.Select(x => x.FormQuestionId.Value).ToList(), dc: dc);
                        var master = registrant.CreateMasterCopy(questions, user.ID.Value, msgs, dc: dc);
                    }
                }
            }

            

            return msgs;
        }

        /*
        public static SystemMessages FixMessedUpPreferences(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var dcbkp = new BasicLibrary.DataAccess.DatabaseConnection(Database.LiveBackUpCredentials.ConnectionString);
            var dclive = new BasicLibrary.DataAccess.DatabaseConnection(Database.LiveCredentials.ConnectionString);

            var ras = RegistrantActivity.Load(
                                            criterias: new Hashtable()
                                            {
                                                { "ActivityGroupId", 175 }
                                            },
                                            dc: dcbkp
                                       );

            foreach(var ra in ras)
            {
                var ra2 = RegistrantActivity.Load(ra.ID.Value, dclive);

                if (ra2.IsEmpty())
                {
                    msgs.Add(string.Format("Error loading from live {0} - {1} {2} - {3}", ra.ID, ra.RegistrantFirstName, ra.RegistrantLastName, ra.ActivityName), MessageType.Notice);
                }
                else
                {
                    if (ra2.PreferenceNumber != ra.PreferenceNumber &&
                        ra2.RegistrantFirstName == ra.RegistrantFirstName &&
                        ra2.RegistrantLastName == ra.RegistrantLastName &&
                        ra2.RegistrantEmail == ra.RegistrantEmail &&
                        ra2.ActivityId == ra.ActivityId)
                    {
                        // reset preference number
                        ra2.PreferenceNumber = ra.PreferenceNumber;
                        msgs.AddRange(ra2.Save(dclive));
                    }
                }      
            }


            return msgs;
        }
        */

        public static SystemMessages UpdateRegistrantEmailQuestionGroup(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var questions = FormQuestion.Load(criterias: new Hashtable() { { "QuestionDataTypeId", (int)QuestionDataType.RegistrantEmail } }, dc: dc);

            foreach(var question in questions)
            {
                var currentGroup = question.GetFormQuestionGroup(dc: dc);
                var newGroup = FormQuestionGroup.LoadByFormId(question.FormId.Value, dc: dc).Find(x => x.QuestionGroupDataTypeId == QuestionGroupDataType.ContactInformation);

                if (currentGroup.IsNotEmpty() && newGroup.IsNotEmpty())
                {
                    msgs.AddRange(currentGroup.RemoveQuestion(question, dc: dc));
                    msgs.AddRange(newGroup.AddQuestion(question, 0, dc: dc));
                }
            }

            return msgs;
        }

        public static string AmazonBucketNameProduction { get; set; }
        public static string AmazonBucketNameTest { get; set; }
        public static string AmazonBucketNameStaging { get; set; }
        public static string AmazonBucketNameCI { get; set; }
        public static string AmazonBucketNameQA { get; set; }
        public static string AmazonBucketNameLocal { get; set; }

        public static string AmazonBucketName
        {
            get
            {
                if (Utilities.IsProduction)
                {
                    return AmazonBucketNameProduction;
                }
                else if (Utilities.IsStaging)
                {
                    return AmazonBucketNameStaging;
                }
                else if (Utilities.IsQA)
                {
                    return AmazonBucketNameQA;
                }
                else if (Utilities.IsTest)
                {
                    return AmazonBucketNameTest;
                }
                else if (Utilities.IsCI)
                {
                    return AmazonBucketNameCI;
                }
                else
                {
                    return AmazonBucketNameLocal;
                }
            }
        }
    }

    public class Email
    {
        public static string BaseUrl(bool isAfterSchool)
        {
            return $"{Utilities.BaseUrl(isAfterSchool)}/";
        }

        public static string HostName(bool isAfterSchool)
        {
            return Utilities.HostName(isAfterSchool);
        }

        public static System.Net.Mail.MailAddress AppFromAddress(bool isAfterSchool)
        {
            return new System.Net.Mail.MailAddress(AppEmailAddress, AppEmailFromName(isAfterSchool));
        }
        public static EmailAddress SendGridAppEmailAddress(bool isAfterSchool)
        {
            return new EmailAddress(Email.AppEmailAddress, Utilities.AppName(isAfterSchool));
        }
        public static EmailAddress SendGridAppAdminEmailAddress
        {
            get
            {
                return new EmailAddress(Email.AppAdminEmailAddress);
            }
        }

        public static string AppSupportEmailAddress(bool isAfterSchool)
        {
            return "support@" + HostName(isAfterSchool);
        }

        public static string AppEmailAddress
        {
            get
            {
                return "noreply@" + SenderHostName;
            }
        }
        public static string AppEmailFromName(bool isAfterSchool)
        {
            return Utilities.AppName(isAfterSchool);
        }
        public static string AppAdminEmailAddress { get; set; }

        public static string AppSignature(bool isAfterSchool)
        {
           return string.Format("<p>{0}<br /><a href=\"{1}\">{1}</a></p>", AppEmailFromName(isAfterSchool), BaseUrl(isAfterSchool));
        }
        public static string GetAppSignature(bool isAfterSchool, string utm_campaign, string utm_source = "enrollhq", string utm_medium = "email")
        {
            return string.Format("<p>Powered by <a href=\"{1}?utm_source={2}&utm_medium={3}&utm_campaign={4}\">{0}</a></p>", AppEmailFromName(isAfterSchool), BaseUrl(isAfterSchool), utm_source, utm_medium, utm_campaign);
        }
        public static string AppReplyAboveLineText
        {
            get
            {
                return "--- Reply ABOVE THIS LINE to add a comment ---";
            }
        }
        public static string AppReplyAboveLineTextFormatted
        {
            get
            {
                return "<p style=\"font-size:9px;\">" + AppReplyAboveLineText + "</p><p></p>";
            }
        }
        public static string ReplyHostName { get; set; }

        public static string SenderHostName { get; set; }
    }



}
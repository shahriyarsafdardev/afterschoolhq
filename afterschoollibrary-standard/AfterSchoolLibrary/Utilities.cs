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

        public static bool IsLiveServer
        {
            get
            {
                if (System.Reflection.Assembly.GetExecutingAssembly().CodeBase.ToLower().Contains("-live"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public static bool IsTestServer
        {
            get
            {
                if (System.Reflection.Assembly.GetExecutingAssembly().CodeBase.ToLower().Contains("-test"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public static bool IsDevServer
        {
            get
            {
                if (System.Reflection.Assembly.GetExecutingAssembly().CodeBase.ToLower().Contains("-dev"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public static bool isLocalHost
        {
            get
            {
                if (!IsLiveServer && !IsTestServer && !IsDevServer)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static string RollbarAccessToken { get { return "9d10f02799c544f5bf4c9e27e0743c54"; } }

        public static string GetRollbarEnvironment()
        {
            if (IsLiveServer)
            {
                return "production";
            }
            else if (IsTestServer)
            {
                return "staging";
            }
            else if (IsDevServer)
            {
                return "ci";
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


        //private static string _AppDocumentRoot;
        //public static string AppDocumentRoot
        //{
        //    get
        //    {
        //        return HttpContext.Current.Request.PhysicalApplicationPath.Substring(0, HttpContext.Current.Request.PhysicalApplicationPath.Trim('\\').LastIndexOf("\\"));
        //    }
        //    set
        //    {

        //    }
        //}

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

        //public static string HostName
        //{
        //    get
        //    {
        //        string rtn = HttpContext.Current.Request.Url.Host;
        //        if (HttpContext.Current.Request.Url.Port != 80 && HttpContext.Current.Request.Url.Port != 443)
        //        {
        //            rtn += ":" + HttpContext.Current.Request.Url.Port.ToString();
        //        }
        //        return rtn.Replace("www.", "");
        //    }
        //}

        public static string HostName(bool isAfterSchool)
        {
            if (isAfterSchool)
                return "afterschoolhq.com";
            return "enrollhq.com";
        }

        public static bool UseSSL { get; set; }

        public static string BaseUrl(bool isAfterSchool)
        {
            return UseSSL && !isLocalHost ? BaseUrlHttps(isAfterSchool) : BaseUrlHttp(isAfterSchool);
        }

        public static string BaseUrlHttp(bool isAfterSchool)
        {
            return "http://" + HostName(isAfterSchool);
        }
        public static string BaseUrlHttps(bool isAfterSchool)
        {
            return "https://" + HostName(isAfterSchool);
        }
        
        public static string AppNameEnrollHQ { get { return "Enroll HQ"; } }
        public static string AppNameAfterSchoolHQ { get { return "After School HQ"; } }
        public static string AppName(bool isAfterSchool)
        {
            if (isAfterSchool)
                return AppNameAfterSchoolHQ;
            return AppNameEnrollHQ;
        }

        public static void InitializeApp()
        {
            Utilities.UseSSL = true; //can put in a if statement depending on if is live, etc.
            // AfterSchoolLibrary.Utilities.AppName = "Enroll HQ"; //used in email signatures, etc
            Utilities.AppAESKey = System.Text.Encoding.UTF8.GetBytes("9dj3nshcnshf73heydsg1nshdyciefh7"); //32 alphanumeric chars - only set once per app, never changed afterwards, required

            Utilities.MasterAPIKey = ""; //at least 45 random alphanumeric characters
            Utilities.MasterAPISecret = ""; //at least 65 random alphanumeric characters

            Utilities.AmazonBucketNameDev = "enrollhq.dev"; //all files are stored in amazon bucket
            Utilities.AmazonBucketNameTest = "enrollhq.test";
            Utilities.AmazonBucketNameLive = "enrollhq.live";
            Utilities.AmazonBucketNameLocal = "enrollhq.local";

            Utilities.AmazonAccessKeyId = "AKIAIRWKZE6AEP57RE2Q";
            Utilities.AmazonSecretAccessKeyId = "j8rbG/g32C2f15U0xLy4os5RNkypQgFxxVqCcS9N";

            Email.AppAdminEmailAddress = "support@enrollhq.com"; //all app admin emails are sent to this email address
            Email.ReplyHostName = "enrollhq.com"; // can be modified based on live/dev/test server
            Email.SenderHostName = "mailer.enrollhq.com"; // can be modified based on live/dev/test server

            Email.HostNameLiveEnrollHQ = "enrollhq.com"; //since the hostname may not be able to be picked up from the actual url that is processing the request
            Email.HostNameTestEnrollHQ = "staging.enrollhq.com";
            Email.HostNameDevEnrollHQ = "dev.enrollhq.com";
            Email.HostNameLocalEnrollHQ = "localhost:54849";

            Email.HostNameLiveAfterSchoolHQ = "afterschoolhq.com"; //since the hostname may not be able to be picked up from the actual url that is processing the request
            Email.HostNameTestAfterSchoolHQ = "staging.afterschoolhq.com";
            Email.HostNameDevAfterSchoolHQ = "dev.afterschoolhq.com";
            Email.HostNameLocalAfterSchoolHQ = "localhost:54849";

            Utilities.ReCaptchaPrivateKey = "6Lfk694SAAAAAH5e3gWv3JckrYzz2scslo_rop6M"; //can be used as standard re-captcha
            Utilities.ReCaptchaPublicKey = "6Lfk694SAAAAAIrtt3Hwiz7j4emDRlYuLp73i9VY";
        }

        public static SystemMessages InitDatabase(string adminEmail = null, string adminPassword = null, bool skipTableCreation = false, string groupName = "First Group")
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
                adminEmail = adminEmail.IfEmpty("darye@reborncode.com");
                adminPassword = adminPassword.IfEmpty("aDm1n1906");

                if (WebUser.LoadByEmail(adminEmail, dc) != null)
                    throw new Exception("User with the same email address already exists.");

                WebUser wu = new WebUser();
                wu.FirstName = "Admin";
                wu.LastName = "User";
                wu.Email = adminEmail;
                wu.Username = "admin";
                wu.Password = WebUser.EncryptPassword(adminPassword);
                wu.IsActive = true;
                wu.CreateDate = wu.LastModifiedDate = DateTime.UtcNow;
                wu.CreatedBy = wu.LastModifiedBy = 0;
                msgs.AddRange(wu.Save(dc));

                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(BasicLibrary.Utilities.SpecialCharsRegEx, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                Account a = new Account();
                a.Name = groupName;
                a.CreateDate = a.LastModifiedDate = DateTime.UtcNow;
                a.CreatedBy = a.LastModifiedBy = 0;
                a.UniqueName = AccountService.GenerateUniqueName(a.Name, a.ID);
                a.IsAdminAccount = true;
                a.IsActive = true;
                msgs.AddRange(a.Save(dc));

                msgs.AddRange(a.AddUser(wu, UserType.Owner, 0, dc));

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
                //msgs.AddRange(AdditionalData(dc));
                //msgs.AddRange(UpdateAccountFeatures(dc));
                //msgs.AddRange(UpdateLocations(dc));
                //msgs.AddRange(UpdateQuestionGroups(dc));
                //msgs.AddRange(UpdateRegistrationModes(dc));
                //msgs.AddRange(UpdateRegistrations(dc));
                //msgs.AddRange(UpdateRegistrants(dc));
                //msgs.AddRange(UpdateAccountProperties(dc));
                //msgs.AddRange(UpdateAnswers(dc));
                //msgs.AddRange(UpdateGradeLevels(dc));
                //// msgs.AddRange(AddBellecChasseCustomQuestions(dc));
                //// msgs.AddRange(UpdateApplicationSeatIndexesRunOnce(dc));
                //msgs.AddRange(UpdateSessionActions(dc));
                //// msgs.AddRange(UpdateActivityGroupsRunOnce(dc));
                //// msgs.AddRange(UpdateActivities(dc));
                //msgs.AddRange(UpdateAccountPackages(dc));
                // msgs.AddRange(UpdateActivitySchedules(dc));
                //msgs.AddRange(UpdateRegistrantActivities(dc));
                //// msgs.AddRange(HandleDuplicateRegistrations(dc));
                //msgs.AddRange(FixMissingMasterRecords(dc));
                //// msgs.AddRange(FixMessedUpPreferences(dc));
                // msgs.AddRange(UpdateRegistrantEmailQuestionGroup(dc));
                // msgs.AddRange(CreateMasterRecord(dc));
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

        public static SystemMessages AdditionalData(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            SystemMessages msgs = new SystemMessages();

            var gradeLevels = GradeLevel.Load();
            if (gradeLevels.Count == 0)
            {
                new GradeLevel() { Name = "Pre-K", Abbreviation = "P" }.Save(dc);
                new GradeLevel() { Name = "Kindergarten", Abbreviation = "K" }.Save(dc);
                new GradeLevel() { Name = "1st", Abbreviation = "1" }.Save(dc);
                new GradeLevel() { Name = "2nd", Abbreviation = "2" }.Save(dc);
                new GradeLevel() { Name = "3rd", Abbreviation = "3" }.Save(dc);
                new GradeLevel() { Name = "4th", Abbreviation = "4" }.Save(dc);
                new GradeLevel() { Name = "5th", Abbreviation = "5" }.Save(dc);
                new GradeLevel() { Name = "6th", Abbreviation = "6" }.Save(dc);
                new GradeLevel() { Name = "7th", Abbreviation = "7" }.Save(dc);
                new GradeLevel() { Name = "8th", Abbreviation = "8" }.Save(dc);
                new GradeLevel() { Name = "9th", Abbreviation = "9" }.Save(dc);
                new GradeLevel() { Name = "10th", Abbreviation = "10" }.Save(dc);
                new GradeLevel() { Name = "11th", Abbreviation = "11" }.Save(dc);
                new GradeLevel() { Name = "12th", Abbreviation = "12" }.Save(dc);
            }

            Form f = Form.LoadByAccountId(1, dc).FirstOrDefault();
            if (f.IsEmpty())
            {
                f = new Form();
                f.AccountId = 1;
                f.LastModifiedDate = f.CreateDate = DateTime.UtcNow;
                f.LastModifiedBy = f.CreatedBy = 0;
                f.Name = "Default Form";
                f.IsActive = true;
                msgs.AddRange(f.Save(dc));

                var questions = new QuestionHolder[]
                {
                new QuestionHolder() { Text = "First Name", IsActive = true, IsRequired = true, QuestionTypeId = QuestionType.TextBox, QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation, GroupName = "Personal Information", IsLocked = true, QuestionDataTypeId = QuestionDataType.RegistrantFirstName },
                new QuestionHolder() { Text = "Last Name", IsActive = true, IsRequired = true, QuestionTypeId = QuestionType.TextBox, QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation, GroupName = "Personal Information", IsLocked = true, QuestionDataTypeId = QuestionDataType.RegistrantLastName },
                new QuestionHolder() { Text = "Email", IsActive = true, IsRequired = true, QuestionTypeId = QuestionType.Email, QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation, GroupName = "Personal Information", IsLocked = true, QuestionDataTypeId = QuestionDataType.RegistrantEmail },
                new QuestionHolder() { Text = "Date of Birth", QuestionTypeId = QuestionType.Date, GroupName = "Personal Information", QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation, QuestionDataTypeId = QuestionDataType.RegistrantDateOfBirth },
                new QuestionHolder() { Text = "Grade", QuestionTypeId = QuestionType.DropDownList, GroupName = "Personal Information", QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation, QuestionDataTypeId = QuestionDataType.RegistrantGrade, Options = "Pre-K,K,1st,2nd,3rd,4th,5th,6th,7th,8th,9th,10th,11th,12th" },
                new QuestionHolder() { Text = "Gender", QuestionTypeId = QuestionType.RadioList, GroupName = "Personal Information", QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation, QuestionDataTypeId = QuestionDataType.RegistrantGender, Options = "Male,Female" },
                new QuestionHolder() { Text = "Ethnicity", QuestionTypeId = QuestionType.DropDownList, GroupName = "Personal Information", QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation, QuestionDataTypeId = QuestionDataType.RegistrantEthnicity, Options = "American Indian/Alaskan Native,Asian/Pacific Islander,Black/African-American,Hispanic/Latino,White,Other" },
                new QuestionHolder() { Text = "Organization", QuestionTypeId = QuestionType.TextBox, GroupName = "Personal Information", QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation, IsLocked = false, QuestionDataTypeId = QuestionDataType.RegistrantOrganization },

                new QuestionHolder() { Text = "Phone", QuestionTypeId = QuestionType.TextBox, GroupName = "Contact Information", QuestionGroupDataTypeId = QuestionGroupDataType.ContactInformation, QuestionDataTypeId = QuestionDataType.RegistrantPhone1 },
                new QuestionHolder() { Text = "Phone Type", QuestionTypeId = QuestionType.RadioList, GroupName = "Contact Information", QuestionGroupDataTypeId = QuestionGroupDataType.ContactInformation, QuestionDataTypeId = QuestionDataType.RegistrantPhoneType1, Options = "Mobile, Home, Work" },
                new QuestionHolder() { Text = "Alternate Phone", QuestionTypeId = QuestionType.TextBox, GroupName = "Contact Information", QuestionGroupDataTypeId = QuestionGroupDataType.ContactInformation, QuestionDataTypeId = QuestionDataType.RegistrantPhone2 },
                new QuestionHolder() { Text = "Alternate Phone Type", QuestionTypeId = QuestionType.RadioList, GroupName = "Contact Information", QuestionGroupDataTypeId = QuestionGroupDataType.ContactInformation, QuestionDataTypeId = QuestionDataType.RegistrantPhoneType2, Options = "Mobile, Home, Work" },

                new QuestionHolder() { Text = "Street Address", QuestionTypeId = QuestionType.TextBox, GroupName = "Home Address", QuestionGroupDataTypeId = QuestionGroupDataType.HomeAddress, QuestionDataTypeId = QuestionDataType.RegistrantAddress1 },
                new QuestionHolder() { Text = "Apt/Suite", QuestionTypeId = QuestionType.TextBox, GroupName = "Home Address", QuestionGroupDataTypeId = QuestionGroupDataType.HomeAddress, QuestionDataTypeId = QuestionDataType.RegistrantAddress2 },
                new QuestionHolder() { Text = "City", QuestionTypeId = QuestionType.TextBox, GroupName = "Home Address", QuestionGroupDataTypeId = QuestionGroupDataType.HomeAddress, QuestionDataTypeId = QuestionDataType.RegistrantCity },
                new QuestionHolder() { Text = "State", QuestionTypeId = QuestionType.DropDownList, GroupName = "Home Address", QuestionGroupDataTypeId = QuestionGroupDataType.HomeAddress, Options = QuestionHolder.StateList, QuestionDataTypeId = QuestionDataType.RegistrantState },
                new QuestionHolder() { Text = "Zip Code", QuestionTypeId = QuestionType.TextBox, GroupName = "Home Address", QuestionGroupDataTypeId = QuestionGroupDataType.HomeAddress, QuestionDataTypeId = QuestionDataType.RegistrantPostalCode },

                new QuestionHolder() { Text = "First Name", QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 1 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1FirstName },
                new QuestionHolder() { Text = "Last Name", QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 1 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1LastName },
                new QuestionHolder() { Text = "Email", QuestionTypeId = QuestionType.Email, GroupName = "Parent 1 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1Email },
                new QuestionHolder() { Text = "Phone", QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 1 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1Phone1 },
                new QuestionHolder() { Text = "Phone Type", QuestionTypeId = QuestionType.RadioList, GroupName = "Parent 1 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1PhoneType1, Options = "Mobile, Home, Work" },
                new QuestionHolder() { Text = "Alternate Phone", QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 1 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1Phone2 },
                new QuestionHolder() { Text = "Alternate Phone Type", QuestionTypeId = QuestionType.RadioList, GroupName = "Parent 1 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1PhoneType2, Options = "Mobile, Home, Work" },
                new QuestionHolder() { Text = "Street Address", QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 1 Address", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1Address1 },
                new QuestionHolder() { Text = "Apt/Suite", QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 1 Address", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1Address2 },
                new QuestionHolder() { Text = "City", QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 1 Address", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1City },
                new QuestionHolder() { Text = "State", QuestionTypeId = QuestionType.DropDownList, GroupName = "Parent 1 Address", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, Options = QuestionHolder.StateList, QuestionDataTypeId = QuestionDataType.Parent1State },
                new QuestionHolder() { Text = "Zip Code", QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 1 Address", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1PostalCode },

                new QuestionHolder() { Text = "First Name", QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 2 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent2Information, QuestionDataTypeId = QuestionDataType.Parent2FirstName },
                new QuestionHolder() { Text = "Last Name", QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 2 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent2Information, QuestionDataTypeId = QuestionDataType.Parent2LastName },
                new QuestionHolder() { Text = "Email", QuestionTypeId = QuestionType.Email, GroupName = "Parent 2 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent2Information, QuestionDataTypeId = QuestionDataType.Parent2Email },
                new QuestionHolder() { Text = "Phone", QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 2 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent2Information, QuestionDataTypeId = QuestionDataType.Parent2Phone1 },
                new QuestionHolder() { Text = "Phone Type", QuestionTypeId = QuestionType.RadioList, GroupName = "Parent 2 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent2Information, QuestionDataTypeId = QuestionDataType.Parent2PhoneType1, Options = "Mobile, Home, Work" },
                new QuestionHolder() { Text = "Alternate Phone", QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 2 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent2Information, QuestionDataTypeId = QuestionDataType.Parent2Phone2 },
                new QuestionHolder() { Text = "Alternate Phone Type", QuestionTypeId = QuestionType.RadioList, GroupName = "Parent 2 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent2Information, QuestionDataTypeId = QuestionDataType.Parent2PhoneType2, Options = "Mobile, Home, Work" },
                new QuestionHolder() { Text = "Street Address", QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 2 Address", QuestionGroupDataTypeId = QuestionGroupDataType.Parent2Address, QuestionDataTypeId = QuestionDataType.Parent2Address1 },
                new QuestionHolder() { Text = "Apt/Suite", QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 2 Address", QuestionGroupDataTypeId = QuestionGroupDataType.Parent2Address, QuestionDataTypeId = QuestionDataType.Parent2Address2 },
                new QuestionHolder() { Text = "City", QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 2 Address", QuestionGroupDataTypeId = QuestionGroupDataType.Parent2Address, QuestionDataTypeId = QuestionDataType.Parent2City },
                new QuestionHolder() { Text = "State", QuestionTypeId = QuestionType.DropDownList, GroupName = "Parent 2 Address", QuestionGroupDataTypeId = QuestionGroupDataType.Parent2Address, Options = QuestionHolder.StateList, QuestionDataTypeId = QuestionDataType.Parent2State },
                new QuestionHolder() { Text = "Zip Code", QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 2 Address", QuestionGroupDataTypeId = QuestionGroupDataType.Parent2Address, QuestionDataTypeId = QuestionDataType.Parent2PostalCode },

                new QuestionHolder() { Text = "First Name", QuestionTypeId = QuestionType.TextBox, GroupName = "Emergency Contact 1", QuestionGroupDataTypeId = QuestionGroupDataType.EmergencyContact1, QuestionDataTypeId = QuestionDataType.EmergencyFirstName1 },
                new QuestionHolder() { Text = "Last Name", QuestionTypeId = QuestionType.TextBox, GroupName = "Emergency Contact 1", QuestionGroupDataTypeId = QuestionGroupDataType.EmergencyContact1, QuestionDataTypeId = QuestionDataType.EmergencyLastName1 },
                new QuestionHolder() { Text = "Phone", QuestionTypeId = QuestionType.TextBox, GroupName = "Emergency Contact 1", QuestionGroupDataTypeId = QuestionGroupDataType.EmergencyContact1, QuestionDataTypeId = QuestionDataType.EmergencyPhone1 },

                new QuestionHolder() { Text = "First Name", QuestionTypeId = QuestionType.TextBox, GroupName = "Emergency Contact 2", QuestionGroupDataTypeId = QuestionGroupDataType.EmergencyContact2, QuestionDataTypeId = QuestionDataType.EmergencyFirstName2 },
                new QuestionHolder() { Text = "Last Name", QuestionTypeId = QuestionType.TextBox, GroupName = "Emergency Contact 2", QuestionGroupDataTypeId = QuestionGroupDataType.EmergencyContact2, QuestionDataTypeId = QuestionDataType.EmergencyLastName2 },
                new QuestionHolder() { Text = "Phone", QuestionTypeId = QuestionType.TextBox, GroupName = "Emergency Contact 2", QuestionGroupDataTypeId = QuestionGroupDataType.EmergencyContact2, QuestionDataTypeId = QuestionDataType.EmergencyPhone2 },

                new QuestionHolder() { Text = "Doctor Name", QuestionTypeId = QuestionType.TextBox, GroupName = "Health Information", QuestionGroupDataTypeId = QuestionGroupDataType.HealthInformation, QuestionDataTypeId = QuestionDataType.RegistrantDoctorName },
                new QuestionHolder() { Text = "Doctor Phone", QuestionTypeId = QuestionType.TextBox, GroupName = "Health Information", QuestionGroupDataTypeId = QuestionGroupDataType.HealthInformation, QuestionDataTypeId = QuestionDataType.RegistrantDoctorPhone },
                new QuestionHolder() { Text = "Preferred Hospital (in case of emergency)", QuestionTypeId = QuestionType.TextBox, GroupName = "Health Information", QuestionGroupDataTypeId = QuestionGroupDataType.HealthInformation, QuestionDataTypeId = QuestionDataType.RegistrantPreferredHospital },
                new QuestionHolder() { Text = "Allergies", QuestionTypeId = QuestionType.TextArea, GroupName = "Health Information", QuestionGroupDataTypeId = QuestionGroupDataType.HealthInformation, QuestionDataTypeId = QuestionDataType.RegistrantAllergies },
                new QuestionHolder() { Text = "Medical Conditions", QuestionTypeId = QuestionType.TextArea, GroupName = "Health Information", QuestionGroupDataTypeId = QuestionGroupDataType.HealthInformation, QuestionDataTypeId = QuestionDataType.RegistrantMedicalConditions },
                new QuestionHolder() { Text = "Are you up to date on Immunizations?", QuestionTypeId = QuestionType.RadioList, GroupName = "Health Information", QuestionGroupDataTypeId = QuestionGroupDataType.HealthInformation, QuestionDataTypeId = QuestionDataType.RegistrantImmunizations, Options = "Yes,No,Not Sure" },

                //organization
                new QuestionHolder() { Text = "Organization Name", IsActive = true, IsRequired = true, IsLocked = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Organization Information", QuestionGroupDataTypeId = QuestionGroupDataType.OrganizationInformation, QuestionDataTypeId = QuestionDataType.OrganizationName, RegistrantTypeId = RegistrantType.Organization },
                new QuestionHolder() { Text = "Street Address", IsActive = true, IsRequired = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Organization Information", QuestionGroupDataTypeId = QuestionGroupDataType.OrganizationInformation, QuestionDataTypeId = QuestionDataType.OrganizationAddress1, RegistrantTypeId = RegistrantType.Organization },
                new QuestionHolder() { Text = "Suite", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Organization Information", QuestionGroupDataTypeId = QuestionGroupDataType.OrganizationInformation, QuestionDataTypeId = QuestionDataType.OrganizationAddress2, RegistrantTypeId = RegistrantType.Organization },
                new QuestionHolder() { Text = "City", IsActive = true, IsRequired = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Organization Information", QuestionGroupDataTypeId = QuestionGroupDataType.OrganizationInformation, QuestionDataTypeId = QuestionDataType.OrganizationCity, RegistrantTypeId = RegistrantType.Organization },
                new QuestionHolder() { Text = "State", IsActive = true, IsRequired = true, QuestionTypeId = QuestionType.DropDownList, GroupName = "Organization Information", QuestionGroupDataTypeId = QuestionGroupDataType.OrganizationInformation, Options = QuestionHolder.StateList, QuestionDataTypeId = QuestionDataType.OrganizationState, RegistrantTypeId = RegistrantType.Organization },
                new QuestionHolder() { Text = "Zip Code", IsActive = true, IsRequired = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Organization Information", QuestionGroupDataTypeId = QuestionGroupDataType.OrganizationInformation, QuestionDataTypeId = QuestionDataType.OrganizationPostalCode, RegistrantTypeId = RegistrantType.Organization },

                new QuestionHolder() { Text = "First Name", IsActive = true, IsRequired = true, IsLocked = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Organization Contact", QuestionGroupDataTypeId = QuestionGroupDataType.ContactInformation, QuestionDataTypeId = QuestionDataType.OrganizationContactFirstName, RegistrantTypeId = RegistrantType.Organization },
                new QuestionHolder() { Text = "Last Name", IsActive = true, IsRequired = true, IsLocked = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Organization Contact", QuestionGroupDataTypeId = QuestionGroupDataType.ContactInformation, QuestionDataTypeId = QuestionDataType.OrganizationContactLastName, RegistrantTypeId = RegistrantType.Organization },
                new QuestionHolder() { Text = "Email", IsActive = true, IsRequired = true, IsLocked = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Organization Contact", QuestionGroupDataTypeId = QuestionGroupDataType.ContactInformation, QuestionDataTypeId = QuestionDataType.OrganizationContactEmail, RegistrantTypeId = RegistrantType.Organization },
                new QuestionHolder() { Text = "Phone", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Organization Contact", QuestionGroupDataTypeId = QuestionGroupDataType.ContactInformation, QuestionDataTypeId = QuestionDataType.OrganizationContactPhone, RegistrantTypeId = RegistrantType.Organization },
                new QuestionHolder() { Text = "Phone Type", IsActive = true, QuestionTypeId = QuestionType.RadioList, GroupName = "Organization Contact", QuestionGroupDataTypeId = QuestionGroupDataType.ContactInformation, QuestionDataTypeId = QuestionDataType.OrganizationContactPhoneType, Options = "Mobile, Home, Work", RegistrantTypeId = RegistrantType.Organization },
                };

                int displayOrder = 0;
                int groupDisplayOrder = 0;
                foreach (var question in questions)
                {
                    FormQuestionGroup group = FormQuestionGroup.Load(
                        criterias: new Hashtable() { { "Name", question.GroupName }, { "FormId", f.ID.Value } },
                        dc: dc).FirstOrDefault();

                    if (group.IsEmpty())
                    {
                        group = new FormQuestionGroup();
                        group.Name = question.GroupName;
                        group.FormId = f.ID;
                        group.DisplayOrder = groupDisplayOrder;
                        group.RegistrantTypeId = question.RegistrantTypeId;
                        group.LastModifiedDate = group.CreateDate = DateTime.UtcNow;
                        group.LastModifiedBy = group.CreatedBy = 0;
                        msgs.AddRange(group.Save(dc));

                        groupDisplayOrder++;
                    }

                    FormQuestion q = FormQuestion.LoadByFormQuestionGroupIdByQuestionDataTypeId(group.ID.Value, question.QuestionDataTypeId, dc);
                    if (q.IsEmpty())
                    {
                        q = new FormQuestion();
                        q.Text = question.Text;
                        q.IsActive = question.IsActive;
                        q.IsRequired = question.IsRequired;
                        q.FormId = f.ID;
                        q.FormQuestionGroupId = group.ID;
                        q.IsTemplate = true;
                        q.IsLocked = question.IsLocked;
                        q.DisplayOrder = displayOrder;
                        q.QuestionTypeId = question.QuestionTypeId;
                        q.QuestionDataTypeId = question.QuestionDataTypeId;
                        q.Options = question.Options;
                        q.LastModifiedDate = q.CreateDate = DateTime.UtcNow;
                        q.LastModifiedBy = q.CreatedBy = 0;
                        msgs.AddRange(q.Save(dc));

                        displayOrder++;
                    }
                }
            }
            else
            {
                var forms = Form.Load();
                foreach (var form in forms)
                {
                    var passed = 0;
                    //make modifications
                    var groups = form.GetQuestionGroups(dc);
                    foreach (var group in groups)
                    {
                        if (passed > 1)
                        {
                            group.DisplayOrder = group.DisplayOrder + 2;
                            msgs.AddRange(group.Save(dc));
                        }
                        else if (passed > 0)
                        {
                            group.DisplayOrder = group.DisplayOrder + 1;
                            msgs.AddRange(group.Save(dc));
                        }

                        if (group.Name == "Parent 1 Information" || group.Name == "Parent/Guardian 1")
                        {
                            var hasGroup = FormQuestionGroup.Load(
                                new Hashtable() {
                                    { "Name", "Parent 1 Address" },
                                    { "FormId", form.ID.Value }
                                },
                                null,
                                null,
                                dc);

                            if (hasGroup.Count == 0)
                            {
                                var newGroup = new FormQuestionGroup();
                                newGroup.Name = "Parent 1 Address";
                                newGroup.FormId = form.ID;
                                newGroup.DisplayOrder = group.DisplayOrder + 1;
                                newGroup.RegistrantTypeId = RegistrantType.Individual;
                                newGroup.LastModifiedDate = newGroup.CreateDate = DateTime.UtcNow;
                                newGroup.LastModifiedBy = newGroup.CreatedBy = 0;
                                msgs.AddRange(newGroup.Save(dc));

                                var questions = group.GetAllQuestions(dc);
                                var i = 0;
                                foreach (var question in questions)
                                {
                                    var datatypes = new QuestionDataType[] { QuestionDataType.Parent1Address1, QuestionDataType.Parent1Address2, QuestionDataType.Parent1City, QuestionDataType.Parent1State, QuestionDataType.Parent1PostalCode };

                                    if (datatypes.Contains(question.QuestionDataTypeId))
                                    {
                                        question.FormQuestionGroupId = newGroup.ID;
                                        question.DisplayOrder = i;
                                        msgs.AddRange(question.Save(dc));
                                        i++;
                                    }

                                }
                            }
                            passed++;
                        }
                        else if (group.Name == "Parent 2 Information" || group.Name == "Parent/Guardian 2")
                        {
                            var hasGroup = FormQuestionGroup.Load(
                                   new Hashtable() {
                                    { "Name", "Parent 2 Address" },
                                    { "FormId", form.ID.Value }
                                   },
                                   null,
                                   null,
                                   dc);

                            if (hasGroup.Count == 0)
                            {
                                var newGroup = new FormQuestionGroup();
                                newGroup.Name = "Parent 2 Address";
                                newGroup.FormId = form.ID;
                                newGroup.DisplayOrder = group.DisplayOrder + 1;
                                newGroup.RegistrantTypeId = RegistrantType.Individual;
                                newGroup.LastModifiedDate = newGroup.CreateDate = DateTime.UtcNow;
                                newGroup.LastModifiedBy = newGroup.CreatedBy = 0;
                                msgs.AddRange(newGroup.Save(dc));

                                var questions = group.GetAllQuestions(dc);
                                var i = 0;
                                foreach (var question in questions)
                                {
                                    var datatypes = new QuestionDataType[] { QuestionDataType.Parent2Address1, QuestionDataType.Parent2Address2, QuestionDataType.Parent2City, QuestionDataType.Parent2State, QuestionDataType.Parent2PostalCode };

                                    if (datatypes.Contains(question.QuestionDataTypeId))
                                    {
                                        question.FormQuestionGroupId = newGroup.ID;
                                        question.DisplayOrder = i;
                                        msgs.AddRange(question.Save(dc));
                                        i++;
                                    }

                                }
                            }
                            passed++;
                        }

                    }

                }

            }

            return msgs;
        }

        public static SystemMessages UpdateAccountFeatures(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var aps = AccountPackage.Load(dc: dc);
            foreach (var ap in aps)
            {
                msgs.AddRange(ap.ToPackage().CopyFeaturesToAccountPackage(ap, 0, dc));
            }

            return msgs;
        }

        public static SystemMessages UpdateLocations(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var accounts = Account.Load(dc: dc);
            foreach (var account in accounts)
            {
                var locations = Location.LoadByAccountId(account.ID.Value, dc);
                if (locations.Count == 0)
                {
                    var location = new Location();
                    location.IsDefault = true;
                    location.AccountId = account.ID.Value;
                    location.Address1 = account.Address1;
                    location.Address2 = account.Address2;
                    location.City = account.City;
                    location.State = account.State;
                    location.PostalCode = account.PostalCode;
                    msgs.AddRange(location.Save(dc));
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
                    msgs.AddRange(questionGroup.Save(dc));
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
                var ra = RegistrantActivity.LoadByActivityIdByRegistrantId(answer.ActivityId.Value, answer.RegistrantId.Value, dc);
                if (ra.IsNotEmpty() && ra.RegistrationId.HasValue)
                {
                    answer.RegistrationId = ra.RegistrationId;
                    answer.QuestionDataTypeId = answer.FormQuestionQuestionDataTypeId;
                    msgs.AddRange(answer.Save(dc));
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
                    msgs.AddRange(registration.AddRelatedRegistration(match, 0, dc));
                    registrations.Remove(match);
                }

                registrations.Remove(registration);
            }

            /*
            // RAN and is done now, no need to run again
            var registrants = Registrant.Load(criterias: new Hashtable() { { "IsContactOnly", false } },dc: dc);
            foreach (var registrant in registrants)
            {
                var registrationCount = registrant.GetRegistrationCount(dc);
                var activities = registrant.GetActivities(dc);
                if (registrationCount == 0 && activities.Any())
                {
                    var registration = registrant.AddRegistration(0, registrant.AccountId, null, msgs, dc);
                    registration.CreateDate = registration.LastModifiedDate = registrant.CreateDate;
                    msgs.AddRange(registration.Save(dc));

                    foreach (var activity in activities)
                    {
                        msgs.AddRange(registration.AddActivity(activity, 0, dc));

                        var regactivity = RegistrantActivity.LoadByActivityIdByRegistrantId(activity.ID.Value, registrant.ID.Value, dc);
                        if (regactivity.IsNotEmpty())
                        {
                            regactivity.RegistrationId = registration.ID.Value;
                            msgs.AddRange(regactivity.Save(dc));

                            registration.StripeChargeId = regactivity.StripeChargeId;
                            registration.PaymentTypeId = regactivity.PaymentTypeId;
                            registration.PayPalTransactionId = regactivity.PayPalTransactionId;
                            registration.DatePaid = regactivity.DatePaid;
                            registration.AmountPaid = regactivity.AmountPaid;
                            msgs.AddRange(registration.Save(dc));

                            var answers = regactivity.GetAnswers(dc);
                            foreach (var answer in answers)
                            {
                                answer.RegistrationId = registration.ID;
                                answer.QuestionDataTypeId = answer.FormQuestionQuestionDataTypeId;
                                msgs.AddRange(answer.Save(dc));
                            }
                        }
                    }
                }
            }

            var registrations = Registration.Load(criterias: new Hashtable() { { "LastUpdated", DBNull.Value } }, dc: dc);
            foreach(var reg in registrations)
            {
                var ras = reg.GetRegistrantActivities(dc);
                if(ras.Any())
                {
                    var maxdate = ras.Max(x => x.LastModifiedDate.Value);
                    reg.LastUpdated = maxdate;
                }
                else
                {
                    reg.LastUpdated = reg.LastModifiedDate;
                }

                msgs.AddRange(reg.Save(dc));
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
                    msgs.AddRange(registrant.AddRelatedRegistrant(match, 0, dc));
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
                var properties = AccountProperty.LoadByAccountId(account.ID.Value, dc);
                if (properties.Count == 0 && account.LandingPageText.IsNotBlank())
                {
                    var property = new AccountProperty();
                    property.AccountId = account.ID;
                    property.AccountPropertyTypeId = AccountPropertyType.LandingPageText;
                    property.Value = account.LandingPageText;
                    property.CreateDate = property.LastModifiedDate = DateTime.UtcNow;
                    property.CreatedBy = property.LastModifiedBy = 0;
                    msgs.AddRange(property.Save(dc));
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
                msgs.AddRange(activity.Save(dc));
            }

            activities = Activity.Load(criterias: new Hashtable() { { "RegistrationModeId", DBNull.Value } }, dc: dc);
            foreach (var activity in activities)
            {
                activity.RegistrationModeId = RegistrationMode.Register;
                msgs.AddRange(activity.Save(dc));
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
                if (activity.GetGradeLevels(dc).Count == 0)
                {
                    for (var i = activity.GradeLevelStart.Value; i <= activity.GradeLevelEnd.Value; i++)
                    {
                        var grade = GradeLevel.Load(i, dc);
                        if (grade.IsNotEmpty())
                        {
                            activity.AddGrade(grade.ID.Value, activity.LastModifiedBy.Value, msgs, dc);
                        }
                    }
                }
            }

            return msgs;
        }

        public static SystemMessages AddBellecChasseCustomQuestions(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            var questions = new QuestionHolder[]
            {
                new QuestionHolder()
                {
                    GroupName = "Personal Information",
                    IsActive = true,
                    IsLocked = false,
                    IsRequired = false,
                    Options = "Yes,No",
                    QuestionDataTypeId = QuestionDataType.NotSet,
                    QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation,
                    QuestionTypeId = QuestionType.RadioList,
                    RegistrantTypeId = RegistrantType.Individual,
                    Text = "Individualized Education Plan (IEP)"
                },
                new QuestionHolder()
                {
                    GroupName = "Personal Information",
                    IsActive = true,
                    IsLocked = false,
                    IsRequired = false,
                    Options = "Teacher 1, Teacher 2",
                    QuestionDataTypeId = QuestionDataType.NotSet,
                    QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation,
                    QuestionTypeId = QuestionType.DropDownList,
                    RegistrantTypeId = RegistrantType.Individual,
                    Text = "Homeroom Teacher"
                },
                new QuestionHolder()
                {
                    GroupName = "Dismissal Information",
                    IsActive = true,
                    IsLocked = false,
                    IsRequired = false,
                    Options = "Monday,Tuesday, Wednesday,Thursday;Bus,Walker,Car Rider",
                    QuestionDataTypeId = QuestionDataType.NotSet,
                    QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
                    QuestionTypeId = QuestionType.RadioMatrix,
                    RegistrantTypeId = RegistrantType.Individual,
                    Text = "Dismissal"
                },
                new QuestionHolder()
                {
                    GroupName = "Dismissal Information",
                    IsActive = true,
                    IsLocked = false,
                    IsRequired = false,
                    Options = "Yes,No",
                    QuestionDataTypeId = QuestionDataType.NotSet,
                    QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
                    QuestionTypeId = QuestionType.RadioList,
                    RegistrantTypeId = RegistrantType.Individual,
                    Text = "Does your child's transportation arrangements change during inclement weather?"
                },
                new QuestionHolder()
                {
                    GroupName = "Dismissal Information",
                    IsActive = true,
                    IsLocked = false,
                    IsRequired = false,
                    Options = "Monday,Tuesday, Wednesday,Thursday;Bus,Walker,Car Rider",
                    QuestionDataTypeId = QuestionDataType.NotSet,
                    QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
                    QuestionTypeId = QuestionType.RadioMatrix,
                    RegistrantTypeId = RegistrantType.Individual,
                    Text = "Dismissal Rainy Day"
                },
                new QuestionHolder()
                {
                    GroupName = "Dismissal Information",
                    IsActive = true,
                    IsLocked = false,
                    IsRequired = false,
                    Options = "N/A,1A,1,2,3,4,5,6,7,8",
                    QuestionDataTypeId = QuestionDataType.NotSet,
                    QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
                    QuestionTypeId = QuestionType.DropDownList,
                    RegistrantTypeId = RegistrantType.Individual,
                    Text = "Bus Number"
                },
                new QuestionHolder()
                {
                    GroupName = "Terms & Conditions",
                    IsActive = true,
                    IsLocked = false,
                    IsRequired = false,
                    Options = "",
                    QuestionDataTypeId = QuestionDataType.NotSet,
                    QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
                    QuestionTypeId = QuestionType.Checkbox,
                    RegistrantTypeId = RegistrantType.Individual,
                    Text = "I understand the policy and procedures for being enrolled in a 21st CCLC activity",
                    Link = "https://rebornprojects.com/file/download/fcf9ba11-79c0-472d-a4cb-bdd67c2bf450/21ST-CCLC-Attendance-Behavior-Policy-2016-2017.docx"
                }
            };

            var accountId = 18;
            if (Utilities.IsLiveServer || Utilities.IsTestServer)
            {
                accountId = 6;
            }

            var forms = Form.Load(criterias: new Hashtable() { { "AccountId", accountId } }, dc: dc);

            foreach (var f in forms)
            {
                var account = Account.Load(f.AccountId.Value, dc);

                foreach (var question in questions)
                {
                    FormQuestionGroup group = FormQuestionGroup.Load(
                        criterias: new Hashtable() { { "Name", question.GroupName }, { "FormId", f.ID.Value } },
                        dc: dc).FirstOrDefault();

                    if (group.IsEmpty())
                    {
                        int groupDisplayOrder = f.GetQuestionGroups(dc).Where(x => x.RegistrantTypeId == RegistrantType.Individual).OrderBy(x => x.DisplayOrder).Last().DisplayOrder.Value + 1;
                        group = new FormQuestionGroup();
                        group.Name = question.GroupName;
                        group.FormId = f.ID;
                        group.DisplayOrder = groupDisplayOrder;
                        group.RegistrantTypeId = question.RegistrantTypeId;
                        group.LastModifiedDate = group.CreateDate = DateTime.UtcNow;
                        group.LastModifiedBy = group.CreatedBy = 0;
                        msgs.AddRange(group.Save(dc));
                    }

                    FormQuestion q = FormQuestion.Load(criterias: new Hashtable() { { "Text", question.Text }, { "FormId", f.ID.Value } }, dc: dc).FirstOrDefault();
                    if (q.IsEmpty())
                    {
                        int displayOrder = 0;
                        if (group.GetAllQuestions(dc).Any())
                        {
                            displayOrder = group.GetAllQuestions(dc).OrderBy(x => x.DisplayOrder).Last().DisplayOrder.Value + 1;
                        }
                        q = new FormQuestion();
                        q.Text = question.Text;
                        q.IsActive = question.IsActive;
                        q.IsRequired = question.IsRequired;
                        q.FormId = f.ID;
                        q.FormQuestionGroupId = group.ID;
                        q.IsTemplate = account.DefaultFormId == f.ID;
                        q.IsLocked = question.IsLocked;
                        q.DisplayOrder = displayOrder;
                        q.QuestionTypeId = question.QuestionTypeId;
                        q.QuestionDataTypeId = question.QuestionDataTypeId;
                        q.Options = question.Options;
                        q.Link = question.Link;
                        q.LastModifiedDate = q.CreateDate = DateTime.UtcNow;
                        q.LastModifiedBy = q.CreatedBy = 0;
                        msgs.AddRange(q.Save(dc));
                    }
                }
            }

            return msgs;
        }

        public static SystemMessages UpdateApplicationSeatIndexesRunOnce(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var ras = RegistrantActivity.Load(dc: dc);

            var activityIds = ras.Select(x => x.ActivityId.Value).ToList();

            var accountId = 18;
            if (Utilities.IsLiveServer || Utilities.IsTestServer)
            {
                accountId = 6;
            }

            foreach (var id in activityIds)
            {
                var activity = Activity.Load(id, dc);
                var activityRas = new List<RegistrantActivity>();
                if (activity.GetDays().Count == 0)
                {
                    var index = 0;
                    activityRas = ras.Where(x => x.ActivityId.Value == id && x.NumberOfApplicationSeats.HasValue && x.ActivityAccountId == accountId).OrderBy(x => x.CreateDate.Value).ToList();
                    foreach (var ra in activityRas)
                    {
                        index++;
                        ra.ApplicationSeatIndex = index;
                        msgs.AddRange(ra.Save(dc));
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
                            msgs.AddRange(ra.Save(dc));
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
                var activity = Activity.Load(action.ActivityId.Value, dc);
                if (activity.IsNotEmpty())
                {
                    var regperiod = activity.GetActiveRegistrationPeriod(dc);
                    if (regperiod.IsNotEmpty())
                    {
                        action.RegistrationPeriodId = regperiod.ID.Value;
                        msgs.AddRange(action.Save(dc));
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
                    group.UniqueName = ActivityGroup.GenerateUniqueName(group.AccountId.Value, group.Name, dc);
                    msgs.AddRange(group.Save(dc));

                    period.ActivityGroupId = group.ID;
                    msgs.AddRange(period.Save(dc));

                    var activities = period.GetActivities(dc);
                    foreach(var activity in activities)
                    {
                        activity.ActivityGroupId = group.ID;
                        msgs.AddRange(activity.Save(dc));
                    }

                    var registrations = period.GetRegistrations(dc: dc);
                    foreach (var registration in registrations)
                    {
                        registration.ActivityGroupId = group.ID;
                        msgs.AddRange(registration.Save(dc));
                    }

                    var ras = period.GetRegistrantActivities(dc: dc);
                    foreach (var ra in ras)
                    {
                        ra.ActivityGroupId = group.ID;
                        msgs.AddRange(ra.Save(dc));
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
                var form = activity.GetActiveActivityForm(dc);
                if (form.IsNotEmpty())
                {
                    activity.IsPublished = form.IsPublished;
                    msgs.AddRange(activity.Save(dc));
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
                msgs.AddRange(accountPackage.Save(dc));
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
                schedule.Name = ActivitySchedule.GenerateUniqueName(activity.AccountId.Value, activity.Name, dc);
                schedule.ScheduleDetails = activity.ScheduleDetails.IfBlank("").MaxChars(4000);
                schedule.AccountId = activity.AccountId;
                schedule.CreateDate = schedule.LastModifiedDate = DateTime.UtcNow;
                schedule.CreatedBy = schedule.LastModifiedBy = activity.LastModifiedBy;
                msgs.AddRange(schedule.Save(dc));

                activity.ActivityScheduleId = schedule.ID;
                msgs.AddRange(activity.Save(dc));
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
                    msgs.AddRange(item.Save(dc));
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
            var registrations = Registration.LoadByActivityGroupId(175, dc);

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
                    var lastRegistrant = lastRegistration.GetRegistrant(dc);

                    var regActivities = new List<RegistrantActivity>();
                    var registrationActivities = new List<RegistrationActivity>();
                    var distinctActivityIds = new List<int>();
                    var finalActivityIds = new List<int>();
                    var allRegistrants = allRelated.Select(x => x.GetRegistrant(dc)).ToList();
                    var studentGrades = allRegistrants.Select(x => x.GetAnswer(QuestionDataType.RegistrantGrade, dc)).DistinctBy(x => x.Text).ToList();

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
                            regActivities.AddRange(item.GetRegistrantActivities(dc));
                            registrationActivities.AddRange(item.GetRegistrationActivities(dc));
                        }

                        distinctActivityIds = regActivities.Select(x => x.ActivityId.Value).Distinct().ToList();

                        // check for activities where the grade level of the student is in the list of grades accepted by the activity
                        // if doesn't contain grade level then it will need to be removed
                        foreach (var activityId in distinctActivityIds)
                        {
                            var activity = Activity.Load(activityId, dc);
                            var activityGrades = activity.GetGradeLevels(dc).Select(x => x.Name.Trim().ToLower()).ToList();

                            if (activityGrades.Contains(studentGrades.First().Text.Trim().ToLower()))
                            {
                                finalActivityIds.Add(activityId);
                            }
                        }

                        // keep last registration
                        foreach (var activityId in finalActivityIds)
                        {
                            var activity = Activity.Load(activityId, dc);
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
                                msgs.AddRange(regActivityToMove.Save(dc));

                                // move the matching registration
                                // TODO: probably should cross reference the original registrationId of the regactivity to move with this one that is found
                                var registrationActivityToMove = registrationActivities.Where(x => x.ActivityId == activityId).OrderBy(x => x.CreateDate).FirstOrDefault();
                                registrationActivityToMove.RegistrationId = lastRegistration.ID;
                                msgs.AddRange(registrationActivityToMove.Save(dc));
                            }
                        }

                        // remove activities not in the grade level from the last registration and registrant
                        var activityIdsToRemove = distinctActivityIds.Where(x => !finalActivityIds.Contains(x)).ToList();
                        foreach (var activityId in activityIdsToRemove)
                        {
                            // removes it from the registrant as well
                            msgs.AddRange(lastRegistration.RemoveActivity(activityId, dc));
                        }

                        // Remove all activities from other registrations and registrants
                        // except for the last one registrant and registration
                        foreach (var item in allRelated.Where(x => x.ID != lastRegistration.ID))
                        {
                            foreach (var activityId in regActivities.Where(x => x.RegistrationId == item.ID).Select(x => x.ActivityId.Value))
                            {
                                // removes it from the registrant as well
                                msgs.AddRange(item.RemoveActivity(activityId, dc));
                            }
                        }

                        // now delete the other registrants and registrations
                        foreach (var item in allRelated.Where(x => x.ID != lastRegistration.ID))
                        {
                            var registrant = item.GetRegistrant(dc);
                            msgs.AddRange(registrant.Delete(dc));
                            msgs.AddRange(item.Delete(dc));

                            if (lastRegistration.FirstRegistrationId == item.ID)
                            {
                                lastRegistration.FirstRegistrationId = null;
                                msgs.AddRange(lastRegistration.Save(dc));
                            }
                            if (lastRegistrant.FirstRegistrantId == registrant.ID)
                            {
                                lastRegistrant.FirstRegistrantId = null;
                                msgs.AddRange(lastRegistrant.Save(dc));
                            }
                        }

                        var newRegistrantActivities = lastRegistration.GetRegistrantActivities(dc).OrderBy(x => x.PreferenceNumber).ThenBy(x => x.CreateDate);
                        // update preference order
                        int i = 1;
                        foreach(var ra in newRegistrantActivities)
                        {
                            ra.PreferenceNumber = i;
                            msgs.AddRange(ra.Save(dc));
                            i++;
                        }

                        // check registrations and activities
                        var newRelatedRegs = lastRegistration.GetRelatedRegistrations(dc).Where(x => x.ActivityGroupId == 175).ToList();
                        var newRegistrationActivityIds = lastRegistration.GetRegistrationActivities(dc).Select(x => x.ActivityId.Value).ToList();
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
            var registrant = Registrant.Load(5735, dc);
            var questionIds = registrant.GetAnswers(dc).Select(x => x.FormQuestionId.Value).ToList();
            var questions = FormQuestion.Load(questionIds, dc);
            registrant.CreateMasterCopy(questions, 0, msgs, dc);
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
                var user = WebUser.Load(registrant.WebUserId.Value, dc);
                if (user.Password.IsNotEmpty() && !user.GetAccounts(dc).Any())
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
                        msgs.AddRange(registrant.Save(dc));
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
                var user = WebUser.Load(registrant.WebUserId.Value, dc);
                if (user.Password.IsNotEmpty())
                {
                    var accounts = user.GetAccounts(dc);
                    foreach(var account in accounts)
                    {
                        msgs.AddRange(account.Delete(dc));
                    }
                    var awus = AccountWebUser.LoadByWebUserId(user.ID.Value, dc);
                    foreach (var awu in awus)
                    {
                        msgs.AddRange(awu.Delete(dc));
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
                        msgs.AddRange(registrant.Save(dc));
                    }
                    else
                    {
                        var answers = registrant.GetAnswers(dc);
                        var questions = FormQuestion.Load(answers.Select(x => x.FormQuestionId.Value).ToList(), dc);
                        var master = registrant.CreateMasterCopy(questions, user.ID.Value, msgs, dc);
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
                var currentGroup = question.GetFormQuestionGroup(dc);
                var newGroup = FormQuestionGroup.LoadByFormId(question.FormId.Value, dc).Find(x => x.QuestionGroupDataTypeId == QuestionGroupDataType.ContactInformation);

                if (currentGroup.IsNotEmpty() && newGroup.IsNotEmpty())
                {
                    msgs.AddRange(currentGroup.RemoveQuestion(question, dc));
                    msgs.AddRange(newGroup.AddQuestion(question, 0, dc));
                }
            }

            return msgs;
        }

        private class QuestionHolder
        {
            public string Text;
            public bool IsLocked;
            public bool IsActive;
            public bool IsRequired;
            public QuestionType QuestionTypeId;
            public string GroupName;
            public string Options;
            public QuestionDataType QuestionDataTypeId;
            public QuestionGroupDataType QuestionGroupDataTypeId;
            public RegistrantType RegistrantTypeId;
            public string Link;

            public static string StateList
            {
                get
                {
                    return BasicLibrary.Utilities.GetStateList();
                }
            }

            public QuestionHolder()
            {
                RegistrantTypeId = RegistrantType.Individual;
            }
        }

        public static string AmazonBucketNameLive { get; set; }
        public static string AmazonBucketNameTest { get; set; }
        public static string AmazonBucketNameDev { get; set; }
        public static string AmazonBucketNameLocal { get; set; }

        public static string AmazonBucketName
        {
            get
            {
                if (Utilities.IsLiveServer)
                {
                    return AmazonBucketNameLive;
                }
                else if (Utilities.IsTestServer)
                {
                    return AmazonBucketNameTest;
                }
                else if (Utilities.IsDevServer)
                {
                    return AmazonBucketNameDev;
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
        public static string HostNameLiveEnrollHQ = "";
        public static string HostNameLiveAfterSchoolHQ = "";
        public static string HostNameTestEnrollHQ = "";
        public static string HostNameTestAfterSchoolHQ = "";
        public static string HostNameDevEnrollHQ = "";
        public static string HostNameDevAfterSchoolHQ = "";
        public static string HostNameLocalEnrollHQ = "";
        public static string HostNameLocalAfterSchoolHQ = "";

        public static string HostName(bool isAfterSchool)
        {            
            if (Utilities.IsLiveServer)
            {
                if (isAfterSchool)
                    return HostNameLiveAfterSchoolHQ;
                return HostNameLiveEnrollHQ;
            }
            else if (Utilities.IsTestServer)
            {
                if (isAfterSchool)
                    return HostNameTestAfterSchoolHQ;
                return HostNameTestEnrollHQ;
            }
            else if (Utilities.IsDevServer)
            {
                if (isAfterSchool)
                    return HostNameDevAfterSchoolHQ;
                return HostNameDevEnrollHQ;
            }
            else
            {
                if (isAfterSchool)
                    return HostNameLocalAfterSchoolHQ;
                return HostNameLocalEnrollHQ;
            }
        }

        public static string BaseUrl(bool isAfterSchool)
        {
            return "http" + (Utilities.UseSSL ? "s" : "") + "://" + HostName(isAfterSchool) + "/";
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
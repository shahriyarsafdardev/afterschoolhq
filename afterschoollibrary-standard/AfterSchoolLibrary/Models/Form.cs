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
    /// A registration form object. It contains multiple Form Question Groups.
    /// </summary>
	
    public class Form : BasicLookUpObject
    {
		#region "ClassGenerator"

		/// <summary>
		/// Required
		/// </summary>
		public int? AccountId { get { return (int?)MDV["AccountId"].Value; } set { MDV["AccountId"].Value = value; } }
		/// <summary>
		/// Required
		/// </summary>
		public Guid Uid { get { return (Guid)MDV["Uid"].Value; } set { MDV["Uid"].Value = value; } }
		public bool IsActive { get { return (bool)MDV["IsActive"].Value; } set { MDV["IsActive"].Value = value; } }
		public string AccountName { get { return (string)MDV["AccountName"].Value; } }
		

        public Form()
        {
            this.Config(ObjectMap, DatabaseQuery, typeof(Form).Name, "ID");
            this.LoadObjectValues(ObjectMap);
			this.Uid = Guid.NewGuid();
			this.IsActive = false;
			
        }

        public static BasicLibrary.DataAccess.DatabaseParameters ObjectMap
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseParameters _dp = new BasicLibrary.DataAccess.DatabaseParameters();
                string className = typeof(Form).Name;
                _dp = BasicLookUpObject.get_BaseObjectMap(className);
				_dp.Add(className, "AccountId", System.Data.SqlDbType.Int);
				_dp.Add(className, "Uid", System.Data.SqlDbType.UniqueIdentifier);
				_dp.Add(className, "IsActive", System.Data.SqlDbType.Bit);
				_dp.Add(className, "AccountName", typeof(Account).Name, "Name", System.Data.SqlDbType.NVarChar, "Account");
				
                return _dp;
            }
        }

        protected static Form LoadObject(System.Data.DataRow row)
        {
            Form obj = new Form();
            obj.LoadObjectValues(ObjectMap, row);
            return obj;
        }

        protected static BasicLibrary.DataAccess.DatabaseQuery DatabaseQuery
        {
            get
            {
                BasicLibrary.DataAccess.DatabaseQuery dq = BaseObject.get_DatabaseQuery(ObjectMap, typeof(Form).Name);
				dq.Joins.Add(Account.ObjectMap["ID"], Form.ObjectMap["AccountId"], BasicLibrary.DataAccess.DatabaseJoinTypeEnum.InnerJoin, "Account");
				
                return dq;
            }
        }

        public static Form Load(int id, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["ID"], id);
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public static List<Form> Load(Hashtable criterias = null, int? start = null, int? length = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
			dq.UseParameterizedQuery = false;
			dq.SelectRange.Start = start;
			dq.SelectRange.Length = length;
            return Form.BaseLoad(criterias, ObjectMap, dq, msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
        }

        public static int GetCount(Hashtable criterias = null, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            return Form.BaseGetCount(criterias, ObjectMap, dq, msgs);
        }

		public SystemMessages Save(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
			var msgs = new SystemMessages();

			if (this.AccountId.IsEmpty())
				msgs.Add("AccountId is required on Form object", MessageType.Error);
			if (this.Uid.IsEmpty())
				msgs.Add("Uid is required on Form object", MessageType.Error);
			
			if (msgs.HasNoErrors())
				msgs.AddRange(base._Save2(DatabaseQuery, dc, ObjectMap, typeof(Form).Name));

			return msgs;
        }

        public SystemMessages Delete(BasicLibrary.DataAccess.DatabaseConnection dc = null)
        {
            return base._Delete2(DatabaseQuery, dc, ObjectMap, typeof(Form).Name);
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
			dps.Add(ObjectMap["AccountId"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["Uid"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			dps = new BasicLibrary.DataAccess.DatabaseParameters();
			dps.Add(ObjectMap["IsActive"]);
			msgs.AddRange(BaseObject._Alter(DatabaseQuery, dc, dps));

			

            return msgs;
        }

		public Form Reload(SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			return Form.Load(this.ID.Value, msgs, dc: dc);
		}
		
		public static List<Form> LoadByAccountId(int AccountId, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject);
		}

		public static Form LoadByUid(Guid Uid, SystemMessages msgs = null, BasicLibrary.DataAccess.DatabaseConnection dc = null)
		{
			var dq = DatabaseQuery;
			dq.Connection = dc;
			dq.Criterias.Add(ObjectMap["Uid"], Uid);
			return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
		}

		

        #endregion

		#region "UserDefined"

        public int GetQuestionCount(DatabaseConnection dc = null)
        {
            return FormQuestion.GetCount(
                new Hashtable() {
                    { "FormId", this.ID.Value }
                },
                dc: dc
            );
        }

        public static Form GetOrCreateAfterSchoolAccountTemplateForm(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return GetLatestTemplateForm(1, msgs, dc) ?? CreateAfterSchoolAccountDefaultForm(msgs, dc);
        }

        public static Form GetLatestTemplateForm(int AccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            BasicLibrary.DataAccess.DatabaseQuery dq = DatabaseQuery;
            dq.Connection = dc;
            dq.Criterias.Add(ObjectMap["AccountId"], AccountId);
            dq.Criterias.Add(ObjectMap["IsActive"], true);
            dq.OrderBys.Add(ObjectMap["ID"], OrderByDirectionEnum.Descending);
            dq.SelectRange.Start = 0;
            dq.SelectRange.Length = 1;
            return dq.Select(msgs).AsEnumerable().ToList().ConvertAll(LoadObject).FirstOrDefault();
        }

        public List<FormQuestion> GetAllQuestions(DatabaseConnection dc = null)
        {
            return FormQuestion.LoadByFormId(this.ID.Value, dc: dc);
        }

        public List<FormQuestion> GetActiveQuestions(DatabaseConnection dc = null)
        {
            return FormQuestion.LoadByFormIdByIsActive(this.ID.Value, true, dc: dc);
        }

        public List<FormQuestionGroup> GetQuestionGroups(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return FormQuestionGroup.LoadByFormId(this.ID.Value, msgs, dc).OrderBy(x => x.DisplayOrder).ToList();
        }

        public Form Copy(int loggedInUserId, int newAccountId, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var form = new Form
            {
                Name = this.Name,
                Description = this.Description,
                LastModifiedDate = DateTime.UtcNow,
                CreateDate = DateTime.UtcNow,
                LastModifiedBy = loggedInUserId,
                CreatedBy = loggedInUserId,
                AccountId = newAccountId,
                IsActive = true,
                Uid = Guid.NewGuid()
            };
            msgs.AddRange(form.Save(dc: dc));

            var groups = FormQuestionGroup.LoadByFormId(this.ID.Value, dc: dc);
            foreach (var group in groups)
            {
                var questions = FormQuestion.LoadByFormQuestionGroupId(group.ID.Value, dc: dc);

                var newGroup = group.Copy();
                newGroup.LastModifiedDate = DateTime.UtcNow;
                newGroup.CreateDate = DateTime.UtcNow;
                newGroup.LastModifiedBy = newGroup.CreatedBy = loggedInUserId;
                newGroup.FormId = form.ID;
                msgs.AddRange(newGroup.Save(dc: dc));

                questions.ForEach(question => {
                    var newQuestion = question.Copy();
                    newQuestion.LastModifiedDate = newQuestion.CreateDate = DateTime.UtcNow;
                    newQuestion.LastModifiedBy = newQuestion.CreatedBy = loggedInUserId;
                    newQuestion.FormQuestionGroupId = newGroup.ID;
                    newQuestion.FormId = form.ID;
                    newQuestion.IsTemplate = false;
                    msgs.AddRange(newQuestion.Save(dc: dc));
                });
            }

            //check for default form existence
            var account = Account.Load(newAccountId, dc: dc);
            if (account.IsNotEmpty() && !account.DefaultFormId.HasValue)
            {
                account.DefaultFormId = form.ID;
                msgs.AddRange(account.Save(dc: dc));
            }

            return form;
        }

        public static Form CreateAfterSchoolAccountDefaultForm(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Form f = Form.LoadByAccountId(1, dc: dc).FirstOrDefault();
            if (f.IsEmpty())
            {
                f = new Form();
                f.AccountId = 1;
                f.LastModifiedDate = f.CreateDate = DateTime.UtcNow;
                f.LastModifiedBy = f.CreatedBy = 0;
                f.Name = "Default Form";
                f.IsActive = true;
                msgs.AddRange(f.Save(dc: dc));

                var questions = new QuestionHolder[]
                {
                new QuestionHolder() { Text = "First Name", IsActive = true, IsRequired = true, QuestionTypeId = QuestionType.TextBox, QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation, GroupName = "Student Information", IsLocked = true, QuestionDataTypeId = QuestionDataType.RegistrantFirstName },
                new QuestionHolder() { Text = "Last Name", IsActive = true, IsRequired = true, QuestionTypeId = QuestionType.TextBox, QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation, GroupName = "Student Information", IsLocked = true, QuestionDataTypeId = QuestionDataType.RegistrantLastName },
                new QuestionHolder() { Text = "Email", IsActive = true, IsRequired = true, QuestionTypeId = QuestionType.Email, QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation, GroupName = "Student Information", IsLocked = true, QuestionDataTypeId = QuestionDataType.RegistrantEmail },
                new QuestionHolder() { Text = "Date of Birth", IsActive = true, QuestionTypeId = QuestionType.Date, GroupName = "Student Information", QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation, QuestionDataTypeId = QuestionDataType.RegistrantDateOfBirth },
                new QuestionHolder() { Text = "Grade", IsActive = true, QuestionTypeId = QuestionType.DropDownList, GroupName = "Student Information", QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation, QuestionDataTypeId = QuestionDataType.RegistrantGrade, Options = "Pre-K,K,1st,2nd,3rd,4th,5th,6th,7th,8th,9th,10th,11th,12th" },
                new QuestionHolder() { Text = "Gender", IsActive = true, QuestionTypeId = QuestionType.RadioList, GroupName = "Student Information", QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation, QuestionDataTypeId = QuestionDataType.RegistrantGender, Options = "Male,Female" },
                new QuestionHolder() { Text = "T-Shirt Size", IsActive = true, QuestionTypeId = QuestionType.RadioList, GroupName = "Student Information", QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation, QuestionDataTypeId = QuestionDataType.RegistrantTShirtSize, Options = "XS,S,M,L,XL,XXL,XXXL" },
                new QuestionHolder() { Text = "Ethnicity", IsActive = true, QuestionTypeId = QuestionType.DropDownList, GroupName = "Student Information", QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation, QuestionDataTypeId = QuestionDataType.RegistrantEthnicity, Options = "American Indian/Alaskan Native,Asian/Pacific Islander,Black/African-American,Hispanic/Latino,White,Other" },
                new QuestionHolder() { Text = "Organization", QuestionTypeId = QuestionType.TextBox, GroupName = "Student Information", QuestionGroupDataTypeId = QuestionGroupDataType.PersonalInformation, IsLocked = false, QuestionDataTypeId = QuestionDataType.RegistrantOrganization },

                new QuestionHolder() { Text = "Phone", QuestionTypeId = QuestionType.TextBox, GroupName = "Contact Information", QuestionGroupDataTypeId = QuestionGroupDataType.ContactInformation, QuestionDataTypeId = QuestionDataType.RegistrantPhone1 },
                new QuestionHolder() { Text = "Phone Type", QuestionTypeId = QuestionType.RadioList, GroupName = "Contact Information", QuestionGroupDataTypeId = QuestionGroupDataType.ContactInformation, QuestionDataTypeId = QuestionDataType.RegistrantPhoneType1, Options = "Mobile, Home, Work" },
                new QuestionHolder() { Text = "Alternate Phone", QuestionTypeId = QuestionType.TextBox, GroupName = "Contact Information", QuestionGroupDataTypeId = QuestionGroupDataType.ContactInformation, QuestionDataTypeId = QuestionDataType.RegistrantPhone2 },
                new QuestionHolder() { Text = "Alternate Phone Type", QuestionTypeId = QuestionType.RadioList, GroupName = "Contact Information", QuestionGroupDataTypeId = QuestionGroupDataType.ContactInformation, QuestionDataTypeId = QuestionDataType.RegistrantPhoneType2, Options = "Mobile, Home, Work" },

                new QuestionHolder() { Text = "Street Address", QuestionTypeId = QuestionType.TextBox, GroupName = "Home Address", QuestionGroupDataTypeId = QuestionGroupDataType.HomeAddress, QuestionDataTypeId = QuestionDataType.RegistrantAddress1 },
                new QuestionHolder() { Text = "Apt/Suite", QuestionTypeId = QuestionType.TextBox, GroupName = "Home Address", QuestionGroupDataTypeId = QuestionGroupDataType.HomeAddress, QuestionDataTypeId = QuestionDataType.RegistrantAddress2 },
                new QuestionHolder() { Text = "City", QuestionTypeId = QuestionType.TextBox, GroupName = "Home Address", QuestionGroupDataTypeId = QuestionGroupDataType.HomeAddress, QuestionDataTypeId = QuestionDataType.RegistrantCity },
                new QuestionHolder() { Text = "State", QuestionTypeId = QuestionType.DropDownList, GroupName = "Home Address", QuestionGroupDataTypeId = QuestionGroupDataType.HomeAddress, Options = QuestionHolder.StateList, QuestionDataTypeId = QuestionDataType.RegistrantState },
                new QuestionHolder() { Text = "Zip Code", QuestionTypeId = QuestionType.TextBox, GroupName = "Home Address", QuestionGroupDataTypeId = QuestionGroupDataType.HomeAddress, QuestionDataTypeId = QuestionDataType.RegistrantPostalCode },

                new QuestionHolder() { Text = "First Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 1 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1FirstName },
                new QuestionHolder() { Text = "Last Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 1 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1LastName },
                new QuestionHolder() { Text = "Email", IsActive = true, QuestionTypeId = QuestionType.Email, GroupName = "Parent 1 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1Email },
                new QuestionHolder() { Text = "Phone", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 1 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1Phone1 },
                new QuestionHolder() { Text = "Phone Type", IsActive = true, QuestionTypeId = QuestionType.RadioList, GroupName = "Parent 1 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1PhoneType1, Options = "Mobile, Home, Work" },
                new QuestionHolder() { Text = "Alternate Phone", QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 1 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1Phone2 },
                new QuestionHolder() { Text = "Alternate Phone Type", QuestionTypeId = QuestionType.RadioList, GroupName = "Parent 1 Information", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1PhoneType2, Options = "Mobile, Home, Work" },
                new QuestionHolder() { Text = "Street Address", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 1 Address", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1Address1 },
                new QuestionHolder() { Text = "Apt/Suite", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 1 Address", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1Address2 },
                new QuestionHolder() { Text = "City", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 1 Address", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1City },
                new QuestionHolder() { Text = "State", IsActive = true, QuestionTypeId = QuestionType.DropDownList, GroupName = "Parent 1 Address", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, Options = QuestionHolder.StateList, QuestionDataTypeId = QuestionDataType.Parent1State },
                new QuestionHolder() { Text = "Zip Code", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Parent 1 Address", QuestionGroupDataTypeId = QuestionGroupDataType.Parent1Information, QuestionDataTypeId = QuestionDataType.Parent1PostalCode },

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

                new QuestionHolder() { Text = "First Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Emergency Contact 1", QuestionGroupDataTypeId = QuestionGroupDataType.EmergencyContact1, QuestionDataTypeId = QuestionDataType.EmergencyFirstName1 },
                new QuestionHolder() { Text = "Last Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Emergency Contact 1", QuestionGroupDataTypeId = QuestionGroupDataType.EmergencyContact1, QuestionDataTypeId = QuestionDataType.EmergencyLastName1 },
                new QuestionHolder() { Text = "Phone", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Emergency Contact 1", QuestionGroupDataTypeId = QuestionGroupDataType.EmergencyContact1, QuestionDataTypeId = QuestionDataType.EmergencyPhone1 },

                new QuestionHolder() { Text = "First Name", QuestionTypeId = QuestionType.TextBox, GroupName = "Emergency Contact 2", QuestionGroupDataTypeId = QuestionGroupDataType.EmergencyContact2, QuestionDataTypeId = QuestionDataType.EmergencyFirstName2 },
                new QuestionHolder() { Text = "Last Name", QuestionTypeId = QuestionType.TextBox, GroupName = "Emergency Contact 2", QuestionGroupDataTypeId = QuestionGroupDataType.EmergencyContact2, QuestionDataTypeId = QuestionDataType.EmergencyLastName2 },
                new QuestionHolder() { Text = "Phone", QuestionTypeId = QuestionType.TextBox, GroupName = "Emergency Contact 2", QuestionGroupDataTypeId = QuestionGroupDataType.EmergencyContact2, QuestionDataTypeId = QuestionDataType.EmergencyPhone2 },

                new QuestionHolder() { Text = "Doctor Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Health Information", QuestionGroupDataTypeId = QuestionGroupDataType.HealthInformation, QuestionDataTypeId = QuestionDataType.RegistrantDoctorName },
                new QuestionHolder() { Text = "Doctor Phone", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Health Information", QuestionGroupDataTypeId = QuestionGroupDataType.HealthInformation, QuestionDataTypeId = QuestionDataType.RegistrantDoctorPhone },
                new QuestionHolder() { Text = "Preferred Hospital (in case of emergency)", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = "Health Information", QuestionGroupDataTypeId = QuestionGroupDataType.HealthInformation, QuestionDataTypeId = QuestionDataType.RegistrantPreferredHospital },
                new QuestionHolder() { Text = "Allergies", IsActive = true, QuestionTypeId = QuestionType.TextArea, GroupName = "Health Information", QuestionGroupDataTypeId = QuestionGroupDataType.HealthInformation, QuestionDataTypeId = QuestionDataType.RegistrantAllergies },
                new QuestionHolder() { Text = "Medical Conditions", IsActive = true, QuestionTypeId = QuestionType.TextArea, GroupName = "Health Information", QuestionGroupDataTypeId = QuestionGroupDataType.HealthInformation, QuestionDataTypeId = QuestionDataType.RegistrantMedicalConditions },
                new QuestionHolder() { Text = "Are you up to date on Immunizations?", IsActive = true, QuestionTypeId = QuestionType.RadioList, GroupName = "Health Information", QuestionGroupDataTypeId = QuestionGroupDataType.HealthInformation, QuestionDataTypeId = QuestionDataType.RegistrantImmunizations, Options = "Yes,No,Not Sure" },

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
                        group = new FormQuestionGroup
                        {
                            Name = question.GroupName,
                            FormId = f.ID,
                            QuestionGroupDataTypeId = question.QuestionGroupDataTypeId,
                            DisplayOrder = groupDisplayOrder,
                            RegistrantTypeId = question.RegistrantTypeId,
                            LastModifiedDate = DateTime.UtcNow,
                            CreateDate = DateTime.UtcNow,
                            LastModifiedBy = 0,
                            CreatedBy = 0
                        };
                        msgs.AddRange(group.Save(dc: dc));

                        groupDisplayOrder++;
                    }

                    FormQuestion q = FormQuestion.LoadByFormQuestionGroupIdByQuestionDataTypeId(group.ID.Value, question.QuestionDataTypeId, dc: dc);
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
                        msgs.AddRange(q.Save(dc: dc));

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
                    var groups = form.GetQuestionGroups(dc: dc);
                    foreach (var group in groups)
                    {
                        if (passed > 1)
                        {
                            group.DisplayOrder = group.DisplayOrder + 2;
                            msgs.AddRange(group.Save(dc: dc));
                        }
                        else if (passed > 0)
                        {
                            group.DisplayOrder = group.DisplayOrder + 1;
                            msgs.AddRange(group.Save(dc: dc));
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
                                dc: dc);

                            if (hasGroup.Count == 0)
                            {
                                var newGroup = new FormQuestionGroup();
                                newGroup.Name = "Parent 1 Address";
                                newGroup.FormId = form.ID;
                                newGroup.DisplayOrder = group.DisplayOrder + 1;
                                newGroup.RegistrantTypeId = RegistrantType.Individual;
                                newGroup.LastModifiedDate = newGroup.CreateDate = DateTime.UtcNow;
                                newGroup.LastModifiedBy = newGroup.CreatedBy = 0;
                                msgs.AddRange(newGroup.Save(dc: dc));

                                var questions = group.GetAllQuestions(dc: dc);
                                var i = 0;
                                foreach (var question in questions)
                                {
                                    var datatypes = new QuestionDataType[] { QuestionDataType.Parent1Address1, QuestionDataType.Parent1Address2, QuestionDataType.Parent1City, QuestionDataType.Parent1State, QuestionDataType.Parent1PostalCode };

                                    if (datatypes.Contains(question.QuestionDataTypeId))
                                    {
                                        question.FormQuestionGroupId = newGroup.ID;
                                        question.DisplayOrder = i;
                                        msgs.AddRange(question.Save(dc: dc));
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
                                   dc: dc);

                            if (hasGroup.Count == 0)
                            {
                                var newGroup = new FormQuestionGroup();
                                newGroup.Name = "Parent 2 Address";
                                newGroup.FormId = form.ID;
                                newGroup.DisplayOrder = group.DisplayOrder + 1;
                                newGroup.RegistrantTypeId = RegistrantType.Individual;
                                newGroup.LastModifiedDate = newGroup.CreateDate = DateTime.UtcNow;
                                newGroup.LastModifiedBy = newGroup.CreatedBy = 0;
                                msgs.AddRange(newGroup.Save(dc: dc));

                                var questions = group.GetAllQuestions(dc: dc);
                                var i = 0;
                                foreach (var question in questions)
                                {
                                    var datatypes = new QuestionDataType[] { QuestionDataType.Parent2Address1, QuestionDataType.Parent2Address2, QuestionDataType.Parent2City, QuestionDataType.Parent2State, QuestionDataType.Parent2PostalCode };

                                    if (datatypes.Contains(question.QuestionDataTypeId))
                                    {
                                        question.FormQuestionGroupId = newGroup.ID;
                                        question.DisplayOrder = i;
                                        msgs.AddRange(question.Save(dc: dc));
                                        i++;
                                    }

                                }
                            }
                            passed++;
                        }

                    }

                }

            }

            return f;
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

        #endregion
    }
}

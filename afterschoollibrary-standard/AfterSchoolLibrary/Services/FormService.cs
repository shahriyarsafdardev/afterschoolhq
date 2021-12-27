using System;
using System.Collections.Generic;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Collections;
using System.Linq;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibrary.Services
{
    public static class FormService
    {
        /// <summary>
        /// get template from default afterschool account with ID = 1
        /// </summary>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static Form GetOrCreateAfterSchoolAccountTemplateForm(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var afterSchoolAccount = AccountService.GetAfterSchoolAccount(msgs, dc);
            return Form.GetLatestTemplateForm(accountId: afterSchoolAccount.ID.Value, msgs: msgs, dc: dc) ??
                AddNewAfterSchoolDefaultForm(msgs, dc);
        }

        public static Form AddNewAfterSchoolDefaultForm(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var account = AccountService.GetAfterSchoolAccount(msgs, dc);
            var formCount = Form.GetCount(new Hashtable() { { "AccountId", account.ID } }, msgs, dc);

            var f = new Form
            {
                AccountId = account.ID,
                LastModifiedDate = DateTime.UtcNow,
                CreateDate = DateTime.UtcNow,
                LastModifiedBy = 0,
                CreatedBy = 0,
                Name = $"Default Form {(formCount + 1)}",
                IsActive = true
            };
            msgs.AddRange(f.Save(dc));

            msgs.AddRange(account.SetDefaultForm(f, null, dc));

            var questions = new List<QuestionHolder>();
            questions.AddRange(CreateStudentInformationGroup());
            // questions.AddRange(CreateStudentContactInformationGroup());
            questions.AddRange(CreateStudentAddressGroup());
            questions.AddRange(CreateParent1InformationGroup());
            questions.AddRange(CreateParent1AddressGroup());
            questions.AddRange(CreateParent2InformationGroup());
            questions.AddRange(CreateParent2AddressGroup());
            questions.AddRange(CreateEmergencyContact1Group());
            questions.AddRange(CreateEmergencyContact2Group());
            questions.AddRange(CreateHealthInformationGroup());

            int displayOrder = 0;
            int groupDisplayOrder = 0;

            foreach (var question in questions)
            {
                var group = FormQuestionGroup.Load(
                    criterias: new Hashtable()
                    {
                        { "Name", question.GroupName },
                        { "FormId", f.ID.Value }
                    },
                    msgs: msgs,
                    dc: dc)
                    .FirstOrDefault();

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
                    msgs.AddRange(group.Save(dc));

                    groupDisplayOrder++;
                }

                var q = FormQuestion.LoadByFormQuestionGroupIdByQuestionDataTypeId(group.ID.Value, question.QuestionDataTypeId, msgs, dc);
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
                    q.DisplayOrder = group.GetQuestionCount(msgs, dc) + 1;
                    q.QuestionTypeId = question.QuestionTypeId;
                    q.QuestionDataTypeId = question.QuestionDataTypeId;
                    q.Options = question.Options;
                    q.LastModifiedDate = q.CreateDate = DateTime.UtcNow;
                    q.LastModifiedBy = q.CreatedBy = 0;
                    msgs.AddRange(q.Save(dc));

                    displayOrder++;
                }
            }

            return f;
        }

        private static List<QuestionType> SurveyOnlyQuestionTypes
        {
            get
            {
                return new List<QuestionType> { QuestionType.Audio, QuestionType.Video, QuestionType.Photo, QuestionType.Emoji, QuestionType.Rating };
            }
        }

        public static List<QuestionType> GetTypesUserCannotAdd(this Form form)
        {
            var typesUserCannotAdd = new List<QuestionType>() { QuestionType.TextBlock, QuestionType.NotSet, QuestionType.RadioMatrix };

            if (form.FormTypeId != FormType.Survey)
                 typesUserCannotAdd.AddRange(SurveyOnlyQuestionTypes);

            return typesUserCannotAdd;
        }

        /// <summary>
        /// get template from default afterschool account with ID = 1
        /// </summary>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static Form GetOrCreateAfterSchoolAccountInterestForm(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var afterSchoolAccount = AccountService.GetAfterSchoolAccount(msgs, dc);

            if (afterSchoolAccount.InterestFormId.HasValue)
                return Form.Load(afterSchoolAccount.InterestFormId.Value, msgs, dc);
                                        
            return AddNewAfterSchoolInterestForm(msgs, dc);
        }

        public static Form CreateBlankForm(Form form, int userId, int id, string name, string description, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
               Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        form = new Form
                        {
                            Name = name,
                            Description = description,
                            LastModifiedDate = DateTime.UtcNow,
                            CreateDate = DateTime.UtcNow,
                            FormTypeId = FormType.Survey,
                            LastModifiedBy = userId,
                            CreatedBy = userId,
                            AccountId = id,
                            IsActive = true,
                            Uid = Guid.NewGuid()
                        };
                        msgs.AddRange(form.Save(dc: dc));

                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        msgs.Add("Form Saved", MessageType.Success);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );

            return form;
        }

        public static Form AddNewAfterSchoolInterestForm(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var account = AccountService.GetAfterSchoolAccount(msgs, dc);
            var formCount = Form.GetCount(new Hashtable() { { "AccountId", account.ID } }, msgs, dc);

            var f = new Form
            {
                AccountId = account.ID,
                LastModifiedDate = DateTime.UtcNow,
                CreateDate = DateTime.UtcNow,
                LastModifiedBy = 0,
                CreatedBy = 0,
                Name = $"Interest Form {(formCount + 1)}",
                IsActive = true
            };
            msgs.AddRange(f.Save(dc));
            msgs.AddRange(account.SetInterestForm(f, null, dc));

            var questions = CreateParent1InterestGroup();
            int displayOrder = 0;
            int groupDisplayOrder = 0;

            questions.ForEach(question =>
            {
                var group = FormQuestionGroup.Load(
                    criterias: new Hashtable()
                    {
                        { "Name", question.GroupName },
                        { "FormId", f.ID.Value }
                    },
                    msgs: msgs,
                    dc: dc)
                    .FirstOrDefault();

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
                    msgs.AddRange(group.Save(dc));

                    groupDisplayOrder++;
                }

                var q = FormQuestion.LoadByFormQuestionGroupIdByQuestionDataTypeId(group.ID.Value, question.QuestionDataTypeId, msgs, dc);
                if (q.IsEmpty())
                {
                    q = new FormQuestion
                    {
                        Text = question.Text,
                        IsActive = question.IsActive,
                        IsRequired = question.IsRequired,
                        FormId = f.ID,
                        FormQuestionGroupId = group.ID,
                        IsTemplate = true,
                        IsLocked = question.IsLocked,
                        DisplayOrder = group.GetQuestionCount(msgs, dc) + 1,
                        QuestionTypeId = question.QuestionTypeId,
                        QuestionDataTypeId = question.QuestionDataTypeId,
                        Options = question.Options,
                        LastModifiedDate = DateTime.UtcNow,
                        CreateDate = DateTime.UtcNow,
                        LastModifiedBy = 0,
                        CreatedBy = 0
                    };
                    msgs.AddRange(q.Save(dc));

                    displayOrder++;
                }
            });

            return f;
        }

        private static List<QuestionHolder> CreateHealthInformationGroup()
        {
            var qhs = new List<QuestionHolder>();
            var groupType = QuestionGroupDataType.HealthInformation;
            var groupLabel = groupType.ToString().SplitByUpperCase();

            qhs.Add(new QuestionHolder() { Text = "Doctor Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantDoctorName });
            qhs.Add(new QuestionHolder() { Text = "Doctor Phone", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantDoctorPhone });
            qhs.Add(new QuestionHolder() { Text = "Preferred Hospital (in case of emergency)", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantPreferredHospital });
            qhs.Add(new QuestionHolder() { Text = "Allergies", IsActive = true, QuestionTypeId = QuestionType.TextArea, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantAllergies });
            qhs.Add(new QuestionHolder() { Text = "Medical Conditions", IsActive = true, QuestionTypeId = QuestionType.TextArea, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantMedicalConditions });
            qhs.Add(new QuestionHolder() { Text = "Are you up to date on Immunizations?", IsActive = true, QuestionTypeId = QuestionType.RadioList, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantImmunizations, Options = "Yes,No,Not Sure" });

            return qhs;
        }

        private static List<QuestionHolder> CreateEmergencyContact1Group()
        {
            var qhs = new List<QuestionHolder>();
            var groupType = QuestionGroupDataType.EmergencyContact1;
            var groupLabel = groupType.ToString().SplitByUpperCase();

            qhs.Add(new QuestionHolder() { Text = "First Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.EmergencyContact1FirstName });
            qhs.Add(new QuestionHolder() { Text = "Last Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.EmergencyContact1LastName });
            qhs.Add(new QuestionHolder() { Text = "Phone", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.EmergencyContact1Phone });

            return qhs;
        }

        private static List<QuestionHolder> CreateEmergencyContact2Group()
        {
            var qhs = new List<QuestionHolder>();
            var groupType = QuestionGroupDataType.EmergencyContact2;
            var groupLabel = groupType.ToString().SplitByUpperCase();

            qhs.Add(new QuestionHolder() { Text = "First Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.EmergencyContact2FirstName });
            qhs.Add(new QuestionHolder() { Text = "Last Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.EmergencyContact2LastName });
            qhs.Add(new QuestionHolder() { Text = "Phone", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.EmergencyContact2Phone });

            return qhs;
        }

        private static List<QuestionHolder> CreateParent1AddressGroup()
        {
            var qhs = new List<QuestionHolder>();
            var groupType = QuestionGroupDataType.Parent1Address;
            var groupLabel = "Parent/Guardian Address";

            qhs.Add(new QuestionHolder() { Text = "Street Address", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1Address1 });
            qhs.Add(new QuestionHolder() { Text = "Apt/Suite", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1Address2 });
            qhs.Add(new QuestionHolder() { Text = "City", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1City });
            qhs.Add(new QuestionHolder() { Text = "State", IsActive = true, QuestionTypeId = QuestionType.DropDownList, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, Options = QuestionHolder.StateList, QuestionDataTypeId = QuestionDataType.Parent1State });
            qhs.Add(new QuestionHolder() { Text = "Zip Code", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1PostalCode });

            return qhs;
        }

        private static List<QuestionHolder> CreateParent2AddressGroup()
        {
            var qhs = new List<QuestionHolder>();
            var groupType = QuestionGroupDataType.Parent2Address;
            var groupLabel = "Additional Parent/Guardian Address";

            qhs.Add(new QuestionHolder() { Text = "Street Address", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2Address1 });
            qhs.Add(new QuestionHolder() { Text = "Apt/Suite", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2Address2 });
            qhs.Add(new QuestionHolder() { Text = "City", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2City });
            qhs.Add(new QuestionHolder() { Text = "State", IsActive = true, QuestionTypeId = QuestionType.DropDownList, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, Options = QuestionHolder.StateList, QuestionDataTypeId = QuestionDataType.Parent2State });
            qhs.Add(new QuestionHolder() { Text = "Zip Code", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2PostalCode });

            return qhs;
        }

        private static List<QuestionHolder> CreateParent1InformationGroup()
        {
            var qhs = new List<QuestionHolder>();
            var groupType = QuestionGroupDataType.Parent1Information;
            var groupLabel = "Parent/Guardian Information";

            qhs.Add(new QuestionHolder() { Text = "First Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1FirstName });
            qhs.Add(new QuestionHolder() { Text = "Last Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1LastName });
            qhs.Add(new QuestionHolder() { Text = "Email", IsActive = true, QuestionTypeId = QuestionType.Email, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1Email });

            qhs.Add(new QuestionHolder() { Text = "Parent Guardian Relationship To Student", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1Employer, Options = "Mother,Father,Grandmother,Grandfather,Sister,Brother,Aunt,Uncle,Legal Guardian,Other" });
            qhs.Add(new QuestionHolder() { Text = "Parent Guardian Employer", QuestionTypeId = QuestionType.DropDownList, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1RelationshipToRegistrant });
           
            qhs.Add(new QuestionHolder() { Text = "Mobile Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1MobilePhone });
            qhs.Add(new QuestionHolder() { Text = "Home Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1HomePhone });
            qhs.Add(new QuestionHolder() { Text = "Work Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1WorkPhone });

            //qhs.Add(new QuestionHolder() { Text = "Phone", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1Phone1 });
            //qhs.Add(new QuestionHolder() { Text = "Phone Type", IsActive = true, QuestionTypeId = QuestionType.RadioList, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1PhoneType1, Options = "Mobile, Home, Work" });
            //qhs.Add(new QuestionHolder() { Text = "Alternate Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1Phone2 });
            //qhs.Add(new QuestionHolder() { Text = "Alternate Phone Type", QuestionTypeId = QuestionType.RadioList, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1PhoneType2, Options = "Mobile, Home, Work" });

            return qhs;
        }

        private static List<QuestionHolder> CreateParent2InformationGroup()
        {
            var qhs = new List<QuestionHolder>();
            var groupType = QuestionGroupDataType.Parent2Information;
            var groupLabel = "Additional Parent/Guardian Information";

            qhs.Add(new QuestionHolder() { Text = "First Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2FirstName });
            qhs.Add(new QuestionHolder() { Text = "Last Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2LastName });
            qhs.Add(new QuestionHolder() { Text = "Email", IsActive = true, QuestionTypeId = QuestionType.Email, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2Email });

            qhs.Add(new QuestionHolder() { Text = "Mobile Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2MobilePhone });
            qhs.Add(new QuestionHolder() { Text = "Home Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2HomePhone });
            qhs.Add(new QuestionHolder() { Text = "Work Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2WorkPhone });

            qhs.Add(new QuestionHolder() { Text = "Secondary Guardian Relationship To Student", QuestionTypeId = QuestionType.DropDownList, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2Employer, Options = "Mother,Father,Grandmother,Grandfather,Sister,Brother,Aunt,Uncle,Legal Guardian,Other" });
            qhs.Add(new QuestionHolder() { Text = "Secondary Guardian Employer", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2RelationshipToRegistrant });
          
            //qhs.Add(new QuestionHolder() { Text = "Phone", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2Phone1 });
            //qhs.Add(new QuestionHolder() { Text = "Phone Type", IsActive = true, QuestionTypeId = QuestionType.RadioList, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2PhoneType1, Options = "Mobile, Home, Work" });
            //qhs.Add(new QuestionHolder() { Text = "Alternate Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2Phone2 });
            //qhs.Add(new QuestionHolder() { Text = "Alternate Phone Type", QuestionTypeId = QuestionType.RadioList, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2PhoneType2, Options = "Mobile, Home, Work" });

            return qhs;
        }

        private static List<QuestionHolder> CreateStudentAddressGroup()
        {
            var qhs = new List<QuestionHolder>();
            var groupLabel = "Student Home Address";
            var groupType = QuestionGroupDataType.HomeAddress;

            qhs.Add(new QuestionHolder() { Text = "Street Address", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantAddress1 });
            qhs.Add(new QuestionHolder() { Text = "Apt/Suite", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantAddress2 });
            qhs.Add(new QuestionHolder() { Text = "City", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantCity });
            qhs.Add(new QuestionHolder() { Text = "State", QuestionTypeId = QuestionType.DropDownList, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, Options = QuestionHolder.StateList, QuestionDataTypeId = QuestionDataType.RegistrantState });
            qhs.Add(new QuestionHolder() { Text = "Zip Code", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantPostalCode });
            qhs.Add(new QuestionHolder() { Text = "Parent Guardian Relationship To Student", QuestionTypeId = QuestionType.DropDownList, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1Employer, Options = "Mother,Father,Grandmother,Grandfather,Sister,Brother,Aunt,Uncle,Legal Guardian,Other" });
            qhs.Add(new QuestionHolder() { Text = "Parent Guardian Employer", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1RelationshipToRegistrant });

            return qhs;
        }

        private static List<QuestionHolder> CreateStudentContactInformationGroup()
        {
            var qhs = new List<QuestionHolder>();
            var groupLabel = "Student Contact Information";
            var groupType = QuestionGroupDataType.ContactInformation;

            qhs.Add(new QuestionHolder() { Text = "Mobile Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantMobilePhone });
            qhs.Add(new QuestionHolder() { Text = "Home Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantHomePhone });

            //qhs.Add(new QuestionHolder() { Text = "Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantPhone1 });
            //qhs.Add(new QuestionHolder() { Text = "Phone Type", QuestionTypeId = QuestionType.RadioList, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantPhoneType1, Options = "Mobile, Home, Work" });
            //qhs.Add(new QuestionHolder() { Text = "Alternate Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantPhone2 });
            //qhs.Add(new QuestionHolder() { Text = "Alternate Phone Type", QuestionTypeId = QuestionType.RadioList, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantPhoneType2, Options = "Mobile, Home, Work" });

            return qhs;
        }

        private static List<QuestionHolder> CreateStudentInformationGroup()
        {
            var qhs = new List<QuestionHolder>();
            var groupLabel = "Student Information";
            var groupType = QuestionGroupDataType.PersonalInformation;

            qhs.Add(new QuestionHolder()
            {
                Text = "First Name",
                IsActive = true,
                IsRequired = true,
                QuestionTypeId = QuestionType.TextBox,
                QuestionGroupDataTypeId = groupType,
                GroupName = groupLabel,
                IsLocked = true,
                QuestionDataTypeId = QuestionDataType.RegistrantFirstName
            });
            qhs.Add(new QuestionHolder()
            {
                Text = "Last Name",
                IsActive = true,
                IsRequired = true,
                QuestionTypeId = QuestionType.TextBox,
                QuestionGroupDataTypeId = groupType,
                GroupName = groupLabel,
                IsLocked = true,
                QuestionDataTypeId = QuestionDataType.RegistrantLastName
            });
            qhs.Add(new QuestionHolder()
            {
                Text = "Email",
                IsActive = true,
                IsRequired = true,
                QuestionTypeId = QuestionType.Email,
                QuestionGroupDataTypeId = groupType,
                GroupName = groupLabel,
                IsLocked = true,
                QuestionDataTypeId = QuestionDataType.RegistrantEmail
            });
            qhs.Add(new QuestionHolder()
            {
                Text = "Mobile Phone",
                QuestionTypeId = QuestionType.TextBox,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantMobilePhone
            });
            qhs.Add(new QuestionHolder()
            {
                Text = "Home Phone",
                QuestionTypeId = QuestionType.TextBox,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantHomePhone
            });
            qhs.Add(new QuestionHolder()
            {
                Text = "Work Phone",
                QuestionTypeId = QuestionType.TextBox,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantWorkPhone,
                IsActive = false
            });
            qhs.Add(new QuestionHolder()
            {
                Text = "Date of Birth",
                IsActive = true,
                QuestionTypeId = QuestionType.Date,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantDateOfBirth
            });
            qhs.Add(new QuestionHolder()
            {
                Text = "Grade",
                IsActive = true,
                QuestionTypeId = QuestionType.DropDownList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantGrade,
                Options = "Pre-K,K,1st,2nd,3rd,4th,5th,6th,7th,8th,9th,10th,11th,12th"
            });
            qhs.Add(new QuestionHolder()
            {
                Text = "Gender",
                IsActive = true,
                QuestionTypeId = QuestionType.RadioList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantGender,
                Options = "Male,Female,Other"
            });
            qhs.Add(new QuestionHolder()
            {
                Text = "T-Shirt Size",
                IsActive = true,
                QuestionTypeId = QuestionType.RadioList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantTShirtSize,
                Options = "XXS,XS,S,M,L,XL,XXL,XXXL"
            });
            qhs.Add(new QuestionHolder()
            {
                Text = "Ethnicity",
                IsActive = true,
                QuestionTypeId = QuestionType.DropDownList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantEthnicity,
                Options = "American Indian/Alaskan Native,Asian/Pacific Islander,Black/African-American,Hispanic/Latino,White,Multiracial,Other"
            });
            qhs.Add(new QuestionHolder()
            {
                Text = "Organization",
                QuestionTypeId = QuestionType.TextBox,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                IsLocked = false,
                QuestionDataTypeId = QuestionDataType.RegistrantOrganization
            });
            return qhs;
        }

        private static void CreateOrganizationQuestionGroup()
        {
            var questions = new List<QuestionHolder>()
                {   

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
        }

        private static List<QuestionHolder> CreateParent1InterestGroup()
        {
            var qhs = new List<QuestionHolder>();
            var groupType = QuestionGroupDataType.RegistrantInterestInformation;
            var groupLabel = "Parent/Guardian Information";

            qhs.Add(new QuestionHolder() { Text = "First Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantFirstName });
            qhs.Add(new QuestionHolder() { Text = "Last Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantLastName });
            qhs.Add(new QuestionHolder() { Text = "Email", IsActive = true, QuestionTypeId = QuestionType.Email, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantEmail });

            qhs.Add(new QuestionHolder() { Text = "Mobile Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantMobilePhone });

            var commonGradeOptions = "Pre-K,K,1st,2nd,3rd,4th,5th,6th,7th,8th,9th,10th,11th,12th";
            qhs.Add(new QuestionHolder()
            {
                Text = "Child 1 Grade",
                QuestionTypeId = QuestionType.DropDownList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantChild1Grade,
                Options = commonGradeOptions,
            });
            qhs.Add(new QuestionHolder()
            {
                Text = "Child 2 Grade",
                QuestionTypeId = QuestionType.DropDownList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantChild2Grade,
                Options = commonGradeOptions,
            });
            qhs.Add(new QuestionHolder()
            {
                Text = "Child 3 Grade",
                QuestionTypeId = QuestionType.DropDownList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantChild3Grade,
                Options = commonGradeOptions,
            });
            qhs.Add(new QuestionHolder()
            {
                Text = "Child 4 Grade",
                QuestionTypeId = QuestionType.DropDownList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantChild4Grade,
                Options = commonGradeOptions,
            });
            qhs.Add(new QuestionHolder()
            {
                Text = "Child 5 Grade",
                QuestionTypeId = QuestionType.DropDownList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantChild5Grade,
                Options = commonGradeOptions,
            });

            return qhs;
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
    }
}

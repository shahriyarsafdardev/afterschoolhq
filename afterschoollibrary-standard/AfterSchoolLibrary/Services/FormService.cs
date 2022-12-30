using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using SendGrid.Helpers.Mail;
using LibraryUtilities = AfterSchoolLibrary.Helpers.Utilities;
using Email = AfterSchoolLibrary.Helpers.Email;


namespace AfterSchoolLibrary.Services
{
    public static class FormService
    {
        private static List<QuestionType> SurveyOnlyQuestionTypes =>
            new List<QuestionType>
            {
                QuestionType.Audio, QuestionType.Video, QuestionType.Photo, QuestionType.Emoji, QuestionType.Rating 
            };

        public static string RegistrantGrade =>
            "Pre-K,K,1st,2nd,3rd,4th,5th,6th,7th,8th,9th,10th,11th,12th";

        public static string RegistrantGender =>
            "Male,Female,Other";

        public static string RegistrantTShirtSize =>
            "Youth XS,Youth S,Youth M,Youth L,Youth XL,Adult S,Adult M,Adult L,Adult XL,Adult 2XL, Adult 3XL";

        public static string RegistrantRace =>
            "American Indian or Alaska Native,Asian,Black or African American,Native Hawaiian or Other Pacific Islander,White,Two or More Races";

        public static string RegistrantEthnicity =>
            "Hispanic or Latino,Not Hispanic or Latino";

        public static string RelationshipToRegistrantForEmergencyContactsList =>
            "Father,Mother,Grandfather,Grandmother,Brother,Sister,Uncle,Aunt,Legal Guardian,Other";

        //Created separate list from Emergency Contact, as later they can differ at any stage
        public static string ParentRelationshipToRegistrant =>
            "Father,Mother,Grandfather,Grandmother,Brother,Sister,Uncle,Aunt,Legal Guardian,Other";

        public static void EmailDataReport(int accountId, int? formId, List<int> activityIds, string search, DateTime? startDate, DateTime? endDate, 
            int[] hiddenQuestions, List<int> searchQuestionIds, List<QuestionDataType> searchQuestionTypeIds, int loggedInUserId)
        {
            var msgs = new SystemMessages();
            var currentAccount = Account.Load(accountId, msgs);
            var loggedInUser = WebUser.Load(loggedInUserId, msgs);
            var questions = new List<FormQuestion>();
            var forms = new List<Form>();

            if (formId.HasValue)
                questions = currentAccount.GetForm(formId.Value)?.GetAllQuestions(msgs)
                        .Where(x => x.ID.Value.NotIn(hiddenQuestions) && !x.IsDeleted && x.QuestionTypeId != QuestionType.TextBlock)
                        .OrderBy(x => x.FormQuestionGroupDisplayOrder)
                        .ThenBy(x => x.DisplayOrder).ToList();

            if (activityIds.Any())
            {
                var selectedActivities = currentAccount.GetActivities(activityIds, msgs);
                selectedActivities.ForEach(x => forms.Add(x.GetActiveForm(msgs)));
            }

            if (!forms.Any())
                forms = currentAccount.GetForms(msgs).Where(x => x.FormTypeId == FormType.StudentForm).ToList();

            if (!questions.Any())
            {
                var allQuestions = new List<FormQuestion>();

                forms.ForEach(x => allQuestions.AddRange(x.GetAllQuestions(msgs).Where(x => !x.IsDeleted && x.QuestionTypeId != QuestionType.TextBlock)));
                questions.AddRange(allQuestions.Where(x => x.QuestionDataTypeId == QuestionDataType.NotSet));
                questions.AddRange(allQuestions.Where(x => x.QuestionDataTypeId != QuestionDataType.NotSet).DistinctBy(x => x.QuestionDataTypeId));
                questions = questions
                    .OrderBy(x => x.FormQuestionGroupDisplayOrder)
                    .ThenBy(x => x.DisplayOrder)
                    .ToList();
            }

            var getFirstRegistrantsOnly = false;

            if (activityIds.Count != 1 && !formId.HasValue)
                getFirstRegistrantsOnly = true;

            var registrants = Registrant.LoadByAnswered(currentAccount.ID.Value, formId, activityIds, search, startDate, endDate, null, null, 
                                getFirstRegistrantsOnly, searchQuestionIds, searchQuestionTypeIds, msgs);         
            var table = new List<List<string>>();
            var row = new List<string>();

            row.Add("");
            row.Add("");

            foreach (var question in questions)
                row.Add(question.FormQuestionGroupName);

            table.Add(row);

            row = new List<string>();
            row.Add("Activities");

            foreach (var question in questions)
                row.Add(question.Text);

            table.Add(row);

            foreach (var registrant in registrants)
            {
                row = new List<string>();              

                var ras = registrant.GetRegistrantActivities(msgs);

                if (getFirstRegistrantsOnly)
                    registrant.GetRelatedRegistrants(msgs)
                        .ForEach(relatedRegistrant => ras.AddRange(relatedRegistrant.GetRegistrantActivities(msgs)));

                if (activityIds.Any())
                    ras = ras.Where(ra => ra.ParticipationObjectTypeId == ParticipationObjectType.Activity && ra.ParticipationObjectId.Value.In(activityIds)).ToList();

                row.Add(ras.Select(ra => ra.GetParticipationObject(msgs).Name).ToStringList(", "));

                var answers = registrant.GetAnswers(msgs);

                if (getFirstRegistrantsOnly)
                {
                    var related = registrant.GetRelatedRegistrants(msgs);
                    related.ForEach(x => answers.AddRange(x.GetAnswers(msgs)));
                }

                foreach (var question in questions)
                {
                    var answer = answers.Where(answer => 
                                    !answer.Text.IsBlank() && 
                                    (answer.RegistrantId == registrant.ID || answer.FormQuestionId == question.ID) && 
                                    (answer.FormQuestionId == question.ID || 
                                        (answer.QuestionDataTypeId != QuestionDataType.NotSet && answer.QuestionDataTypeId == question.QuestionDataTypeId))
                                    ).OrderByDescending(x => x.CreateDate.Value).FirstOrDefault();
                    row.Add(answer?.GetAnswerText().IfEmpty(""));
                }

                table.Add(row);
            }

            var csv = CsvFile.Create(table);
            var csvByteData = Encoding.UTF8.GetBytes(csv);
            var subject = $"Submission Data Report";            
            var body = @$"<p>Your Submission Data report is attached.</p>
                        {Email.GetAppSignature(isAfterSchool: currentAccount.IsAfterSchool, utm_campaign: "submission_data_report")}";

            msgs.AddRange(SendGridHelper.SendMailWithAttachments(
                fromEmail: new EmailAddress(
                    Email.AppNoReplyEmailAddress,
                    "AfterSchool HQ"
                ),
                toEmail: new EmailAddress(loggedInUser.Email),
                subject: subject,
                body: body,
                emailTemplateId: SendGridHelper.GetGenericTemplateId(currentAccount.IsAfterSchool),
                attachmentByteData: csvByteData,
                attachmentFilename: $"Submission-Data-Report.csv"
            ));
        }

        public static Form GetAdministratorAccountStudentForm(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var administratorAccount = AccountService.GetAfterSchoolAccount(msgs, dc);
            return administratorAccount.GetDefaultStudentForm(msgs, dc);
        }

        public static Form GetAdministratorAccountVolunteerForm(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var administratorAccount = AccountService.GetAfterSchoolAccount(msgs, dc);
            return administratorAccount.GetDefaultVolunteerForm(msgs, dc);
        }

        /// <summary>
        ///     get template from default afterschool account with ID = 1
        /// </summary>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static Form GetOrCreateAfterSchoolAccountTemplateForm(FormType formType, SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var afterSchoolAccount = AccountService.GetAfterSchoolAccount(msgs, dc);
            return Form.GetLatestTemplateForm(afterSchoolAccount.ID.Value, formType, msgs, dc) ??
                   AddNewAfterSchoolDefaultForm(formType, msgs, dc);
        }

        private static Form AddNewAfterSchoolDefaultForm(FormType formType, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return formType switch
            {
                FormType.StudentForm => AddNewAfterSchoolDefaultStudentForm(msgs, dc),
                FormType.VolunteerForm => AddNewAfterSchoolDefaultVolunteerForm(msgs, dc),
                FormType.Survey => CreateBlankForm(msgs, dc),
                _ => null,
            };
        }

        public static Form AddNewAfterSchoolDefaultStudentForm(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var account = AccountService.GetAfterSchoolAccount(msgs, dc);
            var formCount = Form.GetCount(new Hashtable {{"AccountId", account.ID}, {"FormTypeId", (int) FormType.StudentForm}}, msgs, dc);

            var f = new Form
            {
                AccountId = account.ID,
                LastModifiedDate = DateTime.UtcNow,
                CreateDate = DateTime.UtcNow,
                LastModifiedBy = 0,
                CreatedBy = 0,
                Name = $"Default Form {formCount + 1}",
                IsActive = true,
                FormTypeId = FormType.StudentForm,
            };
            msgs.AddRange(f.Save(dc));

            msgs.AddRange(account.SetDefaultStudentForm(f, null, dc));

            var questions = new List<QuestionHolder>();
            questions.AddRange(CreateStudentInformationGroup());
            // questions.AddRange(CreateStudentContactInformationGroup());
            questions.AddRange(CreateRegistrantAddressGroup(FormType.StudentForm));
            questions.AddRange(CreateParent1InformationGroup());
            questions.AddRange(CreateParent1AddressGroup());
            questions.AddRange(CreateParent2InformationGroup());
            questions.AddRange(CreateParent2AddressGroup());
            questions.AddRange(CreateEmergencyContact1Group(FormType.StudentForm));
            questions.AddRange(CreateEmergencyContact2Group(FormType.StudentForm));
            questions.AddRange(CreateEmergencyContact3Group(FormType.StudentForm));
            questions.AddRange(CreateHealthInformationGroup());

            var displayOrder = 0;
            var groupDisplayOrder = 0;

            foreach (var question in questions)
            {
                var group = FormQuestionGroup.Load(
                        new Hashtable
                        {
                            {"Name", question.GroupName},
                            {"FormId", f.ID.Value},
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
                        CreatedBy = 0,
                    };
                    msgs.AddRange(group.Save(dc));

                    groupDisplayOrder++;
                }

                var q = FormQuestion.LoadByFormQuestionGroupIdByQuestionDataTypeId(group.ID.Value,
                    question.QuestionDataTypeId, msgs, dc);
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

        public static Form AddNewAfterSchoolDefaultVolunteerForm(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var account = AccountService.GetAfterSchoolAccount(msgs, dc);
            var formCount = Form.GetCount(new Hashtable {{"AccountId", account.ID}, {"FormTypeId", (int) FormType.VolunteerForm}}, msgs, dc);

            var f = new Form
            {
                AccountId = account.ID,
                LastModifiedDate = DateTime.UtcNow,
                CreateDate = DateTime.UtcNow,
                LastModifiedBy = 0,
                CreatedBy = 0,
                Name = $"Default Volunteer Form {formCount + 1}",
                IsActive = true,
                FormTypeId = FormType.VolunteerForm,
            };
            msgs.AddRange(f.Save(dc));

            msgs.AddRange(account.SetDefaultVolunteerForm(f, null, dc));

            var questions = new List<QuestionHolder>();
            questions.AddRange(CreateVolunteerInformationGroup());
            // questions.AddRange(CreateStudentContactInformationGroup());
            questions.AddRange(CreateRegistrantAddressGroup(FormType.VolunteerForm));
            questions.AddRange(CreateEmergencyContact1Group(FormType.VolunteerForm));
            questions.AddRange(CreateEmergencyContact2Group(FormType.VolunteerForm));
            questions.AddRange(CreateEmergencyContact3Group(FormType.VolunteerForm));

            var displayOrder = 0;
            var groupDisplayOrder = 0;

            foreach (var question in questions)
            {
                var group = FormQuestionGroup.Load(
                        new Hashtable
                        {
                            {"Name", question.GroupName},
                            {"FormId", f.ID.Value},
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
                        CreatedBy = 0,
                    };
                    msgs.AddRange(group.Save(dc));

                    groupDisplayOrder++;
                }

                var q = FormQuestion.LoadByFormQuestionGroupIdByQuestionDataTypeId(group.ID.Value,
                    question.QuestionDataTypeId, msgs, dc);
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
        public static List<QuestionType> GetTypesUserCannotAdd(this Form form)
        {
            var typesUserCannotAdd = new List<QuestionType> {QuestionType.TextBlock, QuestionType.NotSet, QuestionType.RadioMatrix};
            
            if (form.FormTypeId != FormType.Survey)
                typesUserCannotAdd.AddRange(SurveyOnlyQuestionTypes);

            return typesUserCannotAdd;
        }

        /// <summary>
        ///     get template from default afterschool account with ID = 1
        /// </summary>
        /// <param name="msgs"></param>
        /// <param name="dc"></param>
        /// <returns></returns>
        public static Form GetOrCreateAfterSchoolAccountInterestForm(SystemMessages msgs = null,
            DatabaseConnection dc = null)
        {
            var afterSchoolAccount = AccountService.GetAfterSchoolAccount(msgs, dc);

            if (afterSchoolAccount.InterestFormId.HasValue)
                return Form.Load(afterSchoolAccount.InterestFormId.Value, msgs, dc);

            return AddNewAfterSchoolInterestForm(msgs, dc);
        }

        private static Form CreateBlankForm(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            Form form = null;
            var account = AccountService.GetAfterSchoolAccount(msgs, dc);
            var formCount = Form.GetCount(new Hashtable {{"AccountId", account.ID}, {"FormTypeId", (int) FormType.Survey}}, msgs, dc);

            Database.ExecuteTransaction(
                (amsgs, adc) =>
                {
                    form = new Form
                    {
                        Name = $"Default Survey Form {formCount + 1}",
                        LastModifiedDate = DateTime.UtcNow,
                        CreateDate = DateTime.UtcNow,
                        FormTypeId = FormType.Survey,
                        LastModifiedBy = 0,
                        CreatedBy = 0,
                        AccountId = account.ID,
                        IsActive = true,
                        Uid = Guid.NewGuid(),
                    };
                    msgs.AddRange(form.Save(dc));
                },
                smsgs =>
                {
                    msgs.AddRange(smsgs);
                    msgs.Add("Form Saved", MessageType.Success);
                },
                fmsgs => { msgs.AddRange(fmsgs); }
            );

            return form;
        }

        public static Form AddNewAfterSchoolInterestForm(SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var account = AccountService.GetAfterSchoolAccount(msgs, dc);
            var formCount = Form.GetCount(new Hashtable {{"AccountId", account.ID}}, msgs, dc);

            var f = new Form
            {
                AccountId = account.ID,
                LastModifiedDate = DateTime.UtcNow,
                CreateDate = DateTime.UtcNow,
                LastModifiedBy = 0,
                CreatedBy = 0,
                Name = $"Interest Form {formCount + 1}",
                IsActive = true,
            };
            msgs.AddRange(f.Save(dc));
            msgs.AddRange(account.SetInterestForm(f, null, dc));

            var questions = CreateParent1InterestGroup();
            var displayOrder = 0;
            var groupDisplayOrder = 0;

            questions.ForEach(question =>
            {
                var group = FormQuestionGroup.Load(
                        new Hashtable
                        {
                            {"Name", question.GroupName},
                            {"FormId", f.ID.Value},
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
                        CreatedBy = 0,
                    };
                    msgs.AddRange(group.Save(dc));

                    groupDisplayOrder++;
                }

                var q = FormQuestion.LoadByFormQuestionGroupIdByQuestionDataTypeId(group.ID.Value,
                    question.QuestionDataTypeId, msgs, dc);
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
                        CreatedBy = 0,
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

            qhs.Add(new QuestionHolder
            {
                Text = "Doctor Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantDoctorName,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Doctor Phone", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantDoctorPhone,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Preferred Hospital (in case of emergency)", IsActive = true,
                QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantPreferredHospital,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Allergies", IsActive = true, QuestionTypeId = QuestionType.TextArea, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantAllergies,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Medical Conditions", IsActive = true, QuestionTypeId = QuestionType.TextArea,
                GroupName = groupLabel, QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantMedicalConditions,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Are you up to date on Immunizations?", IsActive = true, QuestionTypeId = QuestionType.RadioList,
                GroupName = groupLabel, QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantImmunizations, Options = "Yes,No,Not Sure",
            });

            return qhs;
        }

        public static List<QuestionHolder> CreateEmergencyContact1Group(FormType formType)
        {
            var qhs = new List<QuestionHolder>();
            var groupType = QuestionGroupDataType.EmergencyContact1;
            var groupLabel = groupType.ToString().SplitByUpperCase();

            qhs.Add(new QuestionHolder
            {
                Text = "First Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.EmergencyContact1FirstName,
                IsRequired = true,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Last Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.EmergencyContact1LastName,
                IsRequired = true,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Primary Phone", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.EmergencyContact1Phone,
                IsRequired = true,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Additional Phone 1", IsActive = true, QuestionTypeId = QuestionType.TextBox,
                GroupName = groupLabel, QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.EmergencyContact1Phone2,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Additional Phone 2", IsActive = true, QuestionTypeId = QuestionType.TextBox,
                GroupName = groupLabel, QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.EmergencyContact1Phone3,
            });

            if (formType != FormType.VolunteerForm)
                qhs.Add(new QuestionHolder
                {
                    Text = "Relationship To Registrant", IsActive = true, QuestionTypeId = QuestionType.DropDownList,
                    GroupName = groupLabel, QuestionGroupDataTypeId = groupType,
                    QuestionDataTypeId = QuestionDataType.EmergencyContact1RelationshipToRegistrant,
                    Options = RelationshipToRegistrantForEmergencyContactsList,
                });

            return qhs;
        }

        public static List<QuestionHolder> CreateEmergencyContact2Group(FormType formType)
        {
            var qhs = new List<QuestionHolder>();
            var groupType = QuestionGroupDataType.EmergencyContact2;
            var groupLabel = groupType.ToString().SplitByUpperCase();

            qhs.Add(new QuestionHolder
            {
                Text = "First Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.EmergencyContact2FirstName,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Last Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.EmergencyContact2LastName,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Primary Phone", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.EmergencyContact2Phone,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Additional Phone 1", IsActive = true, QuestionTypeId = QuestionType.TextBox,
                GroupName = groupLabel, QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.EmergencyContact2Phone2,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Additional Phone 2", IsActive = true, QuestionTypeId = QuestionType.TextBox,
                GroupName = groupLabel, QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.EmergencyContact2Phone3,
            });
            if (formType != FormType.VolunteerForm)
                qhs.Add(new QuestionHolder
                {
                    Text = "Relationship To Registrant", IsActive = true, QuestionTypeId = QuestionType.DropDownList,
                    GroupName = groupLabel, QuestionGroupDataTypeId = groupType,
                    QuestionDataTypeId = QuestionDataType.EmergencyContact2RelationshipToRegistrant,
                    Options = RelationshipToRegistrantForEmergencyContactsList,
                });

            return qhs;
        }

        public static List<QuestionHolder> CreateEmergencyContact3Group(FormType formType)
        {
            var qhs = new List<QuestionHolder>();
            var groupType = QuestionGroupDataType.EmergencyContact3;
            var groupLabel = groupType.ToString().SplitByUpperCase();

            qhs.Add(new QuestionHolder
            {
                Text = "First Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.EmergencyContact3FirstName,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Last Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.EmergencyContact3LastName,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Primary Phone", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.EmergencyContact3Phone,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Additional Phone 1", IsActive = true, QuestionTypeId = QuestionType.TextBox,
                GroupName = groupLabel, QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.EmergencyContact3Phone2,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Additional Phone 2", IsActive = true, QuestionTypeId = QuestionType.TextBox,
                GroupName = groupLabel, QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.EmergencyContact3Phone3,
            });
            if (formType != FormType.VolunteerForm)
                qhs.Add(new QuestionHolder
                {
                    Text = "Relationship To Registrant", IsActive = true, QuestionTypeId = QuestionType.DropDownList,
                    GroupName = groupLabel, QuestionGroupDataTypeId = groupType,
                    QuestionDataTypeId = QuestionDataType.EmergencyContact3RelationshipToRegistrant,
                    Options = RelationshipToRegistrantForEmergencyContactsList,
                });

            return qhs;
        }

        private static List<QuestionHolder> CreateParent1AddressGroup()
        {
            var qhs = new List<QuestionHolder>();
            var groupType = QuestionGroupDataType.Parent1Address;
            var groupLabel = "Parent/Guardian Address";

            qhs.Add(new QuestionHolder
            {
                Text = "Street Address", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1Address1,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Apt/Suite", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1Address2,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "City", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1City,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "State", IsActive = true, QuestionTypeId = QuestionType.DropDownList, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, Options = QuestionHolder.StateList,
                QuestionDataTypeId = QuestionDataType.Parent1State,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Zip Code", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1PostalCode,
            });

            return qhs;
        }

        private static List<QuestionHolder> CreateParent2AddressGroup()
        {
            var qhs = new List<QuestionHolder>();
            var groupType = QuestionGroupDataType.Parent2Address;
            var groupLabel = "Additional Parent/Guardian Address";

            qhs.Add(new QuestionHolder
            {
                Text = "Street Address", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2Address1,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Apt/Suite", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2Address2,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "City", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2City,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "State", IsActive = true, QuestionTypeId = QuestionType.DropDownList, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, Options = QuestionHolder.StateList,
                QuestionDataTypeId = QuestionDataType.Parent2State,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Zip Code", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2PostalCode,
            });
            return qhs;
        }

        private static List<QuestionHolder> CreateParent1InformationGroup()
        {
            var qhs = new List<QuestionHolder>();
            var groupType = QuestionGroupDataType.Parent1Information;
            var groupLabel = "Parent/Guardian Information";

            qhs.Add(new QuestionHolder
            {
                Text = "First Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1FirstName,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Middle Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1MiddleName,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Last Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1LastName,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Email", IsActive = true, QuestionTypeId = QuestionType.Email, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1Email,
            });

            qhs.Add(new QuestionHolder
            {
                Text = "Mobile Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1MobilePhone,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Home Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1HomePhone,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Work Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1WorkPhone,
            });

            qhs.Add(new QuestionHolder
            {
                Text = "Relationship To Student", QuestionTypeId = QuestionType.DropDownList, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.Parent1RelationshipToRegistrant,
                Options = ParentRelationshipToRegistrant,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Employer", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent1Employer,
            });

            return qhs;
        }

        private static List<QuestionHolder> CreateParent2InformationGroup()
        {
            var qhs = new List<QuestionHolder>();
            var groupType = QuestionGroupDataType.Parent2Information;
            var groupLabel = "Additional Parent/Guardian Information";

            qhs.Add(new QuestionHolder
            {
                Text = "First Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2FirstName,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Middle Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2MiddleName,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Last Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2LastName,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Email", IsActive = true, QuestionTypeId = QuestionType.Email, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2Email,
            });

            qhs.Add(new QuestionHolder
            {
                Text = "Mobile Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2MobilePhone,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Home Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2HomePhone,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Work Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2WorkPhone,
            });

            qhs.Add(new QuestionHolder
            {
                Text = "Relationship To Student", QuestionTypeId = QuestionType.DropDownList, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.Parent2RelationshipToRegistrant,
                Options = ParentRelationshipToRegistrant,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Employer", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.Parent2Employer,
            });

            return qhs;
        }

        private static List<QuestionHolder> CreateRegistrantAddressGroup(FormType formType)
        {
            var qhs = new List<QuestionHolder>();
            var groupLabel = formType == FormType.StudentForm ? "Student Home Address" : "Volunteer Home Address";
            var groupType = QuestionGroupDataType.HomeAddress;

            qhs.Add(new QuestionHolder
            {
                Text = "Street Address", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantAddress1,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Apt/Suite", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantAddress2,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "City", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantCity,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "State", QuestionTypeId = QuestionType.DropDownList, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, Options = QuestionHolder.StateList,
                QuestionDataTypeId = QuestionDataType.RegistrantState,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Zip Code", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantPostalCode,
            });

            return qhs;
        }

        private static List<QuestionHolder> CreateStudentContactInformationGroup()
        {
            var qhs = new List<QuestionHolder>();
            var groupLabel = "Student Contact Information";
            var groupType = QuestionGroupDataType.ContactInformation;

            qhs.Add(new QuestionHolder
            {
                Text = "Mobile Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantMobilePhone,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Home Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantHomePhone,
            });

            //qhs.Add(new QuestionHolder() { Text = "Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantPhone1 });
            //qhs.Add(new QuestionHolder() { Text = "Phone Type", QuestionTypeId = QuestionType.RadioList, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantPhoneType1, Options = "Mobile, Home, Work" });
            //qhs.Add(new QuestionHolder() { Text = "Alternate Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantPhone2 });
            //qhs.Add(new QuestionHolder() { Text = "Alternate Phone Type", QuestionTypeId = QuestionType.RadioList, GroupName = groupLabel, QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantPhoneType2, Options = "Mobile, Home, Work" });

            return qhs;
        }

        public static List<QuestionHolder> CreateStudentInformationGroup()
        {
            var qhs = new List<QuestionHolder>();
            var groupLabel = "Student Information";
            var groupType = QuestionGroupDataType.PersonalInformation;

            qhs.Add(new QuestionHolder
            {
                Text = "First Name",
                IsActive = true,
                IsRequired = true,
                QuestionTypeId = QuestionType.TextBox,
                QuestionGroupDataTypeId = groupType,
                GroupName = groupLabel,
                IsLocked = true,
                QuestionDataTypeId = QuestionDataType.RegistrantFirstName,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Middle Name",
                IsActive = true,
                IsRequired = false,
                QuestionTypeId = QuestionType.TextBox,
                QuestionGroupDataTypeId = groupType,
                GroupName = groupLabel,
                IsLocked = true,
                QuestionDataTypeId = QuestionDataType.RegistrantMiddleName,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Last Name",
                IsActive = true,
                IsRequired = true,
                QuestionTypeId = QuestionType.TextBox,
                QuestionGroupDataTypeId = groupType,
                GroupName = groupLabel,
                IsLocked = true,
                QuestionDataTypeId = QuestionDataType.RegistrantLastName,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Email",
                IsActive = true,
                IsRequired = true,
                QuestionTypeId = QuestionType.Email,
                QuestionGroupDataTypeId = groupType,
                GroupName = groupLabel,
                IsLocked = true,
                QuestionDataTypeId = QuestionDataType.RegistrantEmail,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Mobile Phone",
                QuestionTypeId = QuestionType.TextBox,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantMobilePhone,
            });

            qhs.Add(new QuestionHolder
            {
                Text = "Home Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantHomePhone,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Work Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantWorkPhone,
            });

            qhs.Add(new QuestionHolder
            {
                Text = "Date of Birth",
                IsActive = true,
                QuestionTypeId = QuestionType.Date,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantDateOfBirth,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Grade",
                IsActive = true,
                QuestionTypeId = QuestionType.DropDownList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantGrade,
                Options = RegistrantGrade,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Gender",
                IsActive = true,
                QuestionTypeId = QuestionType.RadioList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantGender,
                Options = RegistrantGender,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Ethnicity",
                IsActive = true,
                IsRequired = false,
                QuestionTypeId = QuestionType.DropDownList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantEthnicityUsFed,
                Options = RegistrantEthnicity,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Race",
                IsActive = true,
                IsRequired = false,
                QuestionTypeId = QuestionType.DropDownList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantRaceUsFed,
                Options = RegistrantRace,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "T-Shirt Size",
                IsActive = true,
                QuestionTypeId = QuestionType.RadioList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantTShirtSize,
                Options = RegistrantTShirtSize,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Organization",
                QuestionTypeId = QuestionType.TextBox,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                IsLocked = false,
                QuestionDataTypeId = QuestionDataType.RegistrantOrganization,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Relationship To Student",
                QuestionTypeId = QuestionType.DropDownList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantParentRelationship,
                Options = ParentRelationshipToRegistrant,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Employer",
                QuestionTypeId = QuestionType.TextBox,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantEmployer,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Student ID",
                QuestionTypeId = QuestionType.TextBox,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.StudentId,
            });

            return qhs;
        }

        public static List<QuestionHolder> CreateVolunteerInformationGroup()
        {
            var qhs = new List<QuestionHolder>();
            var groupLabel = "Volunteer Information";
            var groupType = QuestionGroupDataType.PersonalInformation;

            qhs.Add(new QuestionHolder
            {
                Text = "First Name",
                IsActive = true,
                IsRequired = true,
                QuestionTypeId = QuestionType.TextBox,
                QuestionGroupDataTypeId = groupType,
                GroupName = groupLabel,
                IsLocked = true,
                QuestionDataTypeId = QuestionDataType.RegistrantFirstName,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Middle Name",
                IsActive = true,
                IsRequired = false,
                QuestionTypeId = QuestionType.TextBox,
                QuestionGroupDataTypeId = groupType,
                GroupName = groupLabel,
                IsLocked = true,
                QuestionDataTypeId = QuestionDataType.RegistrantMiddleName,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Last Name",
                IsActive = true,
                IsRequired = true,
                QuestionTypeId = QuestionType.TextBox,
                QuestionGroupDataTypeId = groupType,
                GroupName = groupLabel,
                IsLocked = true,
                QuestionDataTypeId = QuestionDataType.RegistrantLastName,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Email",
                IsActive = true,
                IsRequired = true,
                QuestionTypeId = QuestionType.Email,
                QuestionGroupDataTypeId = groupType,
                GroupName = groupLabel,
                IsLocked = true,
                QuestionDataTypeId = QuestionDataType.RegistrantEmail,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Mobile Phone",
                QuestionTypeId = QuestionType.TextBox,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantMobilePhone,
            });

            qhs.Add(new QuestionHolder
            {
                Text = "Home Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantHomePhone,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Work Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantWorkPhone,
            });

            qhs.Add(new QuestionHolder
            {
                Text = "Date of Birth",
                IsActive = true,
                QuestionTypeId = QuestionType.Date,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantDateOfBirth,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Gender",
                IsActive = true,
                QuestionTypeId = QuestionType.RadioList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantGender,
                Options = RegistrantGender,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Ethnicity",
                IsActive = true,
                IsRequired = false,
                QuestionTypeId = QuestionType.DropDownList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantEthnicityUsFed,
                Options = RegistrantEthnicity,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Race",
                IsActive = true,
                IsRequired = false,
                QuestionTypeId = QuestionType.DropDownList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantRaceUsFed,
                Options = RegistrantRace,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Employer",
                QuestionTypeId = QuestionType.TextBox,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantEmployer,
            });

            return qhs;
        }

        private static List<QuestionHolder> CreateParent1InterestGroup()
        {
            var qhs = new List<QuestionHolder>();
            var groupType = QuestionGroupDataType.RegistrantInterestInformation;
            var groupLabel = "Parent/Guardian Information";

            qhs.Add(new QuestionHolder
            {
                Text = "First Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantFirstName,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Last Name", IsActive = true, QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantLastName,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Email", IsActive = true, QuestionTypeId = QuestionType.Email, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantEmail,
            });

            qhs.Add(new QuestionHolder
            {
                Text = "Mobile Phone", QuestionTypeId = QuestionType.TextBox, GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType, QuestionDataTypeId = QuestionDataType.RegistrantMobilePhone,
            });

            var commonGradeOptions = "Pre-K,K,1st,2nd,3rd,4th,5th,6th,7th,8th,9th,10th,11th,12th";
            qhs.Add(new QuestionHolder
            {
                Text = "Child 1 Grade",
                QuestionTypeId = QuestionType.DropDownList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantChild1Grade,
                Options = commonGradeOptions,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Child 2 Grade",
                QuestionTypeId = QuestionType.DropDownList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantChild2Grade,
                Options = commonGradeOptions,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Child 3 Grade",
                QuestionTypeId = QuestionType.DropDownList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantChild3Grade,
                Options = commonGradeOptions,
            });
            qhs.Add(new QuestionHolder
            {
                Text = "Child 4 Grade",
                QuestionTypeId = QuestionType.DropDownList,
                GroupName = groupLabel,
                QuestionGroupDataTypeId = groupType,
                QuestionDataTypeId = QuestionDataType.RegistrantChild4Grade,
                Options = commonGradeOptions,
            });
            qhs.Add(new QuestionHolder
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

        public static SystemMessages PhoneValidation(string answer, FormQuestion question, SystemMessages msgs)
        {
            if (answer.IsNotEmpty() && !answer.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                msgs.Add($"{question.Text} must be a valid phone number", MessageType.UserError);

            return msgs;
        }

        public static GenderType GetGenderFromAnswer(string answer)
        {
            switch (LibraryUtilities.GetKeyValuePairs(answer).FirstOrDefault()?.value.ToLower())
            {
                case "male":
                    return GenderType.Male;
                case "female":
                    return GenderType.Female;
                case "other":
                    return GenderType.Other;
                default:
                    return GenderType.NotSet;
            }
        }

        public class QuestionHolder
        {
            public string GroupName;
            public bool IsActive;
            public bool IsLocked;
            public bool IsRequired;
            public string Link;
            public string Options;
            public QuestionDataType QuestionDataTypeId;
            public QuestionGroupDataType QuestionGroupDataTypeId;
            public QuestionType QuestionTypeId;
            public RegistrantType RegistrantTypeId;
            public string Text;

            public QuestionHolder()
            {
                RegistrantTypeId = RegistrantType.Individual;
            }

            public static string StateList => BasicLibrary.Utilities.GetStateList();
        }
    }
}
using System.Collections.Generic;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Linq;
using AfterSchoolLibrary.Models;
using System.Collections.Specialized;

namespace AfterSchoolLibrary.Services
{
    public static class FormQuestionService
    {
        public static bool IsPhoneDataType(this FormQuestion question)
        {
            switch (question.QuestionDataTypeId)
            {
                case QuestionDataType.EmergencyContact1Phone:
                case QuestionDataType.EmergencyContact1Phone2:
                case QuestionDataType.EmergencyContact1Phone3:
                case QuestionDataType.EmergencyContact2Phone:
                case QuestionDataType.EmergencyContact2Phone2:
                case QuestionDataType.EmergencyContact2Phone3:
                case QuestionDataType.EmergencyContact3Phone:
                case QuestionDataType.EmergencyContact3Phone2:
                case QuestionDataType.EmergencyContact3Phone3:
                case QuestionDataType.OrganizationContactPhone:
                case QuestionDataType.Parent1HomePhone:
                case QuestionDataType.Parent1MobilePhone:
                case QuestionDataType.Parent1Phone1:
                case QuestionDataType.Parent1Phone2:
                case QuestionDataType.Parent1WorkPhone:
                case QuestionDataType.Parent2HomePhone:
                case QuestionDataType.Parent2MobilePhone:
                case QuestionDataType.Parent2Phone1:
                case QuestionDataType.Parent2Phone2:
                case QuestionDataType.Parent2WorkPhone:
                case QuestionDataType.RegistrantDoctorPhone:
                case QuestionDataType.RegistrantHomePhone:
                case QuestionDataType.RegistrantMobilePhone:
                case QuestionDataType.RegistrantPhone1:
                case QuestionDataType.RegistrantPhone2:
                case QuestionDataType.RegistrantWorkPhone:
                    return true;
            }

            return false;
        }

        public static List<FormQuestion> GetEmergencyContactQuestions(SystemMessages msgs, DatabaseConnection dc = null)
        {
            var questions = new List<FormQuestion>();
            var form = FormService.GetAdministratorAccountStudentForm(msgs, dc);

            form.GetQuestionGroups(msgs, dc)
                .Where(group => group.QuestionGroupDataTypeId.In(GetEmergencyContactQuestionGroups()))
                .ToList()
                .ForEach(group => questions.AddRange(group.GetAllQuestions(msgs, dc)));

            return questions;
        }
        
        public static List<FormQuestion> GetVolunteerEmergencyContactQuestions(SystemMessages msgs, DatabaseConnection dc = null)
        {
            var questions = new List<FormQuestion>();
            var form = FormService.GetAdministratorAccountVolunteerForm(msgs, dc);

            form.GetQuestionGroups(msgs, dc)
                .Where(group => group.QuestionGroupDataTypeId.In(GetEmergencyContactQuestionGroups()))
                .ToList()
                .ForEach(group => questions.AddRange(group.GetAllQuestions(msgs, dc)));

            return questions;
        }

        public static SystemMessages ValidateFormQuestionForEmergencyContacts(List<FormQuestionViewModel> questions, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            // check required fields
            questions.Where(m => m.Question.IsRequired && m.Value.IsEmpty()).ToList().ForEach(z => { msgs.Add($"{z.Question.Text} is required", MessageType.UserError); });

            // check phone numbers
            questions.Where(m => m.Value.IsNotEmpty() && !m.Value.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx) &&
                m.Question.QuestionDataTypeId.In(GetEmergencyContactPhoneTypes()))
                .ToList()
                .ForEach(z => {
                    msgs.Add($"{z.Question.FormQuestionGroupQuestionGroupDataTypeId.ToString().SplitByUpperCase()} {z.Question.Text} must be a valid phone number", MessageType.UserError);
                });

            // emergency phone numbers should not be same
            var quesitonsWithDuplicatePhoneNumbers = questions
                .Where(m => m.Value.IsNotEmpty() && m.Question.QuestionDataTypeId.In(GetEmergencyContactPhoneTypes()))
                .GroupBy(x => x.Value)
                .Where(y => y.Count() > 1)
                .ToList();

            if (quesitonsWithDuplicatePhoneNumbers.Any())
                msgs.Add("Emergency phone numbers should not be the same", MessageType.UserError);

            // validation for ec2 fields, user must enter first-last name and phone numbers if they add any single field
            var ec2 = questions.Where(m => m.Question.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact2).ToList();
            var ec2FirstName = ec2.FirstOrDefault(m => m.Question.QuestionDataTypeId == QuestionDataType.EmergencyContact2FirstName);
            var ec2LastName = ec2.FirstOrDefault(m => m.Question.QuestionDataTypeId == QuestionDataType.EmergencyContact2LastName);
            var ec2Phone = ec2.FirstOrDefault(m => m.Question.QuestionDataTypeId == QuestionDataType.EmergencyContact2Phone);
            ValidateEmergencyContacts(firstName: ec2FirstName.Value, lastName: ec2LastName.Value, phone: ec2Phone.Value, ecNumber: 2, msgs: msgs);

            // validation for ec3 fields
            var ec3 = questions.Where(m => m.Question.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.EmergencyContact3).ToList();
            var ec3FirstName = ec3.FirstOrDefault(m => m.Question.QuestionDataTypeId == QuestionDataType.EmergencyContact3FirstName);
            var ec3LastName = ec3.FirstOrDefault(m => m.Question.QuestionDataTypeId == QuestionDataType.EmergencyContact3LastName);
            var ec3Phone = ec3.FirstOrDefault(m => m.Question.QuestionDataTypeId == QuestionDataType.EmergencyContact3Phone);
            ValidateEmergencyContacts(firstName: ec3FirstName.Value, lastName: ec3LastName.Value, phone: ec3Phone.Value, ecNumber: 3, msgs: msgs);

            return msgs;
        }

        public static SystemMessages ValidateEmergencyContacts(string firstName, string lastName, string phone, int ecNumber, SystemMessages msgs = null)
        {
            if (firstName.IsEmpty() && (lastName.IsNotEmpty() || phone.IsNotEmpty()))
                msgs.Add($"First name for contact {ecNumber} is required.", MessageType.UserError);

            if (lastName.IsEmpty() && (firstName.IsNotEmpty() || phone.IsNotEmpty()))
                msgs.Add($"Last name for contact {ecNumber} is required.", MessageType.UserError);

            if (phone.IsEmpty() && (firstName.IsNotEmpty() || lastName.IsNotEmpty()))
                msgs.Add($"Primary phone number for contact {ecNumber} is required.", MessageType.UserError);

            return msgs;
        }

        #region Helpers

        public static List<QuestionGroupDataType> GetEmergencyContactQuestionGroups() =>
            new List<QuestionGroupDataType>() { QuestionGroupDataType.EmergencyContact1, QuestionGroupDataType.EmergencyContact2, QuestionGroupDataType.EmergencyContact3 };

        public static List<QuestionDataType> GetEmergencyContactPhoneTypes() => new List<QuestionDataType>() {
            QuestionDataType.EmergencyContact1Phone, QuestionDataType.EmergencyContact1Phone2, QuestionDataType.EmergencyContact1Phone3,
            QuestionDataType.EmergencyContact2Phone, QuestionDataType.EmergencyContact2Phone2, QuestionDataType.EmergencyContact2Phone3,
            QuestionDataType.EmergencyContact3Phone, QuestionDataType.EmergencyContact3Phone2, QuestionDataType.EmergencyContact3Phone3
        };

        /// <summary>
        /// Workaround to make ViewModel accessible in service layer
        /// </summary>
        public class FormQuestionViewModel
        {
            public FormQuestion Question { get; set; }
            public NameValueCollection Values { get; set; }

            public FormQuestionViewModel(FormQuestion question, NameValueCollection values)
            {
                Question = question;
                Values = new NameValueCollection(values);
            }

            public FormQuestionViewModel(FormQuestion question)
            {
                Question = question;
                Values = new NameValueCollection();
            }

            public FormQuestionViewModel(FormQuestion question, string answer)
            {
                Question = question;
                Values = new NameValueCollection() { { Id, answer } };
            }

            public FormQuestionViewModel(FormQuestion question, FormQuestionAnswer answer)
            {
                Question = question;
                Values = new NameValueCollection() { { Id, answer.Text } };
            }

            public bool Hide { get; set; }

            public string RequiredProperty
            {
                get
                {
                    return Question.IsRequired ? " data-required " : "";
                }
            }

            public string Id
            {
                get
                {
                    return $"question-{((int)Question.ID).ToString()}";
                }
            }

            public string Name
            {
                get
                {
                    return Id;
                }
            }

            public string ToolTip
            {
                get
                {
                    return Question.IsRequired ? " data-toggle='popover' data-trigger='hover' data-content='required' data-placement='right' " : "";
                }
            }

            public string ToolTipId
            {
                get
                {
                    return Question.IsRequired ? $" data-tooltip-id='{Id}' " : "";
                }
            }

            public string RequiredStar
            {
                get
                {
                    return Question.IsRequired ? "<span class='text-danger'>*</span>" : " <span class='text-muted'><em>(optional)</em></span>";
                }
            }

            public string CheckedProperty
            {
                get
                {
                    return Value == "1" ? " checked=\"checked\" " : "";
                }
            }

            public string DefaultValue
            {
                get
                {
                    if (Question.QuestionTypeId == QuestionType.Checkbox)
                        return "1";

                    return "";
                }
            }

            public string Value
            {
                get
                {
                    return Values[this.Name];
                }
                set
                {
                    Values[this.Name] = value;
                }
            }

            public string FileDownloadLink
            {
                get
                {
                    return FormQuestionAnswer.GetFileDownloadLink(Value);
                }
            }
        }

        public static FormQuestionViewModel ToFormQuestionViewModelForService(this FormQuestion question, NameValueCollection values)
        {
            return new FormQuestionViewModel(question, values);
        }

        #endregion
    }
}

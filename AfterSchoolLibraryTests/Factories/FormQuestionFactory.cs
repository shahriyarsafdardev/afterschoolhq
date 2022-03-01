using System;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using System.Collections.Generic;

namespace AfterSchoolLibraryTests.Factories
{
    public class FormQuestionFactory
    {
        public static FormQuestion Create(int formId, int formQuestionGroupId, string text, string options, QuestionType questionTypeId, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new FormQuestion()
            {
                CreateDate = DateTime.Now,
                CreatedBy = 1,
                LastModifiedBy = 1,
                LastModifiedDate = DateTime.Now,
                FormQuestionGroupId = formQuestionGroupId,
                FormId = formId,
                IsTemplate = true,
                Text = text,
                QuestionTypeId = questionTypeId,
                QuestionDataTypeId = QuestionDataType.RegistrantGender,
                IsActive = true,
                IsRequired = true,
                IsLocked = true,
                Options = options,
                DisplayOrder = 1,
                IsAnswerDescriptionEnabled = false
            };

            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
        public static FormQuestion Create(int formId, int formQuestionGroupId, string text, QuestionDataType questionDataType, QuestionType questionTypeId, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new FormQuestion()
            {
                CreateDate = DateTime.Now,
                CreatedBy = 1,
                LastModifiedBy = 1,
                LastModifiedDate = DateTime.Now,
                FormQuestionGroupId = formQuestionGroupId,
                FormId = formId,
                IsTemplate = true,
                Text = text,
                QuestionTypeId = questionTypeId,
                QuestionDataTypeId = questionDataType,
                IsActive = true,
                IsRequired = true,
                IsLocked = true,
                DisplayOrder = 1,
                IsAnswerDescriptionEnabled = false
            };

            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static FormQuestion Create(int formId, int formQuestionGroupId, string text, string options, QuestionType questionTypeId, bool isLocked, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new FormQuestion()
            {
                CreateDate = DateTime.Now,
                CreatedBy = 1,
                LastModifiedBy = 1,
                LastModifiedDate = DateTime.Now,
                FormQuestionGroupId = formQuestionGroupId,
                FormId = formId,
                IsTemplate = true,
                Text = text,
                QuestionTypeId = questionTypeId,
                QuestionDataTypeId = QuestionDataType.RegistrantGender,
                IsActive = true,
                IsRequired = true,
                IsLocked = isLocked,
                Options = options,
                DisplayOrder = 1,
                IsAnswerDescriptionEnabled = false
            };

            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static List<KeyValuePair<FormQuestion, string>> GetPersonalInformationQuestionAnswerModel(FormQuestionGroup formQuestionGroup, SystemMessages msgs, DatabaseConnection dc)
        {
            List<KeyValuePair<FormQuestion, string>> model = new List<KeyValuePair<FormQuestion, string>>();

            var registrantFirstName = FormQuestionFactory.Create(formId: formQuestionGroup.FormId.Value, formQuestionGroupId: formQuestionGroup.ID.Value, text: "First Name", questionDataType: QuestionDataType.RegistrantFirstName, questionTypeId: QuestionType.TextBox, msgs: msgs, dc: dc);

            var registrantMiddleName = FormQuestionFactory.Create(formId: formQuestionGroup.FormId.Value, formQuestionGroupId: formQuestionGroup.ID.Value, text: "Middle Name", questionDataType: QuestionDataType.RegistrantMiddleName, questionTypeId: QuestionType.TextBox, msgs: msgs, dc: dc);

            var registrantLastName = FormQuestionFactory.Create(formId: formQuestionGroup.FormId.Value, formQuestionGroupId: formQuestionGroup.ID.Value, text: "Last Name", questionDataType: QuestionDataType.RegistrantLastName, questionTypeId: QuestionType.TextBox, msgs: msgs, dc: dc);

            var registrantEmail = FormQuestionFactory.Create(formId: formQuestionGroup.FormId.Value, formQuestionGroupId: formQuestionGroup.ID.Value, text: "Email", questionDataType: QuestionDataType.RegistrantEmail, questionTypeId: QuestionType.TextBox, msgs: msgs, dc: dc);

            var registrantGrade = FormQuestionFactory.Create(formId: formQuestionGroup.FormId.Value, formQuestionGroupId: formQuestionGroup.ID.Value, text: "Grade", options: "Pre-K,K,1st,2nd,3rd,4th,5th,6th,7th,8th,9th,10th,11th,12th", questionTypeId: QuestionType.DropDownList, msgs: msgs, dc: dc);

            var registrantMobilePhone = FormQuestionFactory.Create(formId: 48, formQuestionGroupId: formQuestionGroup.ID.Value, text: "Mobile Phone", questionDataType: QuestionDataType.RegistrantMobilePhone, questionTypeId: QuestionType.TextBox, msgs: msgs, dc: dc);

            model.Add(new KeyValuePair<FormQuestion, string>(registrantFirstName, "John"));
            model.Add(new KeyValuePair<FormQuestion, string>(registrantMiddleName, ""));
            model.Add(new KeyValuePair<FormQuestion, string>(registrantLastName, "William"));
            model.Add(new KeyValuePair<FormQuestion, string>(registrantEmail, $"don+{DateTime.Now.Ticks}@example.com"));
            model.Add(new KeyValuePair<FormQuestion, string>(registrantGrade, "5th"));
            model.Add(new KeyValuePair<FormQuestion, string>(registrantMobilePhone, "518-555-0114"));

            return model;
        }
    }
}

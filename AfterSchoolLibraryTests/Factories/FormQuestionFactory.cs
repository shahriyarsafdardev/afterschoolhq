using System;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

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
    }
}

using System;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class FormQuestionAnswerFactory
    {
        public static FormQuestionAnswer Create(int formQuestionId, string questionText, string answerText, QuestionDataType questionDataTypeId, SystemMessages msgs, DatabaseConnection dc)
        {
            var obj = new FormQuestionAnswer()
            {
                CreateDate = DateTime.Now,
                CreatedBy = 1,
                LastModifiedBy = 1,
                LastModifiedDate = DateTime.Now,
                FormQuestionId = formQuestionId,
                RegistrantId = 1,
                ActivityId = 1,
                RegistrationId = 1,
                Text = answerText,
                QuestionDataTypeId = questionDataTypeId,
                QuestionText = questionText,
                SurveyResponseId = 1,
                Uid = Guid.NewGuid()                
            };

            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}

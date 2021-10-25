using System;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class FormQuestionGroupFactory
    {
        public static FormQuestionGroup Create(int formId, SystemMessages msgs = null, DatabaseConnection dc = null) => Create(formId, true, msgs, dc);

        public static FormQuestionGroup Create(int formId, bool isActive = true, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var obj = new FormQuestionGroup()
            {
                CreateDate = DateTime.Now,
                CreatedBy = 1,
                LastModifiedBy = 1,
                LastModifiedDate = DateTime.Now,
                Name = "Test",
                FormId = formId,
                RegistrantTypeId = RegistrantType.Individual,
                QuestionGroupDataTypeId = QuestionGroupDataType.ContactInformation,
                DisplayOrder = 1,
                IsActive = isActive
            };

            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}

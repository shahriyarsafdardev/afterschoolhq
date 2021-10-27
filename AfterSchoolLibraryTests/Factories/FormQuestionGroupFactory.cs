using System;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibraryTests.Factories
{
    public class FormQuestionGroupFactory
    {
        public static FormQuestionGroup Create(int formId, bool isDeleted = false, SystemMessages msgs = null, DatabaseConnection dc = null)
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
                IsDeleted = isDeleted
            };

            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }

        public static FormQuestionGroup Create(int formId, SystemMessages msgs = null, DatabaseConnection dc = null)
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
                IsDeleted = false
            };

            msgs.AddRange(obj.Save(dc: dc));
            return obj;
        }
    }
}

using System;
using System.Collections;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Linq;
using AfterSchoolLibrary.Services;


namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20220601_AddStudentIdQuestionToAdminDefaultForm {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    using (var connection = new SqlConnection(adc.ConnectionString))
                    {
                        var studentInfoQuestionGroups =
                            FormQuestionGroup.LoadByQuestionGroupDataTypeId((int)QuestionGroupDataType.PersonalInformation, amsgs, adc);
                        
                        studentInfoQuestionGroups.ForEach(questionGroup =>
                        {
                            var question = new FormQuestion
                            {
                                CreateDate = DateTime.UtcNow,
                                CreatedBy = 1,
                                FormId = questionGroup.FormId,
                                LastModifiedBy = 1,
                                LastModifiedDate = DateTime.UtcNow,
                                IsActive = true,
                                IsAnswerDescriptionEnabled = false,
                                IsLocked = true,
                                IsRequired = false,
                                IsTemplate = true,
                                QuestionDataTypeId = QuestionDataType.StudentId,
                                QuestionTypeId = QuestionType.TextBox,
                                Text = "Student ID"
                            };
                            var atIndex = questionGroup.GetQuestionCount() + 1;
                            questionGroup.AddQuestion(question, atIndex, adc);
                        });
                    }
                },
                uniqueIdenfier: "Migration_20220601_AddStudentIdQuestionToAdminDefaultForm",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
using AfterSchool.Areas.Api.Models;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace AfterSchool.Helpers
{
    public class ValidateCustomFormHelper
    {
        public static void ValidateFormQuestionSection(List<FormQuestionDto> questions,  NameValueCollection collection, string formSectionUrl, int groupId, HttpRequestBase request = null, SystemMessages msgs = null)
        {
            questions
                .Where(ques => ((((int) ques.FormQuestionGroupQuestionGroupDataTypeId == groupId &&
                    groupId != (int) QuestionGroupDataType.NotSet) ||
                    ((groupId == 0 || groupId == -1) && formSectionUrl == ques.GroupTitle) ) && ques.IsActive))
                .ToList()
                .ForEach(ques =>
                {
                    var question = FormQuestion.Load(ques.ID, msgs);
                    var answer = question.ToFormQuestionViewModel(collection);
                    var isFileUpload = (ques.QuestionTypeId == QuestionType.FileUpload ||
                                                  ques.QuestionTypeId == QuestionType.Photo ||
                                                  ques.QuestionTypeId == QuestionType.Audio ||
                                                  ques.QuestionTypeId == QuestionType.Video);

                    if (ques.QuestionTypeId != QuestionType.TextBlock && (ques.IsRequired && ((isFileUpload && request.Files["question-" + ques.ID].IsEmpty()) || (!isFileUpload && answer.Value.IsEmpty()))))
                        msgs.Add($"{ques.Text} is required", MessageType.UserError);
                });
        }
    }
}
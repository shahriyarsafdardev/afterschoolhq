using AfterSchool.Areas.Api.Models;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace AfterSchool.Helpers
{
    public class ValidateCustomFormHelper
    {
        public static void ValidateFormQuestionSection(List<FormQuestionDto> questions,  NameValueCollection collection, string formSectionUrl, QuestionAnswerResponse jsonQuestionAnswerResponse = null, UpdateQuestionAnswerResponse jsonUpdateQuestionAnswerResponse = null, SystemMessages msgs = null)
        {
            dynamic json = null;
            if (jsonQuestionAnswerResponse != null)
                json = jsonQuestionAnswerResponse;
            else
                json = jsonUpdateQuestionAnswerResponse;

            var groupId = (int.TryParse(json?.groupid, out int parsedGroupId) ? parsedGroupId : 0);

            questions
                .Where(ques => ((int) ques.FormQuestionGroupQuestionGroupDataTypeId == groupId &&
                    groupId != (int) QuestionGroupDataType.NotSet) ||
                    ((groupId == 0 || groupId == -1) && formSectionUrl == ques.GroupTitle))
                .ToList()
                .ForEach(ques =>
                {
                    var question = FormQuestion.Load(ques.ID);
                    var answer = question.ToFormQuestionViewModel(collection);
                    if (ques.IsRequired && answer.Value.IsEmpty())
                        msgs.Add($"{ques.Text} is required", MessageType.UserError);
                });
        }
    }
}